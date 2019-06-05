using Newtonsoft.Json;

namespace OnlineDataCrawler.Data
{
    public class CashFlow : FinancialReport
    {
        /// <summary>
        /// 销售商品、提供劳务收到的现金
        /// </summary>
        [JsonProperty(PropertyName = "SALEGOODSSERVICEREC")]
        public double? SaleGoodsServiceRec
        {
            get; set;
        }

        /// <summary>
        /// 客户存款和同业存放净增加额
        /// </summary>
        [JsonProperty(PropertyName = "NIDEPOSIT")]
        public double? NiDeposit
        {
            get;
            set;
        }

        /// <summary>
        /// 向央行借款净增加额
        /// </summary>
        [JsonProperty(PropertyName = "NIBORROWFROMCBANK")]
        public double? NetIncomeBorrowFromCBank
        {
            get;
            set;
        }

        /// <summary>
        /// 向其他金融机构拆入资金净增加额
        /// </summary>
        [JsonProperty(PropertyName = "NIBORROWFROMFI")]
        public double? NetIncomeFromFI
        {
            get;
            set;
        }

        /// <summary>
        /// 收到原保险合同报废取得的现金
        /// </summary>
        [JsonProperty(PropertyName = "PREMIUMREC")]
        public double? PremiumRec
        {
            get;
            set;
        }

        /// <summary>
        /// 收到再保险业务现金净额
        /// </summary>
        [JsonProperty(PropertyName = "NETRIREC")]
        public double? NetRIRec
        {
            get;
            set;
        }

        /// <summary>
        /// 保户储金及投资款净增加额
        /// </summary>
        [JsonProperty(PropertyName = "NIINSUREDDEPOSITINV")]
        public double? NetIncomeInsureDepositInv
        {
            get;
            set;
        }

        /// <summary>
        /// 处置交易性金融资产净增加额
        /// </summary>
        [JsonProperty(PropertyName = "NIDISPTRADEFASSET")]
        public double? NetIncomeDispositTradeAsset
        {
            get;
            set;
        }

        /// <summary>
        /// 收取利息、手续费及佣金的现金
        /// </summary>
        [JsonProperty(PropertyName = "INTANDCOMMREC")]
        public double? InterestAndCommisionRec
        {
            get;
            set;
        }

        /// <summary>
        /// 拆入资金净增加额
        /// </summary>
        [JsonProperty(PropertyName = "NIBORROWFUND")]
        public double? NetIncomeBorrowFund
        {
            get;
            set;
        }

        /// <summary>
        /// 回购业务资金净增加额
        /// </summary>
        [JsonProperty(PropertyName = "NIBUYBACKFUND")]
        public double? NetIncomeBuyBackFund
        {
            get;
            set;
        }

        /// <summary>
        /// 收到的税费返还
        /// </summary>
        [JsonProperty(PropertyName = "TAXRETURNREC")]
        public double? TaxReturnRec
        {
            get;
            set;
        }

        /// <summary>
        /// 收到其他与经营活动相关的现金
        /// </summary>
        [JsonProperty(PropertyName = "OTHEROPERATEREC")]
        public double? OtherOperateRec
        {
            get;
            set;
        }

        /// <summary>
        /// 经营活动现金流入小计
        /// </summary>
        [JsonProperty(PropertyName = "SUMOPERATEFLOWIN")]
        public double? SumOperateFlowIn
        {
            get;
            set;
        }

        /// <summary>
        /// 购买商品、接受劳务支付的现金
        /// </summary>
        [JsonProperty(PropertyName = "BUYGOODSSERVICEPAY")]
        public double? BuyGoodsServicePay
        {
            get;
            set;
        }

        /// <summary>
        /// 贷款、垫款净增加额
        /// </summary>
        [JsonProperty(PropertyName = "NILOANADVANCES")]
        public double? NetIncomeLoanAdvances
        {
            get;
            set;
        }

        /// <summary>
        /// 存放中央银行和同业款项净增加额
        /// </summary>
        [JsonProperty(PropertyName = "NIDEPOSITINCBANKFI")]
        public double? NetIncomeDepositInCBankFI
        {
            get;
            set;
        }

        /// <summary>
        /// 支付原保险合同赔付款项的现金
        /// </summary>
        [JsonProperty(PropertyName = "INDEMNITYPAY")]
        public double? IndemnityPay
        {
            get;
            set;
        }

        /// <summary>
        /// 支付利息、手续费及佣金的现金
        /// </summary>
        [JsonProperty(PropertyName = "INTANDCOMMPAY")]
        public double? InterestAndCommisionPay
        {
            get;
            set;
        }

