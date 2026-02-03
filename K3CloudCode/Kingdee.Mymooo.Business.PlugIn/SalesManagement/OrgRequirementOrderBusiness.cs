using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kingdee.Mymooo.Business.PlugIn.SalesManagement
{
    /// <summary>
    /// 组织间需求单
    /// </summary>
    public class OrgRequirementOrderBusiness : IMessageExecute
    {
        private readonly object _lock = new object();

        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            RequirementOrderRequest request = JsonConvertUtils.DeserializeObject<RequirementOrderRequest>(message);

            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (string.IsNullOrWhiteSpace(request.RequirementOrderNo))
            {
                response.Code = ResponseCode.ModelError;
                response.Message = "需求单号不能为空！";
                return response;
            }

            if (request.RequirementDate == null || request.RequirementDate < new DateTime(2023, 1, 1))
            {
                response.Code = ResponseCode.ModelError;
                response.Message = "需求日期不能为空！";
                return response;
            }

            var org = FormMetadataUtils.GetIdForNumber(ctx, new OrganizationsInfo(request.RequirementOrgNumber, ""));
            if (org.Id == 0 || !org.DocumentStatus.EqualsIgnoreCase("C"))
            {
                response.Code = ResponseCode.NoExistsData;
                response.Message = "对应的需求组织不存在或未审核";
                return response;
            }
            request.RequirementOrgId = org.Id;

            org = FormMetadataUtils.GetIdForNumber(ctx, new OrganizationsInfo(request.SupplyOrgNumber, ""));
            if (org.Id == 0 || !org.DocumentStatus.EqualsIgnoreCase("C"))
            {
                response.Code = ResponseCode.NoExistsData;
                response.Message = "对应的供应组织不存在或未审核";
                return response;
            }
            request.SupplyOrgId = org.Id;
            //根据供应组织找到对应的核算组织
            var sql = @"select e.FMAINORGID
from T_ORG_ACCTSYSENTRY e
	inner join T_ORG_ACCTSYSDETAIL d on e.FENTRYID = d.FENTRYID
where e.FACCTSYSTEMID = 1 and d.FSUBORGID = @FSUBORGID";
            request.AccountOrgId = DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0, new SqlParam("@FSUBORGID", KDDbType.Int64, request.SupplyOrgId));
            if (request.AccountOrgId == 0)
            {
                response.Code = ResponseCode.NoExistsData;
                response.Message = "供应组织无对应的核算组织!";
                return response;
            }

            int row = 1;
            foreach (var detail in request.Details)
            {
                if (string.IsNullOrWhiteSpace(detail.MaterialNumber))
                {
                    response.Code = ResponseCode.ModelError;
                    response.Message += $"第{row}行物料编码为空！\n";
                    return response;
                }

                if (detail.DeliveryDate == null || detail.DeliveryDate < new DateTime(2023, 1, 1))
                {
                    response.Code = ResponseCode.ModelError;
                    response.Message += $"第{row}行交期为空！\n";
                    return response;
                }

                if (detail.CostPriceUpdateDate != null && detail.CostPriceUpdateDate < new DateTime(2023, 1, 1))
                {
                    response.Code = ResponseCode.ModelError;
                    response.Message += $"第{row}行成本更新日期格式不对！\n";
                    return response;
                }
                row++;
            }

            var materials = MaterialServiceHelper.TryGetOrAdds(ctx, request.Details.GroupBy(r => r.MaterialNumber.Trim()).Select(p =>
            {
                var material = p.First();
                var materialInfo = new MaterialInfo(p.Key.Trim(), material.MaterialName);
                materialInfo.ProductId = material.ProductId;
                materialInfo.UseOrgId = request.RequirementOrgId;
                materialInfo.ShortNumber = material.ShortNumber;
                materialInfo.PriceType = material.PriceType;
                materialInfo.ProductSmallClass = material.ProductSmallClass;

                return materialInfo;
            }).ToArray(), new List<long>() { request.RequirementOrgId, request.SupplyOrgId, 224428 });

            lock (_lock)
            {
                //操作价目表,不支持多个进程一起操作
                this.CetatePriceList(ctx, request);
            }
            this.CetateBill(ctx, request);
            MaterialServiceHelper.MaterialAllocateToAll(ctx, materials.Select(x => Convert.ToInt64(x.MasterId)).ToList());
            response.Code = ResponseCode.Success;
            return response;
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

        private void CetateBill(Context ctx, RequirementOrderRequest request)
        {
            var billView = FormMetadataUtils.CreateBillView(ctx, "PLN_REQUIREMENTORDER");

            List<DynamicObject> objs = new List<DynamicObject>();
            int row = 1;
            foreach (var item in request.Details)
            {
                if (row > 1)
                {
                    billView.CreateNewModelData();
                }
                billView.Model.SetValue("FDemandOrgId", request.RequirementOrgId);
                billView.Model.SetItemValueByNumber("FMaterialId", item.MaterialNumber, 0);
                billView.InvokeFieldUpdateService("FMaterialId", 0);
                billView.Model.SetValue("FSupplyOrgId", request.SupplyOrgId);
                billView.Model.SetValue("FSupplyOrganId", request.SupplyOrgId);
                billView.Model.SetValue("FDemandDate", request.RequirementDate);
                billView.Model.SetValue("FBillNo", $"{request.RequirementOrderNo}_{item.Seq}");
                billView.Model.SetValue("FDemandQty", item.Qty);
                billView.Model.SetValue("FFirmQty", item.Qty);
                billView.InvokeFieldUpdateService("FFirmQty", 0);
                billView.Model.SetValue("F_PENY_Price", item.Price);
                //billView.Model.SetValue("F_PENY_Amount", item.Amount);
                billView.Model.SetValue("FFirmFinishDate", item.DeliveryDate);
                billView.Model.SetValue("FDemandType", "8");
                billView.Model.SetValue("FSaleOrderNo", request.RequirementOrderNo);
                billView.Model.SetValue("FSaleOrderEntrySeq", item.Seq);

                billView.Model.SetItemValueByNumber("FProductEngineerId", item.ProductEngineerNumber, 0);
                billView.Model.SetItemValueByNumber("FProductManagerId", item.ProductManagerNumber, 0);
                billView.Model.SetValue("FSupplierId", GetSupplierId(ctx, 1, item.SupplierNumber));
                //billView.Model.SetItemValueByNumber("FSupplierId", item.SupplierNumber, 0);
                billView.Model.SetValue("FSupplierUnitPrice", item.SupplierUnitPrice);
                billView.Model.SetValue("FCostPriceUpdateDate", item.CostPriceUpdateDate);
                billView.Model.SetValue("FSupplierUnitPriceSource", item.SupplierUnitPriceSource);
                billView.Model.SetValue("FCostPriceUpdateUser", item.CostPriceUpdateUser);
                billView.Model.SetValue("FSupplierProductCode", item.SupplierProductCode);
                billView.Model.SetValue("FDescription", item.Remark);
                billView.Model.SetValue("FInsideRemark", item.InsideRemark);

                //billView.Model.SetItemValueByNumber("F_PENY_MaterialGroup", item.ProductSmallClass?.Code, 0);
                //billView.Model.SetItemValueByNumber("F_PENY_MaterialParentGroup", item.ProductSmallClass?.ParentCode, 0);
                //billView.Model.SetItemValueByNumber("F_PENY_BusinessDivision", item.BusinessDivisionNumber, 0);

                objs.Add(billView.Model.DataObject);
                row++;
            }

            var oper = MymoooBusinessDataServiceHelper.SaveAndAuditBill(ctx, billView.BusinessInfo, objs.ToArray());
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

        /// <summary>
        /// 根据供应商编号获取供应商ID
        /// </summary>
        /// <param name="SupplierCode"></param>
        /// <returns></returns>
        private long GetSupplierId(Context ctx, long useOrgId, string supplierCode)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@UseOrgId", KDDbType.Int64, useOrgId) ,
                new SqlParam("@SupplierCode", KDDbType.String, supplierCode) };
            var sql = $@"select top 1 FSUPPLIERID from t_BD_Supplier where FUSEORGID=@UseOrgId and  FNUMBER=@SupplierCode   and FDOCUMENTSTATUS='C' and FFORBIDSTATUS='A'  ";
            return DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0, paramList: pars.ToArray());
        }
    }
}
