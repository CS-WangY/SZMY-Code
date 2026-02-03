using Kingdee.Mymooo.Contracts.SalesManagement;
using Kingdee.Mymooo.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.Mymooo.Core.DirectSaleManagement;
using Kingdee.Mymooo.Contracts.DirectSaleManagement;
using Kingdee.BOS;
using Kingdee.Mymooo.Core;

namespace Kingdee.Mymooo.ServiceHelper.DirectSaleManagement
{
    public class DirectSaleServiecHelper
    {
        /// <summary>
        /// 采购订单下推收料通知单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static ResponseMessage<dynamic> PoPushReceiveMaterials(Context ctx, SoDirEntity request)
        {
            IDirectSaleService service = ServiceFactory.GetService<IDirectSaleService>(ctx);
            try
            {
                return service.PoPushReceiveMaterials(ctx, request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ServiceFactory.CloseService(service);
            }
        }

        /// <summary>
        /// 收料通知单下推采购入库单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static ResponseMessage<dynamic> RmPushPurchasing(Context ctx, SoDirEntity request)
        {
            IDirectSaleService service = ServiceFactory.GetService<IDirectSaleService>(ctx);
            try
            {
                return service.RmPushPurchasing(ctx, request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ServiceFactory.CloseService(service);
            }
        }

        /// <summary>
        /// 销售订单下推发货通知单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static ResponseMessage<dynamic> SoPushDelivery(Context ctx, SoDirEntity request)
        {
            IDirectSaleService service = ServiceFactory.GetService<IDirectSaleService>(ctx);
            try
            {
                return service.SoPushDelivery(ctx, request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ServiceFactory.CloseService(service);
            }
        }

        /// <summary>
        /// 生成调拨出库
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static ResponseMessage<dynamic> CreateDeliveryTransferOut(Context ctx, SoDirEntity request)
        {
            IDirectSaleService service = ServiceFactory.GetService<IDirectSaleService>(ctx);
            try
            {
                return service.CreateDeliveryTransferOut(ctx, request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ServiceFactory.CloseService(service);
            }
        }

        /// <summary>
        /// 审核调拨入库
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static ResponseMessage<dynamic> AuditDeliveryTransferIn(Context ctx, SoDirEntity request)
        {
            IDirectSaleService service = ServiceFactory.GetService<IDirectSaleService>(ctx);
            try
            {
                return service.AuditDeliveryTransferIn(ctx, request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ServiceFactory.CloseService(service);
            }
        }

        /// <summary>
        /// 发货通知单下推销售出库单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static ResponseMessage<dynamic> DnPushSalesOutStock(Context ctx, SoDirEntity request)
        {
            IDirectSaleService service = ServiceFactory.GetService<IDirectSaleService>(ctx);
            try
            {
                return service.DnPushSalesOutStock(ctx, request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ServiceFactory.CloseService(service);
            }
        }

        /// <summary>
        /// 直发修改入库预留数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static ResponseMessage<dynamic> DirectEditReserved(Context ctx, SoDirEntity request)
        {
            IDirectSaleService service = ServiceFactory.GetService<IDirectSaleService>(ctx);
            try
            {
                return service.DirectEditReserved(ctx, request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ServiceFactory.CloseService(service);
            }
        }

        /// <summary>
        /// 设置采购订单直发预留数量
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static ResponseMessage<dynamic> SetPoDirReServeQty(Context ctx, SoDirEntity request)
        {
            IDirectSaleService service = ServiceFactory.GetService<IDirectSaleService>(ctx);
            try
            {
                return service.SetPoDirReServeQty(ctx, request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ServiceFactory.CloseService(service);
            }
        }

    }
}
