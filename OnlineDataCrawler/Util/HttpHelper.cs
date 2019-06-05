using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace OnlineDataCrawler.Util
{
    public class HttpHelper
    {
        public const string CharsetReg = @"(meta.*?charset=""?(?<Charset>[^\s""'>]+)""?)|(xml.*?encoding=""?(?<Charset>[^\s"">]+)""?)";
        /// <summary>

        /// 创建GET方式的HTTP请求

        /// </summary>

        /// <param name="url">请求的URL</param>

        /// <param name="myCookieContainer">随同HTTP请求发送的Cookie信息</param>

        /// <returns>返回字符串</returns>

        public static string Get(string url, CookieContainer cookies = null, string[] headers = null, Dictionary<string, string> parameters = null, string userAgent = null, string referer = null, Encoding encode = null)
        {
            if (parameters != null)
            {
                url += "?";
                foreach (var pair in parameters)
                {
                    url += pair.Key + "=" + pair.Value + "&";
                }
                url = url.Substring(0, url.Length - 1);
            }
            HttpWebResponse httpResponse = CreateGetHttpResponse(url, cookies: cookies, userAgent: userAgent, referer: referer);
            string Content = null;
            //缓冲区长度
            const int N_CacheLength = 10000;
            //头部预读取缓冲区，字节形式
            var bytes = new List<byte>();
            int count = 0;
            //头部预读取缓冲区，字符串
            String cache = string.Empty;

            //创建流对象并解码
            Stream ResponseStream;
            switch (httpResponse.ContentEncoding.ToUpperInvariant())
            {
                case "GZIP":
                    ResponseStream = new GZipStream(
                        httpResponse.GetResponseStream(), CompressionMode.Decompress);
                    break;
                case "DEFLATE":
                    ResponseStream = new DeflateStream(
                        httpResponse.GetResponseStream(), CompressionMode.Decompress);
                    break;
                default:
                    ResponseStream = httpResponse.GetResponseStream();
                    break;
            }
            if (cookies == null)
                cookies = new CookieContainer();
            foreach (Cookie cook in httpResponse.Cookies)
            {
                cookies.Add(cook);
            }
            try
            {
                while (
                    !(cache.EndsWith("</head>", StringComparison.OrdinalIgnoreCase)
                      || count >= N_CacheLength))
                {

                    var b = (byte)ResponseStream.ReadByte();
                    if (b == 255) //end of stream
                    {
                        break;
                    }
                    bytes.Add(b);

                    count++;
                    cache += (char)b;
                }


                if (encode == null)
                {
                    try
                    {
                        if (httpResponse.CharacterSet == "ISO-8859-1" || httpResponse.CharacterSet == "zh-cn")
                        {
                            Match match = Regex.Match(cache, CharsetReg, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            if (match.Success)
                            {
                                try
                                {
                                    string charset = match.Groups["Charset"].Value;
                                    encode = Encoding.GetEncoding(charset);
                                }
                                catch { }
                            }
                            else
                                encode = Encoding.GetEncoding("GB2312");
                        }
                        else
                            encode = Encoding.GetEncoding(httpResponse.CharacterSet);
                    }
                    catch { }
                }
                if (encode == null)
                {
                    encode = Encoding.UTF8;
                }

                //缓冲字节重新编码，然后再把流读完
                var Reader = new StreamReader(ResponseStream, encode);
                Content = encode.GetString(bytes.ToArray(), 0, count) + Reader.ReadToEnd();
                Reader.Close();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            finally
            {
                ResponseStream.Close();
                httpResponse.Close();
            }
            //获取返回的Cookies，支持httponly
            string cookiesDomain = httpResponse.ResponseUri.Host;

            cookies = new CookieContainer();
            CookieCollection httpHeaderCookies = SetCookie(httpResponse, cookiesDomain);
            cookies.Add(httpHeaderCookies ?? httpResponse.Cookies);

            return Content;
        }

        /// <summary>

        /// 创建POST方式的HTTP请求 

        /// </summary>

        /// <param name="url">请求的URL</param>

        /// <param name="postDataStr">发送的数据</param>

        /// <returns>返回字符串</returns>

        public static string Post(string url, string postDataStr, CookieContainer cookies)

        {

            var response = CreatePostHttpResponse(url, postData: postDataStr, cookies: cookies);
            string encoding = response.ContentEncoding;
            StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(response.CharacterSet));
            string retStr = sr.ReadToEnd();
            sr.Close();
            response.Close();
            return retStr;

        }

        public static HttpWebResponse CreateGetHttpResponse(string url, int timeout = 60000, string userAgent = "", CookieContainer cookies = null, string referer = "", string[] headers = null)
        {
            HttpWebRequest request = null;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                //对服务端证书进行有效性校验（非第三方权威机构颁发的证书，如自己生成的，不进行验证，这里返回true）
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);
                request = WebRequest.Create(url) as HttpWebRequest;
                //request.ProtocolVersion = HttpVersion.Version10;    //http版本，默认是1.1,这里设置为1.0
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }

            request.Referer = referer;
            request.Method = "GET";

            //设置代理UserAgent和超时
            if (string.IsNullOrWhiteSpace(userAgent))
                userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36 Edge/15.15063";

            request.UserAgent = userAgent;
            request.Timeout = timeout;
            request.KeepAlive = true;
            request.AllowAutoRedirect = true;

            if (headers != null)
            {
                foreach (var str in headers)
                {
                    string[] head = str.Split(':');
                    switch (head[0].Trim().ToLower())
                    {
                        case "user-agent":
                            request.UserAgent = head[1].Trim();
                            break;
                        case "host":
                            request.Host = head[1].Trim();
                            break;
                        case "fds":
                            break;
                        default:
                            request.Headers.Add(str);
                            break;
                    }
                }
            }
            if (cookies == null)
                cookies = new CookieContainer();
            request.CookieContainer = cookies;

            return request.GetResponse() as HttpWebResponse;
        }

        public static HttpWebResponse CreatePostHttpResponse(string url, string postData, int timeout = 60000, string userAgent = "", CookieContainer cookies = null, string referer = "")
        {
            HttpWebRequest request = null;
            //如果是发送HTTPS请求  
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);
                request = WebRequest.Create(url) as HttpWebRequest;
                //request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Referer = referer;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            //设置代理UserAgent和超时
            if (string.IsNullOrWhiteSpace(userAgent))
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36.0.1985.125 Safari/537.36";
            else
                request.UserAgent = userAgent;
            request.Timeout = timeout;
            request.KeepAlive = true;
            request.AllowAutoRedirect = true;

            if (cookies == null)
                cookies = new CookieContainer();
            request.CookieContainer = cookies;

            //发送POST数据  
            if (!string.IsNullOrWhiteSpace(postData))
            {
                byte[] data = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = data.Length;
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            //string[] values = request.Headers.GetValues("Content-Type");
            return request.GetResponse() as HttpWebResponse;
        }

        private static CookieCollection SetCookie(HttpWebResponse response, string defaultDomain)
        {
            try
            {
                string[] setCookie = response.Headers.GetValues("Set-Cookie");

                // there is bug in it,the datetime in "set-cookie" will be sepreated in two pieces.
                List<string> a = new List<string>(setCookie);
                for (int i = setCookie.Length - 1; i > 0; i--)
                {
                    if (a[i].Substring(a[i].Length - 3) == "GMT")
                    {
                        a[i - 1] = a[i - 1] + ", " + a[i];
                        a.RemoveAt(i);
                        i--;
                    }
                }
                setCookie = a.ToArray();
                CookieCollection cookies = new CookieCollection();
                foreach (string str in setCookie)
                {
                    NameValueCollection hs = new NameValueCollection();
                    foreach (string i in str.Split(';'))
                    {
                        int index = i.IndexOf("=");
                        if (index > 0)
                            hs.Add(i.Substring(0, index).Trim(), i.Substring(index + 1).Trim());
                        else
                            switch (i)
                            {
                                case "HttpOnly":
                                    hs.Add("HttpOnly", "True");
                                    break;
                                case "Secure":
                                    hs.Add("Secure", "True");
                                    break;
                            }
                    }
                    Cookie ck = new Cookie();
                    foreach (string Key in hs.AllKeys)
                    {
                        switch (Key.ToLower().Trim())
                        {
                            case "path":
                                ck.Path = hs[Key];
                                break;
                            case "expires":
                                ck.Expires = DateTime.Parse(hs[Key]);
                                break;
                            case "domain":
                                ck.Domain = hs[Key];
                                break;
                            case "httpOnly":
                                ck.HttpOnly = true;
                                break;
                            case "secure":
                                ck.Secure = true;
                                break;
                            default:
                                ck.Name = Key;
                                ck.Value = hs[Key];
                                break;
                        }
                    }
                    if (ck.Domain == "") ck.Domain = defaultDomain;
                    if (ck.Name != "") cookies.Add(ck);
                }
                return cookies;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 遍历CookieContainer
        /// </summary>
        /// <param name="cookieContainer"></param>
        /// <returns>List of cookie</returns>
        public static Dictionary<string, string> GetAllCookies(CookieContainer cookieContainer)
        {
            Dictionary<string, string> cookies = new Dictionary<string, string>();

            Hashtable table = (Hashtable)cookieContainer.GetType().InvokeMember("m_domainTable",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField |
                System.Reflection.BindingFlags.Instance, null, cookieContainer, new object[] { });

            foreach (string pathList in table.Keys)
            {
                StringBuilder _cookie = new StringBuilder();
                SortedList cookieColList = (SortedList)table[pathList].GetType().InvokeMember("m_list",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField
                    | System.Reflection.BindingFlags.Instance, null, table[pathList], new object[] { });
                foreach (CookieCollection colCookies in cookieColList.Values)
                    foreach (Cookie c in colCookies)
                        _cookie.Append(c.Name + "=" + c.Value + ";");

                cookies.Add(pathList, _cookie.ToString().TrimEnd(';'));
            }
            return cookies;
        }

        public static CookieContainer SetAllCookiePath(CookieContainer cookies, string path)
        {
            Dictionary<string, string> dict = GetAllCookies(cookies);
            string keys = string.Empty;
            string value = "bdshare_firstime=1490878103317;CNZZDATA5399792=cnzz_eid%3D782747711-1490874217-%26ntime%3D1490874217;CNZZDATA1252916811=1752926956-1490873529-%7C1490873529;SR_SEL=1_511;U_TRS2=000000b2.58f27c74.59a2ba4d.7978fc47;ULOGIN_IMG=tc-01f7a07f1502c1f506cfdebe3d9269504a76;SUB=_2A250psrjDeRhGedO7VUQ-SrMwj2IHXVX1bsrrDV_PUNbm9AKLUHtkW-XKuHLSWcNvARVTnvYdtCdg9absw..;SUBP=0033WrSXqPxfM725Ws9jqgMF55529P9D9WhM80hkWJWLbfochkOWdGJO5NHD95QpehqNeK.Xeh.pWs4Dqcjyi--NiKy8iKyFi--fiK.7i-8si--ciK.RiKy8Kg8EUsLV;SCF=AvCT3yVJX2WjgTKhR60SmzkZMBNSKkCsnu4vgDY8IUhKvgChueQklp6ll11yNQ4yoldMVtK8dIUe8I-eanecT78.;U_TRS1=000000e2.adcd23b.594d19c8.8cfdb3ef;UOR=my.sina.com.cn;SINAGLOBAL=218.68.185.226_1498225096.707755; ULV=1503836851424:10:1:1::1500295827709;lxlrtst=1498267163_o;lxlrttp=1498824353; SGUID = 1498347937594_396807;sso_info = v02m6alo5qztKWRk6SlkJOkpZCUjKWRk5ClkKOgpY6DhKadlqWkj5OEsI2jnLGOk5CwjpOEwA ==; ALF = 1535372851; Apache = 60.26.242.178_1503836852.604275; FINANCE2 = 8d5626b3132364178ca15d9e87dc4f27";
            dict.Clear();
            dict.Add(path, value.TrimEnd(';'));
            return ConvertToCookieContainer(dict);
        }

        /// <summary>
        /// convert cookies string to CookieContainer
        /// </summary>
        /// <param name="cookies"></param>
        /// <returns></returns>
        public static CookieContainer ConvertToCookieContainer(Dictionary<string, string> cookies)
        {
            CookieContainer cookieContainer = new CookieContainer();

            foreach (var cookie in cookies)
            {
                string[] strEachCookParts = cookie.Value.Split(';');
                int intEachCookPartsCount = strEachCookParts.Length;

                foreach (string strCNameAndCValue in strEachCookParts)
                {
                    if (!string.IsNullOrEmpty(strCNameAndCValue))
                    {
                        Cookie cookTemp = new Cookie();
                        int firstEqual = strCNameAndCValue.IndexOf("=");
                        string firstName = strCNameAndCValue.Substring(0, firstEqual);
                        string allValue = strCNameAndCValue.Substring(firstEqual + 1, strCNameAndCValue.Length - (firstEqual + 1));
                        cookTemp.Name = firstName.Trim();
                        cookTemp.Value = allValue.Trim();
                        cookTemp.Path = "/";
                        cookTemp.Domain = cookie.Key;
                        cookieContainer.Add(cookTemp);
                    }
                }
            }
            return cookieContainer;
        }

        public static CookieContainer CreateCookieContainer(string[] cookieStr)
        {
            Dictionary<string, string> cookies = new Dictionary<string, string>();
            foreach (string str in cookieStr)
            {
                var strs = str.Split(',');
                var key = strs[2];
                var value = strs[0] + "=" + strs[1];
                if (cookies.ContainsKey(key))
                {
                    var oldValue = cookies[key];
                    oldValue += ";" + value;
                    cookies[key] = oldValue;
                }
                else
                {
                    cookies[key] = value;
                }
            }
            return ConvertToCookieContainer(cookies);
        }

        public static string GetClientIP()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("https://ff.sinajs.cn/?_={0}&list=sys_clientip", (long)TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds);
            var result = Get(sb.ToString());

            Regex regex = new Regex("\"(.*)\"");
            var match = regex.Match(result);
            return match.Value.Replace("\"", "");
        }

        private static bool AcceptAllCertifications(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

    }
}
