using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.ShunFeng
{
    public class OrderRoutePushRequest
    {
        public OrderRoutePushBody Body { get; set; }

        public class OrderRoutePushBody
        {
            public Waybillroute[] WaybillRoute { get; set; }
        }

        public class Waybillroute
        {
            public string mailno { get; set; }
            public string acceptAddress { get; set; }
            public string reasonName { get; set; }
            public string orderid { get; set; }
            public string acceptTime { get; set; }
            public string remark { get; set; }
            public string opCode { get; set; }
            public string id { get; set; }
            public string reasonCode { get; set; }
        }
    }
}
