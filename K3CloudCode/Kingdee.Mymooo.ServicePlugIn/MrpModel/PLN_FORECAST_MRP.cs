using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Const;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm;
 using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.K3.MFG.Contracts.PLN;
using Kingdee.K3.MFG.PLN.App.MrpModel;
using Kingdee.Mymooo.Core.StockManagement;
using static Kingdee.K3.Core.MFG.EnumConst.Enums.PLN_MrpModel;
using Kingdee.BOS;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Core.List;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.K3.Core.MFG.PLN.Reserved.ReserveArgs;
using Kingdee.K3.Core.MFG.PLN.Reserved;
using Kingdee.K3.MFG.App;
using Kingdee.K3.MFG.Contracts.PLN.Reserve;
using Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice;
using static Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice.SubmitLock;
using Kingdee.K3.Core.MFG.EnumConst;
using Kingdee.BOS.Log;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.PLN;
using Kingdee.K3.Core.MFG.Utils;
using System.Web.UI.WebControls;
using static Kingdee.K3.FIN.Core.Object.AP_Matck;
using System.Web.Routing;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.K3.Core.MFG.Common;

namespace Kingdee.Mymooo.ServicePlugIn.MrpModel
{
    [Description("MRP运算插件-预测冲销预留转移"), HotUpdate]
    public class PLN_FORECAST_MRP : AbstractMrpLogicUnit
    {
        public override string Description
        {
            get { return "二开自定义逻辑"; }
        }
        protected override void AfterExecuteLogicUnit()
        {
            //本示例实现在MRP收尾环节自动收集物料供需数据表，本逻辑单元须在MRP各标准逻辑单元之后执行
            //OperateOption option = OperateOption.Create();
            //option.SetVariableValue("ComputeId", this.MrpGlobalDataContext.ComputeId);
            //TaskProxyItem taskProxyItem = new TaskProxyItem();
            //option.SetVariableValue("TaskId", taskProxyItem.TaskId);
            //AppServiceContext.GetMFGService<IMtrlDSDataSumCalcService>().Execute(this.Context, option);
            WriteInfoLog("二开关闭预测自动关闭的下游单据--开始", 1, 83);
            Logger.Info("MRP", "开始关闭预测单");
            #region 注释2024.07.11
            //            if (this.MrpGlobalDataContext.IsWriteOff)
            //            {
            //                try
            //                {
            //                    //获取MRP计划方案下的预测冲销方案ID
            //                    var schemeId = Convert.ToInt64(((DynamicObjectCollection)this.MrpGlobalDataContext.SchemaData["OtherOption"])[0]["WriteOffScheme_Id"]);
            //                    string relationResultTableName = NewWriteOffServiceHelper.GetRelationResultTableName(this.Context, schemeId);
            //                    string sSql = @"SELECT t1.FID FROM T_PLN_REQUIREMENTORDER t1
            //                                INNER JOIN T_PLN_FORECASTENTRY t2 ON t1.FSALEORDERENTRYID=t2.FENTRYID
            //                                AND t1.FDEMANDTYPE=2 AND t1.FISCLOSED='A'
            //                                WHERE t2.FENDDATE<GETDATE()";
            //                    var plndatas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
            //                    foreach (var pln in plndatas)
            //                    {
            //                        var plnid = Convert.ToInt64(pln["FID"]);
            //                        var view = FormMetadataUtils.CreateBillView(this.Context, "PLN_REQUIREMENTORDER", plnid);
            //                        using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
            //                        {
            //                            Logger.Info("MRP", $"关闭超期的预测单相关组织间需求单{plnid}");
            //                            SetStatusService setStatusService = new SetStatusService();
            //                            var operateOption = OperateOption.Create();
            //                            operateOption.SetIgnoreWarning(true);
            //                            List<KeyValuePair<object, object>> keyValuePairs = new List<KeyValuePair<object, object>>();
            //                            keyValuePairs.Add(new KeyValuePair<object, object>(plnid, ""));
            //                            setStatusService.SetBillStatus(this.Context, view.BusinessInfo, keyValuePairs, null, "HandClose", operateOption);
            //                            cope.Complete();
            //                        }
            //                        //清除释放网控
            //                        view.CommitNetworkCtrl();
            //                        view.InvokeFormOperation(FormOperationEnum.Close);
            //                        view.Close();
            //                    }

            //                    var plnList = this.MrpGlobalDataContext.ReleaseRowInfos.Where(x => x.FormID.EqualsIgnoreCase("PLN_REQUIREMENTORDER")).Distinct();
            //                    Logger.Info("MRP", $"本次共有{plnList.Count()}条记录参与转移");
            //                    foreach (var item in plnList)
            //                    {
            //                        decimal RoQty = 0;
            //                        //获取需求分配的销售订单
            //                        sSql = $@"SELECT t1.FID,t1.FENTRYID,t1.FBASEQTY,t2.FDEMANDFORMID,t2.FDEMANDINTERID,t2.FDEMANDENTRYID,t2.FDEMANDBILLNO
            //                            ,t4.FSALEORDERENTRYID,te.FSEQ
            //                            FROM T_PLN_RESERVELINKENTRY t1
            //                            INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
            //                            INNER JOIN T_SAL_ORDERENTRY te ON t2.FDEMANDENTRYID=te.FENTRYID
            //                            INNER JOIN T_SAL_ORDERENTRYDELIPLAN t3 ON t2.FDEMANDENTRYID=t3.FENTRYID
            //                            INNER JOIN T_PLN_REQUIREMENTORDER t4 ON t1.FSUPPLYINTERID=t4.FID
            //                            WHERE t1.FSUPPLYINTERID='{item.InterID}' AND t2.FDEMANDFORMID='SAL_SaleOrder' AND t4.FDEMANDTYPE=2
            //                            ORDER BY t3.FPLANDELIVERYDATE";
            //                        var saldatas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
            //                        foreach (var saldata in saldatas)
            //                        {
            //                            long rid = Convert.ToInt64(saldata["FID"]);
            //                            long reid = Convert.ToInt64(saldata["FENTRYID"]);
            //                            decimal baseqty = Convert.ToDecimal(saldata["FBASEQTY"]);
            //                            long salid = Convert.ToInt64(saldata["FDEMANDINTERID"]);
            //                            long salentryid = Convert.ToInt64(saldata["FDEMANDENTRYID"]);
            //                            string salBillno = Convert.ToString(saldata["FDEMANDBILLNO"]);

            //                            long salSeq = Convert.ToInt64(saldata["FSEQ"]);
            //                            long foentryid = Convert.ToInt64(saldata["FSALEORDERENTRYID"]);

            //                            RoQty = baseqty;
            //                            Logger.Info("MRP", $"获取销售订单{salBillno}信息");

            //                            //删除冲销预留记录，创建销售订单新组织间需求单
            //                            Logger.Info("MRP", $"删除销售订单{salBillno}原本冲销预留信息");
            //                            List<string> sqllist = new List<string>{
            //                            $"DELETE T_PLN_RESERVELINKENTRY WHERE FID={rid} AND FSUPPLYINTERID={item.InterID}"
            //                            };
            //                            DBUtils.ExecuteBatch(this.Context, sqllist);

            //                            Logger.Info("MRP", $"销售订单{salBillno}下推产生新的组织间需求单");
            //                            var saveResult = PushReq(this.Context, salid.ToString(), salentryid.ToString(), baseqty);
            //                            long resid = 0;
            //                            if (saveResult.IsSuccess)
            //                            {
            //                                resid = Convert.ToInt64(saveResult.OperateResult[0].PKValue);
            //                                Logger.Info("MRP", $"{saveResult.OperateResult[0].Message}");
            //                            }
            //                            else
            //                            {
            //                                Logger.Info("MRP", $"{string.Join(";", saveResult.ValidationErrors.Select(p => p.Message))}");
            //                            }
            //                            //获取预测单预留信息
            //                            Logger.Info("MRP", $"获取预测单预留信息FDEMANDINTERID=>{item.InterID}");
            //                            sSql = $@"SELECT FID
            //                                FROM T_PLN_RESERVELINK
            //                                WHERE FDEMANDINTERID='{item.InterID}'";
            //                            var foid = DBUtils.ExecuteScalar<long>(this.Context, sSql, 0);
            //                            if (foid != 0)
            //                            {
            //                                //加载预测单预留单据信息
            //                                var billView = FormMetadataUtils.CreateBillView(this.Context, "PLN_RESERVELINK", foid);
            //                                var entryView = billView.Model.DataObject["Entity"] as DynamicObjectCollection;
            //                                List<resEntryInfo> reslist = new List<resEntryInfo>();
            //                                foreach (var resitem in entryView.OrderByDescending(b => b["SupplyPriority"]))
            //                                {
            //                                    if (baseqty > 0)
            //                                    {
            //                                        resEntryInfo entryInfo = new resEntryInfo();
            //                                        decimal qty = Convert.ToDecimal(resitem["BaseSupplyQty"]);

            //                                        entryInfo.SupplyFormID = Convert.ToString(resitem["SupplyFormID_Id"]);
            //                                        entryInfo.SupplyInterID = Convert.ToString(resitem["SupplyInterID"]);
            //                                        entryInfo.SupplyEntryID = Convert.ToString(resitem["SupplyEntryID"]);
            //                                        entryInfo.SupplyBillNO = Convert.ToString(resitem["SupplyBillNo"]);
            //                                        entryInfo.SupplyMaterialID = Convert.ToString(resitem["SupplyMaterialID_Id"]);
            //                                        entryInfo.SupplyOrgID = Convert.ToString(resitem["SupplyOrgId_Id"]);
            //                                        entryInfo.SupplyStockID = Convert.ToString(resitem["SupplyStockID_Id"]);
            //                                        entryInfo.BaseSupplyUnitID = Convert.ToString(resitem["BaseSupplyUnitID_Id"]);

            //                                        if (qty - baseqty >= 0)
            //                                        {
            //                                            resitem["BaseSupplyQty"] = qty - baseqty;
            //                                            entryInfo.BaseActSupplyQty = baseqty;
            //                                            Logger.Info("MRP", $"{entryInfo.SupplyFormID}需求数量{baseqty}可转移数量{qty},本次转移数量{entryInfo.BaseActSupplyQty}");
            //                                            baseqty = 0;

            //                                        }
            //                                        else
            //                                        {
            //                                            resitem["BaseSupplyQty"] = 0;
            //                                            entryInfo.BaseActSupplyQty = qty;
            //                                            Logger.Info("MRP", $"{entryInfo.SupplyFormID}需求数量{baseqty}可转移数量{qty},本次转移数量{entryInfo.BaseActSupplyQty}");
            //                                            baseqty = baseqty - qty;

            //                                        }

            //                                        entryInfo.IntsupplyID = Convert.ToInt64(resitem["IntSupplyId"]);
            //                                        entryInfo.IntsupplyEntryID = Convert.ToInt64(resitem["IntSuppyEntryId"]);

            //                                        reslist.Add(entryInfo);
            //                                    }
            //                                }
            //                                using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
            //                                {
            //                                    Logger.Info("MRP", $"保存预测单预留信息{foid}");
            //                                    var rowcount = billView.Model.GetEntryRowCount("FEntity");
            //                                    for (int i = rowcount; i >= 0; i--)
            //                                    {
            //                                        var eqty = billView.Model.GetValue<decimal>("FBaseSupplyQty", i, 0);
            //                                        if (eqty <= 0)
            //                                        {
            //                                            billView.Model.DeleteEntryRow("FEntity", i);
            //                                        }
            //                                    }
            //                                    List<DynamicObject> list = new List<DynamicObject>();
            //                                    list.Add(billView.Model.DataObject);
            //                                    saveResult = SaveBill(this.Context, billView.BusinessInfo, list.ToArray());
            //                                    if (saveResult.IsSuccess)
            //                                    {
            //                                        Logger.Info("MRP", $"{saveResult.OperateResult[0].PKValue}{saveResult.OperateResult[0].Message}");
            //                                    }
            //                                    else
            //                                    {
            //                                        Logger.Info("MRP", $"{string.Join(";", saveResult.ValidationErrors.Select(p => p.Message))}");
            //                                    }
            //                                    Logger.Info("MRP", $"保存销售订单预留信息{resid}");
            //                                    Logger.Info("MRP", $"{JsonConvertUtils.SerializeObject(reslist)}");
            //                                    CreateReserveLink(resid, reslist);


            //                                    cope.Complete();
            //                                }
            //                            }

            //                            //Logger.Info("MRP", $"修改预测单执行数量{foentryid}");
            //                            //sSql = $@"/*dialect*/UPDATE t1 SET t1.FWRITEOFFQTY=t1.FWRITEOFFQTY+{RoQty}
            //                            //        ,t1.FBASEWRITEOFFQTY=t1.FBASEWRITEOFFQTY+{RoQty}
            //                            //        --SELECT * 
            //                            //        FROM T_PLN_FORECASTENTRY t1
            //                            //        INNER JOIN T_PLN_FORECAST t2 ON t1.FID=t2.FID
            //                            //        INNER JOIN (
            //                            //        SELECT t0.FSALEORDERNO,t0.FSALEORDERENTRYSEQ,t0_A.FFORECASTBILLNO,t0_A.FFORECASTENTRYSEQ
            //                            //        FROM {relationResultTableName} t0 LEFT OUTER JOIN {relationResultTableName}_A t0_A ON t0.FID = t0_A.FID
            //                            //        ) t3 ON t2.FBILLNO=t3.FFORECASTBILLNO AND t1.FSEQ=t3.FFORECASTENTRYSEQ
            //                            //        WHERE t3.FSALEORDERNO='{salBillno}' AND t3.FSALEORDERENTRYSEQ='{salSeq}'";
            //                            //DBUtils.Execute(this.Context, sSql);

            //                            //获取组织间需求单对象
            //                            var view = FormMetadataUtils.CreateBillView(this.Context, "PLN_REQUIREMENTORDER", item.InterID);
            //                            using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
            //                            {
            //                                Logger.Info("MRP", $"减少组织间需求单剩余需求数量{item.InterID}");
            //                                var OldRoQty = Convert.ToDecimal(view.Model.DataObject["ReMainQty"]);
            //                                view.Model.DataObject["ReMainQty"] = OldRoQty - RoQty;
            //                                view.Model.DataObject["ReMainBaseQty"] = OldRoQty - RoQty;
            //                                var oper = SaveBill(this.Context, view.BusinessInfo, new DynamicObject[] { view.Model.DataObject });
            //                                //如果需求单扣减数量为零则关闭需求单
            //                                if (OldRoQty - RoQty <= 0)
            //                                {
            //                                    Logger.Info("MRP", $"组织间需求单剩余需求数量为0，关闭订单{item.InterID}");
            //                                    SetStatusService setStatusService = new SetStatusService();
            //                                    var operateOption = OperateOption.Create();
            //                                    operateOption.SetIgnoreWarning(true);
            //                                    List<KeyValuePair<object, object>> keyValuePairs = new List<KeyValuePair<object, object>>();
            //                                    keyValuePairs.Add(new KeyValuePair<object, object>(item.InterID, ""));
            //                                    setStatusService.SetBillStatus(this.Context, view.BusinessInfo, keyValuePairs, null, "HandClose", operateOption);
            //                                }
            //                                cope.Complete();
            //                            }
            //                            //清除释放网控
            //                            view.CommitNetworkCtrl();
            //                            view.InvokeFormOperation(FormOperationEnum.Close);
            //                            view.Close();
            //                        }
            //                    }
            //                    Logger.Info("MRP", $"修改预测单执行数量");
            //                    sSql = $@"/*dialect*/UPDATE t1 SET t1.FWRITEOFFQTY=t3.FWRITEOFFQTY
            //,t1.FBASEWRITEOFFQTY=t3.FWRITEOFFQTY
            //FROM T_PLN_FORECASTENTRY t1
            //INNER JOIN T_PLN_FORECAST t2 ON t1.FID=t2.FID
            //INNER JOIN (
            //SELECT t3.FRELATIONINTERID,t3.FRELATIONENTRYID,SUM(t3.FWRITEOFFQTY) AS FWRITEOFFQTY FROM {relationResultTableName} t3
            //GROUP BY t3.FRELATIONINTERID,t3.FRELATIONENTRYID
            //) t3 ON t2.FID=t3.FRELATIONINTERID AND t1.FENTRYID=t3.FRELATIONENTRYID AND t3.FWRITEOFFQTY>0";
            //                    //sSql = "/*dialect*/UPDATE T_PLN_FORECASTENTRY SET FWRITEOFFQTY=FORDERQTY,FBASEWRITEOFFQTY=FBASEORDERQTY";
            //                    //DBUtils.Execute(this.Context, sSql);
            //                    sSql = $@"/*dialect*/UPDATE T_PLN_FORECASTENTRY SET FCLOSESTATUS='D'
            //                            --SELECT * 
            //                            FROM T_PLN_FORECASTENTRY
            //                            WHERE FORDERQTY>=FQTY";
            //                    DBUtils.Execute(this.Context, sSql);
            //                }
            //                catch (Exception e)
            //                {
            //                    Logger.Info("MRP", e.ToString());
            //                }
            //            }
            #endregion
            var sSql = @"SELECT t1.FID FROM dbo.T_PLN_REQUIREMENTORDER t1
INNER JOIN T_PLN_FORECASTENTRY t2 ON t1.FSALEORDERENTRYID=t2.FENTRYID AND t1.FDEMANDTYPE=2
WHERE t1.FISCLOSED='A' AND t2.FENDDATE<GETDATE()";
            var fodatas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
            if (fodatas.Count > 0)
            {
                SetStatusService setStatusService = new SetStatusService();
                var operateOption = OperateOption.Create();
                operateOption.SetIgnoreWarning(true);
                List<KeyValuePair<object, object>> keyValuePairs = new List<KeyValuePair<object, object>>();
                foreach (var foitem in fodatas)
                {
                    keyValuePairs.Add(new KeyValuePair<object, object>(foitem["FID"], ""));
                }
                if (keyValuePairs.Count > 0)
                {
                    var view = FormMetadataUtils.CreateBillView(this.Context, "PLN_REQUIREMENTORDER");
                    setStatusService.SetBillStatus(this.Context, view.BusinessInfo, keyValuePairs, null, "HandClose", operateOption);
                }
            }
            sSql = @"SELECT t1.FID FROM T_PLN_PLANORDER_B t1
INNER JOIN T_PLN_PLANORDER t3 ON t1.FID=t3.FID
INNER JOIN T_PLN_FORECASTENTRY t2 ON t1.FSALEORDERENTRYID=t2.FENTRYID AND t1.FDEMANDTYPE=2
WHERE t3.FReleaseStatus IN (0,1,2) AND t2.FENDDATE<GETDATE()";
            var plndatas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
            if (plndatas.Count > 0)
            {
                SetStatusService setStatusService = new SetStatusService();
                var operateOption = OperateOption.Create();
                operateOption.SetIgnoreWarning(true);
                List<KeyValuePair<object, object>> keyValuePairs = new List<KeyValuePair<object, object>>();
                foreach (var plnitem in plndatas)
                {
                    keyValuePairs.Add(new KeyValuePair<object, object>(plnitem["FID"], ""));
                }
                if (keyValuePairs.Count > 0)
                {
                    var view = FormMetadataUtils.CreateBillView(this.Context, "PLN_PLANORDER");
                    setStatusService.SetBillStatus(this.Context, view.BusinessInfo, keyValuePairs, null, "HandClose", operateOption);
                }
            }
            Logger.Info("MRP", "结束关闭预测单");
            WriteInfoLog("二开关闭预测自动关闭的下游单据--结束", 1, 83);
        }
        public void WriteInfoLog(string msg, int logclass, int logDetailclass)
        {
            base.ExtendServiceProvider.GetService<IMrpLogService>().WriteLog(
                msg, (Enu_MrpLogClass)logclass, (Enu_MrpLogDetailClass)logDetailclass, false);
        }

