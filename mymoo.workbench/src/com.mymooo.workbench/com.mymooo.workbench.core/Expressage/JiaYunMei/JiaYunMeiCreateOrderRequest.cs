using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.JiaYunMei
{
    public class JiaYunMeiCreateOrderRequest
    {
        /// <summary>
        /// 合作客户Id
        /// </summary>
        public string CooperatorCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public JiaYunMeiCreateOrderData Data { get; set; }
        /// <summary>
        /// 认证参数
        /// </summary>
        public string Sign { get; set; }
    }

    public class JiaYunMeiCreateOrderData
    {
        /// <summary>
        /// 货件追溯号(新增请求参数 非必填，格式：,JYM001,JYM002,JYM003,)
        /// </summary>
        public string BackCode { get; set; }

        /// <summary>
        /// 客户订单号
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 运单号
        /// </summary>
        public string BillCode { get; set; }

        /// <summary>
        /// 寄件网点
        /// </summary>
        public string SendSite { get; set; }

        /// <summary>
        /// 寄件网点编号(平台、网点客户必填)
        /// </summary>
        public string SendSiteCode { get; set; }

        /// <summary>
        /// 客户类型 (P为平台，W为网点）（必填）
        /// </summary>
        public string CustomerType { get; set; }

        /// <summary>
        /// 客户编号（必填）
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// 客户名称（必填）
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 取件员编号（平台、网点客户必填）
        /// </summary>
        public string RegisterCode { get; set; }

        /// <summary>
        /// 取件员
        /// </summary>
        public string RegisterMan { get; set; }

        /// <summary>
        /// 订单状态（必填）
        /// </summary>
        public int OrderStatus { get; set; }

        /// <summary>
        /// 寄件地址
        /// </summary>
        public string SendAddress { get; set; }
        /// <summary>
        /// 寄件人
        /// </summary>
        public string SendMan { get; set; }
        /// <summary>
        /// 寄件人电话
        /// </summary>
        public string SendPhone { get; set; }

        /// <summary>
        /// 收件人地址
        /// </summary>
        public string AcceptAddress { get; set; }
        /// <summary>
        /// 收件人
        /// </summary>
        public string AcceptMan { get; set; }
        /// <summary>
        /// 收件人电话
        /// </summary>
        public string AcceptPhone { get; set; }

        /// <summary>
        /// 物品名称（必填）
        /// </summary>
        public string GoodName { get; set; }
        /// <summary>
        /// 物品类型（保价金额不为空时必填）
        /// </summary>
        public string GoodType { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 回单标示无回单传0，有回单传1（默认有回单）
        /// </summary>
        public int BlRbillStatus { get; set; }
        /// <summary>
        /// 计费重量
        /// </summary>
        public double FeeWeight { get; set; }
        /// <summary>
        /// 运费（支付方式为到付时 必填）
        /// </summary>
        public decimal Feight { get; set; }

        /// <summary>
        /// 代收货款，无代收货款，传0
        /// </summary>
        public decimal GoodsPayment { get; set; }
        /// <summary>
        /// 到付
        /// </summary>
        public string PaymentType { get; set; }
        /// <summary>
        /// 件数
        /// </summary>
        public int PieceNumber { get; set; }

        /// <summary>
        /// 到付款，支付方式为：到付，到付款必须传值，到付款与运费相同；
        /// </summary>
        public decimal Topayment { get; set; }
    }
}
