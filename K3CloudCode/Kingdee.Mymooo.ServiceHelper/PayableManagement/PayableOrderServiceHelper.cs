using Kingdee.Mymooo.Contracts;
using Kingdee.Mymooo.Core.PayableManagement;
using Kingdee.Mymooo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.Mymooo.Contracts.PayableManagement;

namespace Kingdee.Mymooo.ServiceHelper.PayableManagement
{
	public class PayableOrderServiceHelper
	{
		/// <summary>
		/// MES费用采购下推费用应付
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="entry"></param>
		/// <returns></returns>
		public static ResponseMessage<dynamic> MesCostPurGenerateCostPayableService(Context ctx, MesCostPurGenerateCostPayableRequest request)
		{
			IPayableOrderService service = ServiceFactory.GetService<IPayableOrderService>(ctx);
			try
			{
				return service.MesCostPurGenerateCostPayableService(ctx, request);
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
