using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BaseManagement
{
    public class InvoicingRequest
    {
        /// <summary>
        /// 发票类型
        /// </summary>
        public int? InvoiceType { get; set; }
        /// <summary>
        /// 开票通讯地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 发票抬头
        /// </summary>
        public string InvoiceName { get; set; }

        /// <summary>
        /// 发票寄送地址
        /// </summary>
        public string ReceiveAddres { get; set; }

        public string CompanyCode { get; set; }

        /// <summary>
        /// 纳税人识别码
        /// </summary>
        public string TaxpayerCode { get; set; }

        /// <summary>
        /// 银行账号
        /// </summary>
        public string BankAccount { get; set; }

        /// <summary>
        /// 银行
        /// </summary>
        public string Bank { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public string Telephone { get; set; }

        /// <summary>
        /// 发票接收人
        /// </summary>
        public string Receiver { get; set; }
    }
}
