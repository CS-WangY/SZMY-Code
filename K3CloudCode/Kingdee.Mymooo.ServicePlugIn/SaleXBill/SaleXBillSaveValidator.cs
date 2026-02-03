using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.App.Core.Convertible.UnitConvert;
using Kingdee.BOS.Collections.Generic;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Core;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.BOS;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.MFG.ENG.BomExpand;
using Kingdee.K3.Core.SCM.Args;
using Kingdee.K3.Core.SCM.SAL;
using Kingdee.K3.SCM.App.Sal.ServicePlugIn.SaleOrder;
using Kingdee.K3.SCM.Contracts;
using Kingdee.BOS.App.Data;
using Kingdee.K3.BD.Contracts.SCM;
using Kingdee.K3.SCM.App.Core.Util;

namespace Kingdee.Mymooo.ServicePlugIn.SaleXBill
{
    public class SaleXBillSaveValidator : AbstractValidator
    {
        private bool CustMaterRangeCtrl;

        private string CustMaterRangeItem = "0";

        private bool SelMaterRangeCtrl;

        private string SelMaterRangeItem = "0";

        private bool SelCustRangeCtrl;

        private string SelCustRangeItem = "0";

        private bool SOChangeNotValidatePOMOQty;

        private bool ChangeOutStockSubReturnQty;

