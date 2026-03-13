using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.Kuayue
{
    public class PlanOrderResquest
    {
        public string customerCode { get; set; }
        public string platformFlag { get; set; }
        public Orderinfo[] orderInfos { get; set; }
        public string repeatCheck { get; set; }

        public class Orderinfo
        {
            public Orderinfo()
            {
                this.serviceMode = 20;
                this.payMode = 10;
                this.receiptFlag = 10;
                this.subscriptionService = "10";
            }
            public string subscriptionService { get; set; }
            public string waybillNumber { get; set; }
            public Prewaybilldelivery preWaybillDelivery { get; set; }
            public Prewaybillpickup preWaybillPickup { get; set; }
            public int serviceMode { get; set; }
            public int payMode { get; set; }
            public string paymentCustomer { get; set; }
            public string goodsType { get; set; }
            public int? count { get; set; }
            public decimal? actualWeight { get; set; }
            public int? woodenFrame { get; set; }
            public string orderId { get; set; }
            public string productCode { get; set; }
            public decimal? collectionAmount { get; set; }
            public decimal? insuranceValue { get; set; }
            public int receiptFlag { get; set; }
            public int? receiptCount { get; set; }
            public string waybillRemark { get; set; }
            public string specification { get; set; }
            public int? dismantling { get; set; }
            public string deliveryTime { get; set; }
            public string goodsTime { get; set; }
            public Prewaybillgoodssizelist[] preWaybillGoodsSizeList { get; set; }
            public Extrainfo[] extraInfo { get; set; }
            public Ordernosizerelation[] orderNoSizeRelations { get; set; }
        }

        public class Prewaybilldelivery
        {
            public string companyName { get; set; }
            public string person { get; set; }
            public string phone { get; set; }
            public string mobile { get; set; }
            public string provinceName { get; set; }
            public string cityName { get; set; }
            public string countyName { get; set; }
            public string address { get; set; }
        }

        public class Prewaybillpickup
        {
            public string companyName { get; set; }
            public string person { get; set; }
            public string phone { get; set; }
            public string mobile { get; set; }
            public string provinceName { get; set; }
            public string cityName { get; set; }
            public string countyName { get; set; }
            public string address { get; set; }
        }

        public class Prewaybillgoodssizelist
        {
            public string length { get; set; }
            public string width { get; set; }
            public string height { get; set; }
            public string count { get; set; }
        }

        public class Extrainfo
        {
        }

        public class Ordernosizerelation
        {
            public string orderNo { get; set; }
            public Ordernosize orderNoSize { get; set; }
        }

        public class Ordernosize
        {
            public int length { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }
    }
}
