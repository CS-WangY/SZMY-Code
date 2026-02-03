using Kingdee.Mymooo.Contracts.SalesManagement;
using Kingdee.Mymooo.Contracts;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Core.StockManagement;
using Kingdee.BOS;
using Kingdee.Mymooo.Contracts.StockManagement;

namespace Kingdee.Mymooo.ServiceHelper.StockManagement
{
    public class StockOrderServiceHelper
    {
        public static ResponseMessage<dynamic> MesStkMiscellaneousService(Context ctx, Mes_STK_MiscellaneousRequest request)
        {
            IStockOrderService service = ServiceFactory.GetService<IStockOrderService>(ctx);
            try
            {
                return service.MesStkMiscellaneousService(ctx, request);
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
        /// MES生成盘盈盘亏单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>

        public static ResponseMessage<dynamic> MesGenerateStkCountGainAndLossService(Context ctx, MesGenerateStkCountGainAndLossRequest request)
        {
            IStockOrderService service = ServiceFactory.GetService<IStockOrderService>(ctx);
            try
            {
                return service.MesGenerateStkCountGainAndLossService(ctx, request);
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
