using com.mymooo.mall.core.Config;
using com.mymooo.mall.core.Model.Cache;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;
using mymooo.core.Attributes;

namespace com.mymooo.mall.core.Cache
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="elasticsearch"></param>
	[AutoInject(InJectType.Single)]
	public class ProductSelectionCacheService(IOptions<ElasticsearchConfig> elasticsearch) : ElasticsearchClient(CreateElasticsearchClient(elasticsearch.Value))
	{
		private readonly ElasticsearchConfig _elasticsearchConfig = elasticsearch.Value;
		private static ElasticsearchClientSettings CreateElasticsearchClient(ElasticsearchConfig elasticsearch)
		{
			ElasticsearchClientSettings elasticsearchClient = new(new Uri(elasticsearch.Url));
			return elasticsearchClient;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public bool Index(ProductSelectionIndex index)
		{
			var result = this.Index(index, $"{_elasticsearchConfig.ProductSelectionIndx}-{index.ProductId}");
			if (result.IsValidResponse)
			{
				return true;
			}
			return false;
		}
	}
}
