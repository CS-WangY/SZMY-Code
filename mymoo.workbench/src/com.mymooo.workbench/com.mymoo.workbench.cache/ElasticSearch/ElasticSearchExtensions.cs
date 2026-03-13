using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.mymoo.workbench.cache.ElasticSearch
{
    public static class ElasticSearchExtensions
    {
        //public static void AddElasticsearch(
        //    this IServiceCollection services, IConfiguration configuration)
        //{
        //    var url = configuration["elasticsearch:url"];
        //    //var defaultIndex = configuration["elasticsearch:index"];
        //    var settings = new ConnectionSettings(new Uri(url));
        //    //var settings = new ConnectionSettings(new Uri(url))
        //    //    .DefaultIndex(defaultIndex);

        //    //AddDefaultMappings(settings);

        //    var client = new ElasticClient(settings);

        //    services.AddSingleton<IElasticClient>(client);

        //    //CreateIndex(client, defaultIndex);
        //}


        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="elasticClient"></param>
        public static CreateIndexResponse CreateIndex(this IElasticClient elasticClient, string indexName, int numberOfReplicas = 1, int numberOfShards = 5)
        {
            IIndexState indexState = new IndexState
            {
                Settings = new IndexSettings
                {
                    NumberOfReplicas = numberOfReplicas,
                    
                    NumberOfShards = numberOfShards
                }
            };
            Func<CreateIndexDescriptor, ICreateIndexRequest> func = x => x.InitializeUsing(indexState).Map(m => m.AutoMap());
            CreateIndexResponse response = elasticClient.Indices.Create(indexName, func);
            return response;
        }

        private static QueryContainer[] CreateMatch(string field, string[] param)
        {
            QueryContainer orQuery = null;
            List<QueryContainer> queryContainerList = new List<QueryContainer>();
            foreach (var item in param)
            {
                orQuery = new MatchQuery() { Field = field, Query = item };
                queryContainerList.Add(orQuery);
            }
            return queryContainerList.ToArray();
        }
        public static QueryContainer CreateContainer<T>(string field, string[] param)  where T : class
        {
            return new QueryContainerDescriptor<T>().Bool(
                b => b.Should(
                    CreateMatch(field, param)));
        }

        public static IElasticClient GetClient(string index,string url)
        {
            index = index.ToLower();
            var settings = new ConnectionSettings(new Uri(url)).DefaultIndex(index);
            var elasticClient = new ElasticClient(settings);
            ElasticSearchExtensions.CreateIndex(elasticClient, index);
            return elasticClient;
        }
    }
}
