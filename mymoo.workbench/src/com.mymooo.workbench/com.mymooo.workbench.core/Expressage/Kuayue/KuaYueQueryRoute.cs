using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.Kuayue
{
    public class KuaYueQueryRoute
    {
        /// <summary>
        /// 
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool success { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Data data { get; set; }
    }
    public class ExteriorRouteListItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 揽件完毕
        /// </summary>
        public string routeStep { get; set; }
        /// <summary>
        /// 快件已由硕放分拨揽件完毕！
        /// </summary>
        public string routeDescription { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string uploadDate { get; set; }
    }

    public class EsWaybillItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string waybillNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string productCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string receiveDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string receiver { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string expectedDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ExteriorRouteListItem> exteriorRouteList { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string mailingTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string serviceMode { get; set; }
        /// <summary>
        /// 闵行区浦江镇
        /// </summary>
        public string mailingAddress { get; set; }
        /// <summary>
        /// 宝安区福永街道
        /// </summary>
        public string receivingAddress { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string addressee { get; set; }
        /// <summary>
        /// 林建宝
        /// </summary>
        public string sender { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string totalFreightAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string freightWeight { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int size { get; set; }
    }

    public class Data
    {
        /// <summary>
        /// 
        /// </summary>
        public string resultCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<EsWaybillItem> esWaybill { get; set; }
    }
}
