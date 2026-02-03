using System.Collections.Generic;

namespace Kingdee.Mymooo.Core.ApigatewayConfiguration
{
    public partial class RabbitMQMessage
    {
        /// <summary>
        /// Desc:交换机
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Exchange { get; set; } 

        /// <summary>
        /// Desc:路由
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Routingkey { get; set; } 

        /// <summary>
        /// Desc:头部信息
        /// Default:
        /// Nullable:True
        /// </summary>           
        public Dictionary<string, object> Headers { get; set; }

        /// <summary>
        /// Desc:关键字
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Keyword { get; set; }

        /// <summary>
        /// Desc:消息
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Message { get; set; } 
    }
}
