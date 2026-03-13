using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.SystemManage
{
    /// <summary>
    /// 
    /// </summary>
    public class UserSync
    {

        /// <summary>
        /// 企业微信用户code
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 企业微信名称
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 职务信息；第三方仅通讯录应用可获取；对于非第三方创建的成员，第三方通讯录应用也不可获取
        /// </summary>
        public string Position { get; set; } = string.Empty;

        /// <summary>
        /// 邮箱，第三方仅通讯录应用可获取；对于非第三方创建的成员，第三方通讯录应用也不可获取
        /// </summary>
        public string Mobile { get; set; } = string.Empty;

        /// <summary>
        /// 性别。0表示未定义，1表示男性，2表示女性
        /// </summary>
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// 邮箱，第三方仅通讯录应用可获取；对于非第三方创建的成员，第三方通讯录应用也不可获取
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 座机。第三方仅通讯录应用可获取；对于非第三方创建的成员，第三方通讯录应用也不可获取
        /// </summary>
        public string Telephone { get; set; } = string.Empty;

        /// <summary>
        /// 别名；第三方仅通讯录应用可获取；对于非第三方创建的成员，第三方通讯录应用也不可获取
        /// </summary>
        public string Alias { get; set; } = string.Empty;

        /// <summary>
        /// 地址。第三方仅通讯录应用可获取；对于非第三方创建的成员，第三方通讯录应用也不可获取
        /// </summary>
        public string Address { get; set; } = string.Empty;


        ///// <summary>
        ///// 主部门
        ///// </summary>
        //public long MainDepartmentId { get; set; }

        /// <summary>
        /// 入职时间
        /// </summary>
        public DateTime? EntryDate { get; set; }



        /// <summary>
        /// 资料修改时间
        /// </summary>
        //public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public List<DeptSync> DeptList { get; set; } = [];

    }


    /// <summary>
    /// 
    /// </summary>
    public class DeptSync
    {

        /// <summary>
        /// 部门Id,企业微信的部门Id
        /// </summary>
        public long DeptId { get; set; }

        /// <summary>
        /// 企业微信部门父级Id
        /// </summary>
        public long ParentId { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string DeptName { get; set; } = string.Empty;

        /// <summary>
        /// 是否主部门
        /// </summary>
        public bool IsMain { get; set; }
    }


    /// <summary>
    /// 
    /// </summary>
    public class DeptInfoSync
    {

        /// <summary>
        /// 部门Id,企业微信的部门Id
        /// </summary>
        public long DeptId { get; set; }

        /// <summary>
        /// 企业微信部门父级Id
        /// </summary>
        public long ParentId { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string DeptName { get; set; } = string.Empty;


    }



    /// <summary>
    /// 
    /// </summary>
    public class DeptUserSync
    {

        /// <summary>
        /// 部门Id,企业微信的部门Id
        /// </summary>
        public long DeptId { get; set; }

        /// <summary>
        /// 企业微信部门父级Id
        /// </summary>
        public long ParentId { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string DeptName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public List<UserSync> UserList { get; set; } = [];

 
    }


    /// <summary>
    /// 同步用的
    /// </summary>
    public class  DeptAndUserSyncResponse()
    {
        /// <summary>
        /// 
        /// </summary>
        public  List<UserSync> UserList { get; set; } = [];

        /// <summary>
        /// 
        /// </summary>
        public  List<DeptInfoSync> AllDept { get; set; } = [];
    }


}
