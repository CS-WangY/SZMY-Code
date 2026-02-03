using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.BD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.BusinessFlow.ServiceArgs;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using System.Data;
using Kingdee.BOS.Core.Metadata.FormElement;
using System.Transactions;
using Kingdee.BOS.BusinessEntity.BusinessFlow;
using System.Collections.Concurrent;
using Kingdee.K3.SCM.App.Core.Util;
using Kingdee.K3.SCM.Contracts;
using Kingdee.Mymooo.Core.StockManagement;
using System.Collections;

namespace Kingdee.Mymooo.App.Core.SalesManagement
{
    public class DeliveryNoticeList
    {
        public void SetQty(List<DeliveryNoticeDetSetQtyEntity> detList, Context ctx, IBillView billView)
        {
            bool flag = true;
            decimal num = 0.0m;
            DynamicObjectCollection instanceLinkData = GetInstanceLinkData(ctx, detList.Select(x => x.EntryId).ToList());
            List<Tuple<long, long>> removableInspectDatas = RemoveInspectionFlowDatas(instanceLinkData);
            Dictionary<long, decimal> dictionary = new Dictionary<long, decimal>();
            int infoIndex = 0;
            List<DynamicObject> list3 = new List<DynamicObject>();
            DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)billView.Model.DataObject["SAL_DELIVERYNOTICEENTRY"];
            OperateResultCollection operateResultCollection = new OperateResultCollection();
            if (dynamicObjectCollection == null || dynamicObjectCollection.Count == 0)
            {
                return;
            }
            DynamicObject dataObject = billView.Model.DataObject;
            List<string> list4 = new List<string>();
            foreach (DynamicObject item2 in dynamicObjectCollection)
            {
                if (detList.FirstOrDefault(x => x.EntryId.ToString().Equals(item2["Id"].ToString())) == null)
                {
                    continue;
                }

                DynamicObject dynamicObject = item2["MaterialID"] as DynamicObject;
                flag = CanUpdateQty(item2, instanceLinkData, out infoIndex);
                int num2 = Convert.ToInt32(item2["Seq"]) - 1;
                decimal num3 = 0m;
                num3 = Convert.ToDecimal(item2["STOCKCHANGEBASEQTY"]);
                if (infoIndex == 1 && num3 == 0m)
                {
                    list3.Add(item2);
                    string item = string.Format(ResManager.LoadKDString("注意:发货通知单({0})第{1}行分录,物料({2})没有关联任何下游单据且变更数量为0，将删除分录行!", "004072000037354", SubSystemType.SCM), dataObject["BillNo"], num2 + 1, dynamicObject["Name"]);
                    list4.Add(item);
                    continue;
                }
                if (infoIndex == 2 && num3 == 0m)
                {
                    string message = string.Format(ResManager.LoadKDString("发货通知单({0})第{1}行分录,物料({2})关联有下游单据且变更数量为0，暂不支持变更数量，如需支持请配置此下游单据反写发货通知单的变更数量。", "004072000020370", SubSystemType.SCM), dataObject["BillNo"], num2 + 1, dynamicObject["Name"]);
                    //AddOperateMessage(message, isSuccess: false, MessageType.FatalError, operateResultCollection, dataObject["BillNo"]);
                    continue;
                }
                switch (infoIndex)
                {
                    case 3:
                        {
                            string message3 = string.Format(ResManager.LoadKDString("发货通知单({0})第{1}行分录,物料({2})已关闭，不能变更数量!", "004072000015195", SubSystemType.SCM), dataObject["BillNo"], num2 + 1, dynamicObject["Name"]);
                            //  AddOperateMessage(message3, isSuccess: false, MessageType.FatalError, operateResultCollection, dataObject["BillNo"]);
                            continue;
                        }
                    case 4:
                        {
                            string message2 = string.Format(ResManager.LoadKDString("发货通知单({0})第{1}行分录,物料({2})业务已终止，不能变更数量!", "004072000015196", SubSystemType.SCM), dataObject["BillNo"], num2 + 1, dynamicObject["Name"]);
                            //  AddOperateMessage(message2, isSuccess: false, MessageType.FatalError, operateResultCollection, dataObject["BillNo"]);
                            continue;
                        }
                }
                if (!flag)
                {
                    continue;
                }

                dictionary.Add(Convert.ToInt64(item2["Id"]), Convert.ToDecimal(item2["STOCKCHANGEQTY"]));
                decimal quantity = detList.FirstOrDefault(x => x.EntryId.ToString().Equals(item2["Id"].ToString())).Quantity;
                decimal num6 = quantity;
                num = num6 - Convert.ToDecimal(item2["SumOutQty"]);
                num = ((num > 0.0m) ? num : 0.0m);
                billView.Model.SetValue("FRemainOutQty", num, num2);
                billView.InvokeFieldUpdateService("FRemainOutQty", num2);
                if (num == 0.0m)
                {
                    billView.Model.SetValue("FCLOSESTATUS_MX", "B", num2);
                    billView.InvokeFieldUpdateService("FCLOSESTATUS_MX", num2);
                }
            }
            if (list3.Count > 0 && list3.Count == dynamicObjectCollection.Count)
            {
                string msg = string.Format(ResManager.LoadKDString("发货通知单({0})分录都没有下游单据，请反审核后调整数量!", "004072000015197", SubSystemType.SCM), dataObject["BillNo"]);
                return;
            }

