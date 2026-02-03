using Kingdee.BOS;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.StockManagement
{
    public class CloudStockPackagedService
    {
        private ResponseMessage<dynamic> AddRabbitMessage(Context ctx, string message, string key)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            var result = JsonConvert.DeserializeObject<List<WXUserRequest>>(message);
            long vesion = result.FirstOrDefault().Versions;
            string sql = $"/*dialect*/ insert into RabbitMQScheduledMessage (FAction,FKeyword,FGuid,FMessage,FCreateDate) values  ('{key}',@keyworld,'',@message,SYSDATETIME())";
            List<SqlParam> paramList = new List<SqlParam>()
                {
                    new SqlParam("@keyworld", KDDbType.String, vesion.ToString()),
                new SqlParam("@message", KDDbType.String,message)
                };
            int i = DBServiceHelper.Execute(ctx, sql, paramList);
            if (i > 0)
            {
                response.Code = ResponseCode.Success;
                return response;
            }
            else
            {
                response.Code = ResponseCode.Exception;
                response.Message = "执行失败";
                return response;
            }
        }
    }
}
