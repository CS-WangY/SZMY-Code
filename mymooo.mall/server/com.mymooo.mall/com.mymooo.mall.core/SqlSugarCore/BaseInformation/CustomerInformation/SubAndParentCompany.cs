using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation.CustomerInformation
{
	///<summary>
	///
	///</summary>
	public partial class SubAndParentCompany
    {
        /// <summary>
        /// 
        /// </summary>
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public SubAndParentCompany()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
		{

        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public Guid? CompanyId { get; set; }

        /// <summary>
        /// 公司编码
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        [Required(ErrorMessage = "客户编码必录")]
        public string CompanyCode { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public Guid? ParentCompanyId { get; set; }

        /// <summary>
        /// 子公司编码
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        [Required(ErrorMessage = "母客户编码必录")]
        public string ParentCompanyCode { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public bool IsValid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [Required(ErrorMessage = "审核人必录")]
        public string AuditBy { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [Required(ErrorMessage = "创建人必录")]
        public string CreateBy { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? CreateDateTime { get; set; }

        /// <summary>
        /// Desc:
        /// Default:1
        /// Nullable:False
        /// </summary>           
        public bool UnifiedCreditLine { get; set; }

    }
}