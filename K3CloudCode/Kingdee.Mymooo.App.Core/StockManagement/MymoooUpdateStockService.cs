using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.BusinessService;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.App.Core;
using Kingdee.K3.SCM.App.Core.AppBusinessService;
using Kingdee.Mymooo.App.Core.KafkaProducer;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.App.Core.StockManagement
{
    public class MymoooUpdateStockService : UpdateStockService
    {
        private UpdateStockBusinessServiceMeta _serviceConfig;
        private readonly Dictionary<long, string> _upstockSetting = new Dictionary<long, string>();
        private BusinessInfo _businessInfo;
        private string _operationNumber;
        private string _formId;
        private BaseDataField _materialField;
        private UpdateStockConfig _updateStockConfig;
        private static readonly Dictionary<string, UpdateStockConfig> _updateStockConfigDict;
        static MymoooUpdateStockService()
        {
            _updateStockConfigDict = new Dictionary<string, UpdateStockConfig>(StringComparer.OrdinalIgnoreCase)
            {
                ["STK_MISCELLANEOUS-FEntity"] = new UpdateStockConfig() { SupplierField = "FSUPPLIERID", QtyField = "FQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["STK_MisDelivery-FEntity"] = new UpdateStockConfig() { QtyField = "FQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["SAL_OUTSTOCK-FEntity"] = new UpdateStockConfig() { CustomerField = "FCustomerID", OrderNoField = "FSoorDerno", OrderEntryField = "FSOEntryId", QtyField = "FRealQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["SAL_RETURNSTOCK-FEntity"] = new UpdateStockConfig() { CustomerField = "FRetcustId", OrderNoField = "FOrderNo", OrderEntryField = "FSOEntryId", QtyField = "FRealQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["STK_TRANSFEROUT-FSTKTRSOUTENTRY"] = new UpdateStockConfig() { QtyField = "FQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["STK_TRANSFERIN-FSTKTRSINENTRY"] = new UpdateStockConfig() { QtyField = "FQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["STK_AssembledApp-FEntity"] = new UpdateStockConfig() { QtyField = "FQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["STK_AssembledApp-FSubEntity"] = new UpdateStockConfig() { QtyField = "FQtySETY", UnitField = "FUnitIDSETY", SmallField = "FSMALLIDSETY", ParentSmallField = "FPARENTSMALLIDSETY" },
                ["STK_StockCountGain-FBillEntry"] = new UpdateStockConfig() { QtyField = "FGainQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["STK_StockCountLoss-FBillEntry"] = new UpdateStockConfig() { QtyField = "FLossQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["STK_TransferDirect-FBillEntry"] = new UpdateStockConfig() { QtyField = "FQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["STK_StockConvert-FEntity"] = new UpdateStockConfig() { QtyField = "FConvertQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["STK_StatusConvert-FEntity"] = new UpdateStockConfig() { QtyField = "FConvertQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["STK_InStock-FInStockEntry"] = new UpdateStockConfig() { OrderNoField = "FPOOrderNo", OrderEntryField = "FPOORDERENTRYID", SupplierField = "FSUPPLIERID", QtyField = "FRealQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["PUR_MRB-FPURMRBENTRY"] = new UpdateStockConfig() { OrderNoField = "FORDERNO", OrderEntryField = "FPOORDERENTRYID", SupplierField = "FSUPPLIERID", QtyField = "FRMREALQTY", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["SUB_PickMtrl-FEntity"] = new UpdateStockConfig() { SupplierField = "FSupplierId", QtyField = "FActualQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["SUB_FEEDMTRL-FEntity"] = new UpdateStockConfig() { SupplierField = "FSubSupplierId", QtyField = "FActualQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["SUB_RETURNMTRL-FEntity"] = new UpdateStockConfig() { SupplierField = "FSubSupplierId", QtyField = "FQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["PRD_PickMtrl-FEntity"] = new UpdateStockConfig() { QtyField = "FActualQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["PRD_RetStock-FEntity"] = new UpdateStockConfig() { QtyField = "FRealQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["PRD_INSTOCK-FEntity"] = new UpdateStockConfig() { QtyField = "FRealQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["PRD_ReturnMtrl-FEntity"] = new UpdateStockConfig() { QtyField = "FQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["PRD_FeedMtrl-FEntity"] = new UpdateStockConfig() { QtyField = "FActualQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["SP_ReturnMtrl-FEntity"] = new UpdateStockConfig() { QtyField = "FQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["SP_PickMtrl-FEntity"] = new UpdateStockConfig() { QtyField = "FActualQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["SP_InStock-FEntity"] = new UpdateStockConfig() { QtyField = "FRealQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" },
                ["SP_OUTSTOCK-FEntity"] = new UpdateStockConfig() { QtyField = "FOutQty", UnitField = "FUnitID", SmallField = "FSmallId", ParentSmallField = "FParentSmallId" }
            };
        }

        public override void PreparePropertys(List<string> fieldKeys)
        {
            base.PreparePropertys(fieldKeys);
            fieldKeys.Add("FQty");
            fieldKeys.Add("FRealQty");
            fieldKeys.Add("FUnitId");
            fieldKeys.Add("FUnitIDSETY");
            fieldKeys.Add("FSUPPLIERID");
            fieldKeys.Add("FSoorDerno");
            fieldKeys.Add("FSOEntryId");
            fieldKeys.Add("FOrderNo");
            fieldKeys.Add("FRMREALQTY");
            fieldKeys.Add("FQtySETY");
            fieldKeys.Add("FGainQty");
            fieldKeys.Add("FLossQty");
            fieldKeys.Add("FConvertQty");
            fieldKeys.Add("FPOOrderNo");
            fieldKeys.Add("FPOORDERENTRYID");
            fieldKeys.Add("FRMREALQTY");
            fieldKeys.Add("FParentSmallId");
            fieldKeys.Add("FSmallId");
            fieldKeys.Add("FSMALLIDSETY");
            fieldKeys.Add("FPARENTSMALLIDSETY");
            fieldKeys.Add("FActualQty");
            fieldKeys.Add("FOutQty");
            fieldKeys.Add("FSupplyTargetOrgId");
            fieldKeys.Add("FTrackingNumber");
            fieldKeys.Add("FTrackingName");
            fieldKeys.Add("FTrackingDate");
            fieldKeys.Add("FTrackingUser");
        }

        private bool IsUpdatePoint(string updateStockPoint, string billStatus)
        {
            switch (_operationNumber.ToLower())
            {
                case "save":
                    bool reslut = updateStockPoint.Equals("1");
                    if (!(billStatus == "A") && !(billStatus == "B"))
                    {
                        reslut = reslut && (billStatus == "D");
                    }
                    return reslut;
                case "delete":
                    reslut = updateStockPoint.Equals("1");
                    if (!(billStatus == "A"))
                    {
                        reslut = reslut && (billStatus == "D");
                    }
                    return reslut;
                case "audit":
                case "unaudit":
                    return updateStockPoint.Equals("2");
                case "cancel":
                case "uncancel":
                    return updateStockPoint.Equals("1");
                default:
                    return false;
            }
        }

        public override void DoActionAfterTransaction(AppServiceNoTransArgs e)
        {
            base.DoActionAfterTransaction(e);
            Task.Factory.StartNew(() =>
                {
                    //晚5个s,让事务可以提交成功后在发送消息
                    System.Threading.Thread.Sleep(5000);
                    ApigatewayUtils.InvokeWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/RabbitMQ/SendMqMessage");
                });
        }

        public override void DoActionBatch(AppBusinessServiceArgs e)
        {
            base.DoActionBatch(e);
            //inventory
            GetFiled(e);
            this.InventoryOutInStock(e);

            if (_formId.EqualsIgnoreCase("SAL_OUTSTOCK") || _formId.EqualsIgnoreCase("SAL_DELIVERYNOTICE"))
            {
                //销售出库单不需要传云仓储，直接调拨单-出库在其他地方传云仓储
                return;
            }

            var sql = "insert into RabbitMQScheduledMessage(FAction,FKeyword,FMessage,FCreateDate) values(@FAction,@FKeyword,@FMessage,@FCreateDate)";
            foreach (var data in e.DataEntities)
            {
                long stockOrgId = GetStockOrgId(data.DataEntity);
                if (stockOrgId < 1)
                {
                    break;
                }

                Field field = _businessInfo.GetField(_serviceConfig.DocumentStatusField);
                object value = field.DynamicProperty.GetValue(data.DataEntity);
                string billStatus = ((value == null) ? "" : value.ToString());
                var updateStockPoint = GetUpdateStockPoint(stockOrgId);
                if (!IsUpdatePoint(updateStockPoint, billStatus))
                {
                    break;
                }
                QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
                queryBuilderParemeter.FormId = _formId;
                FormMetadata meta = MetaDataServiceHelper.Load(this.Context, _formId) as FormMetadata;
                queryBuilderParemeter.BusinessInfo = meta.BusinessInfo;
                queryBuilderParemeter.SelectItems = SelectorItemInfo.CreateItems(GetReloadKeys(_businessInfo, _serviceConfig));
                string filter = "";
                if (!_serviceConfig.Filter.IsNullOrEmptyOrWhiteSpace())
                {
                    filter = " and " + _serviceConfig.Filter.Replace('"', '\'');
                }
                queryBuilderParemeter.FilterClauseWihtKey = $"{_businessInfo.GetForm().PkFieldName}={data.DataEntity["Id"]} {filter} and {_serviceConfig.StockField}.FSyncToWarehouse = '1' and {_serviceConfig.MaterialField}.FIsInventory = '1'";
                var queryObject = QueryServiceHelper.GetQueryObject(this.Context, queryBuilderParemeter);
                string dicTableAlias = queryObject.DicTableAlias[_businessInfo.GetField(_serviceConfig.MaterialField).TableName];
                var datas = DBUtils.ExecuteDynamicObject(this.Context, $@"/*dialect*/ 
                select {queryObject.SQLSelect},gll.FNAME GROUP_DESC,gl.FNAME TYPE_DESC,new_t3.FNUMBER STOREUNITCODE 
                from {queryObject.SQLFrom}
                left join T_BD_MATERIAL new_t1 on {dicTableAlias}.FMATERIALID=new_t1.FMATERIALID
                left join T_BD_MATERIALGROUP g on new_t1.FMATERIALGROUP = g.FID
                left join T_BD_MATERIALGROUP_L gl on g.FID = gl.FID and gl.FLOCALEID = {this.Context.UserLocale.LCID}
                left join T_BD_MATERIALGROUP_L gll on g.FPARENTID = gll.FID and gll.FLOCALEID = {this.Context.UserLocale.LCID}
                left join T_BD_MATERIALSTOCK new_t2 on new_t1.FMATERIALID=new_t2.FMATERIALID
                LEFT JOIN  T_BD_UNIT new_t3  ON (new_t3.FUNITID=new_t2.FSTOREUNITID)
                where {queryObject.SQLWhere}");

                if (datas.Count > 0)
                {
                    var billNo = _businessInfo.GetField(_businessInfo.GetBillNoField().Key).DynamicProperty.GetValue(data.DataEntity).ToString();

                    if (_formId.EqualsIgnoreCase("SAL_RETURNSTOCK"))
                    {
                        //退货订单如果供货组织包含全国一部华南二部则退出
                        var entry = data.DataEntity["SAL_RETURNSTOCKENTRY"] as DynamicObjectCollection;
                        if (entry.Select(
                            x => Convert.ToInt64(x["FSupplyTargetOrgId_Id"]) == 7401780
                            || Convert.ToInt64(x["FSupplyTargetOrgId_Id"]) == 7401781
                            ).Count() > 0)
                        {
                            continue;
                        }
                    }
                    //其他入库需要判断是否需要传云仓储
                    if (_formId.EqualsIgnoreCase("STK_MISCELLANEOUS"))
                    {
                        var syncCloudStockSql = $@"select top 1 FNoSyncCloudStock from T_STK_MISCELLANEOUS where FBILLNO='{billNo}' ";
                        var isSyncCloudStock = DBServiceHelper.ExecuteScalar<int>(this.Context, syncCloudStockSql, 0);
                        if (isSyncCloudStock.Equals(1))
                        {
                            continue;
                        }
                    }
                    //其他出库需要判断是否需要传云仓储
                    if (_formId.EqualsIgnoreCase("STK_MisDelivery"))
                    {
                        var syncCloudStockSql = $@"select top 1 FNoSyncCloudStock from T_STK_MISDELIVERY where FBILLNO='{billNo}' ";
                        var isSyncCloudStock = DBServiceHelper.ExecuteScalar<int>(this.Context, syncCloudStockSql, 0);
                        if (isSyncCloudStock.Equals(1))
                        {
                            continue;
                        }
                    }

                    //判断生产领料，是全国一部和同步云存储才传
                    if (_formId.EqualsIgnoreCase("PRD_PickMtrl"))
                    {
                        var syncCloudStockSql = $@"select count(1) from T_PRD_PICKMTRL t1
                        inner join T_PRD_PICKMTRLDATA t2 on t1.FID=t2.FID
                        inner join t_BD_Stock t3 on t3.FSTOCKID=t2.FSTOCKID
                        where t1.FBILLNO='{billNo}' and FPrdOrgId=7401780 and t3.FSyncToWarehouse=1 ";
                        var syncCloudStockCount = DBServiceHelper.ExecuteScalar<int>(this.Context, syncCloudStockSql, 0);
                        if (syncCloudStockCount == 0)
                        {
                            continue;
                        }
                    }

                    //反批核删除云存储(不含组装拆卸单)
                    if (_operationNumber.EqualsIgnoreCase("UnAudit") && !_formId.EqualsIgnoreCase("STK_AssembledApp"))
                    {
                        ArrayList arrList = new ArrayList();
                        var requestUrl = "";
                        if (_serviceConfig.AddQty)
                        {
                            //云仓储撤回出库单接口
                            if (_formId.Equals("PUR_MRB") || _formId.Equals("PRD_ISSUEMTRNOTICE") || _formId.Equals("PRD_RetStock") ||
                                _formId.Equals("STK_StockCountLoss") || _formId.Equals("STK_MisDelivery") || _formId.Equals("PRD_PickMtrl"))
                            {
                                requestUrl = "/api/goods/cancelleddelivery";
                                foreach (var item in datas)
                                {
                                    var result = new
                                    {
                                        itemId = item["BillNo"] + "-" + item["Seq"],
                                        exWarehouseOrderNumber = item["BillNo"]
                                    };
                                    arrList.Add(result);
                                }
                            }
                        }
                        else
                        {
                            //云仓储撤回入库单接口
                            if (_formId.Equals("STK_InStock") || _formId.Equals("SAL_RETURNSTOCK") || _formId.Equals("PRD_INSTOCK") ||
                                _formId.Equals("PRD_ReturnMtrl") || _formId.Equals("STK_StockCountGain") || _formId.Equals("STK_MISCELLANEOUS"))
                            {
                                requestUrl = "/api/goods/unshelved";
                                foreach (var item in datas)
                                {
                                    var result = new
                                    {
                                        itemId = item["BillNo"] + "-" + item["Seq"],
                                        entryWarehouseOrderNumber = item["BillNo"],
                                        quantity = Convert.ToDecimal(item["Qty"])
                                    };
                                    arrList.Add(result);
                                }

                            }
                        }
                        if (!string.IsNullOrWhiteSpace(requestUrl) && arrList.Count > 0)
                        {
                            var cwResult = WarehouseApiRequest.Request(WarehouseApiRequest.CloudStockUrl, WarehouseApiRequest.CloudStockToken, requestUrl, JsonConvertUtils.SerializeObject(arrList), "MYMO", "DELETE");
                            var returnInfo = JsonConvertUtils.DeserializeObject<ResponseCloudWarehouseMessage>(cwResult);
                            if (!returnInfo.IsSuccess)
                            {
                                throw new Exception("反批核失败，云存储：" + returnInfo.Message);
                            }
                        }
                    }
                    else
                    {
                        List<SqlParam> sqlParams = new List<SqlParam>
                        {
                            new SqlParam("@FKeyword", KDDbType.String,billNo),
                            new SqlParam("@FCreateDate", KDDbType.DateTime, DateTime.Now)
                        };

                        // 库存增加-入库
                        if (_serviceConfig.AddQty)
                        {
                            sqlParams.Add(new SqlParam("@FAction", KDDbType.String, "CloudStockUpdateTempStockArea"));
                            List<PutToTempStockAreaRequest> putToTempStockAreaRequests = new List<PutToTempStockAreaRequest>();
                            foreach (var item in datas)
                            {

                                ExternalTypeModel type = new ExternalTypeModel();
                                switch (_formId)
                                {
                                    case "STK_InStock": // 1. 采购入库-采购入库单
                                        type.Value = "RI";
                                        type.Description = "检入";
                                        break;
                                    case "SAL_RETURNSTOCK": // 2. 销售退货-销售退货单
                                        type.Value = "RSO";
                                        type.Description = "销售退货";
                                        break;
                                    case "STK_AssembledApp": // 3. 组装拆卸入库（含替换）-组装拆卸单
                                        type.Value = "RCFG";
                                        type.Description = "重组进仓";
                                        Random r = new Random();
                                        item["BillNo"] = item["BillNo"] + "-" + _operationNumber + r.Next(10000, 99999).ToString();
                                        break;
                                    case "PRD_INSTOCK": //4. 产成品入库-生产入库单
                                        type.Value = "DW";
                                        type.Description = "产成品入库";
                                        break;
                                    case "PRD_ReturnMtrl": // 5. 生产退料-生产退料单
                                        type.Value = "SIN";
                                        type.Description = "车间退料";
                                        break;
                                    //case "STK_TransferDirect": // 6. 仓存转移入库-直接调拨单
                                    //    type.Value = "TIN";
                                    //    type.Description = "调拨进仓";
                                    //    break;
                                    case "STK_StockCountGain": // 7. 仓存盘点调整-盘盈单
                                        type.Value = "AIN";
                                        type.Description = "调整进仓";
                                        break;
                                    case "STK_MISCELLANEOUS": // 8.其它入库-其它入库单
                                        type.Value = "OIN";
                                        type.Description = "其他入库";
                                        break;
                                    default:
                                        return;
                                }


                                //供应商编号
                                string supplierCode = "";
                                //供应商名称
                                string supplierName = "";
                                //采购入库-采购入库单新增供应商信息传云存储
                                if (_formId.Equals("STK_InStock"))
                                {
                                    //获取供应商信息
                                    var supplierSql = $@"/*dialect*/select top 1 t2.FNUMBER,t3.FNAME from  t_STK_InStock t1
                                                    inner join t_BD_Supplier t2 on t1.FSUPPLIERID=t2.FSUPPLIERID
                                                    inner join t_BD_Supplier_L t3 on t2.FSUPPLIERID=t3.FSUPPLIERID and t3.FLOCALEID=2052
                                                    where FBILLNO='{item["BillNo"]}' ";

                                    var supplierDatas = DBServiceHelper.ExecuteDynamicObject(this.Context, supplierSql);
                                    foreach (var items in supplierDatas)
                                    {
                                        supplierCode = Convert.ToString(items["FNUMBER"]);
                                        supplierName = Convert.ToString(items["FNAME"]);
                                    }
                                }
                                PutToTempStockAreaRequest putToTempStockAreaRequest = new PutToTempStockAreaRequest
                                {
                                    ItemId = item["BillNo"] + "-" + item["Seq"],
                                    EntryWarehouseOrderNumber = Convert.ToString(item["BillNo"]),
                                    PurchaseOrderNumber = "",
                                    ModelNumber = Convert.ToString(item["MaterialNumber"]),
                                    Name = Convert.ToString(item["MaterialName"]),
                                    Quantity = Convert.ToDecimal(item["Qty"]),
                                    EntryOnUtc = Convert.ToDateTime(item["Date"]),
                                    Unit = new NewUnitModel
                                    {
                                        Name = Convert.ToString(item["STOREUNITCODE"]),
                                    },
                                    Type = type,
                                    LocCode = Convert.ToString(item["CloudStockCode"]), //云仓储仓库编码
                                    IsAutoHandle = item["PrintAutoStockOut"].ToString().Equals("1") ? true : false,//是否自动上下架
                                    DeliveryplaceCode = Convert.ToString(item["OutSourceStockLoc"]),//仓库发货区域
                                    DeliveryDetOrgCode = Convert.ToString(item["DeliveryDetOrgCode"]),
                                    Remark = "",
                                    GroupDesc = Convert.ToString(item["GROUP_DESC"]),
                                    TypeDesc = Convert.ToString(item["TYPE_DESC"]),
                                    NetWeight = Convert.ToDecimal(item["NETWEIGHT"]),
                                    Supplier = new StockGoodsSupplierModel
                                    {
                                        Coding = supplierCode,
                                        Name = supplierName
                                    }
                                };
                                putToTempStockAreaRequests.Add(putToTempStockAreaRequest);
                            }

                            sqlParams.Add(new SqlParam("@FMessage", KDDbType.String, JsonConvertUtils.SerializeObject(putToTempStockAreaRequests)));
                        }
                        else // 减少-出库
                        {
                            sqlParams.Add(new SqlParam("@FAction", KDDbType.String, "CloudStockUpdateTempDeliveryArea"));
                            List<PutToTempDeliveryAreaRequest> putToTempDeliveryAreaRequests = new List<PutToTempDeliveryAreaRequest>();
                            foreach (var item in datas)
                            {
                                ExternalTypeModel type = new ExternalTypeModel();

                                switch (_formId)
                                {
                                    case "PUR_MRB": // 1. 采购退货-采购退料单
                                        type.Value = "PUR";
                                        type.Description = "采购退货";
                                        break;
                                    case "STK_AssembledApp": // 2. 组装拆卸出库（含替换）-组装拆卸单
                                        type.Value = "ICFG";
                                        type.Description = "重组出仓";
                                        Random r = new Random();
                                        item["BillNo"] = item["BillNo"] + "-" + _operationNumber + r.Next(10000, 99999).ToString();
                                        break;
                                    case "PRD_ISSUEMTRNOTICE": // 3. 生产发料-生产发料通知单
                                        type.Value = "IWIP";
                                        type.Description = "发料";
                                        break;
                                    //case "STK_TransferDirect": //4. 仓存转移出库-直接调拨单
                                    //    type.Value = "TOUT";
                                    //    type.Description = "调拨出仓";
                                    //    break;
                                    case "PRD_RetStock": // 5. 生产退库-生产退库单
                                        type.Value = "SIN";
                                        type.Description = "车间退料";
                                        break;
                                    case "STK_StockCountLoss": // 5.仓存盘点调整-盘亏单
                                        type.Value = "AOUT";
                                        type.Description = "调整出仓";
                                        break;
                                    case "STK_MisDelivery": // 6.其它出库-其它出库单
                                        type.Value = "OUT";
                                        type.Description = "其他出库";
                                        break;
                                    case "PRD_PickMtrl": // 7. 生产领料
                                        type.Value = "SOUT";
                                        type.Description = "领料";
                                        break;
                                    default:
                                        return;
                                }

                                PutToTempDeliveryAreaRequest putToTempDeliveryAreaRequest = new PutToTempDeliveryAreaRequest
                                {
                                    ItemId = item["BillNo"] + "-" + item["Seq"],
                                    ExWarehouseOrderNumber = Convert.ToString(item["BillNo"]),
                                    ModelNumber = Convert.ToString(item["MaterialNumber"]),
                                    Name = Convert.ToString(item["MaterialName"]),
                                    Quantity = Convert.ToDecimal(item["Qty"]),
                                    ExWarehouseOnUtc = Convert.ToDateTime(item["Date"]).ToUniversalTime(),
                                    Unit = new NewUnitModel
                                    {
                                        Name = Convert.ToString(item["STOREUNITCODE"]),
                                    },
                                    Type = type,
                                    LocCode = Convert.ToString(item["CloudStockCode"]), //云仓储仓库编码
                                    IsAutoHandle = item["PrintAutoStockOut"].ToString().Equals("1") ? true : false,
                                    DeliveryplaceCode = Convert.ToString(item["OutSourceStockLoc"]),//仓库发货区域
                                    DeliveryDetOrgCode = Convert.ToString(item["DeliveryDetOrgCode"]),
                                    Remark = ""
                                };
                                putToTempDeliveryAreaRequests.Add(putToTempDeliveryAreaRequest);
                            }
                            sqlParams.Add(new SqlParam("@FMessage", KDDbType.String, JsonConvertUtils.SerializeObject(putToTempDeliveryAreaRequests)));
                        }
                        DBUtils.Execute(this.Context, sql, sqlParams);
                    }
                }
            }
        }

        private void GetFiled(AppBusinessServiceArgs e)
        {
            _businessInfo = e.BusinessInfo;
            _serviceConfig = (UpdateStockBusinessServiceMeta)e.FormBusinessService;
            _operationNumber = e.FormOperation.Operation;
            _formId = _businessInfo.GetForm().Id;
            _materialField = _businessInfo.GetField(_serviceConfig.MaterialField) as BaseDataField;
            string key = $"{_formId}-{_materialField.EntityKey}";
            if (_updateStockConfigDict.ContainsKey(key))
            {
                _updateStockConfig = _updateStockConfigDict[key];
            }
        }

        private void InventoryOutInStock(AppBusinessServiceArgs e)
        {
            List<RabbitMQMessage> messages = new List<RabbitMQMessage>();
            foreach (var dataEntitie in e.DataEntities)
            {
                QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
                {
                    FormId = _formId
                };
                FormMetadata meta = MetaDataServiceHelper.Load(this.Context, _formId) as FormMetadata;
                queryBuilderParemeter.BusinessInfo = meta.BusinessInfo;
                queryBuilderParemeter.SelectItems = SelectorItemInfo.CreateItems("FId");
                string filter = "";
                if (!_serviceConfig.Filter.IsNullOrEmptyOrWhiteSpace())
                {
                    filter = " and " + _serviceConfig.Filter.Replace('"', '\'');
                }
                queryBuilderParemeter.FilterClauseWihtKey = $"{_businessInfo.GetForm().PkFieldName}={dataEntitie.DataEntity["Id"]} {filter}";
                var queryObject = QueryServiceHelper.GetQueryObject(this.Context, queryBuilderParemeter);
                var satisfy = DBUtils.ExecuteScalar<bool>(this.Context, $@"/*dialect*/ select 1 from {queryObject.SQLFrom} where {queryObject.SQLWhere}", false);
                long stockOrgId = GetStockOrgId(dataEntitie.DataEntity);
                if (stockOrgId < 1 || !satisfy)
                {
                    break;
                }

                Field field = _businessInfo.GetField(_serviceConfig.DocumentStatusField);
                object value = field.DynamicProperty.GetValue(dataEntitie.DataEntity);
                string billStatus = ((value == null) ? "" : value.ToString());
                var updateStockPoint = GetUpdateStockPoint(stockOrgId);
                if (!IsUpdatePoint(updateStockPoint, billStatus))
                {
                    break;
                }
                var keyword = _businessInfo.GetField(_businessInfo.GetBillNoField().Key).DynamicProperty.GetValue(dataEntitie.DataEntity).ToString();

                var data = new
                {
                    Id = dataEntitie.DataEntity["Id"],
                    FormId = _formId,
                    BillNo = keyword,
                    OperationNumber = _operationNumber,
                    _serviceConfig.AddQty,
                    Date = _businessInfo.GetField("FDate").DynamicProperty.GetValue(dataEntitie.DataEntity),
                    StockOrg = GetOrgData(dataEntitie.DataEntity, _serviceConfig.StockOrgField),
                    Supplier = GetBaseData(dataEntitie.DataEntity, _updateStockConfig.SupplierField),
                    Customer = GetBaseData(dataEntitie.DataEntity, _updateStockConfig.CustomerField),
                    TrackingNumber = _businessInfo.GetField("FTrackingNumber")?.DynamicProperty.GetValue(dataEntitie.DataEntity),
                    TrackingName = _businessInfo.GetField("FTrackingName")?.DynamicProperty.GetValue(dataEntitie.DataEntity),
                    TrackingDate = _businessInfo.GetField("FTrackingDate")?.DynamicProperty.GetValue(dataEntitie.DataEntity),
                    TrackingUser = _businessInfo.GetField("FTrackingUser")?.DynamicProperty.GetValue(dataEntitie.DataEntity),
                    Details = GetDetailDatas(dataEntitie.DataEntity)
                };
                RabbitMQMessage message = new RabbitMQMessage()
                {
                    Exchange = "inventory-outinstock",
                    Routingkey = _formId,
                    Keyword = keyword,
                    Message = JsonConvertUtils.SerializeObject(data)
                };
                messages.Add(message);
            }
            KafkaProducerService kafkaProducer = new KafkaProducerService();
            kafkaProducer.AddMessage(this.Context, messages.ToArray());
        }

        private List<dynamic> GetDetailDatas(DynamicObject dataEntity)
        {
            List<dynamic> details = new List<dynamic>();
            if (_materialField.Entity is SubEntryEntity subEntity)
            {
                var entryDatas = subEntity.ParentEntity.DynamicProperty.GetValue<DynamicObjectCollection>(dataEntity);
                foreach (var entryData in entryDatas)
                {
                    GetDetailDatas(entryData, details);
                }
            }
            else
            {
                GetDetailDatas(dataEntity, details);
            }

            return details;
        }

        private void GetDetailDatas(DynamicObject dataEntity, List<dynamic> details)
        {
            var detailEntitys = _materialField.Entity.DynamicProperty.GetValue<DynamicObjectCollection>(dataEntity);
            foreach (var detailEntity in detailEntitys)
            {
                dynamic detail = new
                {
                    DetailId = detailEntity["Id"],
                    Seq = detailEntity["Seq"],
                    OrderNo = _businessInfo.GetField(_updateStockConfig.OrderNoField)?.DynamicProperty.GetValue(detailEntity),
                    OrderId = _businessInfo.GetField(_updateStockConfig.OrderIdField)?.DynamicProperty.GetValue(detailEntity),
                    OrderEntryId = _businessInfo.GetField(_updateStockConfig.OrderEntryField)?.DynamicProperty.GetValue(detailEntity),
                    OrderSeq = _businessInfo.GetField(_updateStockConfig.OrderEntrySeqField)?.DynamicProperty.GetValue(detailEntity),
                    Material = GetMaterialData(detailEntity, _materialField),
                    Small = GetBaseData(detailEntity, _updateStockConfig.SmallField),
                    ParentSmall = GetBaseData(detailEntity, _updateStockConfig.ParentSmallField),
                    Stock = GetStockData(detailEntity),
                    Unit = GetBaseData(detailEntity, _updateStockConfig.UnitField),
                    Qty = _businessInfo.GetField(_updateStockConfig.QtyField)?.DynamicProperty.GetValue(detailEntity),
                    BaseUnit = GetBaseData(detailEntity, _serviceConfig.BaseUnitIdField),
                    BaseQty = _businessInfo.GetField(_serviceConfig.BaseQtyField).DynamicProperty.GetValue(detailEntity),
                    SecUnit = GetBaseData(detailEntity, _serviceConfig.SecUnitIdField),
                    SecQty = _businessInfo.GetField(_serviceConfig.SecQtyField)?.DynamicProperty.GetValue(detailEntity),
                    ProjectNo = _businessInfo.GetField(_serviceConfig.ProjectNoField)?.DynamicProperty.GetValue(detailEntity),
                    MtoNo = _businessInfo.GetField(_serviceConfig.MtoNoField)?.DynamicProperty.GetValue(detailEntity),
                    BomId = _businessInfo.GetField(_serviceConfig.BomIdField)?.DynamicProperty.GetValue(detailEntity),
                    StockStatus = GetBaseData(detailEntity, _serviceConfig.StockStatusField),
                    Lot = _businessInfo.GetField(_serviceConfig.LotField)?.DynamicProperty.GetValue(detailEntity),
                    ProductDate = _businessInfo.GetField(_serviceConfig.ProductDateField)?.DynamicProperty.GetValue(detailEntity),
                    ValidateTo = _businessInfo.GetField(_serviceConfig.ValidateToField)?.DynamicProperty.GetValue(detailEntity)
                };
                details.Add(detail);
            }
        }

        private dynamic GetStockData(DynamicObject detailEntity)
        {
            if (!(_businessInfo.GetField(_serviceConfig.StockField) is BaseDataField field))
            {
                return null;
            }
            if (!(field.DynamicProperty.GetValue(detailEntity) is DynamicObject fieldalue))
            {
                return null;
            }
            return new
            {
                Id = fieldalue["Id"],
                Code = fieldalue["Number"],
                Name = fieldalue["Name"].ToString(),
                CloudStockCode = fieldalue["FCloudStockCode"],
                OutSourceStockLoc = fieldalue["FOutSourceStockLoc"],
                //云存储只打单,自动下架
                PrintAutoStockOut = Convert.ToBoolean(fieldalue["FPrintAutoStockOut"]),
                SyncToWarehouse = Convert.ToBoolean(fieldalue["FSyncToWarehouse"]),
                IsDirStock = Convert.ToBoolean(fieldalue["FIsDirStock"]),
                IsOutSourceStock = Convert.ToBoolean(fieldalue["FIsOutSourceStock"])
            };
        }

        private dynamic GetMaterialData(DynamicObject detailEntity, BaseDataField materialField)
        {
            if (!(materialField.DynamicProperty.GetValue(detailEntity) is DynamicObject fieldalue))
            {
                return null;
            }
            return new
            {
                Id = fieldalue["Id"],
                Code = fieldalue["Number"],
                Name = fieldalue["Name"].ToString(),
                Textures = fieldalue["FTextures"],
                NetWeight = Convert.ToDecimal((fieldalue["MaterialBase"] as DynamicObjectCollection)[0]["NetWeight"]),
                IsInventory = Convert.ToBoolean((fieldalue["MaterialBase"] as DynamicObjectCollection)[0]["IsInventory"])
            };
        }

        private dynamic GetOrgData(DynamicObject billData, string key)
        {
            if (!(_businessInfo.GetField(key) is BaseDataField field))
            {
                return null;
            }
            if (!(field.DynamicProperty.GetValue(billData) is DynamicObject fieldalue))
            {
                return null;
            }
            return new
            {
                Id = fieldalue["Id"],
                Code = fieldalue["Number"],
                Name = fieldalue["Name"].ToString(),
                OutSourceStockLoc = fieldalue["FOutSourceStockLoc"],
                //调拨单云仓储自动上下架
                TransIsSynCloudStock = Convert.ToBoolean(fieldalue["FTransIsSynCloudStock"])
            };
        }

        private dynamic GetBaseData(DynamicObject billData, BaseDataField field)
        {
            if (!(field.DynamicProperty.GetValue(billData) is DynamicObject fieldalue))
            {
                return null;
            }
            return new
            {
                Id = fieldalue["Id"],
                Code = fieldalue["Number"],
                Name = fieldalue["Name"].ToString()
            };
        }

        private dynamic GetBaseData(DynamicObject billData, string key)
        {
            if (!(_businessInfo.GetField(key) is BaseDataField field))
            {
                return null;
            }
            return GetBaseData(billData, field);
        }

        private string GetUpdateStockPoint(long stockOrgId)
        {
            //2  审核时  1  保存时
            if (!_upstockSetting.TryGetValue(stockOrgId, out string updateStockPoint))
            {
                updateStockPoint = "2";
                CommonService commonService = new CommonService();
                object systemProfile = commonService.GetSystemProfile(this.Context, stockOrgId, "STK_StockParameter", "UpdateStockPoint", "2");
                if (systemProfile != null && !string.IsNullOrWhiteSpace(systemProfile.ToString()))
                {
                    updateStockPoint = systemProfile.ToString();
                }
                _upstockSetting[stockOrgId] = updateStockPoint;
            }

            return updateStockPoint;
        }

        private long GetStockOrgId(DynamicObject billData)
        {
            string key = _serviceConfig.StockOrgField;
            string id = _businessInfo.GetForm().Id;
            if (id.EqualsIgnoreCase("STK_TRANSFEROUT") || id.EqualsIgnoreCase("STK_TRANSFERIN"))
            {
                key = "FStockOrgID";
            }
            if (id.EqualsIgnoreCase("STK_TransferDirect") || id.EqualsIgnoreCase("REM_TransferDirect"))
            {
                key = "FStockOutOrgId";
            }
            Field field = _businessInfo.GetField(key);
            if (field == null)
            {
                return 0L;
            }
            return Convert.ToInt64(((BaseDataField)field).RefIDDynamicProperty.GetValue(billData));
        }

        private string GetReloadKeys(BusinessInfo billInfo, UpdateStockBusinessServiceMeta upService)
        {
            string result = $@"{billInfo.GetBillNoField().Key} as BillNo,{upService.MaterialField}.FNumber as MaterialNumber,{upService.MaterialField}.FName as MaterialName
                            ,{upService.BaseUnitIdField}.FName as UnitName,{upService.StockField}.FOutSourceStockLoc as OutSourceStockLoc,{upService.StockField}.FCloudStockCode as CloudStockCode
                            ,{upService.StockField}.FName as StockName,{upService.StockField}.FNumber as LocCode,{upService.StockField}.FPrintAutoStockOut as PrintAutoStockOut
                            ,{upService.StockOrgField}.FName as OrgName,{upService.StockOrgField}.FNumber as DeliveryDetOrgCode
                            ,{upService.MaterialField}.FNETWEIGHT as NETWEIGHT";
            if (!string.IsNullOrWhiteSpace(upService.UpdateFlagField))
            {
                var seqFiled = billInfo.GetField(upService.UpdateFlagField);
                if (!(seqFiled.Entity is HeadEntity) && !seqFiled.Entity.SeqFieldKey.IsNullOrEmptyOrWhiteSpace())
                {
                    result += $",{seqFiled.Entity.Key}_{seqFiled.Entity.SeqFieldKey} as Seq";
                }
            }
            if (_formId == "STK_InStock")
            {
                result += ",FRealQty as Qty";
            }
            else if (_formId == "PUR_MRB")
            {
                result += ",FRMREALQTY as Qty";
            }
            else
            {
                result += $",{upService.BaseQtyField} as Qty ";
            }

            if (!string.IsNullOrWhiteSpace(upService.CancelStatusField))
            {
                result += $",{upService.CancelStatusField} as CancelStatus";
            }
            if (!string.IsNullOrWhiteSpace(upService.DocumentStatusField))
            {
                result += $",{upService.DocumentStatusField} as DocumentStatus";
            }
            if (!string.IsNullOrWhiteSpace(upService.BizDateField))
            {
                result += $",{upService.BizDateField} as Date";
            }
            if (!string.IsNullOrWhiteSpace(upService.UpdateFlagField))
            {
                result += $",{upService.UpdateFlagField} as UpdateFlag";
            }

            return result;
        }
    }
}
