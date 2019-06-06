using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OnlineDataCrawler.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq.Expressions;

namespace OnlineDataCrawler.Data
{
    public class FinancialReport : IDatabaseObject
    {
        //http://f10.eastmoney.com/NewFinanceAnalysis/zcfzbAjax?companyType=4&reportDateType=1&reportType=1&endDate=&code=SH600203
        private const string FinancialReportAssestUrl = "http://f10.eastmoney.com/NewFinanceAnalysis/zcfzbAjax?companyType={1}&reportDateType={2}&reportType=1&endDate={3}&code={0}";
        //http://f10.eastmoney.com/NewFinanceAnalysis/lrbAjax?companyType=4&reportDateType=1&reportType=1&endDate=&code=SH600203
        private const string FinancialReportProfitUrl = "http://f10.eastmoney.com/NewFinanceAnalysis/lrbAjax?companyType={1}&reportDateType={2}&reportType=1&endDate={3}&code={0}";
        //http://f10.eastmoney.com/NewFinanceAnalysis/xjllbAjax?companyType=4&reportDateType=1&reportType=1&endDate=&code=SH600203
        //http://f10.eastmoney.com/NewFinanceAnalysis/xjllbAjax?companyType=4&reportDateType=0&reportType=1&endDate=&code=SH600203
        private const string FinancialReportCashUrl = "http://f10.eastmoney.com/NewFinanceAnalysis/xjllbAjax?companyType={1}&reportDateType={2}&reportType=1&endDate={3}&code={0}";
        private const string FinancialReportPrecentUrl = "http://f10.eastmoney.com/NewFinanceAnalysis/PercentAjax_Indx?code={0}";
        private static DateTime financialReportLastTime;

        public static List<FinancialReport> GetFinancialReports(StockBasic sb, StockF10 f10, DateTime? startDate = null, DateTime? endDate = null)
        {
            List<FinancialReport> result = new List<FinancialReport>();
            result.AddRange(GetFinancialReports(sb, f10, FinancialReportType.BalanceSheet, startDate, endDate));
            result.AddRange(GetFinancialReports(sb, f10, FinancialReportType.CashFlow, startDate, endDate));
            result.AddRange(GetFinancialReports(sb, f10, FinancialReportType.ProfitStatement, startDate, endDate));
            return result;
        }

        public static List<FinancialReport> GetFinancialReports(StockBasic sb, StockF10 f10, FinancialReportType type, DateTime? startDate = null, DateTime? endDate = null)
        {
            List<FinancialReport> result = new List<FinancialReport>();
            if (startDate == null)
            {
                startDate = new DateTime(1980, 1, 1);
            }
            if (financialReportLastTime == null)
            {
                financialReportLastTime = new DateTime(1990, 9, 20);
            }
            bool isContinue = true;
            while (isContinue)
            {
                result.AddRange(CrawlFinancialReports(sb, f10, type, 0, endDate));
                if (startDate < result[result.Count - 1].ReportDate)
                {
                    endDate = result[result.Count - 1].ReportDate;
                }
                else
                {
                    isContinue = false;
                }
            }
            return result;
        }