        public bool IsOrderChange { get; set; }

        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            if (ObjectUtils.IsNullOrEmpty((object)dataEntities) || dataEntities.Length == 0)
            {
                return;
            }
            for (int i = 0; i < dataEntities.Length; i++)
            {
                ExtendedDataEntity item = dataEntities[i];
                long num = Convert.ToInt64(((ExtendedDataEntity)(item))["Id"]);
                long num2 = Convert.ToInt64(((ExtendedDataEntity)(item))["SaleOrgId_Id"]);
                string text = Convert.ToString(((ExtendedDataEntity)(item))["VersionNo"]);
                Convert.ToString(((ExtendedDataEntity)(item))["BillTypeId"]);
                long num3 = Convert.ToInt64(((ExtendedDataEntity)(item))["CreatorId_Id"]);
                ICommonService localService = ServiceFactory.GetLocalService<ICommonService>();
                object systemProfile = localService.GetSystemProfile(ctx, num2, "SAL_SystemParameter", "CustMaterRangeCtrl", (object)"False");
                if (systemProfile != null)
                {
                    CustMaterRangeCtrl = Convert.ToBoolean(systemProfile);
                }
                object systemProfile2 = localService.GetSystemProfile(ctx, num2, "SAL_SystemParameter", "CustMaterRangeItem", (object)"False");
                if (Convert.ToString(systemProfile2) != "")
                {
                    CustMaterRangeItem = Convert.ToString(systemProfile2);
                }
                object systemProfile3 = localService.GetSystemProfile(ctx, num2, "SAL_SystemParameter", "SelMaterRangeCtrl", (object)"False");
                if (systemProfile3 != null)
                {
                    SelMaterRangeCtrl = Convert.ToBoolean(systemProfile3);
                }
                object systemProfile4 = localService.GetSystemProfile(ctx, num2, "SAL_SystemParameter", "SelMaterRangeItem", (object)"False");
                if (Convert.ToString(systemProfile4) != "")
                {
                    SelMaterRangeItem = Convert.ToString(systemProfile4);
                }
                object systemProfile5 = localService.GetSystemProfile(ctx, num2, "SAL_SystemParameter", "SelCustRangeCtrl", (object)"False");
                if (systemProfile5 != null)
                {
                    SelCustRangeCtrl = Convert.ToBoolean(systemProfile5);
                }
                object systemProfile6 = localService.GetSystemProfile(ctx, num2, "SAL_SystemParameter", "SelCustRangeItem", (object)"False");
                if (Convert.ToString(systemProfile6) != "")
                {
                    SelCustRangeItem = Convert.ToString(systemProfile6);
                }
                SalesControlValidate(item, ref validateContext, ctx);
                string text2 = localService.GetSystemProfile(ctx, num2, "SAL_SystemParameter", "SellerEqualBillCreator", (object)"false").ToString();
                if (StringUtils.EqualsIgnoreCase(text2, "true"))
                {
                    ISaleService localService2 = ServiceFactory.GetLocalService<ISaleService>();
                    long sellerIdFromUserId = localService2.GetSellerIdFromUserId(ctx, num2, num3);
                    long num4 = Convert.ToInt64(((ExtendedDataEntity)(item))["SalerId_Id"]);
                    if (num4 == 0 || num4 != sellerIdFromUserId)
                    {
                        validateContext.AddError((object)null, new ValidationErrorInfo("FSalerId", num.ToString(), item.DataEntityIndex, item.RowIndex, "SaleOrder005", ResManager.LoadKDString("强制销售员等于制单员时，销售员必录且等于制单员.", "005129030001087", (SubSystemType)5, new object[0]), "", (ErrorLevel)2));
                    }
                }
                object systemProfile7 = localService.GetSystemProfile(ctx, num2, "SAL_SystemParameter", "SOChangeNotValidatePOMOQty", (object)"False");
                if (systemProfile7 != null)
                {
                    SOChangeNotValidatePOMOQty = Convert.ToBoolean(systemProfile7);
                }
                object systemProfile8 = localService.GetSystemProfile(ctx, num2, "SAL_SystemParameter", "ChangeOutStockSubReturnQty", (object)"False");
                if (systemProfile8 != null)
                {
                    ChangeOutStockSubReturnQty = Convert.ToBoolean(systemProfile8);
                }
                DynamicObjectCollection val = (DynamicObjectCollection)((ExtendedDataEntity)(item))["SaleOrderEntry"];
                int count = ((Collection<DynamicObject>)(object)val).Count;
                DynamicObjectCollection val2 = null;
                DynamicObject val3 = null;
                decimal num5 = 0m;
                if (!text.Equals("000"))
                {
                    decimal num6 = 0m;
                    decimal num7 = 0m;
                    decimal num8 = 0m;
                    string text3 = "";
                    decimal num9 = 0m;
                    decimal num10 = 0m;
                    string text4 = "";
                    decimal num11 = 0m;
                    decimal num12 = 0m;
                    for (int j = 0; j < count; j++)
                    {
                        val3 = ((Collection<DynamicObject>)(object)val)[j];
                        num5 = Convert.ToDecimal(val3["Qty"]);
                        num6 = Convert.ToDecimal(val3["OldQty"]);
                        num7 = Convert.ToDecimal(val3["CanOutQty"]);
                        num8 = Convert.ToDecimal(val3["CanReturnQty"]);
                        text3 = Convert.ToString(val3["ReturnType"]);
                        Convert.ToDecimal(val3["TransJoinQty"]);
                        num9 = Convert.ToDecimal(val3["PURJOINQTY"]);
                        num11 = Convert.ToDecimal(val3["ReturnQty"]);
                        num12 = Convert.ToDecimal(val3["TRANSRETURNQTY"]);
                        if ((!StringUtils.EqualsIgnoreCase(text3, "RETURN") && num7 < 0m && num5 < num6) || (StringUtils.EqualsIgnoreCase(text3, "RETURN") && num8 < 0m))
                        {
                            if (ChangeOutStockSubReturnQty && !StringUtils.EqualsIgnoreCase(text3, "RETURN"))
                            {
                                if (num5 < num5 - num7 - (num11 + num12))
                                {
                                    num10 = num5 - num7 - (num11 + num12);
                                    text4 = string.Format(ResManager.LoadKDString("第{0}行分录，订单变更数量改小不能小于已关联下游单据的数量【{1}】{2}，请检查.", "005129000019947", (SubSystemType)5, new object[0]), j + 1, num10, (num11 + num12 > 0m) ? string.Format(ResManager.LoadKDString("（已排除退货数量【{0}】）", "005129030032711", (SubSystemType)5, new object[0]), num11 + num12) : string.Empty);
                                    if (num10 != 0m)
                                    {
                                        validateContext.AddError((object)null, new ValidationErrorInfo("FQty", num.ToString(), item.DataEntityIndex, item.RowIndex, "SaleOrderVersion001", text4, "", (ErrorLevel)2));
                                    }
                                }
                            }
                            else
                            {
                                num10 = (StringUtils.EqualsIgnoreCase(text3, "RETURN") ? Math.Abs(num5 + num8) : (num5 - num7));
                                text4 = string.Format(ResManager.LoadKDString("第{0}行分录，订单数量不能小于已关联下游单据的数量【{1}】，请检查.", "005129030005955", (SubSystemType)5, new object[0]), j + 1, num10);
                                if (Math.Abs(num5) < Math.Abs(num6) && num10 != 0m)
                                {
                                    validateContext.AddError((object)null, new ValidationErrorInfo("FQty", num.ToString(), item.DataEntityIndex, item.RowIndex, "SaleOrderVersion001", text4, "", (ErrorLevel)2));
                                }
                            }
                        }
                        if (!SOChangeNotValidatePOMOQty && num5 < num6 && num5 < num9)
                        {
                            num10 = num9;
                            text4 = string.Format(ResManager.LoadKDString("第{0}行分录，订单数量不能小于已关联下游采购订单(或生产订单)的数量【{1}】，请检查.", "005129000021205", (SubSystemType)5, new object[0]), j + 1, num10);
                            if (Math.Abs(num5) < Math.Abs(num6) && num10 != 0m)
                            {
                                validateContext.AddError((object)null, new ValidationErrorInfo("FQty", num.ToString(), item.DataEntityIndex, item.RowIndex, "SaleOrderVersion001", text4, "", (ErrorLevel)2));
                            }
                        }
                        num10 = 0m;
                    }
                }
                for (int k = 0; k < count; k++)
                {
                    val3 = ((Collection<DynamicObject>)(object)val)[k];
                    num5 = Convert.ToDecimal(val3["Qty"]);
                    val2 = (DynamicObjectCollection)val3["OrderEntryPlan"];
                    int count2 = ((Collection<DynamicObject>)(object)val2).Count;
                    decimal num13 = 0m;
                    for (int l = 0; l < count2; l++)
                    {
                        decimal num14 = Convert.ToDecimal(((Collection<DynamicObject>)(object)val2)[l]["PlanQty"]);
                        num13 += num14;
                    }
                    if (num5 != num13)
                    {
                        validateContext.AddError((object)null, new ValidationErrorInfo("FPlanQty", num.ToString(), item.DataEntityIndex, item.RowIndex, "SaleOrder000", string.Format(ResManager.LoadKDString("第{0}行分录，交货明细的数量与订单明细的数量不一致，请检查.", "005129030001090", (SubSystemType)5, new object[0]), k + 1), "", (ErrorLevel)2));
                    }
                }
                WriteBackReceiveBill writeBackReceiveBill = new WriteBackReceiveBill();
                writeBackReceiveBill.SalesReceivePlanValidate(item, ref validateContext, ctx);
                object obj = ((Collection<DynamicObject>)(DynamicObjectCollection)((ExtendedDataEntity)(item))["SaleOrderFinance"])[0]["RecConditionId"];
                DynamicObject val4 = (DynamicObject)((obj is DynamicObject) ? obj : null);
                if (val4 != null && Convert.ToInt16(val4["RECMETHOD"]) == 3)
                {
                    Dictionary<int, decimal> dictionary = new Dictionary<int, decimal>();
                    DynamicObjectCollection val5 = (DynamicObjectCollection)((ExtendedDataEntity)(item))["SaleOrderPlan"];
                    int num15 = 0;
                    long num16 = 0L;
                    decimal num17 = 0m;
                    bool flag = false;
                    StringBuilder stringBuilder = new StringBuilder(ResManager.LoadKDString("序号为 ", "005129000022120", (SubSystemType)5, new object[0]));
                    foreach (DynamicObject item2 in (Collection<DynamicObject>)(object)val5)
                    {
                        num15 = ((item2["MaterialSeq"] != null) ? Convert.ToInt32(item2["MaterialSeq"]) : 0);
                        num16 = ((item2["PlanMaterialId_Id"] == null) ? 0 : Convert.ToInt64(item2["PlanMaterialId_Id"]));
                        if (num15 != 0 && num16 != 0)
                        {
                            num17 = Convert.ToDecimal(item2["RecAdvanceRate"]);
                            if (dictionary.Keys.Contains(num15))
                            {
                                dictionary[num15] += num17;
                            }
                            else
                            {
                                dictionary.Add(num15, num17);
                            }
                        }
                    }
                    dictionary = dictionary.OrderBy((KeyValuePair<int, decimal> p) => p.Key).ToDictionary((KeyValuePair<int, decimal> p) => p.Key, (KeyValuePair<int, decimal> v) => v.Value);
                    foreach (int key in dictionary.Keys)
                    {
                        if (dictionary[key] > 100m)
                        {
                            stringBuilder.Append(key + ",");
                            flag = true;
                        }
                    }
                    stringBuilder.Remove(stringBuilder.Length - 1, 1);
                    stringBuilder.Append(ResManager.LoadKDString(" 的物料行在收款计划中应收比例超过100%，请检查！", "005129000022121", (SubSystemType)5, new object[0]));
                    if (flag)
                    {
                        validateContext.AddError((object)null, new ValidationErrorInfo("FRecAdvanceRate", num.ToString(), item.DataEntityIndex, item.RowIndex, "SaleOrderPlan", stringBuilder.ToString(), "", (ErrorLevel)2));
                    }
                }
                CheckSalOrderBySuite checkSalOrderBySuite = new CheckSalOrderBySuite();
                checkSalOrderBySuite.CheckOutStockBySuite(dataEntities, validateContext, ctx);
            }
        }

