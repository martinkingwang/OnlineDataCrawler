using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlineDataCrawler.Util;
using HtmlAgilityPack;
using MongoDB.Bson;
using System.Linq.Expressions;

namespace OnlineDataCrawler.Data
{
    public class MarketIndex : IDatabaseObject
    {
        DateTime _date;
        public ObjectId id
        {
            get;
            set;
        }

        public decimal OpenIndex
        {
            get;
            set;
        }

        public decimal CloseIndex
        {
            get;
            set;
        }

        public decimal HighestIndex
        {
            get;
            set;
        }

        public decimal LowestIndex
        {
            get;
            set;
        }

        public DateTime Date
        {
            get { return _date; }
            set
            {
                _date = value.ToLocalTime();
            }
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == GetType())
            {
                if (((MarketIndex)obj).Date == Date)
                    return true;
            }
            return false;
        }

        private const string Zhongzheng500ID = "000905";
        private const string GetAllIndexURL = "http://quotes.money.163.com/trade/lsjysj_zhishu_{0}.html?year={1}&season={2}";
        public static List<MarketIndex> GetAllIndex(DateTime start, DateTime end)
        {
            List<MarketIndex> indices = new List<MarketIndex>();
            int startSeason = DateHelper.GetSeason(start);
            int startYear = start.Year;
            int currentSeason = DateHelper.GetSeason(end);
            int currentYear = end.Year;
            while (currentYear > startYear || (currentYear == startYear && currentSeason >= startYear))
            {
                string content = HttpHelper.Get(string.Format(GetAllIndexURL, Zhongzheng500ID, currentYear, currentSeason));
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(content);
                // /html/body/div[2]/div[3]/table/tbody/tr[1]
                var tbody = document.DocumentNode.SelectSingleNode("/html/body/div[2]/div[3]/table");
                foreach (var tr in tbody.ChildNodes)
                {
                    if (tr.Name != "tr")
                        continue;
                    MarketIndex index = new MarketIndex();
                    int count = 0;
                    foreach (var td in tr.ChildNodes)
                    {
                        if (td.Name != "td")
                            continue;
                        switch (count)
                        {
                            case 0:
                                DateTime date = DateTime.ParseExact(td.InnerText, "yyyyMMdd", null, System.Globalization.DateTimeStyles.AssumeLocal);
                                index.Date = date;
                                break;
                            case 1:
                                index.OpenIndex = decimal.Parse(td.InnerText);
                                break;
                            case 2:
                                index.HighestIndex = decimal.Parse(td.InnerText);
                                break;
                            case 3:
                                index.LowestIndex = decimal.Parse(td.InnerText);
                                break;
                            case 4:
                                index.CloseIndex = decimal.Parse(td.InnerText);
                                break;
                        }
                        count++;
                    }
                    indices.Add(index);
                }
                if (currentSeason > 1)
                    currentSeason--;
                else
                {
                    currentSeason = 4;
                    currentYear--;
                }
            }
            return indices;
        }

        public bool ChcekBadataseIndex()
        {
            var dbHelper = DataStorage.GetInstance().DBHelper;
            if (!dbHelper.CollectionExists(typeof(MarketIndex).Name))
            {
                var type = typeof(MarketIndex);
                string[] name = new string[type.GetProperties().Length];
                for (int i = 0; i < name.Length; i++)
                {
                    name[i] = type.GetProperties()[i].Name;
                }
                dbHelper.CreateCollection<MarketIndex>(name);
            }
            List<Expression<Func<MarketIndex, object>>> fields = new List<Expression<Func<MarketIndex, object>>>();
            List<int> direction = new List<int>();
            fields.Add(x => x.Date);
            direction.Add(-1);
            dbHelper.CreateIndexes<MarketIndex>(fields.ToArray(), direction.ToArray());
            return true;
        }
    }
}
