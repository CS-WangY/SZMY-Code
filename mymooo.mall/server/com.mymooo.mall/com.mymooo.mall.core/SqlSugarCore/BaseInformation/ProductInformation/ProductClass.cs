using System;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using SqlSugar;
using SqlSugar.DbConvert;
namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation
{
	///<summary>
	///平台商品分类总表
	///</summary>
	[SugarTable("F_PRD_CLASS")]
	public partial class ProductClass
	{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
		public ProductClass()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
		{

		}

		/// <summary>
		/// Desc:分类KEY
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "FPC_CLASS_ID")]
		public long ClassId { get; set; }

		/// <summary>
		/// Desc:父分类KEY
		/// Default:-1
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_PARENT_CLASS_ID")]
		public long ParentClassId { get; set; }

		/// <summary>
		/// Desc:分类名称
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_CLASS_NAME")]
		public string ClassName { get; set; } = string.Empty;

		/// <summary>
		/// Desc:分类节点排序
		/// Default:-1
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_SEQUENCE")]
		public int Seq { get; set; }

		/// <summary>
		/// Desc:分库节点标记	   S:子分类分库, 无联接串, 不允许再分库	   C:本分类分库, 有联接串, 不允许再分库	   P:父分类分库, 有联接串, 不允许再分库	   N:无分库分类, 无联接串, 可允许再分库
		/// Default:0
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_SPLIT_IND", SqlParameterDbType = typeof(EnumToStringConvert))]
		[JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonIgnore]
		public DataBaseSplitType SplitType { get; set; }

		/// <summary>
		/// Desc:商品分类联接串	   (仅本分类分库节点可以修改,修改后应用到所有子分类中)
		/// Default:
		/// Nullable:True
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_CONNECTION")]
        [JsonIgnore]
		public string Connection { get; set; } = string.Empty;

		/// <summary>
		/// Desc:分类节点图片路径
		/// Default:
		/// Nullable:True
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_PIC_URL")]
		public string ImageUrl { get; set; } = string.Empty;

		/// <summary>
		/// Desc:分类是否显示首页	   1:是	   0:否
		/// Default:0
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_HOMEPAGE_IND")]
		public bool HomePageShow { get; set; }

		/// <summary>
		/// Desc:分类首页图片路径
		/// Default:
		/// Nullable:True
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_HOMEPAGE_URL")]
		public string HomePageImageUrl { get; set; } = string.Empty;

		/// <summary>
		/// Desc:叶子分类标记	   1:是	   0:否
		/// Default:0
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_LEAF_IND")]
		public bool Leaf { get; set; }

		/// <summary>
		/// Desc:分类发布标记	   1:已经发布	   0:没有发布
		/// Default:0
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_RELEASE_IND")]
		public bool IsRelease { get; set; }

		/// <summary>
		/// Desc:分类创建时间
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_CREATE_DATE")]
        [JsonIgnore]
		public DateTime CreateDate { get; set; }

		/// <summary>
		/// Desc:分类创建用户名称
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_CREATE_USER")]
        [JsonIgnore]
		public string CreateUserName { get; set; } = string.Empty;

		/// <summary>
		/// Desc:分类创建用户KEY
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_CREATE_USER_ID")]
        [JsonIgnore]
		public long CreateUserId { get; set; }

		/// <summary>
		/// Desc:分类修改时间
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_UPDATE_DATE")]
        [JsonIgnore]
		public DateTime? UpdateDate { get; set; }

		/// <summary>
		/// Desc:分类修改用户名称
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_UPDATE_USER")]
        [JsonIgnore]
		public string UpdateUserName { get; set; } = string.Empty;

		/// <summary>
		/// Desc:分类修改用户KEY
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_UPDATE_USER_ID")]
        [JsonIgnore]
		public long UpdateUserId { get; set; }

		/// <summary>
		/// Desc:是否有过滤条件 0 无 1 有
		/// Default:0
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_ISTRUE")]
        [JsonIgnore]
		public bool IsFilter { get; set; }

		/// <summary>
		/// Desc:厂商ID
		/// Default:-1
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_FACTORY_ID")]
		public long FactoryId { get; set; }

		/// <summary>
		/// Desc:厂商名称
		/// Default:平台
		/// Nullable:False
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_FACTORY_NAME")]
		public string FactoryName { get; set; } = string.Empty;

		/// <summary>
		/// Desc:网页标题
		/// Default:
		/// Nullable:True
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_PAGETITLE")]
        [JsonIgnore]
		public string PageTitle { get; set; } = string.Empty;

		/// <summary>
		/// Desc:网页关键词
		/// Default:
		/// Nullable:True
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_PAGEKEYS")]
        [JsonIgnore]
		public string PageKeys { get; set; } = string.Empty;

		/// <summary>
		/// Desc:网页描述
		/// Default:
		/// Nullable:True
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_PAGEDESC")]
        [JsonIgnore]
		public string PageDescription { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
		public string PurchaseOrgCode { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:True
		/// </summary>           
		[SugarColumn(ColumnName = "FPC_LOGO")]
        [JsonIgnore]
		public string Logo { get; set; } = string.Empty;

	}
}