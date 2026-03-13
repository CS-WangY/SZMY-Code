using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.ShunFeng
{
    public class CreateOrderResponse
    {
        public bool success { get; set; }
        public string errorCode { get; set; }
        public string errorMsg { get; set; }
        public Msgdata msgData { get; set; }

        public class Msgdata
        {
            public string orderId { get; set; }
            public string originCode { get; set; }
            public string destCode { get; set; }
            public int filterResult { get; set; }
            public string remark { get; set; }
            public string url { get; set; }
            public string paymentLink { get; set; }
            public object isUpstairs { get; set; }
            public object isSpecialWarehouseService { get; set; }
            public object mappingMark { get; set; }
            public object agentMailno { get; set; }
            public object returnExtraInfoList { get; set; }
            public Waybillnoinfolist[] waybillNoInfoList { get; set; }
            public Routelabelinfo[] routeLabelInfo { get; set; }
            public object contactInfoList { get; set; }
            public object sendStartTm { get; set; }
            public object customerRights { get; set; }
            public object expressTypeId { get; set; }
        }

        public class Waybillnoinfolist
        {
            public int waybillType { get; set; }
            public string waybillNo { get; set; }
            public string boxNo { get; set; }
            public decimal? length { get; set; }
            public decimal? width { get; set; }
            public decimal? height { get; set; }
            public decimal? weight { get; set; }
            public decimal? volume { get; set; }
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
            public object sendAreaCode { get; set; }
            public object destinationStationCode { get; set; }
            public object sxLabelDestCode { get; set; }
            public object sxDestTransferCode { get; set; }
            public object sxCompany { get; set; }
            public object newAbFlag { get; set; }
            public string destAddrKeyWord { get; set; }
            public object rongType { get; set; }
            public object waybillIconList { get; set; }
        }
    }
}
