using System;
using System.Collections.Generic;
using System.Text;

namespace com.mymooo.workbench.core.SystemManage.Filter
{
    public class DepartmentUserFilter
    {
        /// <summary>
        /// 用户编码
        /// </summary>
        public string UserCode { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public long UserId{ get; set; }

        /// <summary>
        /// 用户姓名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 部门id
        /// </summary>
        public List<long> DepartmentIdList { get; set; }

        /// <summary>
        /// 是否递归子部门
        /// </summary>
        public int FetchChild { get; set; }


        /// <summary>
        /// 所属系统
        /// </summary>
        public List<string> MymoooCompany { get; set; }

        /// <summary>
        /// 是否是助理
        /// </summary>
        public bool? IsAssistant { get; set; }

        /// <summary>
        /// 是否是采购员
        /// </summary>
        public bool? IsPurchaser { get; set; }

        /// <summary>
        /// 是否是产品经理
        /// </summary>
        public bool? IsManager { get; set; }

        /// <summary>
        /// 岗位
        /// </summary>
        public long Post { get; set; }

        /// <summary>
        /// 职位
        /// </summary>
        public string Position { get; set; }
    }
}
