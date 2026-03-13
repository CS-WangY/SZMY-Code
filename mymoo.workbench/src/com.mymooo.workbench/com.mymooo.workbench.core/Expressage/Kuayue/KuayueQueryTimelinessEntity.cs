using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.Kuayue
{
    /// <summary>
    /// 获取跨越物流时效请求数据
    /// </summary>
    public class KuayueQueryTimelinessEntity
    {
        /// <summary>
        /// 寄件地详细地址
        /// </summary>
        public string SendAddress { get; set; }
        /// <summary>
        /// 收件地详细地址
        /// </summary>
        public string CollectAddress { get; set; }
        /// <summary>
        /// 客户编码
        /// </summary>
        public string CustomerCode { get; set; }
        /// <summary>
        /// 寄件日期 时间格为: yyyy-MM-dd HH:mm
        /// </summary>
        public string MailingTime { get; set; }
    }
}
