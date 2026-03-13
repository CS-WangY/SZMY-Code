using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.Kuayue
{
    /// <summary>
    /// 跨越预估运费响应参数
    /// </summary>
    public class KuayueQueryFreightChargeResponse
    {
        /// <summary>
        /// 响应代码
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 响应消息
        /// </summary>
        public string Msg { get; set; }
        /// <summary>
        /// 响应状态
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 响应数据集
        /// </summary>
        public List<DataItem> Data { get; set; }
    }
    public class DataItem
    {
        /// <summary>
        /// 服务方式
        /// </summary>
        public string ServiceMode { get; set; }
        /// <summary>
        /// 运费
        /// </summary>
        public decimal TotalJF { get; set; }
        /// <summary>
        /// 运费说明，运费=0.0元，起重应为0.0Kg
        /// </summary>
        public string RetMsg { get; set; }
        /// <summary>
        /// 折前运费
        /// </summary>
        public decimal BeforeDiscountAmount { get; set; }
        /// <summary>
        /// 折后运费
        /// </summary>
        public decimal AfferDiscountAmount { get; set; }
        /// <summary>
        /// 客户调价提示
        /// </summary>
        public string CustomerAdjustPriceTips { get; set; }
        /// <summary>
        /// 节假日调价费
        /// </summary>
        public decimal OfferAdjustPrice { get; set; }
        /// <summary>
        /// 派送费
        /// </summary>
        public decimal DispatchAmount { get; set; }
    }
}
