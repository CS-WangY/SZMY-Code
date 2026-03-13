using mymooo.core.Attributes.Redis;
using SqlSugar;

namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation
{
	public partial class Product
	{

		[Navigate(NavigateType.OneToOne, nameof(SmallId), nameof(ProductSmallClass.Id))]
		[RedisValue(isJson: true)]
		public ProductSmallClass ProductSmallClass { get; set; }

		[Navigate(NavigateType.OneToMany, nameof(ProductId), nameof(ProductType.ProductId))]
		[RedisValue(isJson: true)]
		public List<ProductType> ProductTypes { get; set; }

		[Navigate(NavigateType.OneToOne, nameof(ClassId))]
		[RedisValue(isJson: true)]
		public ProductClass ProductClass { get; set; }
	}
}
