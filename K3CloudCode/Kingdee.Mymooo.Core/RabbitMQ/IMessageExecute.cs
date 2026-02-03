using Kingdee.BOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.RabbitMQ
{
    public interface IMessageExecute
    {
        ResponseMessage<dynamic> Execute(Context ctx, string message);
    }
}
