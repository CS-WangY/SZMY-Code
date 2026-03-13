using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Quotation
{
    public class QueryCappCacheRequest
    {
        /// <summary>
        /// 报价单号
        /// </summary>
        public string? InquiryOrder { get; set; }
        /// <summary>
        /// 图号
        /// </summary>
        public string? DrawingNumber { get; set; }
    }
}
