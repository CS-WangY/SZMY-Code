using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.ShunFeng
{
    public class SearchOrderResponse
    {
        public bool success { get; set; }
        public string errorCode { get; set; }
        public object errorMsg { get; set; }
        public Msgdata msgData { get; set; }

        public class Msgdata
        {
            public string orderId { get; set; }
            public object returnExtraInfoList { get; set; }
            public Waybillnoinfolist[] waybillNoInfoList { get; set; }
            public string origincode { get; set; }
            public string destcode { get; set; }
            public string filterResult { get; set; }
            public object remark { get; set; }
            public Routelabelinfo[] routeLabelInfo { get; set; }
            public object contactInfo { get; set; }
            public string clientCode { get; set; }
            public object serviceList { get; set; }
        }

        public class Waybillnoinfolist
        {
            public int waybillType { get; set; }
            public string waybillNo { get; set; }
        }

        public class Routelabelinfo
        {
            public string code { get; set; }
            public Routelabeldata routeLabelData { get; set; }
            public string message { get; set; }
        }

        public class Routelabeldata
        {
            public string waybillNo { get; set; }
            public string sourceTransferCode { get; set; }
            public string sourceCityCode { get; set; }
            public string sourceDeptCode { get; set; }
            public string sourceTeamCode { get; set; }
            public string destCityCode { get; set; }
            public string destDeptCode { get; set; }
            public string destDeptCodeMapping { get; set; }
            public string destTeamCode { get; set; }
            public string destTeamCodeMapping { get; set; }
            public string destTransferCode { get; set; }
            public string destRouteLabel { get; set; }
            public string proName { get; set; }
            public string cargoTypeCode { get; set; }
            public string limitTypeCode { get; set; }
            public string expressTypeCode { get; set; }
            public string codingMapping { get; set; }
            public string codingMappingOut { get; set; }
            public string xbFlag { get; set; }
            public string printFlag { get; set; }
            public string twoDimensionCode { get; set; }
            public string proCode { get; set; }
            public string printIcon { get; set; }
            public string abFlag { get; set; }
            public string destPortCode { get; set; }
            public string destCountry { get; set; }
            public string destPostCode { get; set; }
            public string goodsValueTotal { get; set; }
            public string currencySymbol { get; set; }
            public string cusBatch { get; set; }
            public string goodsNumber { get; set; }
            public string errMsg { get; set; }
            public string checkCode { get; set; }
            public string proIcon { get; set; }
            public string fileIcon { get; set; }
            public string fbaIcon { get; set; }
            public string icsmIcon { get; set; }
            public string destGisDeptCode { get; set; }
            public object newIcon { get; set; }
        }
    }
}
