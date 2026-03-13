using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.Kuayue
{
    public class KuayueSubscriptionRequest
    {
        /// <summary>
        /// 运单号批量最多20个，不能重复且是跨越单号
        /// </summary>
        public List<string> WaybillNumbers { get; set; }

        /// <summary>
        /// 为空时默认为订阅路由, 10-路由订阅，20-回单订阅，30-签收联订阅
        /// </summary>
        public List<string> Types { get; set; }
    }
}
