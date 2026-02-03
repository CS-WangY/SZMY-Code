using Kingdee.BOS;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Const;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Orm;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Contracts.PurchaseManagement;
using Kingdee.Mymooo.Contracts.SalesManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.StockManagement;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.BOS.Core.Enums;
using Kingdee.BOS.Core.Metadata.Operation;
using Kingdee.BOS.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.App.Core.SalesManagement;
using Kingdee.BOS.Core.Util;
using Kingdee.Mymooo.Core.ProductionManagement;

namespace Kingdee.Mymooo.App.Core.PurchaseManagement
{
    /// <summary>
    /// 采购订单服务
    /// </summary>
    public class PurchaseOrderService : IPurchaseOrderService
    {
        /// <summary>
        /// 采购申请单业务终止
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> StatusPrOrderAction(Context ctx, PrStatus status)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            var operateOption = OperateOption.Create();
            operateOption.SetIgnoreWarning(true);
            FormMetadata meta = MetaDataServiceHelper.Load(ctx, "PUR_Requisition") as FormMetadata;
            object[] ids = status.EntryId.ToArray<object>();
            List<KeyValuePair<object, object>> pkEntryIds = ids.Select((object x) => new KeyValuePair<object, object>(status.PrId, x)).ToList();
            //object[] ids = new object[] { mostatus.MoEntryId };

