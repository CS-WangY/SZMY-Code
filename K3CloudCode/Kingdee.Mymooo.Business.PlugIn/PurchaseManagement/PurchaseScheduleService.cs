using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Log;
using Kingdee.BOS.Core;
using Kingdee.Mymooo.Business.PlugIn.SalesManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.BOS;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.Const;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.BOS.Util;
using Kingdee.BOS.Core.Interaction;

namespace Kingdee.Mymooo.Business.PlugIn.PurchaseManagement
{
    public class Log
    {
        public static void LogWrite(Context ctx, List<LogObject> logs)
        {
            new Kingdee.BOS.App.LogService.Log().BatchWriteLog(ctx, logs);
        }
    }
    /// <summary>
    /// 采购入库单生成应付单
    /// </summary>
    public class InStockScheduleService : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            var context = LoginServiceUtils.BackgroundLogin(ctx);
            string sSql = $@"SELECT t2.FBILLNO,t1.FID,t1.FENTRYID,t2.FPURCHASEORGID FROM T_STK_INSTOCKENTRY t1
                        INNER JOIN T_STK_INSTOCK t2 ON t1.FID=t2.FID
                        INNER JOIN t_STK_InStockFin t3 ON t3.FID=t2.FID
						INNER JOIN T_BD_PaymentCondition t4 ON t3.FPAYCONDITIONID=t4.FID
                        WHERE t2.FDOCUMENTSTATUS='C' AND t4.FNUMBER IN ('001','501','DAP')
                        AND NOT EXISTS(SELECT * FROM T_AP_PAYABLE_LK 
                        WHERE FSTABLENAME='T_STK_INSTOCKENTRY' AND FSBILLID=t1.FID AND FSID=t1.FENTRYID)";
            var reader = DBServiceHelper.ExecuteDynamicObject(context, sSql);
            if (!(reader is null))
            {
                IEnumerable<IGrouping<long, DynamicObject>> enumerable = from g in reader
                                                                         group g by Convert.ToInt64(g["FPURCHASEORGID"]);
                foreach (IGrouping<long, DynamicObject> item in enumerable)
                {
                    var logs = new List<LogObject>();
                    try
                    {
                        var orgid = item.Key;
                        foreach (dynamic obj in item)
                        {
                            List<SalesOrderPushEntity> entry = new List<SalesOrderPushEntity>();
                            entry.Add(new SalesOrderPushEntity
                            {
                                FID = Convert.ToInt64(obj["FID"]),
                                FEntryID = Convert.ToInt64(obj["FEntryID"])
                            });
                            BillPushService.ToReceivable(context, entry, orgid,
                                "AP_InStockToPayableMap",
                                "a83c007f22414b399b0ee9b9aafc75f9",
                                "STK_InStock",
                                "FInStockEntry"
                                );
                        }
                    }
                    catch (Exception ex)
                    {
                        var log = new LogObject();
                        log.pkValue = "0";
                        log.Description = ex.Message;
                        log.OperateName = "采购入库生成应付";
                        log.ObjectTypeId = "BOS_SCHEDULETYPE";
                        log.SubSystemId = "BOS";
                        log.Environment = OperatingEnvironment.BizOperate;
                        logs.Add(log);
                        Log.LogWrite(context, logs);
                    }
                }
            }

        }
    }
    /// <summary>
    /// 采购退料生成应付单
    /// </summary>
    public class Pur_MrbScheduleService : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            var context = LoginServiceUtils.BackgroundLogin(ctx);
            string sSql = $@"SELECT t2.FBILLNO,t1.FID,t1.FENTRYID,t2.FPURCHASEORGID,t2.FMRTYPE FROM T_PUR_MRBENTRY t1
                        INNER JOIN T_PUR_MRBENTRY_F tf ON t1.FENTRYID=tf.FENTRYID
                        INNER JOIN T_PUR_MRB t2 ON t1.FID=t2.FID
                        INNER JOIN T_PUR_MRBFIN t3 ON t2.FID=t3.FID
                        INNER JOIN T_BD_PaymentCondition t4 ON t3.FPAYCONDITIONID=t4.FID
                        WHERE t2.FMRTYPE='B' AND t2.FDOCUMENTSTATUS='C' AND t4.FNUMBER IN ('001','501','DAP')
                        AND NOT EXISTS(SELECT * FROM T_AP_PAYABLE_LK 
                        WHERE FSTABLENAME='T_PUR_MRBENTRY' AND FSBILLID=t1.FID AND FSID=t1.FENTRYID)";
            var reader = DBServiceHelper.ExecuteDynamicObject(context, sSql);
            if (!(reader is null))
            {
                IEnumerable<IGrouping<long, DynamicObject>> enumerable = from g in reader
                                                                         group g by Convert.ToInt64(g["FPURCHASEORGID"]);
                foreach (IGrouping<long, DynamicObject> item in enumerable)
                {
                    var logs = new List<LogObject>();
                    try
                    {
                        var orgid = item.Key;
                        foreach (dynamic obj in item)
                        {
                            List<SalesOrderPushEntity> entry = new List<SalesOrderPushEntity>();
                            entry.Add(new SalesOrderPushEntity
                            {
                                FID = Convert.ToInt64(obj["FID"]),
                                FEntryID = Convert.ToInt64(obj["FEntryID"])
                            });
                            BillPushService.ToReceivable(context, entry, orgid,
                                "AP_MRBToPayableMap",
                                "a83c007f22414b399b0ee9b9aafc75f9",
                                "PUR_MRB",
                                "FPURMRBENTRY"
                                );
                        }
                    }
                    catch (Exception ex)
                    {
                        var log = new LogObject();
                        log.pkValue = "0";
                        log.Description = ex.Message;
                        log.OperateName = "采购退料生成应付";
                        log.ObjectTypeId = "BOS_SCHEDULETYPE";
                        log.SubSystemId = "BOS";
                        log.Environment = OperatingEnvironment.BizOperate;
                        logs.Add(log);
                        Log.LogWrite(context, logs);
                    }
                }
            }

        }
    }

    public static class BillPushService
    {
        public static IOperationResult ToReceivable(Context ctx, List<SalesOrderPushEntity> entry, long TargetOrgId, string ConvertRule, string billType, string SelBillType, string EntryID)
        {
            List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
            StockPushEntity push = new StockPushEntity();
            foreach (var item in entry)
            {
                var row = new ListSelectedRow(item.FID.ToString(), item.FEntryID.ToString(), 0, SelBillType);
                row.EntryEntityKey = EntryID; //这里最容易忘记加，是重点的重点
                selectedRows.Add(row);
            }

            push.listSelectedRow = selectedRows;
            push.ConvertRule = ConvertRule;
            push.TargetOrgId = TargetOrgId;
            push.TargetBillTypeId = billType;

            return BillPush(ctx, push);
        }
        public static IOperationResult BillPush(Context ctx, StockPushEntity pushEntity)
        {
            //得到转换规则
            var convertRule = GetConvertRule(ctx, pushEntity.ConvertRule);
            OperateOption pushOption = OperateOption.Create();//操作选项
                                                              //构建下推参数
                                                              //pushOption.SetVariableValue(ConvertConst.SelectByBillId, pushEntity.SetVariableValue);
                                                              //单据下推参数
            PushArgs pushArgs = new PushArgs(convertRule, pushEntity.listSelectedRow.ToArray());
            //目标单据主组织，可选参数，基础资料隔离，给没有住组织的目标数据包赋值，取当前登录组织即可
            pushArgs.TargetOrgId = pushEntity.TargetOrgId;
            //目标单据类型，必填参数，除非下游单据类型不是必填的，对源单数据进行过滤，给目标单赋值
            pushArgs.TargetBillTypeId = pushEntity.TargetBillTypeId;
            // 自动下推，无需验证用户功能权限
            pushOption.SetVariableValue(BOSConst.CST_ConvertValidatePermission, true);
            // 设置是否整单下推
            pushOption.SetVariableValue(ConvertConst.SelectByBillId, false);

            var convertResult = ConvertServiceHelper.Push(ctx, pushArgs, pushOption);//调用下推接口
            var targetObjs = (from p in convertResult.TargetDataEntities select p.DataEntity).ToArray();//得到目标单据数据包
            var targetBInfo = GetBusinessInfo(ctx, pushArgs.ConvertRule.TargetFormId, null);
            //1. 直接调用保存接口，对数据进行保存
            return SaveTargetBill(ctx, targetBInfo, targetObjs);
        }
        /// <summary>
        /// 保存目标单据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="targetBusinessInfo"></param>
        /// <param name="targetBillObjs"></param>
        private static IOperationResult SaveTargetBill(Context ctx, BusinessInfo targetBusinessInfo, DynamicObject[] targetBillObjs)
        {
            OperateOption saveOption = OperateOption.Create();
            saveOption.SetIgnoreWarning(true);
            saveOption.SetIgnoreInteractionFlag(true);
            SaveService saveService = new SaveService();
            var saveResult = saveService.SaveAndAudit(ctx, targetBusinessInfo, targetBillObjs, saveOption);
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
        // 得到表单元数据
        private static BusinessInfo GetBusinessInfo(Context ctx, string formId, FormMetadata metaData = null)
        {
            if (metaData != null) return metaData.BusinessInfo;
            metaData = FormMetaDataCache.GetCachedFormMetaData(ctx, formId);
            return metaData.BusinessInfo;
        }
        //得到转换规则
        private static ConvertRuleElement GetConvertRule(Context ctx, string convertRuleId)
        {
            var convertRuleMeta = ConvertServiceHelper.GetConvertRule(ctx, convertRuleId);
            return convertRuleMeta.Rule;
        }

    }
}
