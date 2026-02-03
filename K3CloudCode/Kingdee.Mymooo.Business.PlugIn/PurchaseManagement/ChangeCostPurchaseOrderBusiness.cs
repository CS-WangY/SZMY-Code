using Kingdee.BOS;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Orm;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.PayableManagement;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.Mymooo.ServiceHelper.StockManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.PurchaseManagement
{
    public class ChangeCostPurchaseOrderBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            ChangeCostPurchaseOrderRequest request = JsonConvertUtils.DeserializeObject<ChangeCostPurchaseOrderRequest>(message);
            try
            {
                //if (string.IsNullOrWhiteSpace(request.Note))
                //{
                //    response.Code = ResponseCode.Exception;
                //    response.Message = "变更原因不能为空！";
                //    return response;
                //}
                string sql = $@"select top 1 FID from T_AP_PAYABLEENTRY where FSOURCEBILLNO='{request.BillNo}' ";
                if (DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0) > 0)
                {
                    response.Code = ResponseCode.Exception;
                    response.Message = "已生成费用应付，不能变更！";
                    return response;
                }
                sql = $@"select top 1 FId from T_PUR_POORDER where FBILLNO='{request.BillNo}' ";
                long fId = DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0);
                if (fId == 0)
                {
                    response.Code = ResponseCode.Exception;
                    response.Message = "费用采购订单不存在！";
                    return response;
                }

                var billView = FormMetadataUtils.CreateBillView(ctx, "PUR_PurchaseOrder", fId);
                var entrys = billView.Model.GetEntityDataObject(billView.BusinessInfo.GetEntity("FPOOrderEntry"));
                //billView.Model.SetValue("FNote", request.Note);
                foreach (var item in request.Details)
                {
                    var entry = entrys.FirstOrDefault(x => item.MesEntryId == Convert.ToString(x["FMesEntryId"]));
                    if (entry != null)
                    {
                        var rowIndex = entrys.IndexOf(entry);
                        billView.Model.SetValue("FTaxPrice", item.TaxPrice, rowIndex);
                        billView.InvokeFieldUpdateService("FTaxPrice", rowIndex);
                        billView.Model.SetValue("FQty", item.Qty, rowIndex);
                        billView.InvokeFieldUpdateService("FQty", rowIndex);
                    }
                }
                var oper = MymoooBusinessDataServiceHelper.SaveBill(ctx, billView.BusinessInfo, billView.Model.DataObject);
                //清除释放网控
                billView.CommitNetworkCtrl();
                billView.InvokeFormOperation(FormOperationEnum.Close);
                billView.Close();
                if (!oper.IsSuccess)
                {
                    if (oper.ValidationErrors.Count > 0)
                    {
                        response.Message += string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                        response.Code = ResponseCode.ModelError;
                        return response;
                    }
                    else
                    {
                        response.Message += string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                        response.Code = ResponseCode.ModelError;
                        return response;
                    }
                }
                response.Code = ResponseCode.Success;
                response.Message = "变更成功！";

            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.Exception;
                response.Message = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// 费用采购订单删除
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> CostPurchaseOrderDelete(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    response.Message = "参数信息不能为空";
                    response.Code = ResponseCode.NoExistsData;
                    return response;
                }
                CostPurchaseOrderDeleteRequest request = JsonConvertUtils.DeserializeObject<CostPurchaseOrderDeleteRequest>(message);

                if (string.IsNullOrEmpty(request.BillNo))
                {
                    response.Code = ResponseCode.ModelError;
                    response.Message = "请输入费用采购单号";
                    return response;
                }

                string sql = $@"select top 1 FID from T_AP_PAYABLEENTRY where FSOURCEBILLNO='{request.BillNo}' ";
                if (DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0) > 0)
                {
                    response.Code = ResponseCode.Exception;
                    response.Message = "已生成费用应付，不能删除！";
                    return response;
                }

                sql = $@"select top 1 FId from T_PUR_POORDER where FBILLNO='{request.BillNo}' ";
                long fId = DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0);
                if (fId == 0)
                {
                    response.Code = ResponseCode.Exception;
                    response.Message = "费用采购订单不存在！";
                    return response;
                }
                var billView = FormMetadataUtils.CreateBillView(ctx, "PUR_PurchaseOrder", fId);
                string fdocumentStatus = billView.Model.GetValue("FDOCUMENTSTATUS").ToString();

                if (fdocumentStatus.Equals("B") || fdocumentStatus.Equals("C"))
                {
                    var oper = MymoooBusinessDataServiceHelper.SetBillStatus(ctx, billView.BusinessInfo, new object[] { fId }, "UnAudit");
                    if (!oper.IsSuccess)
                    {
                        if (oper.ValidationErrors.Count > 0)
                        {
                            response.Message = string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                        }
                        else
                        {
                            response.Message = string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                        }
                        return response;
                    }
                }
                //删除
                var oper2 = MymoooBusinessDataServiceHelper.DeleteBill(ctx, billView.BusinessInfo, new object[] { fId });
                if (!oper2.IsSuccess)
                {
                    if (oper2.ValidationErrors.Count > 0)
                    {
                        response.Message = string.Join(";", oper2.ValidationErrors.Select(p => p.Message));
                    }
                    else
                    {
                        response.Message = string.Join(";", oper2.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                    }
                    return response;
                }
                response.Code = ResponseCode.Success;
                response.Message = "删除成功";
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.Exception;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
