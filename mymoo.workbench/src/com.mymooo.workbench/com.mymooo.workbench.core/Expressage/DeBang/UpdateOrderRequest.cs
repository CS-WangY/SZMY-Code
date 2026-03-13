using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.DeBang
{
    public class UpdateOrderRequest
    {
        public UpdateOrderRequest()
        {
            this.logisticCompanyID = "DEPPON";
            this.transportType = "RCP";
            this.gmtCommit = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        public string backSignBill { get; set; }
        public string businessNetworkNo { get; set; }
        public string cargoName { get; set; }
        public string codType { get; set; }
        public int codValue { get; set; }
        public string customerID { get; set; }
        public string deliveryType { get; set; }
        public int insuranceValue { get; set; }
        public string logisticCompanyID { get; set; }
        public string orderSource { get; set; }
        public string logisticID { get; set; }
        public string serviceType { get; set; }
        public string payType { get; set; }
        public string gmtCommit { get; set; }
        public Receiver receiver { get; set; }
        public Sender sender { get; set; }
        public bool smsNotify { get; set; }
        public string toNetworkNo { get; set; }
        public int totalNumber { get; set; }
        public int totalVolume { get; set; }
        public int totalWeight { get; set; }
        public string transportType { get; set; }
        public string vistReceive { get; set; }

    public class Receiver
    {
        public string address { get; set; }
        public string city { get; set; }
        public string county { get; set; }
        public string mobile { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string province { get; set; }
    }

    public class Sender
    {
        public string address { get; set; }
        public string city { get; set; }
        public string county { get; set; }
        public string mobile { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string province { get; set; }
    }
    }
}
