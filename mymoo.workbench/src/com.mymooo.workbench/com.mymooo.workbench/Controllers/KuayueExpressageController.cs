using com.mymooo.workbench.core.Expressage.Kuayue;
using com.mymooo.workbench.Filter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using mymooo.core.Utils.JsonConverter;
using System;

namespace com.mymooo.workbench.Controllers
{
    /// <summary>
    /// 跨越
    /// </summary>
    public class KuayueExpressageController(ILogger<KuayueExpressageController> logger, KuayueClient kuayueClient, IOptions<KuayueExpressageInfo> expressageInfo) : Controller
    {
        private readonly ILogger<KuayueExpressageController> _logger = logger;
        private readonly KuayueClient _kuayueClient = kuayueClient;
        private readonly KuayueExpressageInfo _expressageInfo = expressageInfo.Value;

        public IActionResult Index()
        {
            return View();
        }

        [ServiceFilter(typeof(KuayueSignAttribute))]
        public IActionResult Callback([FromBody] KuayuePushMessage[] messages)
        {
            _logger.LogInformation("跨越回调开始");
            var timestamp = Request.Headers["X-KYE-TIMESTAMP"];
            var sign = Request.Headers["X-KYE-SIGN"];
            _logger.LogInformation(timestamp);
            _logger.LogInformation(sign);
            var array = Newtonsoft.Json.JsonConvert.SerializeObject(messages);
            _logger.LogInformation(array);
            _logger.LogInformation(KuayueClient.Md5(_expressageInfo.PlatformFlag + timestamp + array));
            return Json(new { code = 0, msg = "OK" });
        }

        [AllowAnonymous]
        public IActionResult Subscription([FromBody] string[] array)
        {
            return Content(_kuayueClient.InvokeWebService("open.api.openCommon.subscribeRoute", JsonSerializerOptionsUtils.Serialize(new { waybillNumber = array, OrderChannel = "AMAS7RVXC1BS35H3C9YC4OBMSYNHDQTJ" })));
        }

        [AllowAnonymous]
        public IActionResult PlanOrder([FromBody] PlanOrderResquest.Orderinfo[] resquest)
        {
            if (resquest == null || resquest.Length == 0)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }

            Array.ForEach(resquest, p => p.paymentCustomer = _expressageInfo.CustomerCode);

            PlanOrderResquest planOrder = new PlanOrderResquest()
            {
                customerCode = _expressageInfo.CustomerCode,
                platformFlag = _expressageInfo.PlatformFlag,
                orderInfos = resquest
            };
            return Content(_kuayueClient.InvokeWebService("open.api.openCommon.planOrder", JsonSerializerOptionsUtils.Serialize(planOrder)));
        }

        [AllowAnonymous]
        public IActionResult BatchOrder([FromBody] BatchOrderRequest.Orderinfo[] resquest)
        {
            if (resquest == null || resquest.Length == 0)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }

            Array.ForEach(resquest, p => p.paymentCustomer = _expressageInfo.CustomerCode);

            BatchOrderRequest planOrder = new BatchOrderRequest()
            {
                customerCode = _expressageInfo.CustomerCode,
                platformFlag = _expressageInfo.PlatformFlag,
                orderInfos = resquest
            };
            return Content(_kuayueClient.InvokeWebService("open.api.openCommon.batchOrder", JsonSerializerOptionsUtils.Serialize(planOrder)));
        }

        /// <summary>
        /// 修改跨越物流单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult UpdateYdByCondition([FromBody] UpdateYdByConditionRequest request)
        {
            if (request == null)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            request.customerCode = _expressageInfo.CustomerCode;

            return Content(_kuayueClient.InvokeWebService("open.api.openCommon.updateYdByCondition", JsonSerializerOptionsUtils.Serialize(request)));
        }

        [AllowAnonymous]
        public IActionResult CancelOrder([FromBody] CancelOrderRequest request)
        {
            if (request == null)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            request.customerCode = _expressageInfo.CustomerCode;

            return Content(_kuayueClient.InvokeWebService("open.api.openCommon.cancelOrder", JsonSerializerOptionsUtils.Serialize(request)));
        }

        [AllowAnonymous]
        public IActionResult Print([FromBody] PrintRequest request)
        {
            if (request == null)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            request.customerCode = _expressageInfo.CustomerCode;
            request.platformFlag = _expressageInfo.PlatformFlag;
            return Content(_kuayueClient.InvokeWebService("open.api.openCommon.print", JsonSerializerOptionsUtils.Serialize(request)));
        }

        [AllowAnonymous]
        public IActionResult QueryRoute([FromBody] string[] waybillNumbers)
        {
            if (waybillNumbers == null)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            return Content(_kuayueClient.InvokeWebService("open.api.openCommon.queryRoute", JsonSerializerOptionsUtils.Serialize(new { customerCode = _expressageInfo.CustomerCode, waybillNumbers })));
        }
    }
}
