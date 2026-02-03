using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.Mymooo.Business.PlugIn.PurchaseManagement;
using Kingdee.Mymooo.Business.PlugIn.SalesManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.PurchaseManagement;
using Kingdee.Mymooo.ServiceHelper.PurRequisitionManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Text;

namespace Kingdee.Mymooo.WebApi.ServicesStub
{
    /// <summary>
    /// 采购单管理
    /// </summary>
    public class PurchaseManagementService : KDBaseService
    {
        public PurchaseManagementService(KDServiceContext context) : base(context)
        {
        }

        public string GetPurchaseProductQuantity()
        {
            string data = string.Empty;

            using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
            {
                var task = reader.ReadToEndAsync();
                data = task.Result;
            }
            var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
            var request = JsonConvertUtils.DeserializeObject<PurchaseProductQuantityRequest>(data);

            var response = PurchaseOrderServiceHelper.PurPoInventory(ctx, request);

            var settiongs = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return JsonConvert.SerializeObject(response, Formatting.None, settiongs);
        }

        public string PurchaseOrderSyncStatus()
        {
            string data = string.Empty;

            using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
            {
                var task = reader.ReadToEndAsync();
                data = task.Result;
            }
            var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
            var request = JsonConvertUtils.DeserializeObject<SaleOrderRequest>(data);
            PurchaseOrderBusiness business = new PurchaseOrderBusiness();
            var response = business.PurchaseOrderSyncStatus(ctx, request);

            return JsonConvertUtils.SerializeObject(response);
        }

        /// <summary>
        /// 获取产品采购记录
        /// </summary>
        /// <returns></returns>
        public string GetPurchaseOrderSimple()
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
                PurchaseOrderBusiness purchaseOrderBusiness = new PurchaseOrderBusiness();
                response = purchaseOrderBusiness.GetPurchaseOrderSimple(ctx, data);
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
        /// 获取采购单列表
        /// </summary>
        /// <returns></returns>
        public string GetPurchaseOrder()
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
                PurchaseOrderBusiness purchaseOrderBusiness = new PurchaseOrderBusiness();
                response = purchaseOrderBusiness.GetPurchaseOrder(ctx, data);
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
        /// 采购库存信息查询
        /// </summary>
        /// <returns></returns>
        public string GetPurchaseStockInfo()
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
                PurchaseOrderBusiness purchaseOrderBusiness = new PurchaseOrderBusiness();
                response = purchaseOrderBusiness.GetPurchaseStockInfo(ctx, data);
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
        /// 可用库存数明细
        /// </summary>
        /// <returns></returns>
        public string GetAvailableInventoryDetail()
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
                PurchaseOrderBusiness purchaseOrderBusiness = new PurchaseOrderBusiness();
                response = purchaseOrderBusiness.GetAvailableInventoryDetail(ctx, data);
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
        /// 出入库明细
        /// </summary>
        /// <returns></returns>
        public string GetStockDetail()
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
                PurchaseOrderBusiness purchaseOrderBusiness = new PurchaseOrderBusiness();
                response = purchaseOrderBusiness.GetStockDetailRpt(ctx, data);
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
        /// 费用采购订单
        /// </summary>
        /// <returns></returns>
        public string CostPurchaseOrder()
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
                PurchaseOrderBusiness purchaseOrderBusiness = new PurchaseOrderBusiness();
                response = purchaseOrderBusiness.CostPurchaseOrder(ctx, data);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }

        /// <summary>
        /// MES收料下推采购入库
        /// </summary>
        public string MesReceiveGenerateInStock()
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
                PurchaseOrderBusiness purchaseOrderBusiness = new PurchaseOrderBusiness();
                response = purchaseOrderBusiness.MesReceiveGenerateInStock(ctx, data);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }


        /// <summary>
        /// MES退料申请下推采购退料
        /// </summary>
        public string MesMrAppGenerateMrb()
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
                PurchaseOrderBusiness purchaseOrderBusiness = new PurchaseOrderBusiness();
                response = purchaseOrderBusiness.MesMrAppGenerateMrb(ctx, data);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }

        /// <summary>
        /// 变更费用采购订单
        /// </summary>
        /// <returns></returns>
        public string ChangeCostPurchaseOrder()
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
                ChangeCostPurchaseOrderBusiness business = new ChangeCostPurchaseOrderBusiness();
                response = business.Execute(ctx, data);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }

        /// <summary>
        /// 费用采购订单删除
        /// </summary>
        /// <returns></returns>
        public string CostPurchaseOrderDelete()
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
                ChangeCostPurchaseOrderBusiness business = new ChangeCostPurchaseOrderBusiness();
                response = business.CostPurchaseOrderDelete(ctx, data);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }

        /// <summary>
        /// 收料免检下推采购入库
        /// </summary>
        public string ReceiveExemptionInStock()
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
                PurchaseOrderBusiness purchaseOrderBusiness = new PurchaseOrderBusiness();
                response = purchaseOrderBusiness.ReceiveExemptionInStock(ctx, data);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }
    }
}