        private Dictionary<string, List<SaleSuiteData>> GetSuiteSourceData(DynamicObject dyOriginalData, out bool isExistsSonWithoutParent)
        {
            isExistsSonWithoutParent = false;
            if (dyOriginalData == null)
            {
                return null;
            }
            Dictionary<string, List<SaleSuiteData>> dictionary = new Dictionary<string, List<SaleSuiteData>>();
            DynamicObjectCollection val = null;
            if (((KeyedCollectionBase<string, DynamicProperty>)(object)dyOriginalData.DynamicObjectType.Properties).ContainsKey("SaleOrderEntry"))
            {
                object obj = dyOriginalData["SaleOrderEntry"];
                val = (DynamicObjectCollection)((obj is DynamicObjectCollection) ? obj : null);
            }
            if (val == null || ((IEnumerable<DynamicObject>)val).Count() == 0)
            {
                return null;
            }
            List<DynamicObject> list = ((IEnumerable<DynamicObject>)val).Where((DynamicObject p) => Convert.ToString(p["RowType"]) == "Parent").ToList();
            if (list == null || list.Count == 0)
            {
                return null;
            }
            List<string> parentRowIds = new List<string>();
            foreach (DynamicObject item in list)
            {
                List<SaleSuiteData> list2 = new List<SaleSuiteData>();
                DynamicObject val2 = null;
                DynamicObject val3 = null;
                long num = 0L;
                long num2 = 0L;
                string rowId = "";
                decimal num3 = 0m;
                if (!((KeyedCollectionBase<string, DynamicProperty>)(object)item.DynamicObjectType.Properties).ContainsKey("MaterialId") || !((KeyedCollectionBase<string, DynamicProperty>)(object)item.DynamicObjectType.Properties).ContainsKey("RowId") || !((KeyedCollectionBase<string, DynamicProperty>)(object)item.DynamicObjectType.Properties).ContainsKey("BomId"))
                {
                    continue;
                }
                object obj2 = item["BomId"];
                val3 = (DynamicObject)((obj2 is DynamicObject) ? obj2 : null);
                num2 = ((val3 == null) ? 0 : Convert.ToInt64(val3["Id"]));
                object obj3 = item["MaterialId"];
                val2 = (DynamicObject)((obj3 is DynamicObject) ? obj3 : null);
                num = ((val2 == null) ? 0 : Convert.ToInt64(val2["msterID"]));
                if (num == 0)
                {
                    continue;
                }
                rowId = Convert.ToString(item["RowId"]);
                parentRowIds.Add(rowId);
                string key = num + "|" + num2 + "|" + rowId;
                List<DynamicObject> list3 = ((IEnumerable<DynamicObject>)val).Where((DynamicObject p) => Convert.ToString(p["ParentRowId"]) == rowId).ToList();
                foreach (DynamicObject item2 in list3)
                {
                    SaleSuiteData val4 = new SaleSuiteData();
                    object obj4 = item2["MaterialId"];
                    val2 = (DynamicObject)((obj4 is DynamicObject) ? obj4 : null);
                    num = ((val2 == null) ? 0 : Convert.ToInt64(val2["msterID"]));
                    val4.MasterId = (num);
                    if (num != 0)
                    {
                        num3 = Convert.ToDecimal(item2["Qty"]);
                        val4.SaleQty = (num3);
                        list2.Add(val4);
                    }
                }
                if (!dictionary.ContainsKey(key))
                {
                    dictionary.Add(key, list2);
                }
            }
            int num4 = ((IEnumerable<DynamicObject>)val).Where((DynamicObject p) => Convert.ToString(p["RowType"]) == "Son" && !parentRowIds.Contains(Convert.ToString(p["ParentRowId"]))).Count();
            isExistsSonWithoutParent = num4 > 0;
            return dictionary;
        }

        private Dictionary<string, List<SaleSuiteData>> GetSuiteExpandResultData(DynamicObjectCollection dycExpandResult, K3.BD.Contracts.IUnitConvertService unitConverService)
        {
            Dictionary<string, List<SaleSuiteData>> dictionary = new Dictionary<string, List<SaleSuiteData>>();
            List<DynamicObject> list = ((IEnumerable<DynamicObject>)dycExpandResult).Where((DynamicObject p) => Convert.ToInt32(p["BomLevel"]) == 0).ToList();
            if (list == null || list.Count == 0)
            {
                return null;
            }
            foreach (DynamicObject item in list)
            {
                List<SaleSuiteData> list2 = new List<SaleSuiteData>();
                DynamicObject val = null;
                DynamicObject val2 = null;
                long num = 0L;
                long num2 = 0L;
                string rowId = "";
                decimal num3 = 0m;
                if (!((KeyedCollectionBase<string, DynamicProperty>)(object)item.DynamicObjectType.Properties).ContainsKey("BomId"))
                {
                    continue;
                }
                object obj = item["BomId"];
                val2 = (DynamicObject)((obj is DynamicObject) ? obj : null);
                num2 = ((val2 == null) ? 0 : Convert.ToInt64(val2["Id"]));
                object obj2 = item["MaterialId"];
                val = (DynamicObject)((obj2 is DynamicObject) ? obj2 : null);
                num = ((val == null) ? 0 : Convert.ToInt64(val["msterID"]));
                if (num == 0 || num2 == 0)
                {
                    continue;
                }
                rowId = Convert.ToString(item["SrcBillNo"]);
                string key = num + "|" + num2 + "|" + rowId;
                List<DynamicObject> list3 = ((IEnumerable<DynamicObject>)dycExpandResult).Where((DynamicObject p) => Convert.ToString(p["SrcBillNo"]) == rowId && Convert.ToInt32(p["BomLevel"]) > 0).ToList();
                foreach (DynamicObject item2 in list3)
                {
                    SaleSuiteData val3 = new SaleSuiteData();
                    object obj3 = item2["MaterialId"];
                    val = (DynamicObject)((obj3 is DynamicObject) ? obj3 : null);
                    num = ((val == null) ? 0 : Convert.ToInt64(val["msterID"]));
                    val3.MasterId = (num);
                    if (num != 0)
                    {
                        object obj4 = val["MaterialSale"];
                        DynamicObjectCollection val4 = (DynamicObjectCollection)((obj4 is DynamicObjectCollection) ? obj4 : null);
                        long destUnitId = ((val4 == null) ? 0 : Convert.ToInt64(((Collection<DynamicObject>)(object)val4)[0]["SaleUnitId_Id"]));
                        object obj5 = item2["BaseUnitId"];
                        DynamicObject val5 = (DynamicObject)((obj5 is DynamicObject) ? obj5 : null);
                        long sourceUnitId = ((val5 == null) ? 0 : Convert.ToInt64(val5["Id"]));
                        decimal num4 = Convert.ToDecimal(item2["BaseQty"]);
                        Context context = ((AbstractValidator)this).Context;
                        GetUnitConvertRateArgs val6 = new GetUnitConvertRateArgs();
                        val6.MasterId = (num);
                        val6.SourceUnitId = (sourceUnitId);
                        val6.DestUnitId = (destUnitId);
                        UnitConvert unitConvertRate = unitConverService.GetUnitConvertRate(context, val6);
                        num3 = unitConvertRate.ConvertQty(num4, "");
                        val3.SaleQty = (num3);
                        list2.Add(val3);
                    }
                }
                if (!dictionary.ContainsKey(key))
                {
                    dictionary.Add(key, list2);
                }
            }
            return dictionary;
        }

