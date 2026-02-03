using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm;
 using Kingdee.BOS.App.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.BOS;
using System.ComponentModel;
using Kingdee.BOS.Core.Interaction;
using Kingdee.Mymooo.ServicePlugIn.MrpModel.MrpCalingValidator;
using Kingdee.BOS.ServiceHelper;
namespace Kingdee.Mymooo.ServicePlugIn.Stk_TransferOut
{
    [Description("分步式调出单审核生成分步式调入单(提交状态)"), HotUpdate]
    public class Audit : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FStockInOrgID");//调入组织
            e.FieldKeys.Add("FSBILLID");//源单单据id

        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            foreach (var item in e.DataEntitys)
            {
                //if (((DynamicObjectCollection)((DynamicObjectCollection)item["STK_STKTRANSFEROUTENTRY"])[0]["FSTKTSTKRANSFEROUTENTRY_Link"]).Count == 0)
                //{
                //    return;
                //}
                List<string> sSql = new List<string>()
                {
                    "/*dialect*/ IF OBJECT_ID('Tempdb..#TM_STK_TANSSTATUSVALIDATE') IS NOT NULL DROP TABLE #TM_STK_TANSSTATUSVALIDATE"
                };
				DBServiceHelper.ExecuteBatch(Context, sSql);

                var fid = Convert.ToInt64(item["Id"]);
                //下推分步式调入单
                var rules = GetConvertRule(this.Context, "STK_TRANSFEROUT-STK_TRANSFERIN");
                //var rule = rules.FirstOrDefault(t => t.IsDefault);
                if (rules == null)
                {
                    throw new Exception("没有相关转换关系");
                }

                List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();

                selectedRows.Add(new ListSelectedRow(fid.ToString(), "", 0, this.BusinessInfo.GetForm().Id));
                PushArgs pushArgs = new PushArgs(rules, selectedRows.ToArray())
                {
                    //TargetBillTypeId = "f94e4845700c4dab93e652b61edcb819",     // 请设定目标单据单据类型
                    TargetOrgId = 0,            // 请设定目标单据主业务组织
                                                //CustomParams = ,     // 可以传递额外附加的参数给单据转换插件
                };
                //执行下推操作，并获取下推结果
                var convertResult = ConvertServiceHelper.Push(this.Context, pushArgs, null);
                List<DynamicObject> dynamicObjectList = new List<DynamicObject>();
                if (convertResult.IsSuccess)
                {
                    var billtype = Convert.ToString(item["TransferDirect"]);
                    if (billtype.EqualsIgnoreCase("RETURN"))
                    {
                        bool issyn = false;
                        //调拨单云仓储自动上下架
                        issyn = Convert.ToBoolean((item["StockInOrgID"] as DynamicObject)["FTransIsSynCloudStock"]);
                        #region 调出组织是全部一部和华南二部的需要自动批核8.23取消逻辑

                        #endregion
                        var targetObjs = (from p in convertResult.TargetDataEntities select p.DataEntity).ToArray();//得到目标单据数据包
                        var targetBInfo = this.GetBusinessInfo(this.Context, pushArgs.ConvertRule.TargetFormId, null);
                        if (issyn)
                        {
                            //保存
                            var opers = AuditTargetBill(this.Context, targetBInfo, targetObjs);
                            if (!opers.IsSuccess)
                            {
                                if (opers.ValidationErrors.Count > 0)
                                {
                                    throw new Exception(string.Join(";", opers.ValidationErrors.Select(p => p.Message)));
                                }
                                else
                                {
                                    throw new Exception(string.Join(";", opers.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                                }
                            }
                        }
                        else
                        {
                            var opers = SaveTargetBill(this.Context, targetBInfo, targetObjs);
                            if (!opers.IsSuccess)
                            {
                                if (opers.ValidationErrors.Count > 0)
                                {
                                    throw new Exception(string.Join(";", opers.ValidationErrors.Select(p => p.Message)));
                                }
                                else
                                {
                                    throw new Exception(string.Join(";", opers.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                                }
                            }
                        }
                    }
                    else
                    {
                        var targetObjs = (from p in convertResult.TargetDataEntities select p.DataEntity).ToArray();//得到目标单据数据包
                        var targetBInfo = this.GetBusinessInfo(this.Context, pushArgs.ConvertRule.TargetFormId, null);
                        //如果是华南华东五部则自动审核华南五部7401782华东五部7401803
                        long[] OutStockList = new long[] { 7401782, 7401803 };
                        ////调拨单云仓储自动上下架
                        if (Convert.ToBoolean(((DynamicObject)item["StockOrgId"])["FTransIsSynCloudStock"]))
                        {
                            //审核
                            var opers = AuditTargetBill(this.Context, targetBInfo, targetObjs);
                            if (!opers.IsSuccess)
                            {
                                if (opers.ValidationErrors.Count > 0)
                                {
                                    throw new Exception(string.Join(";", opers.ValidationErrors.Select(p => p.Message)));
                                }
                                else
                                {
                                    throw new Exception(string.Join(";", opers.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                                }
                            }
                        }
                        else
                        {
                            var opers = SaveTargetBill(this.Context, targetBInfo, targetObjs);
                            if (!opers.IsSuccess)
                            {
                                if (opers.ValidationErrors.Count > 0)
                                {
                                    throw new Exception(string.Join(";", opers.ValidationErrors.Select(p => p.Message)));
                                }
                                else
                                {
                                    throw new Exception(string.Join(";", opers.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                                }
                            }
                        }

                    }
                }
                else
                {
                    if (convertResult.ValidationErrors.Count > 0)
                    {
                        throw new Exception(string.Join(";", convertResult.ValidationErrors.Select(p => p.Message)));
                    }
                    else
                    {
                        throw new Exception(string.Join(";", convertResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                    }
                }


            }
        }
        //得到转换规则
        private ConvertRuleElement GetConvertRule(Context ctx, string convertRuleId)
        {
            var convertRuleMeta = ConvertServiceHelper.GetConvertRule(ctx, convertRuleId);
            return convertRuleMeta.Rule;
        }
        // 得到表单元数据
        private BusinessInfo GetBusinessInfo(Context ctx, string formId, FormMetadata metaData = null)
        {
            if (metaData != null) return metaData.BusinessInfo;
            metaData = FormMetaDataCache.GetCachedFormMetaData(ctx, formId);
            return metaData.BusinessInfo;
        }
        private IOperationResult SaveTargetBill(Context ctx, BusinessInfo targetBusinessInfo, DynamicObject[] targetBillObjs)
        {
            OperateOption saveOption = OperateOption.Create();
            saveOption.SetIgnoreWarning(true);
            saveOption.SetIgnoreInteractionFlag(true);
            saveOption.AddInteractionFlag(K3.Core.SCM.SCMConst.MinusCheckSensor);
            SaveService saveService = new SaveService();
            //var saveResult = saveService.Save(ctx, targetBusinessInfo, targetBillObjs, saveOption);

            IOperationResult saveResult = new OperationResult();
            saveResult = saveService.Save(ctx, targetBusinessInfo, targetBillObjs, saveOption);
            if (!saveResult.IsSuccess)
            {
                if (saveResult.ValidationErrors.Count > 0)
                {
                    throw new Exception(string.Join(";", saveResult.ValidationErrors.Select(p => p.Message)));
                }
                else
                {
                    throw new Exception(string.Join(";", saveResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                }
            }
            else
            {
                Dictionary<string, string> pkids = new Dictionary<string, string>();
                pkids = saveResult.SuccessDataEnity.ToDictionary(p => p["Id"].ToString(), p => p["BillNo"].ToString());
                SubmitService submitService = new SubmitService();
                //提交单据，若存在工作流，则提交工作流
                var resultlist = Kingdee.K3.Core.MFG.Utils.MFGCommonUtil.SubmitWithWorkFlow(ctx, targetBusinessInfo.GetForm().Id, pkids, saveOption);
            }

            return saveResult;
        }
        private IOperationResult AuditTargetBill(Context ctx, BusinessInfo targetBusinessInfo, DynamicObject[] targetBillObjs)
        {
            OperateOption saveOption = OperateOption.Create();
            saveOption.SetIgnoreWarning(true);
            saveOption.SetIgnoreInteractionFlag(true);
            saveOption.AddInteractionFlag(K3.Core.SCM.SCMConst.MinusCheckSensor);
            SaveService saveService = new SaveService();
            //var saveResult = saveService.Save(ctx, targetBusinessInfo, targetBillObjs, saveOption);

            IOperationResult saveResult = new OperationResult();
            saveResult = saveService.SaveAndAudit(ctx, targetBusinessInfo, targetBillObjs, saveOption);
            if (!saveResult.IsSuccess)
            {
                if (saveResult.ValidationErrors.Count > 0)
                {
                    throw new Exception(string.Join(";", saveResult.ValidationErrors.Select(p => p.Message)));
                }
                else
                {
                    throw new Exception(string.Join(";", saveResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                }
            }

            return saveResult;
        }
        //审核增加运算校验
        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
            e.Validators.Add(new IsMrpCalingValidator() { AlwaysValidate = true, EntityKey = "FBillHead" });
        }
    }
}
