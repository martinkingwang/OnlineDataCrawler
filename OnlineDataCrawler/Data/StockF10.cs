using System;
using System.Collections.Generic;
using System.Text;
using OnlineDataCrawler.Util;
using Newtonsoft.Json.Linq;
using MongoDB.Bson;
using System.Linq.Expressions;

namespace OnlineDataCrawler.Data
{
    public class StockF10 :IDatabaseObject
    {

        private const string F10Url = "http://f10.eastmoney.com/CompanySurvey/CompanySurveyAjax?code={0}";

        public static StockF10 GetStockF10(StockBasic sb)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(F10Url, sb.MarketID + sb.StockID);
            string url = stringBuilder.ToString();
            string content = HttpHelper.Get(url);
            StockF10 f10 = new StockF10();
            JObject result = JObject.Parse(content);
            JToken jbzl = result["jbzl"];
            f10.InnerID = sb.InnerID;
            f10.StockID = sb.StockID;
            f10.StockName = sb.StockName;

            f10.CompanyName = jbzl.Value<string>("gsmc");
            f10.MarketType = jbzl.Value<string>("zqlb");
            f10.Industry = jbzl.Value<string>("sshy");
            f10.IndustryZJH = jbzl.Value<string>("sszjhhy");
            f10.Introduce = jbzl.Value<string>("gsjj");
            f10.Location = jbzl.Value<string>("qy");
            try
            {
                f10.Capital = NumberConventer.ChnToArab(jbzl.Value<string>("zczb"));
            }
            catch
            {
                f10.Capital = 0;
            }
            try
            {
                f10.Employee = jbzl.Value<int>("gyrs");
            }
            catch
            {
                f10.Employee = 0;
            }
            return f10;
        }

        public bool CheckDatabaseIndex()
        {
            var dbHelper = DataStorage.GetInstance().DBHelper;
            if (!dbHelper.CollectionExists(typeof(StockF10).Name))
            {
                var type = typeof(StockF10);
                string[] name = new string[type.GetProperties().Length];
                for (int i = 0; i < name.Length; i++)
                {
                    name[i] = type.GetProperties()[i].Name;
                }
                dbHelper.CreateCollection<StockF10>(name);
            }
            List<Expression<Func<StockF10, object>>> fields = new List<Expression<Func<StockF10, object>>>();
            List<int> direction = new List<int>();
            fields.Add(x => x.StockID);
            direction.Add(1);
            dbHelper.CreateIndexes<StockF10>(fields.ToArray(), direction.ToArray());
            return true;
        }

        public int InnerID
        {
            get;
            set;
        }

        public string CompanyName
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

        public string MarketType
        {
            get;
            set;
        }

        public string Industry
        {
            get;
            set;
        }

        public string IndustryZJH
        {
            get;
            set;
        }

        public string Introduce
        {
            get;
            set;
        }

        public string Location
        {
            get;
            set;
        }

        public decimal Capital
        {
            get;
            set;
        }

        public int Employee
        {
            get;
            set;
        }

        public ObjectId id
        {
            get;
            set;
        }
    }
}