            if (list3.Count == 0 && dictionary.Count == 0)
            {
                return;
            }
            foreach (DynamicObject item4 in list3)
            {
                int modelEntryRow = GetModelEntryRow(dynamicObjectCollection, Convert.ToInt64(item4["Id"]));
                billView.Model.DeleteEntryRow("FEntity", modelEntryRow);
            }
            dynamicObjectCollection = (DynamicObjectCollection)billView.Model.DataObject["SAL_DELIVERYNOTICEENTRY"];
            bool flag2 = true;
            string empty = string.Empty;
            foreach (DynamicObject item5 in dynamicObjectCollection)
            {
                empty = Convert.ToString(item5["CLOSESTATUS"]);
                if (!empty.Equals("B", StringComparison.OrdinalIgnoreCase))
                {
                    flag2 = false;
                    break;
                }
            }
            if (flag2)
            {
                billView.Model.SetValue("FCLOSESTATUS", "B");
                billView.InvokeFieldUpdateService("FCLOSESTATUS", 0);
            }
            DynamicObjectCollection dynamicObjectCollection2 = BreakInspectionFlowDatas(removableInspectDatas, list3, ctx);
            OperateOption operateOption = OperateOption.Create();
            operateOption.SetVariableValue("IgnoreCheckSalAvailableQty", true);
            operateOption.SetVariableValue("IsOrderChange", true);
            IOperationResult operationResult = BusinessDataServiceHelper.Save(ctx, billView.BillBusinessInfo, billView.Model.DataObject, operateOption);
            if (operationResult.IsSuccess)
            {
                IBusinessFlowTrackerService businessFlowTrackerService = BOS.Contracts.ServiceFactory.GetBusinessFlowTrackerService(ctx);
                try
                {
                    businessFlowTrackerService.Audit(ctx, operationResult.SuccessDataEnity.ToArray(), billView.BusinessInfo, OperateOption.Create());
                    if (dynamicObjectCollection2 != null && dynamicObjectCollection2.Count > 0)
                    {
                        ResetSaleDeliveryCount(ctx, dynamicObjectCollection2);
                    }
                }
                finally
                {
                    BOS.Contracts.ServiceFactory.CloseService(businessFlowTrackerService);
                }
                string message4 = ResManager.LoadKDString("发货通知单变更数量保存成功!", "004072000015139", SubSystemType.SCM);
                //  AddOperateMessage(message4, isSuccess: true, MessageType.Normal, operateResultCollection, dataObject["BillNo"]);
                long billId = Convert.ToInt64(dataObject["Id"]);
                //UpdateDeliveryBillCreditDirect(billId, list2,ctx);
            }
            else
            {
                string text = ResManager.LoadKDString("发货通知单变更数量保存失败!", "004072000015140", SubSystemType.SCM);
                text += ((operationResult.InteractionContext != null) ? operationResult.InteractionContext.SimpleMessage : "");
                text += ((operationResult.ValidationErrors.Count > 0) ? operationResult.ValidationErrors[0].Message : "");
                text += ((operationResult.OperateResult.Count > 0) ? operationResult.OperateResult[0].Message : "");
                //   AddOperateMessage(text, isSuccess: false, MessageType.FatalError, operateResultCollection, dataObject["BillNo"]);
            }
            //  View.ShowOperateResult(operateResultCollection);
        }