        public IOperationResult PushReq(Context ctx, string billid, string entryid, decimal salqty)
        {
            List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
            StockPushEntity push = new StockPushEntity();
            var row = new ListSelectedRow(billid, entryid, 0, "SAL_SaleOrder");
            row.EntryEntityKey = "FSaleOrderEntry"; //这里最容易忘记加，是重点的重点
            selectedRows.Add(row);

            push.listSelectedRow = selectedRows;
            push.ConvertRule = "PENY_SalOrder_REQUIREMENTORDER";
            //push.TargetOrgId = Convert.ToInt64(item.TargetOrgId);
            //push.TargetOrgId = Convert.ToInt64(allocates.TargetOrgId);

            var result = this.BillPush(ctx, push, salqty);
            return result;
        }
        public IOperationResult BillPush(Context ctx, StockPushEntity pushEntity, decimal salqty)
        {
            //得到转换规则
            var convertRule = this.GetConvertRule(ctx, pushEntity.ConvertRule);
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
            //pushOption.SetVariableValue(ConvertConst., false);

            var convertResult = ConvertServiceHelper.Push(ctx, pushArgs, pushOption);//调用下推接口
            var targetObjs = (from p in convertResult.TargetDataEntities select p.DataEntity).ToArray();//得到目标单据数据包
            foreach (DynamicObject targeEntry in targetObjs)
            {
                var mid = Convert.ToInt64(targeEntry["MaterialId_Id"]);
                var unitid = Convert.ToInt64(targeEntry["UnitID_Id"]);
                var baseunitid = Convert.ToInt64(targeEntry["BaseUnitId_Id"]);
                var baseqty = salqty;
                IConvertService convertService = Kingdee.BOS.App.ServiceHelper.GetService<IConvertService>();
                if (unitid != baseunitid)
                {
                    baseqty = convertService.GetUnitTransQty(this.Context, mid, unitid, baseunitid, salqty);
                }
                targeEntry["DemandQty"] = salqty;
                targeEntry["BaseDemandQty"] = baseqty;
                targeEntry["FirmQty"] = salqty;
                targeEntry["BaseFirmQty"] = baseqty;
                targeEntry["ReMainQty"] = salqty;
                targeEntry["ReMainBaseQty"] = baseqty;
            }
            var targetBInfo = this.GetBusinessInfo(ctx, pushArgs.ConvertRule.TargetFormId, null);
            //对转换结果进行处理
            //1. 直接调用保存接口，对数据进行保存
            return this.SaveTargetBill(ctx, targetBInfo, targetObjs);
        }

