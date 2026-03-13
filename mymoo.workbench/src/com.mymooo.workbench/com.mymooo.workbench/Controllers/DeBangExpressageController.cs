using com.mymooo.workbench.core.Expressage.DeBang;
using com.mymooo.workbench.core.Utils;
using com.mymooo.workbench.Filter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using mymooo.core.Utils.JsonConverter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace com.mymooo.workbench.Controllers
{
    public class DeBangExpressageController : Controller
    {
        private readonly ILogger<DeBangExpressageController> _logger;
        private readonly DeBangExpressageInfo _expressageInfo;
        private readonly DeBangClient _debangClient;

        public DeBangExpressageController(ILogger<DeBangExpressageController> logger, IOptions<DeBangExpressageInfo> expressageInfo, DeBangClient deBangClient)
        {
            _logger = logger;
            _expressageInfo = expressageInfo.Value;
            _debangClient = deBangClient;
        }

        [AllowAnonymous]
        public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (request == null)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            request.customerCode = _expressageInfo.CustomerCode;
            request.companyCode = _expressageInfo.CompanyCode;
            request.logisticID = _expressageInfo.Sign + request.logisticID;

            var result = _debangClient.InvokeWebService("sandbox-web/dop-standard-ewborder/createOrderNotify.action", JsonSerializerOptionsUtils.Serialize(request));
            var response = JsonSerializerOptionsUtils.Deserialize<CreateOrderResponse>(result);
            return Content(result);
        }

        [AllowAnonymous]
        public IActionResult UpdateOrder([FromBody] UpdateOrderRequest request)
        {
            if (request == null)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            request.customerID = _expressageInfo.CustomerCode;
            request.orderSource = _expressageInfo.CompanyCode;
            request.logisticID = _expressageInfo.Sign + request.logisticID;

            var result = _debangClient.InvokeWebService("sandbox-web/standard-order/updateOrder.action", JsonSerializerOptionsUtils.Serialize(request));
            var response = JsonSerializerOptionsUtils.Deserialize<CreateOrderResponse>(result);
            return Content(result);
        }

        [AllowAnonymous]
        public IActionResult CancelOrder([FromBody] CancelOrderRequest request)
        {
            if (request == null)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            request.logisticID = _expressageInfo.Sign + request.logisticID;

            var result = _debangClient.InvokeWebService("sandbox-web/standard-order/cancelOrder.action", JsonSerializerOptionsUtils.Serialize(request));
            var response = JsonSerializerOptionsUtils.Deserialize<CreateOrderResponse>(result);
            return Content(result);
        }


        [AllowAnonymous]
        public IActionResult QueryOrder(string orderNo)
        {
            if (string.IsNullOrWhiteSpace(orderNo))
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            var logisticID = _expressageInfo.Sign + orderNo;

            var result = _debangClient.InvokeWebService("sandbox-web/standard-order/queryOrder.action", JsonSerializerOptionsUtils.Serialize(new { logisticCompanyID = "DEPPON", logisticID }));
            var response = JsonSerializerOptionsUtils.Deserialize<CreateOrderResponse>(result);
            return Content(result);
        }

        [AllowAnonymous]
        public IActionResult TraceQuery(string mailNo)
        {
            if (string.IsNullOrWhiteSpace(mailNo))
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }

            var result = _debangClient.InvokeWebService("sandbox-web/standard-order/newTraceQuery.action", JsonSerializerOptionsUtils.Serialize(new { mailNo }));
            var response = JsonSerializerOptionsUtils.Deserialize<TraceQueryResponse>(result);
            return Content(result);
        }

        [ServiceFilter(typeof(DeBangSignAttribute))]
        public IActionResult OrderStatusPush([FromForm] PushRequest request)
        {
            var orderStatus = JsonSerializerOptionsUtils.Deserialize<OrderStatusPushRequest>(request.Params);
            
            _logger.LogInformation(request.Params);
            return Json(new
            {
                logisticCompanyID = "DEPPON",
                logisticID = "",
                result = "true",
                resultCode = "1000",
                reason = "成功"
            });
        }

        [ServiceFilter(typeof(DeBangSignAttribute))]
        public IActionResult OrderRoutePush([FromForm] PushRequest request)
        {
            var route = JsonSerializerOptionsUtils.Deserialize<OrderRoutePushRequest>(request.Params);
            return Json(new
            {
                logisticCompanyID = "DEPPON",
                logisticID = "",
                result = "true",
                resultCode = "1000",
                reason = "成功"
            });
        }
    }
}
