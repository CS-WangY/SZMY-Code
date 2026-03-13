using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.DeBang
{
    public class OrderStatusPushRequest
    {
        public string comments { get; set; }
        public object[] fieldList { get; set; }
        public string gmtUpdated { get; set; }
        public object[] goodsList { get; set; }
        public string logisticCompanyID { get; set; }
        public string logisticID { get; set; }
        public string statusType { get; set; }
        public object[] tracesList { get; set; }
    }
}
