using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
 using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.SalesBill
{
    [Description("销售订单-整单关闭修改匹配收款金额"), HotUpdate]
    public class YLBillClose : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FActRecAmount");
            e.FieldKeys.Add("FADVANCENO");
            e.FieldKeys.Add("FADVANCESEQ");
            e.FieldKeys.Add("FRemainAmount");
            e.FieldKeys.Add("FPreMatchAmountFor");
            e.FieldKeys.Add("FADVANCEENTRYID");
            e.FieldKeys.Add("FADVANCEID");
            e.FieldKeys.Add("FRecAmount");
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            // 保存8提交9审核1反审核26
            if (this.FormOperation.OperationId == 38)
            {
                string sSql = "";
                foreach (var dataEntity in e.DataEntitys)
                {
                    var billid = dataEntity["Id"];
                    // 审核后设置采购日期等于审核日期
                    //dataEntity["Date"] = dataEntity["ApproveDate"];
                    foreach (var dataplan in dataEntity["SaleOrderPlan"] as DynamicObjectCollection)
                    {
                        decimal tolamount = 0;
                        foreach (var dataplanentry in dataplan["SAL_ORDERPLANENTRY"] as DynamicObjectCollection)
                        {
                            long advid = Convert.ToInt64(dataplanentry["ADVANCEID"]);
                            long adventid = Convert.ToInt64(dataplanentry["ADVANCEENTRYID"]);
                            decimal amount = Convert.ToDecimal(dataplanentry["ActRecAmount"]);
                            decimal prematamount = Convert.ToDecimal(dataplanentry["PreMatchAmountFor"]);
                            dataplanentry["ActRecAmount"] = dataplanentry["PreMatchAmountFor"];
                            //返回收款关联金额部分
                            sSql = $"UPDATE T_AR_RECEIVEBILLENTRY SET FASSTOTALAMOUNTFOR=FASSTOTALAMOUNTFOR-{amount - prematamount} WHERE FID={advid} AND FENTRYID={adventid}";
                            DBUtils.Execute(this.Context, sSql);
                            //返货收款销售关联金额部分
                            sSql = $"UPDATE T_AR_ASSSALESORDER SET FASSAMOUNTFOR=FASSAMOUNTFOR-{amount - prematamount} WHERE FENTRYID={adventid} AND FASSBILLID={billid}";
                            DBUtils.Execute(this.Context, sSql);
                            tolamount += prematamount;
                        }
                        dataplan["RecAmount"] = tolamount;
                    }
                }
                // 保存变更后的数据
                new BusinessDataWriter(Context).Save(e.DataEntitys);
            }

        }
    }
}
