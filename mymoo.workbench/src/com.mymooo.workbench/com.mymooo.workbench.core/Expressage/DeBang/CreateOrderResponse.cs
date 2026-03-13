using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.DeBang
{
    public class CreateOrderResponse
    {
        public string result { get; set; }
        public string reason { get; set; }
        public string mailNo { get; set; }
        public string logisticID { get; set; }
        public string arrivedOrgSimpleName { get; set; }
        public string resultCode { get; set; }
        public string uniquerRequestNumber { get; set; }
    }

}
