using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Contracts;
using Kingdee.Mymooo.Contracts.PurchaseManagement;

namespace Kingdee.Mymooo.App.Core.PurchaseManagement
{
    public class PrBillToPoBillService : IPrBillToPoBillService
    {
        /// <summary>
        /// PrToPo服务
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="lists"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public IOperationResult PrBillToPoBillAction(Context ctx, List<PrToPoPurchaseRequireEntity> lists)
        {
            IOperationResult opResult = new OperationResult();
            MymoooBusinessDataService service = new MymoooBusinessDataService();
            using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
            {
                //循序更新米重
                foreach (var item in lists)
                {
                    if (item.WeightRate > 0)
                    {
                        var sql = $@"/*dialect*/update T_BD_UNITCONVERTRATE set FConvertDenominator={item.WeightRate} 
                        from T_BD_UNITCONVERTRATE un where un.FMaterialId in(select FMaterialId from T_BD_MATERIAL mat where mat.FNUMBER='{item.ItemNo}')";
                        DBServiceHelper.Execute(ctx, sql);
                    }
                }
                var targetBillTypeId = "83d822ca3e374b4ab01e5dd46a0062bd";//标准采购订单
                //需要区分VMI采购申请和标准
                var isComplete = true;
                foreach (var billType in lists.Select(x => x.BillTypeId).Distinct().ToList())
                {
                    var list = lists.Where(x => x.BillTypeId.EqualsIgnoreCase(billType)).ToList();
                    if (billType.EqualsIgnoreCase("656969d9e809f0"))
                    {
                        //VMI采购订单
                        targetBillTypeId = "0023240234df807511e308990e04cf6a";
                    }
                    else if (billType.EqualsIgnoreCase("63f48c5263b415"))
                    {
                        targetBillTypeId = "6620c92e9496ea";
                    }
                    List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
                    var groupList = list.Select(x => x.FId).Distinct().ToList();
                    foreach (var thisFid in groupList)
                    {
                        var view = FormMetadataUtils.CreateBillView(ctx, "PUR_Requisition", thisFid);
                        var entrys = view.Model.GetEntityDataObject(view.BusinessInfo.GetEntity("FEntity"));
                        foreach (var item in list.Where(x => x.FId == thisFid))
                        {
                            var entry = entrys.FirstOrDefault(x => item.FentryId == Convert.ToInt64(x["Id"]));
                            if (entry != null)
                            {
                                var rowIndex = entrys.IndexOf(entry);
                                view.Model.SetValue("FREQQTY", item.PurchaseNum, rowIndex);
                                view.InvokeFieldUpdateService("FREQQTY", rowIndex);
                                view.Model.SetValue("FSUGGESTPURDATE", item.RequiredDeliveryDate, rowIndex);
                                view.Model.SetValue("FARRIVALDATE", item.RequiredDeliveryDate, rowIndex);
                                //view.Model.SetValue("FSUGGESTSUPPLIERID", item.VendorId, rowIndex);
								view.Model.SetItemValueByNumber("FSUGGESTSUPPLIERID", item.VendorCode, rowIndex);
								view.Model.SetValue("FENTRYNOTE", item.Remark, rowIndex);
                                view.Model.SetValue("FSupplierProductCode", item.SupplierProductCode, rowIndex);
                                //view.Model.SetValue("FSupplierUnitPrice", item.Price, rowIndex);
                            }

                            var oper = service.SaveBill(ctx, view.BusinessInfo, new DynamicObject[] { view.Model.DataObject });
                            //清除释放网控
                            view.CommitNetworkCtrl();
                            view.InvokeFormOperation(FormOperationEnum.Close);
                            view.Close();
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
                            selectedRows.Add(new ListSelectedRow(thisFid.ToString(), item.FentryId.ToString(), 0, "PUR_Requisition") { EntryEntityKey = "FEntity" });
                        }
                    }

                    // 假设：上游单据FormId为sourceFormId，下游单据FormId为targetFormId
                    var rules = ConvertServiceHelper.GetConvertRules(ctx, "PUR_Requisition", "PUR_PurchaseOrder");
                    var rule = rules.FirstOrDefault(t => t.IsDefault);
                    if (rule == null)
                    {
                        throw new Exception("没有从采购申请单到采购订单的转换关系");
                    }
                    //插件下推代码参考
                    PushArgs pushArgs = new PushArgs(rule, selectedRows.ToArray())
                    {
                        TargetBillTypeId = targetBillTypeId,     // 请设定目标单据单据类型
                        TargetOrgId = 0,            // 请设定目标单据主业务组织
                                                    //CustomParams = ,     // 可以传递额外附加的参数给单据转换插件
                    };

                    //执行下推操作，并获取下推结果
                    var operationResult = ConvertServiceHelper.Push(ctx, pushArgs, null);
                    List<DynamicObject> dynamicObjectList = new List<DynamicObject>();
                    if (operationResult.IsSuccess)
                    {
                        var view = FormMetadataUtils.CreateBillView(ctx, "PUR_PurchaseOrder");
                        foreach (var item in operationResult.TargetDataEntities)
                        {
                            view.Model.DataObject = item.DataEntity;
                            ////赋值采购员FPURCHASERID
                            //var buyerId = GetBuyerId(this.Context.UserId);
                            var entrys = view.Model.GetEntityDataObject(view.BusinessInfo.GetEntity("FPOOrderEntry"));
                            int rowIndex = 0;
                            foreach (var entry in entrys)
                            {
                                var thisList = list.FirstOrDefault(x => x.FentryId == Convert.ToInt64(((entry["FPOOrderEntry_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SId"]));
                                view.Model.SetValue("FTAXPRICE", thisList.Price, rowIndex);
                                view.InvokeFieldUpdateService("FTAXPRICE", rowIndex);
                                view.Model.SetValue("FRawMaterialPrice", thisList.RawMaterialPrice, rowIndex);
                                view.Model.SetValue("FProcessFee", thisList.ProcessFee, rowIndex);
                                view.Model.SetValue("FSUGGESTSUPPLIERID", thisList.SuggestSupplierId, rowIndex);
                                view.Model.SetValue("FSupplierUnitPrice", thisList.SuggestSupplierPrice, rowIndex);
                                rowIndex++;
                            }
                            dynamicObjectList.Add(view.Model.DataObject);
                        }
                        try
                        {
                            //保存
                            var opers = service.SaveBill(ctx, view.BusinessInfo, dynamicObjectList.ToArray());
                            //清除释放网控
                            view.CommitNetworkCtrl();
                            view.InvokeFormOperation(FormOperationEnum.Close);
                            view.Close();
                            if (opers.IsSuccess)
                            {
                                opResult.IsSuccess = true;
                                //写入提示信息
                                foreach (var p in opers.OperateResult)
                                {
                                    opResult.OperateResult.Add(new OperateResult()
                                    {
                                        Name = "转PO成功",
                                        Message = p.Message,
                                        MessageType = p.MessageType,
                                        SuccessStatus = true
                                    });
                                }
                            }
                            else
                            {
                                isComplete = false;
                                //错误提示
                                if (opers.ValidationErrors.Count > 0)
                                {
                                    foreach (var p in opers.ValidationErrors)
                                    {
                                        opResult.OperateResult.Add(new OperateResult()
                                        {
                                            Name = "转PO失败",
                                            Message = p.Message,
                                            MessageType = MessageType.FatalError,
                                            SuccessStatus = false
                                        });
                                    }
                                }
                                else
                                {
                                    foreach (var p in opers.OperateResult.Where(p => !p.SuccessStatus))
                                    {
                                        opResult.OperateResult.Add(new OperateResult()
                                        {
                                            Name = "转PO失败",
                                            Message = p.Message,
                                            MessageType = MessageType.FatalError,
                                            SuccessStatus = false
                                        });
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            isComplete = false;
                            opResult.OperateResult.Add(new OperateResult()
                            {
                                Name = "转PO失败",
                                Message = ex.Message,
                                MessageType = MessageType.FatalError,
                                SuccessStatus = false
                            });
                        }
                    }
                    else
                    {
                        if (operationResult.ValidationErrors.Count > 0)
                        {
                            foreach (var p in operationResult.ValidationErrors)
                            {
                                opResult.OperateResult.Add(new OperateResult()
                                {
                                    Name = "转PO下推失败",
                                    Message = p.Message,
                                    MessageType = MessageType.FatalError,
                                    SuccessStatus = false
                                });
                            }
                        }
                        else
                        {
                            foreach (var p in operationResult.OperateResult.Where(p => !p.SuccessStatus))
                            {
                                opResult.OperateResult.Add(new OperateResult()
                                {
                                    Name = "转PO下推失败",
                                    Message = p.Message,
                                    MessageType = MessageType.FatalError,
                                    SuccessStatus = false
                                });
                            }
                        }
                    }
                }
                //执行没有错误才提交
                if (isComplete)
                {
                    opResult.IsSuccess = true;
                    cope.Complete();
                }
                else
                {
                    opResult.IsSuccess = false;
                    opResult.OperateResult.Add(new OperateResult()
                    {
                        Name = "自动取消",
                        Message = "存在错误，转成功的PO订单自动取消。",
                        MessageType = MessageType.FatalError,
                        SuccessStatus = false
                    });
                }

            }
            return opResult;
        }
    }
}
