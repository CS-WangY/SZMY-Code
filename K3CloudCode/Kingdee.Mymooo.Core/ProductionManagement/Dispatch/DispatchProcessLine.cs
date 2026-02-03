using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    public class DispatchProcessLine
    {
        /// <summary>
        /// （***必要参数***）加工顺序，一般步长10，比如：0010、0020
        /// </summary>
        public string operationNum { get; set; }

        /// <summary>
        /// ***必要参数***）工序代码，比如：IMCNC1
        /// 工序代码需按照约定好的填写
        /// </summary>
        public string processId { get; set; }

        /// <summary>
        /// （***必要参数***）工序名称，一般为中文名称，用来解释processId
        /// </summary>
        public string processName { get; set; }

        /// <summary>
        /// （***必要参数***）加工内容详情
        /// </summary>
        public string processInfo { get; set; }

        /// <summary>
        /// 工序难度等级，可输入：10、20、30，其中：10 - A等级，最难图纸；20 - B等级，普通难度图纸；30 - C等级，简单图纸
        /// </summary>
        public int level { get; set; } = 20;

        /// <summary>
        /// 准备时长，默认以分钟为单位
        /// </summary>
        public float setupTime { get; set; }

        /// <summary>
        /// 加工时长，默认以分钟为单位
        /// </summary>
        public float processTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }
    }
}
