using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.ShunFeng
{
    public class SearchRouteRequest
    {
        public SearchRouteRequest()
        {
            this.MethodType = "1";
            this.Language = "zh-cn";
            this.TrackingType = "1";
        }

        public string Language { get; set; }

        /// <summary>
        /// 查询号类别:
        ///   1:根据顺丰运单号查询,trackingNumber将被当作顺丰运单号处理
        ///   2:根据客户订单号查询,trackingNumber将被当作客户订单号处理
        /// </summary>
        public string TrackingType { get; set; }

        /// <summary>
        /// 查询号:
        /// trackingType=1,则此值为顺丰运单号
        /// 如果trackingType = 2, 则此值为客户订单号
        /// </summary>
        public string[] TrackingNumber { get; set; }

        /// <summary>
        /// 路由查询类别:
        ///   1:标准路由查询
        ///   2:定制路由查询
        /// </summary>
        public string MethodType { get; set; }
    }

}
