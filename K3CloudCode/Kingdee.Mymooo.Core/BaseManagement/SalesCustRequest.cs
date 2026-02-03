using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BaseManagement
{
    public class SalesCustRequest
    {
        /// <summary>
        /// 客户对应的所有业务员
        /// </summary>
        public List<string> UserCode { get; set; }
        /// <summary>
        /// 客户编码
        /// </summary>
        public string CustCode { get; set; }

        /// <summary>
        /// 是否首次同步
        /// </summary>
        public bool IsFirstSync { get; set; }

        /// <summary>
        /// 是否解绑
        /// </summary>
        public bool IsUnBind { get; set; }


        /// <summary>
        /// 需要更改业务员的订单
        /// </summary>
        public List<string> OrderNumber { get; set; }

        /// <summary>
        /// 订单转移的业务员
        /// </summary>
        public string TransferUserCode { get; set; }
    }
 
}
