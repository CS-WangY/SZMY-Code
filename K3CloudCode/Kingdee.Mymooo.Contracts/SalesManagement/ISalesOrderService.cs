using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Rpc;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.SalesManagement.SalesOrderScheduled;
using Kingdee.Mymooo.Core.StockManagement;
using System;
using System.Collections.Generic;
using System.Linq;

using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Contracts.SalesManagement
{
    /// <summary>
    /// 销售订单服务
    /// </summary>
    [RpcServiceError]
    [ServiceContract]
    public interface ISalesOrderService
    {
        /// <summary>
        /// 销售订单关闭或行关闭
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="closedType"></param>
        /// <param name="salesOrderId"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> ClosedSalesOrderAction(Context ctx, ApprovalMessageRequest request);
        /// <summary>
        /// 取消销售单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="salesOrderId"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> CancelSalesOrderAction(Context ctx, int salesOrderId, string fdocumentStatus);


        /// <summary>
        /// 修改发货通知单物流等信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> ModifyDeliveryExpressageService(Context ctx, CloudWarehouseDnNoticeEntity request);

        /// <summary>
        /// 全国一部,华南二部调拨出库
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> DeliveryTransferOutService(Context ctx, CloudWarehouseDnNoticeEntity request);

        /// <summary>
        /// 全国一部,华南二部调拨入库
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> DeliveryTransferInService(Context ctx, CloudWarehouseDnNoticeEntity request);

        /// <summary>
        /// 下推生成销售出库单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> GenerateOutStockService(Context ctx, CloudWarehouseDnNoticeEntity request);

        /// <summary>
        /// 发货通知单变更数量
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> ModifyDeliveryQuantityService(Context ctx, CloudWarehouseDnNoticeEntity request);

        /// <summary>
        /// 事业部红字调入
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> RedDeliveryTransferInService(Context ctx, CloudWarehouseDnNoticeEntity request);

        /// <summary>
        /// 生成应收单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="TargetOrgId"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult SalesToReceivable(Context ctx, List<SalesOrderPushEntity> entry, long TargetOrgId);

        /// <summary>
        /// 生成收款单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="entry"></param>
        /// <param name="TargetOrgId"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult SalesToReceiveBill(Context ctx, SalesOrderPushReceiveBillEntity entry, long TargetOrgId);

        /// <summary>
        /// 变更
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="businessInfo"></param>
        /// <param name="dynamicObjects"></param>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void ChangeSalesOrder(Context ctx, BusinessInfo businessInfo, params DynamicObject[] dynamicObjects);


        /// <summary>
        /// 非标销售订单同步创建计划
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> SalBillPushPlanBill(Context ctx, List<ENGBomInfo> bomlist);
        /// <summary>
        /// 非标销售订单创建非标BOM
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="billid"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> FBSalBillCreateBom(Context ctx, ChangeOrderTaskRequest request);

        /// <summary>
        /// MES下推生成销售出库单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> MesGenerateOutStockService(Context ctx, MesGenerateOutStockRequest request);

        /// <summary>
        /// MES下推生成销售退货单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> MesGenerateReturnStockService(Context ctx, MesGenerateReturnStockRequest request);

		[OperationContract]
		[FaultContract(typeof(ServiceFault))]
		ResponseMessage<QueryExpiryResponseList> QueryCustomerExpiryList(Context ctx, RequestExpiry request);
	}
}
