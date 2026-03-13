using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.Kuayue
{
    /// <summary>
    /// 跨越预估运费请求参数
    /// </summary>
    public class KuayueQueryFreightChargeRequest
    {
        /// <summary>
        /// 平台标识
        /// </summary>
        public string PlatformFlag { get; set; }

        /// <summary>
        /// 寄件客户编码
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// 始发区号
        /// </summary>
        public string BeginAreaCode { get; set; }

        /// <summary>
        /// 目的区号
        /// </summary>
        public string EndAreaCode { get; set; }
        /// <summary>
        /// 货好时间
        /// </summary>
        public string BillingTime { get; set; }

        /// <summary>
        /// 付款公司编码(可以跟寄件客户编码一致)
        /// </summary>
        public string PickupCustomerCode { get; set; }

        /// <summary>
        /// 计费重量
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// 件数
        /// </summary>
        public int Unit { get; set; }
        /// <summary>
        /// 方数
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// 经营重量
        /// </summary>
        public int ManageWeight { get; set; }

        /// <summary>
        /// 始发城市
        /// </summary>
        public string BeginCityName { get; set; }

        /// <summary>
        /// 目的城市
        /// </summary>
        public string EndCityName { get; set; }
        /// <summary>
        /// 始发地址
        /// </summary>
        public string BeginAddress { get; set; }

        /// <summary>
        /// 目的地址
        /// </summary>
        public string EndAddress { get; set; }
    }
}
