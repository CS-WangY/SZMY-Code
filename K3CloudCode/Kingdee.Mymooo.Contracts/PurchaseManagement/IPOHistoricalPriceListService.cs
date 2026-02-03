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

    [RpcServiceError]
    [ServiceContract]
    public interface IPOHistoricalPriceListService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> GetHistoricalPriceList(Context ctx, string sDate);
    }

}
