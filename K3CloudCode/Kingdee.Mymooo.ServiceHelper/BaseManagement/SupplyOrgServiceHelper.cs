using Kingdee.Mymooo.Contracts.BaseManagement;
using Kingdee.Mymooo.Contracts;
using Kingdee.Mymooo.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core;

namespace Kingdee.Mymooo.ServiceHelper.BaseManagement
{
    public class SupplyOrgServiceHelper
    {
        public static string GetSupplyOrgBusinessDivision(Context ctx, long supplyOrgId)
        {
            ISupplyOrgService service = ServiceFactory.GetService<ISupplyOrgService>(ctx);
            try
            {
                return service.GetSupplyOrgBusinessDivision(ctx, supplyOrgId);
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
        /// 获取华东五部的供应商信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="supplyOrgId"></param>
        /// <returns></returns>
        public static void SenMesSupplyInfo(Context ctx, string supplierCode)
        {
            ISupplyOrgService service = ServiceFactory.GetService<ISupplyOrgService>(ctx);
            try
            {
                service.SenMesSupplyInfo(ctx, supplierCode);
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
		/// 供应商供应产品小类
		/// </summary>
		/// <param name="ctx"></param>
		/// <returns></returns>
		public static ResponseMessage<dynamic> SupplierSmallService(Context ctx, SupplierSmallRequest[] requests)
		{
			ISupplyOrgService service = ServiceFactory.GetService<ISupplyOrgService>(ctx);
			try
			{
				return service.SupplierSmallService(ctx, requests);
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
