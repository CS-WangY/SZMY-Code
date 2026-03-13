using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.SuTeng
{
    public class SuTengQueryRoute
    {
        /// <summary>
        /// 
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<SuTengRouteDataItem> data { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string msg { get; set; }
    }

    public class SuTengRouteDataItem
    {
        /// <summary>
        /// 运单编号
        /// </summary>
        public string billCode { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public int optTime { get; set; }
        /// <summary>
        /// 操作类型
        /// </summary>
        public string optType { get; set; }
    }
}
