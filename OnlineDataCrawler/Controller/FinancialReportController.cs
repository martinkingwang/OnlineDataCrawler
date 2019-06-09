using System;
using System.Collections.Generic;
using System.Text;
using OnlineDataCrawler.Data;
using System.Net;
using Newtonsoft.Json;
using OnlineDataCrawler.Engine.Annotation;

namespace OnlineDataCrawler.Controller
{
    class FinancialReportController : IController
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

        public FinancialReportType Type
        {
            get;
            set;
        }

        [Mapping(MappingUrl ="/getFinancialReports")]
        public string GetFinancialReport(HttpListenerContext context)
        {
            if(StockID == string.Empty)
            {
                return string.Empty;
            }
            var stock = DataStorage.GetInstance().GetStock(StockID);
            var reports = DataStorage.GetInstance().GetFinancialReports(stock, StartDate, EndDate, Type);
            return JsonConvert.SerializeObject(reports);
        }
    }
}
