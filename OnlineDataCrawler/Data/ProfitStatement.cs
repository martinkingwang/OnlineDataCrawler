using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OnlineDataCrawler.Data
{
    public class ProfitStatement : FinancialReport
    {
        /// <summary>
        /// 营业收入
        /// </summary>
        [JsonProperty(PropertyName = "TOTALOPERATEREVE")]
        public double? TotalOPerateRevenue
        {
            get;
            set;
        }

        /// <summary>
        /// 总营业支出
        /// </summary>
        [JsonProperty(PropertyName = "TOTALOPERATEEXP")]
        public double? totalOperateExpend
        {
            get;
            set;
        }

        /// <summary>
        /// 营业成本
        /// </summary>
        [JsonProperty(PropertyName = "OPERATEEXP")]
        public double? OperateExpend
        {
            get;
            set;
        }

        /// <summary>
        /// 营业税金
        /// </summary>
        [JsonProperty(PropertyName = "OPERATETAX")]
        public double? OperateTax
        {
            get;
            set;
        }

        /// <summary>
        /// 销售费用
        /// </summary>
        [JsonProperty(PropertyName = "SALEEXP")]
        public double? SaleExpend
        {
            get;
            set;
        }

        /// <summary>
        /// 管理费用
        /// </summary>
        [JsonProperty(PropertyName = "MANAGEEXP")]
        public double? ManageExpend
        {
            get;
            set;
        }

        /// <summary>
        /// 财务费用
        /// </summary>
        [JsonProperty(PropertyName = "FINANCEEXP")]
        public double? FinanceExpend
        {
            get;
            set;
        }

        /// <summary>
        /// 资产减值损失
        /// </summary>
        [JsonProperty(PropertyName = "ASSETDEVALUELOSS")]
        public double? AssetDevalueLoss
        {
            get;
            set;
        }

        /// <summary>
        /// 投资收益
        /// </summary>
        [JsonProperty(PropertyName = "INVESTINCOME")]
        public double? InvestIncome
        {
            get;
            set;
        }

        /// <summary>
        /// 其中:对联营企业和合营企业的投资收益 
        /// </summary>
        [JsonProperty(PropertyName = "INVESTJOINTINCOME")]
        public double? InvestJointIncome
        {
            get;
            set;
        }

        /// <summary>
        /// 营业利润
        /// </summary>
        [JsonProperty(PropertyName = "OPERATEPROFIT")]
        public double? OperateProfit
        {
            get;
            set;
        }

        /// <summary>
        /// 营业外收入
        /// </summary>
        [JsonProperty(PropertyName = "NONOPERATEREVE")]
        public double? NonOperateRevenue
        {
            get;
            set;
        }

        /// <summary>
        /// 营业外支出
        /// </summary>
        [JsonProperty(PropertyName = "NONOPERATEEXP")]
        public double? NonOperateExpend
        {
            get;
            set;
        }

        /// <summary>
        /// 利润总额
        /// </summary>
        [JsonProperty(PropertyName = "SUMPROFIT")]
        public double? SumProfit
        {
            get;
            set;
        }

        /// <summary>
        /// 所得税
        /// </summary>
        [JsonProperty(PropertyName = "INCOMETAX")]
        public double? IncomeTax
        {
            get;
            set;
        }

        /// <summary>
        /// 净利润
        /// </summary>
        [JsonProperty(PropertyName = "NETPROFIT")]
        public double? NetProfit
        {
            get;
            set;
        }

        /// <summary>
        /// 基本每股收益
        /// </summary>
        [JsonProperty(PropertyName = "BASICEPS")]
        public double? BasicEPS
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }
}
