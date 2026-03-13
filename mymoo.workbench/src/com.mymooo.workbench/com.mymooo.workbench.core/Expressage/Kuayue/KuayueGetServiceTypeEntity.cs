using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.Kuayue
{
    public class KuayueGetServiceTypeEntity
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
        /// 付款方式
        /// </summary>
        public string PayMode { get; set; }
        /// <summary>
        /// 寄件公司编码
        /// </summary>
        public string ShipingCustomerCode { get; set; }
        /// <summary>
        /// 付款公司编码
        /// </summary>
        public string PaymentCustomerCode { get; set; }
        /// <summary>
        /// 货好时间
        /// </summary>
        public string BillTime { get; set; }
    }
}
