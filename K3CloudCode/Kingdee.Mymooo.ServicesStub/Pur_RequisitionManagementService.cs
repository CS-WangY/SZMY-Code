using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.Mymooo.Core.DirectSaleManagement;
using Kingdee.BOS;
using Kingdee.Mymooo.Core;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.Mymooo.Business.PlugIn.DirectSaleManagement;
using Kingdee.Mymooo.Core.Utils;
using Newtonsoft.Json;
using System.IO;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Newtonsoft.Json.Serialization;
using Kingdee.Mymooo.ServiceHelper.PurRequisitionManagement;
using Kingdee.Mymooo.App.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Business.PlugIn.SalesManagement;
using Kingdee.Mymooo.Business.PlugIn.PurchaseManagement;
using Kingdee.Mymooo.ServiceHelper.PurchaseManagement;

namespace Kingdee.Mymooo.WebApi.ServicesStub
{
    public class Pur_RequisitionManagementService : KDBaseService
    {
        public Pur_RequisitionManagementService(KDServiceContext context) : base(context)
        {
        }
        /// <summary>
        /// 采购申请单业务终止
        /// </summary>
        /// <returns></returns>
        public string StatusPrOrderAction()
        {
            string data = string.Empty;

            using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
            {
                var task = reader.ReadToEndAsync();
                data = task.Result;
            }
            var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
            var request = JsonConvertUtils.DeserializeObject<PrStatus>(data);

            var oper = PurchaseOrderServiceHelper.StatusPrOrderAction(ctx, request);

            var settiongs = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return JsonConvert.SerializeObject(oper, Formatting.None, settiongs);
        }
        public string Mes_PUR_Requisition()
        {
            string data = string.Empty;

            using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
            {
                var task = reader.ReadToEndAsync();
                data = task.Result;
            }
            var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
            var request = JsonConvertUtils.DeserializeObject<PUR_Requisition>(data);
            //创建默认单据类型为标准采购申请
            request.BillTypeID = "93591469feb54ca2b08eb635f8b79de3";
            PurchaseOrderBusiness purOrder = new PurchaseOrderBusiness();
            var response = purOrder.Add_Mes_PUR_Requisition(ctx, request);
            return JsonConvertUtils.SerializeObject(response);
        }
        /// <summary>
        /// MES创建采购申请处理物料
        /// </summary>
        /// <returns></returns>
        public string SyncMes_PUR_RequisitionMaterial()
        {
            string data = string.Empty;

            using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
            {
                var task = reader.ReadToEndAsync();
                data = task.Result;
            }
            var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
            var request = JsonConvertUtils.DeserializeObject<PUR_Requisition>(data);

            PurchaseOrderBusiness purOrder = new PurchaseOrderBusiness();
            var response = purOrder.SyncMes_PUR_RequisitionMaterial(ctx, request);

            return JsonConvertUtils.SerializeObject(response);
        }
        /// <summary>
        /// SRM创建采购申请处理物料
        /// </summary>
        /// <returns></returns>
        public string SyncPUR_RequisitionMaterial()
        {
            string data = string.Empty;

            using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
            {
                var task = reader.ReadToEndAsync();
                data = task.Result;
            }
            var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
            var request = JsonConvertUtils.DeserializeObject<PUR_Requisition>(data);

            PurchaseOrderBusiness purOrder = new PurchaseOrderBusiness();
            var response = purOrder.SyncPUR_RequisitionMaterial(ctx, request);

            var settiongs = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return JsonConvert.SerializeObject(response, Formatting.None, settiongs);
        }


        public string CreatePUR_Requisition()
        {
            string data = string.Empty;

            using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
            {
                var task = reader.ReadToEndAsync();
                data = task.Result;
            }
            var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
            var request = JsonConvertUtils.DeserializeObject<PUR_Requisition>(data);
            //创建默认单据类型为备库采购申请
            request.BillTypeID = "63f48c5263b415";
            var oper = PurRequisitionBillServiceHelper.Add_PUR_Requisition(ctx, request);

            var settiongs = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return JsonConvert.SerializeObject(oper, Formatting.None, settiongs);
        }

        public string Del_PUR_Requisition()
        {
            string data = string.Empty;
            using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
            {
                var task = reader.ReadToEndAsync();
                data = task.Result;
            }
            var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
            //var request = JsonConvertUtils.DeserializeObject<PUR_Requisition>(data);
            //创建默认单据类型为备库采购申请
            //request.FBillTypeID = "63f48c5263b415";
            var response = PurRequisitionBillServiceHelper.Del_PUR_Requisition(ctx, data);
            var settiongs = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return JsonConvert.SerializeObject(response, Formatting.None, settiongs);
        }
    }
}
