using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Reflection;
using OnlineDataCrawler.Logging;
using System.Web;

namespace OnlineDataCrawler.Engine
{
    public class HttpServer
    {
        public int Port
        {
            get;
            private set;
        }

        private HttpListener _listener;
        private Dictionary<string, reflectionControl> _contexts;

        public bool IsListening
        {
            get
            {
                if (_listener != null)
                    return _listener.IsListening;
                else
                    return false;
            }
        }

        public HttpServer(int port)
        {
            _contexts = new Dictionary<string, reflectionControl>();
            Port = port;
        }

        public void Stop()
        {
            if(IsListening)
                _listener.Stop();
        }

        public void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:" + Port + "/");
            _listener.Start();
            var task = Task.Run(new Action(Process));
        }

        private void Process()
        {
            while (true)
            {
                var context = _listener.GetContext();
                Logging.Log.Trace("Get Request: " + context.Request.Url.ToString());
                Task.Run(() => HandleRequest(context));
                
            }
        }
        
        public void HandleRequest(HttpListenerContext context)
        {
            string requestString = context.Request.Url.AbsolutePath;
            if (_contexts.ContainsKey(requestString))
            {
                if (context.Request.Url.Query != string.Empty)
                {
                    var query = context.Request.Url.Query;
                    query = query.Remove(0, 1);
                    var querys = query.Split("&");
                    foreach(var q in querys)
                    {
                        var parameters = q.Split("=");
                        if(parameters.Length != 2)
                        {
                            continue;
                        }
                        var target = _contexts[requestString].target;
                        var type = target.GetType();
                        foreach(var prop in type.GetProperties())
                        {
                            if (prop.Name == parameters[0])
                            {
                                if (prop.PropertyType == typeof(string))
                                {
                                    parameters[1] = HttpUtility.UrlDecode(parameters[1]);
                                    prop.SetValue(target, parameters[1]);
                                }
                                if(prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                                {
                                    DateTime date = DateTime.Now;
                                    if(DateTime.TryParse(parameters[1], out date))
                                    {
                                        prop.SetValue(target, date);
                                    }
                                }
                                if(prop.PropertyType == typeof(double))
                                {
                                    double value = 0;
                                    if(double.TryParse(parameters[1], out value))
                                    {
                                        prop.SetValue(target, value);
                                    }
                                }
                                if (prop.PropertyType == typeof(int))
                                {
                                    int value = 0;
                                    if (int.TryParse(parameters[1], out value))
                                    {
                                        prop.SetValue(target, value);
                                    }
                                }
                                if (prop.PropertyType.IsEnum)
                                {
                                    object value = Enum.Parse(prop.PropertyType, parameters[1]);
                                    prop.SetValue(target, value);
                                }
                            }
                        }
                    }
                }
                var refl = _contexts[requestString];
                object[] param = new object[1];
                param[0] = context;
                requestString = refl.method.Invoke(refl.target, param).ToString();
            }
            else
            {
                requestString = "null";
            }
            byte[] buffer = Encoding.UTF8.GetBytes(requestString);
            char[] charbuffer = Encoding.UTF8.GetChars(buffer);
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.AppendHeader("charset", "utf-8");
            context.Response.ContentType = "application/javascript; charset=UTF-8";
            context.Response.StatusCode = 200;
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        }
        
        public void RegistContext(string query, object target, MethodInfo method)
        {
            reflectionControl reflection = new reflectionControl();
            reflection.method = method;
            reflection.target = target;
            _contexts.Add(query, reflection);
        }
    }
    class reflectionControl
    {
        public object target;
        public MethodInfo method;
    }
}
