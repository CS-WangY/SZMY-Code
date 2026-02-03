using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.Mymooo.Contracts.ProductionManagement;
using Kingdee.Mymooo.Contracts;
using Kingdee.BOS;
using Kingdee.Mymooo.Contracts.PrdMoManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ProductionManagement;

namespace Kingdee.Mymooo.ServiceHelper.PrdMoManagement
{
    public class PrdMoServiceHelper
    {
        public static ResponseMessage<dynamic> StatusMoOrderAction(Context ctx, PrdMoStatus mostatus)
        {
            IMoOrderBillService service = ServiceFactory.GetService<IMoOrderBillService>(ctx);
            try
            {
                return service.StatusMoOrderAction(ctx, mostatus);
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
        public static ResponseMessage<dynamic> PrdPPBomChange(Context ctx, MesPrd_PPBOMRequest request)
        {
            IMoOrderBillService service = ServiceFactory.GetService<IMoOrderBillService>(ctx);
            try
            {
                return service.PrdPPBomChange(ctx, request);
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
        public static ResponseMessage<dynamic> PrdMoChangeWorkShop(Context ctx, PrdMoChangeWorkShopRequest request)
        {
            IMoOrderBillService service = ServiceFactory.GetService<IMoOrderBillService>(ctx);
            try
            {
                return service.PrdMoChangeWorkShop(ctx, request);
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
