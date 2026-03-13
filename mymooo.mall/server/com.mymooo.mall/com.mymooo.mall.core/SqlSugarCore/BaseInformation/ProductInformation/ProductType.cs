using SqlSugar;
using System.Text.Json.Serialization;
namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation
{
	///<summary>
	///F_PRD_TYPE(平台类型总表)
	///</summary>
	[SugarTable("F_PRD_TYPE")]
    public partial class ProductType
    {
        /// <summary>
        /// 
        /// </summary>
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public ProductType()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
		{

        }

        /// <summary>
        /// Desc:类型总表ID
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "FPT_ID")]
        [JsonIgnore]
        public long Id { get; set; }

        /// <summary>
        /// Desc:厂商ID
        /// Default:-1
        /// Nullable:False
        /// </summary>           
        [JsonIgnore]
        [SugarColumn(ColumnName = "FPT_FACTORY_ID")]
        public long FactoryId  { get; set; }

        /// <summary>
        /// Desc:品牌ID
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [JsonIgnore]
        [SugarColumn(ColumnName = "FPT_BRAND_ID")]
        public long BrandId { get; set; }

        /// <summary>
        /// Desc:产品ID
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [JsonIgnore]
        [SugarColumn(ColumnName = "FPT_PRD_ID")]
        public long ProductId { get; set; }

        /// <summary>
        /// Desc:类型ID
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPT_TYPE_ID")]
        public long ProductTypeId { get; set; }

        /// <summary>
        /// Desc:类型编码
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPT_TYPE_CODE")]
        public string TypeCode { get; set; } = string.Empty;

        /// <summary>
        /// Desc:参考类型编码
        /// Default:
        /// Nullable:False
        /// </summary>           
        [JsonIgnore]
        [SugarColumn(ColumnName = "FPT_REF_TYPE_CODE")]
        public string RefTypeCode { get; set; } = string.Empty;

    }
}