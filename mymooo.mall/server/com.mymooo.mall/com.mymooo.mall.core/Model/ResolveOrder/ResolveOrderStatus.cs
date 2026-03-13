using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.ResolveOrder
{

    /// <summary>
    /// 分解状态，
    /// </summary>
    public enum ResolveOrderStatus
    {
        /// <summary>
        /// 未分解。
        /// </summary>
        Nothing = 0,
        /// <summary>
        /// 已分解。
        /// </summary>
        Did = 1,
        /// <summary>
        /// 取消分解。
        /// </summary>
        Cancel = 2,
        /// <summary>
        /// 完成询价或货期。
        /// </summary>
        Completed = 3,
        /// <summary>
        /// 不可分解。
        /// </summary>
        NotAllowed = 4
    }
}
