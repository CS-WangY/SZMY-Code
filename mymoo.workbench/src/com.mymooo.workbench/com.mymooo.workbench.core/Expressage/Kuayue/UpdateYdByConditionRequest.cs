using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.Kuayue
{
    public class UpdateYdByConditionRequest
    {
        public string customerCode { get; set; }
        public string waybillNumber { get; set; }
        public int? count { get; set; }
        public int? actualWeight { get; set; }
        public decimal? insuranceValue { get; set; }
        public Goodssize[] goodsSize { get; set; }
        public string additionalService { get; set; }
        public int? type { get; set; }
        public string platformFlag { get; set; }
        public string volume { get; set; }
        public string goodsTime { get; set; }
        public string serviceMode { get; set; }
        public string receiptFlag { get; set; }
        public int? amountCollection { get; set; }
        public string woodenFrame { get; set; }
        public string goodsType { get; set; }
        public string productCode { get; set; }
        /// <summary>
        /// 回单提供方，10-收方提供，不传默认寄方提供
        /// </summary>
        public string ReceiptProvider { get; set; }

        /// <summary>
        /// 回单份数
        /// </summary>
        public int ReceiptCount { get; set; }
        public Prewaybilldelivery preWaybillDelivery { get; set; }
        public Prewaybillpickup preWaybillPickup { get; set; }

        public class Prewaybilldelivery
        {
            public string person { get; set; }
            public string phone { get; set; }
            public string mobile { get; set; }
            public string address { get; set; }
        }

        public class Prewaybillpickup
        {
            public string person { get; set; }
            public string phone { get; set; }
            public string mobile { get; set; }
            public string address { get; set; }
        }

        public class Goodssize
        {
            public decimal length { get; set; }
            public decimal width { get; set; }
            public decimal height { get; set; }
            public int count { get; set; }
            public string goodsCode { get; set; }
        }
    }
}
