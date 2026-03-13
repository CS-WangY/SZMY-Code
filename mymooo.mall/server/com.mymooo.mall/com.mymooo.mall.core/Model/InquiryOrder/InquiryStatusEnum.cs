using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.InquiryOrder
{

    /// <summary>
    /// 询价单报价状态。
    /// </summary>
    public enum InquiryStatus
    {
        /// <summary>
        /// 待报价。
        /// </summary>
        WaitingForQuote = 0,
        /// <summary>
        /// 不报价。
        /// </summary>
        NoQuotation = 1,
        /// <summary>
        /// 已报价。
        /// </summary>
        Quoted = 2,
        /// <summary>
        /// 已订购。
        /// </summary>
        Purchased = 3,
        /// <summary>
        /// 已取消。
        /// </summary>
        Cancel = 4,
        /// <summary>
        /// 已失效。
        /// </summary>
        Invalid = 5
    }
}
