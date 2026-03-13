using mymooo.core.Attributes.Redis;
using SqlSugar;
using System.Text.Json.Serialization;

namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation
{
	///<summary>
	///平台商品总表（用于商品KEY统一生成和产品查询）
	///</summary>
	[SugarTable("F_PRD_MSTR")]
	[RedisKey("mymooo-product")]
	public partial class Product
    {
        /// <summary>
        /// 
        /// </summary>
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public Product()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
		{

        }

        /// <summary>
        /// Desc:商品KEY
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "FPM_PRD_ID")]
        [RedisPrimaryKey]
        public long ProductId { get; set; }

        /// <summary>
        /// Desc:商品分类KEY
        /// Default:-1
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_CLASS_ID")]
        [RedisValue]   
        public long ClassId { get; set; }

        /// <summary>
        /// Desc:商品推荐分类KEY
        /// Default:-1
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_RECOM_CLASS_ID")]
        public long RecomClassId { get; set; }

        /// <summary>
        /// Desc:商品发布标记	   1:已经发布	   0:没有发布
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_RELEASE_IND")]
        [RedisValue]   
        public bool IsRelease { get; set; }

        /// <summary>
        /// Desc:商品名称
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_PRD_NAME")]
        [RedisValue]   
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:商品描述
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_PRD_DESC")]
        public string ProductDescription { get; set; } = string.Empty;

        /// <summary>
        /// Desc:厂商简称
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_FACTORY_SHORT_NAME")]
        public string FactoryShortName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:商品品牌
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_BRAND_NAME")]
        [RedisValue]   
        public string BrandName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:商品目录图片
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_CATALOG_PIC_URL")]
        [RedisValue]   
        public string CatalogUrl { get; set; } = string.Empty;

        /// <summary>
        /// Desc:商品新特性
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_FEATURE_VALUES")]
        public string FratureValues { get; set; } = string.Empty;

        /// <summary>
        /// Desc:厂商KEY
        /// Default:-1
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_FACTORY_ID")]
        [RedisValue]   
        public long FactoryId { get; set; }

        /// <summary>
        /// Desc:商品排序权重
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_PRD_WEIGHT")]
        public decimal ProductWeight { get; set; }

        /// <summary>
        /// Desc:厂商分类KEY
        /// Default:-1
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_FAC_CLASS_ID")]
        public long FacotryCalssId { get; set; }

        /// <summary>
        /// Desc:品牌Key
        /// Default:-1
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_BRAND_ID")]
        [RedisValue]   
        public long BrandId { get; set; }

        /// <summary>
        /// Desc:品牌英文名
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_BRAND_NAMEEN")]
        [RedisValue]   
        public string BrandNameEn { get; set; } = string.Empty;

        /// <summary>
        /// Desc:评价分数(平均分)
        /// Default:5.0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_EVAL_SCORE")]
        [JsonIgnore]
        public decimal EvalScore { get; set; }

        /// <summary>
        /// Desc:评价数量
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_EVAL_NUM")]
        [JsonIgnore]
        public long EvalNum { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_FACTORY_WEIGHT")]
        [RedisValue]   
        public int FactoryWeight { get; set; }

        /// <summary>
        /// Desc:审核标记 0 未提交 1提交 2审核
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_IS_AUDIT")]
        [RedisValue]   
        public byte IsAudit { get; set; }

        /// <summary>
        /// Desc:提交人Id
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_SUB_USER_ID")]
        [JsonIgnore]
        public long SubmitUserId { get; set; }

        /// <summary>
        /// Desc:提交人名称
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_SUB_USER")]
        [JsonIgnore]
        public string SubmitUserName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:提交时间
        /// Default:1900-01-01
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_SUB_DATE")]
        [JsonIgnore]
        public DateTime? SubmitDate { get; set; }

        /// <summary>
        /// Desc:是否链接到别的官网 0 否 1 是
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_IS_LINK")]
        public byte IsLink { get; set; }

        /// <summary>
        /// Desc:官网地址
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_LINK_URL")]
        [RedisValue]   
        public string LinkUrl { get; set; } = string.Empty;

        /// <summary>
        /// Desc:官网PDF地址
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_OTH_PDF_URL")]
        [RedisValue]   
        public string PdfUrl { get; set; } = string.Empty;

        /// <summary>
        /// Desc:官网3d文件地址
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_OTH_3D_URL")]
        [RedisValue]   
        public string ThreedUrl { get; set; } = string.Empty;

        /// <summary>
        /// Desc:官网2d文件地址
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_OTH_2D_URL")]
        [RedisValue]   
        public string TwoUrl { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_IS_IMPORT")]
        public byte IsImport { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_PRD_PDFID")]
        [RedisValue]   
        public long PdfId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_VIEWCOUNT")]
        [JsonIgnore]
        public int ViewCount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_PDFVIEWCOUNT")]
        [JsonIgnore]
        public int PdfViewCount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_TWODOWNCOUNT")]
        [JsonIgnore]
        public int TwoDownCount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FPM_THREEDOWNCOUNT")]
        [JsonIgnore]
        public int ThreedDownCount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIs3DShow")]
        [RedisValue]   
        public string Is3DShow { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FIsInfluenceTo3dServer")]
        [RedisValue]   
        public bool IsInfluenceTo3dServer { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FMinPrice")]
        [RedisValue]   
        public string MinPrice { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FDeliverDay")]
        [RedisValue]   
        public string DeliverDay { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [SugarColumn(ColumnName = "FShowParameterSeq")]
        [RedisValue]   
        public string ShowParameterSeq { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [RedisValue]   
        public string SEOTitle { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [RedisValue]   
        public string SEOKeywords { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [RedisValue]   
        public string SEODescription { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [RedisValue]   
        public bool ConfirmedPrice { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public bool IsSincerity { get; set; }

        /// <summary>
        /// Desc:
        /// Default:1
        /// Nullable:False
        /// </summary>           
        [RedisValue]   
        public ProductPriceType PriceType { get; set; } 

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        [RedisValue]   
        public long SmallId { get; set; }
	}
}