using Kingdee.BOS.Rpc;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.Mymooo.Core.StockManagement;

namespace Kingdee.Mymooo.Contracts.StockManagement
{
    /// <summary>
    /// 库存订单服务
    /// </summary>
    [RpcServiceError]
    [ServiceContract]
    public interface IStockOrderService
    {

        /// <summary>
        /// MES生成盘盈单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> MesGenerateStkCountGainAndLossService(Context ctx, MesGenerateStkCountGainAndLossRequest request);
        /// <summary>
        /// MES生成其他入库单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ResponseMessage<dynamic> MesStkMiscellaneousService(Context ctx, Mes_STK_MiscellaneousRequest request);
    }
}
