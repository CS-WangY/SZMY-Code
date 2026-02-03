using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Msg;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.App.Core.KafkaProducer;
using Kingdee.Mymooo.Business.PlugIn.PurchaseManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace Kingdee.Mymooo.Business.PlugIn.BaseManagement
{
    public class CustomerServcie
    {
        public CustomerInfo TryGetOrAdd(Context ctx, CustomerInfo customerInfo)
        {
            customerInfo = FormMetadataUtils.GetIdForNumber(ctx, customerInfo);
            if (customerInfo.Id == 0)
            {
                var billView = FormMetadataUtils.CreateBillView(ctx, "BD_Customer");
                billView.Model.SetValue("FNumber", customerInfo.Code);
                billView.Model.SetValue("FName", customerInfo.Name);

                var oper = MymoooBusinessDataServiceHelper.SaveAndAuditBill(ctx, billView.BusinessInfo, billView.Model.DataObject);
                //清除释放网控
                billView.CommitNetworkCtrl();
                billView.InvokeFormOperation(FormOperationEnum.Close);
                billView.Close();
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
                customerInfo.Id = Convert.ToInt64(billView.Model.DataObject["Id"]);
            }
            return customerInfo;
        }
        /// <summary>
        /// 新增客户
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ResponseMessage<dynamic> AddOrMotifyCustomer(Context ctx, string message)
        {
            var request = JsonConvertUtils.DeserializeObject<CustomerRequest>(message);
            string key = "";
            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                key = request.Code;
            }
            return AddRabbitMessage(ctx, message, "Addcustomer", key);
        }
        /// <summary>
        /// 新增/修改联系人
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ResponseMessage<dynamic> AddorMotifyLinkMan(Context ctx, string message)
        {
            var request = JsonConvert.DeserializeObject<LinkManRequest>(message);
            string key = "";
            if (!string.IsNullOrWhiteSpace(request.CompanyCode))
            {
                key = request.CompanyCode + "_" + request.CustCode;
            }
            else
            {
                key = request.CustCode;
            }
            return AddRabbitMessage(ctx, message, "AddLinkMan", key);
        }

        /// <summary>
        /// 新增/修改 客户地址
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ResponseMessage<dynamic> AddOrMotifyAddress(Context ctx, string message)
        {
            var request = JsonConvert.DeserializeObject<AddressRequest>(message);
            long? key = null;
            if (request.AddressId > 0)
            {
                key = request.AddressId;
            }

            return AddRabbitMessage(ctx, message, "AddCustAddress", key?.ToString());
        }

        /// <summary>
        /// 新增/修改 结算方式
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns> 
        /// <exception cref="Exception"></exception>
        public ResponseMessage<dynamic> AddOrMotifyPayWay(Context ctx, string message)
        {
            var request = JsonConvert.DeserializeObject<PayWayRequest>(message);
            string key = "";
            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                key = request.Code;
            }

            return AddRabbitMessage(ctx, message, "AddCustPaymothod", key);
        }

        /// <summary>
        /// 新增/修改发票信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ResponseMessage<dynamic> AddOrMotifyInvoiceNews(Context ctx, string message)
        {
            var request = JsonConvert.DeserializeObject<InvoicingRequest>(message);
            string key = "";
            if (!string.IsNullOrWhiteSpace(request.CompanyCode))
            {
                key = request.CompanyCode;
            }
            return AddRabbitMessage(ctx, message, "AddInvoice", key);
        }

        /// <summary>
        /// 同步通讯录
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> SyncDeptAndUserNews(Context ctx, string message)
        {
            List<WXUserRequest> list = JsonConvertUtils.DeserializeObject<List<WXUserRequest>>(message);
            string versions = "";
            if (list.FirstOrDefault()?.Versions != null)
            {
                versions = list.FirstOrDefault()?.Versions.ToString();
            }
            return AddRabbitMessage(ctx, message, "DeptAndUser", versions);
        }

        /// <summary>
        /// 同步客户与销售员的绑定关系
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> SyncSalesCust(Context ctx, string message)
        {
            SalesCustRequest list = JsonConvertUtils.DeserializeObject<SalesCustRequest>(message);
            string key = "";
            if (list != null)
            {
                key = list.CustCode;
            }
            return AddRabbitMessage(ctx, message, "SalesCust", key);
        }
        /// <summary>
        /// 同步供应商信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> SyncSupplier(Context ctx, string message)
        {
            var request = JsonConvert.DeserializeObject<SupplierRequest>(message);
            string key = "";
            if (request != null)
            {
                key = request.Code;
            }
            return AddRabbitMessage(ctx, message, "AddSupplier", key);
        }

        /// <summary>
        /// 同步供应商银行信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> SyncSupplierBank(Context ctx, string message)
        {
            var request = JsonConvert.DeserializeObject<SupplierRequest>(message);
            string key = "";
            if (request != null)
            {
                key = request.Code;
            }
            return AddRabbitMessage(ctx, message, "AddSupplierBank", key);
        }

        /// <summary>
        /// 同步供应商联系人信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> SyncSupplierContant(Context ctx, string message)
        {
            var request = JsonConvert.DeserializeObject<SupplierContact>(message);
            string key = "";
            if (request != null)
            {
                key = request.Code;
            }
            return AddRabbitMessage(ctx, message, "AddSupplierContant", key);
        }

        /// <summary>
        /// 供应商分配组织
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> SyncSupplierAllotOrg(Context ctx, string message)
        {
            var request = JsonConvert.DeserializeObject<SupplierAllotOrgRequest>(message);
            string key = "";
            if (request != null)
            {
                key = request.CompanyCode + '-' + request.Code;
            }
            return AddRabbitMessage(ctx, message, "SupplierAllotOrg", key);
        }
        public void SyncSupplierAllotOrgV2(Context ctx, List<SupplierAllotOrgRequest> message)
        {
            KafkaProducerService kafkaProducer = new KafkaProducerService();
            List<RabbitMQMessage> messages = new List<RabbitMQMessage>
                {
                    new RabbitMQMessage()
                    {
                        Exchange = "supplierManagement",
                        Routingkey = "AllocateSupplier",
                        Keyword = "",
                        Message = JsonConvertUtils.SerializeObject(message)
                    }
                };
            kafkaProducer.AddMessage(ctx, messages.ToArray());
        }

        /// <summary>
        /// 删除联系人
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> DeleteLinkMan(Context ctx, string message)
        {
            var request = JsonConvert.DeserializeObject<LinkManRequest>(message);
            string key = "";
            if (!string.IsNullOrWhiteSpace(request.CompanyCode))
            {
                key = request.CompanyCode + "_" + request.CustCode;
            }
            else
            {
                key = request.CustCode;
            }
            return AddRabbitMessage(ctx, message, "DeleteLinkMan", key);
        }

        /// <summary>
        /// 删除收货地址
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> DeleteCustAddress(Context ctx, string message)
        {
            var request = JsonConvert.DeserializeObject<AddressRequest>(message);
            long? key = null;
            if (request.AddressId > 0)
            {
                key = request.AddressId;
            }
            return AddRabbitMessage(ctx, message, "DeleteCustAddress", key?.ToString());
        }

        /// <summary>
        /// 删除供应商联系人
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> DeleteSupplierContant(Context ctx, string message)
        {
            var request = JsonConvert.DeserializeObject<SupplierContact>(message);
            string key = "";
            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                key = request.Code;
            }
            return AddRabbitMessage(ctx, message, "DeleteSupplierContant", key);
        }
        /// <summary>
        /// 启用/禁用客户
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> EnableCustomer(Context ctx, string message)
        {
            var request = JsonConvertUtils.DeserializeObject<EnableCustomer>(message);
            string key = "";
            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                key = request.Code;
            }
            return AddRabbitMessage(ctx, message, "EnableCustomer", key);
        }

        public ResponseMessage<dynamic> EnableSupplier(Context ctx, string message)
        {
            var request = JsonConvertUtils.DeserializeObject<EnableSupplier>(message);
            string key = "";
            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                key = request.Code;
            }
            return AddRabbitMessage(ctx, message, "EnableSupplier", key);
        }

        private ResponseMessage<dynamic> AddRabbitMessage(Context ctx, string message, string action, string keyword = "")
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            List<SqlParam> paramList = new List<SqlParam>()
                {
                new SqlParam("@message", KDDbType.String,message)
                };
            string sql = $"/*dialect*/ insert into RabbitMQScheduledMessage (FAction,FKeyword,FGuid,FMessage,FCreateDate) values  ('{action}','{keyword}','',@message,SYSDATETIME())";

            int i = DBServiceHelper.Execute(ctx, sql, paramList);
            if (i > 0)
            {
                response.Code = ResponseCode.Success;
                return response;
            }
            else
            {
                response.Code = ResponseCode.Exception;
                response.Message = "执行失败";
                return response;
            }
        }
    }
}
