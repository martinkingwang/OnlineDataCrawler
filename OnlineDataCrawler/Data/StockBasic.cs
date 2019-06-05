using MongoDB.Bson;
using OnlineDataCrawler.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace OnlineDataCrawler.Data
{
    public class StockBasic
    {
        private const string AllStockURL = "http://nufm.dfcfw.com/EM_Finance2014NumericApplication/JS.aspx?cb=jQuery11240744221785830699_1553645836529&type=CT&token=4f1862fc3b5e77c150a2b985b12db0fd&sty=FCOIATC&js=(%7Bdata%3A%5B(x)%5D%2CrecordsFiltered%3A(tot)%7D)&cmd={2}&st=(Code)&sr=1&p={0}&ps=20&_={1}";
        private const string CmdShanghai = "C.2";
        private const string CmdShenZhen = "C._SZAME";
        private const string CmdFengxian = "C._AB_FXJS";
        private const string CmdAllStock = "C._A";
        private const string CmdNewStock = "C.BK05011";
        private static long WebID = 1553645837459;

        public static List<StockBasic> GetAllStocks()
        {
            List<StockBasic> result = new List<StockBasic>();
            result.AddRange(GetStocks(CmdAllStock));
            result.AddRange(GetStocks(CmdFengxian));
            result.AddRange(GetStocks(CmdNewStock));
            return result;
        }

        private static List<StockBasic> GetStocks(string cmd)
        {
            int count = 0;
            bool isContinue = true;
            List<StockBasic> result = new List<StockBasic>();
            int page = 0;
            while (isContinue)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(AllStockURL, page, WebID, cmd);
                WebID++;
                page++;
                string content = HttpHelper.Get(sb.ToString());
                Regex r = new Regex(",recordsFiltered:\\d+");
                Match matchResult = r.Match(content);
                string stockCount = matchResult.Value;
                content = content.Remove(matchResult.Index, stockCount.Length + 2);
                stockCount = stockCount.Replace(",recordsFiltered:", "");
                count = int.Parse(stockCount);
                r = new Regex("data:\\[");
                matchResult = r.Match(content);
                string data = matchResult.Value;
                content = content.Remove(0, matchResult.Index + data.Length);
                content = content.Replace("]", "");
                string[] arrays = content.Split("\"".ToArray());
                foreach (string d in arrays)
                {
                    string str = d.Replace("\"", "");
                    StockBasic stock = new StockBasic();
                    string[] items = d.Split(",".ToArray());
                    if (items[0] == "")
                    {
                        continue;
                    }

                    if (items[0] == "1")
                    {
                        stock.MarketID = "sh";
                        stock.MarketName = "shanghai";
                        stock.InnerID = 10000000;
                    }
                    else
                    {
                        stock.MarketID = "sz";
                        stock.MarketName = "shenzhen";
                        stock.InnerID = 20000000;
                    }
                    stock.StockID = items[1];
                    stock.InnerID = stock.InnerID + int.Parse(stock.StockID);
                    stock.StockName = items[2];
                    stock.id = ObjectId.GenerateNewId(stock.InnerID);
                    result.Add(stock);
                }
                if (result.Count >= count)
                {
                    isContinue = false;
                }
                else
                {
                    Thread.Sleep(2000);
                }
            }
            return result;
        }

        /// <summary>
        /// shanghai started with 1, shenzhen started with 2, industry start 3
        /// </summary>
        public int InnerID
        {
            get;
            set;
        }

        public string StockID
        {
            get;
            set;
        }

        public string StockName
        {
            get;
            set;
        }

        /// <summary>
        /// industry for "industry"
        /// </summary>
        public string MarketID
        {
            get;
            set;
        }

        public string MarketName
        {
            get;
            set;
        }
        public ObjectId id
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            if (!obj.GetType().Equals(this.GetType()))
            {
                return false;
            }
            StockBasic stock = (StockBasic)obj;
            return StockID.Equals(stock.StockID);
        }
    }
}
