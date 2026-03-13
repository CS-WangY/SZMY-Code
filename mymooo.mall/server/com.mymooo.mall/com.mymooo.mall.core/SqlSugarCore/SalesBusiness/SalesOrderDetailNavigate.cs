using SqlSugar;

namespace com.mymooo.mall.core.SqlSugarCore.SalesBusiness
{
	public partial class SalesOrderDetail
	{
		[Navigate(NavigateType.OneToOne, nameof(SalesOrderId))]
		public SalesOrder SalesOrder { get; set; }
	}
}
