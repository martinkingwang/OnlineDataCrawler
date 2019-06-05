using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MongoDB.Bson;

namespace OnlineDataCrawler.Data
{
    public class BalanceSheet : FinancialReport
    {
        /// <summary>
        /// 货币资金
        /// </summary>
        [JsonProperty(PropertyName = "MONETARYFUND")]
        public double? MonetaryFund
        {
            get;
            set;
        }

        /// <summary>
        /// 应收票据
        /// </summary>
        [JsonProperty(PropertyName = "BILLREC")]
        public double? BillRec
        {
            get;
            set;
        }

        /// <summary>
        /// 交易性金融资产
        /// </summary>
        [JsonProperty(PropertyName = "TRADEFASSET")]
        public double? TransactionalFinancialAssets
        {
            get;
            set;
        }

        /// <summary>
        /// 应收账款
        /// </summary>
        [JsonProperty(PropertyName = "ACCOUNTREC")]
        public double? AccountRec
        {
            get;
            set;
        }

        /// <summary>
        /// 预付账款
        /// </summary>
        [JsonProperty(PropertyName = "ADVANCEPAY")]
        public double? AdvancePay
        {
            get;
            set;
        }

        /// <summary>
        /// 应收股利
        /// </summary>
        [JsonProperty(PropertyName = "DIVIDENDREC")]
        public double? DevideRec
        {
            get;
            set;
        }

        /// <summary>
        /// 应收利息
        /// </summary>
        [JsonProperty(PropertyName = "INTERESTREC")]
        public double? InterestRec
        {
            get;
            set;
        }

        /// <summary>
        /// 其他应收
        /// </summary>
        [JsonProperty(PropertyName = "OTHERREC")]
        public double? OtherRec
        {
            get;
            set;
        }

        /// <summary>
        /// 存货
        /// </summary>
        [JsonProperty(PropertyName = "INVENTORY")]
        public double? Inventory
        {
            get;
            set;
        }

        /// <summary>
        /// 一年内到期的非流动资产
        /// </summary>
        [JsonProperty(PropertyName = "NONLASSETONEYEAR")]
        public double? NonLongOneYear
        {
            get;
            set;
        }

        /// <summary>
        /// 其他流动资产
        /// </summary>
        [JsonProperty(PropertyName = "OTHERLASSET")]
        public double? OtherCurrentAsset
        {
            get;
            set;
        }

        /// <summary>
        /// 流动资产合计
        /// </summary>
        [JsonProperty(PropertyName = "SUMLASSET")]
        public double? SumCurrentAsset
        {
            get;
            set;
        }

        /// <summary>
        /// 可供出售金融资产
        /// </summary>
        [JsonProperty(PropertyName = "SALEABLEFASSET")]
        public double? SaleAsset
        {
            get;
            set;
        }

        /// <summary>
        /// 长期应收款
        /// </summary>
        [JsonProperty(PropertyName = "LTREC")]
        public double? LongTermRec
        {
            get;
            set;
        }

        /// <summary>
        /// 长期股权投资
        /// </summary>
        [JsonProperty(PropertyName = "LTEQUITYINV")]
        public double? LongTermInv
        {
            get;
            set;
        }

        /// <summary>
        /// 投资性房地产
        /// </summary>
        [JsonProperty(PropertyName = "ESTATEINVEST")]
        public double? EstateInv
        {
            get;
            set;
        }

        /// <summary>
        /// 固定资产
        /// </summary>
        [JsonProperty(PropertyName = "FIXEDASSET")]
        public double? FixedAsset
        {
            get;
            set;
        }

        /// <summary>
        /// 工程物资
        /// </summary>
        [JsonProperty(PropertyName = "CONSTRUCTIONMATERIAL")]
        public double? ConstructionMaterial
        {
            get;
            set;
        }

        /// <summary>
        /// 固定资产清理
        /// </summary>
        [JsonProperty(PropertyName = "LIQUIDATEFIXEDASSET")]
        public double? LiquidateFixedAsset
        {
            get;
            set;
        }

