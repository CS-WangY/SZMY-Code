using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Rpc;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core;
using Kingdee.BOS;
using Kingdee.Mymooo.Core.ProductionManagement;

namespace Kingdee.Mymooo.Contracts.PrdMoManagement
{
    /// <summary>
    /// 生产订单接口
    /// </summary>
    [RpcServiceError]
    [ServiceContract]
    public interface IMoOrderBillService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> StatusMoOrderAction(Context ctx, PrdMoStatus mostatus);
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> PrdPPBomChange(Context ctx, MesPrd_PPBOMRequest mostatus);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> PrdMoChangeWorkShop(Context ctx, PrdMoChangeWorkShopRequest mostatus);
    }

}
