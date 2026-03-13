using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.ShunFeng
{
    public class ShunFengQueryFreightChargeRequest
    {
        /// <summary>
        /// 快件产品：可以为空，为空时查询默认时效对应的产品列表。不为空时以数字代码业务类型，例如：1：表示顺丰特快 2：表示顺丰特惠 5：表示顺丰次晨 6：表示即日件
        /// </summary>
        public string BusinessType { get; set; }
        /// <summary>
        /// 寄件时间，格式为YYYY-MM-DD HH24:MM:SS，示例2013-12-27 17:54:20。
        /// </summary>
        public string ConsignedTime { get; set; }
        /// <summary>
        /// 目的地信息
        /// </summary>
        public DestAddress DestAddress { get; set; }
        /// <summary>
        /// 1：表示查询含价格的接口0：表示查询不含价格的接口 备注：限制只能为0,1或者不传searchPrice，不可以为空,null
        /// </summary>
        public string SearchPrice { get; set; }
        /// <summary>
        /// 原寄地信息
        /// </summary>
        public SrcAddress SrcAddress { get; set; }
        /// <summary>
        /// 重量
        /// </summary>
        public decimal Weight { get; set; }
        /// <summary>
        /// 体积
        /// </summary>
        public decimal Volume { get; set; }
    }

    public class DestAddress
    {
        /// <summary>
        /// 详细地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 区
        /// </summary>
        public string District { get; set; }
        /// <summary>
        /// 省
        /// </summary>
        public string Province { get; set; }
    }

    public class SrcAddress
    {
        /// <summary>
        /// 详细地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 区
        /// </summary>
        public string District { get; set; }
        /// <summary>
        /// 省
        /// </summary>
        public string Province { get; set; }
    }
}
