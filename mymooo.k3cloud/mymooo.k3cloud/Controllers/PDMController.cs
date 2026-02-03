using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.pdm.SDK;
using mymooo.pdm.SDK.Dto;
using System.Net.Mime;

namespace mymooo.k3cloud.Controllers
{

    /// <summary>
    /// PDMController
    /// </summary>
    [Route("[controller]/[action]")]
    [ApiController]
    public class PDMController(PdmClient pdmClient) : Controller
    {
        private readonly PdmClient _pdmClient = pdmClient;
        
        /// <summary>
        /// 获取图库信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<List<QuoteRequestDtocs>>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetPDMImageinfo(List<QueryCappCacheRequest> request)
        {
            var response = _pdmClient.GetQuotationByCapp(request);
            return Json(response);
        }
    }
}
