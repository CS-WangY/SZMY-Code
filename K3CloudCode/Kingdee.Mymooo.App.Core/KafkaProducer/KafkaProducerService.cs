using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;

namespace Kingdee.Mymooo.App.Core.KafkaProducer
{
    public class KafkaProducerService
    {
        public void AddMessage(Context context, params RabbitMQMessage[] messages)
        {
            if (messages.Length == 0)
            {
                return;
            }
            var result = ApigatewayUtils.InvokeWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/RabbitMQ/GetMessageId?count={messages.Length}");
            var messageIds = JsonConvertUtils.DeserializeObject<ResponseMessage<List<long>>>(result);
            if (!messageIds.IsSuccess)
            {
                throw new Exception(messageIds.Message ?? messageIds.ErrorMessage);
            }
            var sql = $@"insert into RabbitMQMessage_{DateTime.Now:yyyyMM01}(MessageId,VirtualHost,Exchange,Routingkey,EnvCode,Message,Keyword,CreateDate,CreateUserId,CreateUserName)
                        values(@MessageId,'/erp',@Exchange,@Routingkey,@EnvCode,@Message,@Keyword,@CreateDate,@CreateUserId,@CreateUserName)";
            List<SqlObject> sqlObjects = new List<SqlObject>();
            int index = 0;
            foreach (var message in messages)
            {
                List<SqlParam> sqlParams = new List<SqlParam>()
                {
                    new SqlParam("@MessageId", KDDbType.Int64, messageIds.Data[index]),
                    new SqlParam("@Exchange", KDDbType.String, message.Exchange),
                    new SqlParam("@Routingkey", KDDbType.String, message.Routingkey),
#if DEBUG
					new SqlParam("@EnvCode", KDDbType.String, ApigatewayUtils.ApigatewayConfig.EnvCode),
#else
					new SqlParam("@EnvCode", KDDbType.String, null),
#endif
					new SqlParam("@Message", KDDbType.String, message.Message),
                    new SqlParam("@Keyword", KDDbType.String, message.Keyword),
                    new SqlParam("@CreateDate", KDDbType.DateTime, DateTime.Now),
                    new SqlParam("@CreateUserId", KDDbType.Int64, context.UserId),
                    new SqlParam("@CreateUserName", KDDbType.String, context.UserName),
                };
                sqlObjects.Add(new SqlObject(sql, sqlParams));
                index++;
            }
            DBUtils.ExecuteBatch(context, sqlObjects);
        }
    }
}
