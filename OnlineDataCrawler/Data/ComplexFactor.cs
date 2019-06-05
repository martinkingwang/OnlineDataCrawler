using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public bool ChcekBadataseIndex()
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
            var collection = dbHelper.CreateIndexesAsync()
            return true;

        }
    }
}
