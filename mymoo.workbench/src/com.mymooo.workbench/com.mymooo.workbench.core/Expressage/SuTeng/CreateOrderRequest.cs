using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.SuTeng
{
    public class CreateOrderRequest
    {
        public CreateOrderRequest()
        {
            this.operateEmpCode = "769097887";
            this.operateEmpName = "贺百熊";
            this.operateSiteCode = "769097";
            this.operateSiteName = "东莞大岭山";
            this.backbillFlag = true;
            this.payType = 1;
        }

        public string orderCode { get; set; }
        public string billCode { get; set; }
        public string sendCompany { get; set; }
        public string sendMan { get; set; }
        public string sendPhone { get; set; }
        public string sendProvince { get; set; }
        public string sendCity { get; set; }
        public string sendCounty { get; set; }
        public string sendAddress { get; set; }
        public string recCompany { get; set; }
        public string recMan { get; set; }
        public string recPhone { get; set; }
        public string recProvince { get; set; }
        public string recCity { get; set; }
        public string recCounty { get; set; }
        public string recAddress { get; set; }
        public string goodsType { get; set; }
        public string goodsName { get; set; }
        public decimal weight { get; set; }
        public int? qty { get; set; }

        /// <summary>
        ///  付款方式: 0-到付，1-现金，2-月结
        /// </summary>
        public int payType { get; set; }
        public string customerCode { get; set; }
        public decimal? gp { get; set; }
        public bool backbillFlag { get; set; }
        public string remark { get; set; }
        public string operateEmpCode { get; set; }
        public string operateEmpName { get; set; }
        public string operateSiteCode { get; set; }
        public string operateSiteName { get; set; }
    }
}
