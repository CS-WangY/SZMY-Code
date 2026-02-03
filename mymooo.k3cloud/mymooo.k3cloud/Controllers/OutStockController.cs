using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.k3cloud.business.Services.Sales;
using mymooo.k3cloud.core.Sales;
using System.Net.Mime;

namespace mymooo.k3cloud.Controllers
{
    /// <summary>
    /// 销售出库单
    /// </summary>
    [Route("[controller]/[action]")]
    [ApiController]
    public class OutStockController(OutStockService outStockService) : Controller
    {
        /// <summary>
        /// 获取发货数据
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseMessage<List<OutStockResponse>>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> GetDeliveryList(string orderNo)
        {
            return Json(await outStockService.GetDeliveryList(orderNo));
        }

        /// <summary>
        /// 查询售后单据状态
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<AfsDetailResultResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> AfsDetailResult(AfsDetailResultRequest request)
        {
            return Json(await outStockService.AfsDetailResult(request));
        }
        /// <summary>
        /// 销售出库数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
		[HttpPost]
		[ProducesResponseType(typeof(ResponseMessage<List<OutStockDetail>>), StatusCodes.Status200OK)]
		[Produces(MediaTypeNames.Application.Json)]
		public IActionResult GetOutStockList(SalOutStockRequest request)
		{
			return Json(outStockService.GetOutStockList(request));
		}
	}
}
