using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm;
using Kingdee.BOS.ServiceHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.Mymooo.Core;
using Kingdee.BOS.Util;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.Const;
using Kingdee.BOS.Core.List;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.Mymooo.Core.DirectSaleManagement;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Msg;
using Kingdee.Mymooo.Contracts.BaseManagement;
using Kingdee.Mymooo.Contracts.DirectSaleManagement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;
using Kingdee.Mymooo.Core.StockManagement;
using System.Net;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Contracts;
using System.Security.Cryptography;
using Kingdee.BOS.Core.Bill;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.Mymooo.App.Core.StockManagement;
using Kingdee.BOS.Core.BusinessFlow.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;

namespace Kingdee.Mymooo.App.Core.DirectSaleManagement
{
    public class DirectSaleService : IDirectSaleService
    {
        /// <summary>
        /// 采购订单下推收料通知单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> PoPushReceiveMaterials(Context ctx, SoDirEntity request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            request.DirStockId = GetSaleOrgStockId(ctx, request.SoFId);
            response.Data = request;
            //获取到需要直发的采购订单数据
            List<PoDirQtyDet> poDetList = new List<PoDirQtyDet>();
            foreach (var item in request.SoDet)
            {
                foreach (var items in item.PoDet.Where(x => x.ReqDirQty > 0))
                {
                    poDetList.Add(items);
                }
            }
            if (poDetList.Count == 0)
            {
                response.Code = ResponseCode.ModelError;
                response.Message = "没有可直发的数据";
                return response;
            }

            //验证是否已经生成收料通知单
            if (IsExistsDirNo(ctx, request.DirNo, 0))
            {
                response.Code = ResponseCode.Success;
                response.Message = "已经生成收料通知单";
                return response;
            }
            //验证是否存在采购预留
            var sql = $@"/*dialect*/select top 1 FENTRYID from t_PUR_POOrderDirReServeQty where FDirNo ='{request.DirNo}' ";
            var poEntryId = DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0);
            if (poEntryId == 0)
            {
                response.Code = ResponseCode.ModelError;
                response.Message = "采购预留不存在，请稍后再试！";
                return response;
            }

            MymoooBusinessDataService service = new MymoooBusinessDataService();

