using System;
using System.Collections.Generic;
using System.Text;
using OnlineDataCrawler.Engine.Annotation;
using OnlineDataCrawler.Data;
using Newtonsoft.Json;
using System.Net;

namespace OnlineDataCrawler.Controller
{
    class StockHistoryPriceController
    {
        public string StockID
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

        [Mapping(MappingUrl ="getStockPrices")]
        public string GetStockPrice(HttpListenerContext context)
        {
            var stock = DataStorage.GetInstance().GetStock(StockID);
            var prices = DataStorage.GetInstance().GetHistoryPrices(stock, EndDate, 20);
            return JsonConvert.SerializeObject(prices);
        }
    }
}
