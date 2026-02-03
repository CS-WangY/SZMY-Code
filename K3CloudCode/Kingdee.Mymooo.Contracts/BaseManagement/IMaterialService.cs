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
using Kingdee.Mymooo.Core.Common;

namespace Kingdee.Mymooo.Contracts.BaseManagement
{
    [RpcServiceError]
    [ServiceContract]
    public interface IMaterialService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        MaterialSmallBusinessDivision GetMaterialSmallBusinessDivision(Context ctx, long materialId);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        MaterialInfo TryGetOrAdd(Context ctx, MaterialInfo material, List<long> useorgid, bool IsAllocate = true);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        MaterialInfo TryBomGetOrAdd(Context ctx, MaterialInfo material);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        MaterialInfo[] TryGetOrAdds(Context ctx, MaterialInfo[] materials, List<long> useorgid); 

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> GroupSave(Context ctx, List<SalesOrderBillRequest.Productsmallclass> datas);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> CreateBOM(Context ctx, DispatchInfo request);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        MaterialInfo[] TryGetOrAddCustMsterials(Context ctx, CustomerInfo customer, params MaterialInfo[] materials);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> MaterialAllocate(Context ctx, List<long> materiallist);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void MaterialAllocateToAll(Context ctx, List<long> materiallist);
    }
}
