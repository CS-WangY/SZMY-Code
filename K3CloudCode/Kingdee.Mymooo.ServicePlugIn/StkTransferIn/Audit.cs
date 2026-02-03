using Kingdee.BOS;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Const;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.BomManagement;
using Kingdee.Mymooo.Core.Common;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.Mymooo.ServicePlugIn.MrpModel.MrpCalingValidator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
using Kingdee.BOS.ServiceHelper;
namespace Kingdee.Mymooo.ServicePlugIn.StkTransferIn
{
    [Description("分步式调入单批核插件"), HotUpdate]
    public class Audit : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FDestStockID");//调入仓库
            e.FieldKeys.Add("FQty");
            e.FieldKeys.Add("FMaterialID");
            e.FieldKeys.Add("FUnitID");
            e.FieldKeys.Add("FOwnerID");//调入货主
                                        //e.FieldKeys.Add("FStockOrgID");//调入库存组织
            e.FieldKeys.Add("FStockOutOrgID");//调出库存组织(按调出组织)
            e.FieldKeys.Add("FPENYDeliveryNotice");//发货通知单单号
            e.FieldKeys.Add("FDeliveryNoticeSEQ");//发货通知单序号
            e.FieldKeys.Add("FTransferDirect");//调拨方向
            e.FieldKeys.Add("FIsVmiBusiness");
            e.FieldKeys.Add("FDeliveryNoticeENTRYID");
        }


        /// <summary>
        /// 事务中 操作结束
        /// </summary>
        /// <param name="e"></param>
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);

            foreach (var data in e.DataEntitys)
            {
                //云仓储做入库
                var whInDn = (ArrayList)(GetInDelivery(data));
                if (whInDn.Count > 0)
                {
                    var requestData = JsonConvertUtils.SerializeObject(whInDn);
                    AddRabbitMessageUtils.AddRabbitMessage(this.Context, requestData, "CloudStockUpdateTempStockArea", Convert.ToString(data["BillNo"]));
                }
                //修改云存储调拨入库审核状态
                if (!data["TransferDirect"].ToString().Equals("RETURN"))
                {
                    var InboundDn = (ArrayList)(GetExamineInboundBill(data));
                    if (InboundDn.Count > 0)
                    {
                        var requestData = JsonConvertUtils.SerializeObject(InboundDn);
                        AddRabbitMessageUtils.AddRabbitMessage(this.Context, requestData, "CloudStockExamineInboundBill", Convert.ToString(data["BillNo"]));

                        //调出组织id
                        var StockOutOrgID = Convert.ToInt64(data["StockOutOrgID_Id"]);
                        //是否自动生成出库
                        DynamicObject dynamic = (data["STK_STKTRANSFERINENTRY"] as DynamicObjectCollection)[0];
                        var dnNo = Convert.ToString(dynamic["FPENYDELIVERYNOTICE"]);
                        if (!string.IsNullOrWhiteSpace(dnNo))
                        {
                            //add240627 固定华东五部和柔性产线和华东三部业务才自动出库
                            //del20250528if (StockOutOrgID == 7401803 || StockOutOrgID == 7401782 || StockOutOrgID == 7401801)
                            if (StockOutOrgID == 7401803 || StockOutOrgID == 7401782 || StockOutOrgID == 7401801
                                || StockOutOrgID == 14055372 || StockOutOrgID == 14053641)
                            //if (Convert.ToBoolean(((DynamicObject)data["StockOutOrgID"])["FTransIsSynCloudStock"]))
                            {
                                //直发订单不自动出库
                                var sql = $@"/*dialect*/select top 1 FBILLTYPEID from T_SAL_DELIVERYNOTICE t1 where t1.FBILLNO='{dnNo}' ";
                                var BillTypeID = DBUtils.ExecuteScalar<string>(this.Context, sql, "");
                                if (!BillTypeID.EqualsIgnoreCase("650bfcf4264fca"))
                                {
                                    sql = $@"/*dialect*/select SUM(num) from (
                                                select count(1) num from T_STK_STKTRANSFEROUT t1 
                                                                        inner join T_STK_STKTRANSFEROUTENTRY t2 on t1.FID=t2.FID
                                                                        where FPENYDELIVERYNOTICE='{dnNo}' 
						                                                and t1.FDOCUMENTSTATUS<>'C' and t1.FCANCELSTATUS='A' 
						                                                and t1.FTRANSFERDIRECT='GENERAL'
                                                union all
                                                select count(1) num from T_STK_STKTRANSFERIN t1 
                                                                        inner join T_STK_STKTRANSFERINENTRY t2 on t1.FID=t2.FID
                                                                        where FPENYDELIVERYNOTICE='{dnNo}' 
						                                                and t1.FDOCUMENTSTATUS<>'C' and t1.FCANCELSTATUS='A' 
						                                                and t1.FTRANSFERDIRECT='GENERAL'
                                                ) datas";
                                    var retCount = DBUtils.ExecuteScalar<int>(this.Context, sql, 0);
                                    if (retCount == 0)
                                    {
                                        sql = $@"/*dialect*/select FID from T_SAL_DELIVERYNOTICE t1 where t1.FBILLNO='{dnNo}' and not exists(select top 1 FID from T_SAL_OUTSTOCK t2 where t1.FBILLNO=t2.FBILLNO) ";
                                        long fid = DBUtils.ExecuteScalar<long>(this.Context, sql, 0);
                                        if (fid > 0)
                                        {
                                            var view = FormMetadataUtils.CreateBillView(this.Context, "SAL_DELIVERYNOTICE", fid);
                                            PushSaleOutStorage(this.Context, view.Model.DataObject);
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
                else
                {
                    foreach (var item in data["STK_STKTRANSFERINENTRY"] as DynamicObjectCollection)
                    {
                        var material = item["MaterialID"] as DynamicObject;
                        if (Convert.ToBoolean((
                            (DynamicObjectCollection)
                            (material["MaterialPurchase"])
                            )[0]["IsVmiBusiness"]))
                        {
                            var delnEID = Convert.ToInt64(item["FDeliveryNoticeENTRYID"]);
                            var qty = Convert.ToDecimal(item["FQty"]);
                            string sSql = $@"SELECT t2.FID,t2.FOWNEROUTID,t2.FOWNERTYPEOUTID,t3.FNUMBER FSupplierNumber
                            FROM dbo.T_STK_STKTRANSFERINENTRY t1
                            LEFT JOIN T_STK_STKTRANSFERIN t2 ON t1.FID=t2.FID
                            LEFT JOIN dbo.T_BD_SUPPLIER t3 ON t2.FOWNEROUTID=t3.FSUPPLIERID
                            WHERE t1.FPENYDELIVERYNOTICEEID='{delnEID}' AND t2.FOWNERTYPEOUTID='BD_Supplier'";
                            var trandatas = DBUtils.ExecuteDynamicObject(this.Context, sSql);
                            foreach (var tranitem in trandatas)
                            {
                                PushTransfer(this.Context, tranitem["FID"].ToString(), tranitem["FSupplierNumber"].ToString(), qty);
                            }

                        }
                    }
                }
            }

        }
        /// <summary>
        /// 构建云仓储做入库接口参数
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public object GetInDelivery(DynamicObject data)
        {
            ArrayList arrList = new ArrayList();
            foreach (var item in data["STK_STKTRANSFERINENTRY"] as DynamicObjectCollection)
            {
                if (item["DestStockId"] != null)
                {
                    //是否同步云仓储
                    if (bool.Parse(((DynamicObject)item["DestStockId"])["FSyncToWarehouse"].ToString()))
                    {
                        var IsAutoHandle = false;
                        //存在发货通知单单号才判断自动上下架状态
                        if (!string.IsNullOrWhiteSpace(Convert.ToString(item["FPENYDeliveryNotice"])))
                        {
                            IsAutoHandle = Boolean.Parse(((DynamicObject)data["StockOutOrgID"])["FTransIsSynCloudStock"].ToString());
                        }
                        var result = new PutToTempStockAreaRequest
                        {
                            ExWarehouseOrderNumber = Convert.ToString(item["FPENYDeliveryNotice"]),
                            DeliveryDetOrgCode = Convert.ToString(((DynamicObject)item["OwnerID"])["Number"]),
                            IsAutoHandle = IsAutoHandle,
                            ItemId = data["BillNo"] + "-" + item["Seq"],
                            EntryWarehouseOrderNumber = Convert.ToString(data["BillNo"]),
                            ModelNumber = Convert.ToString(((DynamicObject)item["MaterialID"])["Number"]),//物料编号
                            Name = Convert.ToString(((DynamicObject)item["MaterialID"])["Name"]),//物料名称
                            Specification = Convert.ToString(((DynamicObject)item["MaterialID"])["Specification"]),//物料规格型号
                            Quantity = Math.Abs(decimal.Parse(item["FQty"].ToString())),
                            EntryOnUtc = DateTime.Now,
                            Unit = new NewUnitModel
                            {
                                Name = Convert.ToString(((DynamicObject)item["UnitID"])["Number"])//单位编号
                            },
                            Type = new ExternalTypeModel
                            {
                                Value = "AIN",
                                Description = "调拨入仓"
                            },
                            Remark = "分步式调拨",
                            LocCode = Convert.ToString(((DynamicObject)item["DestStockId"])["FCloudStockCode"]),//(仓库对应的云仓储仓库编码)
                            IsDirectDeliveryStock = Convert.ToBoolean(((DynamicObject)item["DestStockId"])["FIsDirStock"]),//(仓库对应的是否直发仓库)
                            DeliveryplaceCode = Convert.ToString(((DynamicObject)item["DestStockId"])["FOutSourceStockLoc"])//仓库对应的仓库发货区域
                        };
                        arrList.Add(result);
                    }
                }
            }
            return arrList;
        }

        /// <summary>
        /// 构建修改云存储调拨入库审核状态
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public object GetExamineInboundBill(DynamicObject data)
        {
            ArrayList arrList = new ArrayList();
            List<ExamineInboundBillModel> list = new List<ExamineInboundBillModel>();
            foreach (var item in data["STK_STKTRANSFERINENTRY"] as DynamicObjectCollection)
            {
                if (!string.IsNullOrWhiteSpace(item["FPENYDELIVERYNOTICE"].ToString()))
                {
                    list.Add(new ExamineInboundBillModel
                    {
                        ItemId = item["FPENYDELIVERYNOTICE"] + "-" + item["FDeliveryNoticeSEQ"],
                        Qty = Math.Abs(decimal.Parse(item["FQty"].ToString()))
                    });
                }
            }
            arrList.AddRange(list.GroupBy(g => new { g.ItemId }).Select(t => new ExamineInboundBillModel { ItemId = t.Key.ItemId, Qty = t.Sum(s => s.Qty) }).ToList());
            return arrList;
        }

        // 下推销售出库单
        public void PushSaleOutStorage(Context ctx, DynamicObject dynamicObject)
        {
            var rules = ConvertServiceHelper.GetConvertRules(ctx, "SAL_DELIVERYNOTICE", "SAL_OUTSTOCK");
            var rule = rules.FirstOrDefault(t => t.IsDefault);
            if (rule == null)
            {
                throw new Exception("没有从发货通知单到销售出库单的转换关系");
            }
            var fid = Convert.ToString(dynamicObject["Id"]);
            List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
            foreach (var entitem in dynamicObject["SAL_DELIVERYNOTICEENTRY"] as DynamicObjectCollection)
            {
                var entryid = Convert.ToString(entitem["Id"]);
                selectedRows.Add(new ListSelectedRow(fid, entryid, 0, "SAL_DELIVERYNOTICE") { EntryEntityKey = "FEntity" });
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
                List<DynamicObject> dynamicObjectList = new List<DynamicObject>();
                if (operationResult.IsSuccess)
                {
                    var dnView = FormMetadataUtils.CreateBillView(ctx, "SAL_OUTSTOCK");
                    foreach (var item in operationResult.TargetDataEntities)
                    {
                        dnView.Model.DataObject = item.DataEntity;
                        dynamicObjectList.Add(dnView.Model.DataObject);
                    }
                    //保存
                    var opers = MymoooBusinessDataServiceHelper.SaveAndAuditBill(ctx, dnView.BusinessInfo, dynamicObjectList.ToArray());
                    if (!opers.IsSuccess)
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
        }

        public void PushTransfer(Context ctx, string FTransferID, string suppliNumber, decimal qty)
        {
            var rules = ConvertServiceHelper.GetConvertRules(ctx, "STK_TransferDirect", "STK_TransferDirect");
            var rule = rules.FirstOrDefault(t => t.IsDefault);
            if (rule == null)
            {
                throw new Exception("没有从直接调拨单到直接调拨单的转换关系");
            }
            List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
            selectedRows.Add(new ListSelectedRow(FTransferID, string.Empty, 0, "TransferDirect"));

            //有数据才需要下推
            if (selectedRows.Count > 0)
            {
                PushArgs pushArgs = new PushArgs(rule, selectedRows.ToArray())
                {
                    TargetBillTypeId = "005056941128823c11e323525db18103",     // 请设定目标单据单据类型
                    TargetOrgId = 0,            // 请设定目标单据主业务组织
                                                //CustomParams = ,     // 可以传递额外附加的参数给单据转换插件
                };
                //执行下推操作，并获取下推结果
                var operationResult = ConvertServiceHelper.Push(ctx, pushArgs, null);
                List<DynamicObject> dynamicObjectList = new List<DynamicObject>();
                if (operationResult.IsSuccess)
                {
                    var dnView = FormMetadataUtils.CreateBillView(ctx, "STK_TransferDirect");
                    foreach (var item in operationResult.TargetDataEntities)
                    {
                        dnView.Model.DataObject = item.DataEntity;
                        dnView.Model.SetValue("FQty", qty);
                        dnView.Model.SetItemValueByNumber("FOwnerIdHead", suppliNumber, 0);
                        dynamicObjectList.Add(dnView.Model.DataObject);
                    }
                    //保存
                    var opers = MymoooBusinessDataServiceHelper.SaveAndAuditBill(ctx, dnView.BusinessInfo, dynamicObjectList.ToArray());
                    if (!opers.IsSuccess)
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
        }
        //审核增加运算校验
        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
            e.Validators.Add(new IsMrpCalingValidator() { AlwaysValidate = true, EntityKey = "FBillHead" });
        }
    }
}
