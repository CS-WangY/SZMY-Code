using com.mymooo.workbench.cache.ElasticSearch.Service;
using com.mymooo.workbench.core.Account;
using com.mymooo.workbench.core.ElasticSearch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Model.Elasticsearch;
using mymooo.core.Utils.Service;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace com.mymooo.workbench.Controllers
{
    /// <summary>
    /// 忽略写日志
    /// </summary>
	[AllowAnonymous, IgnoreLog]
    [Route("[controller]/[action]")]
    [ApiController]
    public class ElasticSearchController(IElasticSearchService elasticSearchService, IElasticSearchJsonService elasticSearchJsonService, IWebHostEnvironment webHostEnvironment, WorkbenchContext workbenchContext, LogsIndexService<WorkbenchContext, User> logsService) : Controller
    {
        private readonly IElasticSearchService _elasticSearchService = elasticSearchService;
        private readonly IElasticSearchJsonService _elasticSearchJsonService = elasticSearchJsonService;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        private readonly WorkbenchContext _workbenchContext = workbenchContext;
        private readonly LogsIndexService<WorkbenchContext, User> _logsService = logsService;


        #region 缓存Json操作


        /// <summary>
        /// 批量新增文档
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<string>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult EditDocuments([FromBody] ESAddRequest request)
        {
            var result = _elasticSearchJsonService.AddDocumentBulkAsync(request.IndexName, request.Documents);
            return Json(result);
        }

        /// <summary>
        /// 批量更新文档
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<dynamic>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult UpdateDocuments([FromBody] ESUpdateRequest request)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            var result = _elasticSearchJsonService.UpdateDocumentBulkAsync(request.IndexName, request.UpdateFields);
            if (result.IsFaulted)
            {
                message.Code = ResponseCode.Exception;
            }
            else
            {
                message.Code = ResponseCode.Success;
            }

            return Json(message);
        }
        /// <summary>
        /// 根据文档id删除文档
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<dynamic>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult DeleteDocument([FromBody] ESDeleteRequest request)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            var result = _elasticSearchJsonService.DeleteDocumentBulkAsync(request.IndexName, request.DocumentIds);
            if (result.IsFaulted)
            {
                message.Code = ResponseCode.Exception;
            }
            else
            {
                message.Code = ResponseCode.Success;
            }

            return Json(message);
        }

        /// <summary>
        /// 根据文档id删除文档
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<dynamic>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult DeleteDocumentByQuery([FromBody] ESDeleteRequest request)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            var result = _elasticSearchJsonService.DeleteByQuery(request.IndexName, request.DeleteFilter);
            if (result.IsFaulted)
            {
                message.Code = ResponseCode.Exception;
            }
            else
            {
                message.Code = ResponseCode.Success;
            }

            return Json(message);
        }

        /// <summary>
        /// 根据DocumentId获取文档列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<dynamic>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetDocuments([FromBody] ESGetRequest request)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            var result = _elasticSearchJsonService.GetDocumentsAsync(request.IndexName, request.DocumentIds);
            if (result.IsFaulted)
            {
                message.Code = ResponseCode.Exception;
            }
            else
            {
                message.Code = ResponseCode.Success;
                message.Data = result.Result;
            }

            return Json(message);
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        //[AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<string>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult SearchDocument([FromBody] ESSearchRequest request)
        {
            ResponseMessage<string> message = new ResponseMessage<string>();
            var result = _elasticSearchJsonService.SearchDocumentsAsync(request);
            if (result.IsFaulted)
            {
                message.Code = ResponseCode.Exception;
            }
            else
            {
                message.Code = ResponseCode.Success;
                message.Data = result.Result;
            }

            return Json(message);
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        //[AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<string>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult SearchDocumentV2([FromBody] ESSearchRequest request)
        {
            ResponseMessage<string> message = new ResponseMessage<string>();
            var result = _elasticSearchJsonService.SearchDocumentsV2Async(request);

            message.Code = ResponseCode.Success;
            message.Data = result;

            return Json(message);
        }
        #endregion

        #region 非公共方法

        /// <summary>
        /// 更新价目表价格
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<dynamic>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult UpdatePriceListCompanyCode([FromBody] ESUpdateRequest request)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            var result = _elasticSearchJsonService.UpdatePriceListCompanyCodeAsync(request.IndexName, request.PriceListId, request.PriceType, request.CompanyCodes, request.HanldType);
            if (result.IsFaulted)
            {
                message.Code = ResponseCode.Exception;
            }
            else
            {
                message.Code = ResponseCode.Success;
            }

            return Json(message);
        }
        /// <summary>
        /// 更新价目表审核状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<dynamic>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult UpdatePriceListAuditStatus([FromBody] ESUpdateRequest request)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            var result = _elasticSearchJsonService.UpdatePriceListAuditStatusAsync(request.IndexName, request.PriceType, request.PriceListId, request.AuditStatus);
            if (result.IsFaulted)
            {
                message.Code = ResponseCode.Exception;
            }
            else
            {
                message.Code = ResponseCode.Success;
            }

            return Json(message);
        }

        /// <summary>
        /// 更新价目表审核状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<dynamic>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult UpdatePriceListAuditStatusByProductId([FromBody] ESUpdateRequest request)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            var result = _elasticSearchJsonService.UpdatePriceListAuditStatusAsync(request.IndexName, request.ProductId, request.AuditStatus);
            if (result.IsFaulted)
            {
                message.Code = ResponseCode.Exception;
            }
            else
            {
                message.Code = ResponseCode.Success;
            }

            return Json(message);
        }
        /// <summary>
        /// 更新价目表价格
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<dynamic>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult UpdatePriceListPrice([FromBody] ESUpdateRequest request)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            var result = _elasticSearchJsonService.UpdatePriceListPriceAsync(request.IndexName, request.PriceList);
            if (result.IsFaulted)
            {
                message.Code = ResponseCode.Exception;
            }
            else
            {
                message.Code = ResponseCode.Success;
            }

            return Json(message);
        }

        /// <summary>
        /// 更新价目表价格
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResponseMessage<dynamic>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult DownloadProductModel()
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            _elasticSearchJsonService.DownloadCacheModel(_webHostEnvironment.WebRootPath);
            return Json(message);
        }

        #endregion 

        /// <summary>
        /// 重新创建http  logs 索引  用于测试
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseMessage<string>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> ReestablishHttpLogIndex()
        {
            return Json(await _logsService.ReestablishIndex(_workbenchContext, new RequestLogs() { RequestDate = DateTime.Now, SystemCode = _workbenchContext.ApigatewayConfig.SystemCode }));
        }
    }
}
