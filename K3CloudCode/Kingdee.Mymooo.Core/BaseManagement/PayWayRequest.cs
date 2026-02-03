using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BaseManagement
{
    public class PayWayRequest
    {
        public string PayWayCode { get; set; }
        public string PayMothodCode { get; set; }

        /// <summary>
        /// 对账日期
        /// </summary>
        public int ReconctliationTime { get; set; }
        /// <summary>
        /// 开票时间
        /// </summary>
        public int InvoicingTime { get; set; }

        /// <summary>
        /// 付款时间
        /// </summary>
        public int PaymentTime { get; set; }

        public string Code { get; set; }
    }
}
