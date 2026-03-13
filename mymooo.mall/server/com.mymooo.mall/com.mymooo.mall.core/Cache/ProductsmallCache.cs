//using com.mymooo.mall.core.Account;
//using com.mymooo.mall.core.Attributes;
//using com.mymooo.mall.core.Config;
//using com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation;
//using Elastic.Clients.Elasticsearch;
//using Elastic.Clients.Elasticsearch.QueryDsl;
//using Microsoft.Extensions.Options;

//namespace com.mymooo.mall.core.Cache
//{
//	[AutoInject(InJectType.Single)]
//	public class ProductSmallCache(IOptions<ElasticsearchConfig> elasticsearch) : AbstractCacheService<ProductSmallClass>(elasticsearch)
//	{
//		protected override ElasticsearchClient CreateElasticsearchClient()
//		{
//			ElasticsearchClient elasticsearchClient = new(new ElasticsearchClientSettings(new Uri(this.ElasticsearchConfig.Url))
//						.DefaultMappingFor<ProductSmallClass>(i => i
//							.IndexName(this.ElasticsearchConfig.ProductsmallIndex)
//							.IdProperty(p => p.Id)
//						).EnableDebugMode().PrettyJson().RequestTimeout(TimeSpan.FromMinutes(10)));
//			return elasticsearchClient;
//		}

//		public void LoadCache(MymoooContext mymoooContext)
//		{
//			var smalls = mymoooContext.SqlSugar.Queryable<ProductSmallClass>().Includes(p => p.ParentProductSmall).Includes(p => p.SupplyOrgs).Includes(p => p.ProductEngineer).Includes(p => p.ProductManager).ToList();
//			this.ElasticsearchClient.IndexMany(smalls);
//		}

//		public ProductSmallClass? Get(long id)
//		{
//			var result = ElasticsearchClient.Get<ProductSmallClass>(id);
//			if (result.IsValidResponse)
//			{
//				return result.Source;
//			}

//			return default;
//		}

//		public List<ProductSmallClass> Parent(long parentId)
//		{
//			var result = ElasticsearchClient.Search<ProductSmallClass>(s => s.From(0)
//			.Size(1000)
//			.Query(q => q.Term(t => t.ParentId, parentId)));
//			if (result.IsValidResponse)
//			{
//				return [.. result.Documents];
//			}

//			return [];
//		}

//		public List<ProductSmallClass> AllPublish()
//		{
//			var result = ElasticsearchClient.Search<ProductSmallClass>(s => s.From(0)
//			.Size(1000)
//			.Query(BuildQueryDescriptor()));
//			if (result.IsValidResponse)
//			{
//				return [.. result.Documents];
//			}

//			return [];
//		}

//		private Action<QueryDescriptor<ProductSmallClass>> BuildQueryDescriptor()
//		{
//			var mustQueries = new List<Action<QueryDescriptor<ProductSmallClass>>>
//			{
//                // 获取MyServiceXXX服务写的日志
//                //m => m.MatchPhrase(p => p.Field("code").Query("C02")),
//                // 获取XXX机构的日志
//                //m => m.Term(p => p.Field(i => i.InstitutionId).Value(condition.InstitutionId)),
//                // 日志必须包含字段“StartContent”或字段“EndContent”
//    //            m => m.Bool(
//				//	b => b.Should(
//				//		s => s.Exists(f => f.Field(o => o.BusinessDivisionNumber)),
//				//		s => s.Exists(f => f.Field(o => o.BusinessDivisionName))
//				//	)
//				//)
//                m => m.Term(p => p.IsPublish , true),
//				//gte 大于等于
//				//m => m.Range(r => r.NumberRange(d =>d.Field(i => i.ParentId).Gte(0)))
//				//Gt 大于
//				m => m.Range(r => r.NumberRange(d =>d.Field(i => i.ParentId).Gt(0)))
//			};
//			//// StartDate
//			//if (condition.StartDate.HasValue)
//			//{
//			//	// 时区已经是UTC， ELK中的时区也是UTC， 直接使用即可，否则需转时区
//			//	mustQueries.Add(m =>
//			//		m.Range(r =>
//			//			r.DateRange(d =>
//			//				d.Field(i => i.Timestamp).Gte(DateMath.Anchored(condition.StartDate.Value)))));
//			//}
//			//// EndDate
//			//if (condition.EndDate.HasValue)
//			//{
//			//	mustQueries.Add(m =>
//			//		m.Range(r =>
//			//			r.DateRange(d =>
//			//				d.Field(i => i.Timestamp).Lte(DateMath.Anchored(condition.EndDate.Value)))));
//			//}

//			// 关键字检索SearchText
//			//if (!string.IsNullOrWhiteSpace(condition.SearchText))
//			//{
//			//	// 模糊匹配
//			//	var filterValue = $"*{condition.SearchText}*";
//			//	var shouldQueries = new List<Action<QueryDescriptor<OperationLogOutput>>>
//			//	{
//			//		m => m.Wildcard(new WildcardQuery("OperatorEmail.keyword") { Value = filterValue }),
//			//		m => m.Wildcard(new WildcardQuery("StartContent.keyword") { Value = filterValue }),
//			//		m => m.Wildcard(new WildcardQuery("EndContent.keyword") { Value = filterValue })
//			//	};
//			//	mustQueries.Add(m => m.Bool(b => b.Should(shouldQueries.ToArray())));
//			//}

//			Action<QueryDescriptor<ProductSmallClass>> retQuery = q => q.Bool(b => b.Filter(mustQueries.ToArray()));

//			return retQuery;
//		}
//	}
//}
