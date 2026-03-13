using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage
{
    /// <summary>
    /// 修改物流信息参数
    /// </summary>
    public class ExpressageUpdateOrderRequest
    {
        /// <summary>
        /// 数据
        /// </summary>
        public string ExpressageUpdateOrderRequestJsonStr { get; set; }

        /// <summary>
        /// 快递类型
        /// </summary>
        public string ExpressageUpdateOrderRequestType { get; set; }
    }
}
