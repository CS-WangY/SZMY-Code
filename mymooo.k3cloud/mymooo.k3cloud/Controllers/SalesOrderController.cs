using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.k3cloud.business.Services.Sales;
using mymooo.weixinWork.SDK.WeixinWorkMessage.Model;
using System.Net.Mime;

namespace mymooo.k3cloud.Controllers
{
    /// <summary>
    /// 销售订单应用
    /// </summary>
    [Route("[controller]/[action]")]
    [ApiController]
    public class SalesOrderController(SaleOrderService saleOrderService) : Controller
    {
        private readonly SaleOrderService _saleOrderService = saleOrderService;
        /// <summary>
        /// 批量更新销售成本价缓存
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseMessage<SendMessageResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult SalesOrderCostPriceRedisCache()
        {
            return Json(_saleOrderService.GetSalesOrderCostPriceRedisCache());
        }
    }
}
