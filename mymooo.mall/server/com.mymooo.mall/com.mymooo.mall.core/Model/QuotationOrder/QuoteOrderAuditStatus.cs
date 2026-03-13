using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.QuotationOrder
{

    public enum QuoteOrderAuditStatus
    {
        /// <summary>
        /// 草稿。
        /// </summary>
        IsDraft = 0,
        /// <summary>
        /// 提交，待审核。
        /// </summary>
        Submitted = 1,
        /// <summary>
        /// 已审，通过。
        /// </summary>
        Passed = 2,
        /// <summary>
        /// 取消审核，销售的提交操作取消了供应链对指定项的报价。
        /// </summary>
        Cancel = 3,
        /// <summary>
        /// 被驳回
        /// </summary>
        Rejected = 4
    }
}