            SetStatusService setStatusService = new SetStatusService();
            using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
            {
                IOperationResult oper2 = new OperationResult();
                //业务终止
                oper2 = setStatusService.SetBillStatus(ctx, meta.BusinessInfo, pkEntryIds, null, "Terminate", operateOption);
                if (oper2.ValidationErrors.Count > 0)
                {
                    response.Message = string.Join(";", oper2.ValidationErrors.Select(p => p.Message));
                    return response;
                }

                response.Code = ResponseCode.Success;
                response.Message = "操作成功";
                cope.Complete();
                return response;
            }
        }
        /// <summary>
        /// 获取产品采购记录
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> GetPurchaseOrderSimpleAction(Context ctx, List<string> productModels)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            var operateOption = OperateOption.Create();
            operateOption.SetIgnoreWarning(true);
            var dt = new DataTable();
            var column = new DataColumn();
            dt.TableName = "#ProductModelsTemp";
            dt.Columns.Add("ITEM_NO", typeof(string));
            dt.BeginLoadData();
            foreach (var item in productModels)
            {
                dt.LoadDataRow(new object[] { item }, true);
            }
            dt.EndLoadData();
            List<PurchaseOrderSimpleEntity> list = new List<PurchaseOrderSimpleEntity>();
            response.Code = ResponseCode.Success;
            response.Message = "获取成功";
            using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
            {
                DBUtils.Execute(ctx, "/*dialect*/ IF OBJECT_ID('tempdb..#ProductModelsTemp') IS NOT NULL DROP TABLE #ProductModelsTemp ");
                DBUtils.CreateSessionTemplateTable(ctx, "ProductModelsTemp", "(ITEM_NO nvarchar(100)) ");
                DBUtils.BulkInserts(ctx, string.Empty, string.Empty, dt);
                var sql = $@"/*dialect*/select m.FBILLNO PO_NO,m.FDATE PO_DATE,ven.FNUMBER VDR_CODE,ven_l.FNAME VDR_NAMEC,ma.FNUMBER ITEM_NO,d.FQTY QTY,f.FTAXPRICE VAT_PRICE
                ,DATEDIFF(DAY,m.FDATE,dd.FDELIVERYDATE) AS DeliveryDay
                from t_PUR_POOrderEntry d
                inner join t_PUR_POOrder m on m.FID=d.FID
                inner join T_PUR_POORDERENTRY_F f on f.FENTRYID=d.FENTRYID
                inner join T_PUR_POORDERENTRY_D dd on dd.FENTRYID=d.FENTRYID
                left join  t_BD_Supplier ven on ven.FSUPPLIERID=m.FSUPPLIERID
                left join  T_BD_SUPPLIER_L ven_l on ven.FSUPPLIERID=ven_l.FSUPPLIERID
                left join  T_BD_MATERIAL ma on d.FMATERIALID=ma.FMATERIALID
                inner join #ProductModelsTemp temp on ma.FNUMBER = temp.ITEM_NO
                where  m.FDOCUMENTSTATUS='C' and m.FDATE>CONVERT(varchar(12),DATEADD(year,-1,getdate()),23)
                order by ma.FNUMBER,m.FDATE desc";
                var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
                foreach (var item in datas)
                {
                    var purchaseOrder = new PurchaseOrderSimpleEntity();

                    purchaseOrder.ProductModel = item["ITEM_NO"].ToString();
                    if (string.IsNullOrEmpty(purchaseOrder.ProductModel))
                    {
                        continue;
                    }
                    purchaseOrder.SupplierName = item["VDR_NAMEC"].ToString();
                    purchaseOrder.PurchaseOrderNumber = item["PO_NO"].ToString();
                    purchaseOrder.ApplyTime = Convert.ToDateTime(item["PO_DATE"]).ToString("yyyy-MM-dd");
                    purchaseOrder.SupplierCode = item["VDR_CODE"].ToString();
                    purchaseOrder.Qty = Convert.ToDecimal(item["QTY"].ToString());
                    purchaseOrder.Price = Convert.ToDecimal(item["VAT_PRICE"].ToString());
                    purchaseOrder.DeliveryDay = Convert.ToInt32(item["DeliveryDay"]);
                    list.Add(purchaseOrder);
                }
                cope.Complete();
            }
            response.Data = list;
            return response;
        }

        /// <summary>
        /// 采购订单下推生成应付单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="TargetOrgId"></param>
        /// <returns></returns>
        public IOperationResult PurchaseToReceivable(Context ctx, List<PurchaseOrderPushEntity> entry, long TargetOrgId)
        {
            List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
            StockPushEntity push = new StockPushEntity();
            foreach (var item in entry)
            {
                var row = new ListSelectedRow(item.FID.ToString(), item.FEntryID.ToString(), 0, "PUR_PurchaseOrder");
                row.EntryEntityKey = "FPOOrderEntry"; //这里最容易忘记加，是重点的重点
                selectedRows.Add(row);
            }

            push.listSelectedRow = selectedRows;
            push.ConvertRule = "AP_PurOrderToPayable";
            push.TargetOrgId = TargetOrgId;
            push.TargetBillTypeId = "a83c007f22414b399b0ee9b9aafc75f9";

            return this.BillPush(ctx, push);
        }
        public IOperationResult BillPush(Context ctx, StockPushEntity pushEntity)
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
            var targetBInfo = this.GetBusinessInfo(ctx, pushArgs.ConvertRule.TargetFormId, null);
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

        /// <summary>
        /// 费用采购订单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> CostPurchaseOrderService(Context ctx, CostPurchaseOrderRequest request)
        {

            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            //如果已经生成费用采购订单，则跳过
            string sql = $@"select top 1 FBILLNO from T_PUR_POORDER where FBILLNO='{request.BillNo}' ";
            if (!string.IsNullOrWhiteSpace(DBServiceHelper.ExecuteScalar<string>(ctx, sql, "")))
            {
                response.Code = ResponseCode.Success;
                response.Message = "已生成费用采购订单";
                return response;
            }
            MymoooBusinessDataService service = new MymoooBusinessDataService();
            try
            {
                using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                {
                    var billView = FormMetadataUtils.CreateBillView(ctx, "PUR_PurchaseOrder");
                    billView.Model.SetValue("FBillTypeID", "b1985f24f35841fdb418329af6ed7bd0");
                    billView.InvokeFieldUpdateService("FBillTypeID", 0);
                    billView.Model.SetValue("FBillNo", request.BillNo);
                    billView.Model.SetValue("FDate", request.Date);
                    billView.Model.SetItemValueByNumber("FPurchaseOrgId", request.PurchaseOrgCode, 0);
                    billView.InvokeFieldUpdateService("FPurchaseOrgId", 0);
                    billView.Model.SetItemValueByNumber("FSupplierId", request.SupplierCode, 0);
                    billView.InvokeFieldUpdateService("FSupplierId", 0);
                    billView.Model.SetValue("FNote", request.Note);
                    billView.Model.SetItemValueByNumber("FPurchaserId", "WangDongMei", 0);
                    billView.InvokeFieldUpdateService("FPurchaserId", 0);
                    int rowIndex = 0;
                    foreach (var det in request.Details)
                    {
                        if (rowIndex > 0)
                        {
                            billView.Model.CreateNewEntryRow("FPOOrderEntry");
                        }
						billView.Model.SetValue("FMesEntryId", det.MesEntryId, rowIndex);
						billView.Model.SetItemValueByNumber("FMaterialId", det.MaterialCode, rowIndex);
                        billView.InvokeFieldUpdateService("FMaterialId", rowIndex);
                        billView.Model.SetValue("FTaxPrice", det.TaxPrice, rowIndex);
                        billView.InvokeFieldUpdateService("FTaxPrice", rowIndex);
                        billView.Model.SetValue("FQty", det.Qty, rowIndex);
                        billView.InvokeFieldUpdateService("FQty", rowIndex);
                        billView.Model.SetItemValueByNumber("FChargeProjectID", det.ChargeProjectCode, rowIndex);
                        billView.InvokeFieldUpdateService("FChargeProjectID", rowIndex);
                        billView.Model.SetValue("FDeliveryDate", det.DeliveryDate, rowIndex);
                        billView.Model.SetValue("FMoNumber", det.MoNumber, rowIndex);
                        billView.Model.SetValue("FMoEntrySeq", det.MoEntrySeq, rowIndex);
                        billView.Model.SetItemValueByNumber("FWorkShopCode", det.WorkShopCode, rowIndex);
                        billView.Model.SetItemValueByNumber("FCostWorkShopCode", det.CostWorkShopCode, rowIndex);
                        billView.Model.SetValue("FEntryNote", det.Note, rowIndex);
                        rowIndex++;
                    }
                    var oper = service.SaveAndAuditBill(ctx, billView.BusinessInfo, new DynamicObject[] { billView.Model.DataObject }.ToArray());
                    if (!oper.IsSuccess)
                    {
                        if (oper.ValidationErrors.Count > 0)
                        {
                            response.Message = string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                            response.Code = ResponseCode.Exception;
                        }
                        else
                        {
                            response.Message = string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                            response.Code = ResponseCode.Exception;
                        }
                    }
                    else
                    {
                        response.Message = string.Join(";", oper.OperateResult.Select(p => p.Message));
                        response.Code = ResponseCode.Success;
                        billView.CommitNetworkCtrl();
                        billView.InvokeFormOperation(FormOperationEnum.Close);
                        billView.Close();
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
        /// MES收料下推采购入库
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> MesReceiveGenerateInStockService(Context ctx, MesReceiveInStockRequest request)
        {

            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            //如果已经生成采购入库，则跳过
            string sql = $@"select top 1 FBILLNO from T_STK_INSTOCK where FBILLNO='{request.InStockBillNo}' ";
            if (!string.IsNullOrWhiteSpace(DBServiceHelper.ExecuteScalar<string>(ctx, sql, "")))
            {
                response.Code = ResponseCode.Success;
                response.Message = "已生成采购入库单";
                return response;
            }
            foreach (var item in request.Details)
            {
                if (item.ActReceiveQty < item.RealQty)
                {
                    response.Code = ResponseCode.ModelError;
                    response.Message = "实收数量不能大于交货数量";
                    return response;
                }
            }
            MymoooBusinessDataService service = new MymoooBusinessDataService();
            //处理数据
            try
            {
                using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                {
                    //采购收料
                    var view = FormMetadataUtils.CreateBillView(ctx, "PUR_ReceiveBill", request.Id);
                    var entrys = view.Model.GetEntityDataObject(view.BusinessInfo.GetEntity("FDetailEntity"));
                    //修改交货数量
                    foreach (var item in request.Details.Where(x => x.ActReceiveQty > x.RealQty && x.RealQty > 0))
                    {
                        var entry = entrys.FirstOrDefault(x => item.EntryId == Convert.ToInt64(x["Id"]));
                        if (entry != null)
                        {
                            var rowIndex = entrys.IndexOf(entry);
                            view.Model.SetValue("FActReceiveQty", item.RealQty, rowIndex);
                            view.InvokeFieldUpdateService("FActReceiveQty", rowIndex);
                        }
                    }
                    //删除行
                    foreach (var entry in entrys.OrderByDescending(x => Convert.ToInt64(x["Id"])))
                    {
                        var rowIndex = entrys.IndexOf(entry);
                        var itemDet = request.Details.FirstOrDefault(x => x.EntryId == Convert.ToInt64(entry["Id"]));
                        if (itemDet.RealQty == 0)
                        {
                            view.Model.DeleteEntryRow("FDetailEntity", rowIndex);
                        }
                    }
                    //判断使用删除还是保存
                    if (view.Model.GetEntryRowCount("FDetailEntity") > 0)
                    {
                        var oper = service.SaveBill(ctx, view.BusinessInfo, new DynamicObject[] { view.Model.DataObject });
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
                            response.Code = ResponseCode.Success;
                            response.Message += string.Join(";", oper.OperateResult.Select(p => p.Message));
                        }
                    }
                    else
                    {
                        if (view.Model.GetValue("FDOCUMENTSTATUS").Equals("B") || view.Model.GetValue("FDOCUMENTSTATUS").Equals("C"))
                        {
                            var oper = service.SetBillStatus(ctx, view.BusinessInfo, new object[] { request.Id }, "UnAudit");
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
                                response.Code = ResponseCode.Success;
                                response.Message += string.Join(";", oper.OperateResult.Select(p => p.Message));
                            }
                        }
                        var oper2 = BusinessDataServiceHelper.Delete(ctx, view.BusinessInfo, new Object[] { request.Id }, null, "");
                        if (!oper2.IsSuccess)
                        {
                            if (oper2.ValidationErrors.Count > 0)
                            {
                                throw new Exception(string.Join(";", oper2.ValidationErrors.Select(p => p.Message)));
                            }
                            else
                            {
                                throw new Exception(string.Join(";", oper2.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                            }
                        }
                        else
                        {
                            response.Code = ResponseCode.Success;
                            response.Message += string.Join(";", oper2.OperateResult.Select(p => p.Message));
                        }
                    }

                    //清除释放网控
                    view.CommitNetworkCtrl();
                    view.InvokeFormOperation(FormOperationEnum.Close);
                    view.Close();
                    // 下推采购入库
                    List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
                    var rules = ConvertServiceHelper.GetConvertRules(ctx, "PUR_ReceiveBill", "STK_InStock");
                    var rule = rules.FirstOrDefault(t => t.IsDefault);
                    if (rule == null)
                    {
                        throw new Exception("没有从收料通知单到销售入库单的转换关系");
                    }
                    foreach (var item in request.Details)
                    {
                        if (item.RealQty > 0)
                        {
                            selectedRows.Add(new ListSelectedRow(request.Id.ToString(), item.EntryId.ToString(), 0, "PUR_ReceiveBill") { EntryEntityKey = "FDetailEntity" });
                        }
                    }
                    //有数据才需要下推
                    if (selectedRows.Count > 0)
                    {
                        PushArgs pushArgs = new PushArgs(rule, selectedRows.ToArray())
                        {
                            TargetBillTypeId = "a1ff32276cd9469dad3bf2494366fa4f",     // 请设定目标单据单据类型
                            TargetOrgId = 0,            // 请设定目标单据主业务组织
                                                        //CustomParams = ,     // 可以传递额外附加的参数给单据转换插件
                        };
                        //执行下推操作，并获取下推结果
                        var operationResult = ConvertServiceHelper.Push(ctx, pushArgs, null);
                        if (operationResult.IsSuccess)
                        {
                            var inView = FormMetadataUtils.CreateBillView(ctx, "STK_InStock");
                            foreach (var item in operationResult.TargetDataEntities)
                            {
                                inView.Model.DataObject = item.DataEntity;
                                inView.Model.SetValue("FBillNo", request.InStockBillNo);
                                var inEntrys = inView.Model.GetEntityDataObject(inView.BusinessInfo.GetEntity("FInStockEntry"));
                                List<DynamicObject> newRows = new List<DynamicObject>();
                                foreach (var entry in inEntrys)
                                {
                                    var thisList = request.Details.FirstOrDefault(x => x.EntryId == Convert.ToInt64(((entry["FInStockEntry_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SId"]));
                                    if (thisList != null)
                                    {
                                        var rowIndex = inEntrys.IndexOf(entry);
                                        inView.Model.SetValue("FRealQty", thisList.RealQty, rowIndex);
                                        inView.InvokeFieldUpdateService("FRealQty", rowIndex);
                                        inView.Model.SetItemValueByNumber("FStockId", thisList.StockCode, rowIndex);
                                        inView.InvokeFieldUpdateService("FStockId", rowIndex);
                                    }
                                }

                            }
                            //保存批核
                            var opers = service.SaveAndAuditBill(ctx, inView.BusinessInfo, new DynamicObject[] { inView.Model.DataObject }.ToArray());
                            if (opers.IsSuccess)
                            {
                                response.Code = ResponseCode.Success;
                                response.Message += string.Join(";", opers.OperateResult.Select(p => p.Message));
                                //清除释放网控
                                inView.CommitNetworkCtrl();
                                inView.InvokeFormOperation(FormOperationEnum.Close);
                                inView.Close();
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

                    }
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
        /// MES退料申请下推采购退料
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> MesMrAppGenerateMrbService(Context ctx, MesMrAppToMrbRequest request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            string sql = $@"select top 1 FBILLNO from T_PUR_MRB where FBILLNO='{request.MrbBillNo}' ";
            if (!string.IsNullOrWhiteSpace(DBServiceHelper.ExecuteScalar<string>(ctx, sql, "")))
            {
                response.Code = ResponseCode.Success;
                response.Message = "已生成采购退料单";
                return response;
            }

            MymoooBusinessDataService service = new MymoooBusinessDataService();
            //处理数据
            try
            {
                using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                {
                    // 下推
                    List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
                    var rules = ConvertServiceHelper.GetConvertRules(ctx, "PUR_MRAPP", "PUR_MRB");
                    var rule = rules.FirstOrDefault(t => t.IsDefault);
                    if (rule == null)
                    {
                        throw new Exception("没有从退料申请到采购退料的转换关系");
                    }
                    foreach (var item in request.Details)
                    {
                        selectedRows.Add(new ListSelectedRow(request.Id.ToString(), item.EntryId.ToString(), 0, "PUR_MRAPP") { EntryEntityKey = "FDetailEntity" });
                    }
                    //有数据才需要下推
                    if (selectedRows.Count > 0)
                    {
                        PushArgs pushArgs = new PushArgs(rule, selectedRows.ToArray())
                        {
                            TargetBillTypeId = "583ed26e77664a9d8346e78aa917ce08",     // 请设定目标单据单据类型
                            TargetOrgId = 0,            // 请设定目标单据主业务组织
                                                        //CustomParams = ,     // 可以传递额外附加的参数给单据转换插件
                        };
                        //执行下推操作，并获取下推结果
                        var operationResult = ConvertServiceHelper.Push(ctx, pushArgs, null);
                        if (operationResult.IsSuccess)
                        {
                            var view = FormMetadataUtils.CreateBillView(ctx, "PUR_MRB");
                            foreach (var item in operationResult.TargetDataEntities)
                            {
                                view.Model.DataObject = item.DataEntity;
                                view.Model.SetValue("FBillNo", request.MrbBillNo);
                                var entrys = view.Model.GetEntityDataObject(view.BusinessInfo.GetEntity("FPURMRBENTRY"));
                                foreach (var entry in entrys)
                                {
                                    var thisList = request.Details.FirstOrDefault(x => x.EntryId == Convert.ToInt64(((entry["FPURMRBENTRY_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SId"]));
                                    if (thisList != null)
                                    {
                                        var rowIndex = entrys.IndexOf(entry);
                                        view.Model.SetValue("FRMREALQTY", thisList.RmrealQty, rowIndex);
                                        view.InvokeFieldUpdateService("FRMREALQTY", rowIndex);
                                        view.Model.SetItemValueByNumber("FStockId", thisList.StockCode, rowIndex);
                                        view.InvokeFieldUpdateService("FStockId", rowIndex);
                                    }
                                }

                            }
                            //保存批核
                            var opers = service.SaveAndAuditBill(ctx, view.BusinessInfo, new DynamicObject[] { view.Model.DataObject }.ToArray());
                            if (opers.IsSuccess)
                            {
                                response.Code = ResponseCode.Success;
                                response.Message += string.Join(";", opers.OperateResult.Select(p => p.Message));
                                //清除释放网控
                                view.CommitNetworkCtrl();
                                view.InvokeFormOperation(FormOperationEnum.Close);
                                view.Close();
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

                    }
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

        //public ENGBomInfo TryGetOrAdd(Context ctx, ENGBomInfo bom)
        //{
        //    FormMetadata meta = MetaDataServiceHelper.Load(ctx, "ENG_BOM") as FormMetadata;
        //    var billView = FormMetadataUtils.CreateBillView(ctx, "ENG_BOM");
        //    billView.Model.SetValue("FCreateOrgId", 1);
        //    billView.Model.SetValue("FNAME", bom.FMATERIALID, 0);
        //    billView.Model.SetItemValueByNumber("FMATERIALID", bom.FMATERIALID, 0);
        //    //MES项目专用
        //    if (!string.IsNullOrWhiteSpace(bom.FNUMBER))
        //    {
        //        billView.Model.SetValue("FNUMBER", bom.FNUMBER, 0);
        //    }

        //    billView.Model.DeleteEntryData("FTreeEntity");
        //    var rowcount = 0;
        //    foreach (var item in bom.Entity)
        //    {
        //        billView.Model.CreateNewEntryRow("FTreeEntity");
        //        billView.Model.SetItemValueByNumber("FMATERIALIDCHILD", item.FMATERIALIDCHILD, rowcount);
        //        billView.Model.SetValue("FNUMERATOR", item.FNUMERATOR, rowcount);
        //        rowcount++;
        //    }

        //    SaveService saveService = new SaveService();
        //    var operateOption = OperateOption.Create();
        //    operateOption.SetIgnoreWarning(true);
        //    using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
        //    {

        //        var oper = saveService.Save(ctx, billView.BusinessInfo, new DynamicObject[] { billView.Model.DataObject }, operateOption);
        //        if (!oper.IsSuccess)
        //        {
        //            if (oper.ValidationErrors.Count > 0)
        //            {
        //                throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
        //            }
        //            else
        //            {
        //                throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
        //            }
        //        }
        //        cope.Complete();
        //    }
        //    return bom;
        //}
    }
}
