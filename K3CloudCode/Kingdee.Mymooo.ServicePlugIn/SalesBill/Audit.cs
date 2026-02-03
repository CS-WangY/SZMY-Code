using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.ServiceHelper.SalesManagement;
using Kingdee.Mymooo.ServiceHelper.StockManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.SalesBill
{
    [Description("销售订单审核时生成收款单"), HotUpdate]
    public class Audit : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FIsPayOnline");
            e.FieldKeys.Add("FSettleMode_PENY");
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            foreach (var item in e.DataEntitys)
            {
                var fid = Convert.ToInt64(item["Id"]);
                var billtype = item["BillTypeID_Id"].ToString();
                var billno = item["BillNo"].ToString();
                foreach (var entitem in item["SaleOrderPlan"] as DynamicObjectCollection)
                {
                    var eid = Convert.ToInt64(entitem["Id"]);
                    bool ispo = (Boolean)entitem["FIsPayOnline"];
                    if (ispo)
                    {
                        var entitylist = new SalesOrderPushReceiveBillEntity();
                        entitylist.FID = fid;
                        entitylist.FEntryID = eid;
                        var paytype = entitem["FSettleMode_PENY"] as DynamicObject;
                        List<ReplaceVal> replaceVals = new List<ReplaceVal>();
                        replaceVals.Add(new ReplaceVal
                        {
                            EntityType = EntityType.BillEntry,
                            EntityKey = "RECEIVEBILLENTRY",
                            EntityValue = "SETTLETYPEID",
                            valueType = TargetValueType.Object,
                            Val = paytype
                        });

                        entitylist.rval = replaceVals;
                        IOperationResult request = SalesOrderServiceHelper.SalesToReceiveBill(this.Context, entitylist, this.Context.CurrentOrganizationInfo.ID);
                        //SalesOrderServiceHelper.SalesDrawReceiveBill(this.Context, entitylist, this.Context.CurrentOrganizationInfo.ID);
                        if (request.IsSuccess)
                        {
                            //throw new Exception("锁库失败!");
                        }
                    }

                }
            }
        }
    }
}
