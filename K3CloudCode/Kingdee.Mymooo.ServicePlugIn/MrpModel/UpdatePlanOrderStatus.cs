using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.Business;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG;
using Kingdee.K3.Core.MFG.EnumConst;
using Kingdee.K3.Core.MFG.PLN.MrpEntity;
using Kingdee.K3.Core.MFG.PLN.MrpModel;
using Kingdee.K3.MFG.Contracts.PLN;
using Kingdee.K3.MFG.PLN.App.MrpModel;
using Kingdee.K3.MFG.PLN.App.MrpModel.MrpPlugIn.PlugIn.Args;
using Kingdee.K3.MFG.PLN.App.MrpModel.PolicyImpl.NetCalc;
using Kingdee.K3.MFG.PLN.App.MrpModel.Util;
using Kingdee.Mymooo.App.Core;
using Kingdee.Mymooo.Core.Utils;
using static System.Net.WebRequestMethods;
using static Kingdee.K3.Core.MFG.EnumConst.Enums.PLN_MrpModel;

namespace Kingdee.Mymooo.ServicePlugIn.MrpModel
{
    [Description("MRP运算插件-自动审核计划订单逻辑模型"), HotUpdate]
    public class UpdatePlanOrderStatus : AbstractMrpLogicUnit
    {
        public override string Description
        {
            get { return "二开自定义逻辑"; }
        }
        protected override void OnExecuteLogicUnit()
        {
            base.OnExecuteLogicUnit();
        }

        protected override void AfterExecuteLogicUnit()
        {
            base.AfterExecuteLogicUnit();
            WriteInfoLog("处理扩展二开信息--开始", 1, 83);

            var computeid = this.MrpGlobalDataContext.ComputeId;
            StringBuilder sp = new StringBuilder();
            //二开处理逻辑
            string sSql = $@"SELECT FID,FBILLNO FROM T_PLN_PLANORDER WHERE FCOMPUTEID='{computeid}'
                            AND (FDEMANDORGID=224428 OR FDEMANDORGID=1043841 OR FDEMANDORGID=7348029)";
            var datas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
            foreach (var item in datas)
            {
                var planid = Convert.ToString(item["FID"]);
                var billno = Convert.ToString(item["FBILLNO"]);
                // 取到需要自动提交、审核的单据内码
                var billView = FormMetadataUtils.CreateBillView(this.Context, "PLN_PLANORDER", planid);
                if (billView == null)
                {
                    continue;
                }
                try
                {
                    using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                    {
                        MymoooBusinessDataService service = new MymoooBusinessDataService();
                        var oper = service.SaveAndAuditBill(this.Context, billView.BillBusinessInfo, new DynamicObject[] { billView.Model.DataObject });
                        if (!oper.IsSuccess)
                        {
                            var errormsg = "";
                            if (oper.ValidationErrors.Count > 0)
                            {
                                errormsg = string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                            }
                            else
                            {
                                errormsg = string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                            }
                            sp.AppendLine($"计划订单{billno}提交审核失败:{errormsg}");
                        }
                        else
                        {
                            sp.AppendLine($"计划订单{billno}已被处理为自动审核");
                        }
                        cope.Complete();
                    }
                    //清除释放网控
                    billView.CommitNetworkCtrl();
                    billView.InvokeFormOperation(FormOperationEnum.Close);
                    billView.Close();
                }
                catch
                {
                    continue;
                }

            }
            if (!sp.IsNullOrEmptyOrWhiteSpace())
            {
                WriteInfoLog(sp.ToString(), 4, 90);
            }
            //全国一部计划订单触发保存
            sSql = $@"SELECT FID,FBILLNO FROM T_PLN_PLANORDER WHERE FCOMPUTEID='{computeid}'
                            AND FDEMANDORGID=7401780";
            datas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
            foreach (var item in datas)
            {
                var planid = Convert.ToString(item["FID"]);
                var billno = Convert.ToString(item["FBILLNO"]);
                // 取到需要自动提交、审核的单据内码
                var billView = FormMetadataUtils.CreateBillView(this.Context, "PLN_PLANORDER", planid);
                if (billView == null)
                {
                    continue;
                }
                try
                {
                    using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                    {
                        MymoooBusinessDataService service = new MymoooBusinessDataService();
                        var oper = service.SaveBill(this.Context, billView.BillBusinessInfo, new DynamicObject[] { billView.Model.DataObject });
                        if (!oper.IsSuccess)
                        {
                            var errormsg = "";
                            if (oper.ValidationErrors.Count > 0)
                            {
                                errormsg = string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                            }
                            else
                            {
                                errormsg = string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                            }
                            sp.AppendLine($"计划订单{billno}保存失败:{errormsg}");
                        }
                        else
                        {
                            sp.AppendLine($"计划订单{billno}已被处理为创建");
                        }
                        cope.Complete();
                    }
                    //清除释放网控
                    billView.CommitNetworkCtrl();
                    billView.InvokeFormOperation(FormOperationEnum.Close);
                    billView.Close();
                }
                catch
                {
                    continue;
                }

            }
            if (!sp.IsNullOrEmptyOrWhiteSpace())
            {
                WriteInfoLog(sp.ToString(), 4, 90);
            }

            WriteInfoLog("处理扩展二开信息--结束", 1, 83);
        }

        public void WriteInfoLog(string msg, int logclass, int logDetailclass)
        {
            base.ExtendServiceProvider.GetService<IMrpLogService>().WriteLog(
                msg, (Enu_MrpLogClass)logclass, (Enu_MrpLogDetailClass)logDetailclass, false);
        }

    }
}
