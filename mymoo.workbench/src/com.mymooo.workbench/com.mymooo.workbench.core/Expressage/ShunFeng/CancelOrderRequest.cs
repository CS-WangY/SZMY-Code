using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.ShunFeng
{
    public class CancelOrderRequest
    {
        public CancelOrderRequest()
        {
            this.DealType = 2;
        }
        public string OrderId { get; set; }

        /// <summary>
        /// 	客户订单操作标识: 1:确认 (丰桥下订单接口默认自动确认，不需客户重复确认，该操作用在其它非自动确认的场景) 2:取消
        /// </summary>
        public int DealType { get; set; }
    }
}
