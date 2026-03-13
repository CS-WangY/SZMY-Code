using com.mymooo.workbench.core.ElasticSearch;
using mymooo.core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.mymooo.workbench.cache.ElasticSearch.Service
{
	public interface IElasticSearchJsonService
    {
        /// <summary>
        /// 新增单个文档
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="documentId"></param>
        /// <param name="jsonDocument"></param>
        /// <returns></returns>
        Task AddDocumentAsync(string indexName, string documentId, string jsonDocument);

        /// <summary>
        /// 批量新增文档
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="documents"></param>
        /// <returns></returns>
        ResponseMessage<string> AddDocumentBulkAsync(string indexName, Dictionary<string, string> documents);
        /// <summary>
        /// 更新单个文档
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="documentId"></param>
        /// <param name="updateFields"></param>
        /// <returns></returns>
        Task UpdateDocumentAsync(string indexName, string documentId, Dictionary<string, string> updateFields);
        /// <summary>
        /// 批量更新文档
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="updateFields"></param>
        /// <returns></returns>
        Task UpdateDocumentBulkAsync(string indexName, Dictionary<string, Dictionary<string, string>> updateFields);
        /// <summary>
        /// 根据文档id删除文档
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="documentIds"></param>
        /// <returns></returns>
        Task DeleteDocumentBulkAsync(string indexName, string[] documentIds);

        /// <summary>
        /// 根据条件删除文档
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="deleteFilter"></param>
        /// <returns></returns>
        Task DeleteByQuery(string indexName, Dictionary<string, string> deleteFilter);

        /// <summary>
        /// 根据DocumentId获取文档列表
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="documentIds"></param>
        /// <returns></returns>
        Task<string> GetDocumentsAsync(string indexName, string[] documentIds);
        /// <summary>
        /// 根据DocumentId获取文档
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        Task<string> GetDocumentAsync(string indexName, string documentId);

        /// <summary>
        /// 分页搜索文档
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="condition"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        Task<string> SearchDocumentsAsync(ESSearchRequest request);

        /// <summary>
        /// 分页搜索文档
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="condition"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        string SearchDocumentsV2Async(ESSearchRequest request);



        /// <summary>
        /// 更新价目表价格
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="updateFields"></param>
        /// <returns></returns>
        Task UpdatePriceListCompanyCodeAsync(string indexName, string priceListId,int priceType, string[] companyCode, int hanldType);

        /// <summary>
        /// 更新价目表价格
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="updateFields"></param>
        /// <returns></returns>
        Task UpdatePriceListPriceAsync(string indexName, List<ElasticSearchPriceListUpdatePriceDto> PriceList);

        /// <summary>
        /// 更新价目表审核状态
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="priceId"></param>
        /// <param name="auditStatus"></param>
        /// <returns></returns>
        Task UpdatePriceListAuditStatusAsync(string indexName,int priceType, string priceId, int auditStatus);

        /// <summary>
        /// 更新价目表审核状态
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="productId"></param>
        /// <param name="auditStatus"></param>
        /// <returns></returns>
        Task UpdatePriceListAuditStatusAsync(string indexName, string productId, int auditStatus);

        void DownloadCacheModel(string basePath);
    }
}