        /// <summary>
        /// 生产性生物资产
        /// </summary>
        [JsonProperty(PropertyName = "PRODUCTBIOLOGYASSET")]
        public double? ProductBiologyAsset
        {
            get;
            set;
        }

        /// <summary>
        /// 油气资产
        /// </summary>
        [JsonProperty(PropertyName = "OILGASASSET")]
        public double? OilGasAsset
        {
            get;
            set;
        }

        /// <summary>
        /// 无形资产
        /// </summary>
        [JsonProperty(PropertyName = "INTANGIBLEASSET")]
        public double? IntangibleAsset
        {
            get;
            set;
        }

        /// <summary>
        /// 开发支出
        /// </summary>
        [JsonProperty(PropertyName = "DEVELOPEXP")]
        public double? DevelopExp
        {
            get;
            set;
        }

        /// <summary>
        /// 商誉
        /// </summary>
        [JsonProperty(PropertyName = "GOODWILL")]
        public double? GoodWill
        {
            get;
            set;
        }

        /// <summary>
        /// 长期待摊费用
        /// </summary>
        [JsonProperty(PropertyName = "LTDEFERASSET")]
        public double? LongTermDeferAsset
        {
            get;
            set;
        }

        /// <summary>
        /// 抵押所得税资产
        /// </summary>
        [JsonProperty(PropertyName = "DEFERINCOMETAXASSET")]
        public double? DeferINcomeTaxAsset
        {
            get;
            set;
        }

        /// <summary>
        /// 其他非流动资产
        /// </summary>
        [JsonProperty(PropertyName = "OTHERNONLASSET")]
        public double? OtherNonCurrentAsset
        {
            get;
            set;
        }

        /// <summary>
        /// 非流动资产合计
        /// </summary>
        [JsonProperty(PropertyName = "SUMNONLASSET")]
        public double? SumNonCurrentAsset
        {
            get;
            set;
        }

        /// <summary>
        /// 资产合计
        /// </summary>
        [JsonProperty(PropertyName = "SUMASSET")]
        public double? SumAsset
        {
            get;
            set;
        }

        /// <summary>
        /// 短期借款
        /// </summary>
        [JsonProperty(PropertyName = "STBORROW")]
        public double? ShortTermBorrow
        {
            get;
            set;
        }

        /// <summary>
        /// 央行借款
        /// </summary>
        [JsonProperty(PropertyName = "BORROWFROMCBANK")]
        public double? BorrowFromCBank
        {
            get;
            set;
        }

        /// <summary>
        /// 吸收存款及同业存放
        /// </summary>
        [JsonProperty(PropertyName = "DEPOSIT")]
        public double? Deposit
        {
            get;
            set;
        }

        /// <summary>
        /// 拆入资金
        /// </summary>
        [JsonProperty(PropertyName = "BORROWFUND")]
        public double? BorrowFund
        {
            get;
            set;
        }

        /// <summary>
        /// 以公允价值计量且其变动计入当期损益的金融负债
        /// </summary>
        [JsonProperty(PropertyName = "FVALUEFLIAB")]
        public double? FValueLiab
        {
            get;
            set;
        }

        /// <summary>
        /// 应付票据
        /// </summary>
        [JsonProperty(PropertyName = "BILLPAY")]
        public double? BillPay
        {
            get;
            set;
        }

        /// <summary>
        /// 应付账款
        /// </summary>
        [JsonProperty(PropertyName = "ACCOUNTPAY")]
        public double? AccountPay
        {
            get;
            set;
        }

        /// <summary>
        /// 预收款项
        /// </summary>
        [JsonProperty(PropertyName = "ADVANCERECEIVE")]
        public double? AdvanceRecieve
        {
            get;
            set;
        }

        /// <summary>
        /// 卖出回购金融资产款
        /// </summary>
        [JsonProperty(PropertyName = "SELLBUYBACKFASSET")]
        public double? SellBuyBackAsset
        {
            get;
            set;
        }

        /// <summary>
        /// 应付手续费及佣金
        /// </summary>
        [JsonProperty(PropertyName = "COMMPAY")]
        public double? CommisionPay
        {
            get;
            set;
        }

