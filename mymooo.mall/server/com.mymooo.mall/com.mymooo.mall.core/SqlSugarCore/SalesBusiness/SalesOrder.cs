using SqlSugar;
namespace com.mymooo.mall.core.SqlSugarCore.SalesBusiness
{
	///<summary>
	///
	///</summary>
	[SugarTable("F_CUST_BOOK_MSTR")]
    public partial class SalesOrder
    {
        /// <summary>
        /// 
        /// </summary>
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public SalesOrder()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
		{

		}

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "FBM_BOOK_ID")]
        public long SalesOrderId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_CUST_ID")]
        public long CustId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_USER_ID")]
        public long CustUserId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_FACTORY_ID")]
        public long FactoryId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_SUBMIT_DATE")]
        public DateTime? SubmitDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_MODIFY_DATE")]
        public DateTime? ModifyDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_CREATE_DATE")]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_DELIVERY_ADDR_ID")]
        public long DeliveryAddressId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_DELIVERY_ADDR")]
        public string DeliveryAddress { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_DELIVERY_NAME")]
        public string DeliveryName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_BOOK_STATE")]
        public long SalesOrderStatus { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_BOOK_IND")]
        public long SalesOrderInd { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_BOOK_ITEMS")]
        public long SalesOrderItems { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_BOOK_TOTAL")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_BOOK_TAX")]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Desc:运费
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_FREIGHT_FEE")]
        public decimal FreightAmount { get; set; }

        /// <summary>
        /// Desc:运费折扣
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_FREIGHT_DISCOUNT")]
        public decimal FreightDiscount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_BLL_BOOK_TOTAL")]
        public decimal TaxTotalAmount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_SHIPPER_NAME")]
        public string ShipperName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_CASH_STYLE")]
        public string SettlementType { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_TRADER_PROVISION")]
        public string TraderProvision { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_SHIPPER_WAY")]
        public string ShipperWay { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_PAY_ID")]
        public long PaymentId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_PAY_NAME")]
        public string FBM_PAY_NAME { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_DELIVERY_DATE")]
        public DateTime DeliveryDate { get; set; }

        /// <summary>
        /// Desc:传送状态
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_TRANS_IND")]
        public long TransStatus { get; set; }

        /// <summary>
        /// Desc:销售单备注
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_BOOK_MEMO")]
        public string SalesOrderMemo { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_CUST_DEF_CODE")]
        public string FBM_CUST_DEF_CODE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_OFFER_CODE")]
        public string FBM_OFFER_CODE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_BOOK_CODE")]
        public string SalesBillNo { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_BOOK_RATE")]
        public decimal TaxRate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_REASON")]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Desc:1、2、3   数字递增
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_VERSION")]
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Desc:变更人
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_CHANGERID")]
        public long ChangeId { get; set; }

        /// <summary>
        /// Desc:变更原因
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_CHANGE_REASON")]
        public string ChangeReason { get; set; } = string.Empty;

        /// <summary>
        /// Desc:变更前老记录上记录变更后新记录的单据ID
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_REFBILL_ID")]
        public long ReferenceBillId { get; set; }

        /// <summary>
        /// Desc:变更日期
        /// Default:1900-01-01
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_CHANGE_DATE")]
        public DateTime? ChangeDate { get; set; }

        /// <summary>
        /// Desc:订单取消类型
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_REASON_TYPE")]
        public string ReasonType { get; set; } = string.Empty;

        /// <summary>
        /// Desc:收货人电话
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_DELIVERY_TEL")]
        public string DeliveryTel { get; set; } = string.Empty;

        /// <summary>
        /// Desc:收货人手机
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_DELIVERY_MOBILE")]
        public string DeliveryMobile { get; set; } = string.Empty;

        /// <summary>
        /// Desc:询价单内码
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_INQ_ID")]
        public long InquiryId { get; set; }

        /// <summary>
        /// Desc:联系人电话
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_PAY_TEL")]
        public string PaymentTel { get; set; } = string.Empty;

        /// <summary>
        /// Desc:联系人手机
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_PAY_MOBILE")]
        public string PaymentMobile { get; set; } = string.Empty;

        /// <summary>
        /// Desc:发票类型 2001无需发票  2002普通发票  2003增值税发票
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_INVOICE_TYPE")]
        public string InvoiceType { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FDeptName")]
        public string DepartmentName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FCompanyName")]
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FPurchaserDepartment")]
        public string PurchaserDepartment { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FPurchaserCompany")]
        public string PurchaserCompany { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FExpiryDate")]
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:True
        /// </summary>           
        public decimal? Discount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "KnockOff")]
        public decimal KnockOff { get; set; }

        /// <summary>
        /// Desc:运费是否到付
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FreightToBeCollected")]
        public bool FreightToBeCollected { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Coupons { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public long SalesmanId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:newid()
        /// Nullable:False
        /// </summary>           
        public Guid DepartmentId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public long UpdatedBy { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? PaidOn { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ActualPaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public long? InputterId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public byte PayStatus { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public byte DataSources { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public byte InvoiceStatus { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public byte ReceivablesStatus { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? ReceivablesPrice { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? InvoicePrice { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public byte PayType { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public byte AuditStatus { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string AuditPeopleName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? AuditTime { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public decimal AdvanceAmount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string InvoiceReceiver { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ContactOfInvoiceReceiver { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string AddressOfReceiveInvoice { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CancelEnclosure { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CancelRejectReason { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public byte CancelStatus { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public long? CancelType { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CancelReason { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CancelSubmitReason { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ReceivableCode { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? CancelServiceCharge { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public long? CancelWhereabouts { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string AttachmentUrl { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public decimal PayHandFee { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string PayInfo { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public bool IsDeliveredDirectly { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public Guid? PurchasedFrom { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public byte OrderType { get; set; }

        /// <summary>
        /// Desc:
        /// Default:-1
        /// Nullable:False
        /// </summary>           
        public int ExpectedDeliveryDays { get; set; }

        /// <summary>
        /// Desc:
        /// Default:-1
        /// Nullable:False
        /// </summary>           
        public decimal ExpectedTotalWithTax { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? IsCheckCredit { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string MessageHelpForCredit { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:-1
        /// Nullable:False
        /// </summary>           
        public int SelectPaySort { get; set; }

        /// <summary>
        /// Desc:
        /// Default:SZMYGC
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_SALES_DEF_CODE")]
        public string SalesCompanyCode { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:深圳蚂蚁工场科技有限公司
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBM_SALES_DEF_NAME")]
		public string SalesCompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public bool SpecialPrice { get; set; }

    }
}