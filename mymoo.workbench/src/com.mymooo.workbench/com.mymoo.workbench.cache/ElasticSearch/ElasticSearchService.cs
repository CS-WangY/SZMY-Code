using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymoo.workbench.cache.ElasticSearch
{
    public class ElasticSearchService: IElasticSearchService
    {
        //private IElasticClient elasticClient;
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
        /// 新增
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexName"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public async Task AddAsync<T>(string indexName, T doc) where T: class
        {
            var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
            var result = await elasticClient.IndexDocumentAsync<T>(doc);
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
        public async Task AddMultAsync<T>(string indexName, List<T> doc) where T : class
        {
            var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
            var result = await elasticClient.BulkAsync(b => b.Index(indexName).IndexMany(doc));
            if (result.Errors)
            {
                foreach (var itemWithError in result.ItemsWithErrors)
                {
                    _logger.LogError("批量新增数据失败 {0}: {1}",
                        itemWithError.Id, itemWithError.Error);
                }
            }
        }

        public async Task UpdateAsync<T>(string indexName, string field, string[] param) where T : class
        {
            var document =await GetDocumentAsync<T>(indexName, field, param);
            if (document!=null)
            {
                await DeleteDocumentAsync<T>(indexName,document);
            }
            else
            {
                _logger.LogError("更新数据失败未找到要更新的数据");
            }
            
        }

        public async Task<T> GetDocumentAsync<T>(string indexName, string field, string[] param) where T : class
        {
            var documentList = await GetDocumentsAsync<T>(indexName, field, param);
            if (documentList!=null&&documentList.Count>0)
            {
                return documentList.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public async Task<List<T>> GetDocumentsAsync<T>(string indexName, string field, string[] param) where T : class
        {
            var elasticClient = ElasticSearchExtensions.GetClient(indexName,_elasticClientUrl);
            var result = await elasticClient.SearchAsync<T>(s => 
                            s.Index(indexName).Query(q =>
                                ElasticSearchExtensions.CreateContainer<T>(field, param)
                            )
                        );
            if (result.IsValid&&result.Documents.Count>0)
            {
                return result.Documents.ToList();
            }
            else
            {
                return null;
            }
        }

        public async Task DeleteDocumentAsync<T>(string indexName, T document)where T :class
        {
            var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
            await elasticClient.DeleteAsync<T>(document);
        }
    }
}