        private List<DynamicObject> GetBomSourceData(DynamicObject dyOriginalData)
        {
            if (dyOriginalData == null)
            {
                return null;
            }
            DynamicObjectCollection val = null;
            if (((KeyedCollectionBase<string, DynamicProperty>)(object)dyOriginalData.DynamicObjectType.Properties).ContainsKey("SaleOrderEntry"))
            {
                object obj = dyOriginalData["SaleOrderEntry"];
                val = (DynamicObjectCollection)((obj is DynamicObjectCollection) ? obj : null);
            }
            if (val == null || ((IEnumerable<DynamicObject>)val).Count() == 0)
            {
                return null;
            }
            DynamicObject val2 = null;
            long demandOrgId_Id = 0L;
            if (((KeyedCollectionBase<string, DynamicProperty>)(object)dyOriginalData.DynamicObjectType.Properties).ContainsKey("SaleOrgId"))
            {
                object obj2 = dyOriginalData["SaleOrgId"];
                val2 = (DynamicObject)((obj2 is DynamicObject) ? obj2 : null);
                demandOrgId_Id = ((val2 == null) ? 0 : Convert.ToInt64(val2["Id"]));
            }
            DynamicObject val3 = null;
            long supplyOrgId_Id = 0L;
            if (((KeyedCollectionBase<string, DynamicProperty>)(object)dyOriginalData.DynamicObjectType.Properties).ContainsKey("SupplyOrgId"))
            {
                object obj3 = dyOriginalData["SupplyOrgId"];
                val3 = (DynamicObject)((obj3 is DynamicObject) ? obj3 : null);
                supplyOrgId_Id = ((val3 == null) ? 0 : Convert.ToInt64(val3["Id"]));
            }
            List<DynamicObject> list = new List<DynamicObject>();
            DynamicObject val4 = null;
            DynamicObject val5 = null;
            DynamicObject val6 = null;
            string text = "";
            decimal num = 0m;
            long num2 = 0L;
            long bomId_Id = 0L;
            long num3 = 0L;
            string text2 = "";
            foreach (DynamicObject item in (Collection<DynamicObject>)(object)val)
            {
                if (!((KeyedCollectionBase<string, DynamicProperty>)(object)item.DynamicObjectType.Properties).ContainsKey("MaterialId") || !((KeyedCollectionBase<string, DynamicProperty>)(object)item.DynamicObjectType.Properties).ContainsKey("StockBaseQty") || !((KeyedCollectionBase<string, DynamicProperty>)(object)item.DynamicObjectType.Properties).ContainsKey("BaseUnitId") || !((KeyedCollectionBase<string, DynamicProperty>)(object)item.DynamicObjectType.Properties).ContainsKey("RowId"))
                {
                    continue;
                }
                if (((KeyedCollectionBase<string, DynamicProperty>)(object)item.DynamicObjectType.Properties).ContainsKey("RowType"))
                {
                    text = Convert.ToString(item["RowType"]);
                }
                if (StringUtils.EqualsIgnoreCase(text, "Parent"))
                {
                    text2 = Convert.ToString(item["RowId"]);
                    if (((KeyedCollectionBase<string, DynamicProperty>)(object)item.DynamicObjectType.Properties).ContainsKey("SupplyOrgId"))
                    {
                        object obj4 = item["SupplyOrgId"];
                        val3 = (DynamicObject)((obj4 is DynamicObject) ? obj4 : null);
                        supplyOrgId_Id = ((val3 == null) ? 0 : Convert.ToInt64(val3["Id"]));
                    }
                    object obj5 = item["MaterialId"];
                    val4 = (DynamicObject)((obj5 is DynamicObject) ? obj5 : null);
                    num2 = ((val4 == null) ? 0 : Convert.ToInt64(val4["Id"]));
                    if (((KeyedCollectionBase<string, DynamicProperty>)(object)item.DynamicObjectType.Properties).ContainsKey("BomId"))
                    {
                        object obj6 = item["BomId"];
                        val6 = (DynamicObject)((obj6 is DynamicObject) ? obj6 : null);
                        bomId_Id = ((val6 == null) ? 0 : Convert.ToInt64(val6["Id"]));
                    }
                    if (num2 != 0)
                    {
                        num = Convert.ToDecimal(item["StockBaseQty"]);
                        object obj7 = item["BaseUnitId"];
                        val5 = (DynamicObject)((obj7 is DynamicObject) ? obj7 : null);
                        num3 = ((val5 == null) ? 0 : Convert.ToInt64(val5["Id"]));
                        BomForwardSourceDynamicRow val7 = new BomForwardSourceDynamicRow(BomForwardSourceDynamicRow.CreateInstance());
                        //BomForwardSourceDynamicRow val7 = BomForwardSourceDynamicRow.op_Implicit(BomForwardSourceDynamicRow.CreateInstance());
                        val7.DemandOrgId_Id = (demandOrgId_Id);
                        val7.MaterialId_Id = (num2);
                        val7.BomId_Id = (bomId_Id);
                        val7.NeedQty = (num);
                        val7.SupplyOrgId_Id = (supplyOrgId_Id);
                        val7.TimeUnit = (1.ToString());
                        val7.UnitId_Id = (num3);
                        val7.SrcBillNo = (text2);
                        list.Add(((DynamicObjectView)val7).DataEntity);
                    }
                }
            }
            return list;
        }

