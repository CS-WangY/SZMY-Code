using Kingdee.Mymooo.Contracts.SalesManagement;
using Kingdee.Mymooo.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.BOS;
using Kingdee.Mymooo.Contracts.StockManagement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Core.Metadata.BusinessService;

namespace Kingdee.Mymooo.ServiceHelper.StockManagement
{
    public class StockQuantityServiceHelper
    {
        public static List<StockPlatEntity> StockPlatformAction(Context ctx, List<KeyValue> itemNos)
        {
            IStockQuantityService service = ServiceFactory.GetService<IStockQuantityService>(ctx);
            try
            {
                return service.StockPlatformAction(ctx, itemNos);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ServiceFactory.CloseService(service);
            }
        }
        /// <summary>
        /// 获取即时库存
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="itemNos"></param>
        /// <returns></returns>
        public static List<StockQuantityEntity> StockQuantityAction(Context ctx, List<string> itemNos, List<long> itemIds, long orgId)
        {
            IStockQuantityService service = ServiceFactory.GetService<IStockQuantityService>(ctx);
            try
            {
                return service.StockQuantityAction(ctx, itemNos, itemIds, orgId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ServiceFactory.CloseService(service);
            }
        }
        /// <summary>
        /// 获取即时库存(蚂蚁平台专用)
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="itemNos"></param>
        /// <returns></returns>
        public static List<StockPlatEntity> StockQuantityActionV2(Context ctx, List<string> itemNos)
        {
            IStockQuantityService service = ServiceFactory.GetService<IStockQuantityService>(ctx);
            try
            {
                return service.StockQuantityActionV2(ctx, itemNos);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ServiceFactory.CloseService(service);
            }
        }

        /// <summary>
        /// 获取物料即时库存数
        /// </summary>
        /// <param name="ctx">上下文</param>
        /// <param name="masterID">物料分配内码</param>
        /// <returns></returns>
        public static DynamicObjectCollection InventoryQty(Context ctx, long masterID, List<long> orgid)
        {
            IStockQuantityService service = ServiceFactory.GetService<IStockQuantityService>(ctx);
            try
            {
                return service.InventoryQty(ctx, masterID, orgid);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ServiceFactory.CloseService(service);
            }
        }

        /// <summary>
        /// 获取物料可用库存(包含不良品)
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="orgid"></param>
        /// <param name="masterID"></param>
        /// <returns></returns>
        public static List<InventoryQtyV2Entity> InventoryQtyVStatus(Context ctx, long orgid, List<long> masterID)
        {
            IStockQuantityService service = ServiceFactory.GetService<IStockQuantityService>(ctx);
            try
            {
                return service.InventoryQtyVStatus(ctx, orgid, masterID);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ServiceFactory.CloseService(service);
            }
        }

        /// <summary>
        /// 云仓储根据供货组织、云存储仓库编号获取物料总库存量
        /// </summary>
        /// <param name="supplyOrgCode">供货组织编码</param>
        /// <param name="cloudStockCode">仓库编码</param>
        /// <param name="itemNos">物料编码</param>
        /// <returns></returns>
        public static List<CloudStockBaseQtyEntity> CloudStockBaseQty(Context ctx, CloudStockBaseQtyRequest request)
        {
            IStockQuantityService service = ServiceFactory.GetService<IStockQuantityService>(ctx);
            try
            {
                return service.CloudStockBaseQty(ctx, request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ServiceFactory.CloseService(service);
            }
        }

		/// <summary>
		/// MES根据型材，材质，长宽高模糊查询总库存
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="itemNos"></param>
		/// <returns></returns>
		public static List<MesFuzzyQueryStockBaseQtyEntity> GetMesFuzzyQueryStockBaseQty(Context ctx, FuzzyQueryStockBaseQtyRequest request)
		{
			IStockQuantityService service = ServiceFactory.GetService<IStockQuantityService>(ctx);
			try
			{
				return service.GetMesFuzzyQueryStockBaseQty(ctx, request);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				ServiceFactory.CloseService(service);
			}
		}

		/// <summary>
		/// 获取即时库存(MES专用)
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="itemNos"></param>
		/// <returns></returns>
		public static List<MesStockPlatEntity> MesStockQuantityAction(Context ctx, List<string> itemNos)
		{
			IStockQuantityService service = ServiceFactory.GetService<IStockQuantityService>(ctx);
			try
			{
				return service.MesStockQuantityAction(ctx, itemNos);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				ServiceFactory.CloseService(service);
			}
		}
	}
}