            //根据采购订单和序号汇总可直发数量
            var PoDetNewData = poDetList
                    .GroupBy(g => new { g.DirStockId, g.PoOrgID, g.PoFId, g.PoEntryId })
                    .Select(t => new { PoOrgID = t.Key.PoOrgID, DirStockId = t.Key.DirStockId, PoFId = t.Key.PoFId, PoEntryId = t.Key.PoEntryId, Qty = t.Sum(s => s.ReqDirQty) }).ToList();
            try
            {
                //处理数据
                using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                {
                    //先处理释放采购预留
                    request.IsAddReServeQty = false;
                    SetPoDirReServeQty(ctx, request);

                    List<long> longs = new List<long>();
                    //不同采购订单分开下推收料通知单
                    foreach (var poFId in poDetList.Select(o => o.PoFId).Distinct())
                    {
                        long dirStockId = PoDetNewData.Where(x => x.PoFId == poFId).FirstOrDefault().DirStockId;
                        List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
                        // 采购订单下推收料通知单
                        var rules = ConvertServiceHelper.GetConvertRules(ctx, "PUR_PurchaseOrder", "PUR_ReceiveBill");
                        var rule = rules.FirstOrDefault(t => t.IsDefault);
                        if (rule == null)
                        {
                            throw new Exception("没有从采购订单下推收料通知单的转换关系");
                        }
                        foreach (var item in PoDetNewData.Where(x => x.PoFId == poFId))
                        {
                            selectedRows.Add(new ListSelectedRow(item.PoFId.ToString(), item.PoEntryId.ToString(), 0, "PUR_PurchaseOrder") { EntryEntityKey = "FPOOrderEntry" });
                        }
                        //有数据才需要下推
                        if (selectedRows.Count > 0)
                        {
                            PushArgs pushArgs = new PushArgs(rule, selectedRows.ToArray())
                            {
                                TargetBillTypeId = "",     // 请设定目标单据单据类型
                                TargetOrgId = 0,            // 请设定目标单据主业务组织
                            };
                            //执行下推操作，并获取下推结果
                            var operationResult = ConvertServiceHelper.Push(ctx, pushArgs, null);
                            if (operationResult.IsSuccess)
                            {
                                var recView = FormMetadataUtils.CreateBillView(ctx, "PUR_ReceiveBill");
                                foreach (var item in operationResult.TargetDataEntities)
                                {
                                    recView.Model.DataObject = item.DataEntity;
                                    recView.Model.SetValue("FISDIRECTDELIVERY", "1");
                                    recView.Model.SetValue("FDirNo", request.DirNo);
                                    var recEntrys = recView.Model.GetEntityDataObject(recView.BusinessInfo.GetEntity("FDetailEntity"));
                                    foreach (var entry in recEntrys)
                                    {
                                        var thisList = PoDetNewData.FirstOrDefault(x => x.PoEntryId == Convert.ToInt64(((entry["FDetailEntity_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SId"]));
                                        var rowIndex = recEntrys.IndexOf(entry);
                                        recView.Model.SetValue("FActReceiveQty", thisList.Qty, rowIndex);
                                        recView.InvokeFieldUpdateService("FActReceiveQty", rowIndex);
                                        recView.Model.SetValue("FStockID", dirStockId, rowIndex);
                                        recView.InvokeFieldUpdateService("FStockID", rowIndex);
                                        //来料检验
                                        recView.Model.SetValue("FCheckInComing", "0", rowIndex);
                                        recView.Model.SetValue("FEmergencyRelease", "B", rowIndex);
                                        recView.Model.SetValue("FCURRENTSTRINGENCY", "", rowIndex);
                                        recView.InvokeFieldUpdateService("FCheckInComing", rowIndex);
                                        recView.InvokeFieldUpdateService("FEmergencyRelease", rowIndex);
                                    }
                                }
                                //保存批核
                                var opers = service.SaveAndAuditBill(ctx, recView.BusinessInfo, new DynamicObject[] { recView.Model.DataObject }.ToArray());
                                if (opers.IsSuccess)
                                {
                                    var pks = opers.SuccessDataEnity.Select(p => Convert.ToInt64(p["Id"]));
                                    request.ReceiveMaterials.AddRange(pks);
                                    //清除释放网控
                                    recView.CommitNetworkCtrl();
                                    recView.InvokeFormOperation(FormOperationEnum.Close);
                                    recView.Close();
                                }
                                else
                                {
                                    if (opers.ValidationErrors.Count > 0)
                                    {
                                        throw new Exception(string.Join(";", opers.ValidationErrors.Select(p => p.Message)));
                                    }
                                    else
                                    {
                                        throw new Exception(string.Join(";", opers.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                                    }
                                }
                            }
                            else
                            {
                                if (operationResult.ValidationErrors.Count > 0)
                                {
                                    throw new Exception(string.Join(";", operationResult.ValidationErrors.Select(p => p.Message)));
                                }
                                else
                                {
                                    throw new Exception(string.Join(";", operationResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("订单异常，采购订单下推收料通知单的数据丢失。");
                        }
                    }
                    response.Code = ResponseCode.Success;
                    response.Message = "生成收料通知单成功";
                    cope.Complete();
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
        /// 收料通知单下推采购入库单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> RmPushPurchasing(Context ctx, SoDirEntity request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            response.Data = request;
            MymoooBusinessDataService service = new MymoooBusinessDataService();
            //验证是否已经生成采购入库单
            if (IsExistsDirNo(ctx, request.DirNo, 1))
            {
                response.Code = ResponseCode.Success;
                response.Message = "已经存在采购入库单";
                return response;
            }
            try
            {
                //处理数据
                using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                {
                    foreach (var fid in request.ReceiveMaterials)
                    {
                        List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
                        //收料通知单
                        var view = FormMetadataUtils.CreateBillView(ctx, "PUR_ReceiveBill", fid);
                        var entrys = view.Model.GetEntityDataObject(view.BusinessInfo.GetEntity("FDetailEntity"));
                        foreach (var entry in entrys)
                        {
                            selectedRows.Add(new ListSelectedRow(fid.ToString(), Convert.ToString(entry["Id"]), 0, "PUR_ReceiveBill") { EntryEntityKey = "FDetailEntity" });

                        }
                        // 收料通知单下推采购入库单
                        var rules = ConvertServiceHelper.GetConvertRules(ctx, "PUR_ReceiveBill", "STK_InStock");
                        var rule = rules.FirstOrDefault(t => t.IsDefault);
                        if (rule == null)
                        {
                            throw new Exception("没有从收料通知单下推采购入库单的转换关系");
                        }
                        //有数据才需要下推
                        if (selectedRows.Count > 0)
                        {
                            PushArgs pushArgs = new PushArgs(rule, selectedRows.ToArray())
                            {
                                TargetBillTypeId = "",     // 请设定目标单据单据类型
                                TargetOrgId = 0,            // 请设定目标单据主业务组织
                            };
                            //执行下推操作，并获取下推结果
                            var operationResult = ConvertServiceHelper.Push(ctx, pushArgs, null);
                            if (operationResult.IsSuccess)
                            {
                                var inStockView = FormMetadataUtils.CreateBillView(ctx, "STK_InStock");

                                foreach (var item in operationResult.TargetDataEntities)
                                {
                                    inStockView.Model.DataObject = item.DataEntity;
                                }
                                inStockView.Model.SetValue("FDirTrackingNumber", request.TranCode);
                                //保存批核
                                var opers = service.SaveAndAuditBill(ctx, inStockView.BusinessInfo, new DynamicObject[] { inStockView.Model.DataObject }.ToArray());
                                if (opers.IsSuccess)
                                {

                                    //清除释放网控
                                    view.CommitNetworkCtrl();
                                    view.InvokeFormOperation(FormOperationEnum.Close);
                                    view.Close();

                                    //清除释放网控
                                    inStockView.CommitNetworkCtrl();
                                    inStockView.InvokeFormOperation(FormOperationEnum.Close);
                                    inStockView.Close();
                                    //var editReservedInfo = DirectEditReserved(ctx, request);
                                    //if (!editReservedInfo.IsSuccess)
                                    //{
                                    //    throw new Exception("修改预留数据报错：" + editReservedInfo.Message);
                                    //}
                                }
                                else
                                {
                                    if (opers.ValidationErrors.Count > 0)
                                    {
                                        throw new Exception(string.Join(";", opers.ValidationErrors.Select(p => p.Message)));
                                    }
                                    else
                                    {
                                        throw new Exception(string.Join(";", opers.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                                    }
                                }
                            }
                            else
                            {
                                if (operationResult.ValidationErrors.Count > 0)
                                {
                                    throw new Exception(string.Join(";", operationResult.ValidationErrors.Select(p => p.Message)));
                                }
                                else
                                {
                                    throw new Exception(string.Join(";", operationResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("订单异常，收料通知单下推采购入库单的数据丢失。");
                        }
                    }
                    response.Code = ResponseCode.Success;
                    response.Message = "生成采购入库单成功";
                    cope.Complete();
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
        /// 销售订单下推发货通知单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> SoPushDelivery(Context ctx, SoDirEntity request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            MymoooBusinessDataService service = new MymoooBusinessDataService();
            DirDnEntity dnEntity = new DirDnEntity();
            request.dnEntity = dnEntity;
            response.Data = request;
            //验证是否已经生成发货通知单
            if (IsExistsDirNo(ctx, request.DirNo, 2))
            {
                response.Code = ResponseCode.Success;
                response.Message = "已经存在发货通知单";
                return response;
            }
            try
            {
                //处理数据
                using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                {
                    List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();

                    foreach (var sodList in request.SoDet)
                    {
                        selectedRows.Add(new ListSelectedRow(request.SoFId.ToString(), Convert.ToString(sodList.SoEntryId), 0, "SAL_SaleOrder") { EntryEntityKey = "FSaleOrderEntry" });
                    }
                    //{ facbb53a-3904-4d14-b632-57f9b2e535fd_直发发货}
                    //{ 915a7d9b-2933-4a17-a509-636fd2b40513_担保直发流程}
                    var ConvertKey = "facbb53a-3904-4d14-b632-57f9b2e535fd";
                    if (request.IsWarrant)
                    {
                        ConvertKey = "915a7d9b-2933-4a17-a509-636fd2b40513";
                    }
                    // 销售订单下推发货通知单
                    var rules = ConvertServiceHelper.GetConvertRules(ctx, "SAL_SaleOrder", "SAL_DELIVERYNOTICE");
                    var rule = rules.FirstOrDefault(t => t.Key.EqualsIgnoreCase(ConvertKey));
                    if (rule == null)
                    {
                        throw new Exception("没有从销售订单下推发货通知单的直发转换关系");
                    }
                    //有数据才需要下推
                    if (selectedRows.Count > 0)
                    {
                        PushArgs pushArgs = new PushArgs(rule, selectedRows.ToArray())
                        {
                            TargetBillTypeId = "650bfcf4264fca", // 请设定目标单据单据类型
                            TargetOrgId = 0,            // 请设定目标单据主业务组织
                        };
                        //执行下推操作，并获取下推结果
                        var operationResult = ConvertServiceHelper.Push(ctx, pushArgs, null);
                        if (operationResult.IsSuccess)
                        {
                            var dnView = FormMetadataUtils.CreateBillView(ctx, "SAL_DELIVERYNOTICE");

                            foreach (var item in operationResult.TargetDataEntities)
                            {
                                dnView.Model.DataObject = item.DataEntity;
                                var dnEntrys = dnView.Model.GetEntityDataObject(dnView.BusinessInfo.GetEntity("FEntity"));
                                dnView.Model.SetValue("FTrackingNumber", request.TranCode);
                                dnView.Model.SetValue("FTrackingName", "直发");
                                dnView.Model.SetValue("FTrackingDate", DateTime.Now);
                                dnView.Model.SetValue("FDirNo", request.DirNo);
                                dnView.Model.SetItemValueByNumber("FCreatorId", request.CreateUserCode, 0);
                                dnView.Model.SetValue("FExpectedPaymentDate", request.ExpectedPaymentDate);
                                if (!string.IsNullOrWhiteSpace(request.ReceiveAddress))
                                {
                                    dnView.Model.SetValue("FReceiveAddress", request.ReceiveAddress);
                                    dnView.Model.SetValue("FLinkPhone", request.LinkPhone);
                                    dnView.Model.SetValue("FLinkMan", request.LinkMan);
                                    dnView.Model.SetValue("FPENYNOTE", request.CreateUserName);
                                }
                                var rowIndex = 0;
                                foreach (var entry in dnEntrys)
                                {
                                    var soDetId = Convert.ToInt64(((entry["FEntity_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SID"]);
                                    var thisSoList = request.SoDet.FirstOrDefault(x => x.SoEntryId == soDetId);
                                    decimal qty = request.SoDet.Where(x => x.SoEntryId.Equals(soDetId)).FirstOrDefault().PoDet.Sum(q => q.ReqDirQty);
                                    dnView.Model.SetValue("FQty", qty, rowIndex);
                                    dnView.InvokeFieldUpdateService("FQty", rowIndex);
                                    //view.Model.SetValue("FStockID", request.DirStockId, rowIndex);
                                    //view.InvokeFieldUpdateService("FStockID", rowIndex);
                                    rowIndex++;
                                }
                            }
                            //保存批核
                            var opers = service.SaveAndAuditBill(ctx, dnView.BusinessInfo, new DynamicObject[] { dnView.Model.DataObject }.ToArray());
                            if (opers.IsSuccess)
                            {
                                List<DirDnDetEntity> dnDetList = new List<DirDnDetEntity>();
                                foreach (var entry in (opers.SuccessDataEnity.ToList()[0]["SAL_DELIVERYNOTICEENTRY"]) as DynamicObjectCollection)
                                {
                                    dnDetList.Add(new DirDnDetEntity
                                    {
                                        EntryId = Convert.ToInt64(entry["Id"]),
                                        Seq = Convert.ToInt32(entry["Seq"]),
                                        MaterialId = Convert.ToInt64(((DynamicObject)entry["MaterialID"])["id"]),
                                        MsterID = Convert.ToInt64(((DynamicObject)entry["MaterialID"])["msterID"]),
                                        ItemNo = Convert.ToString(((DynamicObject)entry["MaterialID"])["Number"]),
                                        Qty = Convert.ToDecimal(entry["Qty"]),
                                        SalUnitID = Convert.ToInt64(((DynamicObject)entry["UnitID"])["id"]),
                                        SupplyOrgId = Convert.ToInt64(((DynamicObject)entry["FSupplyTargetOrgId"])["id"]),
                                        StockId = Convert.ToInt64(((DynamicObject)entry["StockID"])["id"]),
                                        SBillId = Convert.ToString(((entry["FEntity_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SBillId"]),
                                        SID = Convert.ToString(((entry["FEntity_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SID"]),
                                        OrderNo = Convert.ToString(entry["OrderNo"]),
                                        OrderSeq = Convert.ToString(entry["OrderSeq"])
                                    });
                                }
                                dnEntity.BillNo = Convert.ToString(opers.SuccessDataEnity.ToList()[0]["BillNo"]);
                                dnEntity.FId = Convert.ToInt64(opers.SuccessDataEnity.ToList()[0]["id"]);
                                dnEntity.Det = dnDetList;
                                //清除释放网控
                                dnView.CommitNetworkCtrl();
                                dnView.InvokeFormOperation(FormOperationEnum.Close);
                                dnView.Close();
                            }
                            else
                            {
                                if (opers.ValidationErrors.Count > 0)
                                {
                                    throw new Exception(string.Join(";", opers.ValidationErrors.Select(p => p.Message)));
                                }
                                else
                                {
                                    throw new Exception(string.Join(";", opers.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                                }
                            }
                        }
                        else
                        {
                            if (operationResult.ValidationErrors.Count > 0)
                            {
                                throw new Exception(string.Join(";", operationResult.ValidationErrors.Select(p => p.Message)));
                            }
                            else
                            {
                                throw new Exception(string.Join(";", operationResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("订单异常，销售订单下推发货通知单的数据丢失。");
                    }

                    response.Code = ResponseCode.Success;
                    response.Message = "生成发通知单成功";
                    cope.Complete();
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
        /// 生成调拨出库
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> CreateDeliveryTransferOut(Context ctx, SoDirEntity request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            response.Data = request;
            //验证是否已经生成调拨出库
            if (IsExistsDirNo(ctx, request.DirNo, 3))
            {
                response.Code = ResponseCode.Success;
                response.Message = "已经存在调拨出库";
                return response;
            }
            try
            {
                //需要调拨的数据集合
                List<Allocate> allocates = new List<Allocate>();
                foreach (var item in request.dnEntity.Det)
                {
                    decimal delqty = item.Qty;
                    var orderno = Convert.ToString(item.OrderNo);
                    var orderseq = Convert.ToString(item.OrderSeq);
                    //查询组织间需求单
                    string sSql = $@"/*dialect*/SELECT SUM(t1.FBASEQTY) AS FBASEQTY,t1.FSTOCKID,t2.FDEMANDINTERID,t1.FSUPPLYORGID,t1.FBASEUNITID
                        FROM T_PLN_RESERVELINKENTRY t1
                        INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
                        INNER JOIN T_PLN_REQUIREMENTORDER t3 ON t2.FDEMANDINTERID=t3.FID
                        LEFT JOIN T_BD_STOCK t4 ON t1.FSTOCKID=t4.FSTOCKID
                        WHERE t2.FSRCINTERID='{item.SBillId}' AND t2.FSRCENTRYID='{item.SID}'
                        AND t1.FSUPPLYFORMID='STK_Inventory' AND t2.FDEMANDFORMID='PLN_REQUIREMENTORDER'
                        AND t3.FDEMANDQTY-t3.FBASETRANOUTQTY>=0
                        AND t4.FISDIRSTOCK=1
                        GROUP BY t1.FSTOCKID,t2.FDEMANDINTERID,t1.FSUPPLYORGID,t1.FBASEUNITID";
                    var RequirementorderDatas = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
                    foreach (var reitem in RequirementorderDatas)
                    {
                        if (delqty <= 0)
                        {
                            continue;
                        }
                        decimal sqty = 0;
                        //如果存在单位不一致的情况，做单位转换数量再比较
                        var resunitID = Convert.ToInt64(reitem["FBASEUNITID"]);
                        var baseqty = Convert.ToDecimal(reitem["FBASEQTY"]);
                        IConvertService convertService = Kingdee.BOS.App.ServiceHelper.GetService<IConvertService>();
                        if (item.SalUnitID != resunitID)
                        {
                            baseqty = convertService.GetUnitTransQty(ctx, item.MaterialId, resunitID, item.SalUnitID, baseqty);
                        }

                        if (delqty - baseqty > 0)
                        {
                            sqty = baseqty;
                            delqty -= baseqty;
                        }
                        else
                        {
                            sqty = delqty;
                            delqty -= sqty;
                        }
                        //获取调出仓库
                        var src_ck = LoadBDFullObject(ctx, "BD_STOCK", Convert.ToInt64(reitem["FSTOCKID"]));
                        var dest_ck = LoadBDFullObject(ctx, "BD_STOCK", item.StockId);
                        if (!Convert.ToBoolean(src_ck["FIsDirStock"]))
                        {
                            throw new Exception($"第{item.Seq}行物料{item.ItemNo},预留不属于直发仓库!");
                        }
                        allocates.Add(new Allocate
                        {
                            SalBillNo = orderno,
                            SalBillSEQ = orderseq,
                            DeliveryNoticeNumber = request.dnEntity.BillNo,
                            DeliveryNoticeSEQ = item.Seq,
                            DeliveryNoticeID = request.dnEntity.FId,
                            DeliveryNoticeEntryID = item.EntryId,
                            FID = reitem["FDEMANDINTERID"].ToString(),
                            TargetOrgId = reitem["FSUPPLYORGID"].ToString(),
                            //FDestMaterialID = src_material,
                            FMaterialId = item.MsterID,
                            FBASEQTY = convertService.GetUnitTransQty(ctx, item.MaterialId, item.SalUnitID, resunitID, sqty),
                            FQTY = sqty,
                            FSrcStock_Id = Convert.ToInt64(src_ck["Id"]),
                            FSrcStockId = src_ck,
                            FDestStock_Id = item.StockId,
                            FDestStockId = dest_ck,
                        });

                    }
                    if (delqty > 0)
                    {
                        throw new Exception($"第{item.Seq}行物料{item.ItemNo},可调拨数量不足!");
                    }
                }
                using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                {
                    //组织间需求单下推分步式调出单
                    if (allocates.Count > 0)
                    {
                        var opresult = SalDeliveryNoticePushAllocate(ctx, allocates, request.DirNo);
                        if (opresult.IsSuccess)
                        {
                        }
                    }
                    //存在其他组织未审核的调出单，需要一起审核
                    var sql = $@"/*dialect*/select distinct t1.FId from T_STK_STKTRANSFEROUT t1 
                        inner join T_STK_STKTRANSFEROUTENTRY t2 on t1.FID=t2.FID
                        where t1.FCANCELSTATUS='A' and FDELIVERYNOTICEID={request.dnEntity.FId} and t1.FDOCUMENTSTATUS='B' ";
                    var datas = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDynamicObject(ctx, sql);
                    MymoooBusinessDataService service = new MymoooBusinessDataService();
                    foreach (var item in datas)
                    {
                        var view = FormMetadataUtils.CreateBillView(ctx, "STK_TRANSFEROUT", item["FId"]);
                        var oper = service.Audit(ctx, view.BusinessInfo, new object[] { Convert.ToString(item["FId"]) });
                        if (!oper.IsSuccess)
                        {
                            if (oper.ValidationErrors.Count > 0)
                            {
                                throw new Exception($"调出单[{view.Model.GetValue("FBILLNO")}]批核失败：" + string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
                            }
                            else
                            {

                                throw new Exception($"调出单[{view.Model.GetValue("FBILLNO")}]批核失败：" + string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                            }
                        }
                        //清除释放网控
                        view.CommitNetworkCtrl();
                        view.InvokeFormOperation(FormOperationEnum.Close);
                        view.Close();
                    }
                    cope.Complete();

                }
                response.Code = ResponseCode.Success;
                response.Message = "操作成功";
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.Exception;
                response.Message = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// 审核调拨入库
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> AuditDeliveryTransferIn(Context ctx, SoDirEntity request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            response.Data = request;
            try
            {
                using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                {
                    //批核全部调入单
                    var sql = $@"/*dialect*/select distinct t1.FId from T_STK_STKTRANSFERIN t1 
                        inner join T_STK_STKTRANSFERINENTRY t2 on t1.FID=t2.FID
                        where t1.FCANCELSTATUS='A' and FDELIVERYNOTICEID={request.dnEntity.FId} and t1.FDOCUMENTSTATUS='B' ";
                    var datas = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDynamicObject(ctx, sql);
                    MymoooBusinessDataService service = new MymoooBusinessDataService();
                    foreach (var item in datas)
                    {
                        var view = FormMetadataUtils.CreateBillView(ctx, "STK_TRANSFERIN", item["FId"]);
                        var oper = service.Audit(ctx, view.BusinessInfo, new object[] { Convert.ToString(item["FId"]) });
                        if (!oper.IsSuccess)
                        {
                            if (oper.ValidationErrors.Count > 0)
                            {
                                throw new Exception($"调入单[{view.Model.GetValue("FBILLNO")}]批核失败：" + string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
                            }
                            else
                            {
                                throw new Exception($"调入单[{view.Model.GetValue("FBILLNO")}]批核失败：" + string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                            }
                        }
                        //清除释放网控
                        view.CommitNetworkCtrl();
                        view.InvokeFormOperation(FormOperationEnum.Close);
                        view.Close();
                    }
                    cope.Complete();

                }
                response.Code = ResponseCode.Success;
                response.Message = "操作成功";
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.Exception;
                response.Message = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// 发货通知单下推销售出库单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> DnPushSalesOutStock(Context ctx, SoDirEntity request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            MymoooBusinessDataService service = new MymoooBusinessDataService();
            response.Data = request;
            //验证是否已经生成销售出库单
            if (IsExistsDirNo(ctx, request.DirNo, 4))
            {
                response.Code = ResponseCode.Success;
                response.Message = "已经存在销售出库单";
                return response;
            }
            try
            {
                //处理数据
                using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                {
                    List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
                    // 下推销售出库单
                    var rules = ConvertServiceHelper.GetConvertRules(ctx, "SAL_DELIVERYNOTICE", "SAL_OUTSTOCK");
                    var rule = rules.FirstOrDefault(t => t.IsDefault);
                    if (rule == null)
                    {
                        throw new Exception("没有从发货通知单到销售出库单的转换关系");
                    }
                    foreach (var item in request.dnEntity.Det)
                    {
                        selectedRows.Add(new ListSelectedRow(request.dnEntity.FId.ToString(), item.EntryId.ToString(), 0, "SAL_DELIVERYNOTICE") { EntryEntityKey = "FEntity" });
                    }
                    //有数据才需要下推
                    if (selectedRows.Count > 0)
                    {
                        PushArgs pushArgs = new PushArgs(rule, selectedRows.ToArray())
                        {
                            TargetBillTypeId = "ad0779a4685a43a08f08d2e42d7bf3e9",     // 请设定目标单据单据类型
                            TargetOrgId = 0,            // 请设定目标单据主业务组织
                                                        //CustomParams = ,     // 可以传递额外附加的参数给单据转换插件
                        };
                        //执行下推操作，并获取下推结果
                        var operationResult = ConvertServiceHelper.Push(ctx, pushArgs, null);
                        if (operationResult.IsSuccess)
                        {
                            var outView = FormMetadataUtils.CreateBillView(ctx, "SAL_OUTSTOCK");
                            foreach (var item in operationResult.TargetDataEntities)
                            {
                                outView.Model.DataObject = item.DataEntity;
                                outView.Model.SetValue("FISDIRECTDELIVERY", "1");
                                outView.Model.SetItemValueByNumber("FCreatorId", request.CreateUserCode, 0);
                                outView.Model.SetValue("FCallbackDate", DateTime.Now);
                                var outEntrys = outView.Model.GetEntityDataObject(outView.BusinessInfo.GetEntity("FEntity"));
                                List<DynamicObject> newRows = new List<DynamicObject>();
                                foreach (var entry in outEntrys)
                                {
                                    var thisList = request.dnEntity.Det.FirstOrDefault(x => x.EntryId == Convert.ToInt64(((entry["FEntity_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SId"]));
                                    var isExists = newRows.FirstOrDefault(x => Convert.ToInt64(((x["FEntity_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SId"]) == Convert.ToInt64(((entry["FEntity_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SId"]));
                                    if (isExists == null)
                                    {
                                        DynamicObject newRowObj = (DynamicObject)entry.Clone(false, true);
                                        newRowObj["RealQty"] = thisList.Qty;
                                        newRows.Add(newRowObj);
                                    }
                                }
                                outEntrys.Clear();
                                foreach (var entry in newRows)
                                {
                                    outEntrys.Add(entry);
                                }
                                foreach (var entry in outEntrys)
                                {
                                    var rowIndex = outEntrys.IndexOf(entry);
                                    outView.InvokeFieldUpdateService("FRealQty", rowIndex);
                                }
                            }
                            //保存批核
                            var opers = service.SaveAndAuditBill(ctx, outView.BusinessInfo, new DynamicObject[] { outView.Model.DataObject }.ToArray());
                            if (opers.IsSuccess)
                            {
                                response.Code = ResponseCode.Success;
                                response.Message = string.Join(";", opers.OperateResult.Select(p => p.Message));
                                //清除释放网控
                                outView.CommitNetworkCtrl();
                                outView.InvokeFormOperation(FormOperationEnum.Close);
                                outView.Close();
                                cope.Complete();
                            }
                            else
                            {
                                if (opers.ValidationErrors.Count > 0)
                                {
                                    throw new Exception(string.Join(";", opers.ValidationErrors.Select(p => p.Message)));
                                }
                                else
                                {
                                    throw new Exception(string.Join(";", opers.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                                }
                            }
                        }
                        else
                        {
                            if (operationResult.ValidationErrors.Count > 0)
                            {
                                throw new Exception(string.Join(";", operationResult.ValidationErrors.Select(p => p.Message)));
                            }
                            else
                            {
                                throw new Exception(string.Join(";", operationResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("订单异常，发货通知单下推销售出库单数据丢失。");
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
        /// 根据ID获取实体
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="formId"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        DynamicObject LoadBDFullObject(Context ctx, string formId, long pkid)
        {
            FormMetadata meta = MetaDataServiceHelper.Load(ctx, formId) as FormMetadata;
            // 构建查询参数，设置过滤条件
            QueryBuilderParemeter queryParam = new QueryBuilderParemeter();
            queryParam.FormId = formId;
            queryParam.BusinessInfo = meta.BusinessInfo;
            queryParam.FilterClauseWihtKey = string.Format(" {0} = '{1}' ", meta.BusinessInfo.GetForm().PkFieldName, pkid);
            var bdObjs = BusinessDataServiceHelper.Load(ctx, meta.BusinessInfo.GetDynamicObjectType(), queryParam);
            return bdObjs[0];
        }

        /// <summary>
        /// 发货通知单下推分步式调拨单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="allocates"></param>
        /// <returns></returns>
        public IOperationResult SalDeliveryNoticePushAllocate(Context ctx, List<Allocate> allocates, string dirNo)
        {
            List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
            StockPushEntity push = new StockPushEntity();
            foreach (var item in allocates)
            {
                var row = new ListSelectedRow(item.FID, string.Empty, 0, "PLN_REQUIREMENTORDER");
                //row.EntryEntityKey = "FEntity"; //这里最容易忘记加，是重点的重点
                selectedRows.Add(row);
            }

            push.listSelectedRow = selectedRows;
            push.ConvertRule = "PLN_REQUIREMENTORDER_2_TRANSOUT";
            var result = BillPush(ctx, push, allocates, dirNo);
            return result;
        }
        public IOperationResult BillPush(Context ctx, StockPushEntity pushEntity, List<Allocate> allocates, string dirNo)
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
                List<DynamicObject> newRows = new List<DynamicObject>();
                var stockorgid = Convert.ToString(targeEntry["StockOrgID_Id"]);
                targeEntry["FDirNo"] = dirNo;
                var rowEntry = targeEntry["STK_STKTRANSFEROUTENTRY"] as DynamicObjectCollection;
                foreach (var rowlin in rowEntry)
                {
                    var materialid = Convert.ToInt64(((DynamicObject)rowlin["MaterialId"])["msterID"]);

                    var srcbillid = "";
                    foreach (var itemlink in rowlin["FSTKTSTKRANSFEROUTENTRY_Link"] as DynamicObjectCollection)
                    {
                        srcbillid = itemlink["SBillId"] as string;
                    }

                    var aot = allocates.Where(x => x.FID == srcbillid).ToList();
                    foreach (var itemlink in aot)
                    {
                        targeEntry["FPENYStockID_Id"] = itemlink.FSrcStockId["Id"];
                        targeEntry["FPENYStockID"] = itemlink.FSrcStockId;
                        //复制出新行
                        DynamicObject newRowObj = (DynamicObject)rowlin.Clone(false, true);
                        //修改赋值
                        newRowObj["FPENYDeliveryNotice"] = itemlink.DeliveryNoticeNumber;
                        newRowObj["FDeliveryNoticeSEQ"] = itemlink.DeliveryNoticeSEQ;
                        newRowObj["FPENYSalOrderNo"] = itemlink.SalBillNo;
                        newRowObj["FPENYSalOrderSEQ"] = itemlink.SalBillSEQ;
                        newRowObj["FDeliveryNoticeID"] = itemlink.DeliveryNoticeID;
                        newRowObj["FDeliveryNoticeENTRYID"] = itemlink.DeliveryNoticeEntryID;

                        newRowObj["FQty"] = itemlink.FQTY;
                        newRowObj["BaseQty"] = itemlink.FBASEQTY;
                        //调出
                        newRowObj["SrcStockID_Id"] = itemlink.FSrcStockId["Id"];
                        newRowObj["SrcStockID"] = itemlink.FSrcStockId;
                        //调入
                        newRowObj["DestStockID_Id"] = itemlink.FDestStockId["Id"];
                        newRowObj["DestStockID"] = itemlink.FDestStockId;
                        newRowObj["SrcStockStatusID_Id"] = 10000;
                        newRowObj["DestStockStatusID_Id"] = 10004;

                        //修改货主
                        //newRowObj["OwnerID_Id"] = stockorgid;
                        //newRowObj["KeeperID_Id"] = stockorgid;

                        newRows.Add(newRowObj);
                    }
                }
                rowEntry.Clear();
                foreach (var item in newRows)
                {
                    rowEntry.Add(item);
                }

                DBServiceHelper.LoadReferenceObject(ctx, rowEntry.ToArray(), rowEntry.DynamicCollectionItemPropertyType, true);
            }

            var targetBInfo = this.GetBusinessInfo(ctx, pushArgs.ConvertRule.TargetFormId, null);
            //对转换结果进行处理
            //1. 直接调用保存接口，对数据进行保存
            return SaveTargetBill(ctx, targetBInfo, targetObjs);
        }
        private IOperationResult SaveSubmitTargetBill(Context ctx, BusinessInfo targetBusinessInfo, DynamicObject[] targetBillObjs)
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
        //获取销售主体的外发仓ID
        private long GetSaleOrgStockId(Context ctx, long soFId)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@SoFId", KDDbType.Int64, soFId) };
            string sql = $@"select top 1 t1.FSTOCKID from t_BD_Stock t1 
                            inner join T_SAL_ORDER t2 on t1.FUseOrgId=t2.FSALEORGID
                            where t2.FID=@SoFId and FISDIRSTOCK=1 and t1.FDOCUMENTSTATUS='C' ";
            return DBUtils.ExecuteScalar<long>(ctx, sql, 0, paramList: pars.ToArray());
        }


        /// <summary>
        /// 是否已经存在直发订单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dirNo">直发审批单号</param>
        /// <param name="type">（0：收料通知单，1：采购入库，2：发货通知单，3：调拨出单，4：出库单）</param>
        /// <returns></returns>
        private bool IsExistsDirNo(Context ctx, string dirNo, int type)
        {
            if (string.IsNullOrEmpty(dirNo))
            {
                return false;
            }
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@DirNo", KDDbType.String, dirNo) };
            string formName = " ";
            switch (type)
            {
                case 0:
                    formName = "T_PUR_Receive";
                    break;
                case 1:
                    formName = "t_STK_InStock";
                    break;
                case 2:
                    formName = "T_SAL_DELIVERYNOTICE";
                    break;
                case 3:
                    formName = "T_STK_STKTRANSFEROUT";
                    break;
                case 4:
                    formName = "T_SAL_OUTSTOCK";
                    break;
            }

            string sql = $@"select count(1) from {formName} where FDirNo=@DirNo ";
            return DBUtils.ExecuteScalar<int>(ctx, sql, 0, paramList: pars.ToArray()) > 0 ? true : false;
        }

        private void DelDirectRes(Context ctx, long materialid, long DirStockId, long saleid)
        {
            string sSql = $@"SELECT t1.FID,t1.FBASEQTY FROM T_PLN_RESERVELINKENTRY t1
            INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID AND t2.FDEMANDFORMID='PLN_REQUIREMENTORDER' AND t1.FSUPPLYFORMID='STK_Inventory'
            WHERE t2.FMATERIALID={materialid} AND t1.FSTOCKID={DirStockId} AND t2.FSRCENTRYID = {saleid}";
            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
            foreach (var item in datas)
            {
                List<DynamicObject> list = new List<DynamicObject>();
                var billView = FormMetadataUtils.CreateBillView(ctx, "PLN_RESERVELINK", item["FID"]);
                var rowcount = billView.Model.GetEntryRowCount("FEntity");
                for (int i = rowcount; i >= 0; i--)
                {
                    var stockid = billView.Model.GetValue<DynamicObject>("FSupplyStockID", i, null);
                    if (stockid == null)
                    {
                        continue;
                    }
                    if (Convert.ToInt64(stockid["Id"]) == DirStockId)
                    {
                        billView.Model.DeleteEntryRow("FEntity", i);
                    }
                }
                list.Add(billView.Model.DataObject);
                if (billView.Model.GetEntryRowCount("FEntity") > 0)
                {
                    SaveBill(ctx, billView.BusinessInfo, list.ToArray());
                }
                else
                {
                    BusinessDataServiceHelper.Delete(ctx, billView.BusinessInfo, new Object[] { item["FID"] }, null, "");
                }

            }
        }
        private bool IsDirectStock(Context ctx, DynamicObject StockId)
        {
            if (StockId == null)
            {
                return false;
            }
            string sSql = $"SELECT FSTOCKID FROM dbo.T_BD_STOCK where FISDIRSTOCK=1 AND FSTOCKID={StockId["Id"]}";
            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
            if (datas.Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void SaveBill(Context ctx, BusinessInfo businessInfo, DynamicObject[] dynamicObjects)
        {
            SaveService saveService = new SaveService();
            var operateOption = OperateOption.Create();
            operateOption.SetIgnoreWarning(true);
            using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
            {
                var oper = saveService.Save(ctx, businessInfo, dynamicObjects, operateOption);
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
                cope.Complete();
            }
        }
        private DynamicObjectCollection LoadStockQty(Context ctx, long _masterid, long _targetOrgID)
        {
            StockQuantityService stockQuantity = new StockQuantityService();
            return stockQuantity.InventoryDirQty(ctx, _masterid, new List<long> { _targetOrgID });
        }

        /// <summary>
        /// 直发修改入库后预留数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> DirectEditReserved(Context ctx, SoDirEntity request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            response.Data = request;
            MymoooBusinessDataService service = new MymoooBusinessDataService();
            try
            {
                //验证是否已经生成发货通知单
                if (IsExistsDirNo(ctx, request.DirNo, 2))
                {
                    response.Code = ResponseCode.Success;
                    response.Message = "已经存在发货通知单，无需修改预留。";
                    return response;
                }
                //处理数据
                using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                {
                    foreach (var item in request.SoDet)
                    {
                        //删除除本次申请直发以外为直发仓的预留1
                        var sallist = from u in request.SoDet
                                      where u.ItemNo == item.ItemNo
                                      select u;
                        string sSql = $"SELECT FMATERIALID FROM dbo.T_BD_MATERIAL WHERE FNUMBER='{item.ItemNo}' AND FUSEORGID={item.SupplyOrgId}";
                        var materialid = DBServiceHelper.ExecuteScalar<long>(ctx, sSql, 0);
                        //DelDirectRes(ctx, materialid, item.DirStockId, item.SoEntryId);
                        decimal SoQty = item.PoDet.Sum(x => x.ReqDirQty);
                        //取当前行下组织间需求单
                        sSql = $@"SELECT t1.FID,t1.FENTRYID,t2.FDEMANDFORMID,t2.FDEMANDINTERID,t2.FDEMANDBILLNO,t2.FDEMANDORGID,
t2.FMATERIALID,t2.FBASEDEMANDUNITID,t2.FBASEDEMANDQTY,t1.FSUPPLYINTERID
FROM dbo.T_PLN_RESERVELINKENTRY t1 INNER JOIN T_PLN_RESERVELINK t2 ON t1.FID=t2.FID
WHERE t2.FDEMANDFORMID='PLN_REQUIREMENTORDER' AND t2.FSRCENTRYID={item.SoEntryId}";
                        var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
                        foreach (var resitem in datas)
                        {
                            decimal plnQty = Convert.ToDecimal(resitem["FBASEDEMANDQTY"]);
                            List<DynamicObject> list = new List<DynamicObject>();
                            sSql = $"SELECT * FROM T_PLN_RESERVELINKENTRY WHERE FID={resitem["FID"]}";
                            var reqdatas = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
                            IBillView billView = null;
                            if (reqdatas.Count() > 0)
                            {
                                billView = FormMetadataUtils.CreateBillView(ctx, "PLN_RESERVELINK", resitem["FID"]);
                            }
                            else
                            {
                                //取组织间需求单信息
                                sSql = $@"SELECT FFORMID AS DemandFormID,fid AS DemandInterID,FBILLNO AS DemandBillNO
                                ,'SAL_SaleOrder' AS SrcDemandFormId,FSALEORDERID AS SrcDemandInterId
                                ,FSALEORDERENTRYID AS SrcDemandEntryId,FSALEORDERNO AS SrcDemandBillNo
                                ,FSUPPLYORGID AS DemandOrgID
                                ,FSUPPLYMATERIALID AS MaterialID
                                ,FUNITID AS BaseUnitID 
                                FROM T_PLN_REQUIREMENTORDER WHERE FID={resitem["FDEMANDINTERID"]}";
                                var reqbilldata = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
                                if (reqbilldata.Count > 0)
                                {
                                    billView = FormMetadataUtils.CreateBillView(ctx, "PLN_RESERVELINK");
                                    billView.Model.SetValue("FID", resitem["FID"]);
                                    billView.Model.SetValue("FDEMANDFORMID", reqbilldata[0]["DemandFormID"]);
                                    billView.Model.SetItemValueByID("FDEMANDINTERID", reqbilldata[0]["DemandInterID"], 0);
                                    billView.Model.SetValue("FDEMANDBILLNO", reqbilldata[0]["DemandBillNO"]);
                                    billView.Model.SetValue("FSRCFORMID", reqbilldata[0]["SrcDemandFormId"]);
                                    billView.Model.SetItemValueByID("FSRCINTERID", reqbilldata[0]["SrcDemandInterId"], 0);
                                    billView.Model.SetItemValueByID("FSRCENTRYID", reqbilldata[0]["SrcDemandEntryId"], 0);
                                    billView.Model.SetValue("FSRCBILLNO", reqbilldata[0]["SrcDemandBillNo"]);
                                    //billView.Model.SetValue("FRESERVETYPE", reqbilldata[0]["DemandFormID"]);
                                    billView.Model.SetItemValueByID("FDEMANDORGID", reqbilldata[0]["DemandOrgID"], 0);
                                    billView.Model.SetItemValueByID("FMATERIALID", reqbilldata[0]["MaterialID"], 0);
                                    billView.Model.SetItemValueByID("FBASEDEMANDUNITID", reqbilldata[0]["BaseUnitID"], 0);
                                    billView.Model.SetValue("FBASEDEMANDQTY", plnQty);
                                    //billView.Model.SetValue("FFORMID", reqbilldata[0]["DemandFormID"]);
                                    //billView.Model.SetValue("FINTDEMANDID", reqbilldata[0]["DemandFormID"]);
                                    //billView.Model.SetValue("FINTSRCID", reqbilldata[0]["DemandFormID"]);
                                    //billView.Model.SetValue("FINTSRCENTRYI", reqbilldata[0]["DemandFormID"]);
                                }
                            }

                            var rowcount = billView.Model.GetEntryRowCount("FEntity");
                            //原来直发仓库预留信息
                            bool isDir = true;
                            for (int i = rowcount; i >= 0; i--)
                            {
                                if (IsDirectStock(ctx, billView.Model.GetValue<DynamicObject>("FSupplyStockID", i, null)))
                                {
                                    //billView.Model.DeleteEntryRow("FEntity", i);
                                    //已经存在直发仓的预留则不修改当前预留信息
                                    isDir = false; break;
                                }
                            }
                            if (isDir)
                            {
                                //根据直发申请修改采购在途数量
                                foreach (var podet in item.PoDet)
                                {
                                    for (int i = 0; i < billView.Model.GetEntryRowCount("FEntity"); i++)
                                    {
                                        var eformid = billView.Model.GetValue<string>("FSupplyFormID", i, string.Empty);
                                        var entryid = billView.Model.GetValue<long>("FSUPPLYENTRYID", i, 0);
                                        if (eformid.EqualsIgnoreCase("PUR_PurchaseOrder") && entryid == podet.PoEntryId)
                                        {
                                            var baseqty = billView.Model.GetValue<decimal>("FBaseSupplyQty", i, 0);
                                            billView.Model.SetValue("FBASESUPPLYQTY", baseqty - podet.ReqDirQty, i);
                                        }
                                    }
                                }

                                //重新绑定新的库存预留
                                var liststock = LoadStockQty(ctx, materialid, item.SupplyOrgId);
                                var stklist = liststock.Where(x => Convert.ToInt64(x["FSTOCKID"]) == item.DirStockId).First();
                                decimal sendQty = 0;
                                if (SoQty - plnQty <= 0)
                                {
                                    sendQty = SoQty;
                                }
                                else
                                {
                                    sendQty = plnQty;
                                }
                                billView.Model.CreateNewEntryRow("FEntity");
                                var entitycount = billView.Model.GetEntryRowCount("FEntity") - 1;
                                billView.Model.SetItemValueByNumber("FSupplyFormID", "STK_Inventory", entitycount);
                                billView.Model.SetValue("FSupplyInterID", Convert.ToString(stklist["FID"]), entitycount);
                                billView.Model.SetItemValueByID("FSupplyOrgId", Convert.ToInt64(stklist["FSTOCKORGID"]), entitycount);
                                //billView.Model.SetValue("FSupplyBillNO", Convert.ToString(stockitem["SupplyBillNO"]), rowcount);
                                billView.Model.SetItemValueByID("FSupplyMaterialID", Convert.ToInt64(stklist["FMATERIALID"]), entitycount);
                                billView.Model.SetItemValueByID("FBaseSupplyUnitID", Convert.ToInt64(stklist["FBASEUNITID"]), entitycount);
                                billView.Model.SetValue("FBaseSupplyQty", sendQty, entitycount);
                                //billView.Model.SetValue("FQty", Convert.ToString(stockitem["BaseSupplyQty"]), rowcount);
                                billView.Model.SetValue("FStockQty", Convert.ToString(stklist["FBASEQTY"]), entitycount);
                                billView.Model.SetValue("FSupplyDate", Convert.ToString(System.DateTime.Now), entitycount);
                                billView.Model.SetItemValueByID("FSupplyStockID", Convert.ToInt64(stklist["FSTOCKID"]), entitycount);

                                list.Add(billView.Model.DataObject);
                                SaveBill(ctx, billView.BusinessInfo, list.ToArray());
                            }
                        }

                    }
                    response.Code = ResponseCode.Success;
                    response.Message = "预留修改成功";
                    cope.Complete();
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
        /// 设置采购订单直发预留数量
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> SetPoDirReServeQty(Context ctx, SoDirEntity request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                if (request.SoDet.Count() == 0)
                {
                    response.Code = ResponseCode.ModelError;
                    response.Message = "请传销售订单相关信息";
                    return response;
                }

                //发起审批增加预留数量
                if (request.IsAddReServeQty)
                {
                    List<PoDirQtyDet> polist = new List<PoDirQtyDet>();
                    request.SoDet.Select(x => x.PoDet).ToList().ForEach(x =>
                    {
                        polist.AddRange(x);
                    });
                    var dirDet = polist.GroupBy(it => it.PoEntryId).Select(its => new
                    {
                        PoEntryId = its.Key,
                        ReqDirQty = its.Sum(it => it.ReqDirQty)
                    }).ToList();
                    var sql = $@"/*dialect*/select top 1 FENTRYID from t_PUR_POOrderDirReServeQty where FENTRYID=@FENTRYID and FDirNo = @FDirNo ";
                    var poEntryId = DBUtils.ExecuteScalar<long>(ctx, sql, 0, new SqlParam("@FENTRYID", KDDbType.Int64, dirDet[0].PoEntryId), new SqlParam("@FDirNo", KDDbType.String, request.DirNo));
                    if (poEntryId == 0)
                    {
                        List<SqlObject> list = new List<SqlObject>();
                        foreach (var item in dirDet)
                        {
                            list.Add(new SqlObject(@"/*dialect*/INSERT INTO [dbo].[t_PUR_POOrderDirReServeQty]
                                                                       ([FDirNo],[FEntryId],[FDirReServeQty]
                                                                       ,[CreateUserName],[CreateDate])
                                                                 VALUES
                                                                       (@FDirNo,@FEntryId,@FDirReServeQty,@CreateUserName,getdate())",
                           new List<SqlParam>(){
                             new SqlParam("@FDirReServeQty", KDDbType.Decimal, item.ReqDirQty)
                            ,new SqlParam("@FDirNo", KDDbType.String, request.DirNo)
                            ,new SqlParam("@FENTRYID", KDDbType.Int64, item.PoEntryId)
                            ,new SqlParam("@CreateUserName", KDDbType.String, request.CreateUserName)}));
                        }
                        DBUtils.ExecuteBatch(ctx, list);
                    }
                    response.Code = ResponseCode.Success;
                }
                else
                {
                    //驳回或者撤销，减少预留数量
                    string sql = $@"/*dialect*/delete from t_PUR_POOrderDirReServeQty where FDirNo = @FDirNo";
                    DBUtils.Execute(ctx, sql, new SqlParam("@FDirNo", KDDbType.String, request.DirNo));
                    response.Code = ResponseCode.Abort;
                }
                response.Message = "设置成功";
                return response;
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.ModelError;
                response.Message = "设置失败：" + ex.Message;
                return response;
            }
        }
    }
}
