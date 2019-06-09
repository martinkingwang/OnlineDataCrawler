using System;
using System.Collections.Generic;
using System.Text;
using OnlineDataCrawler.Engine.Annotation;
using OnlineDataCrawler.Data;
using Newtonsoft.Json;
using System.Net;

namespace OnlineDataCrawler.Controller
{
    class GetAllStockController : IController
    {
        [Mapping(MappingUrl ="/stocks")]
        public string GetAllStock(HttpListenerContext context)
        {
            List<StockBasic> stocks = DataStorage.GetInstance().GetAllStocks();
            return JsonConvert.SerializeObject(stocks);
        }
    }
}
