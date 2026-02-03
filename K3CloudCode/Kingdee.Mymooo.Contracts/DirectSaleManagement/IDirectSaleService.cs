using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Rpc;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.DirectSaleManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Contracts.DirectSaleManagement
{
    /// <summary>
    /// 直发业务服务
    /// </summary>
    [RpcServiceError]
    [ServiceContract]
    public interface IDirectSaleService
    {
        /// <summary>
        /// 采购订单下推收料通知单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> PoPushReceiveMaterials(Context ctx, SoDirEntity request);

        /// <summary>
        /// 收料通知单下推采购入库单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> RmPushPurchasing(Context ctx, SoDirEntity request);

        /// <summary>
        /// 销售订单下推发货通知单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> SoPushDelivery(Context ctx, SoDirEntity request);

        /// <summary>
        /// 生成调拨出库
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> CreateDeliveryTransferOut(Context ctx, SoDirEntity request);

        /// <summary>
        /// 审核调拨入库
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> AuditDeliveryTransferIn(Context ctx, SoDirEntity request);

        /// <summary>
        /// 发货通知单下推销售出库单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> DnPushSalesOutStock(Context ctx, SoDirEntity request);

        /// <summary>
        /// 直发修改采购入库后预留数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> DirectEditReserved(Context ctx, SoDirEntity request);

        /// <summary>
        /// 设置采购订单直发预留数量
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> SetPoDirReServeQty(Context ctx, SoDirEntity request);
    }
}
