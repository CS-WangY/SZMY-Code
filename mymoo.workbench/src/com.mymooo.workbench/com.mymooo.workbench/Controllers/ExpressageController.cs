using com.mymooo.workbench.core;
using com.mymooo.workbench.core.Account;
using com.mymooo.workbench.core.Expressage;
using com.mymooo.workbench.core.Expressage.DeBang;
using com.mymooo.workbench.core.Expressage.JiaYunMei;
using com.mymooo.workbench.core.Expressage.Kuayue;
using com.mymooo.workbench.core.Expressage.ShunFeng;
using com.mymooo.workbench.core.Expressage.SuTeng;
using com.mymooo.workbench.core.Minio;
using com.mymooo.workbench.core.Utils;
using com.mymooo.workbench.ef;
using com.mymooo.workbench.Filter;
using Confluent.Kafka;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using mymooo.core;
using mymooo.core.Attributes;
using mymooo.core.Utils.JsonConverter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace com.mymooo.workbench.Controllers
{
    /// <summary>
    /// 物流管理
    /// </summary>
    public class ExpressageController(ILogger<ExpressageController> logger,
            IOptions<ShunFengExpressageInfo> shunFengExpressageInfo,
            IOptions<DeBangExpressageInfo> deBangExpressageInfo,
            IOptions<SuTengExpressageInfo> suTengExpressageInfo,
            KuayueClient kuayueClient,
            IOptions<KuayueExpressageInfo> kuaYueExpressageInfo,
            SuTengClient suTengClient,
            DeBangClient deBangClient,
            WeixinDbContext weixinDbContext,
            IOptions<CloudStockConfig> cloudStockConfig,
            WorkbenchContext workbenchContext,
            CloudStockMinioService cloudStockMinioService,
            IOptions<JiaYunMeiExpressageInfo> jiaYunMeiExpressageInfo,
            JiaYunMeiClient jiaYunMeiClient,
            ShunFengClient shunFengClient, IWebHostEnvironment webHostEnvironment) : Controller
    {
        private readonly ILogger<ExpressageController> _logger = logger;
        private readonly ShunFengExpressageInfo _shunFengExpressageInfo = shunFengExpressageInfo.Value;
        private readonly DeBangExpressageInfo _deBangExpressageInfo = deBangExpressageInfo.Value;
        private readonly JiaYunMeiExpressageInfo _jiaYunMeiExpressageInfo = jiaYunMeiExpressageInfo.Value;
        private readonly SuTengExpressageInfo _suTengExpressageInfo = suTengExpressageInfo.Value;
        private readonly KuayueExpressageInfo _kuaYueExpressageInfo = kuaYueExpressageInfo.Value;
        private readonly KuayueClient _kuayueClient = kuayueClient;
        private readonly SuTengClient _suTengClient = suTengClient;
        private readonly JiaYunMeiClient _jiaYunMeiClient = jiaYunMeiClient;
        private readonly DeBangClient _debangClient = deBangClient;
        private readonly ShunFengClient _shunFengClient = shunFengClient;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        private readonly WeixinDbContext _weixinDbContext = weixinDbContext;
        private readonly CloudStockConfig _cloudStockConfig = cloudStockConfig.Value;
        private readonly WorkbenchContext _workbenchContext = workbenchContext;
        private readonly CloudStockMinioService _cloudStockMinioService = cloudStockMinioService;

        /// <summary>
        /// 下快递单
        /// </summary>
        /// <returns></returns>
        public IActionResult CreateOrder([FromBody] ExpressageCreateOrderRequest expressageCreateOrderRequest)
        {
            if (expressageCreateOrderRequest == null)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }

            switch (expressageCreateOrderRequest.ExpressageCreateOrderType)
            {
                case "shunfeng":
                    var shunFengRequest = JsonSerializerOptionsUtils.Deserialize<com.mymooo.workbench.core.Expressage.ShunFeng.CreateOrderRequest>(expressageCreateOrderRequest.ExpressageCreateOrderRequestJsonStr);
                    if (shunFengRequest.payMethod == 1)
                    {
                        shunFengRequest.monthlyCard = "7690222178";
                    }
                    var shunFengResult = JsonSerializerOptionsUtils.Deserialize<ShunFengApiResponse>(_shunFengClient.InvokeWebService("EXP_RECE_CREATE_ORDER", JsonSerializerOptionsUtils.Serialize(shunFengRequest)));
                    if (shunFengResult.apiResultCode == "A1000")
                    {
                        var shunFengResponse = JsonSerializerOptionsUtils.Deserialize<com.mymooo.workbench.core.Expressage.ShunFeng.CreateOrderResponse>(shunFengResult.apiResultData);
                        return Json(new { code = 1000, msg = @$"下单成功", success = true, data = shunFengResponse });
                    }
                    return Content(shunFengResult.apiResultData);
                case "kuayue":
                    BatchOrderRequest.Orderinfo[] kuaYueRequest = JsonSerializerOptionsUtils.Deserialize<List<BatchOrderRequest.Orderinfo>>(expressageCreateOrderRequest.ExpressageCreateOrderRequestJsonStr).ToArray();
                    Array.ForEach(kuaYueRequest, p => p.paymentCustomer = _kuaYueExpressageInfo.CustomerCode);

                    BatchOrderRequest planOrder = new BatchOrderRequest()
                    {
                        customerCode = _kuaYueExpressageInfo.CustomerCode,
                        platformFlag = _kuaYueExpressageInfo.PlatformFlag,
                        orderInfos = kuaYueRequest
                    };
                    return Content(_kuayueClient.InvokeWebService("open.api.openCommon.batchOrder", JsonSerializerOptionsUtils.Serialize(planOrder)));
                case "debang":
                    var deBangRequest = JsonSerializerOptionsUtils.Deserialize<com.mymooo.workbench.core.Expressage.DeBang.CreateOrderRequest>(expressageCreateOrderRequest.ExpressageCreateOrderRequestJsonStr);
                    deBangRequest.customerCode = _deBangExpressageInfo.CustomerCode;
                    deBangRequest.companyCode = _deBangExpressageInfo.CompanyCode;
                    deBangRequest.logisticID = _deBangExpressageInfo.Sign + deBangRequest.logisticID;

                    var deBangResult = _debangClient.InvokeWebService("sandbox-web/dop-standard-ewborder/createOrderNotify.action", JsonSerializerOptionsUtils.Serialize(deBangRequest));
                    //var deBangResponse = JsonSerializerOptionsUtils.Deserialize<com.mymooo.workbench.core.Expressage.DeBang.CreateOrderResponse>(deBangResult);
                    return Content(deBangResult);
                case "huolala":
                    break;
                case "xinfeng":
                    break;
                case "jiayunmei":
                    var jiayunmeiRequestData = JsonSerializerOptionsUtils.Deserialize<JiaYunMeiCreateOrderData>(expressageCreateOrderRequest.ExpressageCreateOrderRequestJsonStr);
                    if (jiayunmeiRequestData != null)
                    {
                        jiayunmeiRequestData.RegisterCode = _jiaYunMeiExpressageInfo.RegisterCode;
                        jiayunmeiRequestData.SendSiteCode = _jiaYunMeiExpressageInfo.SendSiteCode;
                        jiayunmeiRequestData.CustomerCode = _jiaYunMeiExpressageInfo.CustomerCode;
                        jiayunmeiRequestData.CustomerName = _jiaYunMeiExpressageInfo.CustomerName;
                        jiayunmeiRequestData.CustomerType = _jiaYunMeiExpressageInfo.CustomerType;
                    }
                    string jiayunmeiRequestDataStr = JsonSerializerOptionsUtils.Serialize(jiayunmeiRequestData);
                    var jiayunmeiResult = _jiaYunMeiClient.InvokeWebService("JiaYunMeiSendBillInterface/sendBill.do", jiayunmeiRequestDataStr);
                    return Content(jiayunmeiResult);
                case "suteng":
                    var suTengRequest = JsonSerializerOptionsUtils.Deserialize<com.mymooo.workbench.core.Expressage.SuTeng.CreateOrderRequest>(expressageCreateOrderRequest.ExpressageCreateOrderRequestJsonStr);
                    var suTengResult = _suTengClient.InvokeWebService("order/add", JsonSerializerOptionsUtils.Serialize(suTengRequest));
                    return Content(suTengResult);
                default:
                    break;
            }
            return Json(new { code = 1010, msg = @$"不支持的快递类型{expressageCreateOrderRequest.ExpressageCreateOrderType}" });
        }

        /// <summary>
        /// 根据单号查询快递路由（物流信息）
        /// </summary>
        /// <param name="expressageQueryRouteRequest"></param>
        /// <returns></returns>
        public IActionResult QueryRoute([FromBody] ExpressageQueryRouteRequest expressageQueryRouteRequest)
        {
            if (expressageQueryRouteRequest == null)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            switch (expressageQueryRouteRequest.ExpressageQueryRouteRequestType)
            {
                case "shunfeng":
                    var sunFengRequest = JsonSerializerOptionsUtils.Deserialize<SearchRouteRequest>(expressageQueryRouteRequest.ExpressageQueryRouteRequestJsonStr);
                    var shunFengResult = JsonSerializerOptionsUtils.Deserialize<ShunFengApiResponse>(_shunFengClient.InvokeWebService("EXP_RECE_SEARCH_ROUTES", JsonSerializerOptionsUtils.Serialize(sunFengRequest)));
                    if (shunFengResult.apiResultCode == "A1000")
                    {
                        var shunFengResponse = JsonSerializerOptionsUtils.Deserialize<SearchRoutesResponse>(shunFengResult.apiResultData);
                        if (shunFengResponse.success)
                        {

                        }
                    }
                    return Content(shunFengResult.apiResultData);
                case "kuayue":
                    string[] waybillNumbers = JsonSerializerOptionsUtils.Deserialize<List<string>>(expressageQueryRouteRequest.ExpressageQueryRouteRequestJsonStr).ToArray();
                    if (waybillNumbers == null)
                    {
                        return Json(new { code = 1010, msg = "传入值不正确" });
                    }
                    return Content(_kuayueClient.InvokeWebService("open.api.openCommon.queryRoute", JsonSerializerOptionsUtils.Serialize(new { customerCode = _kuaYueExpressageInfo.CustomerCode, waybillNumbers })));
                case "debang":
                    string mailNo = JsonSerializerOptionsUtils.Deserialize<string>(expressageQueryRouteRequest.ExpressageQueryRouteRequestJsonStr);
                    if (string.IsNullOrWhiteSpace(mailNo))
                    {
                        return Json(new { code = 1010, msg = "传入值不正确" });
                    }

                    var deBangResult = _debangClient.InvokeWebService("sandbox-web/standard-order/newTraceQuery.action", JsonSerializerOptionsUtils.Serialize(new { mailNo }));
                    var deBangResponse = JsonSerializerOptionsUtils.Deserialize<TraceQueryResponse>(deBangResult);
                    return Content(deBangResult);
                case "huolala":
                    break;
                case "xinfeng":
                    break;
                case "suteng":
                    string billCode = JsonSerializerOptionsUtils.Deserialize<string>(expressageQueryRouteRequest.ExpressageQueryRouteRequestJsonStr);
                    if (string.IsNullOrWhiteSpace(billCode))
                    {
                        return Json(new { code = 1010, msg = "传入值不正确" });
                    }

                    var result = _suTengClient.InvokeWebService("bill/track", JsonSerializerOptionsUtils.Serialize(new { billCode }));
                    return Content(result);
                case "jiayunmei":
                    string data = JsonSerializerOptionsUtils.Deserialize<string>(expressageQueryRouteRequest.ExpressageQueryRouteRequestJsonStr);
                    if (string.IsNullOrWhiteSpace(data))
                    {
                        return Json(new { code = 1010, msg = "传入值不正确" });
                    }

                    var jiayunmeiresult = _jiaYunMeiClient.InvokeWebServiceRouter("SuYueWXInterface/qryTrack.do", data);
                    // 物流信息写缓存
                    if (!string.IsNullOrWhiteSpace(jiayunmeiresult))
                    {
                        var jiayunmeiRouterData = JsonSerializerOptionsUtils.Deserialize<JiaYunMeiGetRouterResponse>(jiayunmeiresult);
                        if (jiayunmeiRouterData.Status == 1)
                        {
                            List<JiaYunMeiGetRouterData> jiaYunMeiGetRouterDatas = jiayunmeiRouterData.Data;

                            List<ExpressageRouteDataEntity> expressageRouteDataEntities = new List<ExpressageRouteDataEntity>();
                            int node = 1;
                            foreach (var item in jiaYunMeiGetRouterDatas)
                            {
                                ExpressageRouteDataEntity expressageRouteDataEntity = new ExpressageRouteDataEntity()
                                {
                                    Mailno = item.BillCode,
                                    Node = node,
                                    Step = item.ScanType,
                                    Desc = item.ScanCount,
                                    Time = item.ScanDate
                                };
                                expressageRouteDataEntities.Add(expressageRouteDataEntity);
                                node++;
                            }
                            // 路由信息存缓存
                            ExpressageRedisCache expressageRedisCache = new ExpressageRedisCache()
                            {
                                ExpressageNumber = jiaYunMeiGetRouterDatas.FirstOrDefault().BillCode,
                                Routedata = expressageRouteDataEntities
                            };
                            _workbenchContext.RedisCache.HashSet(expressageRedisCache, p => p.Routedata);
                        }
                    }
                    return Content(jiayunmeiresult);
                default:
                    break;
            }
            return Json(new { code = 1010, msg = @$"不支持的快递类型{expressageQueryRouteRequest.ExpressageQueryRouteRequestType}" });
        }

        /// <summary>
        /// 下单前获取预估运费
        /// </summary>
        /// <param name="expressageQueryFreightCharge"></param>
        /// <returns></returns>
        public IActionResult QueryFreightCharge([FromBody] ExpressageQueryFreightCharge expressageQueryFreightCharge)
        {
            if (expressageQueryFreightCharge == null)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            switch (expressageQueryFreightCharge.ExpressageQueryFreightChargeType)
            {
                case "shunfeng":
                    var shunFengResult = JsonSerializerOptionsUtils.Deserialize<ShunFengApiResponse>(_shunFengClient.InvokeWebService("EXP_RECE_QUERY_DELIVERTM", expressageQueryFreightCharge.ExpressageQueryFreightChargeJsonStr));
                    if (shunFengResult.apiResultCode == "A1000")
                    {
                        return Content(shunFengResult.apiResultData);
                    }
                    else
                    {
                        return Json(new ShunFengQueryFreightChargeResponse
                        {
                            Success = false,
                            ErrorCode = shunFengResult.apiResultCode,
                            ErrorMsg = shunFengResult.apiErrorMsg
                        });
                    }
                case "kuayue":
                    KuayueQueryFreightChargeRequest kuayueQueryFreightChargeRequest = JsonSerializerOptionsUtils.Deserialize<KuayueQueryFreightChargeRequest>(expressageQueryFreightCharge.ExpressageQueryFreightChargeJsonStr);
                    if (kuayueQueryFreightChargeRequest == null)
                    {
                        return Json(new { code = 1010, msg = "传入值不正确" });
                    }
                    kuayueQueryFreightChargeRequest.CustomerCode = _kuaYueExpressageInfo.CustomerCode;
                    kuayueQueryFreightChargeRequest.PlatformFlag = _kuaYueExpressageInfo.PlatformFlag;
                    kuayueQueryFreightChargeRequest.PickupCustomerCode = _kuaYueExpressageInfo.CustomerCode;
                    return Content(_kuayueClient.InvokeWebService("open.api.openCommon.queryFreightCharge", JsonSerializerOptionsUtils.Serialize(kuayueQueryFreightChargeRequest)));
                case "debang":
                    break;
                case "huolala":
                    break;
                case "xinfeng":
                    break;
                case "suteng":
                    break;
                default:
                    break;
            }
            return Json(new { code = 1010, msg = @$"不支持的快递类型{expressageQueryFreightCharge.ExpressageQueryFreightChargeType}" });
        }

        /// <summary>
        /// 修改物流单
        /// </summary>
        /// <param name="expressageUpdateOrderRequest"></param>
        /// <returns></returns>
        public IActionResult UpdateOrder([FromBody] ExpressageUpdateOrderRequest expressageUpdateOrderRequest)
        {
            if (expressageUpdateOrderRequest == null)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }

            switch (expressageUpdateOrderRequest.ExpressageUpdateOrderRequestType)
            {
                case "shunfeng":
                    break;
                case "kuayue":
                    UpdateYdByConditionRequest kuaYueRequest = JsonSerializerOptionsUtils.Deserialize<UpdateYdByConditionRequest>(expressageUpdateOrderRequest.ExpressageUpdateOrderRequestJsonStr);
                    if (kuaYueRequest == null)
                    {
                        return Json(new { code = 1010, msg = "传入值不正确" });
                    }
                    kuaYueRequest.customerCode = _kuaYueExpressageInfo.CustomerCode;

                    return Content(_kuayueClient.InvokeWebService("open.api.openCommon.updateYdByCondition", JsonSerializerOptionsUtils.Serialize(kuaYueRequest)));
                case "debang":
                    var deBangRequest = JsonSerializerOptionsUtils.Deserialize<com.mymooo.workbench.core.Expressage.DeBang.UpdateOrderRequest>(expressageUpdateOrderRequest.ExpressageUpdateOrderRequestJsonStr);
                    if (deBangRequest == null)
                    {
                        return Json(new { code = 1010, msg = "传入值不正确" });
                    }
                    deBangRequest.customerID = _deBangExpressageInfo.CustomerCode;
                    deBangRequest.orderSource = _deBangExpressageInfo.CompanyCode;
                    deBangRequest.logisticID = _deBangExpressageInfo.Sign + deBangRequest.logisticID;

                    var deBangResult = _debangClient.InvokeWebService("sandbox-web/standard-order/updateOrder.action", JsonSerializerOptionsUtils.Serialize(deBangRequest));
                    var deBangResponse = JsonSerializerOptionsUtils.Deserialize<com.mymooo.workbench.core.Expressage.DeBang.CreateOrderResponse>(deBangResult);
                    return Content(deBangResult);
                case "huolala":
                    break;
                case "xinfeng":
                    break;
                case "suteng":
                    var suTengRequest = JsonSerializerOptionsUtils.Deserialize<com.mymooo.workbench.core.Expressage.SuTeng.CreateOrderRequest>(expressageUpdateOrderRequest.ExpressageUpdateOrderRequestJsonStr);
                    if (suTengRequest == null)
                    {
                        return Json(new { code = 1010, msg = "传入值不正确" });
                    }

                    var suTengResult = _suTengClient.InvokeWebService("order/update", JsonSerializerOptionsUtils.Serialize(suTengRequest));
                    return Content(suTengResult);
                default:
                    break;
            }
            return Json(new { code = 1010, msg = @$"不支持的快递类型{expressageUpdateOrderRequest.ExpressageUpdateOrderRequestType}" });
        }

        /// <summary>
        /// 取消物流单
        /// </summary>
        /// <param name="expressageCancelOrderRequest"></param>
        /// <returns></returns>
        public IActionResult CancelOrder([FromBody] ExpressageCancelOrderRequest expressageCancelOrderRequest)
        {
            if (expressageCancelOrderRequest == null)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }

            switch (expressageCancelOrderRequest.ExpressageCancelOrderRequestType)
            {
                case "shunfeng":
                    string orderId = JsonSerializerOptionsUtils.Deserialize<string>(expressageCancelOrderRequest.ExpressageCancelOrderRequestJsonStr);
                    if (string.IsNullOrWhiteSpace(orderId))
                    {
                        return Json(new { code = 1010, msg = "传入值不正确" });
                    }
                    com.mymooo.workbench.core.Expressage.ShunFeng.CancelOrderRequest request = new com.mymooo.workbench.core.Expressage.ShunFeng.CancelOrderRequest()
                    {
                        OrderId = orderId
                    };
                    var shunFengResult = JsonSerializerOptionsUtils.Deserialize<ShunFengApiResponse>(_shunFengClient.InvokeWebService("EXP_RECE_UPDATE_ORDER", JsonSerializerOptionsUtils.Serialize(request)));
                    if (shunFengResult.apiResultCode == "A1000")
                    {
                        var shunFengResponse = JsonSerializerOptionsUtils.Deserialize<CancelOrderResponse>(shunFengResult.apiResultData);
                        if (shunFengResponse.success)
                        {

                        }
                    }
                    return Content(shunFengResult.apiResultData);
                case "kuayue":
                    var kuaYueRequest = JsonSerializerOptionsUtils.Deserialize<com.mymooo.workbench.core.Expressage.Kuayue.CancelOrderRequest>(expressageCancelOrderRequest.ExpressageCancelOrderRequestJsonStr);
                    if (kuaYueRequest == null)
                    {
                        return Json(new { code = 1010, msg = "传入值不正确" });
                    }
                    kuaYueRequest.customerCode = _kuaYueExpressageInfo.CustomerCode;

                    return Content(_kuayueClient.InvokeWebService("open.api.openCommon.cancelOrder", JsonSerializerOptionsUtils.Serialize(kuaYueRequest)));
                case "debang":
                    var deBangRequest = JsonSerializerOptionsUtils.Deserialize<com.mymooo.workbench.core.Expressage.DeBang.CancelOrderRequest>(expressageCancelOrderRequest.ExpressageCancelOrderRequestJsonStr);
                    if (deBangRequest == null)
                    {
                        return Json(new { code = 1010, msg = "传入值不正确" });
                    }
                    deBangRequest.logisticID = _deBangExpressageInfo.Sign + deBangRequest.logisticID;

                    var deBangResult = _debangClient.InvokeWebService("sandbox-web/standard-order/cancelOrder.action", JsonSerializerOptionsUtils.Serialize(deBangRequest));
                    return Content(deBangResult);
                case "huolala":
                    break;
                case "xinfeng":
                    break;
                case "suteng":
                    var orderCode = JsonSerializerOptionsUtils.Deserialize<string>(expressageCancelOrderRequest.ExpressageCancelOrderRequestJsonStr);
                    if (string.IsNullOrWhiteSpace(orderCode))
                    {
                        return Json(new { code = 1010, msg = "传入值不正确" });
                    }

                    var suTengResult = _suTengClient.InvokeWebService("order/cancel", JsonSerializerOptionsUtils.Serialize(new { orderCode }));
                    return Content(suTengResult);
                case "jiayunmei":
                    var billCode = JsonSerializerOptionsUtils.Deserialize<string>(expressageCancelOrderRequest.ExpressageCancelOrderRequestJsonStr);
                    if (string.IsNullOrWhiteSpace(billCode))
                    {
                        return Json(new { code = 1010, msg = "传入值不正确" });
                    }
                    var jiayunmeiResult = _jiaYunMeiClient.InvokeWebService("JiaYunMeiSendBillInterface/delBill.do", JsonSerializerOptionsUtils.Serialize(new { billCode }));
                    return Content(jiayunmeiResult);
                default:
                    break;
            }
            return Json(new { code = 1010, msg = @$"不支持的快递类型{expressageCancelOrderRequest.ExpressageCancelOrderRequestType}" });
        }


        #region 跨越物流接口

        /// <summary>
        /// 跨越打印物流单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IActionResult KuaYuePrint([FromBody] core.Expressage.Kuayue.PrintRequest request)
        {
            if (request == null)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            request.customerCode = _kuaYueExpressageInfo.CustomerCode;
            request.platformFlag = _kuaYueExpressageInfo.PlatformFlag;
            return Content(_kuayueClient.InvokeWebService("open.api.openCommon.print", JsonSerializerOptionsUtils.Serialize(request)));
        }

        /// <summary>
        /// 获取跨越物流单到本地
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IActionResult KuaYueGeneratePrintPdf([FromBody] core.Expressage.Kuayue.PrintRequest request)
        {
            if (request == null)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            request.customerCode = _kuaYueExpressageInfo.CustomerCode;
            request.platformFlag = _kuaYueExpressageInfo.PlatformFlag;
            return Content(_kuayueClient.InvokeWebService("open.api.common.print.generatePrintPdf", JsonSerializerOptionsUtils.Serialize(request)));
        }

        /// <summary>
        /// 跨越订阅路由及图片服务（下单时如果已经传subscriptionService订阅路由、pictureSubscription订阅图片参数则不需要再调用此接口）
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult KuayueSubscription([FromBody] KuayueSubscriptionRequest kuayueSubscriptionRequest)
        {
            return Content(_kuayueClient.InvokeWebService("open.api.openCommon.subscribeRoute", JsonSerializerOptionsUtils.Serialize(new { waybillNumber = kuayueSubscriptionRequest.WaybillNumbers, type = kuayueSubscriptionRequest.Types, OrderChannel = "AMAS7RVXC1BS35H3C9YC4OBMSYNHDQTJ" })));
        }

        /// <summary>
        /// 跨越接收回调路由并传到云仓储
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(KuayueSignAttribute))]
        [AllowAnonymous]
        public IActionResult CallbackRoute([FromBody] List<ExpressageRouteDataEntity> messages)
        {
            // 原始值传云仓储
            var array = Newtonsoft.Json.JsonConvert.SerializeObject(messages);
            //_workbenchContext.RedisCache.HashDelete(expressageRedisCache, expressageRedisCache.RedisCacheMainKey);
            // 路由信息存缓存
            List<ExpressageRouteDataEntity> oldRouteData = _workbenchContext.RedisCache.HashGet(new ExpressageRedisCache() { ExpressageNumber = messages.FirstOrDefault().Mailno }, p => p.Routedata);
            if (oldRouteData != null)
            {
                messages.AddRange(oldRouteData);
            }
            ExpressageRedisCache expressageRedisCache = new ExpressageRedisCache()
            {
                ExpressageNumber = messages.FirstOrDefault().Mailno,
                Routedata = messages
            };
            _workbenchContext.RedisCache.HashSet(expressageRedisCache, p => p.Routedata);
            // 传到云仓储保存
            var result = HttpUtils.CloudstockRequest(_cloudStockConfig.Url, "9D841AB1C71C32D2C86E8FE1E9490FD9", "/api/Expressage/GetKuayueExpressageRoute", array);
            var response = JsonSerializerOptionsUtils.Deserialize<ResponseMessage<string>>(result);

            return Json(new { code = 0, msg = "OK" });
        }

        /// <summary>
        /// 跨越接收回调签回单图片并传到云仓储
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(KuayueReceiptAttribute))]
        [AllowAnonymous]
        [IgnoreLog(true)]
        public IActionResult CallbackReceipt([FromBody] KuaYuePushReceiptPicture messages)
        {
            foreach (var message in messages.filePictureInfoRes)
            {
                // 图片文件上传到minio保存
                byte[] arr = Convert.FromBase64String(message.Picture);
                MemoryStream stream = new MemoryStream(arr);
                stream.Position = 0;

                ExpressageRedisCache expressageRedisCache = new ExpressageRedisCache()
                {
                    ExpressageNumber = messages.WaybillNumber
                };
                // 回单图片
                if (message.PictureType == "10")
                {
                    string fileUrl = _cloudStockMinioService.UploadFile(stream, "expressPicture/ReceiptPictUrl" + messages.WaybillNumber + ".jpg", HttpContext.Request.IsHttps);
                    messages.FileUrl = fileUrl;
                    expressageRedisCache.Receiptpicturl = fileUrl;
                    // 图片链接放缓存
                    _workbenchContext.RedisCache.HashSet(expressageRedisCache, p => p.Receiptpicturl);

                }
                else // 签单图片
                {
                    string fileUrl = _cloudStockMinioService.UploadFile(stream, "expressPicture/SignforPictUrl" + messages.WaybillNumber + ".jpg", HttpContext.Request.IsHttps);
                    messages.FileUrl = fileUrl;
                    expressageRedisCache.Signforpicturl = fileUrl;
                    // 图片链接放缓存
                    _workbenchContext.RedisCache.HashSet(expressageRedisCache, p => p.Signforpicturl);
                }

                stream.Close();
                stream.Dispose();
            }

            var array = Newtonsoft.Json.JsonConvert.SerializeObject(messages);
            // 传到云仓储保存
            var result = HttpUtils.CloudstockRequest(_cloudStockConfig.Url, "9D841AB1C71C32D2C86E8FE1E9490FD9", "/api/Expressage/GetKuayueExpressageReceipt", array);
            var response = JsonSerializerOptionsUtils.Deserialize<ResponseMessage<string>>(result);

            return Json(new { code = 0, msg = "OK" });
        }

        /// <summary>
        /// 跨越查询可选服务方式
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IActionResult KuayueGetServiceType([FromBody] KuayueGetServiceTypeEntity request)
        {
            if (request == null)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            request.PaymentCustomerCode = _kuaYueExpressageInfo.CustomerCode;
            request.ShipingCustomerCode = _kuaYueExpressageInfo.CustomerCode;
            return Content(_kuayueClient.InvokeWebService("open.api.openCommon.getServiceType", JsonSerializerOptionsUtils.Serialize(request)));
        }

        /// <summary>
        /// 跨越获取物流时效
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IActionResult KuayueQueryTimeliness([FromBody] KuayueQueryTimelinessEntity request)
        {
            if (request == null)
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            request.CustomerCode = _kuaYueExpressageInfo.CustomerCode;
            return Content(_kuayueClient.InvokeWebService("open.api.openCommon.queryTimeliness", JsonSerializerOptionsUtils.Serialize(request)));
        }

        #endregion

        #region 顺丰物流接口
        /// <summary>
        /// 顺丰打印物流单
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="expressTypeName"></param>
        /// <param name="remark"></param>
        /// <param name="type">1 面单 2 回单</param>
        /// <returns></returns>
        public IActionResult ShunfengPrint(string orderNo,string expressTypeName,string remark,int type = 1)
        {
            if (string.IsNullOrWhiteSpace(orderNo))
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            core.Expressage.ShunFeng.PrintRequest request = new core.Expressage.ShunFeng.PrintRequest();
            request.sync = true;
            if (type == 1)
            {
                request.documents = new core.Expressage.ShunFeng.PrintRequest.Document[]
                {
                    new core.Expressage.ShunFeng.PrintRequest.Document(){
                        masterWaybillNo = orderNo,
                        remark = remark,
                        customData = new core.Expressage.ShunFeng.PrintRequest.CustomData
                        {
                            waybillNo = orderNo,
                            remark = remark,
                            waybillNoLastFourNumber = orderNo.Substring(orderNo.Length -4),
                            expressTypeName = expressTypeName
                        }
                    }
                };
            }
            else
            {
                request.customTemplateCode = null;
                request.documents = new core.Expressage.ShunFeng.PrintRequest.Document[]
                {
                    new core.Expressage.ShunFeng.PrintRequest.Document(){
                        //masterWaybillNo = orderNo,
                        backWaybillNo = orderNo
                    }
                };
            }
            var result = JsonSerializerOptionsUtils.Deserialize<ShunFengApiResponse>(_shunFengClient.InvokeWebService("COM_RECE_CLOUD_PRINT_WAYBILLS", JsonSerializerOptionsUtils.Serialize(request)));
            if (result.apiResultCode == "A1000")
            {
                return Content(result.apiResultData);
            }
            else
            {
                return Json(new { code = 1010, msg = $@"获取快递面单失败，{result.apiErrorMsg}" });
            }

        }

        /// <summary>
        /// 顺丰路由回调
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult OrderRoutePush([FromBody] core.Expressage.ShunFeng.OrderRoutePushRequest request)
        {
            if (request != null && request.Body != null)
            {
                List<ExpressageRouteDataEntity> expressageRouteDataEntities = new List<ExpressageRouteDataEntity>();
                foreach (var item in request.Body.WaybillRoute)
                {
                    ExpressageRouteDataEntity expressageRouteDataEntity = new ExpressageRouteDataEntity()
                    {
                        Mailno = item.mailno,
                        Node = Convert.ToInt32(item.id),
                        Step = GetShunfengOpName(item.opCode),
                        Desc = item.remark,
                        Time = item.acceptTime
                    };
                    expressageRouteDataEntities.Add(expressageRouteDataEntity);
                }

                List<ExpressageRouteDataEntity> oldRouteData = _workbenchContext.RedisCache.HashGet(new ExpressageRedisCache() { ExpressageNumber = expressageRouteDataEntities.FirstOrDefault().Mailno }, p => p.Routedata);
                if (oldRouteData != null)
                {
                    expressageRouteDataEntities.AddRange(oldRouteData);
                }

                // 路由信息存缓存
                ExpressageRedisCache expressageRedisCache = new ExpressageRedisCache()
                {
                    ExpressageNumber = request.Body.WaybillRoute.FirstOrDefault().mailno,
                    Routedata = expressageRouteDataEntities
                };
                //_workbenchContext.RedisCache.HashDelete(expressageRedisCache, expressageRedisCache.RedisCacheMainKey);
                _workbenchContext.RedisCache.HashSet(expressageRedisCache, p => p.Routedata);
            }

            // 传到云仓储保存
            string jonStr = Newtonsoft.Json.JsonConvert.SerializeObject(request);
            var result = HttpUtils.CloudstockRequest(_cloudStockConfig.Url, "9D841AB1C71C32D2C86E8FE1E9490FD9", "/api/Expressage/GetShunFengExpressageRoute", jonStr);
            var response = JsonSerializerOptionsUtils.Deserialize<ResponseMessage<string>>(result);
            return Json(new
            {
                return_code = "0000",
                return_msg = "成功"
            });
        }

        /// <summary>
        /// 顺丰图片注册接口
        /// </summary>
        /// <returns></returns>
        public IActionResult ShunFengRegisterWayBillPicture([FromBody] RegisterWayBillPictureModel registerWayBillPictureModel)
        {
            if (registerWayBillPictureModel != null)
            {
                foreach (var item in registerWayBillPictureModel.ImgTypes)
                {
                    RegisterWayBillPictureRequest request = new RegisterWayBillPictureRequest()
                    {
                        ClientCode = _shunFengExpressageInfo.AppKey,
                        CustomerAcctCode = _shunFengExpressageInfo.MonthCard,
                        WaybillNo = registerWayBillPictureModel.WaybillNo,
                        Phone = registerWayBillPictureModel.Phone,
                        ImgType = item
                    };
                    var response = JsonSerializerOptionsUtils.Deserialize<RegisterWayBillPictureResponse>(_shunFengClient.InvokeWebService("EXP_RECE_REGISTER_WAYBILL_PICTURE", JsonSerializerOptionsUtils.Serialize(request)));
                }
            }
            return Json(new
            {
                IsSuccess = true
            });
        }

        /// <summary>
        /// 顺丰回单、签单图片回调
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult ShunFengReceiveWayBillPicture([FromBody] ShunFengReceivePicture shunFengReceivePicture)
        {
            if (shunFengReceivePicture != null)
            {
                // 图片解密
                string base64PictureStr = ShunFengPictureDecrypt.DecryptContent(shunFengReceivePicture.Content, _shunFengExpressageInfo.AppSecret);
                shunFengReceivePicture.Content = base64PictureStr;
                // 图片文件上传到minio保存
                byte[] arr = Convert.FromBase64String(shunFengReceivePicture.Content);
                MemoryStream stream = new MemoryStream(arr);
                stream.Position = 0;

                ExpressageRedisCache expressageRedisCache = new ExpressageRedisCache()
                {
                    ExpressageNumber = shunFengReceivePicture.WaybillNo
                };
                // 顺丰回调参数只有运单号和图片，无法确定回调的是回单还是签单，按先后顺序判断先回单后签单
                bool isReceivePictureExist = _workbenchContext.RedisCache.HashExists(expressageRedisCache, "expressPicture/ReceiptPictUrl" + shunFengReceivePicture.WaybillNo);

                // 回单图片
                if (!isReceivePictureExist)
                {
                    string fileUrl = _cloudStockMinioService.UploadFile(stream, "expressPicture/ReceiptPictUrl" + shunFengReceivePicture.WaybillNo + ".jpg", HttpContext.Request.IsHttps);
                    shunFengReceivePicture.FileUrl = fileUrl;
                    expressageRedisCache.Receiptpicturl = fileUrl;
                    // 图片链接放缓存
                    _workbenchContext.RedisCache.HashSet(expressageRedisCache, p => p.Receiptpicturl);

                }
                else // 签单图片
                {
                    string fileUrl = _cloudStockMinioService.UploadFile(stream, "expressPicture/SignforPictUrl" + shunFengReceivePicture.WaybillNo + ".jpg", HttpContext.Request.IsHttps);
                    shunFengReceivePicture.FileUrl = fileUrl;
                    expressageRedisCache.Signforpicturl = fileUrl;
                    // 图片链接放缓存
                    _workbenchContext.RedisCache.HashSet(expressageRedisCache, p => p.Signforpicturl);
                }


                string jonStr = Newtonsoft.Json.JsonConvert.SerializeObject(shunFengReceivePicture);
                // 传到云仓储保存
                var result = HttpUtils.CloudstockRequest(_cloudStockConfig.Url, "9D841AB1C71C32D2C86E8FE1E9490FD9", "/api/Expressage/GetShunFengReceiveWayBillPicture", jonStr);
                var response = JsonSerializerOptionsUtils.Deserialize<ResponseMessage<string>>(result);
            }
            return Json(new
            {
                return_code = "0000",
                return_msg = "成功"
            });
        }

        #endregion

        /// <summary>
        /// 查询德邦物流单信息（返回数据打印快递面单）
        /// </summary>
        /// <returns></returns>
        public IActionResult DeBangQueryOrder(string mailNo)
        {
            if (string.IsNullOrWhiteSpace(mailNo))
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }
            var logisticID = _deBangExpressageInfo.Sign + mailNo;

            var result = _debangClient.InvokeWebService("sandbox-web/standard-order/queryOrder.action", JsonSerializerOptionsUtils.Serialize(new { logisticCompanyID = "DEPPON", logisticID }));
            return Content(result);
        }

        #region 速腾快递接口

        /// <summary>
        /// 速腾打印物流单(按订单号)
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public IActionResult SuTengPrintOrder(string orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }

            var result = _suTengClient.InvokeWebService("order/printInfo", JsonSerializerOptionsUtils.Serialize(new { orderCode }));
            return Content(result);
        }

        /// <summary>
        /// 速腾打印物流单（按物流单号）
        /// </summary>
        /// <param name="billCode"></param>
        /// <returns></returns>
        public IActionResult SuTengPrintOrderByBillCode(string billCode)
        {
            if (string.IsNullOrWhiteSpace(billCode))
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }

            var result = _suTengClient.InvokeWebService("order/printInfo", JsonSerializerOptionsUtils.Serialize(new { billCode }));
            return Content(result);
        }

        // TODO 云仓储发货后在什么时间节点调这个接口待确定
        /// <summary>
        /// 速腾获取物流轨迹（路由信息）
        /// </summary>
        /// <param name="billCode"></param>
        /// <returns></returns>
        public IActionResult Track(string billCode)
        {
            if (string.IsNullOrWhiteSpace(billCode))
            {
                return Json(new { code = 1010, msg = "传入值不正确" });
            }

            var result = _suTengClient.InvokeWebService("bill/track", JsonSerializerOptionsUtils.Serialize(new { billCode }));
            return Content(result);
        }

        #endregion

        /// <summary>
        /// 加运美获取回单或签收单图片
        /// </summary>
        /// <returns></returns>
        public object JiaYunMeiGetBillPicture(JiaYunMeiGetBillPictureRequest jiaYunMeiGetBillPictureRequest)
        {
            // TODO 接口地址尚未提供
            if (jiaYunMeiGetBillPictureRequest != null)
            {
                // 获取图片链接
                var jiayunmeiresult = _jiaYunMeiClient.InvokeWebServicePic("", JsonSerializerOptionsUtils.Serialize(jiaYunMeiGetBillPictureRequest));
                // 获取图片
                JiaYunMeiGetBillPictureResponse jiaYunMeiGetBillPictureResponse = JsonSerializerOptionsUtils.Deserialize<JiaYunMeiGetBillPictureResponse>(jiayunmeiresult);
                if (jiaYunMeiGetBillPictureResponse.Success)
                {
                    string base64Express = null;
                    foreach (var item in jiaYunMeiGetBillPictureResponse.Data)
                    {
                        var pic = _jiaYunMeiClient.GetPicByUrl(item.Path);
                        Stream stream = pic.GetResponseStream();
                        ExpressageRedisCache expressageRedisCache = new ExpressageRedisCache()
                        {
                            ExpressageNumber = item.BillCode
                        };
                        // 回单图片
                        if (jiaYunMeiGetBillPictureRequest.BlType == "2")
                        {
                            string fileUrl = _cloudStockMinioService.UploadFile(stream, "expressPicture/ReceiptPictUrl" + item.BillCode + ".jpg", HttpContext.Request.IsHttps);
                            expressageRedisCache.Receiptpicturl = fileUrl;
                            // 图片链接放缓存
                            _workbenchContext.RedisCache.HashSet(expressageRedisCache, p => p.Receiptpicturl);

                        }
                        else // 签单图片
                        {
                            string fileUrl = _cloudStockMinioService.UploadFile(stream, "expressPicture/SignforPictUrl" + item.BillCode + ".jpg", HttpContext.Request.IsHttps);
                            expressageRedisCache.Signforpicturl = fileUrl;
                            // 图片链接放缓存
                            _workbenchContext.RedisCache.HashSet(expressageRedisCache, p => p.Signforpicturl);
                        }

                        byte[] bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, bytes.Length);
                        stream.Close();
                        base64Express = Convert.ToBase64String(bytes);
                    }
                    return Json(new
                    {
                        IsSuccess = true,
                        Data = base64Express
                    });
                }
                return Json(new
                {
                    IsSuccess = false,
                    Msg = jiaYunMeiGetBillPictureResponse.ErrorMsg
                });
            }
            return Json(new
            {
                IsSuccess = false,
                Msg = "参数不能为空"
            });
        }

        /// <summary>
        /// 顺丰物流根据操作码获取操作描述
        /// </summary>
        /// <param name="opCode"></param>
        /// <returns></returns>
        public string GetShunfengOpName(string opCode)
        {
            var dict = new Dictionary<string, string>()
            {
                { "50", "顺丰已收件" },
                { "51", "一票多件的子件" },
                { "54", "上门收件" },
                { "30", "快件中转" },
                { "31", "快件到达" },
                { "302", "车辆发车" },
                { "304", "离开经停点" },
                { "33", "派件异常原因" },
                { "34", "滞留件出仓" },
                { "36", "封车操作" },
                { "10", "办事处发车/中转发车/海关发车/机场发货" },
                { "105", "航空起飞" },
                { "106", "航空落地" },
                { "11", "办事处到车/中转到车/海关到车/机场提货" },
                { "122", "加时区域派件出仓" },
                { "123", "快件正送往顺丰店/站" },
                { "125", "快递员派件至丰巢" },
                { "126", "快递员取消派件将快件取出丰巢" },
                { "127", "寄件客户将快件放至丰巢" },
                { "128", "客户从丰巢取件成功" },
                { "129", "快递员从丰巢收件成功" },
                { "130", "快件到达顺丰店/站" },
                { "131", "快递员从丰巢收件失败" },
                { "135", "信息记录" },
                { "136", "落地配装车" },
                { "137", "落地配卸车" },
                { "138", "用户自助寄件" },
                { "14", "货件已放行" },
                { "140", "国际件特殊通知" },
                { "141", "预售件准备发运" },
                { "147", "整车在途" },
                { "15", "海关查验" },
                { "151", "分配师傅" },
                { "152", "师傅预约" },
                { "153", "师傅提货" },
                { "154", "师傅上门" },
                { "16", "正式报关待申报" },
                { "17", "海关待查" },
                { "18", "海关扣件" },
                { "186", "KC装车" },
                { "187", "KC卸车" },
                { "188", "KC揽收" },
                { "189", "KC快件交接" },
                { "190", "无人车发车" },
                { "201", "准备拣货" },
                { "202", "出库" },
                { "205", "仓库内操作-订单审核" },
                { "206", "仓库内操作-拣货" },
                { "208", "代理交接" },
                { "211", "星管家派件交接" },
                { "212", "星管家派送" },
                { "214", "星管家收件" },
                { "215", "星管家退件给客户" },
                { "405", "船舶离港" },
                { "406", "船舶到港" },
                { "407", "接驳点收件出仓" },
                { "41", "交收件联(二程接驳收件)" },
                { "43", "收件入仓" },
                { "46", "二程接驳收件" },
                { "47", "二程接驳派件" },
                { "570", "铁路发车" },
                { "571", "铁路到车" },
                { "604", "CFS清关" },
                { "605", "运力抵达口岸" },
                { "606", "清关完成" },
                { "607", "代理收件" },
                { "611", "理货异常" },
                { "612", "暂存口岸待申报" },
                { "613", "海关放行待补税" },
                { "614", "清关时效延长" },
                { "619", "检疫查验" },
                { "620", "检疫待查" },
                { "621", "检疫扣件" },
                { "623", "海关数据放行" },
                { "626", "到转第三方快递" },
                { "627", "寄转第三方快递" },
                { "630", "落地配派件出仓" },
                { "631", "新单退回" },
                { "632", "储物柜交接" },
                { "633", "港澳台二程接驳收件" },
                { "634", "港澳台二程接驳派件" },
                { "64", "晨配转出" },
                { "642", "门市/顺丰站快件上架" },
                { "643", "门市/顺丰站快件转移" },
                { "646", "包装完成" },
                { "647", "寄方准备快件中" },
                { "648", "快件已退回/转寄" },
                { "649", "代理转运" },
                { "65", "晨配转入" },
                { "651", "SLC已揽件" },
                { "655", "合作点收件" },
                { "656", "合作点交接给顺丰" },
                { "657", "合作点从顺丰交接" },
                { "658", "合作点已派件" },
                { "66", "中转批量滞留" },
                { "660", "合作点退件给客户" },
                { "664", "客户接触点交接" },
                { "676", "顺PHONE车" },
                { "677", "顺手派" },
                { "678", "快件到达驿站" },
                { "679", "驿站完成派件" },
                { "70", "由于XXX原因 派件不成功" },
                { "700", "拍照收件" },
                { "701", "一票多件拍照收件" },
                { "72", "标记异常" },
                { "75", "混建包复核" },
                { "77", "中转滞留" },
                { "830", "出库" },
                { "831", "入库" },
                { "833", "滞留件入仓" },
                { "84", "重量复核" },
                { "843", "集收入库" },
                { "844", "配送出库" },
                { "847", "二程接驳" },
                { "850", "集收" },
                { "851", "集收" },
                { "86", "晨配装车" },
                { "87", "晨配卸车" },
                { "870", "非正常派件" },
                { "88", "外资航空起飞" },
                { "880", "上门派件" },
                { "89", "外资航空落地" },
                { "900", "订舱路由" },
                { "921", "晨配在途" },
                { "930", "外配装车" },
                { "931", "外配卸车" },
                { "932", "外配交接" },
                { "933", "外配出仓" },
                { "934", "外配异常" },
                { "935", "外配签收" },
                { "950", "快速收件" },
                { "96", "整车发货" },
                { "97", "整车签收" },
                { "98", "代理路由信息" },
                { "99", "应客户要求,快件正在转寄中" },
                { "44", "我们正在为您的快件分配最合适的快递员，请您稍等" },
                { "204", "派件责任交接" },
                { "80", "已签收,感谢使用顺丰,期待再次为您服务" },
                { "8000", "在官网'运单资料&签收图',可查看签收人信" }
            };
            if (dict.Keys.Contains(opCode))
            {
                return dict[opCode];
            }
            else
            {
                return "";
            }
        }
    }
}
