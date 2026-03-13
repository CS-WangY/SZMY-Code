using System;
using System.Linq;
using System.Text;

using SqlSugar;
using static System.Net.Mime.MediaTypeNames;
namespace com.mymooo.mall.core.SqlSugarCore.SalesBusiness
{
    ///<summary>
    ///
    ///</summary>
	[SugarTable("F_CUST_INQUIRY_MSTR")]
    public partial class InquiryOrder
	{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public InquiryOrder()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
		{

		}

        /// <summary>
        /// Desc:主键
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "FIM_INQ_ID")]
        public long InquiryId { get; set; }

        /// <summary>
        /// Desc:流水Key(序列)
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_INQ_CODE")]
        public string InquiryNumber { get; set; } = string.Empty;

        /// <summary>
        /// Desc:用户KEY
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_USER_ID")]
        public long UserId { get; set; }

        /// <summary>
        /// Desc:客户KEY
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_CUST_ID")]
        public long CustomerId { get; set; }

        /// <summary>
        /// Desc:品牌ID
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_BRAND_ID")]
        public long BrandId { get; set; }

        /// <summary>
        /// Desc:提交日期(创建日期)
        /// Default:1900-01-01
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_SUBMIT_DATE",InsertSql = "GETDATE()")]
        public DateTime? SubmitDate { get; set; }

        /// <summary>
        /// Desc:报价日期
        /// Default:1900-01-01
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_CREATE_DATE", InsertSql = "GETDATE()")]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Desc:生成订单日期
        /// Default:1900-01-01
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_GENERATE_DATE", InsertSql = "GETDATE()")]
        public DateTime? GenerateDate { get; set; }

        /// <summary>
        /// Desc:截止报价时间
        /// Default:1900-01-01
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_STOP_DATE")]
        public DateTime? Deadline { get; set; }

        /// <summary>
        /// Desc:询价单条目数
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_INQ_ITEMS")]
        public long InquiryItemCount { get; set; }

        /// <summary>
        /// Desc:付款方式
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_CASH_STYLE")]
        public string SettlementId { get; set; } = string.Empty;

        /// <summary>
        /// Desc:发货方式	   0 - 最早发货	   1 - 最晚发货	   2 - 指定发货
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_SHIPPER_WAY")]
        public string ShipperWay { get; set; } = string.Empty;

        /// <summary>
        /// Desc:收货人ID
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_DELIVERY_ID")]
        public long DeliveryId { get; set; }

        /// <summary>
        /// Desc:收货地址
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_DELIVERY_ADDR")]
        public string DeliveryAddress { get; set; } = string.Empty;

        /// <summary>
        /// Desc:收货人
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_DELIVERY_NAME")]
        public string DeliveryName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:收货人电话
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_DELIVERY_TEL")]
        public string DeliveryTel { get; set; } = string.Empty;

        /// <summary>
        /// Desc:收货人手机
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_DELIVERY_MOBILE")]
        public string DeliveryMobile { get; set; } = string.Empty;

        /// <summary>
        /// Desc:联系人
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_PAY_NAME")]
        public string PayName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:联系人ID
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_PAY_ID")]
        public long PayId { get; set; }

        /// <summary>
        /// Desc:联系人电话
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_PAY_TEL")]
        public string PayTel { get; set; } = string.Empty;

        /// <summary>
        /// Desc:联系人手机
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_PAY_MOBILE")]
        public string PayMobile { get; set; } = string.Empty;

        /// <summary>
        /// Desc:0待报价	   1已报价	   3订单已提交	   4失效
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_INQ_STATE")]
        public long InquiryStatus { get; set; }

        /// <summary>
        /// Desc:客户采购单号
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_CUST_DEF_CODE")]
        public string CustomerPurchaseNumber { get; set; } = string.Empty;

        /// <summary>
        /// Desc:订单备注
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_REMARK")]
        public string Remark { get; set; } = string.Empty;

        /// <summary>
        /// Desc:审核状态 0 未审核 1 审核
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_INQ_IND")]
        public long AuditStatus { get; set; }

        /// <summary>
        /// Desc:发票类型 2001无需发票  2002普通发票
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIN_INVOICE_TYPE")]
        public string InvoiceType { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FReceiverCompany")]
        public string ReceiverCompany { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FReceiverDepartment")]
        public string ReceiverDepartment { get; set; } = string.Empty;

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
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FPurchaserDepartment")]
        public string PurchaserDepartment { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FDeleted")]
        public bool Deleted { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "CompanyId")]
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FPurchaserAddress")]
        public string PurchaserAddress { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public long SalesmanId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public bool Agent { get; set; }

        /// <summary>
        /// Desc:
        /// Default:newid()
        /// Nullable:False
        /// </summary>           
        public Guid DepartmentId { get; set; }

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
        public bool DeletedByCustomer { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public long? DeletedBy { get; set; }

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
        /// Default:SZMYGC
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_SALES_DEF_CODE")]
        public string FIM_SALES_DEF_CODE { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:深圳蚂蚁工场科技有限公司
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIM_SALES_DEF_NAME")]
        public string FIM_SALES_DEF_NAME { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "Inq_PaymentMethod_Id")]
        public Guid? PaymentMethodId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public bool SpecialPrice { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public bool IsInternal { get; set; }

        /// <summary>
        /// Desc:是否非标单
        /// Default:0
        /// Nullable:False
        /// </summary>         
        public bool IsNonStandard { get; set; }

        /// <summary>
        /// Desc:上传附件的路径
        /// Default:0
        /// Nullable:False
        /// </summary>  

        public string UploadPath { get; set; } = "";
 /// <summary>
        /// 报价单版本号
        /// </summary>
        public string SpecialVersion { get; set; }="";
    }
}