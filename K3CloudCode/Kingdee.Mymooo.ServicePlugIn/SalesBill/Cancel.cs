using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
 using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.K3.FIN.Core;
using Kingdee.Mymooo.App.Core;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.SalesManagement.SalesOrderScheduled;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.SalesBill
{
	[Description("销售订单-作废后删除待查款"), HotUpdate]
	public class Cancel : AbstractOperationServicePlugIn
	{
		private readonly List<ApigatewayTaskInfo> requests = new List<ApigatewayTaskInfo>();
		private readonly MymoooBusinessDataService service = new MymoooBusinessDataService();
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FIsPayOnline");
			e.FieldKeys.Add("FBillAllAmount");
			e.FieldKeys.Add("FSalesOrderDate");
			e.FieldKeys.Add("FCustId");
		}
		public override void EndOperationTransaction(EndOperationTransactionArgs e)
		{
			base.EndOperationTransaction(e);
			foreach (var item in e.DataEntitys)
			{
				var billno = item["BillNo"].ToString();
				string sSql = $@"DELETE PENY_t_MatchMoneyEntry WHERE FSALBILLNO='{billno}'";
				DBUtils.Execute(this.Context, sSql);

				var dataplan = item["SaleOrderPlan"] as DynamicObjectCollection;
				var cust = item["CustId"] as DynamicObject;
				ChangeOrderTaskRequest request = new ChangeOrderTaskRequest()
				{
					SalesOrderNo = billno,
					Operation = this.FormOperation.Operation,
					CustomerNumber = cust.GetValue<string>("Number", string.Empty),
					SalesOrderDate = Convert.ToDateTime(item["FSalesOrderDate"]),
					OriginalPaidAmount = Convert.ToDecimal((item["SaleOrderFinance"] as DynamicObjectCollection)[0]["BillAllAmount"])
				};
				ApigatewayTaskInfo taskInfo = new ApigatewayTaskInfo()
				{
					Url = "RabbitMQ/SendMessage?rabbitCode=platformAdmin_CancelSalesOrder_",
					Message = JsonConvertUtils.SerializeObject(request)
				};

				taskInfo.Id = service.AddRabbitMqMeaage(this.Context, "Apigateway", billno, JsonConvertUtils.SerializeObject(taskInfo)).Data;
				requests.Add(taskInfo);
				dataplan.Clear();
			}
			new BusinessDataWriter(Context).Save(e.DataEntitys);
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
