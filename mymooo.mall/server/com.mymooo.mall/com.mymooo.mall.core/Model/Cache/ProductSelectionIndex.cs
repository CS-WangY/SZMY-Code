namespace com.mymooo.mall.core.Model.Cache
{
	public class ProductSelectionIndex
	{
		public string Id { get; set; } = string.Empty;
		public long ProductId { get; set; }
		//public long ProductTypeId { get; set; }
		public string Number { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		//public long SmallId { get; set; }
		//public string SmallCode { get; set; } = string.Empty;
		//public string SmallName { get; set; } = string.Empty;
		//public string ShortNumber { get; set; } = string.Empty;
		public Dictionary<string, string> Patameters { get; set; } = [];

		//public List<ProductQueryParameter> ProductQueryParamList { get; set; } = [];
	}
}
