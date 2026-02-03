using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Rpc;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ProductionManagement;
using Kingdee.Mymooo.Core.ProductionManagement.Dispatch;
using System.Collections.Generic;
using System.ServiceModel;

namespace Kingdee.Mymooo.Contracts.ProductionManagement
{
	[RpcServiceError]
	[ServiceContract]
	public interface IProductionMoService
	{
		[OperationContract]
		[FaultContract(typeof(ServiceFault))]
		IOperationResult SendMakeDispatch(Context ctx, long entryId);
		[OperationContract]
		[FaultContract(typeof(ServiceFault))]
		IOperationResult SendMakeDispatchs(Context ctx, SendMakeDispatchRequest request);
		[OperationContract]
		[FaultContract(typeof(ServiceFault))]
		IOperationResult SendMakeDispatchs(Context ctx, List<long> entryIds);

		[OperationContract]
		[FaultContract(typeof(ServiceFault))]
		IOperationResult SendMakeDispatchForIds(Context ctx, List<long> headIds);

		[OperationContract]
		[FaultContract(typeof(ServiceFault))]
		IOperationResult CancelMakeDispatch(Context ctx, MakeDispatchRequest request);

		[OperationContract]
		[FaultContract(typeof(ServiceFault))]
		IOperationResult CancelMakeDispatchs(Context ctx, List<long> entryIds);

		[OperationContract]
		[FaultContract(typeof(ServiceFault))]
		IOperationResult CancelMakeDispatchForIds(Context ctx, List<long> headIds);

		[OperationContract]
		[FaultContract(typeof(ServiceFault))]
		ResponseMessage<dynamic> MesProductionMoStatus(Context ctx, SendMesMakeResponse request);

		[OperationContract]
		[FaultContract(typeof(ServiceFault))]
		ResponseMessage<dynamic> MesCancelMoStatus(Context ctx, SendMesMakeResponse request);
		[OperationContract]
		[FaultContract(typeof(ServiceFault))]
		ResponseMessage<dynamic> SendMakeDispatchForBill(Context ctx, MakeRequest request);
        /// <summary>
        /// Mes一键退料
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> MesOneClickReturnMaterial(Context ctx, MesOneClickReturnMaterialRequest request);

    }
}
