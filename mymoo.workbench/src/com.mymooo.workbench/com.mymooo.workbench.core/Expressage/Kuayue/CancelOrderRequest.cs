using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.Kuayue
{
    public class CancelOrderRequest
    {
        public string customerCode { get; set; }
        public string waybillNumber { get; set; }
        public string xdCode { get; set; }
        public string platformFlag { get; set; }
    }

}
