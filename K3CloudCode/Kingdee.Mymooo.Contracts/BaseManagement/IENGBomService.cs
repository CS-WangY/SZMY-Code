using Kingdee.BOS;
using Kingdee.BOS.Rpc;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Core.ProductionManagement;

namespace Kingdee.Mymooo.Contracts.BaseManagement
{
    [RpcServiceError]
    [ServiceContract]
    public interface IENGBomService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ENGBomInfo TryGetOrAdd(Context ctx, ENGBomInfo request);
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ENGBomInfo[] TryGetOrAddsOrg(Context ctx, ENGBomInfo[] request, long[] orgid);
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ENGBomInfo[] TryGetOrAdds(Context ctx, ENGBomInfo[] request);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> MaterialAllocate(Context ctx, List<ENGBomInfo> request);
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> BomAllocate(Context ctx, List<ENGBomInfo> request);
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> BomAllocateByID(Context ctx, long[] request);
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void SendMQAllocate(Context ctx, List<ENGBomInfo> request);
    }
}
