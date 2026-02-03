using Kingdee.Mymooo.Contracts.SalesManagement;
using Kingdee.Mymooo.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Core;
using Kingdee.BOS;
using Kingdee.Mymooo.Contracts.PurchaseManagement;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.Mymooo.Core.PurchaseManagement;

namespace Kingdee.Mymooo.ServiceHelper.PurchaseManagement
{
    public class PrBillToPoBillServiceHelper
    {
        /// <summary>
        /// PrToPo服务
        /// </summary>
        /// <param name="ctx"></param>
        public static IOperationResult PrBillToPoBillAction(Context ctx, List<PrToPoPurchaseRequireEntity> list)
        {
            IPrBillToPoBillService service = ServiceFactory.GetService<IPrBillToPoBillService>(ctx);
            try
            {
                return service.PrBillToPoBillAction(ctx, list);
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
