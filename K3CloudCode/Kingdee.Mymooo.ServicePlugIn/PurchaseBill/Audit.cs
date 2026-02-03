using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.FIN.Core;
using Kingdee.Mymooo.App.Core;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.PurchaseBill
{
	[Description("采购订单审核验证插件")]
	[HotUpdate]
	public class Audit : AbstractOperationServicePlugIn
	{
		List<ApigatewayTaskInfo> requests = new List<ApigatewayTaskInfo>();
		private readonly MymoooBusinessDataService service = new MymoooBusinessDataService();

		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);

			e.FieldKeys.Add("FSupplierId");
			e.FieldKeys.Add("FDate");
			e.FieldKeys.Add("FPurchaserId");
			e.FieldKeys.Add("FPayConditionId");
			e.FieldKeys.Add("FSettleModeId");
			e.FieldKeys.Add("FBillAllAmount");
			e.FieldKeys.Add("FMaterialId");
			e.FieldKeys.Add("FUnitId");
			e.FieldKeys.Add("FQty");
			e.FieldKeys.Add("FPrice");
			e.FieldKeys.Add("FTaxPrice");
			e.FieldKeys.Add("FAllAmount");
			e.FieldKeys.Add("FEntryAmount");
			e.FieldKeys.Add("FDeliveryDate");
			e.FieldKeys.Add("FSMALLID");
			e.FieldKeys.Add("FPARENTSMALLID");
		}

		public override void EndOperationTransaction(EndOperationTransactionArgs e)
		{
			base.EndOperationTransaction(e);
			var sql = "update T_PUR_POOrder set FElectricSyncStatus='syncing' where FID = @FID";
			foreach (var item in e.DataEntitys)
			{
				var supplier = item["SupplierId"] as DynamicObject;
				if (supplier == null || supplier["FMQVirtualHosts"].IsNullOrEmptyOrWhiteSpace())
				{
					continue;
				}
				DBUtils.Execute(this.Context, sql, new SqlParam("@FID", KDDbType.Int64, item["Id"]));
				var billno = item["BillNo"].ToString();
				var fin = (item["POOrderFinance"] as DynamicObjectCollection).FirstOrDefault();
				var entrys = item["POOrderEntry"] as DynamicObjectCollection;
				SaleOrderRequest request = new SaleOrderRequest()
				{
					SalesOrderNo = billno,
					Address = "",
					Contact = "",
					CustomerCode = supplier["FCustomerCode"].ToString(),
					CustomerName = "",
					Date = DateTime.Now,
					Mobile = "",
					SalesOrderDetails = new List<SaleOrderRequest.SalesOrderDetail>(),
					TatolAmount = fin.GetValue<decimal>("BillAllAmount", 0)
					//OriginalPaidAmount = Convert.ToDecimal((item["SaleOrderFinance"] as DynamicObjectCollection)[0]["BillAllAmount"])
				};
				foreach (var entry in entrys)
				{
					var material = entry["MaterialId"] as DynamicObject;
					var small = entry["FSMALLID"] as DynamicObject;
					var parentSmall = entry["FPARENTSMALLID"] as DynamicObject;
					request.SalesOrderDetails.Add(new SaleOrderRequest.SalesOrderDetail()
					{
						ItemNo = material.GetValue<string>("Number", ""),
						ItemName = material["Name"].ToString(),
						ProductId = material.GetValue<long>("FProductId", 0),
						Price = entry.GetValue<decimal>("Price", 0),
						TaxPrice = entry.GetValue<decimal>("TaxPrice", 0),
						SubTotal = entry.GetValue<decimal>("Amount", 0),
						TaxSubTotal = entry.GetValue<decimal>("AllAmount", 0),
						Qty = entry.GetValue<decimal>("Qty", 0),
						DeliveryDate = entry.GetValue<DateTime>("DeliveryDate"),
						SalesDetailId = entry.GetValue<long>("Id", 0),
						ProductSmallClass = new SaleOrderRequest.Productsmallclass()
						{
							Id = small?.GetValue<long>("Id", 0) ?? 0,
							Number = small?.GetValue<string>("Number", ""),
							Name = small?["Name"].ToString(),
							ParentId = parentSmall?.GetValue<long>("Id", 0) ?? 0,
							ParentNumber = parentSmall?.GetValue<string>("Number", ""),
							ParentName = parentSmall?["Name"].ToString()
						},
						Remark = entry.GetValue<string>("Note", "")
					});
				}
				ApigatewayTaskInfo taskInfo = new ApigatewayTaskInfo()
				{
					Url = $"RabbitMQ/SendMessage?virtualHosts={supplier["FMQVirtualHosts"]}&rabbitCode=electricActuator_SalesOrder_",
					Message = JsonConvertUtils.SerializeObject(request)
				};

				taskInfo.Id = service.AddRabbitMqMeaage(this.Context, "Apigateway", billno, JsonConvertUtils.SerializeObject(taskInfo)).Data;
				requests.Add(taskInfo);
			}
		}

		public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
		{
			base.AfterExecuteOperationTransaction(e);
			Task.Factory.StartNew(() =>
			{
				foreach (var apigateway in requests)
				{
					var result = ApigatewayUtils.InvokePostRabbitService(apigateway.Url, apigateway.Message);
					var response = JsonConvertUtils.DeserializeObject<ResponseMessage<dynamic>>(result);
					if (response.IsSuccess)
					{
						service.UpdateRabbitMqMeaage(this.Context, apigateway.Id, result, true);
					}
				}
			});
		}
	}
}
