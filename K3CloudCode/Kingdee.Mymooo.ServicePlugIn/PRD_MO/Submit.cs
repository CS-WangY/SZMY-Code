using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.BOS.Util;
using System.ComponentModel;
 using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.BusinessFlow;

namespace Kingdee.Mymooo.ServicePlugIn.PRD_MO
{
    [Description("生产订单提交调用云平台派产"), HotUpdate]
    public class Submit : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FBillTypeID");
            e.FieldKeys.Add("FMaterialId");
            e.FieldKeys.Add("FModifierId");
        }
        public override void OnPrepareOperationServiceOption(OnPrepareOperationServiceEventArgs e)
        {
            base.OnPrepareOperationServiceOption(e);
            e.SupportTransaction = true;
            e.RollbackWhenValidationError = true;
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);

            foreach (var item in e.DataEntitys)
            {
                var fid = Convert.ToInt64(item["Id"]);
                //var billtype = item["BillTypeID_Id"].ToString();
                var billno = item["BillNo"].ToString();
                var subDate = System.DateTime.Now.ToString();
                var subUser = this.Context.UserName;
                List<string> Parts = new List<string>();

                //string disurl = Kingdee.Mymooo.Core.Utils.WebApiServiceUtils.DispatchToCloudUrl;
                //string url = disurl + "api/cnc/dispatch";

                foreach (var entitem in item["TreeEntity"] as DynamicObjectCollection)
                {
                    var MaterialId = entitem["MaterialId"] as DynamicObject;
                    var MNumber = Convert.ToString(MaterialId["Number"]);
                    foreach (var itemlink in entitem["FTREEENTITY_Link"] as DynamicObjectCollection)
                    {
                        var sbillid = "";
                        var sid = "";

                        var planid = Convert.ToString(itemlink["SBillId"]);
                        string sSql = $"SELECT FSALEORDERID,FSALEORDERENTRYID FROM T_PLN_PLANORDER_B WHERE FID={planid}";
                        var plandata = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                        foreach (var plan in plandata)
                        {
                            sbillid = Convert.ToString(plan["FSALEORDERID"]);
                            sid = Convert.ToString(plan["FSALEORDERENTRYID"]);
                            sSql = $@"SELECT t2.FBillNo,t3.FNUMBER,t1.FISDISPATCH FROM T_SAL_ORDERENTRY t1 
                                            left join T_SAL_ORDER t2 on t1.FID=t2.FID
                                            LEFT JOIN T_BAS_BILLTYPE t3 ON t2.FBILLTYPEID=t3.FBILLTYPEID
                                            where t1.FID={sbillid} AND t1.FENTRYID={sid} AND t3.FNUMBER='FBXSDD'";
                            var sqldata = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                            foreach (var senditem in sqldata)
                            {
                                if (Convert.ToInt16(senditem["FISDISPATCH"]) >= 1)
                                {
                                    continue;
                                }
                                Parts.Add(Convert.ToString(MNumber));
                                var pairs = new
                                {
                                    SalesOrderNumber = Convert.ToString(senditem["FBillNo"]),
                                    Parts,
                                    DispatchDateTime = subDate,
                                    DispatchUser = subUser
                                };
                                //var request = SendApi(url, pairs);
                                //if (request.success)
                                //{
                                //    //更新销售订单同步标识
                                //    sSql = $"/*dialect*/UPDATE dbo.T_SAL_ORDERENTRY SET FISDISPATCH=1 WHERE FENTRYID={sid}";
                                //    DBUtils.Execute(Context, sSql);
                                //}
                                //else
                                //{
                                //    //记录数据，用于查询。
                                //    var logSql = $@"/*dialect*/ insert into RabbitMQScheduledMessage (FAction,FKeyword,FMessage,FCreateDate,FExecuteDate,FIsExecute) 
                                //        values  ('CNCAPI','{billno}','{request.msg}',SYSDATETIME(),SYSDATETIME(),'1')";
                                //    DBUtils.Execute(Context, logSql);
                                //}
                            }

                        }

                    }
                }

            }


        }

        //public NonStandardRequest SendApi(string url, object pairs)
        //{
        //    return JsonConvertUtils.DeserializeObject<NonStandardRequest>(Kingdee.Mymooo.Core.Utils.WebApiServiceUtils.HttpPost(url, pairs));

        //}
    }
}
