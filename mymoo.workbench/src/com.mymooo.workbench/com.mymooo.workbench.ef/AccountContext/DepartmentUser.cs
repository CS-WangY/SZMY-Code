using System;
using System.Collections.Generic;

#nullable disable

namespace com.mymooo.workbench.ef.AccountContext
{
    /// <summary>
    /// 部门和用户关系表
    /// </summary>
    public partial class DepartmentUser
    {
        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 部门Id
        /// </summary>
        public long DepartmentId { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 是否为上级
        /// </summary>
        public int IsLeaderInDepartment { get; set; }

        /// <summary>
        /// 部门详情
        /// </summary>
        public virtual Department Department { get; set; }

        /// <summary>
        /// 用户详情
        /// </summary>
        public virtual User User { get; set; }
    }
}
