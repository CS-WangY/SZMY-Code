using SqlSugar;

namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation
{
	public partial class ProductModelSmallClassMapping
	{
		/// <summary>
		/// 产品小类
		/// </summary>
		[Navigate(NavigateType.OneToOne, nameof(ProductSmallClassId))]
		public ProductSmallClass? ProductSmallClass { get; set; }
	}
}