        public class DeliveryNoticeDetSetQtyEntity
        {
            /// <summary>
            /// 明细ID
            /// </summary>
            public long EntryId { get; set; }

            /// <summary>
            /// 已送货数量
            /// </summary>
            public decimal Quantity { get; set; }

        }

        private DynamicObjectCollection BreakInspectionFlowDatas(List<Tuple<long, long>> removableInspectDatas, List<DynamicObject> deletedEntryDatas, Context ctx)
        {
            DynamicObjectCollection result = null;
            if (removableInspectDatas != null && removableInspectDatas.Count > 0 && deletedEntryDatas != null && deletedEntryDatas.Count > 0)
            {
                List<long> entryIds = deletedEntryDatas.Select((DynamicObject x) => Convert.ToInt64(x["Id"])).ToList();
                List<Tuple<long, long>> list = removableInspectDatas.Where((Tuple<long, long> x) => entryIds.Contains(x.Item1)).ToList();
                if (list != null && list.Count > 0)
                {
                    ISaleService2 saleService = K3.SCM.Contracts.ServiceFactory.GetSaleService2(ctx);
                    result = saleService.BreakDeliveryInspectionFlowDatas(ctx, list, removeSourceData: true);
                }
            }
            return result;
        }

        public DynamicObjectCollection GetInstanceLinkData(Context ctx, List<long> lstEntryIds)
        {
            using (new SessionScope())
            {
                ReadInstDatasWithHisArgs readInstDatasWithHisArgs = new ReadInstDatasWithHisArgs("SAL_DELIVERYNOTICE", "FEntity", lstEntryIds.ToArray());
                readInstDatasWithHisArgs.IsGetInst = false;
                readInstDatasWithHisArgs.IsGetAmount = false;
                IBusinessFlowDataService service = Kingdee.BOS.App.ServiceHelper.GetService<IBusinessFlowDataService>();
                ReadInstDatasWithHisResult readInstDatasWithHisResult = service.LoadInstDatasWithHis(ctx, readInstDatasWithHisArgs);
                string strSQL = string.Format("SELECT INS.FSID AS DNEntryId,INS.FTID AS TargetEId,INS.FTTABLENAME FROM {0} INS \r\n  WHERE FSID in ({1}) AND FSTABLENAME=N'T_SAL_DELIVERYNOTICEENTRY' \r\n                                        AND FFIRSTNODE='0'", readInstDatasWithHisResult.TmpTableEntry, string.Join(",", lstEntryIds.ToArray()));
                DynamicObjectCollection result = DBUtils.ExecuteDynamicObject(ctx, strSQL, (IDataEntityType)null, (IDictionary<string, Type>)null, CommandType.Text, (SqlParam[])null);
                DBUtils.DropSessionTemplateTable(ctx, readInstDatasWithHisResult.TmpTableMasterIds);
                DBUtils.DropSessionTemplateTable(ctx, readInstDatasWithHisResult.TmpTableEntry);
                return result;
            }
        }

