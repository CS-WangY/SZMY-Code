using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.Operation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Rpc;
using Kingdee.Mymooo.Core;
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
    public interface IMymoooBusinessDataService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult SaveAndAuditBill(Context ctx, BusinessInfo businessInfo, DynamicObject[] dynamicObjects);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult SaveBill(Context ctx, BusinessInfo businessInfo, DynamicObject[] dynamicObjects);
		[OperationContract]
		[FaultContract(typeof(ServiceFault))]
		IOperationResult SaveBill(Context ctx, BusinessInfo businessInfo, bool isRemoveValidators, params DynamicObject[] dynamicObjects);

		[OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult SubmitBill(Context ctx, BusinessInfo businessInfo, object[] ids);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult Audit(Context ctx, BusinessInfo businessInfo, object[] ids);
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult DeleteBill(Context ctx, BusinessInfo businessInfo, object[] ids);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult UnAudit(Context ctx, BusinessInfo businessInfo, object[] ids);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult SetBillStatus(Context ctx, BusinessInfo businessInfo, object[] ids, string operationNumber);

        /// <summary>
        /// 分配
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="allocateParameter"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult Allocate(Context ctx, AllocateParameter allocateParameter);

        /// <summary>
        /// 添加mq消息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="action"></param>
        /// <param name="keyword"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<long> AddRabbitMqMeaage(Context ctx, string action, string keyword, string data);

        /// <summary>
        /// 添加mq消息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="action"></param>
        /// <param name="keyword"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> AddRabbitMqMeaageResult(Context ctx, string action, string keyword, string data, bool isSucceed, string result);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void UpdateRabbitMqMeaage(Context ctx, long id, string result, bool isSucceed);
    }
}
