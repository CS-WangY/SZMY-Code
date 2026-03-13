using com.mymooo.mall.business.Service.SalesService;
using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Model.Cache;
using com.mymooo.mall.core.Model.SalesOrder;
using com.mymooo.mall.core.SqlSugarCore.SalesBusiness;
using com.mymooo.mall.core.SqlSugarCore.SystemManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using mymooo.core;
using mymooo.core.Account;
using mymooo.weixinWork.SDK.Approval.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;

namespace com.mymooo.mall.Controllers
{
    /// <summary>
    /// 销售订单
    /// </summary>
    /// <param name="salesOrderService"></param>
    /// <param name="mallContext"></param>
    /// <param name="logger"></param>
    public class SalesOrderController(SalesOrderService salesOrderService, MallContext mallContext, ILogger<SalesOrderController> logger) : Controller
	{
		private readonly SalesOrderService _salesOrderService = salesOrderService;
		private readonly MallContext _mallContext = mallContext;
        private readonly ILogger<SalesOrderController> _logger = logger;
        /// <summary>
        /// 通过mq销售订单消息回调修改最近历史价
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IActionResult UpdateCache([FromBody] UpdateSalesHistoryCacheRequest request)
		{
			if (request == null)
			{
				return Json(new ResponseMessage<string>() { Code = ResponseCode.Success });
			}

			return Json(_salesOrderService.UpdateCache(request));
		}

		/// <summary>
		/// 现金订单申请款到发货审批回调
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public IActionResult ApplyPaymentToDeliveryCallback([FromBody] ApprovalMessageRequest request)
		{
			ArgumentNullException.ThrowIfNull(request);
			var applyData = _mallContext.GatewayRedisCache.HashGet(new ApplyPaymentToDeliveryRequest() { ApplyeventNo = request.ApplyeventNo });
			ArgumentNullException.ThrowIfNull(applyData);
			ResponseMessage<dynamic> responseMessage = new ResponseMessage<dynamic>();
			SalesOrder salesOrder = new()
			{
				SalesBillNo = applyData.SalesOrderNo ?? "",
				SettlementType = "A42FCC2E-0FD2-4580-9A87-CF285D71CAAE"
			};
			_mallContext.SqlSugar.Updateable(salesOrder).UpdateColumns(c => c.SettlementType).WhereColumns(it => new { it.SalesBillNo }).ExecuteCommand();

			return Json(responseMessage);
		}


        /// <summary>
        /// 获取订单的物流情况
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetExpressBySaleOrderId([FromBody] SalesOrderIdReq req)
        {
            try
            {
                var list = _mallContext.SqlSugar.Queryable<Express>().Where(r => r.PurchasedOrderId == req.saleOrderId).ToList();
                // 得到订单的物流号,可能有多个.
                foreach (var item in list)
                {
                    var expressData = _mallContext.RedisCache.HashGet(new ExpressStockCloudCache() { Mailno = item.Number }, p => p.Routedata);
                    if (!string.IsNullOrEmpty(expressData))  // 有缓存应用缓存的数据
                    {
#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
                        List<ExpressStockCloud> exprssFlow = JsonConvert.DeserializeObject<List<ExpressStockCloud>>(expressData);
#pragma warning restore CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。

                        List<Express100> flowList = [];
                        if (exprssFlow!= null)
                        {
                            foreach (var eStock in exprssFlow)
                            {
                                Express100 e100 = new Express100();
                                e100.Time = eStock.Time;
                                e100.FTime = eStock.Time;
                                e100.Context = eStock.Desc + "【" + eStock.Step  + "】";
                                flowList.Add(e100);
                            }

                            item.Flow = JsonConvert.SerializeObject(flowList);
                            item.Name = "[" + item.Name + "]"; //用于前端区分是从哪里取的数据
                        }
                    }
                }

                return Json(new { Success = true, Data = list, Message = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError("获取云仓存物流信息异常： " + ex.Message, ex); 
                return Json(new { Success = false, Message = "内部出现异常 :" + ex.Message });
            }
        }

    }
}
