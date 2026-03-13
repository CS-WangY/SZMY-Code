namespace com.mymooo.mall.core.Model.Product
{
	public class ShortNumberselectionRequest
	{
		public long SupplierId { get; set; }
		public string Code { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string PriceType { get; set; } = string.Empty;

		public List<ShortNumberParameter> Parameters { get; set; } = [];

		public class ShortNumberParameter
		{
			public string ParameterCode { get; set; } = string.Empty;
			public string ParameterName { get; set; } = string.Empty;
			public string ParameterType { get; set; } = string.Empty;
			public bool IsShortNumber { get; set; }
			public long Interval { get; set; }
		}
	}
}
