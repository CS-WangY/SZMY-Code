using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.BusinessEntity.CloudPlatform;

namespace Kingdee.Mymooo.ServicePlugIn.PLN_REQUIREMENTORDER
{
    [Description("组织间需求单审核更新价目表"), HotUpdate]
    public class Audit : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FDemandType");
            e.FieldKeys.Add("FDemandOrgId");
            e.FieldKeys.Add("FSupplyOrganId");
            e.FieldKeys.Add("F_PENY_Price");
            e.FieldKeys.Add("FCreateType");
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            foreach (var item in e.DataEntitys)
            {
                long[] SalOrgList = new long[] { 224428, 1043841, 7348029 };

                if (!SalOrgList.Contains(Convert.ToInt64(item["DemandOrgId_Id"]))
                    && Convert.ToInt64(item["DemandOrgId_Id"]) != Convert.ToInt64(item["SupplyOrgId_Id"]))
                {
                    if (Convert.ToString(item["CreateType"]) != "B")
                    {
                        if (Convert.ToDecimal(item["F_PENY_Price"]) > 0)
                        {
                            var request = new RequirementOrderRequest()
                            {
                                AccountOrgId = Convert.ToInt64(item["DemandOrgId_Id"]),
                                SupplyOrgId = Convert.ToInt64(item["SupplyOrgId_Id"]),
                                Details = new List<RequirementOrderDetailRequest>()
                            {
                                new RequirementOrderDetailRequest
                                {
                                    MaterialNumber = Convert.ToString(((DynamicObject)item["MaterialId"])["Number"]),
                                    Price = Convert.ToDecimal(item["F_PENY_Price"]),
                                }
                            },
                            };
                            CetatePriceList(this.Context, request);
                        }
                    }
                }
            }
        }
        private void CetatePriceList(Context ctx, RequirementOrderRequest request)
        {
            var sql = "select FID from T_IOS_PRICELIST where FCreateOrgId = @FCreateOrgId and F_PENY_SupplyOrgId = @FSupplyOrgId";
            var id = DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0, new SqlParam("@FCreateOrgId", KDDbType.Int64, request.AccountOrgId), new SqlParam("@FSupplyOrgId", KDDbType.Int64, request.SupplyOrgId));
            IBillView billView;
            if (id == 0)
            {
                billView = FormMetadataUtils.CreateBillView(ctx, "IOS_PriceList");
                billView.Model.SetValue("FCreateOrgId", request.AccountOrgId);
                //主业务组织赋值失败.直接对属性操作吧
                //billView.Model.DataObject["CreateOrgId_Id"] = request.AccountOrgId;
                //var createOrgType = (billView.BusinessInfo.GetField("FCreateOrgId") as BaseDataField).RefFormDynamicObjectType;
                //billView.Model.DataObject["CreateOrgId"] = BusinessDataServiceHelper.LoadFromCache(ctx,    new object[] { request.AccountOrgId }, createOrgType)[0];

                billView.Model.SetValue("F_PENY_SupplyOrgId", request.SupplyOrgId);
                billView.Model.SetValue("FName", "组织间价目表");
                billView.Model.SetValue("FIsIncludedTax", true);
                billView.Model.DeleteEntryRow("FEntity", 0);
            }
            else
            {
                billView = FormMetadataUtils.CreateBillView(ctx, "IOS_PriceList", id);
            }
            var entity = billView.BusinessInfo.GetEntity("FEntity");
            var entryDatas = billView.Model.GetEntityDataObject(entity);
            foreach (var item in request.Details.GroupBy(p => p.MaterialNumber).Select(p => p.FirstOrDefault()))
            {
                var data = entryDatas.FirstOrDefault(p => (p["MATERIALID"] as DynamicObject)?["Number"].ToString().EqualsIgnoreCase(item.MaterialNumber) ?? false);
                if (data != null)
                {
                    var index = entryDatas.IndexOf(data);
                    billView.Model.SetValue("FTaxPrice", item.Price, index);
                    billView.InvokeFieldUpdateService("FTaxPrice", index);
                }
                else
                {
                    billView.Model.CreateNewEntryRow("FEntity");
                    var index = billView.Model.GetEntryRowCount("FEntity") - 1;
                    billView.Model.SetItemValueByNumber("FMaterialId", item.MaterialNumber, index);
                    billView.InvokeFieldUpdateService("FMaterialId", index);
                    billView.Model.SetValue("FTaxPrice", item.Price, index);
                    billView.InvokeFieldUpdateService("FTaxPrice", index);
                }
            }
            IOperationResult oper;
            if (id == 0)
            {
                oper = MymoooBusinessDataServiceHelper.SaveAndAuditBill(ctx, billView.BusinessInfo, billView.Model.DataObject);
            }
            else
            {
                oper = MymoooBusinessDataServiceHelper.SaveBill(ctx, billView.BusinessInfo, billView.Model.DataObject);
            }
            //清除释放网控
            billView.CommitNetworkCtrl();
            billView.InvokeFormOperation(FormOperationEnum.Close);
            billView.Close();
            if (!oper.IsSuccess)
            {
                if (oper.ValidationErrors.Count > 0)
                {
                    throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
                }
                else
                {
                    throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                }
            }
        }
    }
}
