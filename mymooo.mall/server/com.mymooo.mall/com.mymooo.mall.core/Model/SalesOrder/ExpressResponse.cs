using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.SalesOrder
{

    public class ExpressResponse
    {
        public string Number { get; set; }

        public long PurchasedOrderId { get; set; }

        public string Name { get; set; }

        public string Flow { get; set; }

        public bool IsCheck { get; set; }
        /// <summary>
        /// 快递公司编号
        /// </summary>
        public string ExpressCompanyCode { get; set; }

        // 订单是否终结.  只有签收才终结. 不包括对异常单的处理逻辑,非终结状态,都转到快递100去查询
        // public bool ExpressIsOver { get; set; }


    }


}
