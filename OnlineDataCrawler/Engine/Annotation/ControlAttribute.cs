using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineDataCrawler.Engine.Annotation
{
    public class Mapping :Attribute
    {
        public string MappingUrl
        {
            get;
            set;
        }
    }
}
