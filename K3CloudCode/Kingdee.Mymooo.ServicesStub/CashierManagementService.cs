using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.Common;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.WebApi.ServicesStub
{
    public class CashierManagementService : KDBaseService
    {
        public CashierManagementService(KDServiceContext context) : base(context)
        {
        }

        /// <summary>
        /// 反审核应收单
        /// </summary>
        /// <returns></returns>
        public string UnAduitReceivebill()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ids = JsonConvertUtils.DeserializeObject<object[]>(data);
                var operateOption = OperateOption.Create();
                operateOption.SetIgnoreWarning(true);
                operateOption.SetVariableValue("RemoveValidators", true);
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                FormMetadata paybillMeta = (FormMetadata)MetaDataServiceHelper.Load(ctx, "AR_RECEIVEBILL");
                var oper = BusinessDataServiceHelper.UnAudit(ctx, paybillMeta.BusinessInfo, ids, operateOption);
                if (oper.IsSuccess)
                {
                    response.Code = ResponseCode.Success;
                }
                else
                {
                    response.Code = ResponseCode.ModelError;
                    if (oper.ValidationErrors.Count > 0)
                    {
                        response.ErrorMessage = string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                    }
                    else
                    {
                        response.ErrorMessage = string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                    }
                }
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>
                {
                    ErrorMessage = ex.Message,
                    Code = ResponseCode.Exception
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }

        /// <summary>
        /// 反审核付款单
        /// </summary>
        /// <returns></returns>
        public string UnAduitPaybill()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ids = JsonConvertUtils.DeserializeObject<object[]>(data);
                var operateOption = OperateOption.Create();
                operateOption.SetIgnoreWarning(true);
                operateOption.SetVariableValue("RemoveValidators", true);
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                FormMetadata paybillMeta = (FormMetadata)MetaDataServiceHelper.Load(ctx, "AP_PAYBILL");
                var oper = BusinessDataServiceHelper.UnAudit(ctx, paybillMeta.BusinessInfo, ids, operateOption);
                if (oper.IsSuccess)
                {
                    response.Code = ResponseCode.Success;
                }
                else
                {
                    response.Code = ResponseCode.ModelError;
                    if (oper.ValidationErrors.Count > 0)
                    {
                        response.ErrorMessage = string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                    }
                    else
                    {
                        response.ErrorMessage = string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                    }
                }
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>
                {
                    ErrorMessage = ex.Message,
                    Code = ResponseCode.Exception
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }

        /// <summary>
        /// 删除银行付款单
        /// </summary>
        /// <returns></returns>
        public string DeleteBankPay()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ids = JsonConvertUtils.DeserializeObject<object[]>(data);
                var operateOption = OperateOption.Create();
                operateOption.SetIgnoreWarning(true);
                operateOption.SetVariableValue("RemoveValidators", true);
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                FormMetadata paybillMeta = (FormMetadata)MetaDataServiceHelper.Load(ctx, "WB_BankPay");
                var oper = BusinessDataServiceHelper.Delete(ctx, paybillMeta.BusinessInfo, ids, operateOption);
                if (oper.IsSuccess)
                {
                    response.Code = ResponseCode.Success;
                }
                else
                {
                    response.Code = ResponseCode.ModelError;
                    if (oper.ValidationErrors.Count > 0)
                    {
                        response.ErrorMessage = string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                    }
                    else
                    {
                        response.ErrorMessage = string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                    }
                }
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>
                {
                    ErrorMessage = ex.Message,
                    Code = ResponseCode.Exception
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }

        /// <summary>
        /// 删除银行付款单 行
        /// </summary>
        /// <returns></returns>
        public string DeleteBankPayForEntry()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ids = JsonConvertUtils.DeserializeObject<object[]>(data);
                var operateOption = OperateOption.Create();
                operateOption.SetIgnoreWarning(true);
                operateOption.SetVariableValue("RemoveValidators", true);
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                var billView = FormMetadataUtils.CreateBillView(ctx, "WB_BankPay", ids[0]);
                for (int i = 1; i < ids.Length; i++)
                {
                    billView.Model.DeleteEntryRow("FReceiveEntry", Convert.ToInt32(ids[i]));
                }
                var oper = BusinessDataServiceHelper.Save(ctx, billView.BusinessInfo, billView.Model.DataObject, operateOption);
                if (oper.IsSuccess)
                {
                    response.Code = ResponseCode.Success;
                }
                else
                {
                    response.Code = ResponseCode.ModelError;
                    if (oper.ValidationErrors.Count > 0)
                    {
                        response.ErrorMessage = string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                    }
                    else
                    {
                        response.ErrorMessage = string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                    }
                }
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>
                {
                    ErrorMessage = ex.Message,
                    Code = ResponseCode.Exception
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }
    }
}
