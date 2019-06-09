using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MongoDB.Bson;
using OnlineDataCrawler.Util;
using System.Linq.Expressions;

namespace OnlineDataCrawler.Data
{
    public class StockBonus : IDatabaseObject
    {

        private const string StockBonusURL = "http://vip.stock.finance.sina.com.cn/corp/go.php/vISSUE_ShareBonus/stockid/{0}.phtml";
        private const string allotmentURL = "http://vip.stock.finance.sina.com.cn/corp/view/vISSUE_ShareBonusDetail.php?stockid={0}&type=2&end_date={1}";

        private static DateTime? lastRun;

        public static List<StockBonus> GetAllBonus(StockBasic stock)
        {
            if (lastRun == null)
            {
                lastRun = DateTime.Now;
            }
            else
            {
                var span = DateTime.Now - lastRun;
                if (span.Value.TotalMilliseconds < 3000)
                {
                    Thread.Sleep(3000 - (int)span.Value.TotalMilliseconds);
                }
            }
            List<StockBonus> result = new List<StockBonus>();
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(StockBonusURL, stock.StockID);
            string requestURL = sb.ToString();
            string response = string.Copy(HttpHelper.Get(requestURL));
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(response);
            HtmlNode node = document.DocumentNode;
            HtmlNode bonusTable = node.SelectNodes("//*[@id=\"sharebonus_1\"]/tbody")[0];
            foreach (HtmlNode row in bonusTable.ChildNodes)
            {
                int count = 0;
                StockBonus bonus = new StockBonus();
                bonus.Stock = stock;
                bool isContinue = true;
                if (row.Name != "tr")
                {
                    continue;
                }
                foreach (HtmlNode col in row.ChildNodes)
                {
                    if (col.Name == "td")
                    {
                        switch (count)
                        {
                            case 0:
                                DateTime date;
                                bool success = DateTime.TryParseExact(col.InnerText, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AdjustToUniversal, out date);
                                if(success)
                                    bonus.AllotmentBasementDate = new DateTime(date.Year - 1, 12, 31);
                                break;
                            case 1:
                                decimal gift = decimal.Parse(col.InnerText);
                                bonus.BonusStockGift = gift;
                                if (gift != 0)
                                    bonus.BonusType = bonus.BonusType | StockBonus.BonusTypeGift;
                                break;
                            case 2:
                                decimal increase = decimal.Parse(col.InnerText);
                                bonus.BonusStockIncrease = increase;
                                if (increase != 0)
                                    bonus.BonusType = bonus.BonusType | StockBonus.BonusTypeIncrease;
                                break;
                            case 3:
                                decimal cash = decimal.Parse(col.InnerText);
                                bonus.BonusCash = cash;
                                if (cash != 0)
                                    bonus.BonusType = bonus.BonusType | StockBonus.BonusTypeCash;
                                break;
                            case 4:
                                if (!col.InnerText.Equals("实施"))
                                {
                                    isContinue = false;
                                }
                                break;
                            case 5:
                                success = DateTime.TryParseExact(col.InnerText, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AdjustToUniversal, out date);
                                if (success)
                                {
                                    bonus.BonusExemptionDate = date;
                                }

                                break;
                            case 6:
                                success = DateTime.TryParseExact(col.InnerText, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AdjustToUniversal, out date);
                                if (success)
                                {
                                    bonus.BonusRegisterDate = date;
                                }

                                break;
                            case 7:
                                success = DateTime.TryParseExact(col.InnerText, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AdjustToUniversal, out date);
                                if (success)
                                {
                                    bonus.BonusListDate = date;
                                }

                                break;
                        }
                        if (!isContinue)
                        {
                            break;
                        }

                        count++;
                    }
                }
                if (isContinue)
                {
                    if(bonus.AllotmentBasementDate == null)
                    {
                        break;
                    }
                    if (bonus.AllotmentBasementDate.Value > new DateTime(1980, 1, 1))
                    {
                        bonus.id = ObjectId.GenerateNewId(bonus.AllotmentBasementDate.Value);
                        result.Add(bonus);
                    }
                }
            }
            HtmlNode allotmentTable = node.SelectNodes("//*[@id=\"sharebonus_2\"]/tbody")[0];
            foreach (HtmlNode row in allotmentTable.ChildNodes)
            {
                if (row.Name != "tr")
                {
                    continue;
                }
                var cols = from c in row.ChildNodes
                           where c.Name == "td"
                           select c;
                var col = cols.ToArray()[0];
                bool isContinue = true;
                StockBonus bonus = new StockBonus();
                DateTime date;
                bool success = DateTime.TryParseExact(col.InnerText, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AssumeLocal, out date);
                if (success)
                {
                    sb.Clear();
                    sb.AppendFormat(allotmentURL, stock.StockID, col.InnerText);
                    var responseDetail = string.Copy(HttpHelper.Get(sb.ToString()));
                    HtmlDocument detailDocument = new HtmlDocument();
                    detailDocument.LoadHtml(responseDetail);

                    var tables = detailDocument.GetElementbyId("sharebonusdetail");
                    foreach (var line in tables.ChildNodes)
                    {
                        if (line.Name != "tr")
                            continue;
                        var items = from item in line.ChildNodes
                                    where item.Name == "td"
                                    select item;
                        var listItem = items.ToArray();
                        if (items.Count() == 2)
                        {
                            switch (listItem[0].InnerText)
                            {
                                case "股本基准日":
                                    try
                                    {
                                        bonus.AllotmentBasementDate = DateTime.ParseExact(listItem[1].InnerText, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AssumeLocal);
                                    }
                                    catch
                                    {
                                        bonus.AllotmentBasementDate = null;
                                    }
                                    break;
                                case "登记日":
                                    try
                                    {
                                        bonus.BonusRegisterDate = DateTime.ParseExact(listItem[1].InnerText, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AssumeLocal);
                                    }
                                    catch
                                    {
                                        bonus.BonusRegisterDate = null;
                                    }
                                    break;
                                case "除息日":
                                    try
                                    {
                                        bonus.BonusExemptionDate = DateTime.ParseExact(listItem[1].InnerText, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AssumeLocal);
                                    }
                                    catch
                                    {
                                        bonus.BonusExemptionDate = null;
                                    }
                                    break;
                                case "上市日":
                                    try
                                    {
                                        bonus.BonusListDate = DateTime.ParseExact(listItem[1].InnerText, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AssumeLocal);
                                    }
                                    catch
                                    {
                                        bonus.BonusListDate = null;
                                    }
                                    break;
                                case "配股比例（10配）":
                                    try
                                    {
                                        bonus.Allotment = decimal.Parse(listItem[1].InnerText);
                                    }
                                    catch
                                    {
                                        bonus.Allotment = 0;
                                    }
                                    break;
                                case "配股价":
                                    try
                                    {
                                        bonus.AllotmentPrice = decimal.Parse(listItem[1].InnerText);
                                    }
                                    catch
                                    {
                                        bonus.AllotmentPrice = 0;
                                    }
                                    break;
                                case "股东大会决议公告日期":
                                    try
                                    {
                                        bonus.AnnounceDate = DateTime.ParseExact(listItem[1].InnerText, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AssumeLocal);
                                    }
                                    catch
                                    {
                                        bonus.AnnounceDate = null;
                                    }
                                    break;
                            }
                        }
                    }
                    if (bonus.AllotmentBasementDate == null)
                    {
                        if(bonus.AnnounceDate != null)
                            bonus.AllotmentBasementDate = new DateTime(bonus.AnnounceDate.Value.Year - 1, 12, 31);
                        else if(bonus.BonusListDate != null)
                            bonus.AllotmentBasementDate = new DateTime(bonus.BonusListDate.Value.Year - 1, 12, 31);
                    }
                    bonus.BonusType = StockBonus.BonusTypeAllotment;
                    if (bonus.AllotmentBasementDate != null)
                        bonus.id = ObjectId.GenerateNewId(bonus.AllotmentBasementDate.Value);
                    else if (bonus.BonusExemptionDate != null)
                        bonus.id = ObjectId.GenerateNewId(bonus.BonusExemptionDate.Value);
                    else
                        bonus.id = ObjectId.GenerateNewId();
                    bonus.Stock = stock;
                    result.Add(bonus);
                    if (!isContinue)
                    {
                        break;
                    }
                }
            }
            lastRun = DateTime.Now;

            return result;
        }

