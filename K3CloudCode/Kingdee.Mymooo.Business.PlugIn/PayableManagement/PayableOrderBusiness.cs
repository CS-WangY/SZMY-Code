
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.Mymooo.Core.PayableManagement;
using Kingdee.Mymooo.ServiceHelper.PayableManagement;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;

namespace Kingdee.Mymooo.Business.PlugIn.PayableManagement
{
    /// <summary>
    /// 应付服务
    /// </summary>
    public class PayableOrderBusiness
    {
        /// <summary>
        /// MES费用采购下推费用应付
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> MesCostPurGenerateCostPayable(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (string.IsNullOrWhiteSpace(message))
            {
                response.Message = "参数信息不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            MesCostPurGenerateCostPayableRequest request = JsonConvertUtils.DeserializeObject<MesCostPurGenerateCostPayableRequest>(message);
            if (string.IsNullOrWhiteSpace(request.PayBillNo))
            {
                response.Message = "费用应付单据编号不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            return PayableOrderServiceHelper.MesCostPurGenerateCostPayableService(ctx, request);
        }

        /// <summary>
        /// 费用应付删除
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> CostPayableOrderDelete(Context ctx, string message)
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
                CostPayableOrderDeleteRequest request = JsonConvertUtils.DeserializeObject<CostPayableOrderDeleteRequest>(message);
                if (string.IsNullOrEmpty(request.BillNo))
                {
                    response.Code = ResponseCode.ModelError;
                    response.Message = "请输入费用应付单号";
                    return response;
                }

                string sql = $@"select top 1 FId from T_AP_PAYABLE where FBILLNO='{request.BillNo}' ";
                long fId = DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0);
                if (fId == 0)
                {
                    response.Code = ResponseCode.Exception;
                    response.Message = "费用应付订单不存在！";
                    return response;
                }
                var billView = FormMetadataUtils.CreateBillView(ctx, "AP_Payable", fId);
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
