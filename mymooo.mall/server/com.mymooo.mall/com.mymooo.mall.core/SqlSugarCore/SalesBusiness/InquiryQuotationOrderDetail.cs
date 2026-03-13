using System;
using System.Linq;
using System.Text;

using SqlSugar;
namespace com.mymooo.mall.core.SqlSugarCore.SalesBusiness
{
    ///<summary>
    ///
    ///</summary>
	[SugarTable("F_CUST_QUOTATION_DETAIL")]
    public partial class InquiryQuotationOrderDetail
	{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public InquiryQuotationOrderDetail()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
		{

		}

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "FQD_DETAIL_ID")]
        public long QuotationDetailId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQD_QUO_MSTR_ID")]
        public long QuotationId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQD_PRD_ID" ,InsertSql = "0")]
		[Obsolete("产品型号存在InquiryOrder表中")]
        public long ProductId { get; set; }

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
        [SugarColumn(ColumnName = "FQD_FACTORY_ID", InsertSql = "0")]
		public long FactoryId { get; set; }

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
        [SugarColumn(ColumnName = "FQD_PRD_CODE", InsertSql = "")]
		[Obsolete("产品型号存在InquiryOrder表中")]
		public string ProductNumber { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQD_PRD_NAME", InsertSql = "")]
		[Obsolete("产品型号存在InquiryOrder表中")]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQD_PUR_PRICE", InsertSql = "")]
        public decimal FQD_PUR_PRICE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQD_UNIT_PRICE")]
        public decimal Price { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQD_UNIT_TAX_PRICE")]
        public decimal TaxPrice { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQD_DELIVE_DAYS")]
        public decimal DeliveDays { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQD_CUST_ITEM", InsertSql = "")]
        public string CustomerProductNumber { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQD_ORG_PRICE", InsertSql = "0")]
        public decimal OriginalPrice { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQD_MEMO")]
        public string Memo { get; set; } = string.Empty;

        /// <summary>
        /// Desc:产品品牌ID
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQD_BRAND_ID")]
        public long BrandId { get; set; }

        /// <summary>
        /// Desc:明细报价状态 1 已报价、0 未报价、-1 无法报价
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQD_QUO_STATE")]
        public int QuotationStatus { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FQD_INQ_DETAIL_ID")]
        public long InquiryDetailId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:DateTime.Now
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "UpdatedOn", InsertSql = "GETDATE()")]
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public bool IsGift { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public byte ResolveStatus { get; set; }

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
        public decimal FirstCost { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public byte AuditStatus { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public long AuditedBy { get; set; }

        /// <summary>
        /// Desc:
        /// Default:DateTime.Now
        /// Nullable:False
        /// </summary>           
        [SugarColumn(InsertSql = "GETDATE()")]
        public DateTime AuditedOn { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public long SubmittedBy { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string InsideRemark { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:DateTime.Now
        /// Nullable:False
        /// </summary>           
        [SugarColumn(InsertSql = "GETDATE()")]
        public DateTime SubmittedOn { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public Guid? ResolveDepartmentId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public bool AskPrice { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? SupplierDispatchDays { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public decimal OriginalUnitPriceWithTax { get; set; }

        /// <summary>
        /// Desc:
        /// Default:1
        /// Nullable:False
        /// </summary>           
        public int Version { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public decimal Quantity { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:True
        /// </summary>           
        public long? ParentId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public byte CategoryType { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public Guid? SupplierId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public decimal QtyDiscount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public decimal LevelDiscount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public long ProductTypeId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? SubtotalWithTax { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? SubtotalWithoutTax { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int RejectType { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? BeforeWholeDiscountUnitPriceWithTax { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ProjectNo { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CustItemNo { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string StockFeatures { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public long? SmallClassId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public long? LargeClassId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? PriceSource { get; set; }

        /// <summary>
        /// Desc:
        /// Default:NULL
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FQD_QUO_LPRICE")]
        public decimal? FQD_QUO_LPRICE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:NULL
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FQD_DEAL_LPRICE")]
        public decimal? FQD_DEAL_LPRICE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:NULL
        /// Nullable:True
        /// </summary>           
        public string ResolvedSubmitUserName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string PurchaseRemark { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? DeliverySource { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string DeliverySubmitBy { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? RequirementDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? RequirementDays { get; set; }


        public decimal? DesirePrice { get; set; }

        public int? DesireDeliveryDays { get; set; }

        public decimal? AutoUnitPrice { get; set; }

        public decimal? AutoDeliveryDays { get; set; }

        public int? AutoPriceSource { get; set; }

        public int? AutoDeliverySource { get; set; }

        public string PdmFilesName { get; set; } = "";

    }
}