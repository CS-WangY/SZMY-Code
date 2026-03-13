namespace com.mymooo.mall.core.Model.Cache
{
	public class ProductSelectionLadderPrice
	{
		public decimal NumberLimit { get; set; }
		public decimal Price { get; set; }
		public decimal SalesPrice { get; set; }
		public int DeliveryDay { get; set; } = -1;
		public decimal QuantityDiscount { get; set; }
		public decimal LevelDiscount { get; set; } = 100;
	}
}
