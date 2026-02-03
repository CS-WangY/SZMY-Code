using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Kingdee.BOS;
using Kingdee.Mymooo.Core.PlanOrderManagement;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.BOS.Util;

namespace Kingdee.Mymooo.Business.PlugIn.PLN_Forecast
{
    public class PLN_ForecastBusiness
    {
        public ResponseMessage<dynamic> ClosedForecastOrderAction(Context ctx, ApprovalMessageRequest request)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (request.SpStatus != "2")
            {
                response.Code = ResponseCode.Abort;
                response.Message = "审批不通过！";
                return response;
            }
            var apiinfo = ApigatewayUtils.InvokeWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/Approval/K3CloudGetForecastApproval?applyeventno={request.ApplyeventNo}");
            var salinfo = JsonConvertUtils.DeserializeObject<ResponseMessage<K3CloudClosedForecastOrderRequest>>(apiinfo);
            response.Data = salinfo;
            var operateOption = OperateOption.Create();
            operateOption.SetIgnoreWarning(false);
            operateOption.SetValidateFlag(false);
            FormMetadata meta = MetaDataServiceHelper.Load(ctx, "PLN_Forecast") as FormMetadata;

            List<KeyValuePair<object, object>> pkEntryIds = null;

            foreach (var item in salinfo.Data.ForecastOrderEntrys)
            {
                object[] ids = new object[] { item };
                pkEntryIds = ids.Select((object x) => new KeyValuePair<object, object>(salinfo.Data.ForecastOrderID, x)).ToList();
                SetStatusService setStatusService = new SetStatusService();
                using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                {
                    var oper = setStatusService.SetBillStatus(ctx, meta.BusinessInfo, pkEntryIds, null, "BillClose", operateOption);
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
                    else
                    {
                        response.Code = ResponseCode.Success;
                        response.Message = "操作成功";
                    }
                    cope.Complete();
                }
            }

            return response;

        }
    }
}