        /// <summary>
        /// 支付保单红利的现金
        /// </summary>
        [JsonProperty(PropertyName = "DIVIPAY")]
        public double? DiviPay
        {
            get;
            set;
        }

        /// <summary>
        /// 支付给员工及为职工支付的现金
        /// </summary>
        [JsonProperty(PropertyName = "EMPLOYEEPAY")]
        public double? EmployeePay
        {
            get;
            set;
        }

        /// <summary>
        /// 支付的各项税费
        /// </summary>
        [JsonProperty(PropertyName = "TAXPAY")]
        public double? TaxPay
        {
            get;
            set;
        }

        /// <summary>
        /// 支付其他与经营活动有关的现金
        /// </summary>
        [JsonProperty(PropertyName = "OTHEROPERATEPAY")]
        public double? OtherOPeratePay
        {
            get;
            set;
        }

        /// <summary>
        /// 经营活动现金流出小计
        /// </summary>
        [JsonProperty(PropertyName = "SUMOPERATEFLOWOUT")]
        public double? SumOperateFlowOut
        {
            get;
            set;
        }

        /// <summary>
        /// 经营活动产生的现金流量净额
        /// </summary>
        [JsonProperty(PropertyName = "NETOPERATECASHFLOW")]
        public double? NetOperateCashFlow
        {
            get;
            set;
        }

        /// <summary>
        /// 收回投资收到的现金
        /// </summary>
        [JsonProperty(PropertyName = "DISPOSALINVREC")]
        public double? DisposalInvestRec
        {
            get;
            set;
        }

        /// <summary>
        /// 取得投资收益收到的现金
        /// </summary>
        [JsonProperty(PropertyName = "INVINCOMEREC")]
        public double? InvestIncomeRec
        {
            get;
            set;
        }

        /// <summary>
        /// 处置固定资产、无形资产和其他长期资产收回的现金净额
        /// </summary>
        [JsonProperty(PropertyName = "DISPFILASSETREC")]
        public double? DispFilAssetRec
        {
            get;
            set;
        }

        /// <summary>
        /// 处置子公司收到的现金净额
        /// </summary>
        [JsonProperty(PropertyName = "DISPSUBSIDIARYREC")]
        public double? DispSubsidiaryRec
        {
            get;
            set;
        }

        /// <summary>
        /// 减少质押和定期存款所收到的现金
        /// </summary>
        [JsonProperty(PropertyName = "REDUCEPLEDGETDEPOSIT")]
        public double? ReducePledgeDeposit
        {
            get;
            set;
        }

        /// <summary>
        /// 收到其他与投资收益相关的现金
        /// </summary>
        [JsonProperty(PropertyName = "OTHERINVREC")]
        public double? OtherInvRec
        {
            get;
            set;
        }

        /// <summary>
        /// 收到其他与投资活动有关的现金
        /// </summary>
        [JsonProperty(PropertyName = "SUMINVFLOWIN")]
        public double? SumInvestFlowIn
        {
            get;
            set;
        }

        /// <summary>
        /// 购建固定资产、无形资产和其他长期资产支付的现金
        /// </summary>
        [JsonProperty(PropertyName = "BUYFILASSETPAY")]
        public double? BuyFILAssetPay
        {
            get;
            set;
        }

        /// <summary>
        /// 投资支付的现金
        /// </summary>
        [JsonProperty(PropertyName = "INVPAY")]
        public double? InvPay
        {
            get;
            set;
        }

        /// <summary>
        /// 质押贷款净增加额
        /// </summary>
        [JsonProperty(PropertyName = "NIPLEDGELOAN")]
        public double? NetIncomeLedgeLoan
        {
            get;
            set;
        }

        /// <summary>
        /// 取得子公司及其他营业单位支付的现金净额
        /// </summary>
        [JsonProperty(PropertyName = "GETSUBSIDIARYPAY")]
        public double? GetSubSidiaryPay
        {
            get;
            set;
        }

        /// <summary>
        /// 增加质押和定期存款所支付的现金
        /// </summary>
        [JsonProperty(PropertyName = "ADDPLEDGETDEPOSIT")]
        public double? AddPledgeDeposit
        {
            get;
            set;
        }

        /// <summary>
        /// 支付其他与投资活动有关的现金
        /// </summary>
        [JsonProperty(PropertyName = "OTHERINVPAY")]
        public double? OtherInvestPay
        {
            get;
            set;
        }

