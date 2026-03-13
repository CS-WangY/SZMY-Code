using com.mymooo.workbench.core.Expressage.ShunFeng;
using com.mymooo.workbench.Filter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using mymooo.core.Utils.JsonConverter;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace com.mymooo.workbench.Controllers
{
	public class ShunFengExpressageController : Controller
    {
        private readonly ILogger<ShunFengExpressageController> _logger;
        private readonly ShunFengExpressageInfo _expressageInfo;
        private readonly ShunFengClient _shunFengClient;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ShunFengExpressageController(ILogger<ShunFengExpressageController> logger, IOptions<ShunFengExpressageInfo> expressageInfo, ShunFengClient shunFengClient, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _expressageInfo = expressageInfo.Value;
            _shunFengClient = shunFengClient;
            _webHostEnvironment = webHostEnvironment;
        }

        [AllowAnonymous]
        public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (request == null)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            var result = JsonSerializerOptionsUtils.Deserialize<ShunFengApiResponse>(_shunFengClient.InvokeWebService("EXP_RECE_CREATE_ORDER", JsonSerializerOptionsUtils.Serialize(request)));
            if (result.apiResultCode == "A1000")
            {
                var response = JsonSerializerOptionsUtils.Deserialize<CreateOrderResponse>(result.apiResultData);
                if (response.success)
                {

                }
            }
            return Content(result.apiResultData);
        }

        [AllowAnonymous]
        public IActionResult SearchOrder(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            SearchOrderRequest request = new SearchOrderRequest()
            {
                OrderId = orderId
            }; 
            var result = JsonSerializerOptionsUtils.Deserialize<ShunFengApiResponse>(_shunFengClient.InvokeWebService("EXP_RECE_SEARCH_ORDER_RESP", JsonSerializerOptionsUtils.Serialize(request)));
            if (result.apiResultCode == "A1000")
            {
                var response = JsonSerializerOptionsUtils.Deserialize<SearchOrderResponse>(result.apiResultData);
                if (response.success)
                {

                }
            }
            return Content(result.apiResultData);
        }

        [AllowAnonymous]
        public IActionResult SearchRoutes([FromBody] SearchRouteRequest request)
        {
            if (request == null)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            var result = JsonSerializerOptionsUtils.Deserialize<ShunFengApiResponse>(_shunFengClient.InvokeWebService("EXP_RECE_SEARCH_ROUTES", JsonSerializerOptionsUtils.Serialize(request)));
            if (result.apiResultCode == "A1000")
            {
                var response = JsonSerializerOptionsUtils.Deserialize<SearchRoutesResponse>(result.apiResultData);
                if (response.success)
                {

                }
            }
            return Content(result.apiResultData); 
        }

        [AllowAnonymous]
        public IActionResult CancelOrder(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            CancelOrderRequest request = new CancelOrderRequest()
            {
                OrderId = orderId
            };
            var result = JsonSerializerOptionsUtils.Deserialize<ShunFengApiResponse>(_shunFengClient.InvokeWebService("EXP_RECE_UPDATE_ORDER", JsonSerializerOptionsUtils.Serialize(request)));
            if (result.apiResultCode == "A1000")
            {
                var response = JsonSerializerOptionsUtils.Deserialize<CancelOrderResponse>(result.apiResultData);
                if (response.success)
                {

                }
            }
            return Content(result.apiResultData);
        }

        [AllowAnonymous]
        public IActionResult Print(string orderNo)
        {
            if (string.IsNullOrWhiteSpace(orderNo))
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            PrintRequest request = new PrintRequest()
            {
                documents = new PrintRequest.Document[]
                 {
                     new PrintRequest.Document(){ masterWaybillNo = orderNo },
                     new PrintRequest.Document(){ backWaybillNo = orderNo }
                 }
            };
            var result = JsonSerializerOptionsUtils.Deserialize<ShunFengApiResponse>(_shunFengClient.InvokeWebService("COM_RECE_CLOUD_PRINT_WAYBILLS", JsonSerializerOptionsUtils.Serialize(request)));
            if (result.apiResultCode == "A1000")
            {
                var response = JsonSerializerOptionsUtils.Deserialize<CancelOrderResponse>(result.apiResultData);
                if (response.success)
                {

                }
            }
            return Content(result.apiResultData);
        }

        [ServiceFilter(typeof(ShunFengSignAttribute))]
        public IActionResult PrintCallback([FromForm] PrintCallbackRequest request)
        {
            PrintFileCallbackRequest printFileCallback = JsonSerializerOptionsUtils.Deserialize<PrintFileCallbackRequest>(request.MsgData);
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "TempAttachment", "ShunFeng");
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            using (FileStream fs = new FileStream(Path.Combine(filePath, printFileCallback.fileName), FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.Write(Convert.FromBase64String(printFileCallback.content));
            }
            return Json(new
            {
                success = "true",
                msg = "成功"
            });
        }

        [AllowAnonymous]
        public IActionResult OrderStatusPush([FromBody] OrderStatusPushRequest request)
        {
            _logger.LogInformation("OrderStatusPush " + string.Join("&", Request.Headers.Select(p => $"{p.Key}={p.Value}")));
            _logger.LogInformation("OrderStatusPush " + Newtonsoft.Json.JsonConvert.SerializeObject(request));
            Task<string> body = null;
            Request.Body.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                body = reader.ReadToEndAsync();
            }
            _logger.LogInformation("OrderStatusPush " + body.Result);
            return Json(new
            {
                success = "true",
                code = "0",
                msg = ""
            });
        }

        [AllowAnonymous]
        public IActionResult OrderRoutePush([FromBody] OrderRoutePushRequest request)
        {
            _logger.LogInformation("OrderRoutePush " + string.Join("&", Request.Headers.Select(p => $"{p.Key}={p.Value}")));
            _logger.LogInformation("OrderRoutePush " + Newtonsoft.Json.JsonConvert.SerializeObject(request));
            Task<string> body = null;
            Request.Body.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                body = reader.ReadToEndAsync();
            }
            _logger.LogInformation("OrderRoutePush " + body.Result);
            return Json(new
            {
                return_code = "0000",
                return_msg = "成功"
            });
        }
    }
}
