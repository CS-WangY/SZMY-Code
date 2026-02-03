using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    public class UserSyncToMesEntity
    {
        /// <summary>
        /// 登录账号
        /// </summary>
        public string username { get; set; } = "";

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string realname { get; set; } = "";

        /// <summary>
        /// 工号
        /// </summary>
        public string workNo { get; set; } = "";

        /// <summary>
        /// 生日
        /// </summary>
        public string birthday { get; set; } = "";

        /// <summary>
        /// 头像URL(全路径)
        /// </summary>
        public string avatar { get; set; } = "";

        /// <summary>
        /// 性别：1、男；2、女（初始值1）
        /// </summary>
        public int sex { get; set; } = 1;

        /// <summary>
        /// 邮箱地址
        /// </summary>
        public string email { get; set; } = "";

        /// <summary>
        /// 电话
        /// </summary>
        public string phone { get; set; } = "";

        /// <summary>
        /// 部门编号
        /// </summary>
        public string orgCode { get; set; } = "";

        /// <summary>
        /// 职务编号
        /// </summary>
        public string post { get; set; } = "";
    }
}
