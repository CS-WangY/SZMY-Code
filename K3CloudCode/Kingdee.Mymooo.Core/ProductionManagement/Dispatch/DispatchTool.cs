using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    public class DispatchTool
    {
        /// <summary>
        /// （***必要参数***）加工顺序，一般步长10，比如：0010、0020
        /// </summary>
        public string operationNum { get; set; }
        /// <summary>
        /// ***必要参数***）工序代码，比如：IMCNC1
        /// </summary>
        public string processId { get; set; }
        /// <summary>
        /// 刀具清单
        /// </summary>
        public List<DispatchToolItem> toolListVOList { get; set; } = new List<DispatchToolItem>();
    }
}
