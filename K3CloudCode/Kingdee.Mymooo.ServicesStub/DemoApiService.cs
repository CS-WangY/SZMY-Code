using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.Mymooo.Business.PlugIn.RabbitMQExecuteService;
using Kingdee.Mymooo.Business.PlugIn.StockManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.Utils;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace Kingdee.Mymooo.WebApi.ServicesStub
{
    public class DemoApiService : KDBaseService
    {
        public DemoApiService(KDServiceContext context)
        : base(context)
        {
        }


        public string ReceiveMessage()
        {
            string data = "";
            using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
            {
                var task = reader.ReadToEndAsync();
                data = task.Result;
            }
            return data;
        }

        public string CreateDemoBill()
        {
            string data = "";
            using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
            {
                var task = reader.ReadToEndAsync();
                data = task.Result;
            }
            try
            {
                DemoBillExecute demoBillExecute = new DemoBillExecute();
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                //demoBillExecute.Execute(ctx, data);
                return "创建单据成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
