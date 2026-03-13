using mymooo.core.Config;

namespace com.mymooo.mall.core.Config
{
	/// <summary>
	/// ES 配置
	/// </summary>
	public class ElasticsearchConfig : MymoooLogsConfig
	{
		/// <summary>
		/// 产品小类索引名
		/// </summary>
		public required string ProductsmallIndex { get; set; }

		/// <summary>
		/// 产品型号索引名
		/// </summary>
		public required string ProductNumberIndx { get; set; }

		/// <summary>
		/// 产品选型索引名
		/// </summary>
		public required string ProductSelectionIndx { get; set; }
	}

}
