using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.App.Core.BusinessFlow;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Collections.Generic;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Core;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.BOS;
using Kingdee.K3.Core.SCM.SAL;
using Kingdee.K3.MFG.App.ServiceValidator;
using Kingdee.K3.SCM.App.Core.Util;
using Kingdee.K3.SCM.App.Sal.ServicePlugIn.SaleOrder;
using Kingdee.K3.SCM.App.Sal.ServicePlugIn;
using Kingdee.K3.SCM.Contracts;
using static Kingdee.K3.Core.SCM.PUR.PurchaseOrderChangeFlagEnum;
using Kingdee.BOS.BusinessEntity.BusinessFlow;
using Kingdee.K3.SCM.App.Validator;
using ServiceFactory = Kingdee.BOS.Contracts.ServiceFactory;
using MaterialAuxPtyItemsValueValidator = Kingdee.K3.MFG.App.ServiceValidator.MaterialAuxPtyItemsValueValidator;

namespace Kingdee.Mymooo.ServicePlugIn.SalesBill
{
    [Description("销售订单深圳蚂蚁通版标准保存操作服务端插件")]
    public class Save : AbstractOperationServicePlugIn
    {
        public string versionNoBeforeSave;

        private bool isOrderChange;

        private WriteBackReceiveBill wb = new WriteBackReceiveBill();

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            e.FieldKeys.Add("FPlanDeliveryDate");
            e.FieldKeys.Add("FMinPlanDeliveryDate");
            e.FieldKeys.Add("FCustId");
            e.FieldKeys.Add("FRecConditionId");
            e.FieldKeys.Add("FContractType");
            e.FieldKeys.Add("FContractId");
            e.FieldKeys.Add("FSrcType");
            e.FieldKeys.Add("FIsUseOEMBomPush");
        }

        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            if (((OperationTransactionArgs)e).DataEntitys != null && ((OperationTransactionArgs)e).DataEntitys.Count() >= 1 && isOrderChange)
            {
                List<long> list = new List<long>();
                ((AbstractOperationServicePlugIn)this).Option.TryGetVariableValue<List<long>>("DeleteEntryWhenOrderChange", out list);
                if (list != null && list.Count > 0)
                {
                    BreakFlowLinkData(((AbstractOperationServicePlugIn)this).Context, list);
                }
            }
        }

        private void BreakFlowLinkData(Context ctx, List<long> deleteEntryIds)
        {
            if (deleteEntryIds != null && deleteEntryIds.Count > 0)
            {
                DynamicObjectCollection deleteItems = GetDeleteItems(ctx, "T_PUR_POORDERENTRY", deleteEntryIds);
                BreakRecFlow(deleteItems, "PUR_PurchaseOrder", "FPOOrderEntry");
                DynamicObjectCollection deleteItems2 = GetDeleteItems(ctx, "T_PUR_REQENTRY", deleteEntryIds);
                BreakRecFlow(deleteItems2, "PUR_Requisition", "FEntity");
                DynamicObjectCollection deleteItems3 = GetDeleteItems(ctx, "T_PRD_MOENTRY", deleteEntryIds);
                BreakRecFlow(deleteItems3, "PRD_MO", "FTREEENTITY");
            }
        }

        private DynamicObjectCollection GetDeleteItems(Context ctx, string linkTableName, List<long> deleteEntryIds)
        {
            new List<DynamicObject>();
            deleteEntryIds = deleteEntryIds.Distinct().ToList();
            string text = string.Format("select ISNULL(POLK.FSBILLID,0) FSBILLID,ISNULL(POLK.FSID,0) FSID, ISNULL(POE.FID,0) FTBILLID,ISNULL(POE.FENTRYID,0) FTID \r\nfrom {0} POLK\r\nleft join {1} POE on POLK.FENTRYID=POE.FENTRYID\r\nleft join T_BF_TABLEDEFINE OTD on OTD.FTABLENUMBER=POLK.FSTABLENAME\r\nwhere OTD.FFORMID='SAL_SaleOrder' and EXISTS (SELECT {2} 1 FROM TABLE(fn_StrSplit(@FENTRYID,',',1)) B WHERE B.FID=POLK.FSID)", linkTableName + "_LK", linkTableName, SCMCommonUtil.GetCardinalityString(ctx, "B", (long)deleteEntryIds.Count));
            SqlParam val = new SqlParam("@FENTRYID", (KDDbType)161, (object)deleteEntryIds.ToArray());
            return DBUtils.ExecuteDynamicObject(ctx, text, (IDataEntityType)null, (IDictionary<string, Type>)null, CommandType.Text, (SqlParam[])(object)new SqlParam[1] { val });
        }

        private void BreakRecFlow(DynamicObjectCollection deleteRows, string billName, string billEntry)
        {
            if (deleteRows == null || ((Collection<DynamicObject>)(object)deleteRows).Count <= 0)
            {
                return;
            }
            BreakFlowData val = new BreakFlowData("SAL_SaleOrder", "FSaleOrderEntry", billName, billEntry, (BreakRowType)1);
            List<long> list = new List<long>();
            foreach (DynamicObject item in (Collection<DynamicObject>)(object)deleteRows)
            {
                long num = Convert.ToInt64(item["FSBILLID"]);
                long num2 = Convert.ToInt64(item["FSID"]);
                long num3 = Convert.ToInt64(item["FTBILLID"]);
                long num4 = Convert.ToInt64(item["FTID"]);
                if (num > 0 && num2 > 0 && num3 > 0 && num4 > 0)
                {
                    List<BreakRow> rows = val.Rows;
                    BreakRow val2 = new BreakRow();
                    val2.SBillId = (num);
                    val2.SId = (num2);
                    val2.TBillId = (num3);
                    val2.TId = (num4);
                    rows.Add(val2);
                    if (!list.Contains(num4))
                    {
                        list.Add(num4);
                    }
                }
            }
            if (val.Rows.Count > 0)
            {
                BreakFlowResult val3 = new BreakBusinessFlowService().BreakBusinessFlow(((AbstractOperationServicePlugIn)this).Context, val);
                if (val3.IsSuccess && list.Count > 0)
                {
                    string targetTableName = ((billName == "PUR_PurchaseOrder") ? "T_PUR_POORDERENTRY_R" : ((billName == "PUR_Requisition") ? "T_PUR_REQENTRY_R" : "T_PRD_MOENTRY"));
                    ClearTargetBillSourceInfo(((AbstractOperationServicePlugIn)this).Context, list, targetTableName);
                }
            }
        }

        private void ClearTargetBillSourceInfo(Context ctx, List<long> targetEntryId, string targetTableName)
        {
            string arg = ((targetTableName == "T_PRD_MOENTRY") ? "FSRCBILLTYPE=' ',FSRCBILLNO=N' ',FSRCBILLENTRYSEQ=0" : "FSRCBILLTYPEID=' ',FSRCBILLNO=N' '");
            string text = string.Format("Update {0} set {1}\r\nwhere EXISTS (SELECT {2} 1 FROM TABLE(fn_StrSplit(@FENTRYID,',',1)) B WHERE B.FID=FENTRYID)", targetTableName, arg, SCMCommonUtil.GetCardinalityString(ctx, "B", (long)targetEntryId.Count));
            List<SqlObject> list = new List<SqlObject>();
            List<SqlParam> list2 = new List<SqlParam>();
            list2.Add(new SqlParam("@FENTRYID", (KDDbType)161, (object)targetEntryId.ToArray()));
            list.Add(new SqlObject(text, list2));
            DBUtils.ExecuteBatch(((AbstractOperationServicePlugIn)this).Context, list);
        }

        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            bool flag = false;
            if (((AbstractOperationServicePlugIn)this).Option != null)
            {
                ((AbstractOperationServicePlugIn)this).Option.TryGetVariableValue<bool>("IsTakeEffect", out flag);
            }
            if (!OperateOptionUtils.GetIsWFInvokeFlag(((AbstractOperationServicePlugIn)this).Option) || flag)
            {
                wb.InitializeBillRecPlans(((AbstractOperationServicePlugIn)this).Context, e.SelectedRows);
            }
            if (e.SelectedRows == null || e.SelectedRows.Count() <= 0)
            {
                return;
            }
            ISaleService service = Kingdee.BOS.App.ServiceHelper.GetService<ISaleService>();
            SuiteRelateFieldPropName val = new SuiteRelateFieldPropName();
            val.RowTypeFieldProp = ("RowType");
            val.MaterialFieldProp = ("MaterialId");
            val.StockFieldProp = ("SOStockId");
            val.StockLocFieldProp = ("SOStockLocalId");
            SetVersionNoBeforeSave(e.SelectedRows.First().DataEntity);
            foreach (ExtendedDataEntity selectedRow in e.SelectedRows)
            {
                DynamicObject dataEntity = selectedRow.DataEntity;
                if (dataEntity == null)
                {
                    continue;
                }
                string text = Convert.ToString(dataEntity["ContractType"]);
                long num = Convert.ToInt64(dataEntity["ContractId"]);
                if ((text == "3" || text == "4") && num > 0)
                {
                    BuildLinkData(((AbstractOperationServicePlugIn)this).Context, dataEntity, text, num);
                }
                object obj = dataEntity["SaleOrgId"];
                DynamicObject val2 = (DynamicObject)((obj is DynamicObject) ? obj : null);
                long num2 = ((val2 == null) ? 0 : Convert.ToInt64(val2["Id"]));
                if (num2 == 0)
                {
                    continue;
                }
                object obj2 = dataEntity["SaleOrderEntry"];
                DynamicObjectCollection val3 = (DynamicObjectCollection)((obj2 is DynamicObjectCollection) ? obj2 : null);
                if (val3 == null || ((Collection<DynamicObject>)(object)val3).Count < 1)
                {
                    continue;
                }
                ICommonService service2 = Kingdee.BOS.App.ServiceHelper.GetService<ICommonService>();
                object systemProfile = service2.GetSystemProfile(((AbstractOperationServicePlugIn)this).Context, num2, "SAL_SystemParameter", "SOChangeType", (object)"A");
                string text2 = ((systemProfile == null) ? "A" : Convert.ToString(systemProfile));
                string text3 = Convert.ToString(dataEntity["DocumentStatus"]);
                val3 = service.SetRightSuiteRowData(((AbstractOperationServicePlugIn)this).Context, val3, val);
                Dictionary<int, string> dictionary = new Dictionary<int, string>();
                foreach (DynamicObject item in (Collection<DynamicObject>)(object)val3)
                {
                    if (string.IsNullOrWhiteSpace(Convert.ToString(item["RowId"])))
                    {
                        string text4 = SequentialGuid.NewGuid().ToString();
                        int key = Convert.ToInt32(item["Seq"]);
                        item["RowId"] = (object)text4;
                        if (!dictionary.ContainsKey(key))
                        {
                            dictionary.Add(key, text4);
                        }
                    }
                    object obj3 = item["OrderEntryPlan"];
                    DynamicObjectCollection source = (DynamicObjectCollection)((obj3 is DynamicObjectCollection) ? obj3 : null);
                    object obj4 = ((IEnumerable<DynamicObject>)source).Select((DynamicObject p) => p["PlanDeliveryDate"]).Min();
                    item["MinPlanDeliveryDate"] = obj4;
                    item["OldQty"] = item["Qty"];
                    if (((StringUtils.EqualsIgnoreCase(text2, "B") && StringUtils.EqualsIgnoreCase(text3, "C")) || flag) && StringUtils.EqualsIgnoreCase(Convert.ToString(item["ChangeFlag"]), ((object)(ChangeFlag)1).ToString()))
                    {
                        item["ChangeFlag"] = (object)(ChangeFlag)2;
                    }
                }
                object obj5 = ((Collection<DynamicObject>)(DynamicObjectCollection)selectedRow.DataEntity["SaleOrderFinance"])[0]["RecConditionId"];
                DynamicObject val4 = (DynamicObject)((obj5 is DynamicObject) ? obj5 : null);
                if (val4 != null && Convert.ToInt16(val4["RECMETHOD"]) == 3)
                {
                    object obj6 = selectedRow.DataEntity["SaleOrderPlan"];
                    DynamicObjectCollection val5 = (DynamicObjectCollection)((obj6 is DynamicObjectCollection) ? obj6 : null);
                    List<DynamicObject> list = new List<DynamicObject>();
                    long num3 = 0L;
                    foreach (DynamicObject item2 in (Collection<DynamicObject>)(object)val5)
                    {
                        if (string.IsNullOrWhiteSpace(Convert.ToString(item2["MaterialRowID"])))
                        {
                            int key2 = Convert.ToInt32(item2["MaterialSeq"]);
                            if (dictionary.ContainsKey(key2))
                            {
                                item2["MaterialRowID"] = (object)dictionary[key2];
                            }
                        }
                        num3 = ((item2["PlanMaterialId_Id"] == null) ? 0 : Convert.ToInt64(item2["PlanMaterialId_Id"]));
                        if (num3 == 0)
                        {
                            list.Add(item2);
                        }
                    }
                    foreach (DynamicObject item3 in list)
                    {
                        ((Collection<DynamicObject>)(object)val5).Remove(item3);
                    }
                    List<DynamicObject> list2 = ((IEnumerable<DynamicObject>)val5).OrderBy((DynamicObject p) => Convert.ToInt32(p["MaterialSeq"])).ToList();
                    ((Collection<DynamicObject>)(object)val5).Clear();
                    int num4 = 1;
                    decimal num5 = 0.0m;
                    object obj7 = selectedRow.DataEntity["SaleOrderFinance"];
                    DynamicObjectCollection val6 = (DynamicObjectCollection)((obj7 is DynamicObjectCollection) ? obj7 : null);
                    decimal num6 = 0.0m;
                    if (val6 != null && ((Collection<DynamicObject>)(object)val6).Count > 0)
                    {
                        num6 = Convert.ToDecimal(((Collection<DynamicObject>)(object)val6)[0]["BillAllAmount"]);
                    }
                    foreach (DynamicObject item4 in list2)
                    {
                        item4["Seq"] = (object)num4++;
                        num5 += Convert.ToDecimal(item4["RecAdvanceAmount"]);
                        ((Collection<DynamicObject>)(object)val5).Add(item4);
                    }
                    if (num4 - 2 >= 0)
                    {
                        ((Collection<DynamicObject>)(object)val5)[num4 - 2]["RecAdvanceAmount"] = (object)(Convert.ToDecimal(((Collection<DynamicObject>)(object)val5)[num4 - 2]["RecAdvanceAmount"]) + num6 - num5);
                    }
                }
                DynamicObjectCollection val7 = (DynamicObjectCollection)dataEntity["SaleOrderPlan"];
                if (val7 == null || ((Collection<DynamicObject>)(object)val7).Count <= 0)
                {
                    continue;
                }
                int count = ((Collection<DynamicObject>)(object)val7).Count;
                for (int i = 0; i < count; i++)
                {
                    DynamicObject val8 = ((Collection<DynamicObject>)(object)val7)[i];
                    if (val8 == null)
                    {
                        continue;
                    }
                    bool flag2 = Convert.ToBoolean(val8["NeedRecAdvance"]);
                    string value = Convert.ToString(val8["RelBillNo"]);
                    if (!flag2 || string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }
                    object obj8 = val8["SAL_ORDERPLANENTRY"];
                    DynamicObjectCollection val9 = (DynamicObjectCollection)((obj8 is DynamicObjectCollection) ? obj8 : null);
                    if (val9 == null || ((Collection<DynamicObject>)(object)val9).Count <= 0)
                    {
                        continue;
                    }
                    _ = ((Collection<DynamicObject>)(object)val9).Count;
                    for (int num7 = ((Collection<DynamicObject>)(object)val9).Count - 1; num7 >= 0; num7--)
                    {
                        DynamicObject val10 = ((Collection<DynamicObject>)(object)val9)[num7];
                        if (val10 != null)
                        {
                            decimal num8 = Convert.ToDecimal(val10["ActRecAmount"]);
                            decimal num9 = Convert.ToDecimal(val10["OverRecAmount"]);
                            if (num8 == 0m && num9 == 0m)
                            {
                                ((Collection<DynamicObject>)(object)val9).Remove(val10);
                            }
                        }
                    }
                }
            }
        }

        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            //SaveValidator saveValidator = new SaveValidator();
            Kingdee.Mymooo.ServicePlugIn.SaleXBill.SaleXBillSaveValidator saveValidator = new SaleXBill.SaleXBillSaveValidator();
            ((AbstractValidator)saveValidator).AlwaysValidate = (true);
            ((AbstractValidator)saveValidator).EntityKey = ("FBillHead");
            ((AbstractOperationServicePlugIn)this).Option.TryGetVariableValue<bool>("IsOrderChange", out isOrderChange);
            saveValidator.IsOrderChange = isOrderChange;
            e.Validators.Add((AbstractValidator)(object)saveValidator);
            FreezeCustValidator val = new FreezeCustValidator();
            ((AbstractValidator)val).AlwaysValidate = (true);
            ((AbstractValidator)val).EntityKey = ("FBillHead");
            val.billHeadOrm = ("SaleOrder");
            val.freezeType = ("SaleOrder");
            val.custId = ("CustId_Id");
            e.Validators.Add((AbstractValidator)(object)val);
            IMetaDataService metaDataService = ServiceFactory.GetMetaDataService(((AbstractOperationServicePlugIn)this).Context);
            FormMetadata val2 = (FormMetadata)metaDataService.Load(((AbstractOperationServicePlugIn)this).Context, ((AbstractOperationServicePlugIn)this).BusinessInfo.GetForm().ParameterObjectId, true);
            IUserParameterService userParameterService = ServiceFactory.GetUserParameterService(((AbstractOperationServicePlugIn)this).Context);
            DynamicObject val3 = userParameterService.Load(((AbstractOperationServicePlugIn)this).Context, val2.BusinessInfo, ((AbstractOperationServicePlugIn)this).Context.UserId, ((AbstractElement)((AbstractOperationServicePlugIn)this).BusinessInfo.GetForm()).Id, "UserParameter");
            bool flag = false;
            bool flag2 = false;
            if (val3 != null && ((KeyedCollectionBase<string, DynamicProperty>)(object)val3.DynamicObjectType.Properties).ContainsKey("AuxMustInput") && val3["AuxMustInput"] != null && !string.IsNullOrWhiteSpace(val3["AuxMustInput"].ToString()))
            {
                flag = Convert.ToBoolean(val3["AuxMustInput"]);
            }
            bool flag3 = false;
            bool flag4 = false;
            if (val3 != null && ((KeyedCollectionBase<string, DynamicProperty>)(object)val3.DynamicObjectType.Properties).ContainsKey("AuxAffectPlanNotMustInput") && val3["AuxAffectPlanNotMustInput"] != null && !string.IsNullOrWhiteSpace(val3["AuxAffectPlanNotMustInput"].ToString()))
            {
                flag3 = Convert.ToBoolean(val3["AuxAffectPlanNotMustInput"]);
            }
            if (e.DataEntities != null && e.DataEntities.Count() > 0)
            {
                DynamicObject val4 = e.DataEntities[0];
                object obj = val4["BillTypeId"];
                DynamicObject val5 = (DynamicObject)((obj is DynamicObject) ? obj : null);
                string text = ((val5 == null) ? "" : Convert.ToString(val5["Id"]));
                if (!string.IsNullOrWhiteSpace(text))
                {
                    ICommonService service = Kingdee.K3.SCM.App.ServiceHelper.GetService<ICommonService>();
                    flag2 = Convert.ToBoolean(service.GetBillTypeParamProfile(((AbstractOperationServicePlugIn)this).Context, text, "Sal_SaleOrderBillTypeParaSetting", "AuxMustInput", (object)"false"));
                    flag4 = Convert.ToBoolean(service.GetBillTypeParamProfile(((AbstractOperationServicePlugIn)this).Context, text, "Sal_SaleOrderBillTypeParaSetting", "AuxAffectPlanNotMustInput", (object)"false"));
                }
            }
            MaterialAuxPtyItemsValueValidator val6 = new MaterialAuxPtyItemsValueValidator();
            ((AbstractValidator)val6).AlwaysValidate = (true);
            ((AbstractValidator)val6).EntityKey = ("FSaleOrderEntry");
            val6.MaterialName = ("MaterialID");
            val6.AuxPtyName = ("AuxPropId");
            if (flag2 || flag)
            {
                e.Validators.Add((AbstractValidator)(object)val6);
            }
            else if (!flag4 && !flag3)
            {
                val6.IsCheckAffectPlan = (true);
                e.Validators.Add((AbstractValidator)(object)val6);
            }
            bool flag5 = false;
            if (val3 != null && ((KeyedCollectionBase<string, DynamicProperty>)(object)val3.DynamicObjectType.Properties).ContainsKey("BomMustInput") && val3["BomMustInput"] != null && !string.IsNullOrWhiteSpace(val3["BomMustInput"].ToString()))
            {
                flag5 = Convert.ToBoolean(val3["BomMustInput"]);
            }
            if (flag5)
            {
                MaterialBomValueValidator val7 = new MaterialBomValueValidator();
                ((AbstractValidator)val7).AlwaysValidate = (true);
                ((AbstractValidator)val7).EntityKey = ("FSaleOrderEntry");
                val7.MaterialName = ("MaterialID");
                val7.BomPtyName = ("BomId");
                e.Validators.Add((AbstractValidator)(object)val7);
            }
            SaleSuiteCommonValidator saleSuiteCommonValidator = new SaleSuiteCommonValidator();
            ((AbstractValidator)saleSuiteCommonValidator).AlwaysValidate = (true);
            ((AbstractValidator)saleSuiteCommonValidator).EntityKey = ("FBillHead");
            e.Validators.Add((AbstractValidator)(object)saleSuiteCommonValidator);
            ReceiveBillAmountValidator receiveBillAmountValidator = new ReceiveBillAmountValidator();
            ((AbstractValidator)receiveBillAmountValidator).EntityKey = ("FBillHead");
            ((AbstractValidator)receiveBillAmountValidator).AlwaysValidate = (true);
            e.Validators.Add((AbstractValidator)(object)receiveBillAmountValidator);
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            if (((OperationTransactionArgs)e).DataEntitys == null || ((OperationTransactionArgs)e).DataEntitys.Count() == 0)
            {
                return;
            }
            bool flag = false;
            bool flag2 = false;
            if (((AbstractOperationServicePlugIn)this).Option != null)
            {
                ((AbstractOperationServicePlugIn)this).Option.TryGetVariableValue<bool>("IsTakeEffect", out flag);
                ((AbstractOperationServicePlugIn)this).Option.TryGetVariableValue<bool>("IsModelDataChangedByWFInvoke", out flag2);
            }
            if (!StringUtils.EqualsIgnoreCase(Convert.ToString(((OperationTransactionArgs)e).DataEntitys[0]["FFormId"]), "SAL_XORDER"))
            {
                bool isWFInvokeFlag = OperateOptionUtils.GetIsWFInvokeFlag(((AbstractOperationServicePlugIn)this).Option);
                if ((isWFInvokeFlag && flag2) || !isWFInvokeFlag || flag)
                {
                    wb.WriteBackForSave(e, ((AbstractOperationServicePlugIn)this).Context);
                }
            }
            List<long> list = new List<long>();
            List<long> list2 = new List<long>();
            DynamicObject val = null;
            long num = 0L;
            long num2 = 0L;
            bool flag3 = false;
            string text = "";
            long num3 = 0L;
            decimal num4 = 0m;
            decimal num5 = 0m;
            _ = string.Empty;
            DynamicObjectCollection val2 = null;
            List<long> list3 = new List<long>();
            List<DynamicObject> list4 = new List<DynamicObject>();
            List<SqlObject> list5 = new List<SqlObject>();
            string empty = string.Empty;
            DynamicObject[] dataEntitys = ((OperationTransactionArgs)e).DataEntitys;
            foreach (DynamicObject val3 in dataEntitys)
            {
                object obj = val3["CustId"];
                val = (DynamicObject)((obj is DynamicObject) ? obj : null);
                num = ((val == null) ? 0 : Convert.ToInt64(val["Id"]));
                if (val != null)
                {
                    flag3 = Convert.ToBoolean(val["IsTrade"]);
                }
                if (!flag3 && !list.Contains(num))
                {
                    list.Add(num);
                }
                object obj2 = ((Collection<DynamicObject>)(DynamicObjectCollection)val3["SaleOrderFinance"])[0]["RecConditionId"];
                DynamicObject val4 = (DynamicObject)((obj2 is DynamicObject) ? obj2 : null);
                if (val4 != null && val4 != null && Convert.ToInt16(val4["RECMETHOD"]) == 3)
                {
                    num2 = Convert.ToInt64(val3["Id"]);
                    if (!list2.Contains(num2))
                    {
                        list2.Add(num2);
                    }
                }
                object obj3 = val3["SaleOrderEntry"];
                val2 = (DynamicObjectCollection)((obj3 is DynamicObjectCollection) ? obj3 : null);
                text = Convert.ToString(val3["ChangeReason"]);
                if (flag || !ObjectUtils.IsNullOrEmptyOrWhiteSpace((object)text))
                {
                    list4.Add(val3);
                    foreach (DynamicObject item3 in (Collection<DynamicObject>)(object)val2)
                    {
                        num3 = Convert.ToInt64(item3["Id"]);
                        num4 = Convert.ToDecimal(item3["LOCKQTY"]);
                        num5 = Convert.ToDecimal(item3["LeftQty"]);
                        if (num4 > 0m || num5 > 0m)
                        {
                            list3.Add(num3);
                        }
                    }
                }
                string text2 = Convert.ToString(val3["ContractType"]);
                long num6 = Convert.ToInt64(val3["ContractId"]);
                switch (text2)
                {
                    case "3":
                    case "4":
                    case "5":
                        if (num6 > 0)
                        {
                            empty = string.Format("UPDATE T_CRM_CONTRACTFIN SET FORDERAMOUNT=(SELECT SUM(FENTRYORDERAMOUNT) FROM T_CRM_CONTRACTENTRY_R WHERE FID={0}) WHERE FID={0} ", num6);
                            list5.Add(new SqlObject(empty, new List<SqlParam>()));
                            empty = $"UPDATE T_CRM_CONTRACTFIN SET FORDERREMAINAMOUNT=(CASE WHEN FBILLALLAMOUNT-FORDERAMOUNT<0 THEN 0 ELSE FBILLALLAMOUNT-FORDERAMOUNT END) WHERE FID={num6} ";
                            list5.Add(new SqlObject(empty, new List<SqlParam>()));
                        }
                        break;
                }
            }
            if (list != null && list.Count > 0)
            {
                empty = string.Format("Merge into T_BD_CUSTOMER TT Using \r\n                    (Select Cust.FCustId,1 as FISTRADE From T_BD_CUSTOMER Cust \r\n                        Inner Join (SELECT {0} FID FROM table(fn_StrSplit(@CustId,',',1))B) T1 on T1.FId=Cust.FCustId \r\n                    ) UT on (TT.FCustId = UT.FCustId) \r\n                    When Matched Then Update Set TT.FISTRADE = UT.FISTRADE ", SCMCommonUtil.GetCardinalityString(((AbstractOperationServicePlugIn)this).Context, "B", (long)list.Distinct().Count()));
                SqlParam val5 = new SqlParam("@CustId", (KDDbType)161, (object)list.Distinct().ToArray());
                SqlObject item = new SqlObject(empty, val5);
                list5.Add(item);
            }
            if (list2 != null && list2.Count > 0)
            {
                empty = string.Format("Merge into T_SAL_ORDERPLAN TT Using \r\n                    (SELECT DISTINCT P.FENTRYID AS FPLANENTRYID,PE.FENTRYID AS FORDERENTRYID FROM T_SAL_ORDERENTRY PE\r\n                        left join T_SAL_ORDERENTRY_E EE on PE.FENTRYID=EE.FENTRYID\r\n                        left join T_SAL_ORDERPLAN P ON P.FID = PE.FID AND P.FMATERIALROWID = EE.FROWID\r\n                        Inner Join (SELECT {0} FID FROM table(fn_StrSplit(@FId,',',1))B) T1 on T1.FId=PE.FID \r\n                        WHERE P.FORDERENTRYID=0\r\n                    ) UT ON (TT.FENTRYID = UT.FPLANENTRYID)\r\n                    When Matched Then Update Set TT.FORDERENTRYID = UT.FORDERENTRYID ", SCMCommonUtil.GetCardinalityString(((AbstractOperationServicePlugIn)this).Context, "B", (long)list2.Distinct().Count()));
                SqlParam val6 = new SqlParam("@FId", (KDDbType)161, (object)list2.Distinct().ToArray());
                SqlObject item2 = new SqlObject(empty, val6);
                list5.Add(item2);
            }
            if (list5.Count > 0)
            {
                DBUtils.ExecuteBatch(((AbstractOperationServicePlugIn)this).Context, list5);
            }
            if (list4.Count > 0)
            {
                IBusinessFlowTrackerService service = Kingdee.K3.SCM.App.ServiceHelper.GetService<IBusinessFlowTrackerService>();
                service.Audit(((AbstractOperationServicePlugIn)this).Context, list4.ToArray(), ((AbstractOperationServicePlugIn)this).BusinessInfo, ((AbstractOperationServicePlugIn)this).Option);
            }
            ISaleService service2 = Kingdee.K3.SCM.App.ServiceHelper.GetService<ISaleService>();
            if (list3.Count > 0)
            {
                service2.BatchUpdateLockQtyAndLockFlag(((AbstractOperationServicePlugIn)this).Context, list3);
            }
            if (list4.Count <= 0 || StringUtils.EqualsIgnoreCase(((OperationTransactionArgs)e).DataEntitys[0]["FFormId"].ToString(), "SAL_XORDER"))
            {
                return;
            }
            List<DynamicObject> list6 = list4.Where((DynamicObject x) => StringUtils.EqualsIgnoreCase(Convert.ToString(x["BillTypeId_Id"]), "88de45ad2499437f9b21096997b297e2") && Convert.ToBoolean(x["IsUseOEMBomPush"])).ToList();
            if (list6.Count > 0)
            {
                IOEMBomService service3 = Kingdee.K3.SCM.App.ServiceHelper.GetService<IOEMBomService>();
                IOperationResult val7 = service3.OrderChangeUpdateOEMBom(((AbstractOperationServicePlugIn)this).Context, list6.ToArray());
                if (!val7.IsSuccess)
                {
                    ((AbstractOperationServicePlugIn)this).OperationResult.ValidationErrors.AddRange(val7.ValidationErrors);
                }
            }
        }

        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            AutoLockFunc autoLockFunc = new AutoLockFunc((AbstractOperationServicePlugIn)(object)this, versionNoBeforeSave);
            autoLockFunc.DoWork(e);
        }

        private void SetVersionNoBeforeSave(DynamicObject dataEntity)
        {
            long num = Convert.ToInt64(dataEntity["Id"]);
            string text = $"SELECT FVERSIONNO FROM T_SAL_ORDER WHERE FID = {num}";
            versionNoBeforeSave = DBUtils.ExecuteScalar<string>(((AbstractOperationServicePlugIn)this).Context, text, (string)null, (SqlParam[])(object)new SqlParam[0]);
        }

        private void BuildLinkData(Context ctx, DynamicObject billData, string contractType, long contractId)
        {
            object obj = billData["SaleOrderEntry"];
            DynamicObjectCollection val = (DynamicObjectCollection)((obj is DynamicObjectCollection) ? obj : null);
            if (val == null || ((Collection<DynamicObject>)(object)val).Count < 1)
            {
                return;
            }
            bool flag = false;
            foreach (DynamicObject item in (Collection<DynamicObject>)(object)val)
            {
                object obj2 = item["FSaleOrderEntry_Link"];
                DynamicObjectCollection val2 = (DynamicObjectCollection)((obj2 is DynamicObjectCollection) ? obj2 : null);
                if (val2 == null || ((Collection<DynamicObject>)(object)val2).Count == 0)
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                RebuildContractLink(ctx, val, contractId, contractType);
            }
        }

        private void RebuildContractLink(Context ctx, DynamicObjectCollection entries, long contractId, string contractType)
        {
            string empty = string.Empty;
            Dictionary<string, decimal> dictionary = new Dictionary<string, decimal>();
            empty = $"select E.FENTRYID,E.FMATERIALGROUP,F.FALLAMOUNT,R.FALLORDERAMOUNT from T_CRM_CONTRACTENTRY E left join T_CRM_CONTRACTENTRY_F F on E.FENTRYID=F.FENTRYID\r\n            left join T_CRM_CONTRACTENTRY_R R on F.FENTRYID=R.FENTRYID where E.FID={contractId} order by E.FSEQ ASC ";
            DynamicObjectCollection val = DBUtils.ExecuteDynamicObject(ctx, empty, (IDataEntityType)null, (IDictionary<string, Type>)null, CommandType.Text, (SqlParam[])(object)new SqlParam[0]);
            if (val == null || ((Collection<DynamicObject>)(object)val).Count < 1)
            {
                return;
            }
            _ = ((Collection<DynamicObject>)(object)val).Count;
            List<string> list = new List<string>();
            List<long> list2 = new List<long>();
            foreach (DynamicObject item in (Collection<DynamicObject>)(object)val)
            {
                string text = Convert.ToString(item["FENTRYID"]);
                long num = Convert.ToInt64(item["FMATERIALGROUP"]);
                string empty2 = string.Empty;
                if (contractType == "3")
                {
                    empty2 = text;
                }
                else
                {
                    empty2 = text + "|" + Convert.ToString(num);
                    if (!list2.Contains(num))
                    {
                        list2.Add(num);
                    }
                }
                list.Add(empty2);
                decimal num2 = Convert.ToDecimal(item["FALLAMOUNT"]);
                decimal num3 = Convert.ToDecimal(item["FALLORDERAMOUNT"]);
                if (!dictionary.ContainsKey(empty2))
                {
                    dictionary.Add(empty2, num2 - num3);
                }
            }
            empty = $"select FBILLNO from T_CRM_CONTRACT where FID={contractId}";
            string text2 = DBUtils.ExecuteScalar<string>(ctx, empty, " ", (SqlParam[])(object)new SqlParam[0]);
            foreach (DynamicObject item2 in (Collection<DynamicObject>)(object)entries)
            {
                string empty3 = string.Empty;
                decimal num4 = Convert.ToDecimal(item2["AllAmount"]);
                object obj = item2["FSaleOrderEntry_Link"];
                DynamicObjectCollection val2 = (DynamicObjectCollection)((obj is DynamicObjectCollection) ? obj : null);
                if (val2 != null && ((Collection<DynamicObject>)(object)val2).Count > 0)
                {
                    DynamicObject val3 = ((Collection<DynamicObject>)(object)val2)[0];
                    if (val3 != null)
                    {
                        string text3 = Convert.ToString(val3["SID"]);
                        object obj2 = item2["MaterialGroup"];
                        DynamicObject val4 = (DynamicObject)((obj2 is DynamicObject) ? obj2 : null);
                        string text4 = ((val4 == null) ? "0" : Convert.ToString(val4["Id"]));
                        empty3 = ((!(contractType == "3")) ? (text3 + "|" + text4) : text3);
                        if (dictionary.ContainsKey(empty3))
                        {
                            dictionary[empty3] -= num4;
                        }
                    }
                    continue;
                }
                object obj3 = item2["MaterialGroup"];
                DynamicObject val5 = (DynamicObject)((obj3 is DynamicObject) ? obj3 : null);
                long num5 = ((val5 == null) ? 0 : Convert.ToInt64(val5["Id"]));
                if (contractType == "4" && (num5 == 0 || (list2.Count > 0 && !list2.Contains(num5))))
                {
                    continue;
                }
                long num6 = 0L;
                string key = "0";
                for (int i = 0; i < list.Count; i++)
                {
                    string text5 = list[i];
                    decimal num7 = dictionary[text5];
                    if (num7 > 0m)
                    {
                        num6 = ((!(contractType == "3")) ? Convert.ToInt64(text5.Split('|')[0]) : Convert.ToInt64(text5));
                        key = text5;
                        break;
                    }
                }
                if (num6 == 0)
                {
                    num6 = ((!(contractType == "3")) ? Convert.ToInt64(list[list.Count - 1].Split('|')[0]) : Convert.ToInt64(list[list.Count - 1]));
                    key = list[list.Count - 1];
                }
                item2["SrcType"] = (object)"CRM_Contract";
                item2["SrcBillNo"] = (object)text2;
                DynamicObject val6 = new DynamicObject(val2.DynamicCollectionItemPropertyType);
                IDBService service = ServiceFactory.GetService<IDBService>(ctx);
                List<long> list3 = new List<long>();
                list3 = service.GetSequenceInt64(ctx, "T_SAL_ORDERENTRY_LK", 1).ToList();
                long num8 = 0L;
                if (list3 != null && list3.Count > 0)
                {
                    num8 = list3[0];
                }
                if (val6.DynamicObjectType.Properties.Contains("Id"))
                {
                    val6["Id"] = (object)num8;
                }
                if (val6.DynamicObjectType.Properties.Contains("RuleId"))
                {
                    val6["RuleId"] = (object)"SaleContractToSaleOrder";
                }
                if (val6.DynamicObjectType.Properties.Contains("STableName"))
                {
                    val6["STableName"] = (object)"T_CRM_CONTRACTENTRY";
                }
                if (val6.DynamicObjectType.Properties.Contains("SBillId"))
                {
                    val6["SBillId"] = (object)contractId;
                }
                if (val6.DynamicObjectType.Properties.Contains("SId"))
                {
                    val6["SId"] = (object)num6;
                }
                if (val6.DynamicObjectType.Properties.Contains("BaseUnitQtyOld"))
                {
                    val6["BaseUnitQtyOld"] = (object)Convert.ToDecimal(item2["BaseUnitQty"]);
                }
                if (val6.DynamicObjectType.Properties.Contains("BaseUnitQty"))
                {
                    val6["BaseUnitQty"] = (object)Convert.ToDecimal(item2["BaseUnitQty"]);
                }
                if (val6.DynamicObjectType.Properties.Contains("StockBaseQtyOld"))
                {
                    val6["StockBaseQtyOld"] = (object)Convert.ToDecimal(item2["StockBaseQty"]);
                }
                if (val6.DynamicObjectType.Properties.Contains("StockBaseQty"))
                {
                    val6["StockBaseQty"] = (object)Convert.ToDecimal(item2["StockBaseQty"]);
                }
                ((Collection<DynamicObject>)(object)val2).Add(val6);
                dictionary[key] -= num4;
            }
        }
    }
}