        public const int BonusTypeCash = 1;
        public const int BonusTypeIncrease = 2;
        public const int BonusTypeGift = 4;
        public const int BonusTypeAllotment = 8;

        public ObjectId id
        {
            get;
            set;
        }
        public StockBasic Stock
        {
            get;
            set;
        }
        /// <summary>
        /// 送股
        /// </summary>
        public decimal BonusStockGift
        {
            get;
            set;
        }

        /// <summary>
        /// 转增
        /// </summary>
        public decimal BonusStockIncrease
        {
            get;
            set;
        }

        /// <summary>
        /// 现金分红
        /// </summary>
        public decimal BonusCash
        {
            get;
            set;
        }

        /// <summary>
        /// 配股
        /// </summary>
        public decimal Allotment
        {
            get;
            set;
        }

        /// <summary>
        /// 配股价格
        /// </summary>
        public decimal AllotmentPrice
        {
            get;
            set;
        }

        private DateTime? _allotmentBasementDate;
        /// <summary>
        /// 股本基准日
        /// </summary>
        public DateTime? AllotmentBasementDate
        {
            get
            {
                return _allotmentBasementDate;
            }
            set
            {
                if (value != null)
                    _allotmentBasementDate = value.Value.ToLocalTime();
                else
                    _allotmentBasementDate = null;
            }
        }