        private void SalesControlValidate(ExtendedDataEntity item, ref ValidateContext validateContext, Context ctx)
        {
            DynamicObject val = null;
            DynamicObject val2 = null;
            DynamicObject val3 = null;
            DynamicObject val4 = null;
            DynamicObject val5 = null;
            DynamicObjectCollection val6 = null;
            string text = "";
            text = Convert.ToString(((ExtendedDataEntity)(item))["BillNo"]);
            long num = Convert.ToInt64(((ExtendedDataEntity)(item))["Id"]);
            object obj = ((ExtendedDataEntity)(item))["CustId"];
            val = (DynamicObject)((obj is DynamicObject) ? obj : null);
            object obj2 = ((ExtendedDataEntity)(item))["SalerId"];
            val2 = (DynamicObject)((obj2 is DynamicObject) ? obj2 : null);
            object obj3 = ((ExtendedDataEntity)(item))["SaleDeptId"];
            val3 = (DynamicObject)((obj3 is DynamicObject) ? obj3 : null);
            object obj4 = ((ExtendedDataEntity)(item))["SaleGroupId"];
            val4 = (DynamicObject)((obj4 is DynamicObject) ? obj4 : null);
            object obj5 = ((ExtendedDataEntity)(item))["SaleOrgId"];
            val5 = (DynamicObject)((obj5 is DynamicObject) ? obj5 : null);
            long num2 = ((val2 == null) ? 0 : Convert.ToInt64(val2["Id"]));
            long saleDeptId = ((val3 != null) ? Convert.ToInt64(val3["Id"]) : ((val2 != null) ? Convert.ToInt64(val2["DeptId_Id"]) : 0));
            long saleDeptId2 = ((val3 == null) ? 0 : Convert.ToInt64(val3["Id"]));
            long saleGroupId = ((val4 == null) ? 0 : Convert.ToInt64(val4["Id"]));
            long num3 = ((val == null) ? 0 : Convert.ToInt64(val["Id"]));
            string custTypeId = ((val == null) ? "" : Convert.ToString(val["CustTypeId_Id"]));
            long saleOrgId = ((val5 == null) ? 0 : Convert.ToInt64(val5["Id"]));
            val6 = (DynamicObjectCollection)((ExtendedDataEntity)(item))["SaleOrderEntry"];
            DynamicObject val7 = null;
            long num4 = 0L;
            string auxPropName = "";
            List<long> list = new List<long>();
            if (IsOrderChange && num > 0)
            {
                string text2 = $"select FENTRYID from T_SAL_ORDERENTRY where FID={num}";
                DynamicObjectCollection val8 = DBUtils.ExecuteDynamicObject(((AbstractValidator)this).Context, text2, (IDataEntityType)null, (IDictionary<string, Type>)null, CommandType.Text, (SqlParam[])(object)new SqlParam[0]);
                if (val8 != null && ((Collection<DynamicObject>)(object)val8).Count > 0)
                {
                    foreach (DynamicObject item4 in (Collection<DynamicObject>)(object)val8)
                    {
                        long num5 = Convert.ToInt64(item4["FENTRYID"]);
                        if (num5 > 0 && !list.Contains(num5))
                        {
                            list.Add(num5);
                        }
                    }
                }
            }
            if (SelCustRangeCtrl)
            {
                if (num3 != 0 && num2 == 0)
                {
                    validateContext.AddError((object)null, new ValidationErrorInfo("", num.ToString(), item.DataEntityIndex, item.RowIndex, "SaleOrderSalesControlCheck003", string.Format(ResManager.LoadKDString("订单启用了销售员客户可销控制，针对订单当前客户【{0}】销售员不允许为空！", "005129030001093", (SubSystemType)5, new object[0]), Convert.ToString(val["Name"])), text, (ErrorLevel)2));
                }
                else if (num3 != 0 && !CheckSalerCust(num2, saleDeptId, saleGroupId, num3, custTypeId, SelCustRangeItem, ctx, saleOrgId))
                {
                    validateContext.AddError((object)null, new ValidationErrorInfo("", num.ToString(), item.DataEntityIndex, item.RowIndex, "SaleOrderSalesControlCheck003", string.Format(ResManager.LoadKDString("订单销售员【{0}】销售部门【{1}】销售组【{2}】针对订单当前客户【{3}】不可销！", "005129030001096", (SubSystemType)5, new object[0]), (val2 == null) ? "" : val2["Name"].ToString(), (val3 == null) ? "" : Convert.ToString(val3["Name"]), (val4 == null) ? "" : Convert.ToString(val4["Name"]), Convert.ToString(val["Name"])), text, (ErrorLevel)2));
                }
            }
            if (val6 == null || ((Collection<DynamicObject>)(object)val6).Count <= 0)
            {
                return;
            }
            if (CustMaterRangeCtrl)
            {
                List<long> list2 = CheckCustomerMaterialNew(val, val6, CustMaterRangeItem, ctx, saleOrgId);
                if (list2 != null && list2.Count > 0)
                {
                    for (int i = 0; i < ((Collection<DynamicObject>)(object)val6).Count; i++)
                    {
                        object obj6 = ((Collection<DynamicObject>)(object)val6)[i]["MaterialId"];
                        val7 = (DynamicObject)((obj6 is DynamicObject) ? obj6 : null);
                        if (val7 == null)
                        {
                            continue;
                        }
                        num4 = Convert.ToInt64(((Collection<DynamicObject>)(object)val6)[i]["Id"]);
                        if ((!((Collection<DynamicObject>)(object)val6)[i].DynamicObjectType.Properties.Contains("ChangeType") || !(Convert.ToString(((Collection<DynamicObject>)(object)val6)[i]["ChangeType"]) != "A")) && (!IsOrderChange || num4 <= 0 || !list.Contains(num4)))
                        {
                            long item2 = Convert.ToInt64(val7["Id"]);
                            bool flag = list2.Contains(item2);
                            bool flag2 = false;
                            if ((((CustMaterRangeItem == "0" || CustMaterRangeItem == "2") && !flag) || (CustMaterRangeItem == "1" && flag)) ? true : false)
                            {
                                validateContext.AddError((object)null, new ValidationErrorInfo("", num.ToString(), item.DataEntityIndex, item.RowIndex, "SaleOrderSalesControlCheck001", string.Format(ResManager.LoadKDString("分录行号【{0}】物料【{1}】针对订单当前客户【{2}】不可销！", "005129030001564", (SubSystemType)5, new object[0]), i + 1, Convert.ToString(((DynamicObject)((Collection<DynamicObject>)(object)val6)[i]["MaterialId"])["Name"]), Convert.ToString(val["Name"])), text, (ErrorLevel)2));
                            }
                        }
                    }
                }
                else if (num3 != 0 && CustMaterRangeItem == "0")
                {
                    bool flag3 = false;
                    for (int j = 0; j < ((Collection<DynamicObject>)(object)val6).Count; j++)
                    {
                        num4 = Convert.ToInt64(((Collection<DynamicObject>)(object)val6)[j]["Id"]);
                        if ((!((Collection<DynamicObject>)(object)val6)[j].DynamicObjectType.Properties.Contains("ChangeType") || !(Convert.ToString(((Collection<DynamicObject>)(object)val6)[j]["ChangeType"]) != "A")) && (!IsOrderChange || num4 <= 0 || !list.Contains(num4)))
                        {
                            flag3 = true;
                            break;
                        }
                    }
                    if (flag3)
                    {
                        validateContext.AddError((object)null, new ValidationErrorInfo("", num.ToString(), item.DataEntityIndex, item.RowIndex, "SaleOrderSalesControlCheck001", string.Format(ResManager.LoadKDString("开启了客户-物料可销控制（允销），订单所有物料与客户【{0}】未设置任何可销控制（允销）关系！", "005129000022507", (SubSystemType)5, new object[0]), Convert.ToString(val["Name"])), text, (ErrorLevel)2));
                    }
                }
            }
            if (!SelMaterRangeCtrl)
            {
                return;
            }
            if (num2 == 0)
            {
                validateContext.AddError((object)null, new ValidationErrorInfo("", num.ToString(), item.DataEntityIndex, item.RowIndex, "SaleOrderSalesControlCheck003", string.Format(ResManager.LoadKDString("订单启用了销售员物料可销控制，销售员不允许为空！", "005129030001105", (SubSystemType)5, new object[0])), text, (ErrorLevel)2));
                return;
            }
            List<long> list3 = CheckSalerMaterialNew(num2, saleDeptId2, saleGroupId, val6, SelMaterRangeItem, ctx, saleOrgId);
            if (list3 != null && list3.Count > 0)
            {
                for (int k = 0; k < ((Collection<DynamicObject>)(object)val6).Count; k++)
                {
                    object obj7 = ((Collection<DynamicObject>)(object)val6)[k]["MaterialId"];
                    DynamicObject val9 = (DynamicObject)((obj7 is DynamicObject) ? obj7 : null);
                    if (val9 == null)
                    {
                        continue;
                    }
                    num4 = Convert.ToInt64(((Collection<DynamicObject>)(object)val6)[k]["Id"]);
                    if ((!((Collection<DynamicObject>)(object)val6)[k].DynamicObjectType.Properties.Contains("ChangeType") || !(Convert.ToString(((Collection<DynamicObject>)(object)val6)[k]["ChangeType"]) != "A")) && (!IsOrderChange || num4 <= 0 || !list.Contains(num4)))
                    {
                        ValidationErrorInfo val10 = SetSalerMatErrorInfo(item, num.ToString(), k + 1, Convert.ToString(((DynamicObject)((Collection<DynamicObject>)(object)val6)[k]["MaterialId"])["Name"]), auxPropName, Convert.ToString(val2["Name"]), (val3 == null) ? "" : Convert.ToString(val3["Name"]), (val4 == null) ? "" : Convert.ToString(val4["Name"]), text);
                        object obj8 = ((Collection<DynamicObject>)(object)val6)[k]["MaterialId"];
                        val7 = (DynamicObject)((obj8 is DynamicObject) ? obj8 : null);
                        long item3 = Convert.ToInt64(val7["Id"]);
                        bool flag4 = list3.Contains(item3);
                        if (((SelMaterRangeItem == "0" || SelMaterRangeItem == "2") && !flag4) || (SelMaterRangeItem == "1" && flag4))
                        {
                            validateContext.AddError((object)null, val10);
                        }
                    }
                }
            }
            else
            {
                if (!(SelMaterRangeItem == "0"))
                {
                    return;
                }
                bool flag5 = false;
                for (int l = 0; l < ((Collection<DynamicObject>)(object)val6).Count; l++)
                {
                    num4 = Convert.ToInt64(((Collection<DynamicObject>)(object)val6)[l]["Id"]);
                    if ((!((Collection<DynamicObject>)(object)val6)[l].DynamicObjectType.Properties.Contains("ChangeType") || !(Convert.ToString(((Collection<DynamicObject>)(object)val6)[l]["ChangeType"]) != "A")) && (!IsOrderChange || num4 <= 0 || !list.Contains(num4)))
                    {
                        flag5 = true;
                        break;
                    }
                }
                if (flag5)
                {
                    validateContext.AddError((object)null, new ValidationErrorInfo("", num.ToString(), item.DataEntityIndex, item.RowIndex, "SaleOrderSalesControlCheck002", string.Format(ResManager.LoadKDString("开启了销售员-物料可销控制，但订单所有物料与当前销售员【{0}】未设置任何可销控制关系！", "005129030001108", (SubSystemType)5, new object[0]), (val2 == null) ? "" : Convert.ToString(val2["Name"])), text, (ErrorLevel)2));
                }
            }
        }

