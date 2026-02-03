using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.Mymooo.Contracts;
using Kingdee.Mymooo.Contracts.BaseManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Core.ProductionManagement;

namespace Kingdee.Mymooo.ServiceHelper.BaseManagement
{
    public class ENGBomServiceHelper
    {
        public static ENGBomInfo TryGetOrAdd(Context ctx, ENGBomInfo request)
        {
            IENGBomService service = ServiceFactory.GetService<IENGBomService>(ctx);
            try
            {
                return service.TryGetOrAdd(ctx, request);
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
        public static ENGBomInfo[] TryGetOrAddsOrg(Context ctx, ENGBomInfo[] request, long[] orgid)
        {
            IENGBomService service = ServiceFactory.GetService<IENGBomService>(ctx);
            try
            {
                return service.TryGetOrAddsOrg(ctx, request, orgid);
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
        public static ENGBomInfo[] TryGetOrAdds(Context ctx, ENGBomInfo[] request)
        {
            IENGBomService service = ServiceFactory.GetService<IENGBomService>(ctx);
            try
            {
                return service.TryGetOrAdds(ctx, request);
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

        public static ResponseMessage<dynamic> MaterialAllocate(Context ctx, List<ENGBomInfo> request)
        {
            IENGBomService service = ServiceFactory.GetService<IENGBomService>(ctx);
            try
            {
                return service.MaterialAllocate(ctx, request);
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
        public static ResponseMessage<dynamic> BomAllocate(Context ctx, List<ENGBomInfo> request)
        {
            IENGBomService service = ServiceFactory.GetService<IENGBomService>(ctx);
            try
            {
                return service.BomAllocate(ctx, request);
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
        public static ResponseMessage<dynamic> BomAllocateByID(Context ctx, long[] request)
        {
            IENGBomService service = ServiceFactory.GetService<IENGBomService>(ctx);
            try
            {
                return service.BomAllocateByID(ctx, request);
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
        public static void SendMQAllocate(Context ctx, List<ENGBomInfo> request)
        {
            IENGBomService service = ServiceFactory.GetService<IENGBomService>(ctx);
            try
            {
                service.SendMQAllocate(ctx, request);
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