        private static List<FinancialReport> CrawlFinancialReports(StockBasic sb, StockF10 f10, FinancialReportType type, int reportDateType = 0, DateTime? endTime = null)
        {
            if (financialReportLastTime == null)
            {
                financialReportLastTime = new DateTime(1990, 9, 20);
            }

            TimeSpan time = DateTime.Now - financialReportLastTime;
            if (time.TotalMilliseconds < 1000)
            {
                Thread.Sleep(1000 - (int)time.TotalMilliseconds);
            }

            financialReportLastTime = DateTime.Now;
            string companyType = "";
            if (f10.Industry == "银行")
            {
                companyType = "3";
            }
            else
            {
                companyType = "4";
            }
            string baseUrl = "";
            switch (type)
            {
                case FinancialReportType.BalanceSheet:
                    baseUrl = FinancialReportAssestUrl;
                    break;
                case FinancialReportType.CashFlow:
                    baseUrl = FinancialReportCashUrl;
                    break;
                case FinancialReportType.ProfitStatement:
                    baseUrl = FinancialReportProfitUrl;
                    break;
            }
            DateTime lastSeason = DateTime.Now;
            if (endTime != null)
            {
                lastSeason = DateHelper.LastSeasonLastDay(endTime.Value);
            }

            string endTimeStr = "";
            if (endTime != null)
            {
                endTimeStr = lastSeason.ToString("yyyy-MM-dd");
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(baseUrl, sb.MarketID + sb.StockID, companyType, reportDateType, endTimeStr);

            string assest = HttpHelper.Get(stringBuilder.ToString());
            assest = assest.Substring(1, assest.Length - 2);
            assest = assest.Replace("\\", "");

            JArray assestResult = JArray.Parse(assest);

            List<FinancialReport> reports = new List<FinancialReport>();

            int id = 0;

            foreach (JToken assetYear in assestResult)
            {
                FinancialReport report = new FinancialReport();
                switch (type)
                {
                    case FinancialReportType.BalanceSheet:
                        report = assetYear.ToObject<BalanceSheet>();
                        id = 1;
                        break;
                    case FinancialReportType.CashFlow:
                        report = assetYear.ToObject<CashFlow>();
                        id = 2;
                        break;
                    case FinancialReportType.ProfitStatement:
                        report = assetYear.ToObject<ProfitStatement>();
                        id = 3;
                        break;
                }
                TimeSpan timeSpan = TimeZoneInfo.Local.GetUtcOffset(report.ReportDate);
                report.ReportDate = report.ReportDate.Subtract(timeSpan);
                report.Stock = sb;
                report = SetAllNullProportiesToZero(report);
                id = id + report.ReportDate.Year * 10000 + report.ReportDate.Month * 100 + report.ReportDate.Day + sb.InnerID * 100000000;
                report.id = ObjectId.GenerateNewId(id);
                reports.Add(report);

            }
            return reports;
        }

        private static FinancialReport SetAllNullProportiesToZero(FinancialReport report)
        {
            Type type = report.GetType();
            foreach (System.Reflection.PropertyInfo property in type.GetProperties())
            {
                if (property.PropertyType.Equals(typeof(double?)))
                {
                    if (property.GetValue(report) == null)
                    {
                        property.SetValue(report, (double?)0.00);
                    }
                }
            }
            return report;
        }

        public ObjectId id
        {
            get;
            set;
        }

        private DateTime _reportTime;

        [JsonProperty(PropertyName = "REPORTDATE")]
        public DateTime ReportDate
        {
            get => _reportTime;
            set
            {
                DateTime time = value.ToLocalTime();
                _reportTime = time;
            }
        }

        public StockBasic Stock
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            if (!GetType().Equals(obj.GetType()))
            {
                return false;
            }

            FinancialReport bs = (FinancialReport)obj;
            if (!Stock.Equals(bs.Stock))
            {
                return false;
            }

            if (!ReportDate.Equals(bs.ReportDate))
            {
                return false;
            }

            return true;
        }

        public bool ChcekBadataseIndex()
        {
            var dbHelper = DataStorage.GetInstance().DBHelper;
            if (!dbHelper.CollectionExists(typeof(FinancialReport).Name))
            {
                var type = GetType();
                var properties = type.GetProperties();
                string[] names = new string[properties.Length];
                for(int i = 0; i < properties.Length; i++)
                {
                    names[i] = properties[i].Name;
                }
                dbHelper.CreateCollection<FinancialReport>(names);
            }
            List<Expression<Func<FinancialReport, object>>> fields = new List<Expression<Func<FinancialReport, object>>>();
            List<int> direction = new List<int>();
            fields.Add(x => x.Stock.InnerID);
            direction.Add(1);
            fields.Add(x => x.ReportDate);
            direction.Add(-1);
            dbHelper.CreateIndexes<FinancialReport>(fields.ToArray(), direction.ToArray());
            return true;
        }
    }
    public enum FinancialReportType
    {
        BalanceSheet = 1,
        ProfitStatement = 2,
        CashFlow = 3,
    }
}
