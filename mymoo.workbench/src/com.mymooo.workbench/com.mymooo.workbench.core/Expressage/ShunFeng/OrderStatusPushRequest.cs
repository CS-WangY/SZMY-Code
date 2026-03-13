using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.ShunFeng
{
    public class OrderStatusPushRequest
    {
        public string requestId { get; set; }
        public long timestamp { get; set; }
        public Orderstate[] orderState { get; set; }

        public class Orderstate
        {
            public string lastTime { get; set; }
            public string orderNo { get; set; }
            public string empPhone { get; set; }
            public string netCode { get; set; }
            public string createTm { get; set; }
            public string empCode { get; set; }
            public string carrierCode { get; set; }
            public string orderStateDesc { get; set; }
            public string bookTime { get; set; }
            public string orderStateCode { get; set; }
            public string waybillNo { get; set; }
        }
    }
}
