using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.StockManagement;
using System.Web.UI.WebControls.WebParts;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.BOS;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.Const;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;
using Kingdee.Mymooo.App.Core.KafkaProducer;
using static Kingdee.K3.Core.SCM.Mobile.SCMMobEnums;
namespace Kingdee.Mymooo.ServicePlugIn.SAL_RETURNSTOCK
{
    [Description("销售退货单审核更新组织间需求单可调拨数量"), HotUpdate]
    public class Audit : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FSOEntryId");
            e.FieldKeys.Add("FRealQty");
            e.FieldKeys.Add("FReturnType");
            e.FieldKeys.Add("FBUSINESSDIVISIONID");
            e.FieldKeys.Add("FSOENTRYID");
            e.FieldKeys.Add("FStockId");
            e.FieldKeys.Add("FPENYReturnType");
            e.FieldKeys.Add("FSupplyTargetOrgId");
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            List<RabbitMQMessage> messages = new List<RabbitMQMessage>();
            foreach (var item in e.DataEntitys)
            {
                var fid = Convert.ToInt64(item["Id"]);
                var billtype = item["BillTypeID_Id"].ToString();
                var billno = item["BillNo"].ToString();
                long[] SalOrgList = new long[] { 224428, 7348029, 1043841 };
                if (!SalOrgList.Contains(Convert.ToInt64(item["SaleOrgId_Id"])))
                {
                    continue;
                }
                //if (!item["SaleOrgId_Id"].ToString().EqualsIgnoreCase("224428"))
                //{
                //    return;
                //}
                //需要调拨的数据集合
                List<TAllocate> list = new List<TAllocate>();
                List<TAllocate> returnlist = new List<TAllocate>();
                IOperationResult result = null;
                List<long> retrieveSOEntryIdList = new List<long>();
                foreach (var entitem in item["SAL_RETURNSTOCKENTRY"] as DynamicObjectCollection)
                {
                    var eid = Convert.ToString(entitem["Id"]);
                    var seq = Convert.ToString(entitem["Seq"]);
                    var soeid = Convert.ToInt64(entitem["SOEntryId"]);
                    var realqty = Convert.ToDecimal(entitem["RealQty"]);
                    var returntype = Convert.ToString(((DynamicObject)entitem["ReturnType"])["FNumber"]);
                    var stockid = Convert.ToString(((DynamicObject)entitem["StockId"])["Id"]);
                    var materialId = Convert.ToString(((DynamicObject)entitem["MaterialId"])["Number"]);
                    var supplyorgid = Convert.ToInt64(entitem["FSupplyTargetOrgId_Id"]);
                    //退货补货获取销售ID
                    if (returntype.EqualsIgnoreCase("THLX02_SYS") && soeid != 0)
                    {
                        retrieveSOEntryIdList.Add(soeid);
                    }
                    switch (Convert.ToInt32(item["FPENYReturnType"]))
                    {
                        case 1:
                            var dstockid = GetOrgStockId(this.Context, supplyorgid);
                            returnlist.Add(new TAllocate
                            {
                                FID = Convert.ToInt64(fid),
                                FENTRYID = Convert.ToInt64(eid),
                                FQty = realqty,
                                FReturnBillNo = billno,
                                FReturnBillSEQ = seq,
                                FStockID = dstockid,
                            });
                            break;
                        case 2:
                            if (soeid <= 0)
                            {
                                return;
                            }
                            //取退货通知单数量
                            var srcbillid = "";
                            var srcrowid = "";
                            foreach (var itemlink in entitem["FEntity_Link"] as DynamicObjectCollection)
                            {
                                srcbillid = itemlink["SBillId"] as string;
                                srcrowid = itemlink["SId"] as string;
                            }
                            //取发货通知单信息
                            string sSql = $@"SELECT t4.FID,t4.FENTRYID,t5.FBILLNO FROM T_SAL_RETURNNOTICEENTRY_LK t1
                    INNER JOIN T_SAL_OUTSTOCKENTRY t2 ON t1.FSBILLID=t2.FID AND t1.FSID=t2.FENTRYID
                    INNER JOIN T_SAL_OUTSTOCKENTRY_LK t3 ON t2.FENTRYID=t3.FENTRYID
                    INNER JOIN T_SAL_DELIVERYNOTICEENTRY t4 ON t3.FSBILLID=t4.FID AND t3.FSID=t4.FENTRYID
                    INNER JOIN T_SAL_DELIVERYNOTICE t5 ON t4.FID=t5.FID
                    WHERE t1.FENTRYID={srcrowid}";
                            var delnDatas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                            if (delnDatas.Count == 0) return;
                            string delnID = "";
                            string delnEID = "";
                            string delnBillNo = "";
                            foreach (var itemlink in delnDatas)
                            {
                                delnID = Convert.ToString(itemlink["FID"]);
                                delnEID = Convert.ToString(itemlink["FENTRYID"]);
                                delnBillNo = Convert.ToString(itemlink["FBILLNO"]);
                            }

                            sSql = $@"SELECT t1.FID,t1.FENTRYID,t1.FQTY-t2.FRECEIVEQTY FQTY FROM T_STK_STKTRANSFERINENTRY t1
                           LEFT JOIN T_STK_STKTRANSFERINENTRY_R t2 ON t1.FENTRYID=t2.FENTRYID
                           WHERE FDELIVERYNOTICEID={delnID} AND FDELIVERYNOTICEENTRYID={delnEID}
                           AND FPENYDELIVERYNOTICE='{delnBillNo}'
                           AND t1.FQTY-t2.FRECEIVEQTY>0
                           ORDER BY t1.FID";
                            var stkDatas = DBUtils.ExecuteDynamicObject(this.Context, sSql);

                            if (realqty <= 0) continue;
                            decimal sendQty = 0;
                            foreach (var stkData in stkDatas)
                            {
                                if (realqty == 0) continue;
                                if (realqty - Convert.ToDecimal(stkData["FQTY"]) >= 0)
                                {
                                    sendQty = Convert.ToDecimal(stkData["FQTY"]);
                                    realqty = realqty - Convert.ToDecimal(stkData["FQTY"]);
                                }
                                else
                                {
                                    sendQty = realqty;
                                    realqty = 0;
                                }
                                list.Add(new TAllocate
                                {
                                    FID = Convert.ToInt64(stkData["FID"]),
                                    FENTRYID = Convert.ToInt64(stkData["FENTRYID"]),
                                    FQty = sendQty,
                                    FReturnBillNo = billno,
                                    FReturnBillSEQ = seq,
                                });
                            }
                            break;
                    }
                }
                string TransferNumber = "";
                if (list.Count > 0)
                {
                    result = SalDeliveryNoticePushAllocate(this.Context, list);
                    TransferNumber = string.Join(";", result.OperateResult.Select(p => p.Number));
                }
                if (returnlist.Count > 0)
                {
                    result = SalReturnNoticePushAllocate(this.Context, returnlist);
                    TransferNumber = string.Join(";", result.OperateResult.Select(p => p.Number));
                }
                var sSqltran = $@"/*dialect*/update T_SAL_RETURNSTOCK set FPENYSTKTRANSFERNO='{TransferNumber}' where FID={fid}";
                DBUtils.Execute(this.Context, sSqltran);
                //判断类型是"退货补货"，调平台接口，把销售订单状态改为未完成
                if (retrieveSOEntryIdList.Count > 0)
                {
                    var sSql = $@"/*dialect*/select tt1.FBILLNO,tt2.FSEQ,tt2.FOrderDetailId,case when (tt2.FQTY-
                                    isnull((select SUM(t2.FREALQTY) FREALQTY from T_SAL_RETURNSTOCK t1
                                    inner join T_SAL_RETURNSTOCKENTRY t2 on t1.FID=t2.FID
                                    inner join T_SAL_RETURNSTOCKENTRY_LK t3 on  t2.FENTRYID=t3.FENTRYID
                                    inner join T_SAL_RETURNNOTICEENTRY t4 on  t3.FSID=t4.FENTRYID and t3.FSBILLID=t4.FID
                                    inner join T_SAL_RETURNNOTICEENTRY_LK t5 on t4.FENTRYID=t5.FENTRYID
                                    inner join T_SAL_OUTSTOCKENTRY  t6 on t6.FENTRYID=t5.FSID and t6.FID=t5.FSBILLID 
                                    inner join T_SAL_OUTSTOCKENTRY_LK  t7 on t6.FENTRYID=t7.FENTRYID  
                                    inner join T_SAL_DELIVERYNOTICEENTRY t8 on t8.FENTRYID=t7.FSID and t8.FID=t7.FSBILLID 
                                    inner join T_SAL_DELIVERYNOTICEENTRY_LK  t9 on t8.FENTRYID=t9.FENTRYID 
                                    inner join T_SAL_ORDERENTRY t10 on t10.FENTRYID=t9.FSID and t10.FID=t9.FSBILLID 
                                    where  t10.FID=tt1.FID and t10.FENTRYID=tt2.FENTRYID and t1.FDOCUMENTSTATUS='C' 
                                    and t2.FRETURNTYPE='4151a33171a04ba6af24524c656b5f79'),0)-tt3.FCANOUTQTY)=0 then 0 else 1 end DeliveryStatus
                                    from T_SAL_ORDER tt1 
                                    inner join T_SAL_ORDERENTRY tt2 on tt1.FID=tt2.FID
                                    inner join T_SAL_ORDERENTRY_R tt3 on tt2.FENTRYID=tt3.FENTRYID 
                                    where tt2.FENTRYID in ({string.Join(",", retrieveSOEntryIdList)}) ";
                    var datas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                    List<RetrieveSo> retrieveSoList = new List<RetrieveSo>();
                    foreach (var data in datas)
                    {
                        retrieveSoList.Add(new RetrieveSo
                        {
                            SoNo = Convert.ToString(data["FBILLNO"]),
                            SoSeq = Convert.ToInt32(data["FSEQ"]),
                            DetailId = Convert.ToInt64(data["FOrderDetailId"]),
                            DeliveryStatus = Convert.ToInt32(data["DeliveryStatus"])
                        });
                    }
                    if (retrieveSoList.Count() > 0)
                    {
                        messages.Add(new RabbitMQMessage
                        {
                            Exchange = "mall_salesOrder",
                            Routingkey = "RetrieveUpSoOrderState",
                            Keyword = billno,
                            Message = JsonConvertUtils.SerializeObject(retrieveSoList)
                        });
                    }
                }
            }
            if (messages.Count() > 0)
            {
                KafkaProducerService kafkaProducer = new KafkaProducerService();
                kafkaProducer.AddMessage(this.Context, messages.ToArray());
            }

        }
        public IOperationResult SalReturnNoticePushAllocate(Context ctx, List<TAllocate> allocates)
        {
            List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
            StockPushEntity push = new StockPushEntity();
            foreach (var item in allocates)
            {
                var row = new ListSelectedRow(Convert.ToString(item.FID), Convert.ToString(item.FENTRYID), 0, "SAL_RETURNSTOCK");
                row.EntryEntityKey = "SAL_RETURNSTOCKENTRY"; //这里最容易忘记加，是重点的重点
                selectedRows.Add(row);
            }

            push.listSelectedRow = selectedRows;
            push.ConvertRule = "PENY_SalReturnStock-StkTransferOut2";
            //push.TargetOrgId = Convert.ToInt64(item.TargetOrgId);
            //push.TargetOrgId = Convert.ToInt64(allocates.TargetOrgId);

            var result = this.BillPushReturn(ctx, push, allocates);
            return result;
        }
        public IOperationResult SalDeliveryNoticePushAllocate(Context ctx, List<TAllocate> allocates)
        {
            List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
            StockPushEntity push = new StockPushEntity();
            foreach (var item in allocates)
            {
                var row = new ListSelectedRow(Convert.ToString(item.FID), Convert.ToString(item.FENTRYID), 0, "STK_TRANSFERIN");
                row.EntryEntityKey = "FSTKTRSINENTRY"; //这里最容易忘记加，是重点的重点
                selectedRows.Add(row);
            }

            push.listSelectedRow = selectedRows;
            push.ConvertRule = "STK_TRANSFERIN-STK_TRANSFEROUT";
            //push.TargetOrgId = Convert.ToInt64(item.TargetOrgId);
            //push.TargetOrgId = Convert.ToInt64(allocates.TargetOrgId);

            var result = this.BillPush(ctx, push, allocates);
            return result;
        }
        public IOperationResult BillPushReturn(Context ctx, StockPushEntity pushEntity, List<TAllocate> allocates)
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
            pushOption.SetVariableValue(ConvertConst.SelectByBillId, false);

            var convertResult = ConvertServiceHelper.Push(ctx, pushArgs, pushOption);//调用下推接口
            var targetObjs = (from p in convertResult.TargetDataEntities select p.DataEntity).ToArray();//得到目标单据数据包

            // 把新拆分出来的单据体行，加入到下推结果中
            // 特别说明：如果去掉此语句，新拆分的行，不会执行表单服务策略
            //e.TargetExtendedDataEntities.AddExtendedDataEntities("FEntity", newRows.ToArray());
            foreach (DynamicObject targeEntry in targetObjs)
            {
                var stockorgid = Convert.ToString(targeEntry["StockOrgID_Id"]);
                //退货类型
                targeEntry["FPENYReturnType"] = 1;
                var rowEntry = targeEntry["STK_STKTRANSFEROUTENTRY"] as DynamicObjectCollection;
                foreach (var rowlin in rowEntry)
                {
                    long srcbillid = 0;
                    foreach (var itemlink in rowlin["FSTKTSTKRANSFEROUTENTRY_Link"] as DynamicObjectCollection)
                    {
                        srcbillid = Convert.ToInt64(itemlink["SID"]);
                    }

                    var aot = allocates.Where(x => x.FENTRYID == srcbillid).ToList().First();
                    //修改赋值
                    rowlin["FQty"] = aot.FQty;
                    var resqty = aot.FQty;
                    var unitid = Convert.ToInt64(rowlin["UnitID_Id"]);
                    var baseunitid = Convert.ToInt64(rowlin["BaseUnitID_Id"]);
                    IConvertService convertService = Kingdee.BOS.App.ServiceHelper.GetService<IConvertService>();
                    if (unitid != baseunitid)
                    {
                        resqty = convertService.GetUnitTransQty(this.Context, Convert.ToInt64(rowlin["MarterlId"]), unitid, baseunitid, resqty);
                    }
                    rowlin["BaseQty"] = resqty;
                    rowlin["FPENYSalReturnNo"] = aot.FReturnBillNo;
                    rowlin["FPENYSalReturnSEQ"] = aot.FReturnBillSEQ;
                    rowlin["DestStockID_Id"] = aot.FStockID;
                    //rowlin["SrcStockStatusID_Id"] = 10000;
                    //rowlin["DestStockStatusID_Id"] = 10004;
                }

                DBServiceHelper.LoadReferenceObject(this.Context, rowEntry.ToArray(), rowEntry.DynamicCollectionItemPropertyType, true);
            }
            var targetBInfo = this.GetBusinessInfo(ctx, pushArgs.ConvertRule.TargetFormId, null);
            //对转换结果进行处理
            //1. 直接调用保存接口，对数据进行保存
            return this.SaveTargetBill(ctx, targetBInfo, targetObjs);
        }

        public IOperationResult BillPush(Context ctx, StockPushEntity pushEntity, List<TAllocate> allocates)
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
            pushOption.SetVariableValue(ConvertConst.SelectByBillId, false);

            var convertResult = ConvertServiceHelper.Push(ctx, pushArgs, pushOption);//调用下推接口
            var targetObjs = (from p in convertResult.TargetDataEntities select p.DataEntity).ToArray();//得到目标单据数据包

            // 把新拆分出来的单据体行，加入到下推结果中
            // 特别说明：如果去掉此语句，新拆分的行，不会执行表单服务策略
            //e.TargetExtendedDataEntities.AddExtendedDataEntities("FEntity", newRows.ToArray());
            foreach (DynamicObject targeEntry in targetObjs)
            {
                var stockorgid = Convert.ToString(targeEntry["StockOrgID_Id"]);
                //退货类型
                targeEntry["FPENYReturnType"] = 1;
                var rowEntry = targeEntry["STK_STKTRANSFEROUTENTRY"] as DynamicObjectCollection;
                foreach (var rowlin in rowEntry)
                {
                    long srcbillid = 0;
                    foreach (var itemlink in rowlin["FSTKTSTKRANSFEROUTENTRY_Link"] as DynamicObjectCollection)
                    {
                        srcbillid = Convert.ToInt64(itemlink["SID"]);
                    }

                    var aot = allocates.Where(x => x.FENTRYID == srcbillid).ToList().First();
                    //修改赋值
                    rowlin["FQty"] = aot.FQty;
                    var resqty = aot.FQty;
                    var unitid = Convert.ToInt64(rowlin["UnitID_Id"]);
                    var baseunitid = Convert.ToInt64(rowlin["BaseUnitID_Id"]);
                    //var material = rowlin["MaterialID"] as DynamicObject;
                    IConvertService convertService = Kingdee.BOS.App.ServiceHelper.GetService<IConvertService>();
                    if (unitid != baseunitid)
                    {
                        resqty = convertService.GetUnitTransQty(this.Context, Convert.ToInt64(rowlin["MaterialID_Id"]), unitid, baseunitid, resqty);
                    }
                    rowlin["BaseQty"] = resqty;
                    rowlin["FPENYSalReturnNo"] = aot.FReturnBillNo;
                    rowlin["FPENYSalReturnSEQ"] = aot.FReturnBillSEQ;

                    //rowlin["SrcStockStatusID_Id"] = 10000;
                    //rowlin["DestStockStatusID_Id"] = 10004;
                }

                DBServiceHelper.LoadReferenceObject(this.Context, rowEntry.ToArray(), rowEntry.DynamicCollectionItemPropertyType, true);
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
            //提交
            //SubmitService submitService = new SubmitService();

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

        private long GetOrgStockId(Context ctx, long orgId)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@orgId", KDDbType.Int64, orgId) };
            string sql = $@"SELECT top 1 t1.FSTOCKID FROM dbo.T_BD_STOCK t1
                            INNER JOIN dbo.T_BD_STOCK_L t2 ON t1.FSTOCKID=t2.FSTOCKID
                            WHERE t1.FUSEORGID=@orgId
							AND t2.FNAME LIKE '%成品%'
							AND FDOCUMENTSTATUS='C'
                            ORDER BY t1.FSTOCKID";
            return DBUtils.ExecuteScalar<long>(ctx, sql, 0, paramList: pars.ToArray());
        }


        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);
            Task.Factory.StartNew(() =>
            {
                //晚5个s,让事务可以提交成功后在发送消息
                System.Threading.Thread.Sleep(5000);
                ApigatewayUtils.InvokeWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/RabbitMQ/SendMqMessage");
            });
        }
    }
    public class TAllocate
    {
        public long FID { get; set; }
        public long FENTRYID { get; set; }
        public decimal FQty { get; set; }
        public string FSalBillNo { get; set; }
        public string FSalBillSEQ { get; set; }
        public string FReturnBillNo { get; set; }
        public string FReturnBillSEQ { get; set; }
        public long FStockID { get; set; }
    }

    public class RetrieveSo
    {
        /// <summary>
        /// 销售单号
        /// </summary>
        public string SoNo { get; set; }

        /// <summary>
        /// 销售序号
        /// </summary>
        public int SoSeq { get; set; } = 0;

        /// <summary>
        /// 平台明细ID
        /// </summary>
        public long DetailId { get; set; } = 0;

        /// <summary>
        // case 0:  "未发货";  case 1:   "部分发货"; 
        /// </summary>
        public int DeliveryStatus { get; set; } = 0;
    }
}
