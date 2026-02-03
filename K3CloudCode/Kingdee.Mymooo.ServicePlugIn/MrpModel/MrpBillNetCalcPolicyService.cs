using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.MFG.PLN.App.MrpModel.PolicyImpl.NetCalc;
using static Kingdee.K3.FIN.Core.Object.AP_Matck;
using static Kingdee.K3.MFG.PLN.App.MrpModel.PolicyImpl.NetCalc.AbstractNetCalcPolicy;

namespace Kingdee.Mymooo.ServicePlugIn.MrpModel
{
    [Description("MRP_DP_NC_CommitData"), HotUpdate]
    public class MrpBillNetCalcPolicyService : AbstractNetCalcPolicy
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
        ///
        protected override bool BeforeExecuteDataPolicy()
        {
            foreach (var demandGroupRow in this.MrpDemandDimContext.MrpNetDemandContextGroupRows)
            {
                if (demandGroupRow.PlanOrderItem != null)
                {
					long[] orglongs = new long[] { 7401803, 7401821, 14053641 };
					///TODO：本分支可对计划订单数据在提交保存前进行干预
					//这里示例更新计划订单量=需求数量+计划订单数量
					//因为在整个MRP过程中，为提升性能，因此内存中都是基本数量在运转，实际业务数量将在提交数据库保存之后直接进行update更新，所以本示例是用基本数量来说明。
					//如果需要取得计划订单业务数据进行再加工，则可跟帖讨论，是在另一个时机的数据策略中可实现
					var demanorgid = Convert.ToInt64(demandGroupRow.PlanOrderItem["DemandOrgId_Id"]);
                    var supplytargetorgid = Convert.ToInt64(demandGroupRow.PlanOrderItem["FSupplyTargetOrgId_Id"]);
                    if (demanorgid == 7401803)
                    {
                        var materialid = Convert.ToInt64(demandGroupRow.PlanOrderItem["MaterialId_Id"]);
                        string sSql = @"SELECT t1.FMATERIALID,t1.FNUMBER,t1.FMATERIALGROUP,t2.FPARENTID,t2.FIsSendMES FROM dbo.T_BD_MATERIAL t1
LEFT JOIN T_BD_MATERIALGROUP t2 ON t1.FMATERIALGROUP=t2.FID
WHERE FMATERIALID=@FMATERIALID";
                        List<SqlParam> pars = new List<SqlParam>();
                        pars.Add(new SqlParam("@FMATERIALID", KDDbType.Int64, materialid));
                        var material = DBUtils.ExecuteDynamicObject(this.Context, sSql, paramList: pars.ToArray());
                        foreach (var item in material)
                        {
                            //demandGroupRow.PlanOrderItem["ReleaseType"] = 3;
                            demandGroupRow.PlanOrderItem["FPARENTSMALLID_Id"] = Convert.ToInt32(item["FPARENTID"]);
                            demandGroupRow.PlanOrderItem["FSMALLID_Id"] = Convert.ToInt32(item["FMATERIALGROUP"]);
                            if (supplytargetorgid != 7401803)
                            {
                                demandGroupRow.PlanOrderItem["FIsSendCNCMES"] = 0;
                            }
                            else
                            {
                                demandGroupRow.PlanOrderItem["FIsSendCNCMES"] = Convert.ToInt32(item["FIsSendMES"]);
                            }
                            //组装件车间
                            if (Convert.ToInt32(item["FMATERIALGROUP"]) == 222)
                            {
                                sSql = $"SELECT FDEPTID FROM dbo.T_BD_DEPARTMENT WHERE FNUMBER='BM000412' AND FUSEORGID={demanorgid}";
                                //pars = new List<SqlParam>();
                                //pars.Add(new SqlParam("@FNUMBER", KDDbType.String, "BM000405"));
                                //pars.Add(new SqlParam("@FUSEORGID", KDDbType.Int64, demanorgid));
                                var deptid = DBUtils.ExecuteScalar<long>(this.Context, sSql, 0);
                                demandGroupRow.PlanOrderItem["PrdDeptId_Id"] = deptid;
                            }
                        }
                    }
					if (orglongs.Contains(demanorgid))
					{
						var materialid = Convert.ToInt64(demandGroupRow.PlanOrderItem["MaterialId_Id"]);
						string sSql = @"SELECT TOP 1 FSUPPLIERID FROM dbo.T_PLN_PLANORDER
                        WHERE FDEMANDORGID=@FDEMANDORGID AND FMATERIALID=@FMATERIALID AND FSupplierId<>0
                        ORDER BY FID DESC";
						List<SqlParam> pars = new List<SqlParam>();
						pars.Add(new SqlParam("@FDEMANDORGID", KDDbType.Int64, demanorgid));
						pars.Add(new SqlParam("@FMATERIALID", KDDbType.Int64, materialid));
						var dynamicObjects = DBUtils.ExecuteDynamicObject(this.Context, sSql, paramList: pars.ToArray());
                        foreach (var item in dynamicObjects)
                        {
							demandGroupRow.PlanOrderItem["SupplierId_Id"] = Convert.ToInt32(item["FSUPPLIERID"]);
						}

                        //demandGroupRow.PlanOrderItem["FPENYAmount"] = Convert.ToDecimal(demandGroupRow.PlanOrderItem["FPENYPrice"])
                        //    * Convert.ToDecimal(demandGroupRow.PlanOrderItem["FirmQty"]);
					}
					//demandGroupRow.PlanOrderItem["BaseSugQty"] =
					//Convert.ToDecimal(demandGroupRow.PlanOrderItem["BaseDemandQty"]) + Convert.ToDecimal(demandGroupRow.PlanOrderItem["BaseOrderQty"]);
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
