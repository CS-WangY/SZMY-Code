using Kingdee.BOS;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Business.PlugIn.BaseManagement;
using Kingdee.Mymooo.Business.PlugIn.ProductionManagement;
using Kingdee.Mymooo.Business.PlugIn.SalesManagement;
using Kingdee.Mymooo.Business.PlugIn.StockManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static Kingdee.Mymooo.Business.PlugIn.BaseManagement.DeleteSupplierContantBusiness;

namespace Kingdee.Mymooo.Business.PlugIn.RabbitMQExecuteService
{
    public class RabbitMQSyncService : IScheduleService
    {
        private static Dictionary<string, IMessageExecute> _matchServices = new Dictionary<string, IMessageExecute>(StringComparer.OrdinalIgnoreCase);

        static RabbitMQSyncService()
        {
            _matchServices["SalesOrder"] = new SalesOrderBusiness();
            _matchServices["/Dispatch/Workshop"] = new DepartmentBusiness();
            _matchServices["DeptAndUser"] = new DeptUserBusiness();
            _matchServices["SalesCust"] = new SalesCustBusiness();
            _matchServices["Addcustomer"] = new CustomerBusiness();
            _matchServices["EnableCustomer"] = new EnableCustomerBusiness();
            _matchServices["AddLinkMan"] = new LinkManBusiness();
            _matchServices["AddCustAddress"] = new CustAddressBusiness();
            _matchServices["DeleteLinkMan"] = new DeleteLinkManBusiness();
            _matchServices["DeleteCustAddress"] = new DeleteCustAddressBusiness();
            _matchServices["AddCustPaymothod"] = new CustPaymothod();
            _matchServices["AddInvoice"] = new CustInvoiceBusiness();
            _matchServices["AddSupplier"] = new SupplierBusiness();
            _matchServices["AddSupplierBank"] = new SupplierBankBusiness();
            _matchServices["AddSupplierContant"] = new SupplierContantBusiness();
            _matchServices["SupplierAllotOrg"] = new SupplierAllotOrgBusiness();//供应商分配组织
            _matchServices["DeleteSupplierContant"] = new DeleteSupplierContantBusiness();
            _matchServices["EnableSupplier"] = new EnableSupplierBusiness();
            _matchServices["CloudStockUpdateTempDeliveryArea"] = new PutToTempDeliveryAreaBusiness(); // 云仓储出库
            _matchServices["CloudStockUpdateTempStockArea"] = new PutToTempStockAreaBusiness();// 云仓储入库
            _matchServices["CloudStockDeleteTempDeliveryArea"] = new DeleteTempDeliveryAreaBusiness();// 云仓储删除出库单
            _matchServices["CloudStockExamineInboundBill"] = new ExamineInboundBillBusiness();// 修改云存储调拨入库审核状态
            _matchServices["CloudStockRevocationStockOut"] = new CloudStockRevocationStockOutBusiness();// 云仓储撤回出库单接口
            _matchServices["CloudStockRevocationStockIn"] = new CloudStockRevocationStockInBusiness();// 云仓储撤回入库单接口
            _matchServices["Apigateway"] = new ApigatewayTaskBusiness();

        }
        public void Run(Context ctx, Schedule schedule)
        {
            var context = LoginServiceUtils.BackgroundLogin(ctx);
            var sql = "select * from RabbitMQScheduledMessage where FIsExecute = '0' and FCreateDate < @FCreateDate order by FId";
            var datas = DBServiceHelper.ExecuteDynamicObject(context, sql, paramList: new SqlParam("@FCreateDate", KDDbType.DateTime, DateTime.Now.AddMinutes(-1)));
            ResponseMessage<dynamic> response;
            var updateSql = "update RabbitMQScheduledMessage set FIsExecute = '1',FExecuteDate=@ExecuteDate,FIsSucceed=@IsSucceed,FCompleteDate=@CompleteDate,FResult=@Result,FStackTrace=@StackTrace where FId = @Id";
            foreach (var data in datas)
            {
                List<SqlParam> paramList = new List<SqlParam>()
                {
                    new SqlParam("@ExecuteDate", KDDbType.DateTime, DateTime.Now),
                    new SqlParam("@Id", KDDbType.Int64, data["FId"])
                };

                try
                {
                    var action = data["FAction"].ToString();
                    if (_matchServices.ContainsKey(action))
                    {
                        response = _matchServices[action].Execute(context, data["FMessage"].ToString());
                    }
                    else
                    {
                        response = new ResponseMessage<dynamic>
                        {
                            Code = ResponseCode.NoExistsData,
                            Message = "不存在消息执行类！"
                        };
                    }
                    paramList.Add(new SqlParam("@IsSucceed", KDDbType.String, response.IsSuccess ? "1" : "0"));
                    paramList.Add(new SqlParam("@CompleteDate", KDDbType.DateTime, DateTime.Now));
                    paramList.Add(new SqlParam("@StackTrace", KDDbType.String, string.Empty));
                }
                catch (Exception ex)
                {
                    response = new ResponseMessage<dynamic>
                    {
                        Code = ResponseCode.Exception,
                        Message = ex.Message
                    };
                    paramList.Add(new SqlParam("@IsSucceed", KDDbType.String, "0"));
                    paramList.Add(new SqlParam("@CompleteDate", KDDbType.DateTime, DateTime.Now));
                    paramList.Add(new SqlParam("@StackTrace", KDDbType.String, ex.StackTrace));
                }
                paramList.Add(new SqlParam("@Result", KDDbType.String, JsonConvert.SerializeObject(response)));
                DBServiceHelper.Execute(context, updateSql, paramList);
            }
        }
    }
}
