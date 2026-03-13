using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.InquiryOrder
{


    /// <summary>
    /// 订单状态枚举
    /// </summary>
    public enum InquiryDataSouce
    {
        /// <summary>
        /// Web Desktop
        /// </summary>
        WebDesktop = 0,
        /// <summary>
        /// Android App
        /// </summary>
        Android = 1,
        /// <summary>
        /// iOS App
        /// </summary>
        IOS = 2,
        /// <summary>
        /// Web Mobile
        /// </summary>
        WebMobile = 3,
    }
}
