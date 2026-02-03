using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Rpc;
using Kingdee.Mymooo.Core.BaseManagement;

namespace Kingdee.Mymooo.Contracts.BaseManagement
{
    [RpcServiceError]
    [ServiceContract]
    public interface IUserService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        UserInfo GetUserInfoForUserID(Context ctx, long supplyOrgId);
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        string GetUserWxCode(Context ctx, long userid);
    }
}
