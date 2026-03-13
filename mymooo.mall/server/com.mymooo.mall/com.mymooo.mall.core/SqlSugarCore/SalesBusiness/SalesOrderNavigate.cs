using SqlSugar;

namespace com.mymooo.mall.core.SqlSugarCore.SalesBusiness
{
	public partial class SalesOrder
	{
		[Navigate(NavigateType.OneToMany, nameof(SalesOrderId))]
		public List<SalesOrderDetail> SalesOrderDetails { get; set; }
	}
}
