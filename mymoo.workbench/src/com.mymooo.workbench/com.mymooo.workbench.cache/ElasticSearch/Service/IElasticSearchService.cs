using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static com.mymooo.workbench.core.ElasticSearch.ElasticSearchEnum;

namespace com.mymooo.workbench.cache.ElasticSearch.Service
{
    public interface IElasticSearchService
    {
        /// <summary>
        /// 新增文档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexName"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        Task AddDocumentAsync<T>(string indexName, T doc) where T : class;
        /// <summary>
        /// 批量新增
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexName"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        Task AddMultDocumentAsync<T>(string indexName, List<T> doc) where T : class;
        /// <summary>
        /// 根据条件更新文档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexName"></param>
        /// <param name="field"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task UpdateDocumentAsync<T>(string indexName, Dictionary<string, string> param, T document) where T : class;
        /// <summary>
        /// 根据文档Id更新文档，注意只能更新Json第一级字段，第二级对象需要完整的数据覆盖，
        /// 可更新指定字段，不需要更新的字段请设置为null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexName"></param>
        /// <param name="documentId"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        Task UpdateDocumentAsync<T>(string indexName, string documentId, T document) where T : class;
        /// <summary>
        /// 删除文档根据DocumentId
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexName"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        Task DeleteDocumentAsync<T>(string indexName, string documentId) where T : class;

        /// <summary>
        /// 获取单个文档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexName"></param>
        /// <param name="param"></param>
        /// <param name="queryType"></param>
        /// <returns></returns>
        Task<T> GetDocumentAsync<T>(string indexName, Dictionary<string, string> param, ElasticSearchConditionLogicType queryType = ElasticSearchConditionLogicType.And) where T : class;
        /// <summary>
        /// 获取文档集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexName"></param>
        /// <param name="param"></param>
        /// <param name="queryType">查询条件的逻辑方式默认And</param>
        /// <returns></returns>
        Task<List<T>> GetDocumentsAsync<T>(string indexName, Dictionary<string, string> param, ElasticSearchConditionLogicType queryType = ElasticSearchConditionLogicType.And) where T : class;
    }
}