        /// <summary>
        /// 保存目标单据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="targetBusinessInfo"></param>
        /// <param name="targetBillObjs"></param>
        private IOperationResult SaveTargetBill(Context ctx, BusinessInfo targetBusinessInfo, DynamicObject[] targetBillObjs)
        {
            OperateOption saveOption = OperateOption.Create();
            saveOption.SetIgnoreWarning(true);
            saveOption.SetIgnoreInteractionFlag(true);
            saveOption.AddInteractionFlag(K3.Core.SCM.SCMConst.MinusCheckSensor);
            //保存
            SaveService saveService = new SaveService();
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
        // 得到表单元数据
        private BusinessInfo GetBusinessInfo(Context ctx, string formId, FormMetadata metaData = null)
        {
            if (metaData != null) return metaData.BusinessInfo;
            metaData = FormMetaDataCache.GetCachedFormMetaData(ctx, formId);
            return metaData.BusinessInfo;
        }
        //得到转换规则
        private ConvertRuleElement GetConvertRule(Context ctx, string convertRuleId)
        {
            var convertRuleMeta = ConvertServiceHelper.GetConvertRule(ctx, convertRuleId);
            return convertRuleMeta.Rule;
        }

        private IOperationResult SaveBill(Context ctx, BusinessInfo businessInfo, DynamicObject[] dynamicObjects)
        {
            SaveService saveService = new SaveService();
            IOperationResult operationResult;
            var operateOption = OperateOption.Create();
            operateOption.SetIgnoreWarning(true);
            operationResult = saveService.Save(ctx, businessInfo, dynamicObjects, operateOption);
            return operationResult;
        }

        public void CreateReserveLink(long reqid, List<resEntryInfo> resinfo)
        {
            //取组织间需求单信息
            string sSql = $@"SELECT FFORMID AS DemandFormID,fid AS DemandInterID,FBILLNO AS DemandBillNO
            ,'SAL_SaleOrder' AS SrcDemandFormId,FSALEORDERID AS SrcDemandInterId,FSALEORDERENTRYID AS SrcDemandEntryId,FSALEORDERNO AS SrcDemandBillNo
            ,FSUPPLYORGID AS DemandOrgID
            ,FSUPPLYMATERIALID AS MaterialID
            ,FUNITID AS BaseUnitID 
            FROM T_PLN_REQUIREMENTORDER WHERE FID={reqid}";
            var salbilldata = DBUtils.ExecuteDynamicObject(this.Context, sSql);
            DemandView demandView = new DemandView();
            string demaInterid = "0";
            if (salbilldata.Count > 0)
            {
                demandView.DemandFormID = salbilldata[0]["DemandFormID"].ToString();
                demandView.DemandInterID = salbilldata[0]["DemandInterID"].ToString();
                demaInterid = salbilldata[0]["DemandInterID"].ToString();
                //demandView.DemandEntryID = string.Empty;
                demandView.DemandEntryID = "";
                demandView.DemandBillNO = salbilldata[0]["DemandBillNO"].ToString();

                //demandView.
                demandView.SrcDemandFormId = salbilldata[0]["SrcDemandFormId"].ToString();
                demandView.SrcDemandInterId = salbilldata[0]["SrcDemandInterId"].ToString();
                demandView.SrcDemandEntryId = salbilldata[0]["SrcDemandEntryId"].ToString();
                demandView.SrcDemandBillNo = salbilldata[0]["SrcDemandBillNo"].ToString();

                demandView.DemandOrgID_Id = Convert.ToInt64(salbilldata[0]["DemandOrgID"]);
                demandView.DemandDate = System.DateTime.Now;
                demandView.MaterialID_Id = Convert.ToInt64(salbilldata[0]["MaterialID"]);
                demandView.BaseUnitID_Id = Convert.ToInt64(salbilldata[0]["BaseUnitID"]);
                //demandView.BaseQty = resinfo.BaseActSupplyQty;

                //取即时库存
                List<SupplyViewItem> viewItems = new List<SupplyViewItem>();
                foreach (var item in resinfo)
                {
                    SupplyViewItem subRowView = new SupplyViewItem();

                    subRowView.SupplyFormID_Id = item.SupplyFormID;
                    subRowView.SupplyInterID = item.SupplyInterID;
                    subRowView.SupplyEntryId = item.SupplyEntryID;
                    subRowView.SupplyBillNO = item.SupplyBillNO;

                    subRowView.SupplyMaterialID_Id = Convert.ToInt64(item.SupplyMaterialID);
                    subRowView.SupplyOrgID_Id = Convert.ToInt64(item.SupplyOrgID);
                    subRowView.SupplyDate = System.DateTime.Now;
                    subRowView.SupplyStockID_Id = Convert.ToInt64(item.SupplyStockID);
                    subRowView.SupplyAuxproID_Id = 0;
                    subRowView.BaseSupplyUnitID_Id = Convert.ToInt64(item.BaseSupplyUnitID);
                    //供应数量
                    //subRowView.BaseActSupplyQty = lockinfo.qty;
                    subRowView.BaseActSupplyQty = item.BaseActSupplyQty;
                    subRowView.IntsupplyID = Convert.ToInt64(item.IntsupplyID);
                    subRowView.IntsupplyEntryId = Convert.ToInt64(item.IntsupplyEntryID);



                    viewItems.Add(subRowView);
                }

                demandView.supplyView = viewItems;
            }


            //创建转移行信息
            List<ReserveLinkSelectRow> lstConvertInfo = CreateReserveRow(demandView);
            //构建预留转移参数
            ReserveArgs<ReserveLinkSelectRow> convertArgs = new ReserveArgs<ReserveLinkSelectRow>();
            //把预留转移行的信息赋给参数
            convertArgs.SelectRows = lstConvertInfo;
            //创建预留服务
            IReserveLinkService linkService = AppServiceContext.GetService<IReserveLinkService>();
            //调用预留创建接口
            linkService.ReserveLinkCreate(this.Context, convertArgs, OperateOption.Create());
        }
        ///预留关系供需行      
        private List<ReserveLinkSelectRow> CreateReserveRow(DemandView demandView)
        {
            List<ReserveLinkSelectRow> linkRows = new List<ReserveLinkSelectRow>();
            //创建表头需求信息
            ReserveLinkSelectRow seleRow = new ReserveLinkSelectRow();
            //销售订单信息
            seleRow.DemandRow.DemandInfo.FormID = demandView.DemandFormID;
            seleRow.DemandRow.DemandInfo.InterID = demandView.DemandInterID;
            seleRow.DemandRow.DemandInfo.EntryID = demandView.DemandEntryID; ;
            seleRow.DemandRow.DemandInfo.BillNo = demandView.DemandBillNO;
            //销售订单信息
            seleRow.DemandRow.SrcDemandInfo.FormID = demandView.SrcDemandFormId;
            seleRow.DemandRow.SrcDemandInfo.InterID = demandView.SrcDemandInterId;
            seleRow.DemandRow.SrcDemandInfo.EntryID = demandView.SrcDemandEntryId;
            seleRow.DemandRow.SrcDemandInfo.BillNo = demandView.SrcDemandBillNo;
            seleRow.DemandRow.ReserveType = ((int)Enums.PLN_ReserveModel.Enu_ReserveType.KdStrong).ToString();
            //需求组织
            seleRow.DemandRow.DemandOrgID = demandView.DemandOrgID_Id;
            //需求日期
            seleRow.DemandRow.DemandDate = demandView.DemandDate;
            //物料内码
            seleRow.DemandRow.DemandMaterialID = demandView.MaterialID_Id;

            //基本单位内码
            seleRow.DemandRow.BaseUnitId = demandView.BaseUnitID_Id;
            //基本单位需求数量(基本单据数量-己出库的数量)
            seleRow.DemandRow.BaseDemandQty = demandView.BaseQty;

            //添加供应信息
            List<ReserveLinkSupplyRow> supplyRows = this.GetLinkSupplyRow(demandView.supplyView);
            seleRow.SupplyRows.AddRange(supplyRows);


            linkRows.Add(seleRow);
            return linkRows;
        }
        //创建供应行
        private List<ReserveLinkSupplyRow> GetLinkSupplyRow(List<SupplyViewItem> supplyView)
        {
            List<ReserveLinkSupplyRow> supplyRows = new List<ReserveLinkSupplyRow>();
            foreach (SupplyViewItem subRowView in supplyView)
            {
                ReserveLinkSupplyRow row = new ReserveLinkSupplyRow();
                //供应单据信息，单据标识，内码
                row.SupplyBillInfo.FormID = subRowView.SupplyFormID_Id;
                row.SupplyBillInfo.InterID = subRowView.SupplyInterID;
                row.SupplyBillInfo.EntryID = subRowView.SupplyEntryId;
                row.SupplyBillInfo.BillNo = subRowView.SupplyBillNO;
                row.SupplyMaterialID = subRowView.SupplyMaterialID_Id;
                row.SupplyOrgID = subRowView.SupplyOrgID_Id;
                row.SupplyDate = subRowView.SupplyDate;
                row.SupplyStockID = subRowView.SupplyStockID_Id;
                //row.SupplyStockLocID = subRowView.SupplyStockLocID_Id;
                //row.SupplyBomID = subRowView.SupplyBomID_Id;
                //row.SupplyLotId = subRowView.SupplyLot_Id;
                //row.SupplyLot_Text = subRowView.SupplyLot_Text;
                //row.SupplyMtoNO = subRowView.SupplyMtoNO;
                //row.SupplyProjectNO = "";
                row.SupplyAuxpropID = subRowView.SupplyAuxproID_Id;
                row.BaseSupplyUnitID = subRowView.BaseSupplyUnitID_Id;
                //供应数量
                row.BaseSupplyQty = subRowView.BaseActSupplyQty;
                row.LinkType = Enums.PLN_ReserveModel.Enu_ReserveBuildType.KdByManual;
                supplyRows.Add(row);
            }
            return supplyRows;
        }
    }

    public class resEntryInfo
    {
        public string SupplyFormID { get; set; }
        public string SupplyInterID { get; set; }
        public string SupplyEntryID { get; set; }
        public string SupplyBillNO { get; set; }
        public string SupplyMaterialID { get; set; }
        public string SupplyOrgID { get; set; }
        public string SupplyStockID { get; set; }
        public string BaseSupplyUnitID { get; set; }
        public decimal BaseActSupplyQty { get; set; }
        public long IntsupplyID { get; set; }
        public long IntsupplyEntryID { get; set; }
    }
}