        private List<Tuple<long, long>> RemoveInspectionFlowDatas(DynamicObjectCollection flowDatas)
        {
            List<Tuple<long, long>> list = new List<Tuple<long, long>>();
            if (flowDatas != null && flowDatas.Count > 0)
            {
                List<DynamicObject> list2 = new List<DynamicObject>();
                IEnumerable<IGrouping<long, DynamicObject>> enumerable = from x in flowDatas
                                                                         group x by Convert.ToInt64(x["DNEntryId"]);
                foreach (IGrouping<long, DynamicObject> item in enumerable)
                {
                    List<string> list3 = item.Select((DynamicObject x) => Convert.ToString(x["FTTABLENAME"])).Distinct().ToList();
                    if (list3.Count != 1 || !(list3[0] == "T_QM_STKAPPINSPECTENTRY"))
                    {
                        continue;
                    }
                    foreach (DynamicObject item2 in item)
                    {
                        list.Add(new Tuple<long, long>(item.Key, Convert.ToInt64(item2["TargetEId"])));
                        list2.Add(item2);
                    }
                }
                if (list2.Count > 0)
                {
                    foreach (DynamicObject item3 in list2)
                    {
                        flowDatas.Remove(item3);
                    }
                    return list;
                }
            }
            return list;
        }


        private bool CanUpdateQty(DynamicObject entryObj, DynamicObjectCollection dycLinkTargetData, out int infoIndex)
        {
            infoIndex = 0;
            bool flag = false;
            string text = Convert.ToString(entryObj["CLOSESTATUS"]);
            if (text.Equals("B"))
            {
                infoIndex = 3;
                return false;
            }
            text = Convert.ToString(entryObj["TerminationStatus"]);
            if (text.Equals("B"))
            {
                infoIndex = 4;
                return false;
            }
            long num = Convert.ToInt64(entryObj["Id"]);
            if (dycLinkTargetData != null && dycLinkTargetData.Count > 0)
            {
                long num2 = 0L;
                foreach (DynamicObject dycLinkTargetDatum in dycLinkTargetData)
                {
                    num2 = Convert.ToInt64(dycLinkTargetDatum["DNEntryId"]);
                    if (num2 == num)
                    {
                        flag = true;
                        infoIndex = 2;
                        break;
                    }
                }
            }
            if (!flag)
            {
                infoIndex = 1;
            }
            return flag;
        }
        private int GetModelEntryRow(DynamicObjectCollection entity, long entryid)
        {
            for (int i = 0; i < entity.Count; i++)
            {
                DynamicObject dynamicObject = entity[i];
                if (Convert.ToInt64(dynamicObject["Id"]) == entryid)
                {
                    return i;
                }
            }
            return -1;
        }

