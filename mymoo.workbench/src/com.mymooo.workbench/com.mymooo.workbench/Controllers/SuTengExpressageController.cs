using com.mymooo.workbench.core.Expressage.SuTeng;
using com.mymooo.workbench.core.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using mymooo.core.Utils.JsonConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.mymooo.workbench.Controllers
{
    public class SuTengExpressageController : Controller
    {
        private readonly ILogger<SuTengExpressageController> _logger;
        private readonly SuTengExpressageInfo _expressageInfo;
        private readonly SuTengClient _suTengClient;

        public SuTengExpressageController(ILogger<SuTengExpressageController> logger, IOptions<SuTengExpressageInfo> expressageInfo, SuTengClient suTengClient)
        {
            _logger = logger;
            _expressageInfo = expressageInfo.Value;
            _suTengClient = suTengClient;
        }

        [AllowAnonymous]
        public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (request == null)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }

            var result = _suTengClient.InvokeWebService("order/add", JsonSerializerOptionsUtils.Serialize(request));
            return Content(result);
        }

        [AllowAnonymous]
        public IActionResult UpdateOrder([FromBody] CreateOrderRequest request)
        {
            if (request == null)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }

            var result = _suTengClient.InvokeWebService("order/update", JsonSerializerOptionsUtils.Serialize(request));
            return Content(result);
        }

        [AllowAnonymous]
        public IActionResult CancelOrder(string orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }

            var result = _suTengClient.InvokeWebService("order/cancel", JsonSerializerOptionsUtils.Serialize(new { orderCode }));
            return Content(result);
        }

        [AllowAnonymous]
        public IActionResult PrintOrder(string orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }

            var result = _suTengClient.InvokeWebService("order/printInfo", JsonSerializerOptionsUtils.Serialize(new { orderCode }));
            return Content(result);
        }

        [AllowAnonymous]
        public IActionResult Track(string billCode)
        {
            if (string.IsNullOrWhiteSpace(billCode))
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }

            var result = _suTengClient.InvokeWebService("bill/track", JsonSerializerOptionsUtils.Serialize(new { billCode }));
            return Content(result);
        }
    }
}
