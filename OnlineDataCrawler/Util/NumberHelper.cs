using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OnlineDataCrawler.Util
{
    public class NumberConventer
    {
        private static string[] ArabinNum = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private static string[] ChnNumS = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十", "百", "千", "万", "亿" };
        private static string[] ChnNumT = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖", "拾", "佰", "仟", "萬", "亿" };
        private static string[] Union = { "", "十", "百", "千" };
        /// <summary>
        /// 实例化
        /// </summary>
        public NumberConventer() { }
        /// <summary>
        /// 阿拉伯数字转换为中文数字
        /// </summary>
        /// <param name="value"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string ArabToChn(Decimal value, out string msg)
        {
            string neg = string.Empty; neg = (value < 0) ? "负" : "";
            string result = string.Empty;
            string[] part = (value.ToString().Replace("-", string.Empty)).Split('.');
            Int64 temp = Convert.ToInt64(part[0]);
            Int64 epart = temp;
            string dotpart = (part.Length > 1) ? part[1] : string.Empty;
            if (part.Length > 1)
            {
                dotpart = GetDotPart(dotpart);
            }
            string tmp = string.Empty;
            string lasttemp = string.Empty;
            for (int i = 0; i <= ((epart.ToString().Length - 1) / 4); i++)
            {
                int thousand = 0;
                thousand = Convert.ToInt32(temp % 10000);
                temp = temp / 10000;
                lasttemp = tmp;
                tmp = GetThousandPart(thousand);
                if (i == 0)
                {
                    result = tmp;
                    lasttemp = tmp;
                }
                if (i == 1)//返回的是万
                {
                    if (result == "零")
                    {
                        result = string.Empty;
                    }
                    result = tmp + "万" + ((lasttemp.IndexOf("千") == -1 && lasttemp != "零") ? "零" : "") + result;
                }
                if (i == 2)//亿
                {
                    if (result.IndexOf("零万") != -1)
                    {
                        result = result.Replace("零万", string.Empty);
                    }
                    result = tmp + "亿" + ((lasttemp.IndexOf("千") == -1 && lasttemp != "零") ? "零" : "") + result;
                }
                if (i == 3)//万亿
                {
                    if (result.IndexOf("零亿") != -1)
                    {
                        result = result.Replace("零亿", "亿");
                    }
                    result = tmp + "万" + ((lasttemp.IndexOf("千") == -1 && lasttemp != "零") ? "零" : "") + result;
                }
                if (i == 4)//亿亿
                {
                    if (result.IndexOf("零万") != -1)
                    {
                        result = result.Replace("零万", string.Empty);
                    }
                    result = tmp + "亿" + ((lasttemp.IndexOf("千") == -1 && lasttemp != "零") ? "零" : "") + result;
                }
            }
            result = neg + result + dotpart;
            msg = "成功转换！";
            return result;
        }
        /// <summary>
        /// 处理小数部分
        /// </summary>
        /// <param name="dotPart"></param>
        /// <returns></returns>
        private static string GetDotPart(string dotPart)
        {
            string result = "点";
            for (int i = 0; i < dotPart.Length; i++)
            {
                result += ChnNumS[Convert.ToInt32(dotPart[
                i].ToString())];
            }
            for (int j = 0; j < result.Length; j++)
            //去除无效零或点
            {
                if (result[result.Length - j - 1].ToString() != "点" && result[result.Length - j - 1].ToString() != "零")
                {
                    break;
                }
                else
                {
                    result = result.Substring(0, (result.Length - j - 1));
                }
            }
            return result;
        }
        /// <summary>
        /// 万位以下的分析
        /// </summary>
        /// <returns></returns>
        private static string GetThousandPart(int number)
        {
            if (number == 0)
            {
                return "零";
            }
            string result = string.Empty;
            bool lowZero = false;
            //记录低位有没有找到非零值，没找到置true
            bool befZero = false;
            //记录前一位是不是非零值，是0则置true
            int temp = number;
            int index = number.ToString().Length;
            for (int i = 0; i < index; i++)
            {
                int n = temp % 10;
                temp = temp / 10;
                if (i == 0) //起始位
                {
                    if (n == 0)
                    {
                        lowZero = true; //低位有0
                        befZero = true; //前位为0
                    }
                    else
                    {
                        result = ChnNumS[n];
                    }
                }
                else
                {
                    if (n != 0)
                    {
                        result = ChnNumS[n] + Union[i] + result;
                        lowZero = false;
                        befZero = false;
                    }
                    else
                    {
                        if (!lowZero)
                        {
                            if (!befZero)
                            //低位有数，且前位不为0，本位为0填零
                            //eg.5906
                            {
                                result = ChnNumS[n] + result;
                                befZero = true;
                            }
                            else
                            //低位有数，且前位为0省略零eg. 5008
                            {
                            }
                        }
                        else //低位为0
                        {
                            if (!befZero)//理论上不存在eg 5080
                            {
                                result = ChnNumS[n] + result;
                                befZero = true;
                            }
                            else //eg. 5000
                            {
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 中文数字转换为阿拉伯数字
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Decimal ChnToArab(string value)
        {
            decimal result = 0;
            foreach (var ch in value.ToArray())
            {
                if (SwitchNum(ch.ToString()) == -1)
                    continue;
                value = value.Replace(ch.ToString(), SwitchNum(ch.ToString()).ToString());
            }
            var values = value.Split("点".ToArray());
            value = values[0];
            decimal dotPart = 0;
            try
            {
                dotPart = GetArabDotPart(values[1]);
            }
            catch
            {

            }
            Regex regex = new Regex(@"\d+\.?\d*[\u4e00-\u9fa5]+");
            var regexs = regex.Match(value);
            List<decimal> parts = new List<decimal>();
            while (regexs.Success)
            {
                Regex reg = new Regex(@"\d+\.?\d*");
                var dec = reg.Match(regexs.Value);
                decimal previousNum = 0;
                int zeros = 0;
                foreach (var part in parts)
                {
                    previousNum += part;
                }
                decimal temp = 0;
                if (regexs.Value.Contains("十"))
                {
                    if (temp != 0)
                        temp = temp * 10;
                    else
                    {
                        if (dec.Success)
                        {
                            if (decimal.Parse(dec.Value) == 0)
                                temp = 10;
                            else
                                temp = decimal.Parse(dec.Value) * 10;
                        }
                        else
                        {
                            temp = 10;
                        }
                    }
                    zeros = 1;
                }
                if (regexs.Value.Contains("百"))
                {
                    if (temp != 0)
                        temp = temp * 100;
                    else
                    {
                        if (dec.Success)
                        {
                            if (decimal.Parse(dec.Value) == 0)
                                temp = 100;
                            else
                                temp = decimal.Parse(dec.Value) * 100;
                        }
                        else
                        {
                            temp = 100;
                        }
                    }
                    zeros = 2;
                }
                if (regexs.Value.Contains("千"))
                {
                    if (temp != 0)
                        temp = temp * 1000;
                    else
                    {
                        if (dec.Success)
                        {
                            if (decimal.Parse(dec.Value) == 0)
                                temp = 1000;
                            else
                                temp = decimal.Parse(dec.Value) * 1000;
                        }
                        else
                        {
                            temp = 1000;
                        }
                    }
                    zeros = 3;
                }
                if (regexs.Value.Contains("万"))
                {
                    if (temp != 0)
                        temp = temp * 10000;
                    else
                    {
                        if (dec.Success)
                        {
                            if (decimal.Parse(dec.Value) == 0)
                                temp = 10000;
                            else
                                temp = decimal.Parse(dec.Value) * 10000;
                        }
                        else
                        {
                            temp = 10000;
                        }
                    }
                    zeros = 4;
                }
                if (regexs.Value.Contains("亿"))
                {
                    if (temp != 0)
                        temp = temp * 100000000;
                    else
                    {
                        if (dec.Success)
                        {
                            if (decimal.Parse(dec.Value) == 0)
                                temp = 100000000;
                            else
                                temp += decimal.Parse(dec.Value) * 100000000;
                        }
                        else
                        {
                            temp = 100000000;
                        }
                    }
                    zeros = 8;
                    result = result * 100000000;
                }
                if (temp > previousNum && previousNum != 0)
                {
                    parts.Clear();
                    result += (decimal)Math.Pow(10, zeros) * previousNum + temp;
                }
                else
                {
                    parts.Add(temp);
                }
                regexs = regexs.NextMatch();
            }
            foreach (var part in parts)
            {
                result += part;
            }
            return result + dotPart;
        }
        /// <summary>
        /// 处理亿以下内容。
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private static Decimal HandlePart(string num)
        {
            Decimal result = 0;
            string temp = num;
            temp = temp.Replace("万", ",");
            string[] part = temp.Split(',');
            for (int i = 0; i < part.Length; i++)
            {
                result += Convert.ToDecimal(GetArabThousandPart(part[part.Length - i - 1])) * Convert.ToDecimal((System.Math.Pow(10000, Convert.ToDouble(i))));
            }
            return result;
        }
        /// <summary>
        /// 取得阿拉伯数字小数部分。
        /// </summary>
        /// <returns></returns>
        private static Decimal GetArabDotPart(string dotpart)
        {
            Decimal result = 0.00M;
            string spe = "0.";
            for (int i = 0; i < dotpart.Length; i++)
            {
                int results = SwitchNum(dotpart[i].ToString());
                if (results != -1)
                {
                    spe += results.ToString();
                }
                else
                {
                    spe += dotpart[i];
                }
            }
            result = Convert.ToDecimal(spe);
            return result;
        }
        /// <summary>
        /// 得到到阿拉伯数字千位
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private static int GetArabThousandPart(string number)
        {
            string ChnNumString = number;
            if (ChnNumString == "零")
            {
                return 0;
            }
            if (ChnNumString != string.Empty)
            {
                if (ChnNumString[0].ToString() == "十")
                {
                    ChnNumString = "一" + ChnNumString;
                }
            }
            ChnNumString = ChnNumString.Replace("零", string.Empty);
            //去除所有的零
            int result = 0;
            int index = ChnNumString.IndexOf("千");
            if (index != -1)
            {
                result +=
                SwitchNum(ChnNumString.Substring(0, index)) * 1000;
                ChnNumString =
                ChnNumString.Remove(0, index + 1);
            }
            index = ChnNumString.IndexOf("百");
            if (index != -1)
            {
                result +=
                SwitchNum(ChnNumString.Substring(0, index)) * 100;
                ChnNumString =
                ChnNumString.Remove(0, index + 1);
            }
            index = ChnNumString.IndexOf("十");
            if (index != -1)
            {
                result +=
                SwitchNum(ChnNumString.Substring(0, index)) * 10;
                ChnNumString =
                ChnNumString.Remove(0, index + 1);
            }
            if (ChnNumString != string.Empty)
            {
                result += SwitchNum(ChnNumString);
            }
            return result;
        }
        /// <summary>
        /// 取得汉字对应的阿拉伯数字
        /// </summary>
        /// <param name="cnNum"></param>
        /// <returns></returns>
        private static int SwitchNum(string cnNum)
        {
            switch (cnNum)
            {
                case "零":
                case "〇":
                    return 0;
                case "一":
                case "壹":
                    return 1;
                case "二":
                case "贰":
                    return 2;
                case "三":
                case "叁":
                    return 3;
                case "四":
                case "肆":
                    return 4;
                case "五":
                case "伍":
                    return 5;
                case "六":
                case "陆":
                    return 6;
                case "七":
                case "柒":
                    return 7;
                case "八":
                case "捌":
                    return 8;
                case "九":
                case "玖":
                    return 9;
                default: return -1;
            }
        }
    }
}
