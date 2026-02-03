using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Rpc;
using Kingdee.Mymooo.Core.StockManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Contracts.StockManagement
{
    /// <summary>
    /// 锁库解锁服务
    /// </summary>
    [RpcServiceError]
    [ServiceContract]
    public interface IStockLockUnLockService
    {
        /// <summary>
        /// 锁库
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        DynamicObjectCollection SaveLockInfo(Context ctx, StockLockUnLockEntity stockLock,string billtype);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void SaveUnLockInfo(Context ctx, StockLockUnLockEntity billno);
    }
}
