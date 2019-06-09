using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace OnlineDataCrawler.Data
{
    class ComplexFactor : IDatabaseObject
    {
        private DateTime _date;
        public DateTime Date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value.ToLocalTime();
            }
        }

        public double Factor
        {
            get;
            set;
        }

        public StockBasic Stock
        {
            get;
            set;
        }

        public bool CheckDatabaseIndex()
        {
            var dbHelper = DataStorage.GetInstance().DBHelper;
            if (!dbHelper.CollectionExists(typeof(ComplexFactor).Name))
            {
                var type = typeof(ComplexFactor);
                string[] name = new string[type.GetProperties().Length];
                for(int i = 0; i < name.Length; i++)
                {
                    name[i] = type.GetProperties()[i].Name;
                }
                dbHelper.CreateCollection<ComplexFactor>(name);
            }
            List<Expression<Func<ComplexFactor, object>>> fields = new List<Expression<Func<ComplexFactor, object>>>();
            List<int> direction = new List<int>();
            fields.Add(x => x.Stock.InnerID);
            direction.Add(1);
            fields.Add(x => x.Date);
            direction.Add(-1);
            dbHelper.CreateIndexes<ComplexFactor>(fields.ToArray(), direction.ToArray());
            return true;
        }

        public static List<StockHistoryPrice> ComputeComplexFactor(List<StockBonus> bonus, List<StockHistoryPrice> prices, StockHistoryPrice lastPrice)
        {
            var sortedBonus = bonus.OrderBy(x => x.BonusExemptionDate);
            var bonusArray = sortedBonus.ToArray();
            int bonusIndex = 0;
            decimal lastFactor = 1;
            if (lastPrice != null)
                lastFactor = lastPrice.AnswerAuthority;
            StockHistoryPrice lastDayPrice = null;
            foreach(var price in prices)
            {
                var b = bonusArray[bonusIndex];
                if(price.Date < b.BonusExemptionDate)
                {
                    price.AnswerAuthority = lastFactor;
                }
                else
                {
                    if(lastDayPrice != null)
                    {
                        decimal allotment = b.Allotment / 10 * b.AllotmentPrice;
                        decimal authority = (lastDayPrice.ClosePrice + allotment) / (1 + allotment + b.BonusStockGift);
                        authority = price.ClosePrice / authority;
                        lastFactor *= authority;
                        price.AnswerAuthority = lastFactor;
                    }
                    if(bonusIndex + 1 < bonusArray.Length)
                        bonusIndex++;
                }
                lastDayPrice = price;
            }
            return prices;
        }
    }
}
