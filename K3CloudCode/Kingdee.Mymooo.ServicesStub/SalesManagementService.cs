using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.Exceptions;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.App.Core;
using Kingdee.Mymooo.Business.PlugIn.SalesManagement;
using Kingdee.Mymooo.Business.PlugIn.StockManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.SalesManagement.SalesOrderScheduled;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Kingdee.Mymooo.ServiceHelper.SalesManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.WebApi.ServicesStub
{
	public class SalesManagementService : KDBaseService
	{
		public SalesManagementService(KDServiceContext context) : base(context)
		{
		}

		public string SyncCreateSalesOrderReceivebill()
		{
			string data = string.Empty;
			ResponseMessage<SalesOrderBillRequest> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}

				if (string.IsNullOrWhiteSpace(data))
				{
					response = new ResponseMessage<SalesOrderBillRequest>()
					{
						Code = ResponseCode.Empty,
						Message = "数据不能为空！"
					};
				}
				else
				{
					var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
					SalesOrderBusiness salesOrder = new SalesOrderBusiness();
					response = salesOrder.SyncCreateSalesOrderReceivebill(ctx, data);
				}
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<SalesOrderBillRequest>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvertUtils.SerializeObject(response);
		}

		/// <summary>
		/// 跳过所有验证,反审核销售订单
		/// </summary>
		/// <returns></returns>
		public string UnAduitSalesOrder()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ids = JsonConvertUtils.DeserializeObject<object[]>(data);
				var operateOption = OperateOption.Create();
				operateOption.SetIgnoreWarning(true);
				operateOption.SetVariableValue("RemoveValidators", true);
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				FormMetadata paybillMeta = (FormMetadata)MetaDataServiceHelper.Load(ctx, "SAL_SaleOrder");
				var oper = BusinessDataServiceHelper.UnAudit(ctx, paybillMeta.BusinessInfo, ids, operateOption);

			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>();
				response.Message = ex.Message;
				response.Code = ResponseCode.Exception;
			}
			return JsonConvertUtils.SerializeObject(response);
		}

		/// <summary>
		/// 修改发货通知单物流等信息
		/// </summary>
		/// <returns></returns>
		public string ModifyDeliveryExpressage()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				CloudStockPackagedBusiness business = new CloudStockPackagedBusiness();
				response = business.ModifyDeliveryExpressage(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvertUtils.SerializeObject(response);
		}

		/// <summary>
		/// 全国一部,华南二部调拨出库
		/// </summary>
		/// <returns></returns>
		public string DeliveryTransferOut()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				CloudStockPackagedBusiness business = new CloudStockPackagedBusiness();
				response = business.DeliveryTransferOut(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvertUtils.SerializeObject(response);
		}

		/// <summary>
		/// 全国一部,华南二部调拨入库
		/// </summary>
		/// <returns></returns>
		public string DeliveryTransferIn()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				CloudStockPackagedBusiness business = new CloudStockPackagedBusiness();
				response = business.DeliveryTransferIn(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvertUtils.SerializeObject(response);
		}

		/// <summary>
		/// 下推生成销售出库单
		/// </summary>
		/// <returns></returns>
		public string GenerateOutStock()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				CloudStockPackagedBusiness business = new CloudStockPackagedBusiness();
				response = business.GenerateOutStock(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvertUtils.SerializeObject(response);
		}

		/// <summary>
		/// 发货通知单变更数量
		/// </summary>
		/// <returns></returns>
		public string ModifyDeliveryQuantity()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				CloudStockPackagedBusiness business = new CloudStockPackagedBusiness();
				response = business.ModifyDeliveryQuantity(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvertUtils.SerializeObject(response);
		}

		/// <summary>
		/// 事业部红字调入
		/// </summary>
		/// <returns></returns>
		public string RedDeliveryTransferIn()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				CloudStockPackagedBusiness business = new CloudStockPackagedBusiness();
				response = business.RedDeliveryTransferIn(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvertUtils.SerializeObject(response);
		}

		public string SyncSalesOrderMaterial()
		{
			string data = string.Empty;
			ResponseMessage<SalesOrderBillRequest> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}

				if (string.IsNullOrWhiteSpace(data))
				{
					response = new ResponseMessage<SalesOrderBillRequest>()
					{
						Code = ResponseCode.Empty,
						Message = "数据不能为空！"
					};
				}
				else
				{
					var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
					SalesOrderBusiness salesOrder = new SalesOrderBusiness();
					response = salesOrder.SyncSalesOrderMaterial(ctx, data);
				}
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<SalesOrderBillRequest>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvertUtils.SerializeObject(response);
		}

		public string SyncSalesOrder()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}

				if (string.IsNullOrWhiteSpace(data))
				{
					response = new ResponseMessage<dynamic>()
					{
						Code = ResponseCode.Empty,
						Message = "数据不能为空！"
					};
				}
				else
				{
					var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
					SalesOrderBusiness salesOrder = new SalesOrderBusiness();
					response = salesOrder.CreateSalesOrder(ctx, data);
				}
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvertUtils.SerializeObject(response);
		}

		public string CreateSalesOrder()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}

				if (string.IsNullOrWhiteSpace(data))
				{
					response = new ResponseMessage<dynamic>()
					{
						Code = ResponseCode.Empty,
						Message = "数据不能为空！"
					};
				}
				else
				{
					var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
					SalesOrderBusiness salesOrder = new SalesOrderBusiness();
					response = salesOrder.Execute(ctx, data);
				}
			}
			catch (OrmException ormex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Success,
					Message = ormex.Message,
				};
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 创建销售单
		/// </summary>
		/// <returns></returns>
		public string SalesOrder()
		{
			string data = string.Empty;
			ResponseMessage<long> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}

				if (string.IsNullOrWhiteSpace(data))
				{
					response = new ResponseMessage<long>()
					{
						Code = ResponseCode.Empty,
						Message = "数据不能为空！"
					};
				}
				else
				{
					var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
					SalesOrderBillRequest request = JsonConvertUtils.DeserializeObject<SalesOrderBillRequest>(data);
					response = MymoooBusinessDataServiceHelper.AddRabbitMqMeaage(ctx, "SalesOrder", request.SalesOrderNo, data);
				}
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<long>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		public string CreateRequirementOrder()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}

				if (string.IsNullOrWhiteSpace(data))
				{
					response = new ResponseMessage<dynamic>()
					{
						Code = ResponseCode.Empty,
						Message = "数据不能为空！"
					};
				}
				else
				{
					var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
					OrgRequirementOrderBusiness salesOrder = new OrgRequirementOrderBusiness();
					response = salesOrder.Execute(ctx, data);
				}
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 整单销售订单作废(非标使用)
		/// </summary>
		/// <returns></returns>
		public string CloseSalesOrder()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				response = salesOrderBusiness.CloseSalesOrder(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 销售订单明细项作废(非标使用)
		/// </summary>
		/// <returns></returns>
		public string CloseSalesOrderDetail()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				response = salesOrderBusiness.CloseSalesOrderDetail(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 取消销售订单(作废订单)
		/// </summary>
		/// <returns></returns>
		public string CancelSalesOrder()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				response = salesOrderBusiness.CancelSalesOrder(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 查询是否可以变更销售单接口
		/// </summary>
		/// <returns></returns>
		public string IsCanUpdate()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				response = salesOrderBusiness.IsCanUpdateSalesOrder(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 变更销售单
		/// </summary>
		/// <returns></returns>
		public string ChangeSalesOrderBill()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				Logger.Info("销售订单", "变更销售单：" + data);
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				ChangeSalesOrderBusiness salesOrderBusiness = new ChangeSalesOrderBusiness();
				response = salesOrderBusiness.Execute(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 计算信用额度
		/// </summary>
		/// <returns></returns>
		public string CalcAmountOccupied()
		{
			string data = string.Empty;
			ResponseMessage<decimal> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				response = salesOrderBusiness.CalcAmountOccupied(ctx, data);
			}
			catch (Exception ex)
			{

				response = new ResponseMessage<decimal>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 查询逾期信息
		/// </summary>
		/// <returns></returns>
		public string QueryCustomerExpiry()
		{
			string data = string.Empty;
			ResponseMessage<QueryExpiryResponse> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				response = salesOrderBusiness.QueryCustomerExpiry(ctx, data);
			}
			catch (Exception ex)
			{

				response = new ResponseMessage<QueryExpiryResponse>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}
		/// <summary>
		/// 查询最后逾期信息
		/// </summary>
		/// <returns></returns>
		public string QueryCustomerExpiryTopList()
		{
			string data = string.Empty;
			ResponseMessage<QueryExpiryResponseList> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				RequestExpiry request = JsonConvertUtils.DeserializeObject<RequestExpiry>(data);
				response = SalesOrderServiceHelper.QueryCustomerExpiryList(ctx, request);
				//SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				//response = salesOrderBusiness.QueryCustomerExpiryList(ctx, data);
			}
			catch (Exception ex)
			{

				response = new ResponseMessage<QueryExpiryResponseList>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 获取销售订单成本价列表
		/// </summary>
		/// <returns></returns>
		public string GetSalesOrderCostPriceDet()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				response = salesOrderBusiness.GetSalesOrderCostPriceDet(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 更新销售订单成本价
		/// </summary>
		/// <returns></returns>
		public string UpdateSalesOrderCostPriceDet()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				response = salesOrderBusiness.UpdateSalesOrderCostPriceDet(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 填充成本
		/// </summary>
		/// <returns></returns>
		public string WritePoVatTtl()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				response = salesOrderBusiness.WritePoVatTtl(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 批量更新订单毛利差异的成本
		/// </summary>
		/// <returns></returns>
		public string UpdateBalance()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				response = salesOrderBusiness.UpdateBalance(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 零成本更新
		/// </summary>
		/// <returns></returns>
		public string UpdateZeroCost()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				response = salesOrderBusiness.UpdateZeroCost(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 查询订单毛利异常数据，调整单价<0或调整单价<系统计算单价
		/// </summary>
		/// <returns></returns>
		public string GetOrderGrossProfitExceptList()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				response = salesOrderBusiness.GetOrderGrossProfitExceptList(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 获取销售订单预估毛利汇总
		/// </summary>
		/// <returns></returns>
		public string GetOrderGrossProfitList()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				response = salesOrderBusiness.GetOrderGrossProfitList(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 查询订单毛利成本分析数据
		/// </summary>
		/// <returns></returns>
		public string GetOrderGrossProfitAnalysisList()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				response = salesOrderBusiness.GetOrderGrossProfitAnalysisList(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 获取销售订单毛利预估明细
		/// </summary>
		/// <returns></returns>
		public string GetOrderGrossProfitDetailList()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				response = salesOrderBusiness.GetOrderGrossProfitDetailList(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 获取产品毛利汇总数据
		/// </summary>
		/// <returns></returns>
		public string GetProductGrossList()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				response = salesOrderBusiness.GetProductGrossList(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 更新发货通知物流信息
		/// </summary>
		/// <returns></returns>
		public string UpDnTracking()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				response = salesOrderBusiness.UpDnTracking(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 根据销售订单号获取物流信息
		/// </summary>
		/// <returns></returns>
		public string GetSoLogisticsInfo()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				response = salesOrderBusiness.GetSoLogisticsInfo(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 更新物流方式变更申请审批信息
		/// </summary>
		/// <returns></returns>
		public string UpDnLogisticsChangesInfo()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				response = salesOrderBusiness.UpDnLogisticsChangesInfo(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}
		/// <summary>
		/// 非标订单创建计划订单
		/// </summary>
		/// <returns></returns>
		public string CreateSalesOrderTPlan()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				var request = JsonConvertUtils.DeserializeObject<List<ENGBomInfo>>(data);
				response = SalesOrderServiceHelper.SalBillPushPlanBill(ctx, request);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvertUtils.SerializeObject(response);
		}
		/// <summary>
		/// 非标订单创建BOM
		/// </summary>
		/// <returns></returns>
		public string FBSalBillCreateBom()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				var request = JsonConvertUtils.DeserializeObject<ChangeOrderTaskRequest>(data);
				response = SalesOrderServiceHelper.FBSalBillCreateBom(ctx, request);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvertUtils.SerializeObject(response);
		}
		/// <summary>
		/// 销售订单关闭或行关闭
		/// </summary>
		/// <returns></returns>
		public string ClosedSalesOrderAction()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				ApprovalMessageRequest request = JsonConvertUtils.DeserializeObject<ApprovalMessageRequest>(data);
				response = SalesOrderServiceHelper.ClosedSalesOrderAction(ctx, request);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}
		/// <summary>
		/// 销售订单构建预测单关系转移预留
		/// </summary>
		/// <returns></returns>
		public string SalesOrder2Forecast()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}

				if (string.IsNullOrWhiteSpace(data))
				{
					response = new ResponseMessage<dynamic>()
					{
						Code = ResponseCode.Empty,
						Message = "数据不能为空！"
					};
				}
				else
				{
					var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
					SalesOrderBusiness salesOrder = new SalesOrderBusiness();
					response = salesOrder.SalesOrder2Forecast(ctx, data);
				}
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvertUtils.SerializeObject(response);
		}

		/// <summary>
		/// MES下推生成销售出库单
		/// </summary>
		/// <returns></returns>
		public string MesGenerateOutStock()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrder = new SalesOrderBusiness();
				response = salesOrder.MesGenerateOutStock(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvertUtils.SerializeObject(response);
		}

		/// <summary>
		/// MES下推生成销售退货单
		/// </summary>
		/// <returns></returns>
		public string MesGenerateReturnStock()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrder = new SalesOrderBusiness();
				response = salesOrder.MesGenerateReturnStock(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvertUtils.SerializeObject(response);
		}

		/// <summary>
		/// 更新加急发货申请审批信息
		/// </summary>
		/// <returns></returns>
		public string UpDnUrgentShipmentState()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
				response = salesOrderBusiness.UpDnUrgentShipmentState(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

        public string ChangingOrRefunding()
        {
            string data = string.Empty;
            ResponseMessage<AfterSalesResponse> response;
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                SalesOrderBusiness salesOrderBusiness = new SalesOrderBusiness();
                response = salesOrderBusiness.ChangingOrRefunding(ctx, data);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<AfterSalesResponse>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvert.SerializeObject(response);
        }
    }
}
