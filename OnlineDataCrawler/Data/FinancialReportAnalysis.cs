using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace OnlineDataCrawler.Data
{
    public class FinancialReportAnalysis
    {
        BalanceSheet balanceSheet;
        ProfitStatement profitStatement;
        CashFlow cashFlow;
        DataStorage ds;

        public FinancialReportAnalysis()
        {

        }

        public FinancialReportAnalysis(BalanceSheet bs, ProfitStatement ps, CashFlow cf)
        {
            if (bs == null || ps == null)
                return;
            balanceSheet = bs;
            profitStatement = ps;
            cashFlow = cf;
            ds = DataStorage.GetInstance();
            AssetLiablityRatio = divide(bs.SumLiab, bs.SumAsset);
            if (bs.Stock.InnerID < 80000000)
                PERatio = divide((double)ds.GetHistoryPrices(bs.Stock, bs.ReportDate, bs.ReportDate)[0].ClosePrice, ps.BasicEPS);
            LiquidityRatio = divide(bs.SumCurrentAsset, bs.SumLLiab);
            QuickRatio = divide((bs.SumCurrentAsset - bs.Inventory), bs.SumLLiab);
            TotalAssetTurnoverRate = divide(ps.TotalOPerateRevenue, bs.SumAsset);
            ReceivableTurnoverRate = divide(ps.TotalOPerateRevenue, bs.AccountRec);
            InventoryTurnoverRate = divide(ps.TotalOPerateRevenue, bs.Inventory);
            AdvanceReceiveRate = divide(ps.TotalOPerateRevenue, bs.AdvanceRecieve);
            AdvancePayRate = divide(ps.totalOperateExpend, bs.AdvancePay);
            GrossProfitRate = divide((ps.TotalOPerateRevenue - ps.OperateExpend), ps.TotalOPerateRevenue);
            NetProfitRate = divide(ps.NetProfit, ps.TotalOPerateRevenue);
            if (cf != null)
            {
                OperateIncomeRevenueRate = divide(cf.NetOperateCashFlow, ps.TotalOPerateRevenue);
            }
            else
            {
                OperateIncomeRevenueRate = 0;
            }
        }

        private double divide(double? a, double? b)
        {
            double result = 0.0;
            try
            {
                result = (double)a / (double)b;
            }
            catch (Exception ex)
            {
                Logging.Log.Error("devided by zero");
            }
            return result;
        }
        //资产负债率、市盈率、EBITDA、
        //流动比率、速动比率、总资产周转率、应收账款周转率、存货周转率、预收周转率、预付周转率、
        //毛利润率、净利润率

        public StockBasic Stock
        {
            get;
            set;
        }

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

        public ObjectId id
        {
            get;
            set;
        }
        /// <summary>
        /// 资产负债率
        /// </summary>
        public double AssetLiablityRatio
        {
            get;
            set;
        }

        /// <summary>
        /// 市盈率
        /// </summary>
        public double PERatio
        {
            get;
            set;
        }

        /// <summary>
        /// 流动比率
        /// </summary>
        public double LiquidityRatio
        {
            get;
            set;
        }

        /// <summary>
        /// 速动比率
        /// </summary>
        public double QuickRatio
        {
            get;
            set;
        }

        /// <summary>
        /// 总资产周转率
        /// </summary>
        public double TotalAssetTurnoverRate
        {
            get;
            set;
        }

        /// <summary>
        /// 应收账款周转率
        /// </summary>
        public double ReceivableTurnoverRate
        {
            get;
            set;
        }

        /// <summary>
        /// 存货周转率
        /// </summary>
        public double InventoryTurnoverRate
        {
            get;
            set;
        }

        /// <summary>
        /// 预收账款周转率
        /// </summary>
        public double AdvanceReceiveRate
        {
            get;
            set;
        }

        /// <summary>
        /// 预付周转率
        /// </summary>
        public double AdvancePayRate
        {
            get;
            set;
        }

        /// <summary>
        /// 毛利润率
        /// </summary>
        public double GrossProfitRate
        {
            get;
            set;
        }

        /// <summary>
        /// 净利润率
        /// </summary>
        public double NetProfitRate
        {
            get;
            set;
        }

        /// <summary>
        /// 经营活动现金流收入比率
        /// </summary>
        public double OperateIncomeRevenueRate
        {
            get;
            set;
        }
    }
}
