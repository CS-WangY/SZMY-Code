using Kingdee.BOS;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
 using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Kingdee.K3.MFG.App.AppServiceContext;

namespace Kingdee.Mymooo.ServicePlugIn.Stk_TransferOut
{
    [Description("分步式调出单提交发送企业微信消息"), HotUpdate]
    public class Submit : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FStockOrgID");//调出组织
            e.FieldKeys.Add("FStockInOrgID");//调入组织
            e.FieldKeys.Add("FMaterialID");
            e.FieldKeys.Add("FSrcStockID");
            e.FieldKeys.Add("FQty");
            e.FieldKeys.Add("FUnitID");

        }
        private void autoAudit(long fid)
        {
            // 构建单据主键参数
            List<KeyValuePair<object, object>> pkEntityIds = new List<KeyValuePair<object, object>>();
            pkEntityIds.Add(new KeyValuePair<object, object>(fid, ""));
            // 构建操作可选参数对象
            OperateOption auditOption = OperateOption.Create();
            auditOption.SetIgnoreWarning(this.Option.GetIgnoreWarning());
            auditOption.SetInteractionFlag(this.Option.GetInteractionFlag());
            auditOption.SetIgnoreInteractionFlag(this.Option.GetIgnoreInteractionFlag());
            List<object> paras = new List<object>();
            paras.Add("1");
            paras.Add("");
            // 调用审核操作
            ISetStatusService setStatusService = ServiceFactory.GetSetStatusService(this.Context);
            // 如下调用方式，需显示交互信息
            IOperationResult auditResult = setStatusService.SetBillStatus(this.Context,
            this.BusinessInfo,
            pkEntityIds,
            paras,
            "Audit",
            auditOption);
            // 判断审核结果，如果失败，则内部会抛出错误，回滚代码
            if (CheckOpResult(auditResult) == false)
            {
                return;
            }
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);

            foreach (var item in e.DataEntitys)
            {
                var fid = Convert.ToInt64(item["Id"]);
                var billno = Convert.ToString(item["BillNo"]);
                var billtype = Convert.ToString(item["TransferDirect"]);
                if (billtype.EqualsIgnoreCase("RETURN"))
                {
                    #region 如果是退货全国一部二部自动审核9.11加入华东五部华南五部

                    //如果是全国一部则自动审核调出单
                    bool issyn = false;
                    //调拨单云仓储自动上下架
                    issyn = Convert.ToBoolean((item["StockInOrgID"] as DynamicObject)["FTransIsSynCloudStock"]);
                    if (issyn)
                    {
                        autoAudit(fid);
                    }
                    #endregion
                }
                //else
                //{
                //    //如果是华南华东五部则自动审核华南五部7401782华东五部7401803
                //    long[] OutStockList = new long[] { 7401782, 7401803 };
                //    if (OutStockList.Contains(Convert.ToInt64(item["StockOrgID_Id"])))
                //    {
                //        autoAudit(fid);
                //    }
                //}


                List<string> stockWxCode = new List<string>();
                List<string> send_WxContent = new List<string>();

                foreach (var entitem in item["STK_STKTRANSFEROUTENTRY"] as DynamicObjectCollection)
                {

                    var stockid = Convert.ToInt64(entitem["SrcStockID_Id"]);
                    var userwxCode = GetStockUserWxCode(Context, stockid);
                    if (!stockWxCode.Contains(userwxCode))
                    {
                        stockWxCode.Add(userwxCode);
                    }
                    send_WxContent.Add(Convert.ToString(((DynamicObject)entitem["MaterialID"])["Number"]) + "[" + Convert.ToString(entitem["FQty"]) + "]");

                }
                //发送企业微信消息
                if (stockWxCode.Count > 0)
                {
                    //SendTextMessageUtils.SendTextMessage(string.Join("|", stockWxCode), string.Join(",", send_WxContent));
                    SendTextMessageUtils.SendTextMessage(string.Join("|", stockWxCode), "您有一条新的分步式调出单需要处理：" + billno);
                }

            }

        }

        private string GetStockUserWxCode(Context ctx, long stockId)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FSTOCKID", KDDbType.Int64, stockId) };
            string sql = $@"SELECT t3.FWECHATCODE FROM dbo.T_BD_STOCK t1
                            LEFT JOIN T_BD_STAFF t2 ON t1.FPRINCIPAL=t2.FSTAFFID
                            LEFT JOIN T_HR_EMPINFO t3 ON t2.FEMPINFOID=t3.FID
                            WHERE t1.FSTOCKID=@FSTOCKID
            ";

            return DBUtils.ExecuteScalar<string>(ctx, sql, string.Empty, paramList: pars.ToArray());
        }

        ///
        /// 判断操作结果是否成功，如果不成功，则直接抛错中断进程
        ///
        ///
        ///
        private bool CheckOpResult(IOperationResult opResult)
        {
            bool isSuccess = false;
            if (opResult.IsSuccess == true)
            {
                // 操作成功
                isSuccess = true;
            }
            else
            {
                if (opResult.InteractionContext != null
                && opResult.InteractionContext.Option.GetInteractionFlag().Count > 0)
                {// 有交互性提示
                    // 传出交互提示完整信息对象
                    this.OperationResult.InteractionContext = opResult.InteractionContext;
                    // 传出本次交互的标识，
                    // 用户在确认继续后，会重新进入操作；
                    // 将以此标识取本交互是否已经确认过，避免重复交互
                    this.OperationResult.Sponsor = opResult.Sponsor;
                    // 抛出错误，终止本次操作
                    //throw new KDBusinessException("", "本次操作需要用户确认是否继续，暂时中断");
                    throw new KDBusinessException("", opResult.InteractionContext.SimpleMessage);
                }
                else
                {
                    // 操作失败，拼接失败原因，然后抛出中断
                    opResult.MergeValidateErrors();
                    if (opResult.OperateResult == null)
                    {// 未知原因导致提交失败
                        throw new KDBusinessException("", "未知原因导致自动提交、审核失败！");
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("自动提交、审核失败，失败原因：");
                        foreach (var operateResult in opResult.OperateResult)
                        {
                            sb.AppendLine(operateResult.Message);
                        }
                        throw new KDBusinessException("", sb.ToString());
                    }
                }
            }
            return isSuccess;
        }

    }
}
