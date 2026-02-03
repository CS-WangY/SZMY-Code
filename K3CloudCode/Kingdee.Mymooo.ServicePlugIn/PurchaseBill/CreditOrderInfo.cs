using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Util;
using Kingdee.BOS;
using Kingdee.Mymooo.App.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.SalesManagement.SalesOrderScheduled;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.BOS.Orm.DataEntity;
using System.Runtime.CompilerServices;
using Kingdee.K3.FIN.Core;

namespace Kingdee.Mymooo.ServicePlugIn.PurchaseBill
{
    [Description("采购订单--关闭,反关闭,审核,反审核"), HotUpdate]
    public class CreditOrderInfo : AbstractOperationServicePlugIn
    {
        private readonly List<ApigatewayTaskInfo> requests = new List<ApigatewayTaskInfo>();
        private readonly MymoooBusinessDataService service = new MymoooBusinessDataService();
        public readonly Dictionary<string, string> rabbitCode = new Dictionary<string, string>();
        public readonly Dictionary<string, string> billIdKey = new Dictionary<string, string>();

        public CreditOrderInfo()
        {
            rabbitCode.Add("Audit", "K3Cloud_AuditPurchaseOrder_");
            rabbitCode.Add("UnAudit", "K3Cloud_UnAuditPurchaseOrder_");
            rabbitCode.Add("BillClose", "K3Cloud_ClosePurchaseOrder_");
            rabbitCode.Add("BillUnClose", "K3Cloud_UnClosePurchaseOrder_");

            //rabbitCode.Add("TakeEffect", "platformAdmin_ChangeSalesOrder_");

            billIdKey.Add("BillClose", "Id");
            billIdKey.Add("BillUnClose", "Id");
            billIdKey.Add("Audit", "Id");
            //billIdKey.Add("TakeEffect", "PKIDX");
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            foreach (var dataEntity in e.DataEntitys)
            {
                var billNo = Convert.ToString(dataEntity["BillNo"]);

                List<POOrderEntryItem> list = new List<POOrderEntryItem>();
                foreach (var item in dataEntity["POOrderEntry"] as DynamicObjectCollection)
                {
                    var poentry = new POOrderEntryItem
                    {
                        Id = Convert.ToInt64(item["Id"]),
                        Seq = Convert.ToInt32(item["Seq"]),
                        MaterialId_Id = Convert.ToInt64(item["MaterialId_Id"])
                    };
                    list.Add(poentry);
                }

                var bill = new POOrder
                {
                    Id = Convert.ToInt64(dataEntity["Id"]),
                    FFormId = Convert.ToString(dataEntity["FFormId"]),
                    BillNo = billNo,
                    DocumentStatus = Convert.ToString(dataEntity["DocumentStatus"]),
                    POOrderEntry = list
                };
                var k3CloudRabbitMQMessage = new K3CloudRabbitMQMessage<POOrder, POOrderEntryItem>()
                {
                    Id = bill.Id,
                    FormId = bill.FFormId,
                    BillNo = bill.BillNo,
                    OperationNumber = this.FormOperation.Operation,
                    Head = bill,
                    Details = bill.POOrderEntry
                };

            ApigatewayTaskInfo taskInfo = new ApigatewayTaskInfo()
            {
                Url = $"RabbitMQ/SendMessage?rabbitCode={rabbitCode[this.FormOperation.Operation]}",
                Message = JsonConvertUtils.SerializeObject(k3CloudRabbitMQMessage)
            };

            taskInfo.Id = service.AddRabbitMqMeaage(this.Context, "Apigateway", billNo, JsonConvertUtils.SerializeObject(taskInfo)).Data;
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
