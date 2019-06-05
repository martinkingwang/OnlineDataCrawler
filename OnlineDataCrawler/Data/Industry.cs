using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace OnlineDataCrawler.Data
{
    public class Industry
    {
        public static List<Industry> GetAllStockIndustry()
        {
            DataStorage dataStorage = DataStorage.GetInstance();
            List<StockBasic> stocks = dataStorage.GetAllStocks();
            List<Industry> result = new List<Industry>();
            foreach (StockBasic stock in stocks)
            {
                StockF10 f10 = dataStorage.GetF10(stock);
                bool contains = false;
                Industry industry = null;
                foreach (Industry ind in result)
                {
                    if (ind.IndustryName == f10.Industry)
                    {
                        contains = true;
                        industry = ind;
                        break;
                    }
                }
                if (!contains)
                {
                    industry = new Industry();
                    industry.IndustryName = f10.Industry;
                    industry.Stocks = new List<StockBasic>();
                    industry.Stocks.Add(stock);
                    industry.id = ObjectId.GenerateNewId();
                    result.Add(industry);
                }
                else
                {
                    industry.Stocks.Add(stock);
                }
            }
            List<Industry> tempResult = new List<Industry>();
            //calculate average finiancial report
            Task<List<Industry>>[] tasks = new Task<List<Industry>>[8];
            int start = 0;
            int end = result.Count / 8;
            for (int i = 0; i < 8; i++)
            {
                tasks[i] = Task<List<Industry>>.Factory.StartNew(() => calcIndustry(result.GetRange(start, end - start)));
                start = end;
                if (i >= 6)
                {
                    end = result.Count;
                }
                else
                {
                    end += result.Count / 8;
                }
            }
            foreach (Task<List<Industry>> task in tasks)
            {
                task.Wait();
                tempResult.AddRange(task.Result);
            }
            List<Industry> industries = new List<Industry>();
            //data clean
            foreach (Industry ind in result)
            {
                IEnumerable<IGrouping<DateTime, Industry>> temp = from t in tempResult
                                                                  where t.IndustryName == ind.IndustryName
                                                                  group t by t.Date into g
                                                                  select g;
                foreach (IGrouping<DateTime, Industry> i in temp)
                {
                    Industry industry = new Industry();
                    industry.Date = i.Key;
                    foreach (Industry item in i)
                    {
                        industry.IndustryName = item.IndustryName;
                        industry.Stocks = item.Stocks;
                        if (item.Balance != null)
                        {
                            industry.Balance = item.Balance;
                        }

                        if (item.Cash != null)
                        {
                            industry.Cash = item.Cash;
                        }

                        if (item.Profit != null)
                        {
                            industry.Profit = item.Profit;
                        }
                    }
                    industries.Add(industry);
                }
            }
            foreach (Industry ind in industries)
            {
                ind.ReportAnalysis = new FinancialReportAnalysis(ind.Balance, ind.Profit, ind.Cash);
            }
            return industries;
        }

        public static List<Industry> GetIndustryByDate(DateTime date)
        {
            DataStorage dataStorage = DataStorage.GetInstance();
            List<Industry> result = new List<Industry>();
            List<StockBasic> stocks = dataStorage.GetAllStocks();
            foreach (StockBasic stock in stocks)
            {
                StockF10 f10 = dataStorage.GetF10(stock);
                bool contains = false;
                Industry industry = null;
                foreach (Industry ind in result)
                {
                    if (ind.IndustryName == f10.Industry)
                    {
                        contains = true;
                        industry = ind;
                        break;
                    }
                }
                if (!contains)
                {
                    industry = new Industry();
                    industry.IndustryName = f10.Industry;
                    industry.Stocks = new List<StockBasic>();
                    industry.Stocks.Add(stock);
                    industry.id = ObjectId.GenerateNewId();
                    result.Add(industry);
                }
                else
                {
                    industry.Stocks.Add(stock);
                }
            }
            foreach (Industry industry in result)
            {
                List<BalanceSheet> balanceSheets = new List<BalanceSheet>();
                List<ProfitStatement> profitStatements = new List<ProfitStatement>();
                List<CashFlow> cashFlows = new List<CashFlow>();
                foreach (StockBasic stock in industry.Stocks)
                {
                    List<FinancialReport> reports = dataStorage.GetFinancialReports(stock, date, date);
                    foreach (FinancialReport report in reports)
                    {
                        if (report.GetType() == typeof(BalanceSheet))
                        {
                            balanceSheets.Add((BalanceSheet)report);
                        }
                        if (report.GetType() == typeof(CashFlow))
                        {
                            cashFlows.Add((CashFlow)report);
                        }
                        if (report.GetType() == typeof(ProfitStatement))
                        {
                            profitStatements.Add((ProfitStatement)report);
                        }
                    }
                }
                BalanceSheet balanceSheet = new BalanceSheet();
                balanceSheet.ReportDate = date;
                balanceSheet.Stock = new IndustrySmbol();
                balanceSheet.Stock.InnerID = 80000000;
                balanceSheet.Stock.StockName = industry.IndustryName;
                Type type = balanceSheet.GetType();
                System.Reflection.PropertyInfo[] properties = type.GetProperties();
                foreach (System.Reflection.PropertyInfo property in properties)
                {
                    if (property.GetValue(balanceSheet) == null)
                    {
                        double value = 0;
                        foreach (var sheets in balanceSheets)
                        {
                            if (property.GetValue(sheets) != null)
                            {
                                value += (double)property.GetValue(sheets);
                            }
                        }
                        property.SetValue(balanceSheet, value / balanceSheets.Count);
                    }
                }
                industry.Balance = balanceSheet;
                CashFlow cashFlow = new CashFlow();
                cashFlow.ReportDate = date;
                cashFlow.Stock = new IndustrySmbol();
                cashFlow.Stock.InnerID = 80000000;
                cashFlow.Stock.StockName = industry.IndustryName;
                type = cashFlow.GetType();
                properties = type.GetProperties();
                foreach (System.Reflection.PropertyInfo property in properties)
                {
                    if (property.GetValue(cashFlow) == null)
                    {
                        double value = 0;
                        foreach (CashFlow sheets in cashFlows)
                        {
                            if (property.GetValue(sheets) != null)
                            {
                                value += (double)property.GetValue(sheets);
                            }
                        }
                        property.SetValue(cashFlow, value / cashFlows.Count);
                    }
                }
                industry.Cash = cashFlow;
                ProfitStatement profit = new ProfitStatement();
                profit.ReportDate = date;
                profit.Stock = new IndustrySmbol();
                profit.Stock.InnerID = 80000000;
                profit.Stock.StockName = industry.IndustryName;
                type = profit.GetType();
                properties = type.GetProperties();
                foreach (System.Reflection.PropertyInfo property in properties)
                {
                    if (property.GetValue(profit) == null)
                    {
                        double value = 0;
                        foreach (ProfitStatement sheets in profitStatements)
                        {
                            if (property.GetValue(sheets) != null)
                            {
                                value += (double)property.GetValue(sheets);
                            }
                        }
                        property.SetValue(profit, value / profitStatements.Count);
                    }
                }
                industry.Profit = profit;
                industry.ReportAnalysis = new FinancialReportAnalysis(industry.Balance, industry.Profit, industry.Cash);
            }
            return result;
        }

        private static List<Industry> calcIndustry(List<Industry> result)
        {
            DataStorage dataStorage = DataStorage.GetInstance();
            List<Industry> tempResult = new List<Industry>();
            foreach (Industry ind in result)
            {
                Dictionary<DateTime, List<BalanceSheet>> balanceSheets = new Dictionary<DateTime, List<BalanceSheet>>();
                Dictionary<DateTime, List<CashFlow>> cashFlows = new Dictionary<DateTime, List<CashFlow>>();
                Dictionary<DateTime, List<ProfitStatement>> profitStatements = new Dictionary<DateTime, List<ProfitStatement>>();
                foreach (StockBasic stock in ind.Stocks)
                {
                    List<FinancialReport> list = dataStorage.GetFinancialReports(stock, new DateTime(1980, 1, 1), DateTime.Now, FinancialReportType.BalanceSheet);
                    foreach (FinancialReport item in list)
                    {
                        if (balanceSheets.ContainsKey(item.ReportDate))
                        {
                            balanceSheets[item.ReportDate].Add((BalanceSheet)item);
                        }
                        else
                        {
                            List<BalanceSheet> values = new List<BalanceSheet>();
                            values.Add((BalanceSheet)item);
                            balanceSheets.Add(item.ReportDate, values);
                        }
                    }
                    list = dataStorage.GetFinancialReports(stock, new DateTime(1980, 1, 1), DateTime.Now, FinancialReportType.CashFlow);
                    foreach (FinancialReport item in list)
                    {
                        if (cashFlows.ContainsKey(item.ReportDate))
                        {
                            cashFlows[item.ReportDate].Add((CashFlow)item);
                        }
                        else
                        {
                            List<CashFlow> values = new List<CashFlow>();
                            values.Add((CashFlow)item);
                            cashFlows.Add(item.ReportDate, values);
                        }
                    }
                    list = dataStorage.GetFinancialReports(stock, new DateTime(1980, 1, 1), DateTime.Now, FinancialReportType.ProfitStatement);
                    foreach (FinancialReport item in list)
                    {
                        if (profitStatements.ContainsKey(item.ReportDate))
                        {
                            profitStatements[item.ReportDate].Add((ProfitStatement)item);
                        }
                        else
                        {
                            List<ProfitStatement> values = new List<ProfitStatement>();
                            values.Add((ProfitStatement)item);
                            profitStatements.Add(item.ReportDate, values);
                        }
                    }
                }
                foreach (DateTime key in balanceSheets.Keys)
                {
                    BalanceSheet bs = new BalanceSheet();
                    bs.ReportDate = key;
                    bs.Stock = new IndustrySmbol();
                    bs.Stock.InnerID = 80000000;
                    bs.Stock.StockName = ind.IndustryName;
                    Type type = bs.GetType();
                    System.Reflection.PropertyInfo[] properties = type.GetProperties();
                    foreach (System.Reflection.PropertyInfo property in properties)
                    {
                        if (property.GetValue(bs) == null)
                        {
                            double value = 0;
                            foreach (BalanceSheet sheets in balanceSheets[key])
                            {
                                if (property.GetValue(sheets) != null)
                                {
                                    value += (double)property.GetValue(sheets);
                                }
                            }
                            property.SetValue(bs, value / balanceSheets[key].Count);
                        }
                    }
                    Industry industry = new Industry();
                    industry.Date = key;
                    industry.IndustryName = ind.IndustryName;
                    industry.Stocks = ind.Stocks;
                    industry.id = ind.id;
                    industry.Balance = bs;
                    tempResult.Add(industry);
                }
                foreach (DateTime key in cashFlows.Keys)
                {
                    CashFlow bs = new CashFlow();
                    bs.ReportDate = key;
                    bs.Stock = new IndustrySmbol();
                    bs.Stock.InnerID = 80000000;
                    bs.Stock.StockName = ind.IndustryName;
                    Type type = bs.GetType();
                    System.Reflection.PropertyInfo[] properties = type.GetProperties();
                    foreach (System.Reflection.PropertyInfo property in properties)
                    {
                        if (property.GetValue(bs) == null)
                        {
                            double value = 0;
                            foreach (CashFlow sheets in cashFlows[key])
                            {
                                if (property.GetValue(sheets) != null)
                                {
                                    value += (double)property.GetValue(sheets);
                                }
                            }
                            property.SetValue(bs, value / cashFlows[key].Count);
                        }
                    }
                    Industry industry = new Industry();
                    industry.Date = key;
                    industry.IndustryName = ind.IndustryName;
                    industry.Stocks = ind.Stocks;
                    industry.id = ind.id;
                    industry.Cash = bs;
                    tempResult.Add(industry);
                }
                foreach (DateTime key in profitStatements.Keys)
                {
                    ProfitStatement bs = new ProfitStatement();
                    bs.ReportDate = key;
                    bs.Stock = new IndustrySmbol();
                    bs.Stock.InnerID = 80000000;
                    bs.Stock.StockName = ind.IndustryName;
                    Type type = bs.GetType();
                    System.Reflection.PropertyInfo[] properties = type.GetProperties();
                    foreach (System.Reflection.PropertyInfo property in properties)
                    {
                        if (property.GetValue(bs) == null)
                        {
                            double value = 0;
                            foreach (ProfitStatement sheets in profitStatements[key])
                            {
                                if (property.GetValue(sheets) != null)
                                {
                                    value += (double)property.GetValue(sheets);
                                }
                            }
                            property.SetValue(bs, value / profitStatements[key].Count);
                        }
                    }
                    Industry industry = new Industry();
                    industry.Date = key;
                    industry.IndustryName = ind.IndustryName;
                    industry.Stocks = ind.Stocks;
                    industry.id = ind.id;
                    industry.Profit = bs;
                    tempResult.Add(industry);
                }
            }
            return tempResult;
        }
        public ObjectId id
        {
            get;
            set;
        }

        public DateTime Date
        {
            get;
            set;
        }

        public string IndustryName
        {
            get;
            set;
        }

        public CashFlow Cash
        {
            get;
            set;
        }

        public BalanceSheet Balance
        {
            get;
            set;
        }

        public ProfitStatement Profit
        {
            get;
            set;
        }

        public FinancialReportAnalysis ReportAnalysis
        {
            get;
            set;
        }

        public decimal TotalCapital
        {
            get;
            set;
        }

        public List<StockBasic> Stocks
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            return obj.GetType().Equals(typeof(Industry)) && ((Industry)obj).IndustryName == IndustryName && ((Industry)obj).Date == Date;
        }
    }

    public class IndustrySmbol : StockBasic
    {
        public string IndustryName
        {
            get;
            set;
        }
    }

}
