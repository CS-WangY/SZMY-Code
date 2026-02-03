using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.PlanOrderBill
{
    [Description("计划订单投放插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class Release : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FBILLNO");//单据编号
            e.FieldKeys.Add("FDataSource");//数据来源(1运算生成/2手工录入/3拆分生成/4合并生成)
            e.FieldKeys.Add("FSupplyOrgId");//采购/生产组织
            e.FieldKeys.Add("FSaleOrderNo");//需求单据编号
            e.FieldKeys.Add("FSaleOrderEntrySeq");//需求单据行号
            e.FieldKeys.Add("FDemandType");//需求来源(1销售订单)
            e.FieldKeys.Add("FReleaseType");//投放类型(2委外订单/3采购申请订单)
            e.FieldKeys.Add("FIsSendCNCMES");//是否发送cncmes
        }
        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
            ReleaseValidator isPoValidator = new ReleaseValidator();
            isPoValidator.AlwaysValidate = true;
            isPoValidator.EntityKey = "FBillHead";
            e.Validators.Add(isPoValidator);
        }
    }
}
