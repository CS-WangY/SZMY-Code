using Kingdee.Mymooo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using System.Threading.Tasks;
using Kingdee.Mymooo.Contracts.BaseManagement;
using Kingdee.Mymooo.Contracts;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.BOS.App.Data;
using Kingdee.Mymooo.Core.SalesManagement;

namespace Kingdee.Mymooo.ServiceHelper.BaseManagement
{
    public class BasicDataSyncServiceHelper
    {
        public static ResponseMessage<dynamic> SyncSaleCust(Context ctx, long custId, List<KeyValuePair<long, long>> salesId, bool isFirstSync, List<string> userCode, List<string> orderNumber, string transferUserCode)
        {
            IBasicDataSyncService service = ServiceFactory.GetService<IBasicDataSyncService>(ctx);
            try
            {
                return service.SyncSaleCust(ctx, custId, salesId, isFirstSync, userCode, orderNumber, transferUserCode);
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
        public static ResponseMessage<dynamic> UnBindSaleCust(Context ctx, long custId)
        {
            IBasicDataSyncService service = ServiceFactory.GetService<IBasicDataSyncService>(ctx);
            try
            {
                return service.UnBindSaleCust(ctx, custId);
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

        public static ResponseMessage<dynamic> AddOrMotifyCustomer(Context ctx, CustomerRequest request)
        {
            IBasicDataSyncService service = ServiceFactory.GetService<IBasicDataSyncService>(ctx);
            try
            {
                return service.AddOrMotifyCustomer(ctx, request);
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

        public static ResponseMessage<dynamic> AddOrMotifySupplier(Context ctx, SupplierRequest request)
        {
            IBasicDataSyncService service = ServiceFactory.GetService<IBasicDataSyncService>(ctx);
            try
            {
                return service.AddOrMotifySupplier(ctx, request);
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

        public static ResponseMessage<dynamic> AddSupplierAllotOrg(Context ctx, List<SupplierAllotOrgRequest> request)
        {
            IBasicDataSyncService service = ServiceFactory.GetService<IBasicDataSyncService>(ctx);
            try
            {
                return service.AddSupplierAllotOrg(ctx, request);
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

        public static long GetSaleId(Context ctx, long orgId, string weChatCode)
        {
            IBasicDataSyncService service = ServiceFactory.GetService<IBasicDataSyncService>(ctx);
            try
            {
                return service.GetSaleId(ctx, orgId, weChatCode);
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
        /// CRM同步客诉到金蝶
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static ResponseMessage<dynamic> SynCompanyComplaintService(Context ctx, CompanyComplaintRequest request)
        {
            IBasicDataSyncService service = ServiceFactory.GetService<IBasicDataSyncService>(ctx);
            try
            {
                return service.SynCompanyComplaintService(ctx, request);
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
        /// 周合格率统计
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static ResponseMessage<dynamic> WeeklyPassRateStatisticsService(Context ctx)
        {
            IBasicDataSyncService service = ServiceFactory.GetService<IBasicDataSyncService>(ctx);
            try
            {
                return service.WeeklyPassRateStatisticsService(ctx);
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
        /// 根据认岗信息完善业务员业务组信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static List<KeyValuePair<long, long>> SyncSAL_SC_SalerCust(Context ctx, List<SalerCustList> keys)
        {
            IBasicDataSyncService service = ServiceFactory.GetService<IBasicDataSyncService>(ctx);
            try
            {
                return service.SyncSAL_SC_SalerCust(ctx, keys);
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
