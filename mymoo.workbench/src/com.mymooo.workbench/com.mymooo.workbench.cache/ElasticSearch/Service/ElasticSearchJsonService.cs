using com.mymooo.workbench.cache.ElasticSearch.Model;
using com.mymooo.workbench.core.ElasticSearch;
using com.mymooo.workbench.core.Utils;
using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using mymooo.core;
using mymooo.core.Utils.JsonConverter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace com.mymooo.workbench.cache.ElasticSearch.Service
{
	public class ElasticSearchJsonService : IElasticSearchJsonService
    {
        private IConfiguration _configuration;
        private string _elasticClientUrl;
        private readonly ILogger _logger;
        private readonly HttpUtils _httpUtils;
        public ElasticSearchJsonService(IConfiguration configuration, ILogger<ElasticSearchService> logger, HttpUtils httpUtils)
        {
            _configuration = configuration;
            _logger = logger;
            _elasticClientUrl = configuration["elasticsearch:url"];
            _httpUtils = httpUtils;
            //_elasticClientUrl = "http://192.168.5.61:9200/";
        }

        /// <summary>
        /// 新增单个文档
        /// </summary>
        /// <param name="indexName">所有操作索引名称必须全小写</param>
        /// <param name="documentId"></param>
        /// <param name="jsonDocument"></param>
        /// <returns></returns>
        public async Task AddDocumentAsync(string indexName, string documentId, string jsonDocument)
        {
            var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
            try
            {
                var result = await elasticClient.LowLevel.IndexAsync<StringResponse>(indexName.ToLower(), documentId, jsonDocument);
                if (!result.Success)
                {
                    _logger.LogError("新增单个ES文档失败 {0},{1}:{2}",
                            result.Body, indexName, jsonDocument);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("新增单个ES文档异常 {0},{1}:{2}",
                            ex.Message, indexName, jsonDocument);
                throw;
            }
        }
        /// <summary>
        /// 批量新增文档
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="documents"></param>
        /// <returns></returns>
        public ResponseMessage<string> AddDocumentBulkAsync(string indexName, Dictionary<string, string> documents)
        {
            var response = new ResponseMessage<string>();
            var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
            try
            {
                //TODO
                //超过1000数据后分批处理，超数量会失败
                var request = new List<string>();
                var pages = documents.Count / 1000;
                if (documents.Count % 1000 > 0)
                {
                    pages++;
                }
                for (int page = 1; page <= pages; page++)
                {
                    request = [];
                    for (int i = 0; i < 1000; i++)
                    {
                        var _index = (page - 1) * 1000 + i;
                        if (_index >= documents.Count)
                        {
                            break;
                        }
                        var item = documents.ElementAt(_index);
                        request.Add(JsonConvert.SerializeObject(new { index = new { _index = indexName.ToLower(), _id = item.Key } }));
                        request.Add(item.Value);
                        //request.Add(JsonConvert.SerializeObject(item.Value));
                    }
                    request.Add("\n");
                    var result = elasticClient.LowLevel.Bulk<StringResponse>(PostData.MultiJson(request));
                    var resultobj = JsonSerializerOptionsUtils.Deserialize<ElasticSearchBulkAddResponse>(result.Body);
                    if (!result.Success || resultobj.errors)
                    {
                        response.Code = ResponseCode.Exception;
                        response.ErrorMessage = String.Format("批量新增ES文档失败 {0},{1}:{2}",
                                result.Body, indexName, JsonSerializerOptionsUtils.Serialize(request));
                        _logger.LogError("批量新增ES文档失败 {0},{1}:{2}",
                                result.Body, indexName, JsonSerializerOptionsUtils.Serialize(request));
                    }
                    else
                    {
                        response.Code = ResponseCode.Success;
                        response.Message = "更新成功";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.Exception;
                response.ErrorMessage = String.Format("批量新增ES文档失败 {0},{1}:{2}",
                             ex.Message, indexName, JsonSerializerOptionsUtils.Serialize(documents));
                _logger.LogError("批量新增ES文档失败 {0},{1}:{2}",
                             ex.Message, indexName, JsonSerializerOptionsUtils.Serialize(documents));
            }
            return response;
        }

        /// <summary>
        /// 更新单个文档
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="documentId"></param>
        /// <param name="updateFields"></param>
        /// <returns></returns>
        public async Task UpdateDocumentAsync(string indexName, string documentId, Dictionary<string, string> updateFields)
        {
            try
            {
                var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
                var updateRequest = new ElasticSearchUpdateRequest();
                foreach (var item in updateFields)
                {
                    updateRequest.script.source += $"ctx._source.{item.Key}=params.{item.Value};"; //拼接参数
                }
                updateRequest.script.lang = "painless";
                updateRequest.script.@params = updateFields;
                var result = await elasticClient.LowLevel.UpdateAsync<StringResponse>(indexName.ToLower(), documentId, PostData.Serializable(updateRequest));
                if (!result.Success)
                {
                    _logger.LogError("更新单个ES文档失败 {0},{1}:{2}",
                             result.Body, indexName, JsonSerializerOptionsUtils.Serialize(updateFields));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("更新单个ES文档异常 {0},{1}:{2}",
                             ex.Message, indexName, JsonSerializerOptionsUtils.Serialize(updateFields));
            }
        }
        /// <summary>
        /// 批量更新文档
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="updateFields"></param>
        /// <returns></returns>
        public async Task UpdateDocumentBulkAsync(string indexName, Dictionary<string, Dictionary<string, string>> updateFields)
        {
            try
            {
                var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
                var request = new List<string>();
                foreach (var item in updateFields)
                {
                    var bulkHead = new { update = new { _id = item.Key, _index = indexName.ToLower() } };
                    request.Add(JsonConvert.SerializeObject(bulkHead));
                    var updateRequest = new ElasticSearchUpdateRequest();
                    foreach (var childItem in item.Value)
                    {
                        updateRequest.script.source += $"ctx._source.{childItem.Key} = params.{childItem.Key};";
                    }
                    updateRequest.script.lang = "painless";
                    updateRequest.script.@params = item.Value;
                }
                request.Add("\n");
                var result = await elasticClient.LowLevel.BulkAsync<StringResponse>(indexName.ToLower(), PostData.MultiJson(request));
                var resultobj = JsonSerializerOptionsUtils.Deserialize<ElasticSearchBulkAddResponse>(result.Body);
                if (!result.Success || resultobj.errors)
                {
                    _logger.LogError("批量更新文档失败 {0},{1}:{2}",
                            result.Body, indexName, JsonSerializerOptionsUtils.Serialize(updateFields));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("批量更新文档异常 {0},{1}:{2}",
                            ex.Message, indexName, JsonSerializerOptionsUtils.Serialize(updateFields));
            }
        }


        /// <summary>
        /// 根据文档id删除文档
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="documentIds"></param>
        /// <returns></returns>
        public async Task DeleteDocumentBulkAsync(string indexName, string[] documentIds)
        {
            var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
            try
            {
                var documentList = new List<string>();
                foreach (var item in documentIds)
                {
                    documentList.Add(JsonConvert.SerializeObject(new { delete = new { _index = indexName.ToLower(), _id = item } }));
                }
                var result = await elasticClient.LowLevel.BulkAsync<StringResponse>(indexName.ToLower(), PostData.MultiJson(documentList));
                var resultobj = JsonSerializerOptionsUtils.Deserialize<ElasticSearchBulkAddResponse>(result.Body);
                if (!result.Success || resultobj.errors)
                {
                    _logger.LogError("根据DocumentId删除文档失败 {0},{1}:{2}",
                             result.Body, indexName, JsonSerializerOptionsUtils.Serialize(documentIds));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("根据DocumentId删除文档异常 {0},{1}:{2}",
                             ex.Message, indexName, JsonSerializerOptionsUtils.Serialize(documentIds));
            }
        }

        /// <summary>
        /// 根据条件删除文档
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="deleteFilter"></param>
        /// <returns></returns>
        public async Task DeleteByQuery(string indexName, Dictionary<string, string> deleteFilter)
        {
            var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
            try
            {
                var result = await elasticClient.LowLevel.DeleteByQueryAsync<StringResponse>(indexName.ToLower(), PostData.Serializable(new { query = new { term = deleteFilter } }));

                var resultobj = JsonSerializerOptionsUtils.Deserialize<ElasticSearchBulkAddResponse>(result.Body);
                if (!result.Success || resultobj.errors)
                {
                    _logger.LogError("根据条件删除文档失败 {0},{1}:{2}",
                             result.Body, indexName, JsonSerializerOptionsUtils.Serialize(deleteFilter));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("根据条件删除文档异常 {0},{1}:{2}",
                             ex.Message, indexName, JsonSerializerOptionsUtils.Serialize(deleteFilter));
            }
        }

        /// <summary>
        /// 根据DocumentId获取文档列表
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="documentIds"></param>
        /// <returns></returns>
        public async Task<string> GetDocumentsAsync(string indexName, string[] documentIds)
        {
            var documents = "";
            var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
            try
            {
                var request = new ElasticSearchBulkGetRequest();
                foreach (var item in documentIds)
                {
                    var doc = new DocItem();
                    doc._id = item;
                    doc._index = indexName.ToLower();
                    request.docs.Add(doc);
                }
                var result = await elasticClient.LowLevel.MultiGetAsync<StringResponse>(indexName.ToLower(), PostData.Serializable(request));
                if (!result.Success)
                {
                    _logger.LogError("根据DocumentId获取文档列表失败 {0},{1}:{2}",
                            result.Body, indexName, JsonSerializerOptionsUtils.Serialize(documentIds));
                }
                else
                {
                    var resultDto = JsonConvert.DeserializeObject<ElasticSearchBulkGetResponse>(result.Body);

                    foreach (var item in resultDto.docs)
                    {
                        if (!string.IsNullOrEmpty(documents))
                        {
                            documents += ",";
                        }
                        documents += item._source;
                    }
                    documents = $"[{documents}]";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("根据DocumentId获取文档列表异常 {0},{1}:{2}",
                            ex.Message, indexName, JsonSerializerOptionsUtils.Serialize(documentIds));
            }
            return documents;
        }


        /// <summary>
        /// 根据DocumentId获取文档
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public async Task<string> GetDocumentAsync(string indexName, string documentId)
        {
            var document = "";
            var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
            try
            {
                var result = await elasticClient.LowLevel.GetAsync<StringResponse>(indexName.ToLower(), documentId);
                if (!result.Success)
                {
                    _logger.LogError("根据DocumentId获取文档失败 {0},{1}:{2}",
                             result.Body, indexName, documentId);
                }
                else
                {
                    var resultDto = JsonConvert.DeserializeObject<ElasticSearchGetResponse>(result.Body);
                    document = resultDto.docs._source;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("根据DocumentId获取文档失败 {0},{1}:{2}",
                             ex.Message, indexName, documentId);
            }
            return document;
        }


        /// <summary>
        /// 分页搜索文档
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="condition"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public async Task<string> SearchDocumentsAsync(ESSearchRequest request)
        {
            var documents = "";
            var elasticClient = ElasticSearchExtensions.GetClient(request.IndexName.ToLower(), _elasticClientUrl);
            //elasticClient.ServerCertificateValidationCallback((a, b, c, d) => true);
            try
            {
                //_logger.LogError($"ESUrl:{_elasticClientUrl},Param:{request}");
                var searchEntity = ElasticSearchExtensions.CreateSearchCondition(request);
                var json = JsonSerializerOptionsUtils.Serialize(searchEntity);
                //_logger.LogError($"ESUrl:{_elasticClientUrl},Param:{json}");
                var result = await elasticClient.LowLevel.SearchAsync<StringResponse>(request.IndexName.ToLower(), PostData.Serializable(searchEntity));
                _logger.LogError($"ESUrl:{_elasticClientUrl},Param:{json},Result:{JsonConvert.SerializeObject(result)}");
                var resultobj = JsonSerializerOptionsUtils.Deserialize<ElasticSearchBulkAddResponse>(result.Body);
                //_logger.LogError($"ESUrl:{_elasticClientUrl},Param:{json},Result:{result.Body}");
                if (!result.Success || resultobj.errors)
                {
                    _logger.LogError("分页搜索文档失败 {0},{1}:{2}",
                             result.Body, request.IndexName.ToLower(), JsonSerializerOptionsUtils.Serialize(request));
                }
                else
                {
                    var resultDto = JsonConvert.DeserializeObject<ElasticSearchSearchResponse>(result.Body);
                    if (resultDto.hits.hits != null && resultDto.hits.hits.Count > 0)
                    {
                        foreach (var item in resultDto.hits.hits)
                        {
                            if (!string.IsNullOrEmpty(documents))
                            {
                                documents += ",";
                            }
                            documents += item._source;
                        }
                    }
                    documents = $"[{documents}]";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("分页搜索文档异常 {0},{1}:{2}",
                    ex.Message, request.IndexName, JsonSerializerOptionsUtils.Serialize(request));
            }
            return documents;
            //var obj = JsonSerializerOptionsUtils.Deserialize<Object>(documents);
            //return JsonSerializerOptionsUtils.Deserialize<JObject>(documents);
        }

        /// <summary>
        /// 分页搜索文档
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="condition"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public string SearchDocumentsV2Async(ESSearchRequest request)
        {
            var documents = "";
            var elasticClient = ElasticSearchExtensions.GetClient(request.IndexName.ToLower(), _elasticClientUrl);
            //elasticClient.ServerCertificateValidationCallback((a, b, c, d) => true);
            try
            {
                var searchEntity = ElasticSearchExtensions.CreateSearchCondition(request);
                var json = JsonSerializerOptionsUtils.Serialize(searchEntity);

                //var result = HttpUtils.InvokeWebService($"{_elasticClientUrl}{request.IndexName}/_search", "post", json);

                //_logger.LogError($"ESUrl:{_elasticClientUrl},Param:{json},Result:{JsonConvert.SerializeObject(result)}");
                //var resultobj = JsonSerializerOptionsUtils.Deserialize<ElasticSearchSearchResponse>(result);
                ////_logger.LogError($"ESUrl:{_elasticClientUrl},Param:{json},Result:{result.Body}");
                //if (!result. || resultobj.errors)
                //{
                //    _logger.LogError("分页搜索文档失败 {0},{1}:{2}",
                //             result.Body, request.IndexName.ToLower(), JsonSerializerOptionsUtils.Serialize(request));
                //}
                //else
                //{
                var resultDto = _httpUtils.InvokePostWebService<ElasticSearchSearchResponseV2>($"{_elasticClientUrl}{request.IndexName}/_search", json);
                //var resultDto = JsonConvert.DeserializeObject<ElasticSearchSearchResponseV2>(result);
                if (resultDto.hits.hits != null && resultDto.hits.hits.Count > 0)
                {
                    foreach (var item in resultDto.hits.hits)
                    {
                        if (!string.IsNullOrEmpty(documents))
                        {
                            documents += ",";
                        }
                        documents += item._source;
                    }
                }
                documents = $"[{documents}]";
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError("分页搜索文档异常 {0},{1}:{2}",
                    ex.Message, request.IndexName, JsonSerializerOptionsUtils.Serialize(request));
            }
            return documents;
            //var obj = JsonSerializerOptionsUtils.Deserialize<Object>(documents);
            //return JsonSerializerOptionsUtils.Deserialize<JObject>(documents);
        }
        public string esHost = "http://192.168.5.61:9200/";
        public void DownloadCacheModel(string webRootPath)
        {
            var searchPara = new { size = 100 };
            var json = JsonSerializerOptionsUtils.Serialize(searchPara);
            var result = _httpUtils.InvokePostWebService($"{esHost}matrixprices/_search?scroll=1d", json);
            var resultDto = JsonConvert.DeserializeObject<ElasticSearchSearchResponse>(result);
            var productModelList = new List<string>();
            while (resultDto.hits.hits!=null&& resultDto.hits.hits.Count > 0)
            {
                var scrollId = resultDto._scroll_id;
                var models = resultDto.hits.hits.Select(e=> e._id).ToList();
                if (models != null)
                {
                    productModelList.AddRange(models);
                }
                resultDto = GetResultByScroll(scrollId);
            }

            string filePath = Path.Combine(webRootPath,$"TempAttachment/Export/ElasticSearchModel{DateTime.Now.ToString("yyyyMMdd")}.txt");
            
            string path = Path.Combine(webRootPath, $"TempAttachment/Export");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (!File.Exists(filePath))
            {
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    sw.WriteLine(string.Join(',',productModelList));
                }
            }
            
        }
        public ElasticSearchSearchResponse GetResultByScroll(string scrollId)
        {
            var result = _httpUtils.InvokeGetWebService<ElasticSearchSearchResponse>($"{esHost}_search/scroll?scroll=1d&pretty=&scroll_id=" +scrollId);
            //var resultDto = JsonConvert.DeserializeObject<ElasticSearchSearchResponse>(result);
            return result;
        }

        #region 非公共方法
        /// <summary>
        /// 更新价目表组织编码
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="priceListId"></param>
        /// <param name="priceType"></param>
        /// <param name="companyCode"></param>
        /// <param name="hanldType">1新增组织编码，0删除组织编码</param>
        /// <returns></returns>
        public async Task UpdatePriceListCompanyCodeAsync(string indexName, string priceListId, int priceType, string[] companyCode, int hanldType)
        {
            try
            {
                var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
                var request = new List<string>();
                var documentIds = await GetPriceListDocumentIdsAndIndexAsync(indexName, priceListId, priceType);
                if (documentIds == null || documentIds.Count == 0)
                {
                    return;
                }

                foreach (var item in documentIds)
                {
                    foreach (var code in companyCode)
                    {
                        var bulkHead = new { update = new { _id = item.DocumentId, _index = indexName } };
                        request.Add(JsonConvert.SerializeObject(bulkHead));
                        var updateRequest = new ElasticSearchUpdateRequest();
                        //if (hanldType == 1)
                        //{
                        updateRequest.script.source = $"ctx._source.suppliserPrice[{item.Index}].companyCodes.add(params.companyCode);";   //增加企业编码 
                        //}
                        //else
                        //{
                        //    updateRequest.script.source = $"ctx._source.suppliserPrice[{item.Value}].companyCodes.remove(params.companyCode);";   //增加企业编码 
                        //}
                        updateRequest.script.lang = "painless";
                        updateRequest.script.@params = new Dictionary<string, string>();
                        updateRequest.script.@params.Add("companyCode", code);
                        request.Add(JsonConvert.SerializeObject(updateRequest));
                    }
                }
                request.Add("\n");
                var result = await elasticClient.LowLevel.BulkAsync<StringResponse>(indexName.ToLower(), PostData.MultiJson(request));
                var resultobj = JsonSerializerOptionsUtils.Deserialize<ElasticSearchBulkAddResponse>(result.Body);
                if (!result.Success || resultobj.errors)
                {
                    _logger.LogError("批量更新文档失败 {0},{1}:{2}",
                            result.Body, indexName, JsonSerializerOptionsUtils.Serialize(new { priceListId = priceListId, companyCode = companyCode }));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("批量更新文档异常 {0},{1}:{2}",
                            ex.Message, indexName, JsonSerializerOptionsUtils.Serialize(new { priceListId = priceListId, companyCode = companyCode }));
            }
        }



        /// <summary>
        /// 更新价目表审核状态
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="priceListType"></param>
        /// <param name="productId"></param>
        /// <param name="auditStatus"></param>
        /// <returns></returns>
        public async Task UpdatePriceListAuditStatusAsync(string indexName, int priceType, string priceId, int auditStatus)
        {
            try
            {
                var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
                var request = new List<string>();
                var dic = new Dictionary<string, string>();
                dic.Add("suppliserPrice.priceListId", priceId.ToString());
                dic.Add("suppliserPrice.priceType", priceType.ToString());
                var documentIds = await GetPriceListDocumentIdsAndIndexAsync(indexName, dic);
                if (documentIds != null && documentIds.Count != 0)
                {
                    foreach (var item in documentIds)
                    {
                        var bulkHead = new { update = new { _id = item.DocumentId, _index = indexName } };
                        request.Add(JsonConvert.SerializeObject(bulkHead));
                        var updateRequest = new ElasticSearchUpdateRequest();
                        updateRequest.script.source = $"ctx._source.suppliserPrice[{item.Index}].auditStatus=params.auditStatus;";   //增加企业编码 
                        updateRequest.script.lang = "painless";
                        updateRequest.script.@params = new Dictionary<string, string>();
                        updateRequest.script.@params.Add("auditStatus", auditStatus.ToString());
                        request.Add(JsonConvert.SerializeObject(updateRequest));
                    }
                    request.Add("\n");
                    var result = await elasticClient.LowLevel.BulkAsync<StringResponse>(indexName.ToLower(), PostData.MultiJson(request));
                    var resultobj = JsonSerializerOptionsUtils.Deserialize<ElasticSearchBulkAddResponse>(result.Body);
                    if (!result.Success || resultobj.errors)
                    {
                        _logger.LogError("批量更新文档失败 {0},{1}:{2}",
                                result.Body, indexName, JsonSerializerOptionsUtils.Serialize(new { priceId = priceId, auditStatus = auditStatus }));
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("批量更新文档异常 {0},{1}:{2}",
                            ex.Message, indexName, JsonSerializerOptionsUtils.Serialize(new { priceId = priceId, auditStatus = auditStatus }));
            }
        }

        /// <summary>
        /// 更新价目表审核状态
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="priceListType"></param>
        /// <param name="productId"></param>
        /// <param name="auditStatus"></param>
        /// <returns></returns>
        public async Task UpdatePriceListAuditStatusAsync(string indexName, string productId, int auditStatus)
        {
            try
            {
                var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
                var request = new List<string>();
                var dic = new Dictionary<string, string>();
                dic.Add("productId", productId.ToString());
                var documentIds = await GetPriceListDocumentIdsAndIndexAsync(indexName, dic);
                if (documentIds != null && documentIds.Count != 0)
                {
                    foreach (var item in documentIds)
                    {
                        var bulkHead = new { update = new { _id = item.DocumentId, _index = indexName } };
                        request.Add(JsonConvert.SerializeObject(bulkHead));
                        var updateRequest = new ElasticSearchUpdateRequest();
                        updateRequest.script.source = $"ctx._source.suppliserPrice[{item.Index}].auditStatus=params.auditStatus;";   //增加企业编码 
                        updateRequest.script.lang = "painless";
                        updateRequest.script.@params = new Dictionary<string, string>();
                        updateRequest.script.@params.Add("auditStatus", auditStatus.ToString());
                        request.Add(JsonConvert.SerializeObject(updateRequest));
                    }
                    request.Add("\n");
                    var result = await elasticClient.LowLevel.BulkAsync<StringResponse>(indexName.ToLower(), PostData.MultiJson(request));
                    var resultobj = JsonSerializerOptionsUtils.Deserialize<ElasticSearchBulkAddResponse>(result.Body);
                    if (!result.Success || resultobj.errors)
                    {
                        _logger.LogError("批量更新文档失败 {0},{1}:{2}",
                                result.Body, indexName, JsonSerializerOptionsUtils.Serialize(new { productId = productId, auditStatus = auditStatus }));
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("批量更新文档异常 {0},{1}:{2}",
                            ex.Message, indexName, JsonSerializerOptionsUtils.Serialize(new { productId = productId, auditStatus = auditStatus }));
            }
        }

        /// <summary>
        /// 更新价目表价格
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="updateFields"></param>
        /// <returns></returns>
        public async Task UpdatePriceListPriceAsync(string indexName, List<ElasticSearchPriceListUpdatePriceDto> PriceList)
        {
            try
            {
                var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
                var request = new List<string>();
                foreach (var item in PriceList)
                {
                    var bulkHead = new { update = new { _id = item.Number, _index = indexName } };
                    request.Add(JsonConvert.SerializeObject(bulkHead));
                    var updateRequest = new ElasticSearchUpdateRequest();
                    updateRequest.script.source = $"ctx._source.suppliserPrice[{item.PriceIndex}].price = params.price{item.PriceIndex};";
                    updateRequest.script.lang = "painless";
                    updateRequest.script.@params = new Dictionary<string, string>();
                    updateRequest.script.@params.Add($"price{item.PriceIndex}", item.Price.ToString());
                    request.Add(JsonConvert.SerializeObject(updateRequest));
                }
                request.Add("\n");
                var result = await elasticClient.LowLevel.BulkAsync<StringResponse>(indexName.ToLower(), PostData.MultiJson(request));
                var resultobj = JsonSerializerOptionsUtils.Deserialize<ElasticSearchBulkAddResponse>(result.Body);
                if (!result.Success || resultobj.errors)
                {
                    _logger.LogError("批量更新价目表价格失败 {0},{1}:{2}",
                            result.Body, indexName, JsonSerializerOptionsUtils.Serialize(PriceList));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("批量更新价目表价格异常 {0},{1}:{2}",
                            ex.Message, indexName, JsonSerializerOptionsUtils.Serialize(PriceList));
            }
        }
        /// <summary>
        /// 根据价目表Id获取文档Id和价目表下标
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="priceListId"></param>
        /// <returns></returns>
        public async Task<List<ElasticSearchHanldDto>> GetPriceListDocumentIdsAndIndexAsync(string indexName, string priceListId, int priceType)
        {
            var documents = new List<ElasticSearchHanldDto>();
            var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
            try
            {
                var request = new ElasticSearchSearchRequest();
                request.query.constant_score.filter.@bool.must = new ChildBool();
                request.query.constant_score.filter.@bool.must.@bool.must = new List<object>();
                var term = new TermWrap();
                term.term = new Dictionary<string, string>();
                term.term.Add("suppliserPrice.priceListId", priceListId);
                request.query.constant_score.filter.@bool.must.@bool.must.Add(term);

                var priceTypeTerm = new TermWrap();
                priceTypeTerm.term = new Dictionary<string, string>();
                priceTypeTerm.term.Add("suppliserPrice.priceType", priceType.ToString());
                request.query.constant_score.filter.@bool.must.@bool.must.Add(priceTypeTerm);

                //只查当前正在使用中的价目表
                var priceEnableTerm = new TermWrap();
                priceEnableTerm.term = new Dictionary<string, string>();
                priceEnableTerm.term.Add("suppliserPrice.auditStatus", "1");
                request.query.constant_score.filter.@bool.must.@bool.must.Add(priceEnableTerm);


                var auditStatusTerm = new TermWrap();
                auditStatusTerm.term = new Dictionary<string, string>();
                auditStatusTerm.term.Add("suppliserPrice.isEnable", "true");
                request.query.constant_score.filter.@bool.must.@bool.must.Add(auditStatusTerm);

                var json = JsonSerializerOptionsUtils.Serialize(request);
                var result = await elasticClient.LowLevel.SearchAsync<StringResponse>(indexName, PostData.Serializable(request));
                if (!result.Success)
                {
                    _logger.LogError("根据条件获取文档Id集合失败 {0},{1}:{2}",
                             result.Body, indexName.ToLower(), JsonSerializerOptionsUtils.Serialize(new { priceListId = priceListId }));
                }
                else
                {
                    var resultDto = JsonConvert.DeserializeObject<ElasticSearchSearchResponse>(result.Body);
                    if (resultDto.hits.hits != null && resultDto.hits.hits.Count > 0)
                    {
                        foreach (var item in resultDto.hits.hits)
                        {
                            if (item._source.suppliserPrice == null || item._source.suppliserPrice.Count == 0)
                            {
                                continue;
                            }
                            //找到价目表的下标
                            for (int i = 0; i < item._source.suppliserPrice.Count; i++)
                            {
                                if (item._source.suppliserPrice[i].priceListId == priceListId)
                                {
                                    var doc = new ElasticSearchHanldDto();
                                    doc.DocumentId = item._id;
                                    doc.Index = i;
                                    documents.Add(doc);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("根据条件获取文档Id集合异常 {0},{1}:{2}",
                    ex.Message, indexName, JsonSerializerOptionsUtils.Serialize(new { priceListId = priceListId }));
            }
            return documents;
        }

        /// <summary>
        /// 根据产品Id获取文档Id和价目表下标
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="priceListId"></param>
        /// <returns></returns>
        public async Task<List<ElasticSearchHanldDto>> GetPriceListDocumentIdsAndIndexAsync(string indexName, Dictionary<string, string> paraDic)
        {
            var documents = new List<ElasticSearchHanldDto>();
            var elasticClient = ElasticSearchExtensions.GetClient(indexName, _elasticClientUrl);
            try
            {
                var request = new ElasticSearchSearchRequest();
                request.query.constant_score.filter.@bool.must = new ChildBool();
                request.query.constant_score.filter.@bool.must.@bool.must = new List<object>();

                foreach (var dic in paraDic)
                {
                    var term = new TermWrap();
                    term.term = new Dictionary<string, string>();
                    term.term.Add(dic.Key, dic.Value);
                    request.query.constant_score.filter.@bool.must.@bool.must.Add(term);
                }

                //只查当前正在使用中的价目表
                var priceEnableTerm = new TermWrap();
                priceEnableTerm.term = new Dictionary<string, string>();
                priceEnableTerm.term.Add("suppliserPrice.isEnable", "true");
                request.query.constant_score.filter.@bool.must.@bool.must.Add(priceEnableTerm);

                var json = JsonSerializerOptionsUtils.Serialize(request);
                var result = await elasticClient.LowLevel.SearchAsync<StringResponse>(indexName, PostData.Serializable(request));
                if (!result.Success)
                {
                    _logger.LogError("根据条件获取文档Id集合失败 {0},{1}:{2}",
                             result.Body, indexName.ToLower(), JsonSerializerOptionsUtils.Serialize(paraDic));
                }
                else
                {
                    var resultDto = JsonConvert.DeserializeObject<ElasticSearchSearchResponse>(result.Body);
                    if (resultDto.hits.hits != null && resultDto.hits.hits.Count > 0)
                    {
                        foreach (var item in resultDto.hits.hits)
                        {
                            if (item._source.suppliserPrice == null || item._source.suppliserPrice.Count == 0)
                            {
                                continue;
                            }
                            //找到价目表的下标
                            for (int i = 0; i < item._source.suppliserPrice.Count; i++)
                            {
                                if (Convert.ToBoolean(item._source.suppliserPrice[i].isEnable))
                                {
                                    var doc = new ElasticSearchHanldDto();
                                    doc.DocumentId = item._id;
                                    doc.Index = i;
                                    documents.Add(doc);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("根据条件获取文档Id集合异常 {0},{1}:{2}",
                    ex.Message, indexName, JsonSerializerOptionsUtils.Serialize(paraDic));
            }
            return documents;
        }
        #endregion
    }



}
