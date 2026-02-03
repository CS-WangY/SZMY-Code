using Kingdee.BOS.Rpc;
using Kingdee.Mymooo.Core;
using System;
using Kingdee.BOS;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;

namespace Kingdee.Mymooo.Contracts.BaseManagement
{
    /// <summary>
    /// 基础信息同步服务
    /// </summary>
    [RpcServiceError]
    [ServiceContract]
    public interface IBasicDataSyncService
    {
        /// <summary>
        /// 同步业务员和客户的关系
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="custId"></param>
        /// <param name="sourceUserCode"></param>
        /// <param name="salesId"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> SyncSaleCust(Context ctx, long custId, List<KeyValuePair<long, long>> salesId, bool isFirstSync, List<string> userCode, List<string> OrderNumber, string transferUserCode);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> UnBindSaleCust(Context ctx, long custId);

        /// <summary>
        /// 同步客户信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> AddOrMotifyCustomer(Context ctx, CustomerRequest request);
        /// <summary>
        /// 同步供应商
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> AddOrMotifySupplier(Context ctx, SupplierRequest request);
        /// <summary>
        /// 供应商分配组织
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> AddSupplierAllotOrg(Context ctx, List<SupplierAllotOrgRequest> request);

        /// <summary>
        /// 根据企业微信编码获取到对应Id
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="orgId">组织</param>
        /// <param name="weChatCode">企业微信编码</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        long GetSaleId(Context ctx, long orgId, string weChatCode);

        /// <summary>
        /// CRM同步客诉到金蝶
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> SynCompanyComplaintService(Context ctx, CompanyComplaintRequest request);

        /// <summary>
        /// 周合格率统计
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> WeeklyPassRateStatisticsService(Context ctx);

        /// <summary>
        /// 根据认岗信息完善业务员业务组信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<KeyValuePair<long, long>> SyncSAL_SC_SalerCust(Context ctx, List<SalerCustList> keys);
    }
}
