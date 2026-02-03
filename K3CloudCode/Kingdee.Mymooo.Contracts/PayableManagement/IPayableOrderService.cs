using Kingdee.BOS.Rpc;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Core.PayableManagement;
using Kingdee.BOS;

namespace Kingdee.Mymooo.Contracts.PayableManagement
{
	/// <summary>
	/// 应付订单服务
	/// </summary>
	[RpcServiceError]
	[ServiceContract]
	public interface IPayableOrderService
	{
		/// <summary>
		/// MES费用采购下推费用应付
		/// </summary>
		/// <param name="ctx"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(ServiceFault))]
		ResponseMessage<dynamic> MesCostPurGenerateCostPayableService(Context ctx, MesCostPurGenerateCostPayableRequest request);
	}
}