        private ValidationErrorInfo SetSalerMatErrorInfo(ExtendedDataEntity item, string billId, int seq, string matName, string auxPropName, string salerName, string salerDeptName, string salerGroupName, string billNo)
        {
            return new ValidationErrorInfo("", billId.ToString(), item.DataEntityIndex, item.RowIndex, "SaleOrderSalesControlCheck002", string.Format(ResManager.LoadKDString("分录行号【{0}】物料【{1}】针对订单当前销售员【{2}】销售部门【{3}】销售组【{4}】不可销！", "005129030001567", (SubSystemType)5, new object[0]), seq, matName, salerName, salerDeptName, salerGroupName), billNo, (ErrorLevel)2);
        }

        private bool CheckSalerDeptGroupEffective(IList<DynamicObject> SCInfo, long salerId, long saleDeptId, long saleGroupId, string controlItem)
        {
            if (controlItem == "2")
            {
                controlItem = "0";
            }
            bool result = false;
            if (SCInfo.Count((DynamicObject p) => Convert.ToInt64(p["FSalerId"]) == salerId && Convert.ToInt64(p["FSalerDeptId"]) == 0 && Convert.ToString(p["FControlItem"]) == controlItem && Convert.ToInt64(p["FSalerGroupId"]) == 0) > 0)
            {
                result = true;
            }
            else if (SCInfo.Count((DynamicObject p) => Convert.ToInt64(p["FSalerId"]) == salerId && Convert.ToInt64(p["FSalerDeptId"]) == saleDeptId && Convert.ToString(p["FControlItem"]) == controlItem && Convert.ToInt64(p["FSalerGroupId"]) == saleGroupId) > 0)
            {
                result = true;
            }
            else if (SCInfo.Count((DynamicObject p) => Convert.ToInt64(p["FSalerId"]) == salerId && Convert.ToInt64(p["FSalerDeptId"]) == saleDeptId && Convert.ToString(p["FControlItem"]) == controlItem && Convert.ToInt64(p["FSalerGroupId"]) == 0) > 0)
            {
                result = true;
            }
            else if (SCInfo.Count((DynamicObject p) => Convert.ToInt64(p["FSalerId"]) == salerId && Convert.ToInt64(p["FSalerDeptId"]) == 0 && Convert.ToString(p["FControlItem"]) == controlItem && Convert.ToInt64(p["FSalerGroupId"]) == saleGroupId) > 0)
            {
                result = true;
            }
            else if (SCInfo.Count((DynamicObject p) => Convert.ToInt64(p["FSalerId"]) == 0 && Convert.ToInt64(p["FSalerDeptId"]) == 0 && Convert.ToString(p["FControlItem"]) == controlItem && Convert.ToInt64(p["FSalerGroupId"]) == saleGroupId) > 0)
            {
                result = true;
            }
            else if (SCInfo.Count((DynamicObject p) => Convert.ToInt64(p["FSalerId"]) == 0 && Convert.ToInt64(p["FSalerDeptId"]) == saleDeptId && Convert.ToString(p["FControlItem"]) == controlItem && Convert.ToInt64(p["FSalerGroupId"]) == 0) > 0)
            {
                result = true;
            }
            return result;
        }

