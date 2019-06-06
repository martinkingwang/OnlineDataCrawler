using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OnlineDataCrawler.Data;
using OnlineDataCrawler.Util;
using System.Reflection;

namespace OnlineDataCrawler.Engine
{
    public class MainLoop
    {
        public void Init()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes();
            foreach(var type in types)
            {
                if (type.IsSubclassOf(typeof(IDatabaseObject)))
                {
                    var databaseObject = (IDatabaseObject)Activator.CreateInstance(type);
                    databaseObject.ChcekBadataseIndex();
                }
            }
        }

        public void Loop()
        {
            while (true)
            {

            }
        }
    }
}
