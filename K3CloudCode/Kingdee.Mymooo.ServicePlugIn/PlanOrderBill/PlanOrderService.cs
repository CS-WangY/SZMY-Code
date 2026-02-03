using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.Const;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.SCM;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core;
using Kingdee.Mymooo.Core.DirectSaleManagement;
using Kingdee.Mymooo.Core.PlanOrderManagement;
using Kingdee.Mymooo.ServiceHelper;
using System.Collections;
using Kingdee.Mymooo.Core.Common;
using Kingdee.BOS.Core.BusinessFlow.ServiceArgs;
using System.Web.UI.WebControls;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.BOS.ServiceHelper;
namespace Kingdee.Mymooo.ServicePlugIn.PlanOrderBill
{
    [Description("计划订单审核插件-关闭当前计划订单，生成组织间需求单(审核状态)"), HotUpdate]
    public class PlanOrderService : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FSupplyTargetOrgId");//供货组织

        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            foreach (var item in e.DataEntitys)
            {
                var fid = Convert.ToInt64(item["Id"]);
                var ids = new object[] { fid };
                //如果是华东五部
                if (Convert.ToInt64(item["DemandOrgId_Id"]) == 7401803 &&
                    Convert.ToInt64(item["DemandOrgId_Id"]) != Convert.ToInt64(item["FSupplyTargetOrgId_Id"]))
                {
                    goto NetxF;
                }
                //如果是北京蚂蚁
                if (Convert.ToInt64(item["DemandOrgId_Id"]) == 1043841 &&
                    Convert.ToInt64(item["DemandOrgId_Id"]) != Convert.ToInt64(item["FSupplyTargetOrgId_Id"]))
                {
                    goto NetxF;
                }
                //如果是江苏蚂蚁
                if (Convert.ToInt64(item["DemandOrgId_Id"]) == 7348029 &&
                    Convert.ToInt64(item["DemandOrgId_Id"]) != Convert.ToInt64(item["FSupplyTargetOrgId_Id"]))
                {
                    goto NetxF;
                }
                //如果是深圳224428广东蚂蚁669144
                long[] OutStockList = new long[] { 224428, 669144 };
                if (!OutStockList.Contains(Convert.ToInt64(item["DemandOrgId_Id"])))
                {
                    return;
                }
            NetxF:
                //已经存在下游单据则跳过
                if (BusinessFlowDataServiceHelper.IsPush(
                    this.Context,
                    new IsPushArgs(this.BusinessInfo, "FBillHead", item)
                    ))
                { return; }


                //下推组织间需求单
                var rules = GetConvertRule(this.Context, "PENY_PLANORDER_REQUIREMENTORDER");
                //var rule = rules.FirstOrDefault(t => t.IsDefault);
                if (rules == null)
                {
                    throw new Exception("没有相关转换关系");
                }

                List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();

                selectedRows.Add(new ListSelectedRow(fid.ToString(), "", 0, "PLN_PLANORDER"));
                //有数据才需要下推
                if (selectedRows.Count > 0)
                {
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
                        var targetObjs = (from p in convertResult.TargetDataEntities select p.DataEntity).ToArray();//得到目标单据数据包
                        var targetBInfo = this.GetBusinessInfo(this.Context, pushArgs.ConvertRule.TargetFormId, null);
                        //保存
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
                        else
                        {
                            var result = new OperateResult();
                            result.SuccessStatus = true;
                            result.PKValue = e.DataEntitys[0]["Id"];
                            var sContent = opers.SuccessDataEnity.Select(p => p["BillNo"].ToString());
                            //result.Number = ObjectUtils.Object2String(this.BusinessInfo.GetBillNoField().DynamicProperty.GetValueFast(e.DataEntitys[0]));
                            //result.Number = string.Join(",", sContent);
                            result.Message = "生成组织间需求单:" + string.Join(",", sContent);
                            this.OperationResult.OperateResult.Add(result);
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

                //关闭订单
                var operateOption = OperateOption.Create();
                operateOption.SetIgnoreWarning(true);
                List<KeyValuePair<object, object>> pkEntryIds = ids.Select((object x) => new KeyValuePair<object, object>(x, "")).ToList();
                SetStatusService setStatusService = new SetStatusService();
                var closeopers = setStatusService.SetBillStatus(this.Context, this.BusinessInfo, pkEntryIds, null, "HandClose", operateOption);
                if (!closeopers.IsSuccess)
                {
                    if (closeopers.ValidationErrors.Count > 0)
                    {
                        throw new Exception(string.Join(";", closeopers.ValidationErrors.Select(p => p.Message)));
                    }
                    else
                    {
                        throw new Exception(string.Join(";", closeopers.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
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
            var saveResult = saveService.SaveAndAudit(ctx, targetBusinessInfo, targetBillObjs, saveOption);
            //var saveResult = BusinessDataServiceHelper.Save(ctx, targetBusinessInfo, targetBillObjs, saveOption);
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

    }
    [Description("计划订单保存插件-根据供货组织携带大小类事业部"), HotUpdate]
    public class PlanOrderServiceSave : AbstractOperationServicePlugIn
    {
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);
            foreach (var item in e.DataEntitys)
            {
                var fid = Convert.ToInt64(item["Id"]);
                string sSql = $@"/*dialect*/UPDATE T_PLN_PLANORDER SET FSMALLID=t2.FMATERIALGROUP,FPARENTSMALLID=t3.FPARENTID
                    ,FBUSINESSDIVISIONID=ISNULL(t4.FBUSINESSDIVISION,'')
                    --SELECT *
                    FROM dbo.T_PLN_PLANORDER t1
                    LEFT JOIN dbo.T_BD_MATERIAL t2 ON t1.FMATERIALID=t2.FMATERIALID
                    LEFT JOIN dbo.T_BD_MATERIALGROUP t3 ON t2.FMATERIALGROUP=t3.FID
                    LEFT JOIN T_BD_MasterialGroupTaxCode t4 ON t2.FMATERIALGROUP=t4.FMATERIALGROUP
                    WHERE t1.FID={fid}";
                DBUtils.Execute(this.Context, sSql);

				//sSql = $@"/*dialect*/UPDATE T_PLN_PLANORDER SET FPENYAMOUNT=t1.FPENYPRICE*FDEMANDQTY
    //                --SELECT *
    //                FROM dbo.T_PLN_PLANORDER t1
    //                WHERE t1.FID={fid}";
				//DBUtils.Execute(this.Context, sSql);
			}

        }
    }
}