        private List<long> CheckCustomerMaterialNew(DynamicObject dyCust, DynamicObjectCollection dycEntrys, string sCustMaterRangeItem, Context ctx, long saleOrgId)
        {
            long num = ((dyCust == null) ? 0 : Convert.ToInt64(dyCust["Id"]));
            string custType = ((dyCust == null) ? "" : Convert.ToString(dyCust["custTypeId_Id"]));
            List<long> result = new List<long>();
            if (num > 0 && dycEntrys != null && ((Collection<DynamicObject>)(object)dycEntrys).Count > 0)
            {
                foreach (DynamicObject item in (Collection<DynamicObject>)(object)dycEntrys)
                {
                    object obj = item["MaterialId"];
                    DynamicObject val = (DynamicObject)((obj is DynamicObject) ? obj : null);
                }
                SalCtrlArgs val2 = new SalCtrlArgs();
                val2.para_CustMaterRangeItem = sCustMaterRangeItem;
                val2.para_CustMaterRangeCtrl = (CustMaterRangeCtrl);
                val2.custId = (num);
                val2.custType = (custType);
                val2.saleOrgId = (saleOrgId);
                ISCMServiceForBD service = ServiceFactory.GetService<ISCMServiceForBD>(ctx);
                result = service.GetAllMaterialCustCtrl(ctx, val2);
            }
            return result;
        }

        private DynamicObjectCollection CheckCustomerMaterial(DynamicObject dyCust, DynamicObjectCollection dycEntrys, string sCustMaterRangeItem, Context ctx, long saleOrgId)
        {
            if (sCustMaterRangeItem == "2")
            {
                sCustMaterRangeItem = "0";
            }
            long num = ((dyCust == null) ? 0 : Convert.ToInt64(dyCust["Id"]));
            string text = ((dyCust == null) ? "" : Convert.ToString(dyCust["custTypeId_Id"]));
            DynamicObjectCollection result = null;
            List<long> list = new List<long>();
            List<long> list2 = new List<long>();
            string text2 = "";
            if (num > 0 && dycEntrys != null && ((Collection<DynamicObject>)(object)dycEntrys).Count > 0)
            {
                long num2 = 0L;
                long num3 = 0L;
                foreach (DynamicObject item in (Collection<DynamicObject>)(object)dycEntrys)
                {
                    object obj = item["MaterialId"];
                    DynamicObject val = (DynamicObject)((obj is DynamicObject) ? obj : null);
                    if (val != null)
                    {
                        num2 = Convert.ToInt64(((DynamicObject)item["MaterialId"])["MaterialGroup_Id"]);
                        num3 = Convert.ToInt64(item["MaterialId_Id"]);
                        if (!list2.Contains(num2))
                        {
                            list2.Add(num2);
                        }
                        if (!list.Contains(num3))
                        {
                            list.Add(num3);
                        }
                    }
                }
                if (list.Count > 0 && list2.Count > 0)
                {
                    text2 = string.Format("SELECT FMATERIALID,FAUXPROPID,FMATCATEGORYID \r\nFROM T_SAL_SCCUSTMAT \r\nWHERE FSALEORGID = @SaleOrgId AND \r\n(\r\n    (FCUSTOMERID = @CustId AND FCUSTTYPEID = @CustTypeId) OR \r\n    (FCUSTOMERID = 0 AND FCUSTTYPEID = @CustTypeId2) OR \r\n    (FCUSTOMERID = @CustId2 AND FCUSTTYPEID = '0')\r\n) \r\nAND FControlItem = @ControlItem  AND EXISTS\r\n(\r\n    SELECT {0} 1 \r\n    FROM TABLE(fn_StrSplit(@FMatID, ',', 1)) B \r\n    WHERE B.FID = FMaterialId \r\n    UNION ALL\r\n    SELECT {1} 1 \r\n    FROM TABLE(fn_StrSplit(@FMatCategoryId, ',', 1)) C \r\n    WHERE C.FID = FMatcategoryId\r\n)", SCMCommonUtil.GetCardinalityString(ctx, "B", (long)list.Count), SCMCommonUtil.GetCardinalityString(ctx, "C", (long)list2.Count));
                    List<SqlParam> list3 = new List<SqlParam>();
                    list3.Add(new SqlParam("@SaleOrgId", (KDDbType)12, (object)saleOrgId));
                    list3.Add(new SqlParam("@CustId", (KDDbType)12, (object)num));
                    list3.Add(new SqlParam("@CustId2", (KDDbType)12, (object)num));
                    list3.Add(new SqlParam("@CustTypeId", (KDDbType)16, (object)text));
                    list3.Add(new SqlParam("@CustTypeId2", (KDDbType)16, (object)text));
                    list3.Add(new SqlParam("@ControlItem", (KDDbType)16, (object)sCustMaterRangeItem));
                    list3.Add(new SqlParam("@FMatID", (KDDbType)161, (object)list.ToArray()));
                    list3.Add(new SqlParam("@FMatCategoryId", (KDDbType)161, (object)list2.ToArray()));
                    result = DBUtils.ExecuteDynamicObject(ctx, text2, (IDataEntityType)null, (IDictionary<string, Type>)null, CommandType.Text, list3.ToArray());
                }
            }
            return result;
        }

        private List<long> CheckSalerMaterialNew(long salerId, long saleDeptId, long saleGroupId, DynamicObjectCollection dycEntrys, string sSelMaterRangeItem, Context ctx, long saleOrgId)
        {
            List<long> result = new List<long>();
            if (salerId > 0 && dycEntrys != null && ((Collection<DynamicObject>)(object)dycEntrys).Count > 0)
            {
                foreach (DynamicObject item in (Collection<DynamicObject>)(object)dycEntrys)
                {
                    object obj = item["MaterialId"];
                    DynamicObject val = (DynamicObject)((obj is DynamicObject) ? obj : null);
                }
                SalCtrlArgs val2 = new SalCtrlArgs();
                val2.para_SelMaterRangeItem = sSelMaterRangeItem;
                val2.para_SelMaterRangeCtrl = (SelMaterRangeCtrl);
                val2.saleGroupId = (saleGroupId);
                val2.saleDeptId = (saleDeptId);
                val2.salerId = (salerId);
                val2.saleOrgId = (saleOrgId);
                ISCMServiceForBD service = ServiceFactory.GetService<ISCMServiceForBD>(ctx);
                result = service.GetAllMaterialSaleCtrl(ctx, val2);
            }
            return result;
        }

