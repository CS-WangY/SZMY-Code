using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.DeBang
{
    public class CancelOrderRequest
    {
        public CancelOrderRequest()
        {
            this.logisticCompanyID = "DEPPON";
            this.cancelTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        public string cancelTime { get; set; }
        public string logisticCompanyID { get; set; }
        public string logisticID { get; set; }
        public string mailNo { get; set; }
        public string remark { get; set; }
    }

}
