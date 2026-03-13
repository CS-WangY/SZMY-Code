using System;
using System.Linq;
using System.Text;

using SqlSugar;
namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation
{
	///<summary>
	///
	///</summary>
	[SugarTable("ProductModel_SmallClass_Mapping")]
	public partial class ProductModelSmallClassMapping
	{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
		public ProductModelSmallClassMapping()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
		{

		}

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(IsPrimaryKey = true)]
		public string Model { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
		public long ProductSmallClassId { get; set; }

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:True
		/// </summary>           
		public string ImageUrl { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
		public string Description { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:0
		/// Nullable:False
		/// </summary>           
		public long CreateUser { get; set; }

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:True
		/// </summary>           
		public DateTime? CreateDate { get; set; }

		/// <summary>
		/// Desc:
		/// Default:0
		/// Nullable:False
		/// </summary>           
		public long ProductId { get; set; }

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
		public string PriceType { get; set; }

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
		public string ShortNumber { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:0
		/// Nullable:False
		/// </summary>           
		public bool IsImport { get; set; }

	}
}