        private DynamicObjectCollection CheckSalerMaterial(long salerId, long saleDeptId, long saleGroupId, DynamicObjectCollection dycEntrys, string sSelMaterRangeItem, Context ctx, long saleOrgId)
        {

            if (sSelMaterRangeItem == "2")
            {
                sSelMaterRangeItem = "0";
            }
            DynamicObjectCollection result = null;
            List<long> list = new List<long>();
            List<long> list2 = new List<long>();
            string text = "";
            if (salerId > 0 && dycEntrys != null && ((Collection<DynamicObject>)(object)dycEntrys).Count > 0)
            {
                long num = 0L;
                long num2 = 0L;
                foreach (DynamicObject item in (Collection<DynamicObject>)(object)dycEntrys)
                {
                    object obj = item["MaterialId"];
                    DynamicObject val = (DynamicObject)((obj is DynamicObject) ? obj : null);
                    if (val != null)
                    {
                        num = Convert.ToInt64(((DynamicObject)item["MaterialId"])["MaterialGroup_Id"]);
                        num2 = Convert.ToInt64(item["MaterialId_Id"]);
                        if (!list2.Contains(num))
                        {
                            list2.Add(num);
                        }
                        if (!list.Contains(num2))
                        {
                            list.Add(num2);
                        }
                    }
                }
                if (list.Count > 0 && list2.Count > 0)
                {
                    text = string.Format("\r\nSELECT FMATERIALID, FAUXPROPID, FSALERID, FSALERDEPTID, FSALERGROUPID, FMATCATEGORYID, FCONTROLITEM \r\nFROM T_SAL_SCSALERMAT \r\nWHERE FSALEORGID = @SaleOrgId AND \r\n(\r\n    (FSALERID = @SalerId AND FSAlERDEPTID = @DeptId AND FSAlERGROUPID = @GroupId) OR \r\n    (FSALERID = @SalerId2 AND FSAlERDEPTID = @DeptId3 AND FSAlERGROUPID = 0) OR \r\n    (FSALERID = @SalerId3 AND FSAlERDEPTID = 0 AND FSAlERGROUPID = @GroupId3) OR \r\n    (FSALERID = 0 AND FSAlERDEPTID = @DeptId2 AND FSALERGROUPID = 0) OR \r\n    (FSALERID = 0 AND FSALERDEPTID = 0 AND FSAlERGROUPID = @GroupId2) OR \r\n    (FSALERID = @SalerId4 AND FSALERDEPTID = 0 AND FSAlERGROUPID = 0)\r\n) \r\nAND FControlItem = @ControlItem AND EXISTS\r\n(\r\n        SELECT {0} 1 \r\n        FROM TABLE(fn_StrSplit(@FMatID,',',1)) B \r\n        WHERE B.FID = FMaterialId \r\n        UNION ALL\r\n        SELECT {1} 1 \r\n        FROM TABLE(fn_StrSplit(@FMatCategoryId,',',1)) C \r\n        WHERE C.FID = FMatcategoryId\r\n)", SCMCommonUtil.GetCardinalityString(ctx, "B", (long)list.Count), SCMCommonUtil.GetCardinalityString(ctx, "C", (long)list2.Count));
                    List<SqlParam> list3 = new List<SqlParam>();
                    list3.Add(new SqlParam("@SaleOrgId", (KDDbType)12, (object)saleOrgId));
                    list3.Add(new SqlParam("@SalerId", (KDDbType)12, (object)salerId));
                    list3.Add(new SqlParam("@SalerId2", (KDDbType)12, (object)salerId));
                    list3.Add(new SqlParam("@SalerId3", (KDDbType)12, (object)salerId));
                    list3.Add(new SqlParam("@SalerId4", (KDDbType)12, (object)salerId));
                    list3.Add(new SqlParam("@DeptId", (KDDbType)12, (object)saleDeptId));
                    list3.Add(new SqlParam("@GroupId", (KDDbType)12, (object)saleGroupId));
                    list3.Add(new SqlParam("@DeptId2", (KDDbType)12, (object)saleDeptId));
                    list3.Add(new SqlParam("@GroupId2", (KDDbType)12, (object)saleGroupId));
                    list3.Add(new SqlParam("@DeptId3", (KDDbType)12, (object)saleDeptId));
                    list3.Add(new SqlParam("@GroupId3", (KDDbType)12, (object)saleGroupId));
                    list3.Add(new SqlParam("@ControlItem", (KDDbType)16, (object)sSelMaterRangeItem));
                    list3.Add(new SqlParam("@FMatID", (KDDbType)161, (object)list.ToArray()));
                    list3.Add(new SqlParam("@FMatCategoryId", (KDDbType)161, (object)list2.ToArray()));
                    result = DBUtils.ExecuteDynamicObject(ctx, text, (IDataEntityType)null, (IDictionary<string, Type>)null, CommandType.Text, list3.ToArray());
                }
            }
            return result;
        }

        private bool CheckSalerCust(long salerId, long saleDeptId, long saleGroupId, long custId, string custTypeId, string controlItem, Context ctx, long saleOrgId)
        {
            bool result = true;
            //销售变更单校验可销非常卡，这里直接返回true
            string text = controlItem;
            if (controlItem == "2")
            {
                controlItem = "0";
            }
            if ((salerId > 0 || saleDeptId > 0 || saleGroupId > 0) && custId > 0)
            {
                SalCtrlArgs val = new SalCtrlArgs();
                val.para_SelCustRangeItem = controlItem;
                val.para_SelCustRangeCtrl = (SelCustRangeCtrl);
                val.saleGroupId = (saleGroupId);
                val.saleDeptId = (saleDeptId);
                val.salerId = (salerId);
                val.saleOrgId = (saleOrgId);
                //原销售可销查询非常慢，修改为自定义sql
                //string empty = string.Empty;
                //ISCMServiceForBD service = ServiceFactory.GetService<ISCMServiceForBD>(ctx);
                //List<long> list = service.GetAllSaleCustCtrlNew(((AbstractValidator)this).Context, val, "bill", out empty);
                string sSql = $@"SELECT t0.FCUSTID fcustid FROM T_BD_CUSTOMER t0
WHERE 
              t0.FISTRADE = '1'
              AND t0.FFORBIDSTATUS = 'A'
			  AND EXISTS
    (
        SELECT 1
        FROM T_SAL_SCSALERCUST sg
            INNER JOIN T_BD_OPERATORDETAILS g
                ON sg.FSALERGROUPID = g.FOPERATORGROUPID
            INNER JOIN T_BD_OPERATORENTRY e
                ON (
                       g.FENTRYID = e.FENTRYID
                       AND e.FOPERATORTYPE = 'XSY'
                   )
            INNER JOIN V_BD_SALESMAN vsal
                ON e.FSTAFFID = vsal.FSTAFFID
        WHERE (
                  vsal.fid = {salerId}
                  AND t0.FCUSTID = sg.FCUSTOMERID
              )
    )";
                List<long> list = DBUtils.ExecuteDynamicObject(this.Context, sSql).Select(x => Convert.ToInt64(x[0])).ToList();
                if (list == null)
                {
                    list = new List<long>();
                }
                result = ((text == "0") ? (list.Count > 0 && list.Contains(custId)) : ((text == "2") ? (list.Count <= 0 || list.Contains(custId)) : (list.Count <= 0 || !list.Contains(custId))));
            }
            return result;
        }
    }
}
