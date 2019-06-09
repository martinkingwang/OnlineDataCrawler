using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OnlineDataCrawler.Data;
using System.Reflection;
using OnlineDataCrawler.Controller;
using OnlineDataCrawler.Engine.Annotation;
using OnlineDataCrawler.Util;

namespace OnlineDataCrawler.Engine
{
    public class MainLoop
    {
        public HttpServer Server
        {
            get;
            private set;
        }
        public void Init()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes();
            Server = new HttpServer(25645);
            Server.Start();
            foreach(var type in types)
            {
                var interfaces = type.GetInterfaces();
                foreach(var inter in interfaces)
                {
                    if (inter.Equals(typeof(IDatabaseObject)))
                    {
                        var databaseObject = (IDatabaseObject)Activator.CreateInstance(type);
                        databaseObject.CheckDatabaseIndex();
                    }
                    if (inter.Equals(typeof(IController)))
                    {
                        foreach(var method in type.GetMethods())
                        {
                            var attributes = method.GetCustomAttributes();
                            foreach(var attr in attributes)
                            {
                                if(attr.GetType() == typeof(Mapping))
                                {
                                    Mapping mapping = (Mapping)attr;
                                    object target = Activator.CreateInstance(type);
                                    Server.RegistContext(mapping.MappingUrl, target, method);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Loop()
        {
            while (true)
            {
                var updateResult = DataStorage.GetInstance().CheckLocalData();
                var logPath = Logging.Log.GetLogFilePath();
                string mailDestination = Config.Get("sendMailDestination", "");
                if (mailDestination != "")
                {
                    try
                    {
                        MailHelper.SendMail(mailDestination, DateTime.Now.ToLongDateString(), updateResult, logPath);
                    }
                    catch(Exception ex)
                    {
                        Logging.Log.Error(ex.Message);
                    }
                }
                Thread.Sleep(1000 * 60 * 60 * 24);
            }
        }
    }
}
