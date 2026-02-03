using Kingdee.BOS.Rpc;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Orm.DataEntity;

namespace Kingdee.Mymooo.Contracts.PurchaseManagement
{
    /// <summary>
    /// 采购申请单服务
    /// </summary>
    [RpcServiceError]
    [ServiceContract]
    public interface IPur_RequisitionOrderService
    {
        /// <summary>
        /// 新增采购申请接口
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="entry"></param>
        /// <param name="TargetOrgId"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> Add_PUR_Requisition(Context ctx, PUR_Requisition bill);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult Del_PUR_Requisition(Context ctx, string billid);
    }
}
