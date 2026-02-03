using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;
using Kingdee.BOS;
using Kingdee.BOS.Util;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Const;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.Mymooo.Core.PlanOrderManagement;
using Kingdee.Mymooo.Core.ProductionManagement;

namespace Kingdee.Mymooo.Business.PlugIn.ProductionManagement
{
    public class PrdMOOrderBusiness
    {
        
        public ResponseMessage<dynamic> PrdMoChange(Context ctx, PrdMoChangeRequest request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();

            IOperationResult oper = new OperationResult();
            response.Code = ResponseCode.Success;
            response.Message = "创建成功";
            using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
            {
                List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
                var row = new ListSelectedRow(Convert.ToString(request.MoId), Convert.ToString(request.MoEntryId), 0, "PRD_MO");
                row.EntryEntityKey = "FTreeEntity"; //这里最容易忘记加，是重点的重点
                selectedRows.Add(row);
                var result = this.SaleBillPushRo(ctx, "PRD_MO2MOCHANGE", request, selectedRows);
                if (!result.IsSuccess)
                {
                    if (result.ValidationErrors.Count > 0)
                    {
                        throw new Exception(string.Join(";", result.ValidationErrors.Select(p => p.Message)));
                    }
                    else
                    {
                        throw new Exception(string.Join(";", result.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                    }
                }
                response.Data = result.OperateResult.Select(x => new { x.PKValue, x.Number, x.Message });
                cope.Complete();
            }

            return response;
        }

        private IOperationResult SaleBillPushRo(Context ctx, string ConvertRule, PrdMoChangeRequest pushEntity, List<ListSelectedRow> listSelectedRow)
        {
            //得到转换规则
            var convertRule = this.GetConvertRule(ctx, ConvertRule);
            OperateOption pushOption = OperateOption.Create();//操作选项
            //构建下推参数
            //pushOption.SetVariableValue(ConvertConst.SelectByBillId, pushEntity.SetVariableValue);
            //单据下推参数
            PushArgs pushArgs = new PushArgs(convertRule, listSelectedRow.ToArray());
            //目标单据主组织，可选参数，基础资料隔离，给没有住组织的目标数据包赋值，取当前登录组织即可
            //pushArgs.TargetOrgId = pushEntity.TargetOrgId;
            //目标单据类型，必填参数，除非下游单据类型不是必填的，对源单数据进行过滤，给目标单赋值
            //pushArgs.TargetBillTypeId = pushEntity.TargetBillTypeId;
            // 自动下推，无需验证用户功能权限
            pushOption.SetVariableValue(BOSConst.CST_ConvertValidatePermission, true);
            // 设置是否整单下推
            //pushOption.SetVariableValue(ConvertConst., false);

            var convertResult = ConvertServiceHelper.Push(ctx, pushArgs, pushOption);//调用下推接口
            var targetObjs = (from p in convertResult.TargetDataEntities select p.DataEntity).ToArray();//得到目标单据数据包
            foreach (DynamicObject targetObj in targetObjs)
            {
                var targeEntry = ((DynamicObjectCollection)targetObj["Entity"])
                    .Where(x => Convert.ToInt32(x["ChangeType"]) == 3
                    && Convert.ToInt64(x["SrcBillEntryId"]) == pushEntity.MoEntryId).First();

                targeEntry["Qty"] = pushEntity.ChangeQty;
                targeEntry["BaseQty"] = pushEntity.ChangeQty;
                targeEntry["PlanFinishDate"] = pushEntity.PlanFinishDate;
                targeEntry["StockInLimitH"] = pushEntity.ChangeQty;
                targeEntry["BaseStockInLimitH"] = pushEntity.ChangeQty;
                targeEntry["StockInLimitL"] = pushEntity.ChangeQty;
                targeEntry["BaseStockInLimitL"] = pushEntity.ChangeQty;
            }
            var targetBInfo = this.GetBusinessInfo(ctx, pushArgs.ConvertRule.TargetFormId, null);
            //对转换结果进行处理
            //1. 直接调用保存接口，对数据进行保存
            return MymoooBusinessDataServiceHelper.SaveAndAuditBill(ctx, targetBInfo, targetObjs);
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
    }
}
