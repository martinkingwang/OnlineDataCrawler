using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using OnlineDataCrawler.Util;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;

namespace OnlineDataCrawler.Data
{
    public class StockHistoryPrice :IDatabaseObject
    {
        private const string URL_HISTORY_DATA = "http://q.stock.sohu.com/hisHq?code=cn_{0}&start={1}&end={2}&stat=1&order=D&period=d&callback=historySearchHandler&rt=jsonp&r=0.27381376446028094&0.1785492201739558";

        private static DateTime lastPriceTime = new DateTime(1980, 1, 1);

        public static List<StockHistoryPrice> GetHistoryPrice(StockBasic stock, DateTime startDate, DateTime endDate)
        {
            List<StockHistoryPrice> stockHistory = new List<StockHistoryPrice>();

            TimeSpan time = DateTime.Now - lastPriceTime;
            if (time.TotalMilliseconds < 3000)
            {
                Thread.Sleep(3000 - (int)time.TotalMilliseconds);
            }

            StringBuilder sb = new StringBuilder();
            string start = startDate.ToString("yyyyMMdd");
            string end = endDate.ToString("yyyyMMdd");
            sb.AppendFormat(URL_HISTORY_DATA, stock.StockID, start, end);
            string response = HttpHelper.Get(sb.ToString());
            response = response.Replace("historySearchHandler(", "");
            response = response.Replace(")", "");
            JArray jArray = JArray.Parse(response);
            JObject jObject = jArray.Children()[0].Value<JObject>();
            JArray hq = jObject.Value<JArray>("hq");
            foreach (JToken item in hq)
            {
                StockHistoryPrice price = new StockHistoryPrice();
                DateTime date = DateTime.Now;
                var list = item.Children();
                price.Date = DateTime.ParseExact(list[0].ToString(), "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AssumeLocal);

                price.OpenPrice = list[1].Value<decimal>();
                price.ClosePrice = list[2].Value<decimal>();
                price.LowestPrice = list[5].Value<decimal>();
                price.HighestPrice = list[6].Value<decimal>();
                price.Volume = list[7].Value<int>();
                price.TurnoverAmount = (long)(list[8].Value<double>() * 10000);
                price.TurnoverRatio = double.Parse(list[9].Value<string>().Replace("%", ""));
                price.Stock = stock;
                stockHistory.Add(price);
            }
            lastPriceTime = DateTime.Now;
            return stockHistory;
        }

        public ObjectId id
        {
            get;
            set;
        }
        /// <summary>
        /// 开盘价格
        /// </summary>
        public decimal OpenPrice
        {
            get;
            set;
        }

        /// <summary>
        /// 收盘价格
        /// </summary>
        public decimal ClosePrice
        {
            get;
            set;
        }

        /// <summary>
        /// 最高价格
        /// </summary>
        public decimal HighestPrice
        {
            get;
            set;
        }

        /// <summary>
        /// 最低价格
        /// </summary>
        public decimal LowestPrice
        {
            get;
            set;
        }

        /// <summary>
        /// 成交量
        /// </summary>
        public int Volume
        {
            get;
            set;
        }

        /// <summary>
        /// 成交金额
        /// </summary>
        public long TurnoverAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 换手率
        /// </summary>
        public double TurnoverRatio
        {
            get;
            set;
        }

        /// <summary>
        /// 股票
        /// </summary>
        public StockBasic Stock
        {
            get;
            set;
        }

        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date
        {
            get;
            set;
        }

        /// <summary>
        /// 复权因子
        /// </summary>
        public double AnswerAuthority
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            if (!this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            var price2 = (StockHistoryPrice)obj;
            if (!Stock.Equals(price2.Stock))
                return false;
            if (!Date.Equals(price2.Date))
                return false;
            return true;
        }

        public bool ChcekBadataseIndex()
        {
            var dbHelper = DataStorage.GetInstance().DBHelper;
            if (!dbHelper.CollectionExists(typeof(StockHistoryPrice).Name))
            {
                var type = typeof(StockHistoryPrice);
                string[] name = new string[type.GetProperties().Length];
                for (int i = 0; i < name.Length; i++)
                {
                    name[i] = type.GetProperties()[i].Name;
                }
                dbHelper.CreateCollection<StockHistoryPrice>(name);
            }
            List<Expression<Func<StockHistoryPrice, object>>> fields = new List<Expression<Func<StockHistoryPrice, object>>>();
            List<int> direction = new List<int>();
            fields.Add(x => x.Stock.InnerID);
            direction.Add(1);
            fields.Add(x => x.Date);
            direction.Add(-1);
            dbHelper.CreateIndexes<StockHistoryPrice>(fields.ToArray(), direction.ToArray());
            return true;
        }
    }
}
