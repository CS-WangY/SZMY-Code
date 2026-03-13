using SqlSugar;

namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation
{
	public partial class ProductClass
	{
		[Navigate(NavigateType.OneToMany, nameof(ClassId), nameof(Product.ClassId))]
		public List<Product> Products { get; set; }

		[Navigate(NavigateType.OneToOne, nameof(ParentClassId), nameof(ClassId))]
		public ProductClass ParentProductCalss { get; set; }
	}
}
