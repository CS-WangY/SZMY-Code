using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.Mymooo.Business.PlugIn.DirectSaleManagement;
using Kingdee.Mymooo.Business.PlugIn.StockManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.DirectSaleManagement;
using Kingdee.Mymooo.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kingdee.Mymooo.WebApi.ServicesStub
{
    public class DirectSaleManagementService : KDBaseService
    {
        public DirectSaleManagementService(KDServiceContext context) : base(context)
        {
        }

        /// <summary>
        /// 根据销售订单明细获取采购单号信息和直发数量
        /// </summary>
        /// <returns></returns>
        public string GetSoToPoDirQty()
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
                reqSoPoDirQtyFilter request = JsonConvertUtils.DeserializeObject<reqSoPoDirQtyFilter>(data);
                DirectSaleBusiness purchaseBusiness = new DirectSaleBusiness();
                response = purchaseBusiness.GetSoToPoDirQty(ctx, request);
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
        /// 采购订单下推收料通知单
        /// </summary>
        /// <returns></returns>
        public string PoPushReceiveMaterials()
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
                SoDirEntity request = JsonConvertUtils.DeserializeObject<SoDirEntity>(data);
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                DirectSaleBusiness business = new DirectSaleBusiness();
                response = business.PoPushReceiveMaterials(ctx, request);
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
        /// 收料通知单下推采购入库单
        /// </summary>
        /// <returns></returns>
        public string RmPushPurchasing()
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
                SoDirEntity request = JsonConvertUtils.DeserializeObject<SoDirEntity>(data);
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                DirectSaleBusiness business = new DirectSaleBusiness();
                response = business.RmPushPurchasing(ctx, request);
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
        /// 直发修改入库预留数据
        /// </summary>
        /// <returns></returns>
        public string DirectEditReserved()
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
                SoDirEntity request = JsonConvertUtils.DeserializeObject<SoDirEntity>(data);
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                DirectSaleBusiness business = new DirectSaleBusiness();
                response = business.DirectEditReserved(ctx, request);
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
        /// 销售订单下推发货通知单
        /// </summary>
        /// <returns></returns>
        public string SoPushDelivery()
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
                SoDirEntity request = JsonConvertUtils.DeserializeObject<SoDirEntity>(data);
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                DirectSaleBusiness business = new DirectSaleBusiness();
                response = business.SoPushDelivery(ctx, request);
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
        /// 生成调拨出库
        /// </summary>
        /// <returns></returns>
        public string CreateDeliveryTransferOut()
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
                SoDirEntity request = JsonConvertUtils.DeserializeObject<SoDirEntity>(data);
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                DirectSaleBusiness business = new DirectSaleBusiness();
                response = business.CreateDeliveryTransferOut(ctx, request);
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
        /// 审核调拨入库
        /// </summary>
        /// <returns></returns>
        public string AuditDeliveryTransferIn()
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
                SoDirEntity request = JsonConvertUtils.DeserializeObject<SoDirEntity>(data);
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                DirectSaleBusiness business = new DirectSaleBusiness();
                response = business.AuditDeliveryTransferIn(ctx, request);
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
        /// 发货通知单下推销售出库单
        /// </summary>
        /// <returns></returns>
        public string DnPushSalesOutStock()
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
                SoDirEntity request = JsonConvertUtils.DeserializeObject<SoDirEntity>(data);
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                DirectSaleBusiness business = new DirectSaleBusiness();
                response = business.DnPushSalesOutStock(ctx, request);
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
        /// 设置采购订单直发预留数量
        /// </summary>
        /// <returns></returns>
        public string SetPoDirReServeQty()
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
                SoDirEntity request = JsonConvertUtils.DeserializeObject<SoDirEntity>(data);
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                DirectSaleBusiness business = new DirectSaleBusiness();
                response = business.SetPoDirReServeQty(ctx, request);
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
