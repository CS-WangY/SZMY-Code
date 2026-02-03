using Kingdee.BOS.Core.Msg;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.Mymooo.App.Core.BaseManagement;
using Kingdee.Mymooo.Business.PlugIn.BaseManagement;
using Kingdee.Mymooo.Business.PlugIn.PurchaseManagement;
using Kingdee.Mymooo.Business.PlugIn.SalesManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using static Kingdee.Mymooo.Business.PlugIn.BaseManagement.DeleteSupplierContantBusiness;

namespace Kingdee.Mymooo.WebApi.ServicesStub
{
    public class CustomerManagermentService : KDBaseService
    {
        public CustomerManagermentService(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 同步客户信息
        /// </summary>
        /// <returns></returns>
        public string SyncCustomer()
        {
            return SyncFactory("Addcustomer");
        }
        /// <summary>
        /// 同步联系人信息
        /// </summary>
        /// <returns></returns>
        public string SyncCUstomerLinkMan()
        {
            return SyncFactory("AddLinkMan");
        }
        /// <summary>
        /// 同步地址信息
        /// </summary>
        /// <returns></returns>
        public string SyncCUstomerAddress()
        {
            return SyncFactory("AddCustAddress");
        }
        /// <summary>
        /// 同步结算方式
        /// </summary>
        /// <returns></returns>
        public string SyncPayMethod()
        {
            return SyncFactory("AddCustPaymothod");
        }

        /// <summary>
        /// 同步发票信息
        /// </summary>
        /// <returns></returns>
        public string SyncInvoice()
        {
            return SyncFactory("AddInvoice");
        }

        /// <summary>
        /// 同步部门用户信息
        /// </summary>
        /// <returns></returns>
        public string SysDeptAndUserNews()
        {
            return SyncFactory("DeptAndUser");
        }
        /// <summary>
        /// 同步客户与销售员的绑定关系
        /// </summary>
        /// <returns></returns>
        public string SyncSalesCust()
        {
            return SyncFactory("SalesCust");
        }
        /// <summary>
        /// 同步供应商信息
        /// </summary>
        /// <returns></returns>
        public string SyncSupplier()
        {
            return SyncFactory("AddSupplier");
        }
        /// <summary>
        /// 同步供应商银行信息
        /// </summary>
        /// <returns></returns>
        public string SyncSupplierBank()
        {
            return SyncFactory("AddSupplierBank");
        }
        /// <summary>
        /// 同步供应商联系人信息
        /// </summary>
        /// <returns></returns>
        public string SyncSupplierContant()
        {
            return SyncFactory("AddSupplierContant");
        }

        /// <summary>
        /// 供应商分配组织
        /// </summary>
        /// <returns></returns>
        public string SyncSupplierAllotOrg()
        {
            //return SyncFactory2("SupplierAllotOrg");   
            string data = string.Empty;
            ResponseMessage<dynamic> response;
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                var request = JsonConvertUtils.DeserializeObject<List<SupplierAllotOrgRequest>>(data);
                CustomerServcie customerServcie = new CustomerServcie();
                customerServcie.SyncSupplierAllotOrgV2(ctx, request);
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Success,
                    Message = "分配成功!"
                };
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvert.SerializeObject(response);
        }

        public string SyncSupplierAllotOrgV2()
        {
            string data = string.Empty;

            using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
            {
                var task = reader.ReadToEndAsync();
                data = task.Result;
            }
            var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
            BasicDataSyncService basicOrder = new BasicDataSyncService();
            var request = JsonConvert.DeserializeObject<List<SupplierAllotOrgRequest>>(data);
            var response = basicOrder.AddSupplierAllotOrg(ctx, request);

            return JsonConvertUtils.SerializeObject(response);
        }

        public string DeleteLinkMan()
        {
            return SyncFactory("DeleteLinkMan");
        }

        public string DeleteCustAddress()
        {
            return SyncFactory("DeleteCustAddress");
        }

        public string DeleteSupplierContant()
        {
            return SyncFactory("DeleteSupplierContant");
        }

        public string EnableCustomer()
        {
            return SyncFactory("EnableCustomer");
        }

        public string EnableSupplier()
        {
            return SyncFactory("EnableSupplier");
        }

        public string SyncCustomerGrade()
        {
            return SyncFactory("SyncCustomerGrade");
        }

        private string SyncFactory2(string action)
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response;
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);

