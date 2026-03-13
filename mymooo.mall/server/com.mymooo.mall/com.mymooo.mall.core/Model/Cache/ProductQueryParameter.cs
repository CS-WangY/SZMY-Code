namespace com.mymooo.mall.core.Model.Cache
{
	public class ProductQueryParameter
	{
		public long ParamId { get; set; }
		public string ParamName { get; set; } = string.Empty;
		public string SelectData { get; set; } = string.Empty;
		public bool IsSpecOption { get; set; }
	}

}