        private DateTime? _bonusExemptionDate;
        /// <summary>
        /// 除权日
        /// </summary>
        public DateTime? BonusExemptionDate
        {
            get
            {
                return _bonusExemptionDate;
            }
            set
            {
                if (value != null)
                    _bonusExemptionDate = value.Value.ToLocalTime();
                else
                    _bonusExemptionDate = null;
            }
        }

        private DateTime? _bonusRegisterDate;

        /// <summary>
        /// 登记日
        /// </summary>
        public DateTime? BonusRegisterDate
        {
            get
            {
                return _bonusRegisterDate;
            }
            set
            {
                if (value != null)
                    _bonusRegisterDate = value.Value.ToLocalTime();
                else
                    _bonusRegisterDate = null;
            }
        }

        private DateTime? _bonusListDate;

        public DateTime? BonusListDate
        {
            get
            {
                return _bonusListDate;
            }
            set
            {
                if (value != null)
                    _bonusListDate = value.Value.ToLocalTime();
                else
                    _bonusListDate = null;
            }
        }

        private DateTime? _announceDate;

        public DateTime? AnnounceDate
        {
            get
            {
                return _announceDate;
            }
            set
            {
                if (value != null)
                    _announceDate = value.Value.ToLocalTime();
                else
                    _announceDate = null;
            }
        }

        /// <summary>
        /// 1 现金分红 2转增 4 送股  8 配股
        /// </summary>
        public int BonusType
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == this.GetType())
            {
                StockBonus bonus = (StockBonus)obj;
                if (bonus.BonusType == BonusType && bonus.AllotmentBasementDate == AllotmentBasementDate && bonus.Stock == Stock)
                    return true;
            }
            return false;
        }

        public bool CheckDatabaseIndex()
        {
            var dbHelper = DataStorage.GetInstance().DBHelper;
            if (!dbHelper.CollectionExists(typeof(StockBonus).Name))
            {
                var type = typeof(StockBonus);
                string[] name = new string[type.GetProperties().Length];
                for (int i = 0; i < name.Length; i++)
                {
                    name[i] = type.GetProperties()[i].Name;
                }
                dbHelper.CreateCollection<StockBonus>(name);
            }
            List<Expression<Func<StockBonus, object>>> fields = new List<Expression<Func<StockBonus, object>>>();
            List<int> direction = new List<int>();
            fields.Add(x => x.Stock.InnerID);
            direction.Add(1);
            fields.Add(x => x.BonusListDate);
            direction.Add(-1);
            dbHelper.CreateIndexes<StockBonus>(fields.ToArray(), direction.ToArray());
            return true;
        }
    }
}
