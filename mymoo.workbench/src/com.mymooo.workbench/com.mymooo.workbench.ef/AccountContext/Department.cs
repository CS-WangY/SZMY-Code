using com.mymooo.workbench.ef.ThirdpartyApplication;
using System;
using System.Collections.Generic;

#nullable disable

namespace com.mymooo.workbench.ef.AccountContext
{
    public partial class Department
    {
        public Department()
        {
            DepartmentUsers = new HashSet<DepartmentUser>();
        }

        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// appid
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 部门Id,企业微信的部门Id
        /// </summary>
        public long DepartmentId { get; set; }

        /// <summary>
        /// 企业微信部门父级Id
        /// </summary>
        public long ParentId { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 部门英文名称
        /// </summary>
        public string NameEn { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        public long Versions { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { get; set; }

        /// <summary>
        /// 职能属性
        /// </summary>
        public string FunctionAttr { get; set; }

        public virtual ThirdpartyApplicationConfig App { get; set; }
        public virtual ICollection<DepartmentUser> DepartmentUsers { get; set; }
    }
}