        /// <summary>
        /// 应付职工薪酬
        /// </summary>
        [JsonProperty(PropertyName = "SALARYPAY")]
        public double? SalaryPay
        {
            get;
            set;
        }

        /// <summary>
        /// 应交税费
        /// </summary>
        [JsonProperty(PropertyName = "TAXPAY")]
        public double? TaxPay
        {
            get;
            set;
        }

        /// <summary>
        /// 应付利息
        /// </summary>
        [JsonProperty(PropertyName = "INTERESTPAY")]
        public double? InterestsPay
        {
            get;
            set;
        }

        /// <summary>
        /// 应付股利
        /// </summary>
        [JsonProperty(PropertyName = "DIVIDENDPAY")]
        public double? DividendPay
        {
            get;
            set;
        }

        /// <summary>
        /// 应付分保账款
        /// </summary>
        [JsonProperty(PropertyName = "RIPAY")]
        public double? RIPay
        {
            get;
            set;
        }

        /// <summary>
        /// 内部应付款
        /// </summary>
        [JsonProperty(PropertyName = "INTERNALPAY")]
        public double? InternalPay
        {
            get;
            set;
        }

        /// <summary>
        /// 其他应付款
        /// </summary>
        [JsonProperty(PropertyName = "OTHERPAY")]
        public double? OtherPay
        {
            get;
            set;
        }

        /// <summary>
        /// 预计流动负债
        /// </summary>
        [JsonProperty(PropertyName = "ANTICIPATELLIAB")]
        public double? AnticipatelLIAB
        {
            get;
            set;
        }

        /// <summary>
        /// 保险合同准备金
        /// </summary>
        [JsonProperty(PropertyName = "CONTACTRESERVE")]
        public double? ContactReserve
        {
            get;
            set;
        }

        /// <summary>
        /// 代理买卖证券款
        /// </summary>
        [JsonProperty(PropertyName = "AGENTTRADESECURITY")]
        public double? AgentTradeSecurity
        {
            get;
            set;
        }

        /// <summary>
        /// 代理承销证券款
        /// </summary>
        [JsonProperty(PropertyName = "AGENTUWSECURITY")]
        public double? AgentUWSecurity
        {
            get;
            set;
        }

        /// <summary>
        /// 一年内的递延收益
        /// </summary>
        [JsonProperty(PropertyName = "DEFERINCOMEONEYEAR")]
        public double? DeferIncomeOneYear
        {
            get;
            set;
        }

        /// <summary>
        /// 应付短期债券
        /// </summary>
        [JsonProperty(PropertyName = "STBONDREC")]
        public double? ShortTermBondRec
        {
            get;
            set;
        }

        /// <summary>
        /// 划分为持有待售的负债
        /// </summary>
        [JsonProperty(PropertyName = "CLHELDSALELIAB")]
        public double? CLHeldSaleLiab
        {
            get;
            set;
        }

        /// <summary>
        /// 一年内到期的非流动负债
        /// </summary>
        [JsonProperty(PropertyName = "NONLLIABONEYEAR")]
        public double? NonLiabOneYear
        {
            get;
            set;
        }

        /// <summary>
        /// 其他流动负债
        /// </summary>
        [JsonProperty(PropertyName = "OTHERLLIAB")]
        public double? OtherLiab
        {
            get;
            set;
        }

        /// <summary>
        /// 流动负债合计
        /// </summary>
        [JsonProperty(PropertyName = "SUMLLIAB")]
        public double? SumLLiab
        {
            get;
            set;
        }

        /// <summary>
        /// 长期借款
        /// </summary>
        [JsonProperty(PropertyName = "LTBORROW")]
        public double? LongTermBorrow
        {
            get;
            set;
        }

        /// <summary>
        /// 应付债券
        /// </summary>
        [JsonProperty(PropertyName = "BONDPAY")]
        public double? BondPay
        {
            get;
            set;
        }

        /// <summary>
        /// 长期应付款
        /// </summary>
        [JsonProperty(PropertyName = "LTACCOUNTPAY")]
        public double? LongTermAccountPay
        {
            get;
            set;
        }

