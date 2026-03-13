using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage
{
    /// <summary>
    /// 取消物流单参数
    /// </summary>
    public class ExpressageCancelOrderRequest
    {
        /// <summary>
        /// 取消参数
        /// </summary>
        public string ExpressageCancelOrderRequestJsonStr { get; set; }

        /// <summary>
        /// 快递类型
        /// </summary>
        public string ExpressageCancelOrderRequestType { get; set; }
    }
}
