using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Rpc;
using Kingdee.Mymooo.Contracts;
using Kingdee.Mymooo.Contracts.BaseManagement;
using Kingdee.Mymooo.Contracts.SalesManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.SalesManagement.SalesOrderScheduled;
using Kingdee.Mymooo.Core.StockManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServiceHelper.SalesManagement
{
    public class SalesOrderServiceHelper
    {
        /// <summary>
        /// 销售订单关闭或行关闭
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="closedType"></param>
        /// <param name="salesOrderId"></param>
        /// <returns></returns>
        public static ResponseMessage<dynamic> ClosedSalesOrderAction(Context ctx, ApprovalMessageRequest request)
        {
            ISalesOrderService service = ServiceFactory.GetService<ISalesOrderService>(ctx);
            try
            {
                return service.ClosedSalesOrderAction(ctx, request);
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
        /// 取消销售单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="salesOrderId"></param>
        public static ResponseMessage<dynamic> CloseSalesOrderAction(Context ctx, int salesOrderId, string fdocumentStatus)
        {
            ISalesOrderService service = ServiceFactory.GetService<ISalesOrderService>(ctx);
            try
            {
                return service.CancelSalesOrderAction(ctx, salesOrderId, fdocumentStatus);
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

        public static UserInfo GetUserInfoForUserID(Context ctx, long supplyOrgId)
        {
            IUserService service = ServiceFactory.GetService<IUserService>(ctx);
            try
            {
                return service.GetUserInfoForUserID(ctx, supplyOrgId);
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
        /// 修改发货通知单物流等信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>

        public static ResponseMessage<dynamic> ModifyDeliveryExpressageService(Context ctx, CloudWarehouseDnNoticeEntity request)
        {
            ISalesOrderService service = ServiceFactory.GetService<ISalesOrderService>(ctx);
            try
            {
                return service.ModifyDeliveryExpressageService(ctx, request);
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
        /// 全国一部,华南二部调拨出库
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>

        public static ResponseMessage<dynamic> DeliveryTransferOutService(Context ctx, CloudWarehouseDnNoticeEntity request)
        {
            ISalesOrderService service = ServiceFactory.GetService<ISalesOrderService>(ctx);
            try
            {
                return service.DeliveryTransferOutService(ctx, request);
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
        /// 全国一部,华南二部调拨入库
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>

        public static ResponseMessage<dynamic> DeliveryTransferInService(Context ctx, CloudWarehouseDnNoticeEntity request)
        {
            ISalesOrderService service = ServiceFactory.GetService<ISalesOrderService>(ctx);
            try
            {
                return service.DeliveryTransferInService(ctx, request);
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
        /// 下推生成销售出库单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>

        public static ResponseMessage<dynamic> GenerateOutStockService(Context ctx, CloudWarehouseDnNoticeEntity request)
        {
            ISalesOrderService service = ServiceFactory.GetService<ISalesOrderService>(ctx);
            try
            {
                return service.GenerateOutStockService(ctx, request);
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
        /// 发货通知单变更数量
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>

        public static ResponseMessage<dynamic> ModifyDeliveryQuantityService(Context ctx, CloudWarehouseDnNoticeEntity request)
        {
            ISalesOrderService service = ServiceFactory.GetService<ISalesOrderService>(ctx);
            try
            {
                return service.ModifyDeliveryQuantityService(ctx, request);
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
        /// 事业部红字调入
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>

        public static ResponseMessage<dynamic> RedDeliveryTransferInService(Context ctx, CloudWarehouseDnNoticeEntity request)
        {
            ISalesOrderService service = ServiceFactory.GetService<ISalesOrderService>(ctx);
            try
            {
                return service.RedDeliveryTransferInService(ctx, request);
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
        /// 非标订单创建计划订单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static ResponseMessage<dynamic> SalBillPushPlanBill(Context ctx, List<ENGBomInfo> bomlist)
        {
            ISalesOrderService service = ServiceFactory.GetService<ISalesOrderService>(ctx);
            try
            {
                return service.SalBillPushPlanBill(ctx, bomlist);
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
        /// 生成应收单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="entry"></param>
        /// <param name="TargetOrgId"></param>
        /// <returns></returns>
        public static IOperationResult SalesToReceivable(Context ctx, List<SalesOrderPushEntity> entry, long TargetOrgId)
        {
            ISalesOrderService service = ServiceFactory.GetService<ISalesOrderService>(ctx);
            try
            {
                return service.SalesToReceivable(ctx, entry, TargetOrgId);
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
        /// 生成收款单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="entry"></param>
        /// <param name="TargetOrgId"></param>
        /// <returns></returns>
        public static IOperationResult SalesToReceiveBill(Context ctx, SalesOrderPushReceiveBillEntity entry, long TargetOrgId)
        {
            ISalesOrderService service = ServiceFactory.GetService<ISalesOrderService>(ctx);
            try
            {
                return service.SalesToReceiveBill(ctx, entry, TargetOrgId);
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

        public static void ChangeSalesOrder(Context ctx, BusinessInfo businessInfo, params DynamicObject[] dynamicObjects)
        {
            ISalesOrderService service = ServiceFactory.GetService<ISalesOrderService>(ctx);
            try
            {
                service.ChangeSalesOrder(ctx, businessInfo, dynamicObjects);
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
        /// 非标订单创建BOM
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static ResponseMessage<dynamic> FBSalBillCreateBom(Context ctx, ChangeOrderTaskRequest request)
        {
            ISalesOrderService service = ServiceFactory.GetService<ISalesOrderService>(ctx);
            try
            {
                return service.FBSalBillCreateBom(ctx, request);
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
        /// MES下推生成销售出库单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>

        public static ResponseMessage<dynamic> MesGenerateOutStockService(Context ctx, MesGenerateOutStockRequest request)
        {
            ISalesOrderService service = ServiceFactory.GetService<ISalesOrderService>(ctx);
            try
            {
                return service.MesGenerateOutStockService(ctx, request);
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
        /// MES下推生成销售退货单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>

        public static ResponseMessage<dynamic> MesGenerateReturnStockService(Context ctx, MesGenerateReturnStockRequest request)
        {
            ISalesOrderService service = ServiceFactory.GetService<ISalesOrderService>(ctx);
            try
            {
                return service.MesGenerateReturnStockService(ctx, request);
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

		public static ResponseMessage<QueryExpiryResponseList> QueryCustomerExpiryList(Context ctx, RequestExpiry request)
		{
			ISalesOrderService service = ServiceFactory.GetService<ISalesOrderService>(ctx);
			try
			{
				return service.QueryCustomerExpiryList(ctx, request);
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
