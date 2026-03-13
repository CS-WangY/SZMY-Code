using System;
using System.Linq;
using System.Text;

using SqlSugar;
namespace com.mymooo.mall.core.SqlSugarCore.SalesBusiness
{
    ///<summary>
    ///
    ///</summary>
	[SugarTable("F_CUST_QUOTATION_MSTR")]
    public partial class InquiryQuotationOrder
    {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public InquiryQuotationOrder()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
		{

		}

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "FQM_QUO_ID")]
        public long QuotationId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_QUO_CODE", InsertSql = "NEXT VALUE FOR billseq")]
        public string QuotationNumber { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_USER_ID", InsertSql = "0")]
        public long UserId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_CUST_ID", InsertSql = "0")]
        public long CustomerId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_FACTORY_ID", InsertSql = "0")]
        public long FactoryId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_SUBMIT_DATE", InsertSql = "GETDATE()")]
        public DateTime? SubmitDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_MODIFY_DATE", InsertSql = "GETDATE()")]
        public DateTime? ModifyDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_CREATE_DATE", InsertSql = "GETDATE()")]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_GENERATE_DATE", InsertSql = "GETDATE()")]
        public DateTime? GenerateDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_QUO_ITEMS")]
        public long QuotationItemCount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_SHIPPER_NAME", InsertSql = "")]
        public long ShipperName { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_CASH_STYLE", InsertSql = "")]
        public string SettlementId { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_TRADER_PROVISION", InsertSql = "")]
        public string TraderProvision { get; set; } = string.Empty;

        /// <summary>
        /// Desc:发货方式	   0 - 最早发货	   1 - 最晚发货	   2 - 指定发货
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_SHIPPER_WAY", InsertSql = "")]
        public string ShipperWay { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_QUO_TOTAL")]
        public decimal QuotationTotalAmount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_QUO_TAX")]
        public decimal QuotationTaxAmount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_FREIGHT_FEE")]
        public decimal FQM_FREIGHT_FEE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_FREIGHT_DISCOUNT")]
        public decimal FQM_FREIGHT_DISCOUNT { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_BILL_QUO_TOTAL")]
        public decimal QuotationTotalTaxAmount { get; set; }

        /// <summary>
        /// Desc:-1,订单有错误条目	   0,未提交	   	   	   2.已批核	   3.等待付款	   4.等待发货	   5.订单关闭
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_QUO_STATE")]
        public long QuotationStatus { get; set; }

        /// <summary>
        /// Desc:0,未批核	   1.已批核
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_QUO_IND")]
        public long AuditStatus { get; set; }

        /// <summary>
        /// Desc:传送状态	   0-未传送	   1-已传送
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_TRANS_IND")]
        public long TransStatus { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_QUO_MEMO", InsertSql = "")]
        public string QuotationMemo { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_CUST_DEF_CODE", InsertSql = "")]
        public string FQM_CUST_DEF_CODE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:17
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_QUO_RATE")]
        public decimal TaxRate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_QUO_RATECODE")]
        public string TaxRateCode { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_INVOICE_TYPE")]
        public string InvoiceType { get; set; } = string.Empty;

        /// <summary>
        /// Desc:客服是否删除 0 否 1 是
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_IS_DELETE")]
        public bool IsDelete { get; set; }

        /// <summary>
        /// Desc:询价单内码
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_INQ_ID")]
        public long InquiryId { get; set; }

        /// <summary>
        /// Desc:是否失效 0 否 1 是
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_IS_FAILURE")]
        public bool IsFailure { get; set; }

        /// <summary>
        /// Desc:是否已读 0 否 1 已读
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQM_IS_READ")]
        public bool IsRead { get; set; }

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
        public decimal KnockOff { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public bool FreightToBeCollected { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string FileName { get; set; } = string.Empty;

    }
}