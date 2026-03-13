using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.DeBang
{
    public class CreateOrderRequest
    {
        public CreateOrderRequest()
        {
            this.orderType = "2";
            this.transportType = "RCP";
            this.gmtCommit = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.packageInfo = new Packageinfo()
            {
                cargoName = "五金",
                totalNumber = 1,
                totalWeight = 1,
                deliveryType = "4"
            };
            this.payType = "2";
            this.addServices = new AddServices()
            {
                backSignBill = "1"
            };
            this.isOut = "Y";
        }

        public string companyCode { get; set; }
        public string custOrderNo { get; set; }
        public string customerCode { get; set; }
        public string logisticID { get; set; }
        public int needTraceInfo { get; set; }
        public string orderType { get; set; }
        public Packageinfo packageInfo { get; set; }
        public Receiver receiver { get; set; }
        public Sender sender { get; set; }
        public string transportType { get; set; }
        public string gmtCommit { get; set; }
        public string payType { get; set; }
        public string isOut { get; set; }
        public AddServices addServices { get; set; }
        public class AddServices
        {
            public string backSignBill { get; set; }
        }

        public class Packageinfo
        {
            public string cargoName { get; set; }
            public string deliveryType { get; set; }
            public int totalNumber { get; set; }
            public float totalVolume { get; set; }
            public int totalWeight { get; set; }
            public string packageService { get; set; }
        }

        public class Receiver
        {
            public string address { get; set; }
            public string city { get; set; }
            public string companyName { get; set; }
            public string county { get; set; }
            public string mobile { get; set; }
            public string name { get; set; }
            public string province { get; set; }
            public string town { get; set; }
        }

        public class Sender
        {
            public string address { get; set; }
            public string city { get; set; }
            public string companyName { get; set; }
            public string county { get; set; }
            public string mobile { get; set; }
            public string name { get; set; }
            public string province { get; set; }
            public string town { get; set; }
        }
    }
}
