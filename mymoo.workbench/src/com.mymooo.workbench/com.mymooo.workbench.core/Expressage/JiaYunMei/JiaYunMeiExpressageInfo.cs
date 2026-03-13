using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.JiaYunMei
{
    public class JiaYunMeiExpressageInfo
    {
        public string Url { get; set; }

        public string RouterUrl { get; set; }
        /// <summary>
        /// 客户Id
        /// </summary>
        public string CooperatorCode { get; set; }
        /// <summary>
        /// 认证key
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 获取物流轨迹专用Key
        /// </summary>
        public string RouterKey { get; set; }
        /// <summary>
        /// 取件员编号
        /// </summary>
        public string RegisterCode { get; set; }
        /// <summary>
        /// 寄件网点编号
        /// </summary>
        public string SendSiteCode { get; set; }
        /// <summary>
        /// 客户编号
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 客户类型 (P为平台，W为网点）
        /// </summary>
        public string CustomerType { get; set; }
    }
}
