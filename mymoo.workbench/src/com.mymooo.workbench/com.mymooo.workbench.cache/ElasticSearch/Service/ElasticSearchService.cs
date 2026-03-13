using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static com.mymooo.workbench.core.ElasticSearch.ElasticSearchEnum;

namespace com.mymooo.workbench.cache.ElasticSearch.Service
{
    public class ElasticSearchService : IElasticSearchService
    {
        private IConfiguration _configuration;
        private string _elasticClientUrl;
        private readonly ILogger _logger;


        public ElasticSearchService(IConfiguration configuration, ILogger<ElasticSearchService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _elasticClientUrl = configuration["elasticsearch:url"];

        }
        /// <summary>
        /// 新增文档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexName"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public async Task AddDocumentAsync<T>(string indexName, T doc) where T : class
        {
            var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);

            var result = await elasticClient.IndexDocumentAsync(doc);
            if (!result.IsValid)
            {
                _logger.LogError("新增数据失败 {0}: {1}",
                        result.Id, result.ServerError.ToString());
            }
        }

        /// <summary>
        /// 批量新增
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexName"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public async Task AddMultDocumentAsync<T>(string indexName, List<T> doc) where T : class
        {
            var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
            var result = await elasticClient.BulkAsync(b => b.Index(indexName.ToLower()).IndexMany(doc));
            if (result.Errors)
            {
                foreach (var itemWithError in result.ItemsWithErrors)
                {
                    _logger.LogError("批量新增数据失败 {0}: {1}",
                        itemWithError.Id, itemWithError.Error);
                }
            }
        }

        /// <summary>
        /// 根据条件更新文档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexName"></param>
        /// <param name="field"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task UpdateDocumentAsync<T>(string indexName, Dictionary<string, string> param, T document) where T : class
        {
            var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
            var documentEntity = await GetDocumentAsync<T>(indexName, param);

        }

        /// <summary>
        /// 根据文档Id更新文档，注意只能更新Json第一级字段，第二级对象需要完整的数据覆盖，
        /// 可更新指定字段，不需要更新的字段请设置为null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexName"></param>
        /// <param name="documentId"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public async Task UpdateDocumentAsync<T>(string indexName, string documentId, T document) where T : class
        {
            var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
            var result = await elasticClient.UpdateAsync<T>(documentId, e => e.Index(indexName.ToLower()).Doc(document));
            if (!result.IsValid)
            {
                _logger.LogError("修改数据失败 {0}: {1}",
                        result.Id, result.ServerError.ToString());
            }
        }

        /// <summary>
        /// 删除文档根据DocumentId
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexName"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public async Task DeleteDocumentAsync<T>(string indexName, string documentId) where T : class
        {
            var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
            await elasticClient.DeleteAsync<T>(documentId);
        }

        /// <summary>
        /// 删除文档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexName"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        private async Task DeleteDocumentAsync<T>(string indexName, T document) where T : class
        {
            var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
            await elasticClient.DeleteAsync<T>(document);
        }

        /// <summary>
        /// 获取单个文档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexName"></param>
        /// <param name="param"></param>
        /// <param name="queryType"></param>
        /// <returns></returns>
        public async Task<T> GetDocumentAsync<T>(string indexName, Dictionary<string, string> param, ElasticSearchConditionLogicType queryType = ElasticSearchConditionLogicType.And) where T : class
        {
            var documentList = await GetDocumentsAsync<T>(indexName, param, queryType);
            if (documentList != null && documentList.Count > 0)
            {
                return documentList.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取文档集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexName"></param>
        /// <param name="param"></param>
        /// <param name="queryType">查询条件的逻辑方式默认And</param>
        /// <returns></returns>
        public async Task<List<T>> GetDocumentsAsync<T>(string indexName, Dictionary<string, string> param, ElasticSearchConditionLogicType queryType = ElasticSearchConditionLogicType.And) where T : class
        {
            var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
            var result = await elasticClient.SearchAsync<T>(
                       s => s.Index(indexName).Query(
                           q => ElasticSearchExtensions.CreateQueryContainer<T>(param, queryType)
                           )
                       );

            if (result.IsValid && result.Documents.Count > 0)
            {
                return result.Documents.ToList();
            }
            else
            {
                return null;
            }
        }
    }
}