        /// <summary>
        /// 长期应付职工薪酬
        /// </summary>
        [JsonProperty(PropertyName = "LTSALARYPAY")]
        public double? LongTermSalaryPay
        {
            get;
            set;
        }

        /// <summary>
        /// 专项应付款
        /// </summary>
        [JsonProperty(PropertyName = "SPECIALPAY")]
        public double? SpecialPay
        {
            get;
            set;
        }

        /// <summary>
        /// 预计负债
        /// </summary>
        [JsonProperty(PropertyName = "ANTICIPATELIAB")]
        public double? AnticipateLiab
        {
            get;
            set;
        }

        /// <summary>
        /// 递延收益
        /// </summary>
        [JsonProperty(PropertyName = "DEFERINCOME")]
        public double? DeferIncome
        {
            get;
            set;
        }

        /// <summary>
        /// 递延所得税负债
        /// </summary>
        [JsonProperty(PropertyName = "DEFERINCOMETAXLIAB")]
        public double? DeferIncomeTaxLiab
        {
            get;
            set;
        }

        /// <summary>
        /// 其他非流动负债
        /// </summary>
        [JsonProperty(PropertyName = "OTHERNONLLIAB")]
        public double? OtherNoneLongLiab
        {
            get;
            set;
        }

        /// <summary>
        /// 非流动负债合计
        /// </summary>
        [JsonProperty(PropertyName = "SUMNONLLIAB")]
        public double? SumNonLiab
        {
            get;
            set;
        }

        /// <summary>
        /// 负债合计
        /// </summary>
        [JsonProperty(PropertyName = "SUMLIAB")]
        public double? SumLiab
        {
            get;
            set;
        }

        /// <summary>
        /// 实收资本（或股本）
        /// </summary>
        [JsonProperty(PropertyName = "SHARECAPITAL")]
        public double? ShareCapital
        {
            get;
            set;
        }

        /// <summary>
        /// 其他权益工具
        /// </summary>
        [JsonProperty(PropertyName = "OTHEREQUITY")]
        public double? OtherEquity
        {
            get;
            set;
        }

        /// <summary>
        /// 资本公积
        /// </summary>
        [JsonProperty(PropertyName = "CAPITALRESERVE")]
        public double? CapitalReserve
        {
            get;
            set;
        }

        /// <summary>
        /// 库存股
        /// </summary>
        [JsonProperty(PropertyName = "INVENTORYSHARE")]
        public double? InventoryShare
        {
            get;
            set;
        }

        /// <summary>
        /// 专项储备
        /// </summary>
        [JsonProperty(PropertyName = "SPECIALRESERVE")]
        public double? SpecialReserve
        {
            get;
            set;
        }

        /// <summary>
        /// 盈余公积
        /// </summary>
        [JsonProperty(PropertyName = "SURPLUSRESERVE")]
        public double? SurpluseReserve
        {
            get;
            set;
        }

        /// <summary>
        /// 一般风险准备
        /// </summary>
        [JsonProperty(PropertyName = "GENERALRISKPREPARE")]
        public double? GeneralRiskPrepare
        {
            get;
            set;
        }

        /// <summary>
        /// 未分配利润
        /// </summary>
        [JsonProperty(PropertyName = "RETAINEDEARNING")]
        public double? RetainEdearing
        {
            get;
            set;
        }

        /// <summary>
        /// 归属于母公司股东权益合计
        /// </summary>
        [JsonProperty(PropertyName = "SUMPARENTEQUITY")]
        public double? SumParentEquity
        {
            get;
            set;
        }

        /// <summary>
        /// 少数股东权益
        /// </summary>
        [JsonProperty(PropertyName = "MINORITYEQUITY")]
        public double? MinorityEquity
        {
            get;
            set;
        }

        /// <summary>
        /// 股东权益合计
        /// </summary>
        [JsonProperty(PropertyName = "SUMSHEQUITY")]
        public double? SumShareEquity
        {
            get;
            set;
        }

        /// <summary>
        /// 负债和股东权益合计
        /// </summary>
        [JsonProperty(PropertyName = "SUMLIABSHEQUITY")]
        public double? SumLiabShareEquity
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
