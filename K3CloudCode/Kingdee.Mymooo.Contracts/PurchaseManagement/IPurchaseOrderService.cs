using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Rpc;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.PurchaseManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Contracts.PurchaseManagement
{
    /// <summary>
    /// 采购订单服务
    /// </summary>
    [RpcServiceError]
    [ServiceContract]
    public interface IPurchaseOrderService
    {
        /// <summary>
        /// 采购申请单业务终止
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> StatusPrOrderAction(Context ctx, PrStatus status);
        /// <summary>
        /// 获取产品采购记录
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> GetPurchaseOrderSimpleAction(Context ctx, List<string> productModels);
        /// <summary>
        /// 采购订单生成应付单
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult PurchaseToReceivable(Context ctx, List<PurchaseOrderPushEntity> entry, long TargetOrgId);

		/// <summary>
		/// 费用采购订单
		/// </summary>
		/// <param name="ctx"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(ServiceFault))]
		ResponseMessage<dynamic> CostPurchaseOrderService(Context ctx, CostPurchaseOrderRequest request);

		/// <summary>
		/// MES收料下推采购入库
		/// </summary>
		/// <param name="ctx"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(ServiceFault))]
		ResponseMessage<dynamic> MesReceiveGenerateInStockService(Context ctx, MesReceiveInStockRequest request);

		/// <summary>
		/// MES退料申请下推采购退料
		/// </summary>
		/// <param name="ctx"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(ServiceFault))]
		ResponseMessage<dynamic> MesMrAppGenerateMrbService(Context ctx, MesMrAppToMrbRequest request);
	}
}
