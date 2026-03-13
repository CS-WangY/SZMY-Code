using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage
{
    /// <summary>
    /// 创建快递单请求参数
    /// </summary>
    public class ExpressageCreateOrderRequest
    {
        /// <summary>
        /// 数据
        /// </summary>
        public string ExpressageCreateOrderRequestJsonStr { get; set; }
        /// <summary>
        /// 快递类型
        /// </summary>
        public string ExpressageCreateOrderType { get; set; }
    }
}
