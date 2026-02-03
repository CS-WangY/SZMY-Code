using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ApigatewayConfiguration
{
    public class K3CloudRabbitMQMessage<T, D>
    {
        //
        // 摘要:
        //     表单数据Id
        public long Id { get; set; }

        //
        // 摘要:
        //     表单
        public string FormId { get; set; } = string.Empty;


        //
        // 摘要:
        //     单据编码
        public string BillNo { get; set; } = string.Empty;


        //
        // 摘要:
        //     操作编码
        public string OperationNumber { get; set; } = string.Empty;

        public T Head { get; set; }
        public List<D> Details { get; set; }
    }
}
