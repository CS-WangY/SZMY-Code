using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;
using Kingdee.BOS;
using Kingdee.BOS.Util;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.Mymooo.Core.ProductionManagement;
using Kingdee.Mymooo.Contracts.PrdMoManagement;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Const;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Core.Metadata.ConvertElement;

namespace Kingdee.Mymooo.App.Core.PrdMoManagement
{
    public class MoOrderBillService : IMoOrderBillService
    {
        public ResponseMessage<dynamic> PrdMoChangeWorkShop(Context ctx, PrdMoChangeWorkShopRequest request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();

            IOperationResult oper = new OperationResult();
            response.Code = ResponseCode.Success;
            response.Message = "执行成功";
            using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
            {
                //查询生产订单是否存在
                var sSql = $@"SELECT t2.FSTATUS FROM T_PRD_MOENTRY t1 INNER JOIN T_PRD_MOENTRY_A t2 ON t1.FENTRYID=t2.FENTRYID
INNER JOIN T_PRD_MO t3 ON t1.FID=t3.FID WHERE t1.FENTRYID={request.MoEntryId}";
                var modata = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
                if (modata.Count <= 0)
                {
                    throw new Exception("未能找到生产订单！");
                }
                if (Convert.ToInt32(modata.First()["FSTATUS"]) > 5)
                {
                    throw new Exception("已结案的订单无法再变更车间数据！");
                }
                //查询车间是否存在
                sSql = $"SELECT FDEPTID FROM dbo.T_BD_DEPARTMENT WHERE FNUMBER='{request.WorkShopNumber}'";
                var deptdata = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
                if (deptdata.Count <= 0)
                {
                    throw new Exception("未能找到需要变更的生产车间！");
                }
                var workshopid = deptdata.First()["FDEPTID"];
                //更新生产订单车间
                //sSql = "UPDATE T_PRD_MO SET FWORKSHOPID=1 WHERE FID=12";
                //DBUtils.Execute(ctx,sSql);
                //更新生产订单明细车间
                sSql = $"/*dialect*/UPDATE T_PRD_MOENTRY SET FWORKSHOPID={workshopid} WHERE FENTRYID={request.MoEntryId}";
                DBServiceHelper.Execute(ctx, sSql);
                //更新生产领料单车间
                sSql = $"/*dialect*/UPDATE T_PRD_PICKMTRLDATA SET FWORKSHOPID={workshopid} WHERE FMOENTRYID={request.MoEntryId}";
                DBServiceHelper.Execute(ctx, sSql);
                sSql = $"/*dialect*/UPDATE T_PRD_PPBOM SET FWORKSHOPID={workshopid} WHERE FMOENTRYID={request.MoEntryId}";
                DBServiceHelper.Execute(ctx, sSql);
                cope.Complete();
            }

            return response;
        }
        public ResponseMessage<dynamic> PrdPPBomChange(Context ctx, MesPrd_PPBOMRequest request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            IOperationResult oper = new OperationResult();
            response.Code = ResponseCode.Success;
            response.Message = "创建成功";
            using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
            {
                string sSql = "SELECT FID FROM dbo.T_PRD_PPBOM WHERE FMOID=@FMOID";
                var ppid = DBServiceHelper.ExecuteScalar<long>(ctx, sSql, 0, new SqlParam("@FMOID", KDDbType.Int64, request.MoId));
                sSql = "SELECT FENTRYID FROM dbo.T_PRD_PPBOMENTRY WHERE FID=@FID";
                var ppeid = DBServiceHelper.ExecuteScalar<long>(ctx, sSql, 0, new SqlParam("@FID", KDDbType.Int64, ppid));
                List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();

                var row = new ListSelectedRow(Convert.ToString(ppid), Convert.ToString(ppeid), 0, "PRD_PPBOM");
                row.EntryEntityKey = "FEntity"; //这里最容易忘记加，是重点的重点
                selectedRows.Add(row);

                var result = this.PrdPPBOMPushChange(ctx, "PRD_PPBOM2PPBOMCHANGE", request.Entry, selectedRows);
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
        private IOperationResult PrdPPBOMPushChange(Context ctx, string ConvertRule, List<PPBomEntry> pushEntity, List<ListSelectedRow> listSelectedRow)
        {
            MymoooBusinessDataService service = new MymoooBusinessDataService();
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
            pushArgs.TargetBillTypeId = "6a4321986af642c895449a7671e4d10f";
            // 自动下推，无需验证用户功能权限
            pushOption.SetVariableValue(BOSConst.CST_ConvertValidatePermission, true);
            // 设置是否整单下推
            //pushOption.SetVariableValue(ConvertConst., false);

            var convertResult = ConvertServiceHelper.Push(ctx, pushArgs, pushOption);//调用下推接口
            var targetObjs = (from p in convertResult.TargetDataEntities select p.DataEntity).ToArray();//得到目标单据数据包
            foreach (DynamicObject targetObj in targetObjs)
            {
                foreach (var item in pushEntity)
                {
                    var targeEntry = ((DynamicObjectCollection)targetObj["PPBomEntry"])
                    .Where(x => Convert.ToInt32(x["ChangeType"]) == 3
                    && Convert.ToInt64(x["MoEntryId"]) == item.MoEntryId).First();

                    targeEntry["Numerator"] = item.Numerator;
                    targeEntry["Denominator"] = item.Denominator;
                    targeEntry["FixScrapQty"] = item.FFixScrapQty;
                    targeEntry["ScrapRate"] = item.ScrapRate;
                }
            }
            var targetBInfo = this.GetBusinessInfo(ctx, pushArgs.ConvertRule.TargetFormId, null);
            //对转换结果进行处理
            //1. 直接调用保存接口，对数据进行保存
            return service.SaveAndAuditBill(ctx, targetBInfo, targetObjs);
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
        /// <summary>
        /// 生产关闭
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="mostatus"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> StatusMoOrderAction(Context ctx, PrdMoStatus mostatus)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            var operateOption = OperateOption.Create();
            operateOption.SetIgnoreWarning(true);
            FormMetadata meta = MetaDataServiceHelper.Load(ctx, "PRD_MO") as FormMetadata;
            object[] ids = mostatus.MoEntryId.ToArray<object>();
            List<KeyValuePair<object, object>> pkEntryIds = ids.Select((object x) => new KeyValuePair<object, object>(mostatus.MoId, x)).ToList();
            try
            {
                SetStatusService setStatusService = new SetStatusService();
                IOperationResult oper2 = new OperationResult();
                switch (mostatus.Type)
                {
                    case 1:
                        //执行至结案
                        oper2 = setStatusService.SetBillStatus(ctx, meta.BusinessInfo, pkEntryIds, null, "ToClose", operateOption);
                        break;
                    case 2:
                        //强制结案
                        oper2 = setStatusService.SetBillStatus(ctx, meta.BusinessInfo, pkEntryIds, null, "ForceClose", operateOption);
                        break;
                }
                if (oper2.ValidationErrors.Count > 0)
                {
                    response.Message = string.Join(";", oper2.ValidationErrors.Select(p => p.Message));
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.Exception;
                response.Message = ex.Message;
                return response;
            }
            response.Code = ResponseCode.Success;
            response.Message = "操作成功";
            return response;

        }
    }
}
