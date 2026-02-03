using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.MFG.Contracts.PRD;
using Kingdee.Mymooo.Contracts.SalesManagement;
using Kingdee.Mymooo.Contracts.StockManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS;
using Kingdee.Mymooo.Core.StockManagement;
using System.Net;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.BOS.Util;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata;
using Kingdee.Mymooo.Core.PlanOrderManagement;
using Kingdee.BOS.BusinessEntity.CloudHealthCenter;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.Mymooo.App.Core.BaseManagement;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.Enums;
using Kingdee.BOS.Core.Metadata.Operation;
using Kingdee.BOS.Core;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;

namespace Kingdee.Mymooo.App.Core.StockManagement
{
    /// <summary>
    /// 库存订单服务
    /// </summary>
    public class StockOrderService : IStockOrderService
    {
        /// <summary>
        /// MES生成其他入库单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> MesStkMiscellaneousService(Context ctx, Mes_STK_MiscellaneousRequest request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            IOperationResult oper = new OperationResult();
            //如果已经生成其他入库单，则跳过
            string sql = $@"select top 1 FBILLNO from T_STK_MISCELLANEOUS where FBILLNO='{request.BillNo}' ";
            if (!string.IsNullOrWhiteSpace(DBServiceHelper.ExecuteScalar<string>(ctx, sql, "")))
            {
                response.Code = ResponseCode.Success;
                response.Message = "已生成其他入库单";
                return response;
            }
            var org = FormMetadataUtils.GetIdForNumber(ctx, new OrganizationsInfo(request.StockOrgNumber, ""));
            if (org.Id == 0 || !org.DocumentStatus.EqualsIgnoreCase("C")) 
            {
                response.Code = ResponseCode.ModelError;
                response.Message = "对应的组织不存在或未审核";
                return response;
            }
            request.StockOrgId = org.Id;
            //创建物料和分配
            CreateMaterialInfo(ctx, request, org);
            response.Data = CreateStkMiscellaneousBillView(ctx, request);
            response.Code = ResponseCode.Success;
            return response;
        }
        //创建物料和分配
        private void CreateMaterialInfo(Context ctx, Mes_STK_MiscellaneousRequest request, OrganizationsInfo org)
        {
            MaterialService materialService = new MaterialService();
            FormMetadata meta = MetaDataServiceHelper.Load(ctx, "BD_MATERIAL") as FormMetadata;
            Dictionary<string, MaterialInfo> materials = new Dictionary<string, MaterialInfo>(StringComparer.OrdinalIgnoreCase);
            List<long> materialss = new List<long>();
            AllocateService allocateService = new AllocateService();
            foreach (var detail in request.Details)
            {
                detail.MaterialCode = detail.MaterialCode.Trim();

                var materialInfo = FormMetadataUtils.GetIdForNumber(ctx, new MaterialInfo(detail.MaterialCode, ""));
                var materialId = materialInfo.MasterId;
                //是否需要分配
                var IsAllocate = false;
                materialInfo.Name = detail.MaterialName.Trim();
                materialInfo.Length = detail.Length;
                materialInfo.Width = detail.Width;
                materialInfo.Height = detail.Height;
                materialInfo.Weight = detail.Weight;
                materialInfo.Volume = detail.Volume;
                materialInfo.Textures = detail.Textures;
                materialInfo.MaterialType = detail.MaterialType;
                materialInfo.ProductId = detail.ProductId;
                materialInfo.UseOrgId = 1;
                materialInfo.ShortNumber = "";
                materialInfo.PriceType = "";
                materialInfo.ProductSmallClass = detail.ProductSmallClass;
                materialInfo.WeightUnitid = Convert.ToString(GetUnitId(ctx, detail.WeightUnitid));
                materialInfo.VolumeUnitid = Convert.ToString(GetUnitId(ctx, detail.VolumeUnitid));
                materialInfo.Specs = detail.Specs;
                string baseUnit = "";
                string storeUnit = "";
                string purchaseUnit = "";
                string saleUnit = "";
                switch (detail.VolumeUnitid.ToUpper())
                {
                    case "CM":
                        baseUnit = "m";
                        storeUnit = "cm";
                        purchaseUnit = "cm";
                        saleUnit = "cm";
                        break;
                    case "MM":
                        baseUnit = "m";
                        storeUnit = "mm";
                        purchaseUnit = "mm";
                        saleUnit = "mm";
                        break;
                    default:
                        baseUnit = detail.VolumeUnitid;
                        storeUnit = detail.VolumeUnitid;
                        purchaseUnit = detail.VolumeUnitid;
                        saleUnit = detail.VolumeUnitid;
                        break;
                }

                materialInfo.FBaseUnitId = baseUnit;
                materialInfo.FStoreUnitID = storeUnit;
                materialInfo.FPurchaseUnitId = purchaseUnit;
                materialInfo.FPurchasePriceUnitId = purchaseUnit;
                materialInfo.FSaleUnitId = saleUnit;
                //新增
                if (!materials.ContainsKey(detail.MaterialCode) && materialId == 0)
                {
                    IsAllocate = true;
                    var material = materialService.TryGetOrAdd(ctx, materialInfo, new List<long>() { org.Id }, false);
                    materialId = material.MasterId;
                }
                else
                {
                    IsAllocate = false;
                    //没有更新过才更新。防止1个物料分2行
                    if (!materials.ContainsKey(detail.MaterialCode))
                    {
                        materialService.TryGetOrAdd(ctx, materialInfo, new List<long>() { org.Id }, false);
                    }
                }
                materialss.Add(materialId);
                //不存在就新增，防止1个物料分2行
                if (!materials.ContainsKey(detail.MaterialCode))
                {
                    materials[detail.MaterialCode] = materialInfo;
                }
                //分配组织
                if (IsAllocate)
                {
                    var billView = FormMetadataUtils.CreateBillView(ctx, "BD_MATERIAL", materialId);
                    AllocateParameter allocateParameter = new AllocateParameter(billView.BusinessInfo, meta.InheritPath, 1, BOSEnums.Enu_AllocateType.Allocate, OperationNumberConst.OperationNumber_Allocate)
                    {
                        PkId = new List<object>() { Convert.ToInt64(billView.Model.DataObject["Id"]) },
                        AutoSubmitAndAudit = true,
                        AllocateUserId = ctx.UserId,
                        DestOrgId = (org.Id),
                        DestOrgName = org.Name
                    };
                    var oper = allocateService.Allocate(ctx, allocateParameter);
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
                MaterialServiceHelper.MaterialAllocateToAll(ctx, materialss);
            }
        }

        private long GetUnitId(Context ctx, string number)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@UnitId", KDDbType.String, number) };
            var sql = $@"/*dialect*/SELECT FUNITID FROM dbo.T_BD_UNIT WHERE FNUMBER=@UnitId";
            return DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0, paramList: pars.ToArray());
        }

        private IOperationResult CreateStkMiscellaneousBillView(Context ctx, Mes_STK_MiscellaneousRequest request)
        {
            MymoooBusinessDataService service = new MymoooBusinessDataService();
            var billView = FormMetadataUtils.CreateBillView(ctx, "STK_MISCELLANEOUS");
            //billView.Model.SetValue("FBillTypeID", bill.FBillTypeID, 0);
            billView.Model.SetValue("FBillNo", request.BillNo, 0);
            billView.Model.SetValue("FStockOrgId", request.StockOrgId, 0);
            billView.InvokeFieldUpdateService("FStockOrgId", 0);
            billView.Model.SetValue("FBillTypeID", request.BillTypeID, 0);
            billView.Model.SetValue("FStockDirect", request.StockDirect, 0);
            billView.Model.SetItemValueByNumber("FDEPTID", request.DeptID, 0);
            billView.Model.SetValue("FDate", request.FDate, 0);
            billView.Model.SetValue("FNOTE", request.Note, 0);

            billView.Model.DeleteEntryData("FEntity");
            var rowcount = 0;
            foreach (var item in request.Details)
            {
                billView.Model.CreateNewEntryRow("FEntity");
                billView.Model.SetItemValueByNumber("FMATERIALID", item.MaterialCode, rowcount);
                billView.InvokeFieldUpdateService("FMATERIALID", rowcount);
                billView.Model.SetItemValueByNumber("FSTOCKID", item.StockCode, rowcount);
                billView.InvokeFieldUpdateService("FSTOCKID", rowcount);
                billView.Model.SetValue("FQty", item.Qty, rowcount);
                billView.InvokeFieldUpdateService("FQty", rowcount);
                billView.Model.SetValue("FEntryNote", item.Note, rowcount);
                rowcount++;
            }
            var oper = service.SaveAndAuditBill(ctx, billView.BillBusinessInfo, new DynamicObject[] { billView.Model.DataObject });
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
            else
            {
                return oper;
            }
        }
        /// <summary>
        /// MES生成盘盈盘亏单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> MesGenerateStkCountGainAndLossService(Context ctx, MesGenerateStkCountGainAndLossRequest request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            //如果已经生成盘盈或者盘亏，则跳过
            string sql = $@"select top 1 FBILLNO from T_STK_STKCOUNTGAIN where FBILLNO='{request.BillNo}' ";
            if (!string.IsNullOrWhiteSpace(DBServiceHelper.ExecuteScalar<string>(ctx, sql, "")))
            {
                response.Code = ResponseCode.Success;
                response.Message = "已生成盘盈单";
                return response;
            }
            sql = $@"select top 1 FBILLNO from T_STK_STKCOUNTLOSS where FBILLNO='{request.BillNo}' ";
            if (!string.IsNullOrWhiteSpace(DBServiceHelper.ExecuteScalar<string>(ctx, sql, "")))
            {
                response.Code = ResponseCode.Success;
                response.Message = "已生成盘亏单";
                return response;
            }
            var org = FormMetadataUtils.GetIdForNumber(ctx, new OrganizationsInfo(request.StockOrgCode, ""));
            if (org.Id == 0 || !org.DocumentStatus.EqualsIgnoreCase("C"))
            {
                response.Code = ResponseCode.ModelError;
                response.Message = "对应的组织不存在或未审核";
                return response;
            }
            request.StockOrgId = org.Id;
            //获取账面数量
            foreach (var item in request.Details)
            {
                var material = FormMetadataUtils.GetIdForNumber(ctx, new MaterialInfo(item.MaterialCode, ""));
                item.AcctQty = GetItemStockQty(ctx, request.StockOrgId, material.MasterId, item.StockCode);
            }
            MymoooBusinessDataService service = new MymoooBusinessDataService();
            bool isSuccess = true;
            try
            {
                using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                {
                    //盘点数量小于账存数量 走盘亏
                    if (request.Details.Where(x => x.CountQty < x.AcctQty).Count() > 0)
                    {
                        var billView = FormMetadataUtils.CreateBillView(ctx, "STK_StockCountLoss");
                        billView.Model.SetValue("FBillNo", request.BillNo);
                        billView.Model.SetValue("FStockOrgId", request.StockOrgId, 0);
                        billView.InvokeFieldUpdateService("FStockOrgId", 0);
                        billView.Model.SetItemValueByNumber("FOwnerTypeIdHead", "BD_OwnerOrg", 0);
                        billView.InvokeFieldUpdateService("FOwnerTypeIdHead", 0);
                        billView.Model.SetItemValueByNumber("FOwnerIdHead", request.StockOrgCode, 0);
                        billView.InvokeFieldUpdateService("FOwnerIdHead", 0);
                        billView.Model.SetValue("FDate", request.Date);
                        billView.Model.SetValue("FNoteHead", request.NoteHead);
                        int rowIndex = 0;
                        List<DynamicObject> dynamicObjects = new List<DynamicObject>();
                        foreach (var det in request.Details.Where(x => x.CountQty < x.AcctQty))
                        {
                            if (rowIndex > 0)
                            {
                                billView.Model.CreateNewEntryRow("FBillEntry");
                            }

                            billView.Model.SetItemValueByNumber("FMaterialId", det.MaterialCode, rowIndex);
                            billView.InvokeFieldUpdateService("FMaterialId", rowIndex);
                            billView.Model.SetItemValueByNumber("FStockId", det.StockCode, rowIndex);
                            billView.InvokeFieldUpdateService("FStockId", rowIndex);
                            billView.Model.SetValue("FCountQty", det.CountQty, rowIndex);
                            billView.InvokeFieldUpdateService("FCountQty", rowIndex);
                            billView.Model.SetValue("FNote", det.Note, rowIndex);
                            rowIndex++;
                        }
                        var oper = service.SaveAndAuditBill(ctx, billView.BusinessInfo, new DynamicObject[] { billView.Model.DataObject }.ToArray());
                        if (!oper.IsSuccess)
                        {
                            isSuccess = false;
                            if (oper.ValidationErrors.Count > 0)
                            {
                                response.Message += "盘亏单：" + string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                                response.Code = ResponseCode.Exception;
                            }
                            else
                            {
                                response.Message += "盘亏单：" + string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                                response.Code = ResponseCode.Exception;
                            }
                        }
                        else
                        {
                            response.Message = "盘亏单：" + string.Join(";", oper.OperateResult.Select(p => p.Message));
                            response.Code = ResponseCode.Success;
                            billView.CommitNetworkCtrl();
                            billView.InvokeFormOperation(FormOperationEnum.Close);
                            billView.Close();
                        }
                    }
                    if (!isSuccess)
                    {
                        return response;
                    }
                    //盘点数量大于账存数量 走盘盈
                    if (request.Details.Where(x => x.CountQty > x.AcctQty).Count() > 0)
                    {
                        var billView = FormMetadataUtils.CreateBillView(ctx, "STK_StockCountGain");
                        billView.Model.SetValue("FBillNo", request.BillNo);
                        billView.Model.SetValue("FStockOrgId", request.StockOrgId, 0);
                        billView.InvokeFieldUpdateService("FStockOrgId", 0);
                        billView.Model.SetValue("FDate", request.Date);
                        billView.Model.SetValue("FNoteHead", request.NoteHead);
                        billView.Model.SetItemValueByNumber("FOwnerTypeIdHead", "BD_OwnerOrg", 0);
                        billView.InvokeFieldUpdateService("FOwnerTypeIdHead", 0);
                        billView.Model.SetItemValueByNumber("FOwnerIdHead", request.StockOrgCode, 0);
                        billView.InvokeFieldUpdateService("FOwnerIdHead", 0);
                        int rowIndex = 0;
                        List<DynamicObject> dynamicObjects = new List<DynamicObject>();
                        foreach (var det in request.Details.Where(x => x.CountQty > x.AcctQty))
                        {
                            if (rowIndex > 0)
                            {
                                billView.Model.CreateNewEntryRow("FBillEntry");
                            }
                            billView.Model.SetItemValueByNumber("FMaterialId", det.MaterialCode, rowIndex);
                            billView.InvokeFieldUpdateService("FMaterialId", rowIndex);
                            billView.Model.SetItemValueByNumber("FStockId", det.StockCode, rowIndex);
                            billView.InvokeFieldUpdateService("FStockId", rowIndex);
                            billView.Model.SetValue("FCountQty", det.CountQty, rowIndex);
                            billView.InvokeFieldUpdateService("FCountQty", rowIndex);
                            billView.Model.SetValue("FNote", det.Note, rowIndex);
                            rowIndex++;
                        }
                        var oper = service.SaveAndAuditBill(ctx, billView.BusinessInfo, new DynamicObject[] { billView.Model.DataObject }.ToArray());
                        if (!oper.IsSuccess)
                        {
                            isSuccess = false;
                            if (oper.ValidationErrors.Count > 0)
                            {
                                response.Message += " 盘盈单：" + string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                                response.Code = ResponseCode.Exception;
                            }
                            else
                            {
                                response.Message += " 盘盈单：" + string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                                response.Code = ResponseCode.Exception;
                            }
                        }
                        else
                        {
                            response.Message += "盘盈单：" + string.Join(";", oper.OperateResult.Select(p => p.Message));
                            response.Code = ResponseCode.Success;
                            billView.CommitNetworkCtrl();
                            billView.InvokeFormOperation(FormOperationEnum.Close);
                            billView.Close();
                        }
                    }
                    //全部成功则提交事务
                    if (isSuccess)
                    {
                        if (string.IsNullOrEmpty(response.Code))
                        {
                            response.Message += "当前盘点数量和实际库存完全一致，无需生成盘盈盘亏订单。";
                            response.Code = ResponseCode.Success;
                        }
                        cope.Complete();
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.Exception;
                response.Message = ex.Message;
            }
            return response;
        }
        /// <summary>
        /// 获取物料库存量(按库存单位)
        /// </summary>
        /// <returns></returns>
        private decimal GetItemStockQty(Context ctx, long stockOrgId, long masterId, string stockCode)
        {
            List<SqlParam> pars = new List<SqlParam>() {
                new SqlParam("@StockOrgId", KDDbType.Int64, stockOrgId),
                new SqlParam("@MasterId", KDDbType.Int64, masterId) ,
                new SqlParam("@StockCode", KDDbType.String, stockCode) };
            var sql = $@"/*dialect*/SELECT 
                            ISNULL(t1.FQTY, 0) as FQTY FROM V_STK_INVENTORY_CUS t1
                            LEFT JOIN T_BD_MATERIAL M on t1.FMATERIALID=M.FMASTERID
                            LEFT JOIN T_BD_STOCK t3 on t1.FSTOCKID=t3.FSTOCKID
                            WHERE M.FMATERIALID =@MasterId and FSTOCKORGID=@StockOrgId AND t3.FNUMBER=@StockCode AND t3.FISDIRSTOCK=0
                            ORDER BY t3.FISOUTSOURCESTOCK,t3.FNUMBER DESC";
            return DBServiceHelper.ExecuteScalar<decimal>(ctx, sql, 0, paramList: pars.ToArray());
        }
    }
}