                CustomerServcie customerServcie = new CustomerServcie();
                switch (action)
                {

                    case "SupplierAllotOrg":
                        response = customerServcie.SyncSupplierAllotOrg(ctx, data);
                        break;
                    default:
                        response = new ResponseMessage<dynamic>();
                        break;
                }

            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvert.SerializeObject(response);
        }

        /// <summary>
        /// 历史供应商分配组织
        /// </summary>
        /// <returns></returns>
        public string HistorySyncSupplierAllotOrg()
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
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                BasicDataSyncService basicOrder = new BasicDataSyncService();
                //var request = JsonConvert.DeserializeObject<SupplierAllotOrgRequest>(data);
                //response = basicOrder.AddSupplierAllotOrg(ctx, request);

                //自动分配当前需要分配的供应商
                var list = basicOrder.GetAllotOrgSupplier(ctx);
                CustomerServcie customerServcie = new CustomerServcie();
                foreach (var item in list)
                {
                    var dataInfo = JsonConvert.SerializeObject(item);
                    customerServcie.SyncSupplierAllotOrg(ctx, dataInfo);
                }
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvert.SerializeObject(response);
        }

        /// <summary>
        /// 测试供应商分配组织
        /// </summary>
        /// <returns></returns>
        public string TestSupplierAllotOrg()
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
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                BasicDataSyncService basicOrder = new BasicDataSyncService();
                var request = JsonConvert.DeserializeObject<List<SupplierAllotOrgRequest>>(data);
                response = basicOrder.AddSupplierAllotOrg(ctx, request);

            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvert.SerializeObject(response);
        }


        /// <summary>
        /// 删除同步重复客户
        /// </summary>
        /// <returns></returns>
        public string deleteCust()
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                BasicDataSyncService basicOrder = new BasicDataSyncService();
                response = basicOrder.deleteCust(ctx);

            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvert.SerializeObject(response);
        }

        /// <summary>
        /// 测试创建客户联系人
        /// </summary>
        /// <returns></returns>
        private string SyncFactory(string type)
        {
            string data = string.Empty;
            IMessageExecute basicOrder = new CustomerBusiness();
            switch (type)
            {
                case "Addcustomer":
                    break;
                case "AddLinkMan":
                    basicOrder = new LinkManBusiness();
                    break;
                case "AddCustAddress":
                    basicOrder = new CustAddressBusiness();
                    break;
                case "AddCustPaymothod":
                    basicOrder = new CustPaymothod();
                    break;
                case "AddInvoice":
                    basicOrder = new CustInvoiceBusiness();
                    break;
                case "DeptAndUser":
                    basicOrder = new DeptUserBusiness();
                    break;
                case "SalesCust":
                    basicOrder = new SalesCustBusiness();
                    break;
                case "AddSupplier":
                    basicOrder = new SupplierBusiness();
                    break;
                case "AddSupplierBank":
                    basicOrder = new SupplierBankBusiness();
                    break;
                case "AddSupplierContant":
                    basicOrder = new SupplierContantBusiness();
                    break;
                case "DeleteLinkMan":
                    basicOrder = new DeleteLinkManBusiness();
                    break;
                case "DeleteCustAddress":
                    basicOrder = new DeleteCustAddressBusiness();
                    break;
                case "DeleteSupplierContant":
                    basicOrder = new DeleteSupplierContantBusiness();
                    break;
                case "EnableCustomer":
                    basicOrder = new EnableCustomerBusiness();
                    break;
                case "EnableSupplier":
                    basicOrder = new EnableSupplierBusiness();
                    break;
                case "SyncCustomerGrade":
                    basicOrder = new SyncCustomerGrade();
                    break;
            }
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                response = basicOrder.Execute(ctx, data);

            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvert.SerializeObject(response);
        }

        /// <summary>
        /// 更新历史联系人审核状态
        /// </summary>
        /// <returns></returns>
        public string UpHistoryLinkManAudit()
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);

                LinkManBusiness basicOrder = new LinkManBusiness();
                response = basicOrder.UpHistoryLinkManAudit(ctx);

            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvert.SerializeObject(response);
        }

        /// <summary>
        /// 测试同步供应商银行信息
        /// </summary>
        /// <returns></returns>
        public string TestSupplierBankInfo()
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
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                SupplierBankBusiness basicOrder = new SupplierBankBusiness();
                response = basicOrder.Execute(ctx, data);

            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvert.SerializeObject(response);
        }

        /// <summary>
        /// 测试启用/禁用供应商
        /// </summary>
        /// <returns></returns>
        public string TestEnableSupplierBusiness()
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
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                EnableSupplierBusiness basicOrder = new EnableSupplierBusiness();
                response = basicOrder.Execute(ctx, data);

            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvert.SerializeObject(response);
        }

        /// <summary>
        /// 测试同步客户地址
        /// </summary>
        /// <returns></returns>
        public string TestCustAddress()
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
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                CustAddressBusiness basicOrder = new CustAddressBusiness();
                response = basicOrder.Execute(ctx, data);

            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvert.SerializeObject(response);
        }
    }
}
