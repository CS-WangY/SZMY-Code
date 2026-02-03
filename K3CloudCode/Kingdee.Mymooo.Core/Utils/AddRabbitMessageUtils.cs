using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS;
using Kingdee.Mymooo.Core.BaseManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.Utils
{
    /// <summary>
    /// 写入rabbitmq消息队列
    /// </summary>
    public static class AddRabbitMessageUtils
    {
        public static ResponseMessage<dynamic> AddRabbitMessage(Context ctx, string message, string key, string keyworld)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            var result = JsonConvert.DeserializeObject<List<WXUserRequest>>(message);
            if (string.IsNullOrEmpty(keyworld))
            {
                keyworld = Convert.ToString(result.FirstOrDefault()?.Versions);
            }

            string sql = $"/*dialect*/ insert into RabbitMQScheduledMessage (FAction,FKeyword,FGuid,FMessage,FCreateDate) values  (@key,@keyworld,'',@message,SYSDATETIME())";
            List<SqlParam> paramList = new List<SqlParam>()
                {
                    new SqlParam("@key", KDDbType.String, key),
                    new SqlParam("@keyworld", KDDbType.String,keyworld),
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
