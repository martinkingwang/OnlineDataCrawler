using OnlineDataCrawler.Engine.Annotation;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using OnlineDataCrawler.Data;
using Newtonsoft.Json;

namespace OnlineDataCrawler.Controller
{
    class StockDetailController : IController
    {
        public string StockID
        {
            get;
            set;
        }

        [Mapping(MappingUrl = "/stocksDetail")]
        public string GetStockF10(HttpListenerContext context)
        {
            if(StockID == "")
            {
                return null;
            }
            var stock = DataStorage.GetInstance().GetStock(StockID);
            var stockF10 = DataStorage.GetInstance().GetF10(stock);
            return JsonConvert.SerializeObject(stockF10);
        }
    }
}
