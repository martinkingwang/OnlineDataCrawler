using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using OnlineDataCrawler.Logging;
using OnlineDataCrawler.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Concurrent;
using OnlineDataCrawler.Engine;

namespace OnlineDataCrawler.Data
{
    public class DataStorage
    {
        private List<StockBasic> stocks;
        private List<StockF10> f10s;
        private MongoDBHelper dbHelper;
        private ConcurrentDictionary<StockBasic, List<FinancialReport>> allReports;

        private static DataStorage dataStorage;

        public static DataStorage GetInstance()
        {
            if (dataStorage == null)
            {
                dataStorage = new DataStorage();
                dataStorage.CheckLocalData();
            }
            return dataStorage;
        }

        private DataStorage()
        {
            BsonClassMap.RegisterClassMap<BalanceSheet>();
            BsonClassMap.RegisterClassMap<CashFlow>();
            BsonClassMap.RegisterClassMap<ProfitStatement>();
            BsonClassMap.RegisterClassMap<IndustrySmbol>();
            dbHelper = new MongoDBHelper();
            allReports = new ConcurrentDictionary<StockBasic, List<FinancialReport>>();
            var stocks = GetAllStocks();
            foreach (var stock in stocks)
            {
                List<FinancialReport> financialReports = dbHelper.Find<FinancialReport>(x => x.Stock.InnerID == stock.InnerID);
                allReports.AddOrUpdate(stock, financialReports, (k, v) => v.Concat(financialReports).ToList());
            }
        }

