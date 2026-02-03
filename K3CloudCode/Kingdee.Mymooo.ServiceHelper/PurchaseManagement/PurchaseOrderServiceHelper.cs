using Kingdee.Mymooo.Contracts.SalesManagement;
using Kingdee.Mymooo.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Core;
using Kingdee.BOS;
using Kingdee.Mymooo.Contracts.PurchaseManagement;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Contracts.StockManagement;
using Kingdee.BOS.Core.DynamicForm;

namespace Kingdee.Mymooo.ServiceHelper.PurchaseManagement
{
    public class PurchaseOrderServiceHelper
    {
        public static ResponseMessage<dynamic> StatusPrOrderAction(Context ctx, PrStatus status)
        {
            IPurchaseOrderService service = ServiceFactory.GetService<IPurchaseOrderService>(ctx);
            try
            {
                return service.StatusPrOrderAction(ctx, status);
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
        public static ResponseMessage<dynamic> PurPoInventory(Context ctx, PurchaseProductQuantityRequest request)
        {
            IStockQuantityService service = ServiceFactory.GetService<IStockQuantityService>(ctx);
            try
            {
                return service.PurPoInventory(ctx, request);
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
        /// 获取产品采购记录
        /// </summary>
        /// <param name="ctx"></param>
        public static ResponseMessage<dynamic> GetPurchaseOrderSimpleAction(Context ctx, List<string> productModels)
        {
            IPurchaseOrderService service = ServiceFactory.GetService<IPurchaseOrderService>(ctx);
            try
            {
                return service.GetPurchaseOrderSimpleAction(ctx, productModels);
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
        public static IOperationResult PurchaseToReceivable(Context ctx, List<PurchaseOrderPushEntity> entry, long TargetOrgId)
        {
            IPurchaseOrderService service = ServiceFactory.GetService<IPurchaseOrderService>(ctx);
            try
            {
                return service.PurchaseToReceivable(ctx, entry, TargetOrgId);
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
		/// 费用采购订单
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="entry"></param>
		/// <returns></returns>
		public static ResponseMessage<dynamic> CostPurchaseOrderService(Context ctx, CostPurchaseOrderRequest request)
		{
			IPurchaseOrderService service = ServiceFactory.GetService<IPurchaseOrderService>(ctx);
			try
			{
				return service.CostPurchaseOrderService(ctx, request);
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
		/// MES收料下推采购入库
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="entry"></param>
		/// <returns></returns>
		public static ResponseMessage<dynamic> MesReceiveGenerateInStockService(Context ctx, MesReceiveInStockRequest request)
		{
			IPurchaseOrderService service = ServiceFactory.GetService<IPurchaseOrderService>(ctx);
			try
			{
				return service.MesReceiveGenerateInStockService(ctx, request);
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
		/// MES退料申请下推采购退料
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="entry"></param>
		/// <returns></returns>
		public static ResponseMessage<dynamic> MesMrAppGenerateMrbService(Context ctx, MesMrAppToMrbRequest request)
		{
			IPurchaseOrderService service = ServiceFactory.GetService<IPurchaseOrderService>(ctx);
			try
			{
				return service.MesMrAppGenerateMrbService(ctx, request);
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
