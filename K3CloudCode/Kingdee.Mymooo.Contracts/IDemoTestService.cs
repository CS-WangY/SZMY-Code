using Kingdee.BOS;
using Kingdee.BOS.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Contracts
{
    [RpcServiceError]
    [ServiceContract]
    public interface IDemoTestService
    {

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        string DemoTestAction(Context ctx);
    }
}
