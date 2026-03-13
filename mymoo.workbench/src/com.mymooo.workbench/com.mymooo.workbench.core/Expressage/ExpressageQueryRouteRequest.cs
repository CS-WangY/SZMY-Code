using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage
{
    /// <summary>
    /// 查询物流参数
    /// </summary>
    public class ExpressageQueryRouteRequest
    {
        /// <summary>
        /// 参数
        /// </summary>
        public string ExpressageQueryRouteRequestJsonStr { get; set; }

        /// <summary>
        /// 快递类型
        /// </summary>
        public string ExpressageQueryRouteRequestType { get; set; }
    }
}
