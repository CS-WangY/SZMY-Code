using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kingdee.Mymooo.Core.SalesManagement
{
    /// <summary>
    /// 售后申请请求实体
    /// </summary>
    public class AfterSalesRequest
    {
        /// <summary>
        /// EVE采购商城平台的订单编号
        /// </summary>
        [Required(ErrorMessage = "订单ID不能为空")]
        public string OrderId { get; set; }

        /// <summary>
        /// 第三方接入系统的订单编号
        /// </summary>
        [Required(ErrorMessage = "第三方订单ID不能为空")]
        public string ThirdOrderId { get; set; }

        /// <summary>
        /// 售后类型：1=退货；2=换货
        /// </summary>
        [Required(ErrorMessage = "售后类型不能为空")]
        [Range(1, 2, ErrorMessage = "售后类型值无效")]
        public int AfsType { get; set; }

        /// <summary>
        /// 商品返回方式：1=第三方接入系统上门取件；2=用户物流快递寄回
        /// </summary>
        [Required(ErrorMessage = "返回方式不能为空")]
        [Range(1, 2, ErrorMessage = "返回方式值无效")]
        public int ReturnType { get; set; }

        /// <summary>
        /// 取件时间（当ReturnType为1时必需）
        /// </summary>
        public string PickTime { get; set; }

        /// <summary>
        /// 售后原因
        /// </summary>
        [Required(ErrorMessage = "售后原因不能为空")]
        public string Reason { get; set; }

        /// <summary>
        /// 售后申请日期（格式示例：yyyy-MM-dd HH:mm:ss）
        /// </summary>
        [Required(ErrorMessage = "申请日期不能为空")]
        public string ApplyTime { get; set; }

        /// <summary>
        /// 售后申请人姓名
        /// </summary>
        [Required(ErrorMessage = "申请人姓名不能为空")]
        public string CreatorName { get; set; }

        /// <summary>
        /// 售后申请人手机号
        /// </summary>
        public string CreatorMobile { get; set; }

        /// <summary>
        /// 售后商品列表
        /// </summary>
        [Required(ErrorMessage = "商品列表不能为空")]
        [MinLength(1, ErrorMessage = "至少需要一件商品")]
        public List<AfterSalesSku> Skus { get; set; }

        /// <summary>
        /// 售后商品明细
        /// </summary>
        public class AfterSalesSku
        {
            /// <summary>
            /// 订单行号
            /// </summary>
            [Required(ErrorMessage = "订单行号不能为空")]
            public string DetailId { get; set; }

            /// <summary>
            /// 申请售后商品的SKU编码
            /// </summary>
            [Required(ErrorMessage = "SKU编码不能为空")]
            public string SkuId { get; set; }

            /// <summary>
            /// 申请售后商品的数量（需大于0）
            /// </summary>
            [Required(ErrorMessage = "商品数量不能为空")]
            [Range(1, 99999, ErrorMessage = "商品数量无效")]
            public int SkuNum { get; set; }
        }
    }
}
