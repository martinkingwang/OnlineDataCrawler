using System;
using System.Collections.Generic;
using System.Text;
using OnlineDataCrawler.Engine.Annotation;
using OnlineDataCrawler.Data;
using Newtonsoft.Json;
using System.Net;


namespace OnlineDataCrawler.Controller
{
    class MarketIndexController : IController
    {
        public DateTime StartDate
        {
            get;
            set;
        }

        public DateTime EndDate
        {
            get;
            set;
        }

        [Mapping(MappingUrl ="getMarketIndex")]
        public string GetMarketIndex(HttpListenerContext context)
        {
            var index = DataStorage.GetInstance().GetAllMarketIndex(StartDate, EndDate);
            return JsonConvert.SerializeObject(index);
        }
    }
}
