using Kingdee.BOS;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.SalesManagement
{
    public class ApigatewayTaskBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            ApigatewayTaskInfo apigateway = JsonConvertUtils.DeserializeObject<ApigatewayTaskInfo>(message);

            return JsonConvertUtils.DeserializeObject<ResponseMessage<dynamic>>(ApigatewayUtils.InvokePostRabbitService(apigateway.Url, apigateway.Message));
        }
    }
}
