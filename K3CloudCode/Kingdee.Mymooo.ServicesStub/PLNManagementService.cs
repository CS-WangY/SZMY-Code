using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.Exceptions;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.App.Core;
using Kingdee.Mymooo.Business.PlugIn.PLN_Forecast;
using Kingdee.Mymooo.Business.PlugIn.PurchaseManagement;
using Kingdee.Mymooo.Business.PlugIn.SalesManagement;
using Kingdee.Mymooo.Business.PlugIn.StockManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.SalesManagement.SalesOrderScheduled;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Kingdee.Mymooo.ServiceHelper.SalesManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.WebApi.ServicesStub
{
    public class PLNManagementService : KDBaseService
    {
        public PLNManagementService(KDServiceContext context) : base(context)
        {
        }
        public string ClosedOrderAction()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response;
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                ApprovalMessageRequest request = JsonConvertUtils.DeserializeObject<ApprovalMessageRequest>(data);
                //response = SalesOrderServiceHelper.ClosedSalesOrderAction(ctx, request);
                PLN_ForecastBusiness plnOrder = new PLN_ForecastBusiness();
                response = plnOrder.ClosedForecastOrderAction(ctx, request);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }
    }
}
