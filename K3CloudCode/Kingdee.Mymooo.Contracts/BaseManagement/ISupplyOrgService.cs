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
    public interface ISupplyOrgService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        string GetSupplyOrgBusinessDivision(Context ctx, long supplyOrgId);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void SenMesSupplyInfo(Context ctx, string supplierCode);

		[OperationContract]
		[FaultContract(typeof(ServiceFault))]
		ResponseMessage<dynamic> SupplierSmallService(Context ctx, SupplierSmallRequest[] requests);

	}
}