        public void ResetSaleDeliveryCount(Context ctx, DynamicObjectCollection links)
        {
            if (links == null || links.Count <= 0)
            {
                return;
            }
            IEnumerable<IGrouping<long, DynamicObject>> enumerable = from x in links
                                                                     group x by Convert.ToInt64(x["FSID"]);
            DataTable dataTable = new DataTable();
            DataRow dataRow = null;
            dataTable.Columns.Add("FENTRYID", typeof(long));
            dataTable.Columns.Add("FCANOUTQTY", typeof(decimal));
            dataTable.Columns.Add("FBASECANOUTQTY", typeof(decimal));
            dataTable.Columns.Add("FSTOCKBASECANOUTQTY", typeof(decimal));
            dataTable.Columns.Add("FDELIQTY", typeof(decimal));
            dataTable.Columns.Add("FBASEDELIQTY", typeof(decimal));
            List<Tuple<long, decimal, decimal>> list = new List<Tuple<long, decimal, decimal>>();
            foreach (IGrouping<long, DynamicObject> item in enumerable)
            {
                DynamicObject dynamicObject = item.First();
                decimal num = Convert.ToDecimal(dynamicObject["FCANOUTQTY"]);
                decimal num2 = Convert.ToDecimal(dynamicObject["FBASECANOUTQTY"]);
                decimal num3 = Convert.ToDecimal(dynamicObject["FSTOCKBASECANOUTQTY"]);
                decimal num4 = Convert.ToDecimal(dynamicObject["FBASEDELIQTY"]);
                decimal num5 = Convert.ToDecimal(dynamicObject["FDELIQTY"]);
                int decimals = Convert.ToInt32(dynamicObject["FPRECISION"]);
                decimal num6 = Convert.ToDecimal(dynamicObject["FPROPORTION"]);
                decimal num7 = item.Sum((DynamicObject x) => Convert.ToDecimal(x["FBASEUNITQTY"]));
                decimal num8 = item.Sum((DynamicObject x) => Convert.ToDecimal(x["FSTOCKBASEQTY"]));
                decimal num9 = Math.Round((num6 == 0m) ? num7 : (num7 / num6), decimals);
                num += num9;
                num2 += num7;
                num3 += num8;
                num4 -= num7;
                if (num4 < 0m)
                {
                    num4 = 0m;
                }
                num5 -= num9;
                if (num5 < 0m)
                {
                    num5 = 0m;
                }
                dataRow = dataTable.NewRow();
                dataRow.BeginEdit();
                dataRow[0] = item.Key;
                dataRow[1] = num;
                dataRow[2] = num2;
                dataRow[3] = num3;
                dataRow[4] = num5;
                dataRow[5] = num4;
                dataRow.EndEdit();
                dataTable.Rows.Add(dataRow);
                list.Add(new Tuple<long, decimal, decimal>(item.Key, num7, num9));
            }
            BatchSqlParam batchSqlParam = new BatchSqlParam("T_SAL_ORDERENTRY_R", dataTable);
            batchSqlParam.TableAliases = "SALE_R";
            batchSqlParam.AddWhereExpression("FENTRYID", KDDbType.Int64, "FENTRYID");
            batchSqlParam.AddSetExpression("FCANOUTQTY", KDDbType.Decimal, "FCANOUTQTY");
            batchSqlParam.AddSetExpression("FBASECANOUTQTY", KDDbType.Decimal, "FBASECANOUTQTY");
            batchSqlParam.AddSetExpression("FSTOCKBASECANOUTQTY", KDDbType.Decimal, "FSTOCKBASECANOUTQTY");
            batchSqlParam.AddSetExpression("FDELIQTY", KDDbType.Decimal, "FDELIQTY");
            batchSqlParam.AddSetExpression("FBASEDELIQTY", KDDbType.Decimal, "FBASEDELIQTY");
            BatchSqlParam batchSqlParamForPlan = GetBatchSqlParamForPlan(ctx, list);
            using (KDTransactionScope kDTransactionScope = new KDTransactionScope(TransactionScopeOption.Required))
            {
                DBUtils.BatchUpdate(ctx, batchSqlParam);
                if (batchSqlParamForPlan != null)
                {
                    DBUtils.BatchUpdate(ctx, batchSqlParamForPlan);
                }
                kDTransactionScope.Complete();
            }
        }

