using System;
using System.Collections.Generic;
using System.Text;
using OnlineDataCrawler.Engine.Annotation;
using OnlineDataCrawler.Data;
using Newtonsoft.Json;
using System.Net;

namespace OnlineDataCrawler.Controller
{
    class IndustryController : IController
    {
        public string Name
        {
            get;
            set;
        }

        public DateTime? StartDate
        {
            get;
            set;
        }

        public DateTime? EndDate
        {
            get;
            set;
        }

        [Mapping(MappingUrl ="/getIndustries")]
        public string GetIndustry(HttpListenerContext context)
        {
            var industries = DataStorage.GetInstance().GetIndustry(Name, StartDate, EndDate);
            return JsonConvert.SerializeObject(industries);
        }
    }
}
