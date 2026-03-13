using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.ShunFeng
{
    public class CreateOrderRequest
    {
        public CreateOrderRequest ()
        {
            this.payMethod = 1;
            this.expressTypeId = 2;
            this.language = "zh-CN";
            this.monthlyCard = "7690222178";
        }
        public string language { get; set; }
        public string orderId { get; set; }
        public Customsinfo customsInfo { get; set; }
        public Cargodetail[] cargoDetails { get; set; }
        public string cargoDesc { get; set; }
        public Extrainfolist[] extraInfoList { get; set; }
        public Servicelist[] serviceList { get; set; }
        public Contactinfolist[] contactInfoList { get; set; }
        /// <summary>
        /// 月结卡号
        /// </summary>
        public string monthlyCard { get; set; }

        /// <summary>
        /// 付款方式，支持以下值： 1:寄方付 2:收方付 3:第三方付
        /// </summary>
        public int payMethod { get; set; }

        /// <summary>
        /// https://open.sf-express.com/developSupport/734349?activeIndex=324604
        /// </summary>
        public int expressTypeId { get; set; }
        public int parcelQty { get; set; }
        public decimal totalLength { get; set; }
        public decimal totalWidth { get; set; }
        public decimal totalHeight { get; set; }
        public decimal volume { get; set; }
        public decimal totalWeight { get; set; }
        public string totalNetWeight { get; set; }
        public string sendStartTm { get; set; }
        public int isDocall { get; set; }
        public int isSignBack { get; set; }
        public string remark { get; set; }

        public class Customsinfo
    {
        public decimal declaredValue { get; set; }
    }

    public class Cargodetail
    {
        public decimal amount { get; set; }
        public decimal count { get; set; }
        public string currency { get; set; }
        public string goodPrepardNo { get; set; }
        public string hsCode { get; set; }
        public string name { get; set; }
        public string productRecordNo { get; set; }
        public string sourceArea { get; set; }
        public string taxNo { get; set; }
        public string unit { get; set; }
        public decimal weight { get; set; }
    }

    public class Extrainfolist
    {
        public string attrName { get; set; }
        public string attrVal { get; set; }
    }

    public class Servicelist
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class Contactinfolist
    {
        public string address { get; set; }
        public string city { get; set; }
        public string contact { get; set; }
        public int contactType { get; set; }
        public string country { get; set; }
        public string county { get; set; }
        public string mobile { get; set; }
        public string postCode { get; set; }
        public string province { get; set; }
        public string tel { get; set; }
        public string company { get; set; }
    }
    }
}
