using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.App.Core.ProductionManagement;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.ProductionManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.App.Core;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;

namespace Kingdee.Mymooo.ServicePlugIn.PRD_MO
{
    [Description("生产订单人工结案、强制结案"), HotUpdate]
    public class Close : AbstractOperationServicePlugIn
    {
        List<ApigatewayTaskInfo> requests = new List<ApigatewayTaskInfo>();
        private readonly MymoooBusinessDataService service = new MymoooBusinessDataService();
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FCloseType");
            e.FieldKeys.Add("FStatus");
            e.FieldKeys.Add("FCLOSEREASON");
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            foreach (var item in e.DataEntitys)
            {
                this.CreateMakeRequest(item);
            }
        }

        private void CreateMakeRequest(DynamicObject data)
        {
            MesProductionCloseRequest request = new MesProductionCloseRequest();
            request.MakeNo = data["BillNo"].ToString();
            var org = data["PrdOrgId"] as DynamicObject;
            request.OrgNo = Convert.ToString(org["Number"]);
            request.OrgName = Convert.ToString(org["Name"]);
            request.Entity = SendMakeColseForBill(this.Context, Convert.ToInt64(data["Id"]));
            request.IsClose = true;
            ApigatewayTaskInfo taskInfo = new ApigatewayTaskInfo()
            {
                Url = "RabbitMQ/SendMessage?rabbitCode=WorkOrder_Close_send_",
                Message = JsonConvertUtils.SerializeObject(request)
            };

            taskInfo.Id = service.AddRabbitMqMeaage(this.Context, "Apigateway", request.MakeNo, JsonConvertUtils.SerializeObject(taskInfo)).Data;
            requests.Add(taskInfo);
        }

        public List<MesProductionCloseEntity> SendMakeColseForBill(Context ctx, long id)
        {
            var sql = $@"/*dialect*/ select e.FENTRYID,mo.FBILLNO makeNo,e.FSEQ makeSeq,a.FStatus status,isnull(d.FNUMBER,'') worksNo,m.FNUMBER dwgNo,ml.FNAME dwgName,e.FQTY qty,a.FSTOCKINQUAAUXQTY inQty,
                    isnull(ul.FNAME,'') closeUserName,FCloseType closeType
                    from T_PRD_MO mo
	                    inner join T_PRD_MOENTRY e on mo.FID = e.FID
	                    LEFT JOIN  T_PRD_MOENTRY_Q mq on e.FENTRYID = mq.FENTRYID
                        LEFT JOIN  T_PRD_MOENTRY_A a on e.FENTRYID = a.FENTRYID
	                    left join  T_BD_DEPARTMENT d on e.FWORKSHOPID = d.FDEPTID
	                    left join T_BD_MATERIAL m on e.FMATERIALID = m.FMATERIALID
	                    left join T_BD_MATERIAL_L ml on e.FMATERIALID = ml.FMATERIALID and ml.FLOCALEID = 2052
	                    left join T_SEC_USER ul on ul.FUSERID=mq.FFORCECLOSERID
	                    where mo.FID= @FID
	                    order by e.FENTRYID ";
            var datas = DBUtils.ExecuteDynamicObject(ctx, sql, paramList: new SqlParam("@FID", KDDbType.Int64, id));
            List<MesProductionCloseEntity> list = new List<MesProductionCloseEntity>();
            foreach (var data in datas)
            {
                list.Add(new MesProductionCloseEntity
                {
                    MakeSeq = Convert.ToInt32(data["makeSeq"]),
                    WorksNo = Convert.ToString(data["worksNo"]),
                    DwgNo = Convert.ToString(data["dwgNo"]),
                    DwgVer = Convert.ToString(data["dwgName"]),
                    Qty = Convert.ToDecimal(data["qty"]),
                    InQty = Convert.ToDecimal(data["inQty"]),
                    CloseUserName = Convert.ToString(data["closeUserName"]),
                    CloseType = Convert.ToString(data["closeType"]),
                    Status = Convert.ToInt32(data["status"]),
                });
            }

            return list;
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