        private BatchSqlParam GetBatchSqlParamForPlan(Context ctx, List<Tuple<long, decimal, decimal>> salesPlans)
        {
            string strSQL = string.Format("\r\nSELECT FENTRYID, FNOTICEQTY, FNOTICEBASEQTY, FNOTICEREMAINQTY, FNOTICEREMAINBASEQTY\r\nFROM T_SAL_ORDERENTRYDELIPLAN\r\nWHERE EXISTS (SELECT {0} 1 FROM TABLE(fn_StrSplit(@FENTRYID, ',', 1)) B WHERE B.FID = FENTRYID)", SCMCommonUtil.GetCardinalityString(ctx, "B", salesPlans.Count));
            SqlParam sqlParam = new SqlParam("@FENTRYID", KDDbType.udt_inttable, salesPlans.Select((Tuple<long, decimal, decimal> x) => x.Item1).ToArray());
            DynamicObjectCollection source = DBUtils.ExecuteDynamicObject(ctx, strSQL, null, null, CommandType.Text, sqlParam);
            IEnumerable<IGrouping<long, DynamicObject>> enumerable = from x in source
                                                                     group x by Convert.ToInt64(x["FENTRYID"]);
            DataTable dataTable = new DataTable();
            DataRow dataRow = null;
            dataTable.Columns.Add("FENTRYID", typeof(long));
            dataTable.Columns.Add("FNOTICEQTY", typeof(decimal));
            dataTable.Columns.Add("FNOTICEBASEQTY", typeof(decimal));
            dataTable.Columns.Add("FNOTICEREMAINQTY", typeof(decimal));
            dataTable.Columns.Add("FNOTICEREMAINBASEQTY", typeof(decimal));
            foreach (IGrouping<long, DynamicObject> groupedPlan in enumerable)
            {
                Func<Tuple<long, decimal, decimal>, bool> predicate = (Tuple<long, decimal, decimal> x) => x.Item1 == groupedPlan.Key;
                Tuple<long, decimal, decimal> tuple = salesPlans.FirstOrDefault(predicate);
                if (tuple == null)
                {
                    continue;
                }
                decimal item = tuple.Item2;
                decimal item2 = tuple.Item3;
                foreach (DynamicObject item3 in groupedPlan)
                {
                    dataRow = dataTable.NewRow();
                    dataRow.BeginEdit();
                    decimal num = Convert.ToDecimal(item3["FNOTICEQTY"]);
                    decimal num2 = Convert.ToDecimal(item3["FNOTICEBASEQTY"]);
                    decimal num3 = Convert.ToDecimal(item3["FNOTICEREMAINQTY"]);
                    decimal num4 = Convert.ToDecimal(item3["FNOTICEREMAINBASEQTY"]);
                    bool flag = false;
                    decimal num5 = 0m;
                    decimal num6 = 0m;
                    if (num2 >= item)
                    {
                        num5 = num2 - item;
                        num6 = num - item2;
                    }
                    else
                    {
                        num6 = (num5 = 0m);
                        flag = true;
                        item -= num2;
                        item2 -= num;
                    }
                    dataRow[0] = groupedPlan.Key;
                    dataRow[1] = num6;
                    dataRow[2] = num5;
                    dataRow[3] = num3 + Math.Abs(num - num6);
                    dataRow[4] = num4 + Math.Abs(num2 - num5);
                    dataRow.EndEdit();
                    dataTable.Rows.Add(dataRow);
                    if (!flag)
                    {
                        break;
                    }
                }
            }
            BatchSqlParam batchSqlParam = null;
            if (dataTable.Rows.Count > 0)
            {
                batchSqlParam = new BatchSqlParam("T_SAL_ORDERENTRYDELIPLAN", dataTable);
                batchSqlParam.TableAliases = "SALEPLAN";
                batchSqlParam.AddWhereExpression("FENTRYID", KDDbType.Int64, "FENTRYID");
                batchSqlParam.AddSetExpression("FNOTICEQTY", KDDbType.Decimal, "FNOTICEQTY");
                batchSqlParam.AddSetExpression("FNOTICEBASEQTY", KDDbType.Decimal, "FNOTICEBASEQTY");
                batchSqlParam.AddSetExpression("FNOTICEREMAINQTY", KDDbType.Decimal, "FNOTICEREMAINQTY");
                batchSqlParam.AddSetExpression("FNOTICEREMAINBASEQTY", KDDbType.Decimal, "FNOTICEREMAINBASEQTY");
            }
            return batchSqlParam;
        }

    }
}
