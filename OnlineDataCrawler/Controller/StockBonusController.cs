using System;
using System.Collections.Generic;
using System.Text;
using OnlineDataCrawler.Engine.Annotation;
using OnlineDataCrawler.Data;
using Newtonsoft.Json;
using System.Net;

namespace OnlineDataCrawler.Controller
{
    class StockBonusController : IController
    {
        public string StockID
        {
            get;
            set;
        }

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

        public string GetStockBonus(HttpListenerContext context)
        {
            var stock = DataStorage.GetInstance().GetStock(StockID);
            var bonus = DataStorage.GetInstance().GetAllBonus(stock, StartDate, EndDate);
            return JsonConvert.SerializeObject(bonus);
        }
    }
}
