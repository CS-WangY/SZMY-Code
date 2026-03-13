using System;
using System.Linq;
using System.Text;

using SqlSugar;
namespace com.mymooo.mall.core.SqlSugarCore.SalesBusiness
{
    ///<summary>
    ///
    ///</summary>
	[SugarTable("F_CUST_BOOK_DETAIL")]
    public partial class SalesOrderDetail
    {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public SalesOrderDetail()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
		{

		}

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "FBD_DETAIL_ID")]
        public long SalesOrderDetailId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBD_BOOK_MSTR_ID")]
        public long SalesOrderId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBD_FACTORY_ID")]
        public long FactoryId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBD_PRD_CODE")]
        public string ProductNumber { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBD_PRD_NAME")]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FBD_PUR_PRICE")]
        public decimal? FBD_PUR_PRICE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBD_UNIT_PRICE")]
        public decimal Price { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBD_UNIT_TAX_PRICE")]
        public decimal TaxPrice { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBD_SUBTOTAL")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBD_TAX_SUBTOTAL")]
        public decimal TaxTotalAmount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBD_DELIVE_DATE")]
        public DateTime DeliveDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBD_CUST_ITEM")]
        public string CustProductNumber { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBD_MEMO")]
        public string Memo { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FBD_ORG_PRICE")]
        public decimal? FBD_ORG_PRICE { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBD_NUM")]
        public decimal Qty { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBD_PRD_ID")]
        public long ProductId { get; set; }

        /// <summary>
        /// Desc:产品品牌ID
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FBD_BRAND_ID")]
        public long BrandId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int DispatchDays { get; set; }

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
        public bool IsGift { get; set; }

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
        public bool IsDelete { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? ChangeDeliVeDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:newid()
        /// Nullable:True
        /// </summary>           
        public string DetailUid { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? ProductEngineerId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ProductEngineerName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ProductManagerName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? ProductManagerId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public long? InquiryItemId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public byte CategoryType { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public byte ProductType { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public Guid? SupplierId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Materials { get; set; } = string.Empty;

        /// <summary>
        /// Desc:客户产品名称
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "CustItemName")]
        public string CustProductName { get; set; } = string.Empty;

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
        public string LocFactory { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:1
        /// Nullable:False
        /// </summary>           
        public bool NotReplace { get; set; }

        /// <summary>
        /// Desc:不知道是什么,先留着
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
        public string InsideRemark { get; set; } = string.Empty;

        /// <summary>
        /// Desc:事业部Id
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string BusinessDivisionId { get; set; } = string.Empty;

        /// <summary>
        /// Desc:事业部名称
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string BusinessDivisionName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:供货组织Id
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public long SupplyOrgId { get; set; }

        /// <summary>
        /// Desc:供货组织名称
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string SupplyOrgName { get; set; } = string.Empty;

    }
}