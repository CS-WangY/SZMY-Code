using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Collections.Generic;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.App.Sal.ServicePlugIn;
using Kingdee.K3.SCM.App.Sal.ServicePlugIn.SaleOrder;
using Kingdee.K3.SCM.App.Sal.ServicePlugIn.SaleOrderX;
using Kingdee.K3.SCM.Contracts;
using Kingdee.K3.SCM.Core.SAL;
using static Kingdee.K3.Core.SCM.PUR.PurchaseOrderChangeFlagEnum;
using ServiceFactory = Kingdee.K3.SCM.Contracts.ServiceFactory;
using Kingdee.K3.SCM.Common.BusinessEntity.Common;

namespace Kingdee.Mymooo.ServicePlugIn.SaleXBill
{
    [Description("销售订单新变更单深圳蚂蚁通版标准保存操作服务端插件"),HotUpdate]
    public class Save : AbstractSaleOrderXOprerationServicePlugIn
    {
        private string _previousStatus = "";

        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            //((AbstractOperationServicePlugIn)this).BeginOperationTransaction(e);
            _previousStatus = Convert.ToString(((OperationTransactionArgs)e).DataEntitys[0]["DocumentStatus"]);
            bool para_IsUseTaxCombination = false;
            DynamicObject[] dataEntitys = ((OperationTransactionArgs)e).DataEntitys;
            foreach (DynamicObject val in dataEntitys)
            {
                if (Convert.ToInt64(val["Id"]) > 0)
                {
                    XSaleOrderCommon.SetXPKID(val);
                }
                try
                {
                    long num = Convert.ToInt64(val["PKIDX"]);
                    long orgId = Convert.ToInt64(val["SaleOrgId_Id"]);
                    object paramter = BOS.App.ServiceHelper.GetService<ISystemParameterService>().GetParamter(((AbstractOperationServicePlugIn)this).Context, -1L, -1L, "EnalbeTaxMixSystemParameter", "ENABLEDTAXMIX", 0L);
                    if (paramter != null)
                    {
                        para_IsUseTaxCombination = Convert.ToBoolean(paramter);
                    }
                    XSaleOrderCommon val2 = new XSaleOrderCommon(((AbstractOperationServicePlugIn)this).Context);
                    FormMetadata val3 = (FormMetadata)Kingdee.BOS.App.ServiceHelper.GetService<IMetaDataService>().Load(((AbstractOperationServicePlugIn)this).Context, "SAL_SaleOrder", true);
                    val2.orderBusinessInfo = val3.BusinessInfo;
                    DynamicObject[] array = Kingdee.BOS.App.ServiceHelper.GetService<IViewService>().Load(((AbstractOperationServicePlugIn)this).Context, new object[1] { num }, val3.BusinessInfo.GetDynamicObjectType());
                    DynamicObject[] array2 = Kingdee.BOS.App.ServiceHelper.GetService<IViewService>().Load(((AbstractOperationServicePlugIn)this).Context, new object[1] { num }, val3.BusinessInfo.GetDynamicObjectType());
                    if (array == null || array.Count() < 1 || array2 == null || array2.Count() < 1)
                    {
                        continue;
                    }
                    val2.lstNotFields = (((AbstractOperationServicePlugIn)this).Option.GetVariableValue<List<string>>("TakeEffectNotWriteBackFields", new List<string>()));
                    val2.baseOrderData = (array2[0]);
                    val2.changeOrderData = (val);
                    val2.para_IsUseTaxCombination = (para_IsUseTaxCombination);
                    val2.SetCommonVarValue();
                    List<long> list = new List<long>();
                    if (val2.changeOrderData != null)
                    {
                        object obj = val2.changeOrderData["SaleOrderFinance"];
                        DynamicObjectCollection val4 = (DynamicObjectCollection)((obj is DynamicObjectCollection) ? obj : null);
                        if (val4 != null && ((Collection<DynamicObject>)(object)val4).Count > 0 && !ObjectUtils.IsNullOrEmptyOrWhiteSpace(((Collection<DynamicObject>)(object)val4)[0]["PARTOFENTRYIDS"]))
                        {
                            string[] array3 = ((Collection<DynamicObject>)(object)val4)[0]["PARTOFENTRYIDS"].ToString().Split(',');
                            string[] array4 = array3;
                            foreach (string text in array4)
                            {
                                if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace((object)text))
                                {
                                    list.Add(Convert.ToInt64(text));
                                }
                            }
                        }
                    }
                    bool notCountSonAmount = GetNotCountSonAmount(orgId);
                    List<string> notCountSonAmountFields = GetNotCountSonAmountFields(val3);
                    val2.SetNewDataFromOriginalData(val2.baseOrderData, val2.changeOrderData, true, val2.lstNotFields, list, true, notCountSonAmount, notCountSonAmountFields);
                    try
                    {
                        if (!val2.CheckNeedToReCountRecPlan(val2.baseOrderData, array[0], val))
                        {
                            continue;
                        }
                        object obj2 = val["SaleOrderPlan"];
                        DynamicObjectCollection val5 = (DynamicObjectCollection)((obj2 is DynamicObjectCollection) ? obj2 : null);
                        object obj3 = val2.baseOrderData["SaleOrderPlan"];
                        DynamicObjectCollection val6 = (DynamicObjectCollection)((obj3 is DynamicObjectCollection) ? obj3 : null);
                        XSaleOrderCommon.DealWithChangeBillRecPlan(val5, val6);
                        if (!((IEnumerable<DynamicObject>)val5).ToList().Exists((DynamicObject n) => Convert.ToInt64(n["Id"]) <= 0))
                        {
                            continue;
                        }
                        List<DynamicObject> list2 = (from n in ((IEnumerable<DynamicObject>)val5).ToList()
                                                     where Convert.ToInt64(n["Id"]) <= 0
                                                     select n into k
                                                     orderby Convert.ToInt32(k["Seq"])
                                                     select k).ToList();
                        IDBService dBService = BOS.Contracts.ServiceFactory.GetDBService(((AbstractOperationServicePlugIn)this).Context);
                        List<long> list3 = dBService.GetSequenceInt64(((AbstractOperationServicePlugIn)this).Context, "T_SAL_XORDERPLAN", list2.Count).ToList();
                        for (int l = 0; l < list2.Count; l++)
                        {
                            if (list3.Count > l)
                            {
                                list2[l]["Id"] = (object)list3[l];
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerWithContext.Error(((AbstractOperationServicePlugIn)this).Context, "SCM", ex.Message, ex);
                    }
                }
                catch (Exception ex2)
                {
                    LoggerWithContext.Error(((AbstractOperationServicePlugIn)this).Context, "SCM", ex2.Message, ex2);
                }
            }
        }

        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            SaleXBillSaveValidator saleXBillSaveValidator = new SaleXBillSaveValidator();
            //SaveValidator saveValidator = new SaveValidator();
            ((AbstractValidator)saleXBillSaveValidator).AlwaysValidate = (true);
            ((AbstractValidator)saleXBillSaveValidator).EntityKey = "FBillHead";
            saleXBillSaveValidator.IsOrderChange = true;
            e.Validators.Add((AbstractValidator)(object)saleXBillSaveValidator);
            SOXSaveValidator sOXSaveValidator = new SOXSaveValidator();
            ((AbstractValidator)sOXSaveValidator).AlwaysValidate = (true);
            ((AbstractValidator)sOXSaveValidator).EntityKey = ("FBillHead");
            e.Validators.Add((AbstractValidator)(object)sOXSaveValidator);
            XOrderRecPlanValidator xOrderRecPlanValidator = new XOrderRecPlanValidator();
            ((AbstractValidator)xOrderRecPlanValidator).AlwaysValidate = (true);
            ((AbstractValidator)xOrderRecPlanValidator).EntityKey = ("FBillHead");
            e.Validators.Add((AbstractValidator)(object)xOrderRecPlanValidator);
            IMetaDataService metaDataService = BOS.Contracts.ServiceFactory.GetMetaDataService(((AbstractOperationServicePlugIn)this).Context);
            FormMetadata val = (FormMetadata)metaDataService.Load(((AbstractOperationServicePlugIn)this).Context, ((AbstractOperationServicePlugIn)this).BusinessInfo.GetForm().ParameterObjectId, true);
            IUserParameterService userParameterService = BOS.Contracts.ServiceFactory.GetUserParameterService(((AbstractOperationServicePlugIn)this).Context);
            DynamicObject val2 = userParameterService.Load(((AbstractOperationServicePlugIn)this).Context, val.BusinessInfo, ((AbstractOperationServicePlugIn)this).Context.UserId, ((AbstractElement)((AbstractOperationServicePlugIn)this).BusinessInfo.GetForm()).Id, "UserParameter");
            bool flag = false;
            if (val2 != null && ((KeyedCollectionBase<string, DynamicProperty>)(object)val2.DynamicObjectType.Properties).ContainsKey("AuxMustInput") && val2["AuxMustInput"] != null && !string.IsNullOrWhiteSpace(val2["AuxMustInput"].ToString()))
            {
                flag = Convert.ToBoolean(val2["AuxMustInput"]);
            }
            K3.MFG.App.ServiceValidator.MaterialAuxPtyItemsValueValidator val3 = new K3.MFG.App.ServiceValidator.MaterialAuxPtyItemsValueValidator();
            ((AbstractValidator)val3).AlwaysValidate = (true);
            ((AbstractValidator)val3).EntityKey = ("FSaleOrderEntry");
            val3.MaterialName = ("MaterialID");
            val3.AuxPtyName = ("AuxPropId");
            if (flag)
            {
                e.Validators.Add((AbstractValidator)(object)val3);
            }
            ReceiveBillAmountValidator receiveBillAmountValidator = new ReceiveBillAmountValidator();
            ((AbstractValidator)receiveBillAmountValidator).EntityKey = ("FBillHead");
            ((AbstractValidator)receiveBillAmountValidator).AlwaysValidate = (true);
            e.Validators.Add((AbstractValidator)(object)receiveBillAmountValidator);
            List<AbstractValidator> validators = e.Validators;
            XOrderOEMBomValidator xOrderOEMBomValidator = new XOrderOEMBomValidator();
            ((AbstractValidator)xOrderOEMBomValidator).EntityKey = ("FBillHead");
            ((AbstractValidator)xOrderOEMBomValidator).TimingPointString = (",Save,");
            validators.Add((AbstractValidator)(object)xOrderOEMBomValidator);
            RelateContractValidator relateContractValidator = new RelateContractValidator();
            ((AbstractValidator)relateContractValidator).EntityKey = ("FBillHead");
            ((AbstractValidator)relateContractValidator).AlwaysValidate = (true);
            e.Validators.Add((AbstractValidator)(object)relateContractValidator);
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            if (((OperationTransactionArgs)e).DataEntitys == null || ((OperationTransactionArgs)e).DataEntitys.Count() < 1 || (OperateOptionUtils.GetIsWFInvokeFlag(((AbstractOperationServicePlugIn)this).Option) && StringUtils.EqualsIgnoreCase(((AbstractOperationServicePlugIn)this).FormOperation.Operation, "Save")))
            {
                return;
            }
            if (!Convert.ToString(((OperationTransactionArgs)e).DataEntitys[0]["DocumentStatus"]).Equals("C"))
            {
                List<long> list = new List<long>();
                long num = 0L;
                DynamicObject[] dataEntitys = ((OperationTransactionArgs)e).DataEntitys;
                foreach (DynamicObject val in dataEntitys)
                {
                    object obj = val["SaleOrderEntry"];
                    DynamicObjectCollection val2 = (DynamicObjectCollection)((obj is DynamicObjectCollection) ? obj : null);
                    foreach (DynamicObject item in (Collection<DynamicObject>)(object)val2)
                    {
                        num = Convert.ToInt64(item["PKIdX"]);
                        if (num > 0 && !list.Contains(num))
                        {
                            list.Add(num);
                        }
                    }
                }
                ChangeOrderBillChangeFlag(list, (ChangeFlag)1);
            }
            long num2 = 0L;
            long num3 = 0L;
            bool para_IsUseTaxCombination = false;
            DynamicObject[] dataEntitys2 = ((OperationTransactionArgs)e).DataEntitys;
            foreach (DynamicObject val3 in dataEntitys2)
            {
                num2 = Convert.ToInt64(val3["SaleOrgId_Id"]);
                object paramter = BOS.App.ServiceHelper.GetService<ISystemParameterService>().GetParamter(((AbstractOperationServicePlugIn)this).Context, -1L, -1L, "EnalbeTaxMixSystemParameter", "ENABLEDTAXMIX", 0L);
                if (paramter != null)
                {
                    para_IsUseTaxCombination = Convert.ToBoolean(paramter);
                }
                Convert.ToInt64(val3["Id"]);
                num3 = Convert.ToInt64(val3["PKIDX"]);
                XSaleOrderCommon val4 = new XSaleOrderCommon(((AbstractOperationServicePlugIn)this).Context);
                FormMetadata val5 = (FormMetadata)BOS.App.ServiceHelper.GetService<IMetaDataService>().Load(((AbstractOperationServicePlugIn)this).Context, "SAL_SaleOrder", true);
                val4.orderBusinessInfo = val5.BusinessInfo;
                DynamicObject[] array = BOS.App.ServiceHelper.GetService<IViewService>().Load(((AbstractOperationServicePlugIn)this).Context, new object[1] { num3 }, val5.BusinessInfo.GetDynamicObjectType());
                DynamicObject[] array2 = BOS.App.ServiceHelper.GetService<IViewService>().Load(((AbstractOperationServicePlugIn)this).Context, new object[1] { num3 }, val5.BusinessInfo.GetDynamicObjectType());
                if (array != null && array.Count() >= 1 && array2 != null && array2.Count() >= 1)
                {
                    val4.lstNotFields=(((AbstractOperationServicePlugIn)this).Option.GetVariableValue<List<string>>("TakeEffectNotWriteBackFields", new List<string>()));
                    val4.baseOrderData=(array2[0]);
                    val4.changeOrderData=(val3);
                    val4.para_IsUseTaxCombination=(para_IsUseTaxCombination);
                    val4.SetCommonVarValue();
                    bool notCountSonAmount = GetNotCountSonAmount(num2);
                    List<string> notCountSonAmountFields = GetNotCountSonAmountFields(val5);
                    val4.SetNewDataFromOriginalData(val4.baseOrderData, val4.changeOrderData, true, val4.lstNotFields, (List<long>)null, false, notCountSonAmount, notCountSonAmountFields);
                    bool isFirstNewSave = !((DataEntityBase)val3).DataEntityState.FromDatabase || (((DataEntityBase)val3).DataEntityState.FromDatabase && _previousStatus == "Z" && Convert.ToString(val3["DocumentStatus"]) == "A");
                    DoOrderChangeVersionDataSave(num2, val4, array[0], isFirstNewSave);
                    RestoreChangeFlag(((AbstractOperationServicePlugIn)this).Context, ((OperationTransactionArgs)e).DataEntitys);
                }
            }
        }

        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            if (((AbstractOperationServicePlugIn)this).OperationResult.IsSuccess && e.DataEntitys != null && e.DataEntitys.Length > 0)
            {
                List<long> list = new List<long>();
                DynamicObject[] dataEntitys = e.DataEntitys;
                foreach (DynamicObject val in dataEntitys)
                {
                    object obj = val["SaleOrderEntry"];
                    DynamicObjectCollection val2 = (DynamicObjectCollection)((obj is DynamicObjectCollection) ? obj : null);
                    if (val2 != null)
                    {
                        IEnumerable<long> enumerable = (from n in (IEnumerable<DynamicObject>)val2
                                                        where StringUtils.EqualsIgnoreCase(Convert.ToString(n["ChangeType"]), "A") && Convert.ToInt64(n["ContractEntryId"]) > 0
                                                        select Convert.ToInt64(n["ContractEntryId"])).ToList();
                        if (enumerable.Any())
                        {
                            list.AddRange(enumerable);
                        }
                    }
                }
                if (list.Any())
                {
                    ISaleService2 saleService = ServiceFactory.GetSaleService2(((AbstractOperationServicePlugIn)this).Context);
                    saleService.BatchCommitNetCtrlByCondition(((AbstractOperationServicePlugIn)this).Context, "ContractToXOrder", list);
                }
            }
            //((AbstractOperationServicePlugIn)this).AfterExecuteOperationTransaction(e);
        }

        private void DoOrderChangeVersionDataSave(long lMainSaleOrgId, XSaleOrderCommon xSOCommon, DynamicObject beforeChangeSO, bool isFirstNewSave)
        {
            if (xSOCommon == null)
            {
                return;
            }
            ICommonService service = BOS.App.ServiceHelper.GetService<ICommonService>();
            object systemProfile = service.GetSystemProfile(((AbstractOperationServicePlugIn)this).Context, lMainSaleOrgId, "SAL_SystemParameter", "EnableVersionManage", (object)false);
            if (systemProfile == null || !Convert.ToBoolean(systemProfile))
            {
                return;
            }
            List<OrderVersion> list = new List<OrderVersion>();
            if (isFirstNewSave && Convert.ToString(beforeChangeSO["VersionNo"]).Equals("000"))
            {
                ISaleService2 saleService = ServiceFactory.GetSaleService2(((AbstractOperationServicePlugIn)this).Context);
                long num = Convert.ToInt64(beforeChangeSO["Id"]);
                if (!saleService.CheckIsHasSaveSaleOrderOriginalVersion(((AbstractOperationServicePlugIn)this).Context, num))
                {
                    OrderVersion item = xSOCommon.SetOrderVersionObjectForSave(xSOCommon.orderBusinessInfo, beforeChangeSO, "C");
                    list.Add(item);
                }
            }
            object obj = xSOCommon.changeOrderData["SaleOrderFinance"];
            DynamicObjectCollection val = (DynamicObjectCollection)((obj is DynamicObjectCollection) ? obj : null);
            string text = Convert.ToString(((Collection<DynamicObject>)(object)val)[0]["ISACTIVE"]);
            OrderVersion item2 = xSOCommon.SetOrderVersionObjectForSave(xSOCommon.orderBusinessInfo, xSOCommon.baseOrderData, text);
            list.Add(item2);
            service.SaveOrderChangeVersionData(((AbstractOperationServicePlugIn)this).Context, list, isFirstNewSave);
        }

        private void RestoreChangeFlag(Context context, DynamicObject[] entiryDatas)
        {
            List<DynamicObject> list = new List<DynamicObject>();
            for (int i = 0; i < entiryDatas.Length; i++)
            {
                object obj = entiryDatas[i]["SaleOrderEntry"];
                DynamicObjectCollection val = (DynamicObjectCollection)((obj is DynamicObjectCollection) ? obj : null);
                if (val == null || val.DeleteRows.Count <= 0)
                {
                    return;
                }
                list.AddRange(val.DeleteRows);
            }
            if (list.Count > 0 && list[0].DynamicObjectType.Properties.Contains("PKIDX"))
            {
                List<long> list2 = list.Select((DynamicObject n) => Convert.ToInt64(n["PKIDX"])).ToList();
                string text = string.Format("UPDATE T_SAL_ORDERENTRY SET FCHANGEFLAG='N' WHERE FENTRYID IN({0})", string.Join(",", list2.ToArray()));
                DBUtils.Execute(context, text);
            }
        }

        private List<string> GetNotCountSonAmountFields(FormMetadata formMetadata)
        {
            List<string> variableValue = ((AbstractOperationServicePlugIn)this).Option.GetVariableValue<List<string>>("NotCountSonFields", new List<string>());
            if (variableValue.Count > 0)
            {
                return variableValue;
            }
            List<FormOperation> formOperations = formMetadata.BusinessInfo.GetForm().FormOperations;
            FormOperation val = formOperations.FirstOrDefault((FormOperation o) => StringUtils.EqualsIgnoreCase(o.Operation, "BillNotSumSonFields"));
            List<string> list = ((val != null) ? val.ReLoadKeys : new List<string>());
            foreach (string item in list)
            {
                Field field = formMetadata.BusinessInfo.GetField(item);
                if (field != null)
                {
                    variableValue.Add(item);
                }
            }
            return variableValue;
        }

        private bool GetNotCountSonAmount(long orgId)
        {
            if (orgId <= 0)
            {
                return false;
            }
            ICommonService commonService = ServiceFactory.GetCommonService(((AbstractOperationServicePlugIn)this).Context);
            return Convert.ToBoolean(commonService.GetSystemProfile(((AbstractOperationServicePlugIn)this).Context, orgId, "SAL_SystemParameter", "NotCountSonAmount", (object)false));
        }
    }
}
