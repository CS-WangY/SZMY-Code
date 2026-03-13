using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.DeBang
{
    public class TraceQueryResponse
    {
        public string result { get; set; }
        public string reason { get; set; }
        public string resultCode { get; set; }
        public Responseparam responseParam { get; set; }
        public string uniquerRequestNumber { get; set; }

        public class Responseparam
        {
            public object[] trace_list { get; set; }
            public string tracking_number { get; set; }
        }
    }
}
