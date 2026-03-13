using SqlSugar;

namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation
{
	public partial class ProductType
	{

		[Navigate(NavigateType.OneToOne, nameof(ProductId), nameof(Product.ProductId))]
		public Product Product { get; set; }
	}
}
