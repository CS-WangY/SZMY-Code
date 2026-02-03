using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.core.Model.BussinessModel.K3Cloud;
using mymooo.core.Utils;
using mymooo.k3cloud.business.Services.Purchase;
using mymooo.k3cloud.business.Services.Sales;
using mymooo.k3cloud.core.Account;
using mymooo.k3cloud.core.Approver;
using mymooo.k3cloud.core.PurchaseModel;
using mymooo.weixinWork.SDK.Approval;
using mymooo.weixinWork.SDK.Approval.Model;
using mymooo.weixinWork.SDK.WeixinWorkMessage.Model;
using Newtonsoft.Json.Linq;
using System.Net.Mime;

namespace mymooo.k3cloud.Controllers
{
    /// <summary>
    /// 采购订单应用
    /// </summary>
    [Route("[controller]/[action]")]
    [ApiController]
    public class PurchaseOrderController(PurchaseOrderService purchaseOrderService) : Controller
    {
        private readonly PurchaseOrderService _purchaseOrderService = purchaseOrderService;

        /// <summary>
        /// 插入缓存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<SendMessageResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult LoadRedisCache([FromBody] K3CloudRabbitMQMessage<POOrder, POOrderEntryItem> request)
        {
            _purchaseOrderService.GetPurchaseOrder(request);
            return Json(request);
        }
        /// <summary>
        /// 批量更新历史采购价缓存
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseMessage<SendMessageResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult FullPurchaseRedisCache()
        {
            return Json(_purchaseOrderService.FullPurchaseOrderHistoryPrice());
        }
    }
}