        public MongoDBHelper DBHelper
        {
            get
            {
                return dbHelper;
            }
        }
        public List<StockBasic> GetAllStocks()
        {
            if (stocks != null)
            {
                return stocks;
            }

            try
            {
                stocks = dbHelper.Find<StockBasic>(x => x.StockID != null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                stocks = StockBasic.GetAllStocks();
                dbHelper.InsertMany<StockBasic>(stocks);
            }
            if (stocks == null || stocks.Count == 0)
            {
                stocks = StockBasic.GetAllStocks();
                dbHelper.InsertMany<StockBasic>(stocks);
            }
            return stocks;
        }

        public StockBasic GetStock(string stockID)
        {
            StockBasic stock;
            if (stocks != null)
            {
                stock = stocks.Find(x => { return x.StockID.Equals(stockID); });
                if (stock != null)
                {
                    return stock;
                }
            }
            else
            {
                try
                {
                    stocks = dbHelper.Find<StockBasic>(x => x.StockID != null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    stocks = StockBasic.GetAllStocks();
                    dbHelper.InsertMany<StockBasic>(stocks);
                }
            }
            stock = stocks.Find(x => { return x.StockID.Equals(stockID); });
            return stock;
        }

        public StockF10 GetF10(StockBasic sb)
        {
            StockF10 f10;
            if (f10s != null)
            {
                f10 = f10s.Find(x => x.StockID.Equals(sb.StockID));
                if (f10 != null)
                {
                    return f10;
                }
            }
            else
            {
                f10s = dbHelper.Find<StockF10>(x => x.StockID != null);
                f10 = f10s.Find(x => x.StockID.Equals(sb.StockID));
                if (f10 != null)
                {
                    return f10;
                }
            }
            try
            {
                f10 = StockF10.GetStockF10(sb);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            if (f10 != null)
            {
                f10s.Add(f10);
                dbHelper.Insert<StockF10>(f10);
            }
            return f10;
        }

        public List<FinancialReport> GetFinancialReports(StockBasic sb, DateTime? endTime = null, DateTime? startTime = null)
        {
            List<FinancialReport> reports = null;
            foreach (var key in allReports.Keys)
            {
                if (key.Equals(sb))
                {
                    reports = allReports[key];
                    break;
                }
            }
            if (reports == null)
            {
                reports = new List<FinancialReport>();
                reports.AddRange(AddAllFinancialReports(sb, FinancialReportType.BalanceSheet, endTime));
                reports.AddRange(AddAllFinancialReports(sb, FinancialReportType.CashFlow, endTime));
                reports.AddRange(AddAllFinancialReports(sb, FinancialReportType.ProfitStatement, endTime));
            }

            var result = from rep in reports
                         where rep.ReportDate <= endTime && rep.ReportDate >= startTime
                         select rep;
            if (result.Count() == 0)
            {
                var temp = new List<FinancialReport>();
                temp.AddRange(FinancialReport.GetFinancialReports(sb, GetF10(sb), startTime, endTime));
                var distinct = temp.Intersect(reports);
                if (distinct.Count() > 0)
                {
                    dbHelper.InsertMany(distinct);
                }
                reports.AddRange(distinct);
            }
            result = from rep in reports
                     where rep.ReportDate <= endTime && rep.ReportDate >= startTime
                     select rep;
            return result.ToList();
        }

        public List<FinancialReport> GetFinancialReports(StockBasic stock, DateTime startTime, DateTime endTime, FinancialReportType type)
        {
            List<FinancialReport> reports = null;
            foreach (var key in allReports.Keys)
            {
                if (key.Equals(stock))
                {
                    reports = allReports[key];
                    break;
                }
            }
            if (reports == null || reports.Count == 0)
            {
                reports = new List<FinancialReport>();
                reports.AddRange(AddAllFinancialReports(stock, type, endTime));
            }
            Type desiredReportType = null;
            switch (type)
            {
                case FinancialReportType.BalanceSheet:
                    desiredReportType = typeof(BalanceSheet);
                    break;
                case FinancialReportType.CashFlow:
                    desiredReportType = typeof(CashFlow);
                    break;
                case FinancialReportType.ProfitStatement:
                    desiredReportType = typeof(ProfitStatement);
                    break;
            }
            IEnumerable<FinancialReport> selectedReports = from rep in reports
                                                           where rep.GetType() == desiredReportType
                                                           select rep;
            return selectedReports.ToList();
        }

        public List<FinancialReport> AddAllFinancialReports(StockBasic sb, FinancialReportType reportType, DateTime? endTime = null)
        {
            List<FinancialReport> reports = new List<FinancialReport>();
            StockF10 f10 = GetF10(sb);
            while (true)
            {
                try
                {
                    if (endTime != null)
                    {
                        reports.AddRange(FinancialReport.GetFinancialReports(sb, f10, reportType, null, endTime));
                        endTime = reports.Last().ReportDate;
                    }
                    else
                    {
                        reports.AddRange(FinancialReport.GetFinancialReports(sb, f10, reportType, null, endTime));
                        endTime = reports.Last().ReportDate;
                    }
                }
                catch (Exception ex)
                {
                    Log.Trace(ex.StackTrace);
                    Log.Trace("无可用报表");
                    break;
                }
            }
            try
            {
                List<FinancialReport> existsReport = dbHelper.Find<FinancialReport>(x => x.Stock.InnerID == sb.InnerID && x.ReportDate < endTime);
                foreach (FinancialReport rep in reports)
                {
                    if (existsReport.Contains(rep))
                    {
                        reports.Remove(rep);
                    }
                }
                dbHelper.InsertMany<FinancialReport>(reports);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return reports;
        }

        public List<StockHistoryPrice> GetHistoryPrices(StockBasic stock, DateTime? endDate, int count)
        {
            List<StockHistoryPrice> result = new List<StockHistoryPrice>();
            if (endDate == null)
                endDate = DateTime.Now;
            int outCount = 0;
            var dbResult = dbHelper.FindByPage<StockHistoryPrice>(x => x.Stock.InnerID == stock.InnerID, 0, count, out outCount);
            if (outCount == 0)
            {
                var tempList = StockHistoryPrice.GetHistoryPrice(stock, new DateTime(1980, 1, 1), DateTime.Now);
                if (tempList.Count != 0)
                {
                    var temp = from item in tempList
                               where item.Date < endDate
                               orderby item.Date descending
                               select item;
                    dbHelper.InsertMany(temp.ToList().GetRange(0, count));
                    result.AddRange(temp.ToList().GetRange(0, count));
                }
            }
            return result;
        }

        public StockHistoryPrice GetHistoryPrice(StockBasic stock, DateTime date)
        {
            int outCount = 0;
            var dbResult = dbHelper.FindByPage<StockHistoryPrice>(x => x.Stock.InnerID == stock.InnerID && x.Date.Day == date.Day && x.Date.Month == date.Month && x.Date.Year == date.Year, 1, 1, out outCount);
            if (outCount > 0)
            {
                return dbResult[0];
            }
            else
            {
                var price = StockHistoryPrice.GetHistoryPrice(stock, date, date);
                if (price.Count > 0)
                {
                    dbHelper.InsertMany(price);
                    return price[0];
                }
                else
                {
                    return null;
                }
            }

        }

        public List<StockHistoryPrice> GetHistoryPrices(StockBasic stock, DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null)
                startDate = new DateTime(1980, 1, 1);
            if (endDate == null)
                endDate = DateTime.Now;
            List<StockHistoryPrice> result = new List<StockHistoryPrice>();
            try
            {
                result = dbHelper.Find<StockHistoryPrice>(x => x.Stock.InnerID == stock.InnerID && x.Date <= endDate && x.Date >= startDate);
                if (result == null || result.Count == 0)
                {
                    List<StockHistoryPrice> list;
                    list = StockHistoryPrice.GetHistoryPrice(stock, startDate.Value, endDate.Value);
                    var sortedResult = from re in result
                                       orderby re.Date descending
                                       select re;
                    List<StockHistoryPrice> toAdd = new List<StockHistoryPrice>();
                    foreach (var item in list)
                    {
                        bool isFound = false;
                        foreach (var re in sortedResult)
                        {
                            if (item.Date.Day == re.Date.Day && item.Date.Month == re.Date.Month && item.Date.Year == re.Date.Year)
                            {
                                isFound = true;
                                break;
                            }
                        }
                        if (!isFound)
                        {
                            toAdd.Add(item);
                        }
                    }
                    result.AddRange(toAdd);
                    dbHelper.InsertMany(toAdd);
                }
                else
                {
                    var sortedResult = from re in result
                                       orderby re.Date descending
                                       select re;
                    var list = StockHistoryPrice.GetHistoryPrice(stock, sortedResult.ToArray()[0].Date, endDate.Value);
                    List<StockHistoryPrice> toAdd = new List<StockHistoryPrice>();
                    foreach (var item in list)
                    {
                        bool isFound = false;
                        foreach (var re in sortedResult)
                        {
                            if (item.Date.Day == re.Date.Day && item.Date.Month == re.Date.Month && item.Date.Year == re.Date.Year)
                            {
                                isFound = true;
                                break;
                            }
                        }
                        if (!isFound)
                        {
                            toAdd.Add(item);
                        }
                    }
                    result.AddRange(toAdd);
                    dbHelper.InsertMany(toAdd);
                }
            }
            catch (Exception ex)
            {
                Log.Error("GetHistoryPrices Error " + ex.ToString());
                Log.Trace(ex.StackTrace);
            }
            return result;
        }

        public List<StockBonus> GetAllBonus(StockBasic stock, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (startDate == null)
            {
                startDate = new DateTime(1980, 1, 1);
            }
            if (endDate == null || endDate > DateTime.Now)
            {
                endDate = DateTime.Now;
            }

            try
            {
                return dbHelper.Find<StockBonus>(x => x.BonusListDate >= startDate && x.BonusListDate < endDate && x.Stock.InnerID == stock.InnerID);
            }
            catch
            {
                List<StockBonus> bonus = StockBonus.GetAllBonus(stock);
                dbHelper.InsertMany(bonus);
                return bonus;
            }
        }
        public List<StockBonus> FindStockBonus(Expression<Func<StockBonus, bool>> func)
        {
            return dbHelper.Find(func);
        }

        public List<MarketIndex> GetAllMarketIndex(DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!dbHelper.CollectionExists(typeof(MarketIndex).Name))
            {
                UpdateAllMarketIndex();
            }
            DateTime defaultTime = Config.GetDateTime("startDate");
            if (startDate == null || startDate < defaultTime)
            {
                startDate = defaultTime;
            }

            if (endDate == null || endDate > DateTime.Now)
            {
                endDate = DateTime.Now;
            }

            if (startDate == endDate)
            {
                List<MarketIndex> result = new List<MarketIndex>();
                MarketIndex index = GetExactMarketIndex(startDate.Value);
                if (index != null)
                {
                    result.Add(index);
                }
                return result;
            }
            else
            {
                List<MarketIndex> indices = dbHelper.Find<MarketIndex>(x => x.Date < endDate && x.Date > startDate);
                return indices;
            }
        }

        public MarketIndex GetExactMarketIndex(DateTime date)
        {
            List<MarketIndex> list = dbHelper.Find<MarketIndex>(x => x.Date.Equals(date));
            if (list.Count == 0)
            {
                return null;
            }
            else
            {
                return list[0];
            }
        }

        public void UpdateStockPrice()
        {
            List<StockBasic> stocks = GetAllStocks();
            foreach (StockBasic stock in stocks)
            {
                List<StockHistoryPrice> prices = dbHelper.FindAndSort<StockHistoryPrice>(x => x.Stock.InnerID == stock.InnerID, 10, -1, y => y.Date);
                if (prices.Count > 0)
                {
                    var lastOpenDay = DateHelper.LastWorkingDay(DateTime.Now);
                    if (prices[0].Date.Day == lastOpenDay.Day && prices[0].Date.Month == lastOpenDay.Month && prices[0].Date.Year == lastOpenDay.Year)
                    {
                        continue;
                    }
                    else
                    {
                        GetHistoryPrices(stock, prices[0].Date, DateTime.Now);
                    }
                }
                else
                {
                    GetHistoryPrices(stock, new DateTime(2008, 1, 1), DateTime.Now);
                }
            }
        }
        
        public void UpdateAllStocks()
        {
            List<StockBasic> currentStocks = dbHelper.Find<StockBasic>(x => x.StockID != null);
            List<StockBasic> onlineStocks = StockBasic.GetAllStocks();
            List<StockBasic> changeName = new List<StockBasic>();
            foreach (var current in onlineStocks)
            {
                //找到更名的并将原数据库中数据更新
                StockBasic stock = currentStocks.Find(x => { return x.StockID.Equals(current.StockID) && x.StockName != current.StockName; });
                if (stock != null)
                {
                    StockBasic sb = new StockBasic();
                    sb.StockName = current.StockName;
                    sb.StockID = stock.StockID;
                    sb.id = stock.id;
                    sb.InnerID = current.InnerID;
                    sb.MarketID = current.MarketID;
                    sb.MarketName = current.MarketName;
                    dbHelper.Update(sb, x => x.id.Equals(sb.id));
                    StockF10 f10 = StockF10.GetStockF10(sb);
                    var currentF10 = dbHelper.Find<StockF10>(x => x.InnerID == sb.InnerID);
                    if (currentF10.Count == 0)
                    {
                        dbHelper.Insert(f10);
                    }
                    else
                    {
                        f10.id = currentF10[0].id;
                        dbHelper.Update(f10, x => x.id.Equals(currentF10[0].id));
                    }
                    changeName.Add(sb);
                }
            }
            foreach (var stock in changeName)
            {
                onlineStocks.RemoveAll(x => { return x.InnerID == stock.InnerID; });
            }
            foreach (var stock in currentStocks)
            {
                onlineStocks.RemoveAll(x => { return x.Equals(stock); });
            }
            //剩余全部为新上市股票
            List<StockF10> f10s = new List<StockF10>();
            foreach (StockBasic stock in onlineStocks)
            {
                f10s.Add(StockF10.GetStockF10(stock));
            }
            if (onlineStocks.Count > 0)
                dbHelper.InsertMany(onlineStocks);
            if (f10s.Count > 0)
                dbHelper.InsertMany(f10s);
        }
        
        public void UpdateFinancialReport()
        {
            List<FinancialReport> result = new List<FinancialReport>();
            List<StockBasic> stocks = GetAllStocks();
            DateTime endDate;
            DateTime now = DateTime.Now;
            if (now.Month <= 3)
            {
                endDate = new DateTime(now.Year - 1, 12, 31);
            }
            else if (now.Month <= 6)
            {
                endDate = new DateTime(now.Year, 3, 31);
            }
            else if (now.Month <= 9)
            {
                endDate = new DateTime(now.Year, 6, 30);
            }
            else
            {
                endDate = new DateTime(now.Year, 9, 30);
            }
            foreach (StockBasic s in stocks)
            {
                List<FinancialReport> reports = dbHelper.Find<FinancialReport>(x => x.Stock.InnerID == s.InnerID);
                IOrderedEnumerable<FinancialReport> sortedReports = from r in reports
                                                                    orderby r.ReportDate
                                                                    select r;
                List<FinancialReport> balanceSheet = new List<FinancialReport>();
                List<FinancialReport> cashFlow = new List<FinancialReport>();
                List<FinancialReport> profitStatement = new List<FinancialReport>();
                foreach (FinancialReport rep in sortedReports)
                {
                    if (rep.GetType().Equals(typeof(BalanceSheet)))
                    {
                        balanceSheet.Add(rep);
                    }
                    if (rep.GetType().Equals(typeof(CashFlow)))
                    {
                        cashFlow.Add(rep);
                    }
                    if (rep.GetType().Equals(typeof(ProfitStatement)))
                    {
                        profitStatement.Add(rep);
                    }
                }
                List<FinancialReport> balance = null;
                List<FinancialReport> cash = null;
                List<FinancialReport> profit = null;
                if (balanceSheet.Count > 0)
                {
                    balance = FinancialReport.GetFinancialReports(s, GetF10(s), FinancialReportType.BalanceSheet, balanceSheet[balanceSheet.Count - 1].ReportDate, endDate);

                }
                else
                {
                    balance = FinancialReport.GetFinancialReports(s, GetF10(s), FinancialReportType.BalanceSheet, new DateTime(1980, 1, 1), DateTime.Now);
                }
                if (cashFlow.Count > 0)
                {
                    cash = FinancialReport.GetFinancialReports(s, GetF10(s), FinancialReportType.CashFlow, cashFlow[cashFlow.Count - 1].ReportDate, endDate);
                }
                else
                {
                    cash = FinancialReport.GetFinancialReports(s, GetF10(s), FinancialReportType.CashFlow, new DateTime(1980, 1, 1), DateTime.Now);
                }

                if (profitStatement.Count > 0)
                {
                    profit = FinancialReport.GetFinancialReports(s, GetF10(s), FinancialReportType.ProfitStatement, profitStatement[profitStatement.Count - 1].ReportDate, endDate);
                }
                else
                {
                    profit = FinancialReport.GetFinancialReports(s, GetF10(s), FinancialReportType.ProfitStatement, new DateTime(1980, 1, 1), DateTime.Now);
                }

                foreach (FinancialReport b in balance)
                {
                    if (!balanceSheet.Contains(b))
                    {
                        result.Add(b);
                    }
                }
                foreach (FinancialReport c in cash)
                {
                    if (!cashFlow.Contains(c))
                    {
                        result.Add(c);
                    }
                }
                foreach (FinancialReport p in profit)
                {
                    if (!profitStatement.Contains(p))
                    {
                        result.Add(p);
                    }
                }

            }
            dbHelper.InsertMany(result);
        }

        private void UpdateFinancialReportTime()
        {
            foreach (var key in allReports.Keys)
            {
                var list = allReports[key];
                foreach (var item in list)
                {

                }
            }
        }
        public void UpdateAllFinancialReports1()
        {
            List<StockBasic> stockList = GetAllStocks();
            Dictionary<StockBasic, List<FinancialReport>> stockFinancialReport = new Dictionary<StockBasic, List<FinancialReport>>();
            int count = 0;
            foreach (StockBasic stock in stockList)
            {
                bool found = false;
                StockBasic foundKey = null;
                foreach (var key in allReports.Keys)
                {
                    if (key.Equals(stock))
                    {
                        found = true;
                        foundKey = key;
                        break;
                    }
                }
                if (!found)
                {
                    try
                    {
                        var list = FinancialReport.GetFinancialReports(stock, GetF10(stock), null, null);
                        allReports.TryAdd(stock, list);
                        dbHelper.InsertMany(list);
                        count += list.Count;
                    }
                    catch (Exception ex)
                    {
                        Log.Trace(ex.StackTrace);
                        Log.Trace("无可用报表");
                    }
                }
                else
                {
                    var reports = allReports[foundKey];
                    IOrderedEnumerable<FinancialReport> reportList = from rep in reports
                                                                     where rep.Stock.Equals(stock)
                                                                     orderby rep.ReportDate
                                                                     select rep;
                    StockF10 f10 = GetF10(stock);
                    var last = reportList.Last();
                    var lastSeason = DateHelper.LastSeasonLastDay(DateTime.Now);
                    if (last.ReportDate != lastSeason)
                    {
                        List<FinancialReport> list = FinancialReport.GetFinancialReports(stock, f10, reportList.Last().ReportDate, null);
                        if (list.Count > 0)
                        {
                            var insert = from rep in list
                                         where rep.ReportDate == lastSeason
                                         select rep;
                            if (insert.Count() > 0)
                                dbHelper.InsertMany(insert);
                            count += insert.Count();
                        }
                    }
                }
            }
            Log.Trace("total update " + count + " financial reports.");
        }
        
        public void UpdateAllBonusShare()
        {
            List<StockBasic> stocks = GetAllStocks();
            List<StockBonus> result = new List<StockBonus>();
            foreach (StockBasic stock in stocks)
            {
                List<StockBonus> bonus;
                try
                {
                    bonus = dbHelper.Find<StockBonus>(x => x.Stock.InnerID == stock.InnerID);
                    if (bonus == null)
                    {
                        bonus = new List<StockBonus>();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    dbHelper.CreateCollection<StockBonus>();
                    bonus = new List<StockBonus>();
                }
                List<StockBonus> nBonus = StockBonus.GetAllBonus(stock);
                foreach (StockBonus n in nBonus)
                {
                    if (!bonus.Contains(n))
                    {
                        result.Add(n);
                    }
                }
            }
            dbHelper.InsertMany(result);
        }
        
        public void UpdateAllMarketIndex()
        {
            List<MarketIndex> indices;
            List<MarketIndex> result = new List<MarketIndex>();
            try
            {
                indices = dbHelper.Find<MarketIndex>(x => x != null);

            }
            catch
            {
                Log.Trace("Error - no MarketIndex database, using default value");
                result = MarketIndex.GetAllIndex(new DateTime(2005, 1, 4), DateTime.Now);
                indices = new List<MarketIndex>();
            }
            IOrderedEnumerable<MarketIndex> sortedIndex = from index in indices
                                                          orderby index.Date
                                                          select index;
            if (sortedIndex.Count() > 0)
            {
                List<MarketIndex> webIndex = MarketIndex.GetAllIndex(sortedIndex.ToArray()[sortedIndex.Count() - 1].Date, DateTime.Now);

                foreach (MarketIndex index in webIndex)
                {
                    if (!result.Contains(index))
                    {
                        result.Add(index);
                    }
                }
            }
            dbHelper.InsertMany(result);
        }
        
        public void UpdateAllIndustries()
        {
            List<Industry> currentIndustries = dbHelper.Find<Industry>(x => x.IndustryName != null);
            List<Industry> CalcIndustries = Industry.GetAllStockIndustry();
            IEnumerable<Industry> toAddIndustries = from ind in CalcIndustries
                                                    where !currentIndustries.Contains(ind)
                                                    select ind;
            dbHelper.InsertMany(toAddIndustries);
        }

        public void CheckLocalData()
        {
            var lastRunTime = Config.GetDateTime("UpdateAllStock", new DateTime(1, 1, 1));
            var timespan = DateTime.Now - lastRunTime;
            if (timespan.TotalDays > 7)
            {
                Log.Trace("update all stock runs at " + lastRunTime.ToLongDateString() + ", update it");
                UpdateAllStocks();
                Config.SetDateTime("UpdateAllStock", DateTime.Now);
            }
            else
            {
                Log.Trace("check all stock success.");
            }
            var stockslist = GetAllStocks();

            lastRunTime = Config.GetDateTime("UpdateAllFinancialReports", new DateTime(1, 1, 1));
            timespan = DateTime.Now - lastRunTime;
            if (timespan.TotalDays > 90)
            {
                Log.Trace("update all financial reports at " + lastRunTime.ToLongDateString() + ", update it.");
                UpdateAllFinancialReports1();
                Config.SetDateTime("UpdateAllFinancialReports", DateTime.Now);
            }
            else
            {
                Log.Trace("check all financial reports success.");
            }
            lastRunTime = Config.GetDateTime("UpdateMarketIndex", new DateTime(1, 1, 1));
            timespan = DateTime.Now - lastRunTime;
            if (timespan.TotalDays > 7)
            {
                Log.Trace("update market index at " + lastRunTime.ToLongDateString() + ", update it.");
                UpdateAllMarketIndex();
                Config.SetDateTime("UpdateMarketIndex", DateTime.Now);
            }
            else
            {
                Log.Trace("check market index success.");
            }
            lastRunTime = Config.GetDateTime("UpdateAllIndustries", new DateTime(1, 1, 1));
            timespan = DateTime.Now - lastRunTime;
            if (timespan.TotalDays > 90)
            {
                Log.Trace("update All industries at " + lastRunTime.ToLongDateString() + ", update it.");
                UpdateAllIndustries();
                Config.SetDateTime("UpdateAllIndustries", DateTime.Now);
            }
            else
            {
                Log.Trace("check industry information success.");
            }
            lastRunTime = Config.GetDateTime("UpdateStockPrice", new DateTime(1, 1, 1));
            timespan = DateTime.Now - lastRunTime;
            if (timespan.TotalDays > 7)
            {
                Log.Trace("update All Stock Price at " + lastRunTime.ToLongDateString() + ", update it.");
                UpdateStockPrice();
                Config.SetDateTime("UpdateStockPrice", DateTime.Now);
            }
            else
            {
                Log.Trace("check stock price success.");
            }
            lastRunTime = Config.GetDateTime("UpdateAllBonusShare", new DateTime(1, 1, 1));
            timespan = DateTime.Now - lastRunTime;
            if (timespan.TotalDays > 7)
            {
                Log.Trace("update All Stock Bonus at " + lastRunTime.ToLongDateString() + ", update it.");
                UpdateAllBonusShare();
                Config.SetDateTime("UpdateAllBonusShare", DateTime.Now);
            }
            else
            {
                Log.Trace("check stock bonus success.");
            }
        }
    }
}
