namespace com.mymooo.mall.core.Srm
{
	public class ReplaceModelQueryModelReplaceRequest
	{
		public string CompanyCode { get; set; } = string.Empty;
		public required List<ReplaceModel> ModelList { get; set;}

		public class ReplaceModel
		{
			public string ProductModel { get; set;} = string.Empty;
		}
	}
}
