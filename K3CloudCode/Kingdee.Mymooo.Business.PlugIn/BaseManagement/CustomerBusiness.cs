using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.BusinessEntity.BusinessFlow;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.ServiceHelper.ManagementCenter;
using Kingdee.BOS.Util;
using Kingdee.BOS.Web.Bill;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.UI.WebControls.WebParts;

namespace Kingdee.Mymooo.Business.PlugIn.BaseManagement
{
    /// <summary>
    /// 同步客户信息
    /// </summary>
    public class CustomerBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            CustomerRequest request = JsonConvertUtils.DeserializeObject<CustomerRequest>(message);
            if (request == null || string.IsNullOrWhiteSpace(request.Code))
            {
                ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
                response.Message = "参数信息不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            return BasicDataSyncServiceHelper.AddOrMotifyCustomer(ctx, request);
        }
    }

    /// <summary>
    /// 批量同步客户等级
    /// </summary>
    public class SyncCustomerGrade : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            var request = JsonConvert.DeserializeObject<List<CustomerRequest>>(message);
            using (var cope = new KDTransactionScope(TransactionScopeOption.Required))
            {
                var pageSize = 500;
                var pageCount = request.Count / pageSize + (request.Count % pageSize > 0 ? 1 : 0);
                for (var j = 0; j < pageCount; j++)
                {
                    var items = request.Skip(j * pageSize).Take(pageSize).ToList();
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine($@"update T_BD_CUSTOMER set FPENYCUSTOMERLEVEL=
                                case FNUMBER");
                    foreach (var item in items)
                    {
                        sql.AppendLine($"when '{item.Code}' Then '{item.CustomerLevel}' ");
                    }
                    sql.AppendLine("else FPENYCUSTOMERLEVEL end");
                    DBServiceHelper.Execute(ctx, sql.ToString());
                }
                cope.Complete();
                response.Code = ResponseCode.Success;
                return response;
            }
        }
    }

    /// <summary>
    /// 启用/禁用客户
    /// </summary>
    public class EnableCustomerBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            EnableCustomer request = JsonConvertUtils.DeserializeObject<EnableCustomer>(message);
            if (request == null || string.IsNullOrEmpty(request.Code))
            {
                response.Message = "参数信息不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            var cust = FormMetadataUtils.GetIdForNumber(ctx, new CustomerInfo(request.Code, ""));
            if (cust.Id == 0)
            {
                response.Message = "客户不存在";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            var billView = FormMetadataUtils.CreateBillView(ctx, "BD_Customer", cust.Id.ToString());
            if (request.IsEnabled)
            {
                if (billView.Model.DataObject["ForbidStatus"].ToString() == "A")
                {
                    response.Message = "该客户已是启用状态";
                    response.Code = ResponseCode.Success;
                    return response;
                }
                string[] fid = new string[] { cust.Id.ToString() };
                var oper3 = MymoooBusinessDataServiceHelper.SetBillStatus(ctx, billView.BusinessInfo, fid, "Enable");
                if (!oper3.IsSuccess)
                {
                    if (oper3.ValidationErrors.Count > 0)
                    {
                        response.Message += string.Join(";", oper3.ValidationErrors.Select(p => p.Message));
                    }
                    else
                    {
                        response.Message += string.Join(";", oper3.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                    }
                }
            }
            else
            {
                if (billView.Model.DataObject["ForbidStatus"].ToString() == "B")
                {
                    response.Message = "该客户已是禁用状态";
                    response.Code = ResponseCode.Success;
                    return response;
                }
                string[] fid = new string[] { cust.Id.ToString() };
                var oper3 = MymoooBusinessDataServiceHelper.SetBillStatus(ctx, billView.BusinessInfo, fid, "Forbid");
                if (!oper3.IsSuccess)
                {
                    if (oper3.ValidationErrors.Count > 0)
                    {
                        response.Message += string.Join(";", oper3.ValidationErrors.Select(p => p.Message));
                    }
                    else
                    {
                        response.Message += string.Join(";", oper3.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                    }
                }
            }
            //清除释放网控
            billView.CommitNetworkCtrl();
            billView.InvokeFormOperation(FormOperationEnum.Close);
            billView.Close();
            response.Code = ResponseCode.Success;
            return response;
        }
    }
    /// <summary>
    /// 同步联系人信息
    /// </summary>
    public class LinkManBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            var request = JsonConvert.DeserializeObject<LinkManRequest>(message);

            if (request == null)
            {
                response.Message = "参数信息不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            string Id = string.Empty;
            string LinkCode = string.Empty;
            if (!string.IsNullOrWhiteSpace(request.CompanyCode))
            {
                if (string.IsNullOrWhiteSpace(request.CustCode))
                {
                    response.Code = ResponseCode.NoExistsData;
                    response.Message = "个人客户编码不存在";
                    return response;
                }
                var cust = FormMetadataUtils.GetIdForNumber(ctx, new CustomerInfo(request.CompanyCode, ""));
                if (cust.Id == 0)
                {
                    response.Code = ResponseCode.NoExistsData;
                    response.Message = "客户不存在";
                    return response;
                }

                Id = cust.Id.ToString();
                LinkCode = request.CompanyCode + "_" + request.CustCode;
            }
            else
            {
                var cust = FormMetadataUtils.GetIdForNumber(ctx, new CustomerInfo(request.CustCode, ""));
                if (cust.Id == 0)
                {
                    response.Code = ResponseCode.NoExistsData;
                    response.Message = "客户不存在";
                    return response;
                }
                Id = cust.Id.ToString();
                LinkCode = request.CustCode;

            }
            string sql = "select FCONTACTID from T_BD_COMMONCONTACT where FNUMBER =@code";
            int contactid = DBServiceHelper.ExecuteScalar(ctx, sql, 0, new SqlParam("@code", KDDbType.String, LinkCode));
            if (contactid > 0)
            {
                var linkBillView = FormMetadataUtils.CreateBillView(ctx, "BD_CustContact", contactid);
                linkBillView.Model.SetValue("FNumber", LinkCode);
                linkBillView.Model.SetValue("FName", request.Name);
                linkBillView.Model.SetValue("FMobile", request.Mobile);
                linkBillView.Model.SetValue("FCompanyType", "BD_Customer");
                linkBillView.Model.SetValue("FForbidStatus", request.IsValid ? "A" : "B");
                linkBillView.Model.SetValue("FCustId", Id);
                linkBillView.Model.SetValue("FCompany", Id);
                linkBillView.Model.SetValue("FBizAddress", request.Address);
                linkBillView.Model.SetValue("FEmail", request.Email);
                if (!string.IsNullOrWhiteSpace(request.Sex))
                {
                    linkBillView.Model.SetValue("Fex", request.Sex == "男" ? "eb28b65afd62405299f061c9173a81de" : "b589b771e6004d2282fddb03accc7b43");
                }

                linkBillView.Model.SetValue("FPost", request.ProfessionName);

                var oper = MymoooBusinessDataServiceHelper.SaveBill(ctx, linkBillView.BusinessInfo, linkBillView.Model.DataObject);
                //清除释放网控
                linkBillView.CommitNetworkCtrl();
                linkBillView.InvokeFormOperation(FormOperationEnum.Close);
                linkBillView.Close();
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
            }
            else
            {
                var linkBillView = FormMetadataUtils.CreateBillView(ctx, "BD_CustContact");
                linkBillView.Model.SetValue("FNumber", LinkCode);
                linkBillView.Model.SetValue("FName", request.Name);
                linkBillView.Model.SetValue("FMobile", request.Mobile);
                linkBillView.Model.SetValue("FCompanyType", "BD_Customer");
                linkBillView.Model.SetValue("FForbidStatus", request.IsValid ? "A" : "B");
                linkBillView.Model.SetValue("FCustId", Id);
                linkBillView.Model.SetValue("FCompany", Id);
                linkBillView.Model.SetValue("FBizAddress", request.Address);
                linkBillView.Model.SetValue("FEmail", request.Email);
                if (!string.IsNullOrWhiteSpace(request.Sex))
                {
                    linkBillView.Model.SetValue("Fex", request.Sex == "男" ? "eb28b65afd62405299f061c9173a81de" : "b589b771e6004d2282fddb03accc7b43");
                }

                linkBillView.Model.SetValue("FPost", request.ProfessionName);

                var oper = MymoooBusinessDataServiceHelper.SaveBill(ctx, linkBillView.BusinessInfo, linkBillView.Model.DataObject);

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
                FormMetadata meta = MetaDataServiceHelper.Load(ctx, "BD_CommonContact") as FormMetadata;
                var oper2 = MymoooBusinessDataServiceHelper.Audit(ctx, meta.BusinessInfo, new object[] { oper.OperateResult[0].PKValue });
                //清除释放网控
                linkBillView.CommitNetworkCtrl();
                linkBillView.InvokeFormOperation(FormOperationEnum.Close);
                linkBillView.Close();
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
                }
            }
            response.Code = ResponseCode.Success;
            response.Data = string.Empty;
            return response;
        }

        /// <summary>
        /// 更新历史联系人审核状态
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> UpHistoryLinkManAudit(Context ctx)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();

            string sql = $@"/*dialect*/select FCONTACTID from T_BD_COMMONCONTACT where isnull(FDOCUMENTSTATUS,'')='' and FFORBIDSTATUS='A' and FCOMPANYTYPE='BD_Customer' ";
            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            var operateOption = OperateOption.Create();
            operateOption.SetIgnoreWarning(true);
            FormMetadata meta = MetaDataServiceHelper.Load(ctx, "BD_CommonContact") as FormMetadata;
            List<object> ids = new List<object>();
            int i = 1;
            foreach (var data in datas)
            {
                ids.Add(data["FCONTACTID"]);
                if (i % 10000 == 0)
                {
                    var oper = MymoooBusinessDataServiceHelper.Audit(ctx, meta.BusinessInfo, ids.ToArray());
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
                    }
                    ids.Clear();
                    i = 1;
                }
                i += 1;
            }

            response.Code = ResponseCode.Success;
            response.Data = string.Empty;
            return response;
        }
    }


    /// <summary>
    /// 删除联系人
    /// </summary>
    public class DeleteLinkManBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            var request = JsonConvert.DeserializeObject<LinkManRequest>(message);
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (string.IsNullOrWhiteSpace(message))
            {
                response.Message = "地址ID不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            string LinkCode = string.Empty;
            if (!string.IsNullOrWhiteSpace(request.CompanyCode))
            {
                if (string.IsNullOrWhiteSpace(request.CustCode))
                {
                    response.Code = ResponseCode.NoExistsData;
                    response.Message = "个人客户编码不存在";
                    return response;
                }
                LinkCode = request.CompanyCode + "_" + request.CustCode;
            }
            else
            {
                LinkCode = request.CustCode;
            }
            string sql = "select FCONTACTID from T_BD_COMMONCONTACT where FNUMBER =@code";
            int contactid = DBServiceHelper.ExecuteScalar(ctx, sql, 0, new SqlParam("@code", KDDbType.String, LinkCode));
            if (contactid > 0)
            {
                FormMetadata meta = MetaDataServiceHelper.Load(ctx, "BD_CustContact") as FormMetadata;
                var oper = MymoooBusinessDataServiceHelper.DeleteBill(ctx, meta.BusinessInfo, contactid);
                if (!oper.IsSuccess)
                {
                    if (oper.ValidationErrors.Count > 0)
                    {
                        response.Message += string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                    }
                    else
                    {
                        response.Message += string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                    }
                    response.Code = ResponseCode.ModelError;
                    return response;
                }
            }
            response.Code = ResponseCode.Success;
            return response;
        }
    }
    /// <summary>
    /// 同步地址信息
    /// </summary>
    public class CustAddressBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            var request = JsonConvert.DeserializeObject<AddressRequest>(message);
            if (request == null)
            {
                response.Message = "参数信息不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                response.Code = ResponseCode.NoExistsData;
                response.Message = "编码不存在";
                return response;
            }
            var cust = FormMetadataUtils.GetIdForNumber(ctx, new CustomerInfo(request.Code, ""));
            if (cust.Id == 0)
            {
                response.Code = ResponseCode.NoExistsData;
                response.Message = "客户不存在";
                return response;
            }
            //如果是存在默认，则取消其他地址的默认
            if (request.IsDefault == 1)
            {
                string sql = $"/*dialect*/update t1 set t1.FIsDefaultConsignee=0 from T_BD_CUSTLOCATION t1,T_BD_CUSTOMER t2 where t1.FCUSTID=t2.FCUSTID and t2.FNUMBER='{request.Code}' ";
                DBServiceHelper.Execute(ctx, sql);
            }

            var billView = FormMetadataUtils.CreateBillView(ctx, "BD_Customer", cust.Id.ToString());
            var custDynamic = billView.Model.DataObject["BD_CUSTCONTACT"] as DynamicObjectCollection;
            var addressNews = custDynamic.FirstOrDefault(it => it["NUMBER"].ToString() == request.AddressId.ToString());
            if (addressNews != null)
            {
                addressNews["ADDRESS"] = request.Address;
                addressNews["NUMBER"] = request.AddressId.ToString();
                addressNews["RECEIVER"] = request.Receiver;
                addressNews["MOBILE"] = request.Mobile;
                addressNews["IsDefaultConsignee"] = request.IsDefault;
            }
            else
            {
                billView.Model.CreateNewEntryRow("FT_BD_CUSTCONTACT");
                billView.Model.SetValue("FADDRESS1", request.Address, custDynamic.Count - 1);
                billView.Model.SetValue("FNUMBER1", request.AddressId, custDynamic.Count - 1);
                billView.Model.SetValue("FRECEIVER", request.Receiver, custDynamic.Count - 1);
                billView.Model.SetValue("FMOBILE", request.Mobile, custDynamic.Count - 1);
                billView.Model.SetValue("FIsDefaultConsignee", request.IsDefault, custDynamic.Count - 1);
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
                    throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
                }
                else
                {
                    throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                }
            }
            response.Code = ResponseCode.Success;
            response.Data = string.Empty;
            return response;
        }
    }

    /// <summary>
    /// 删除地址
    /// </summary>
    public class DeleteCustAddressBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            var request = JsonConvert.DeserializeObject<AddressRequest>(message);
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            if (request.AddressId == 0)
            {
                response.Code = ResponseCode.NoExistsData;
                response.Message = "ID不能为空";
            }
            try
            {

                string sql = $"/*dialect*/Delete T_BD_CUSTLOCATION where FNUMBER ={request.AddressId}";
                DBServiceHelper.Execute(ctx, sql);
                response.Code = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.ModelError;
                response.Message = ex.Message;
                return response;
            }
        }
    }

    /// <summary>
    /// 同步结算方式
    /// </summary>
    public class CustPaymothod : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            var request = JsonConvert.DeserializeObject<PayWayRequest>(message);
            if (request == null)
            {
                response.Message = "参数信息不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                response.Code = ResponseCode.NoExistsData;
                response.Message = "编码不存在";
                return response;
            }
            var cust = FormMetadataUtils.GetIdForNumber(ctx, new CustomerInfo(request.Code, ""));
            if (cust.Id == 0)
            {
                response.Code = ResponseCode.NoExistsData;
                response.Message = "客户不存在";
                return response;
            }
            var payway = FormMetadataUtils.GetIdForNumber(ctx, new PayWayInfo(request.PayWayCode, ""));
            var payMethod = FormMetadataUtils.GetIdForNumber(ctx, new PayMethodInfo(request.PayMothodCode, ""));
            var billView = FormMetadataUtils.CreateBillView(ctx, "BD_Customer", cust.Id.ToString());

            billView.Model.SetValue("FSETTLETYPEID", payway.Id);
            billView.Model.SetValue("FRECCONDITIONID", payMethod.Id);
            billView.Model.SetValue("FReconciliationTime", request.ReconctliationTime);
            billView.Model.SetValue("FInvoicingTime", request.InvoicingTime);
            billView.Model.SetValue("FPaymentTime", request.PaymentTime);
            var oper = MymoooBusinessDataServiceHelper.SaveBill(ctx, billView.BusinessInfo, billView.Model.DataObject);
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
            response.Code = ResponseCode.Success;
            response.Data = string.Empty;
            return response;
        }
    }

    /// <summary>
    /// 同步客户发票信息
    /// </summary>
    public class CustInvoiceBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            var request = JsonConvert.DeserializeObject<InvoicingRequest>(message);
            if (request == null || string.IsNullOrWhiteSpace(request?.CompanyCode))
            {
                response.Message = "参数信息不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            var cust = FormMetadataUtils.GetIdForNumber(ctx, new CustomerInfo(request.CompanyCode, ""));
            if (cust.Id > 0)
            {
                var billView = FormMetadataUtils.CreateBillView(ctx, "BD_Customer", cust.Id.ToString());
                billView.Model.SetValue("FINVOICEADDRESS", request.Address);
                billView.Model.SetValue("FINVOICETEL", request.Telephone);
                billView.Model.SetValue("FINVOICEBANKNAME", request.Bank);
                billView.Model.SetValue("FINVOICEBANKACCOUNT", request.BankAccount);
                billView.Model.SetValue("FCustInvoiceType", request.InvoiceType);
                billView.Model.SetValue("FTAXREGISTERCODE", request.TaxpayerCode);
                billView.Model.SetValue("FSendAddress", request.ReceiveAddres);
                billView.Model.SetValue("FInvoiceRecipient", request.Receiver);
                billView.Model.SetValue("FINVOICETITLE", request.InvoiceName);
                var oper = MymoooBusinessDataServiceHelper.SaveBill(ctx, billView.BusinessInfo, billView.Model.DataObject);
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
                response.Code = ResponseCode.Success;
                return response;
            }
            else
            {
                response.Code = ResponseCode.NoExistsData;
                response.Message = "客户信息不存在";
                return response;
            }
        }
    }

    /// <summary>
    /// 同步供应商银行信息
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public class SupplierBankBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            var request = JsonConvert.DeserializeObject<SupplierBankRequest>(message);
            //if (string.IsNullOrWhiteSpace(request.BankName) && string.IsNullOrWhiteSpace(request.BankCode))
            //{
            //    response.Message = "同步信息不能为空";
            //    response.Code = ResponseCode.ModelError;
            //    return response;
            //}

            if (request.SupplierBanks.Count == 0)
            {
                response.Message = "同步信息不能为空";
                response.Code = ResponseCode.ModelError;
                return response;
            }
            int supplierId = 0;
            string sql = "/*dialect*/select top 1 FSupplierId from t_BD_Supplier where FNUMBER=@code and FCREATEORGID=1 and FUSEORGID=1";
            using (var reader = DBServiceHelper.ExecuteReader(ctx, sql, new List<SqlParam> { new SqlParam("@code", KDDbType.String, request.Code) }))
            {
                if (reader.Read())
                {
                    supplierId = Convert.ToInt32(reader["FSupplierId"]);
                }
            }
            if (supplierId == 0)
            {
                response.Message += "供应商信息不存在";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            if (supplierId > 0)
            {
                sql = $@"/*dialect*/delete from T_BD_SUPPLIERBANK_L where FBANKID in (select FBANKID from T_BD_SUPPLIERBANK where FSUPPLIERID={supplierId}) ";
                DBServiceHelper.Execute(ctx, sql);
                sql = $@"/*dialect*/delete from T_BD_SUPPLIERBANK where FSUPPLIERID={supplierId} ";
                DBServiceHelper.Execute(ctx, sql);

                var billView = FormMetadataUtils.CreateBillView(ctx, "BD_Supplier", supplierId);
                //billView.Model.SetValue("FOpenBankName", request.BankName, 0);
                //billView.Model.SetValue("FBankIsDefault", 1, 0);
                //billView.Model.SetValue("FBANKCODE", request.BankCode.Replace(" ", "").Replace("\r\n", "").Replace("\t", ""), 0);
                var entrys = billView.Model.GetEntityDataObject(billView.BusinessInfo.GetEntity("FBankInfo"));
                int count = 0;
                foreach (var item in request.SupplierBanks)
                {
                    if (count > 0)
                    {
                        billView.Model.CreateNewEntryRow("FBankInfo");
                    }
                    billView.Model.SetValue("FOpenBankName", item.OpenBankName, count);//开户银行名称
                    billView.Model.SetValue("FBankIsDefault", item.IsDefault, count);//是否默认
                    billView.Model.SetValue("FBankHolder", item.BankAccountName, count);//账户名称
                    billView.Model.SetValue("FBankCode", item.BankCode.Replace(" ", "").Replace("\r\n", "").Replace("\t", ""), count);//银行账号
                    //billView.Model.SetValue("FCNAPS", item.BankLineNumber, count);//联行号
                    billView.Model.SetItemValueByNumber("FBankDetail", item.BankLineNumber, count);//银行网点
                    billView.InvokeFieldUpdateService("FBankDetail", count);
                    billView.Model.SetValue("FOpenAddressRec", item.OpenBankAddress, count);//开户行地址
                    count += 1;
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
                //发送MES
                SupplyOrgServiceHelper.SenMesSupplyInfo(ctx, request.Code);
            }
            //else
            //{
            //    var billView = FormMetadataUtils.CreateBillView(ctx, "BD_Supplier");
            //    billView.Model.SetValue("FOpenBankName", request.BankName, 0);
            //    billView.Model.SetValue("FBANKCODE", request.BankCode.Replace(" ", "").Replace("\r\n", "").Replace("\t", ""), 0);
            //    var oper = MymoooBusinessDataServiceHelper.SaveBill(ctx, billView.BusinessInfo, billView.Model.DataObject);
            //    billView.Close();
            //    if (!oper.IsSuccess)
            //    {
            //        if (oper.ValidationErrors.Count > 0)
            //        {
            //            response.Message += string.Join(";", oper.ValidationErrors.Select(p => p.Message));
            //            response.Code = ResponseCode.ModelError;
            //            return response;
            //        }
            //        else
            //        {
            //            response.Message += string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
            //            response.Code = ResponseCode.ModelError;
            //            return response;
            //        }
            //    }
            //}

            response.Code = ResponseCode.Success;
            return response;
        }
    }


    /// <summary>
    /// 同步供应商联系人信息
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public class SupplierContantBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            var request = JsonConvert.DeserializeObject<SupplierContact>(message);
            int supplierId = 0;
            string sql = "select top 1 FSupplierId from t_BD_Supplier where FNUMBER=@code and FCREATEORGID=1 and FUSEORGID=1 ";
            using (var reader = DBServiceHelper.ExecuteReader(ctx, sql, new List<SqlParam> { new SqlParam("@code", KDDbType.String, request.Code) }))
            {
                if (reader.Read())
                {
                    supplierId = Convert.ToInt32(reader["FSupplierId"]);
                }
            }
            if (supplierId == 0)
            {
                response.Message += "供应商信息不存在";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            var billView = FormMetadataUtils.CreateBillView(ctx, "BD_Supplier", supplierId);
            var Contact = billView.Model.DataObject["SupplierContact"] as DynamicObjectCollection;
            var contactValue = Contact.FirstOrDefault(it => it["ContactNumber"].ToString() == request.Id);
            if (contactValue != null)
            {
                contactValue["Contact"] = request.Name;
                contactValue["ContactNumber"] = request.Id;
                contactValue["FPost"] = request.Post;
                contactValue["Tel"] = request.Tel;
                contactValue["EMail"] = request.Email;
                contactValue["Fax"] = request.Fax;
                contactValue["Mobile"] = request.Mobile;
                contactValue["Gender_Id"] = request.Sex == "1" ? "eb28b65afd62405299f061c9173a81de" : "b589b771e6004d2282fddb03accc7b43";
            }
            else
            {
                billView.Model.CreateNewEntryRow("FSupplierContact");
                billView.Model.SetValue("FContact", request.Name, Contact.Count - 1);
                billView.Model.SetValue("FContactNumber", request.Id, Contact.Count - 1);
                billView.Model.SetValue("FPost", request.Post, Contact.Count - 1);
                billView.Model.SetValue("FTel", request.Tel, Contact.Count - 1);
                billView.Model.SetValue("FEMail", request.Email, Contact.Count - 1);
                billView.Model.SetValue("FFax", request.Fax, Contact.Count - 1);
                billView.Model.SetValue("FMobile", request.Mobile, Contact.Count - 1);
                billView.Model.SetValue("FGender", request.Sex == "1" ? "eb28b65afd62405299f061c9173a81de" : "b589b771e6004d2282fddb03accc7b43", Contact.Count - 1);
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

            //发送MES
            SupplyOrgServiceHelper.SenMesSupplyInfo(ctx, request.Code);
            return response;
        }
    }

    /// <summary>
    /// 删除供应商联系人
    /// </summary>
    public class DeleteSupplierContantBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            var request = JsonConvert.DeserializeObject<SupplierContact>(message);
            if (request == null || string.IsNullOrEmpty(request.Id))
            {
                response.Code = ResponseCode.Success;
                response.Message = "参数不能为空";
                return response;
            }

            try
            {
                //获取到供应商的编码(华东五部)
                string sql = $@"/*dialect*/select top 1 t2.FNUMBER from T_BD_SUPPLIERCONTACT t1
                                inner join T_BD_SUPPLIER t2 on t1.FSUPPLIERID=t2.FSUPPLIERID
                                where FCONTACTNUMBER='{request.Id}' and  FUSEORGID=7401803";
                string supplierCode = DBServiceHelper.ExecuteScalar<string>(ctx, sql, "");
                //删除联系人
                sql = $"/*dialect*/delete T_BD_SUPPLIERCONTACT where FCONTACTNUMBER = '{request.Id}'";
                DBServiceHelper.Execute(ctx, sql);
                response.Code = ResponseCode.Success;
                if (!string.IsNullOrEmpty(supplierCode))
                {
                    //发送MES
                    SupplyOrgServiceHelper.SenMesSupplyInfo(ctx, supplierCode);
                }
                return response;
            }
            catch (Exception ex)
            {

                response.Code = ResponseCode.ModelError;
                response.Message = ex.Message;
                return response;
            }
            //string sql = $"delete T_BD_SUPPLIERCONTACT where FCONTACTNUMBER='{request.Id}' ";
            //DBServiceHelper.Execute(ctx, sql);
            //FormMetadata meta = MetaDataServiceHelper.Load(ctx, "BD_CustContact") as FormMetadata;
            //var oper = MymoooBusinessDataServiceHelper.DeleteBill(ctx, meta.BusinessInfo, supplierId);
            //if (!oper.IsSuccess)
            //{
            //    if (oper.ValidationErrors.Count > 0)
            //    {
            //        response.Message += string.Join(";", oper.ValidationErrors.Select(p => p.Message));
            //    }
            //    else
            //    {
            //        response.Message += string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
            //    }
            //    response.Code = ResponseCode.ModelError;
            //    return response;
            //}

        }

        /// <summary>
        /// 启用/禁用供应商
        /// </summary>
        public class EnableSupplierBusiness : IMessageExecute
        {
            public ResponseMessage<dynamic> Execute(Context ctx, string message)
            {
                ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
                EnableSupplier request = JsonConvertUtils.DeserializeObject<EnableSupplier>(message);
                if (request == null || string.IsNullOrEmpty(request.Code))
                {
                    response.Message = "参数信息不能为空";
                    response.Code = ResponseCode.NoExistsData;
                    return response;
                }

                List<SqlParam> paramList = new List<SqlParam>()
                {
                    new SqlParam("@number", KDDbType.String, request.Code)
                };

                string sql = @"select FSUPPLIERID from T_BD_SUPPLIER where FNUMBER=@number order by FUSEORGID ";
                var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: paramList.ToArray());
                List<string> fidList = new List<string>();
                foreach (var item in datas)
                {
                    fidList.Add(Convert.ToString(item["FSUPPLIERID"]));
                }
                foreach (var SupplierId in fidList)
                {
                    var billView = FormMetadataUtils.CreateBillView(ctx, "BD_Supplier", SupplierId);
                    if (request.IsEnabled)
                    {
                        if (billView.Model.GetValue("FForbidStatus").Equals("A"))
                        {
                            continue;
                        }
                        string[] fid = new string[] { SupplierId };
                        var oper = MymoooBusinessDataServiceHelper.SetBillStatus(ctx, billView.BusinessInfo, fid, "Enable");
                        if (!oper.IsSuccess)
                        {
                            if (oper.ValidationErrors.Count > 0)
                            {
                                response.Message += string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                            }
                            else
                            {
                                response.Message += string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                            }
                            response.Code = ResponseCode.NoExistsData;
                            return response;
                        }
                    }
                    else
                    {
                        if (billView.Model.GetValue("FForbidStatus").Equals("B"))
                        {
                            continue;
                        }
                        string[] fid = new string[] { SupplierId };
                        var oper = MymoooBusinessDataServiceHelper.SetBillStatus(ctx, billView.BusinessInfo, fid, "Forbid");
                        if (!oper.IsSuccess)
                        {
                            if (oper.ValidationErrors.Count > 0)
                            {
                                response.Message += string.Join(";", oper.ValidationErrors.Select(p => p.Message));
                            }
                            else
                            {
                                response.Message += string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
                            }
                            response.Code = ResponseCode.NoExistsData;
                            return response;
                        }
                    }
                    //清除释放网控
                    billView.CommitNetworkCtrl();
                    billView.InvokeFormOperation(FormOperationEnum.Close);
                    billView.Close();
                }
                //发送MES
                SupplyOrgServiceHelper.SenMesSupplyInfo(ctx, request.Code);
                response.Code = ResponseCode.Success;
                return response;
            }
        }

    }



}
