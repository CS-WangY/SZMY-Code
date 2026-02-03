using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.SalesManagement
{
    public class MessageHelpForCredit
    {
        /// <summary>
        ///     是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
        public string Code { get; set; }

        /// <summary>
        ///     返回消息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        ///返回的数据
        /// </summary>
        public CreditData Data { get; set; }
        public string Message { get; set; }
    }

    public class CreditData
    {
        public bool IsPass { get; set; }

        /// <summary>
        /// 信用额度
        /// </summary>
        public decimal CreditLine { get; set; }
        /// <summary>
        /// 可用额度
        /// </summary>
        public decimal AvailableCredit { get; set; }

        /// <summary>
        /// 占用额度
        /// </summary>
        public decimal OccupyCredit { get; set; }

        /// <summary>
        /// 逾期天数
        /// </summary>
        public int ExpiryDay { get; set; }

        /// <summary>
        /// 逾期金额
        /// </summary>
        public decimal ExpiryAmount { get; set; }

        /// <summary>
        /// 企业微信的审核单号
        /// </summary>
        public string Sp_no { get; set; }


    }
}
