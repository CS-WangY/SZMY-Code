using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.Metadata.QueryElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
 using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.K3.MFG.PLN.App.MrpModel.PolicyImpl.NetCalc;
using Kingdee.Mymooo.App.Core.BaseManagement;
using static System.Net.WebRequestMethods;

namespace Kingdee.Mymooo.ServicePlugIn.MrpModel
{
    public class ROPMtrlStockData : AbstractNetCalcPolicy
    {
        ///
        /// 后置策略：单条执行模式
        ///
        public override AbstractNetCalcPolicy.Enu_NetCalcPolicyCallStyle CallStyle
        {
            get { return Enu_NetCalcPolicyCallStyle.KdLastSingleExecutionMode; }
        }

        ///
        /// 策略执行前事件
        ///
        protected override bool BeforeExecuteDataPolicy()
        {
            foreach (var demandGroupRow in this.MrpDemandDimContext.MrpNetDemandContextGroupRows)
            {
                if (demandGroupRow.PlanOrderItem != null)
                {
                    ///TODO：本分支可对计划订单数据在提交保存前进行干预
                    //因为在整个MRP过程中，为提升性能，因此内存中都是基本数量在运转，实际业务数量将在提交数据库保存之后直接进行update更新
                    //demandGroupRow.PlanOrderItem["BaseSugQty"] =
                    //Convert.ToDecimal(demandGroupRow.PlanOrderItem["BaseDemandQty"]) + Convert.ToDecimal(demandGroupRow.PlanOrderItem["BaseOrderQty"]);
                    var materialid = Convert.ToInt64(demandGroupRow.PlanOrderItem["SupplyMaterialId_Id"]);
                    string sSql = $@"select FUSEORGID from T_BD_MATERIAL WHERE FMATERIALID = {materialid}";
                    var supplyorgid = DBUtils.ExecuteScalar<long>(this.Context, sSql, 0);
                    if (Convert.ToInt64(demandGroupRow.PlanOrderItem["FSupplyTargetOrgId_Id"]) == 0)
                    {
                        if (supplyorgid != 0)
                        {
                            demandGroupRow.PlanOrderItem["FSupplyTargetOrgId_Id"] = supplyorgid;
                        }
                    }
                }
                if (demandGroupRow.RequirementOrderItem != null)
                {
                    ///TODO：本分支可对组织间需求单数据在提交保存前进行干预
                }
            }
            return base.BeforeExecuteDataPolicy();
        }
    }
}