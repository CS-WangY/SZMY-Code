using System;
using System.Linq;
using System.Text;

using SqlSugar;
namespace com.mymooo.mall.core.SqlSugarCore.SalesBusiness
{
	///<summary>
	///
	///</summary>
	[SugarTable("F_CUST_INQUIRY_DETAIL")]
	public partial class InquiryOrderDetail
	{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
		public InquiryOrderDetail()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
		{

		}

		/// <summary>
		/// Desc:询价单明细KEY
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "FID_DETAIL_ID")]
		public long InquiryDetailId { get; set; }

		/// <summary>
		/// Desc:询价单KEY
		/// Default:0
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FID_INQ_MSTR_ID")]
		public long InquiryId { get; set; }

		/// <summary>
		/// Desc:产品KEY
		/// Default:0
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FID_PRD_ID")]
		public long ProductId { get; set; }

		/// <summary>
		/// Desc:产品型号
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FID_PRD_CODE")]
		public string ProductNumber { get; set; } = string.Empty;

		/// <summary>
		/// Desc:产品名称
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FID_PRD_NAME")]
		public string ProductName { get; set; } = string.Empty;

		/// <summary>
		/// Desc:数量
		/// Default:0
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FID_NUM")]
		public decimal Qty { get; set; }

		/// <summary>
		/// Desc:0正常	   -1 型号错误
		/// Default:0
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FID_ERROR_STATE")]
		public int ErrorStatus { get; set; }

		/// <summary>
		/// Desc:0正常	   -1 型号错误
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FID_ERROR_DESC")]
		public string ErrorDescription { get; set; } = string.Empty;

		/// <summary>
		/// Desc:给定的智能提示
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FID_ERROR_TIP")]
		public string ErrorTooltip { get; set; } = string.Empty;

		/// <summary>
		/// Desc:客户物料号
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FID_CUST_ITEM")]
		public string CustomerProductNumber { get; set; } = string.Empty;

		/// <summary>
		/// Desc:备注
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FID_REMARK")]
		public string Remark { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:0
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FID_UnitPrice")]
		public decimal UnitPrice { get; set; }

		/// <summary>
		/// Desc:
		/// Default:0
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FStatus")]
		public long Status { get; set; }

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:True
		/// </summary>           
		public Guid? BrandId { get; set; }

		/// <summary>
		/// Desc:
		/// Default:0
		/// Nullable:True
		/// </summary>           
		public long? ParentId { get; set; }
		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:True
		/// </summary>           
		public long ProductEngineerId { get; set; }

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
		public long ProductManagerId { get; set; }

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
		public string Materials { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:0
		/// Nullable:False
		/// </summary>           
		public long ProductTypeId { get; set; }

		/// <summary>
		/// Desc:
		/// Default:0
		/// Nullable:False
		/// </summary>           
		public bool IsHistory { get; set; }

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:True
		/// </summary>           
		[SugarColumn(ColumnName = "CustItemName")]
		public string CustomerProductName { get; set; } = string.Empty;

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
		[SugarColumn(ColumnName = "CustItemNo")]
		public string CustItemNo { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
		public string BusinessDivisionId { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
		public string BusinessDivisionName { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:0
		/// Nullable:False
		/// </summary>           
		public long SupplyOrgId { get; set; }

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
		public string SupplyOrgName { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:0
		/// Nullable:False
		/// </summary>           
		public bool IsInternal { get; set; }

		/// <summary>
		/// Desc:
		/// Default:0
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FIM_SEQ")]
		public int Seq { get; set; }


		/// <summary>
		/// Desc:库位工厂
		/// Default:
		/// Nullable:True
		/// </summary>     
		public string Storage { get; set; } = string.Empty;

		/// <summary>
        /// Desc:简易型号
        /// Default:
        /// Nullable:True
        /// </summary>  
        public string ShortNumber { get; set; } = string.Empty;

        /// <summary>
        /// Desc:是否项目特价
        /// Default:
        /// Nullable:True
        /// </summary>  
        public bool IsSpecial { get; set; }
    }
 }