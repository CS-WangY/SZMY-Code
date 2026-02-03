using Kingdee.Mymooo.Contracts.StockManagement;
using Kingdee.Mymooo.Contracts;
using Kingdee.Mymooo.Core.StockManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS;

namespace Kingdee.Mymooo.ServiceHelper.StockManagement
{
    public class StockLockUnLockServiceHelper
    {
        public static DynamicObjectCollection SaveLockInfo(Context ctx, StockLockUnLockEntity stockLock, string billtype)
        {
            IStockLockUnLockService service = ServiceFactory.GetService<IStockLockUnLockService>(ctx);
            try
            {
                return service.SaveLockInfo(ctx, stockLock,billtype);
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
        public static void SaveUnLockInfo(Context ctx, StockLockUnLockEntity billno)
        {
            IStockLockUnLockService service = ServiceFactory.GetService<IStockLockUnLockService>(ctx);
            try
            {
                service.SaveUnLockInfo(ctx, billno);
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
