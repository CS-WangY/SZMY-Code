using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.ShunFeng
{
    public class ShunFengQueryFreightChargeResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ErrorCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ErrorMsg { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public MsgData MsgData { get; set; }
    }

    public class DeliverTmDtoItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string BbusinessType { get; set; }
        /// <summary>
        /// 顺丰特惠
        /// </summary>
        public string BusinessTypeDesc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DeliverTime { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Fee { get; set; }
        /// <summary>
        /// 是否查询价格（1,返回价格；0，不返回价格）；
        /// </summary>
        public string SearchPrice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CloseTime { get; set; }
    }

    public class MsgData
    {
        /// <summary>
        /// 
        /// </summary>
        public List<DeliverTmDtoItem> DeliverTmDto { get; set; }
    }
}
