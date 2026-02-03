using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.Mymooo.Business.PlugIn.PurchaseManagement;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.Utils;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Kingdee.Mymooo.Core.PlanOrderManagement;
using Kingdee.Mymooo.Business.PlugIn.PlanManagement;

namespace Kingdee.Mymooo.WebApi.ServicesStub
{
    public class PlanManagementService : KDBaseService
    {
        public PlanManagementService(KDServiceContext context) : base(context)
        {
        }
        public string SyncMES2PlanSplitService()
        {
            string data = string.Empty;

            using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
            {
                var task = reader.ReadToEndAsync();
                data = task.Result;
            }
            var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
            var request = JsonConvertUtils.DeserializeObject<PlanOrderSplitEntity>(data);

            PlanOrderBillService plnOrder = new PlanOrderBillService();
            var response = plnOrder.SyncMES2PlanSplitService(ctx, request);

            return JsonConvertUtils.SerializeObject(response);
        }
        public string Mes2ErpCreateMo()
        {
            string data = string.Empty;

            using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
            {
                var task = reader.ReadToEndAsync();
                data = task.Result;
            }
            var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
            var request = JsonConvertUtils.DeserializeObject<PlanOrderSplitEntity>(data);

            PlanOrderBillService plnOrder = new PlanOrderBillService();
            var response = plnOrder.Mes2ErpCreateMo(ctx, request);

            return JsonConvertUtils.SerializeObject(response);
        }

        public string mes_SyncOrderMaterialService()
        {
            string data = string.Empty;

            using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
            {
                var task = reader.ReadToEndAsync();
                data = task.Result;
            }
            var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
            var request = JsonConvertUtils.DeserializeObject<PlanOrderSplitEntity>(data);

            PlanOrderBillService plnOrder = new PlanOrderBillService();
            var response = plnOrder.mes_SyncOrderMaterialService(ctx, request);

            return JsonConvertUtils.SerializeObject(response);
        }
        public string mes_SyncOrderService()
        {
            string data = string.Empty;

            using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
            {
                var task = reader.ReadToEndAsync();
                data = task.Result;
            }
            var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
            var request = JsonConvertUtils.DeserializeObject<PlanOrderSplitEntity>(data);

            PlanOrderBillService plnOrder = new PlanOrderBillService();
            var response = plnOrder.mes_SyncOrderService(ctx, request);

            return JsonConvertUtils.SerializeObject(response);
        }
        public string mes_AllocateMaterialAllService()
        {
            string data = string.Empty;

            using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
            {
                var task = reader.ReadToEndAsync();
                data = task.Result;
            }
            var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
            var request = JsonConvertUtils.DeserializeObject<PlanOrderSplitEntity>(data);

            PlanOrderBillService plnOrder = new PlanOrderBillService();
            var response = plnOrder.mes_AllocateMaterialAllService(ctx, request);

            return JsonConvertUtils.SerializeObject(response);
        }
        public string mes_AllocateBomAllService()
        {
            string data = string.Empty;

            using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
            {
                var task = reader.ReadToEndAsync();
                data = task.Result;
            }
            var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
            var request = JsonConvertUtils.DeserializeObject<PlanOrderSplitEntity>(data);

            PlanOrderBillService plnOrder = new PlanOrderBillService();
            var response = plnOrder.mes_AllocateBomAllService(ctx, request);

            return JsonConvertUtils.SerializeObject(response);
        }
    }
}
