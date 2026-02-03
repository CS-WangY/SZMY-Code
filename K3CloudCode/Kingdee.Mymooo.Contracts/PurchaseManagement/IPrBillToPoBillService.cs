using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Rpc;
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
    /// PrToPo服务
    /// </summary>
    [RpcServiceError]
    [ServiceContract]
    public interface IPrBillToPoBillService
    {
        /// <summary>
        /// PrToPo服务
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="salesOrderId"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult PrBillToPoBillAction(Context ctx, List<PrToPoPurchaseRequireEntity> list);
    }
}
