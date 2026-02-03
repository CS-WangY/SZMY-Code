using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.RabbitMQ
{
    public class RabbitMQDataMessage<T>
    {
        public string Action { get; set; }
        public string Guid { get; set; }
        public string KeyWord { get; set; }
        public string CallbackQueneName { get; set; }

        public T Data { get; set; }
    }
}