        /// <summary>
        /// 投资活动现金流出小计
        /// </summary>
        [JsonProperty(PropertyName = "SUMINVFLOWOUT")]
        public double? SumInvestFlowOut
        {
            get;
            set;
        }

        /// <summary>
        /// 投资活动产生的现金流量净额
        /// </summary>
        [JsonProperty(PropertyName = "NETINVCASHFLOW")]
        public double? NetInvestCashFlow
        {
            get;
            set;
        }

        /// <summary>
        /// 吸收投资收到的现金
        /// </summary>
        [JsonProperty(PropertyName = "ACCEPTINVREC")]
        public double? AcceptInvestRec
        {
            get;
            set;
        }

        /// <summary>
        /// 子公司吸收少数股东投资收到的现金
        /// </summary>
        [JsonProperty(PropertyName = "SUBSIDIARYACCEPT")]
        public double? SubsidiaryAccept
        {
            get;
            set;
        }

        /// <summary>
        /// 取得借款收到的现金
        /// </summary>
        [JsonProperty(PropertyName = "LOANREC")]
        public double? LoanRec
        {
            get;
            set;
        }

        /// <summary>
        /// 发行债券收到的现金
        /// </summary>
        [JsonProperty(PropertyName = "ISSUEBONDREC")]
        public double? IssueBondRec
        {
            get;
            set;
        }

        /// <summary>
        /// 收到其他与筹资活动有关的现金
        /// </summary>
        [JsonProperty(PropertyName = "OTHERFINAREC")]
        public double? OtherFinaRec
        {
            get;
            set;
        }

        /// <summary>
        /// 筹资活动现金流入小计
        /// </summary>
        [JsonProperty(PropertyName = "SUMFINAFLOWIN")]
        public double? SumFinaFlowIn
        {
            get;
            set;
        }

        /// <summary>
        /// 偿还债务支付的现金
        /// </summary>
        [JsonProperty(PropertyName = "REPAYDEBTPAY")]
        public double? RepayDeptPay
        {
            get;
            set;
        }

        /// <summary>
        /// 分配股利、利润或偿付利息支付的现金
        /// </summary>
        [JsonProperty(PropertyName = "DIVIPROFITORINTPAY")]
        public double? DiviProfitOrintPay
        {
            get;
            set;
        }

        /// <summary>
        /// 子公司支付给少数股东的股利、利润
        /// </summary>
        [JsonProperty(PropertyName = "SUBSIDIARYPAY")]
        public double? SubsidiaryPay
        {
            get;
            set;
        }

        /// <summary>
        /// 购买子公司少数股权而支付的现金
        /// </summary>
        [JsonProperty(PropertyName = "BUYSUBSIDIARYPAY")]
        public double? BuySunsidiaryPay
        {
            get;
            set;
        }

        /// <summary>
        /// 支付其他与筹资活动有关的现金
        /// </summary>
        [JsonProperty(PropertyName = "OTHERFINAPAY")]
        public double? OtherFinaPay
        {
            get;
            set;
        }

        /// <summary>
        /// 子公司减资支付给少数股东的现金
        /// </summary>
        [JsonProperty(PropertyName = "SUBSIDIARYREDUCTCAPITAL")]
        public double? SubsidiaryReduceCapital
        {
            get;
            set;
        }

        /// <summary>
        /// 筹资活动现金流出小计
        /// </summary>
        [JsonProperty(PropertyName = "SUMFINAFLOWOUT")]
        public double? SumFinaFlowOut
        {
            get;
            set;
        }

        /// <summary>
        /// 筹资活动产生的现金流量净额
        /// </summary>
        [JsonProperty(PropertyName = "NETFINACASHFLOW")]
        public double? NetFinaCashFlow
        {
            get;
            set;
        }

        /// <summary>
        /// 汇率变动对现金及现金等价物的影响
        /// </summary>
        [JsonProperty(PropertyName = "EFFECTEXCHANGERATE")]
        public double? EffectExchangeRate
        {
            get;
            set;
        }

        /// <summary>
        /// 现金及现金等价物净增加额
        /// </summary>
        [JsonProperty(PropertyName = "NICASHEQUI")]
        public double? NetIncomeCashFlow
        {
            get;
            set;
        }

        /// <summary>
        /// 现金及现金等价物净增加额
        /// </summary>
        [JsonProperty(PropertyName = "CASHEQUIBEGINNING")]
        public double? CashBegining
        {
            get;
            set;
        }

        /// <summary>
        /// 期末现金及现金等价物余额
        /// </summary>
        [JsonProperty(PropertyName = "CASHEQUIENDING")]
        public double? CashEnding
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
