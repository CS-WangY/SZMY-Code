using mymooo.core.Attributes.Redis;
using SqlSugar;

namespace com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation
{
	public partial class ProductSmallClass
	{
		[Navigate(NavigateType.OneToOne, nameof(ProductEngineerId))]
		public ManagementUser ProductEngineer { get; set; }

		[Navigate(NavigateType.OneToOne, nameof(ProductManagerId))]
		public ManagementUser ProductManager { get; set; }

		[Navigate(NavigateType.OneToOne, nameof(ParentId))]
		public ProductSmallClass ParentProductSmall { get; set; }

		[Navigate(NavigateType.OneToMany, nameof(BusinessDivisionId), nameof(BusinessDivisionSupplyOrg.BusinessDivisionId))]
		[RedisValue(isJson: true)]
		public List<BusinessDivisionSupplyOrg> SupplyOrgs { get; set; }

		[RedisValue]
		[SugarColumn(IsIgnore = true)]
		public bool IsLeaf { get { return ParentId > 0; } }
	}
}
