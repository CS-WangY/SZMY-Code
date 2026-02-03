using Kingdee.BOS;
using Kingdee.BOS.Core.Metadata.BusinessService;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Rpc;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.StockManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Contracts.StockManagement
{
    /// <summary>
    /// 库存查询服务
    /// </summary>
    [RpcServiceError]
    [ServiceContract]
    public interface IStockQuantityService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> PurPoInventory(Context ctx, PurchaseProductQuantityRequest request);
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<StockPlatEntity> StockPlatformAction(Context ctx, List<KeyValue> itemNos);
        /// <summary>
        /// 获取即时库存数
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<StockQuantityEntity> StockQuantityAction(Context ctx, List<string> itemNos, List<long> itemIds, long orgId);
        /// <summary>
        /// 获取即时库存(蚂蚁平台专用)
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<StockPlatEntity> StockQuantityActionV2(Context ctx, List<string> itemNos);

        /// <summary>
        /// 获取即时库存数
        /// </summary>
        /// <param name="ctx">上下文</param>
        /// <param name="masterID">物料分配内码</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        DynamicObjectCollection InventoryQty(Context ctx, long masterID, List<long> orgid);


        /// <summary>
        /// 获取物料可用库存(包含不良品)
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="orgid"></param>
        /// <param name="masterID"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<InventoryQtyV2Entity> InventoryQtyVStatus(Context ctx, long orgid, List<long> masterID);


        /// <summary>
        /// 云仓储根据供货组织、云存储仓库编号获取物料总库存量
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="supplyOrgCode">供货组织编码</param>
        /// <param name="cloudStockCode">仓库编码</param>
        /// <param name="itemNos">物料编码</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<CloudStockBaseQtyEntity> CloudStockBaseQty(Context ctx, CloudStockBaseQtyRequest request);

		/// <summary>
		/// MES根据型材，材质，长宽高模糊查询总库存
		/// </summary>
		/// <param name="ctx"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(ServiceFault))]
		List<MesFuzzyQueryStockBaseQtyEntity> GetMesFuzzyQueryStockBaseQty(Context ctx, FuzzyQueryStockBaseQtyRequest request);

		/// <summary>
		/// 获取即时库存(MES专用)
		/// </summary>
		/// <param name="ctx"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(ServiceFault))]
		List<MesStockPlatEntity> MesStockQuantityAction(Context ctx, List<string> itemNos);

	}

}
