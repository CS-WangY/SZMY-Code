using Kingdee.BOS;
using Kingdee.Mymooo.Contracts.BaseManagement;
using Kingdee.Mymooo.Contracts.DirectSaleManagement;
using Kingdee.Mymooo.Contracts.PayableManagement;
using Kingdee.Mymooo.Contracts.PrdMoManagement;
using Kingdee.Mymooo.Contracts.ProductionManagement;
using Kingdee.Mymooo.Contracts.PurchaseManagement;
using Kingdee.Mymooo.Contracts.SalesManagement;
using Kingdee.Mymooo.Contracts.StockManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Contracts
{
    public class ServiceFactory
    {
        public static ServicesContainer _mapServer;

        private static bool noRegistered = true;

        public static T GetService<T>(Context ctx)
        {
            if (noRegistered)
            {
                RegisterService();
            }
            T service = _mapServer.GetService<T>(typeof(T).AssemblyQualifiedName, ctx.ServerUrl);
            if (service == null)
            {
                throw new KDException("???", "instance == null");
            }
            return service;
        }

        public static void CloseService(object service)
        {
            if (service is IDisposable)
            {
                var disposable = service as IDisposable;
                disposable.Dispose();
            }
        }

        public static void RegisterService()
        {
            _mapServer = new ServicesContainer();
            lock (_mapServer)
            {
                if (noRegistered)
                {
                    _mapServer.Add(typeof(IMymoooBusinessDataService), "Kingdee.Mymooo.App.Core.MymoooBusinessDataService,Kingdee.Mymooo.App.Core");
                    _mapServer.Add(typeof(IDemoTestService), "Kingdee.Mymooo.App.Core.DemoTestService,Kingdee.Mymooo.App.Core");
                    _mapServer.Add(typeof(ISalesOrderService), "Kingdee.Mymooo.App.Core.SalesManagement.SalesOrderService,Kingdee.Mymooo.App.Core");
                    _mapServer.Add(typeof(IMaterialService), "Kingdee.Mymooo.App.Core.BaseManagement.MaterialService,Kingdee.Mymooo.App.Core");
                    _mapServer.Add(typeof(IPurchaseOrderService), "Kingdee.Mymooo.App.Core.PurchaseManagement.PurchaseOrderService,Kingdee.Mymooo.App.Core");
                    _mapServer.Add(typeof(IBasicDataSyncService), "Kingdee.Mymooo.App.Core.BaseManagement.BasicDataSyncService,Kingdee.Mymooo.App.Core");
                    _mapServer.Add(typeof(IENGBomService), "Kingdee.Mymooo.App.Core.BaseManagement.ENGBomService,Kingdee.Mymooo.App.Core");
                    _mapServer.Add(typeof(IStockQuantityService), "Kingdee.Mymooo.App.Core.StockManagement.StockQuantityService,Kingdee.Mymooo.App.Core");
                    _mapServer.Add(typeof(IDirectSaleService), "Kingdee.Mymooo.App.Core.DirectSaleManagement.DirectSaleService,Kingdee.Mymooo.App.Core");
                    _mapServer.Add(typeof(IStockLockUnLockService), "Kingdee.Mymooo.App.Core.StockManagement.StockLockUnLockService,Kingdee.Mymooo.App.Core");
                    _mapServer.Add(typeof(IPrBillToPoBillService), "Kingdee.Mymooo.App.Core.PurchaseManagement.PrBillToPoBillService,Kingdee.Mymooo.App.Core");
                    _mapServer.Add(typeof(IPur_RequisitionOrderService), "Kingdee.Mymooo.App.Core.PurchaseManagement.Pur_RequisitionOrderService,Kingdee.Mymooo.App.Core");
                    _mapServer.Add(typeof(ISupplyOrgService), "Kingdee.Mymooo.App.Core.BaseManagement.SupplyOrgService,Kingdee.Mymooo.App.Core");
                    _mapServer.Add(typeof(IUserService), "Kingdee.Mymooo.App.Core.BaseManagement.UserService,Kingdee.Mymooo.App.Core");
                    _mapServer.Add(typeof(IProductionMoService), "Kingdee.Mymooo.App.Core.ProductionManagement.ProductionMoService,Kingdee.Mymooo.App.Core");
                    _mapServer.Add(typeof(IStockOrderService), "Kingdee.Mymooo.App.Core.StockManagement.StockOrderService,Kingdee.Mymooo.App.Core");
                    _mapServer.Add(typeof(IPayableOrderService), "Kingdee.Mymooo.App.Core.PayableManagement.PayableOrderService,Kingdee.Mymooo.App.Core");
                    _mapServer.Add(typeof(IMoOrderBillService), "Kingdee.Mymooo.App.Core.PrdMoManagement.MoOrderBillService,Kingdee.Mymooo.App.Core");
                    noRegistered = false;
                }
            }
        }
    }
}
