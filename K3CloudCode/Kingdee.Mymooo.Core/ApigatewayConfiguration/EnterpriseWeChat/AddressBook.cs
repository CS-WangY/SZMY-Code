using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ApigatewayConfiguration.EnterpriseWeChat
{
    public class ParentUserInfo
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int userId { get; set; }
        /// <summary>
        /// 用户编码
        /// </summary>
        public string userCode { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string userName { get; set; }
        /// <summary>
        /// 上级id
        /// </summary>
        public int parentUserId { get; set; }
        /// <summary>
        /// 上级编码
        /// </summary>
        public string parentUserCode { get; set; }
        /// <summary>
        /// 索引
        /// </summary>
        public int rowIndex { get; set; }
        /// <summary>
        /// 上级名称
        /// </summary>
        public string parentUserName { get; set; }
        /// <summary>
        /// 部门id
        /// </summary>
        public int departmentId { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string departmentName { get; set; }
        /// <summary>
        /// 上级部门id
        /// </summary>
        public int parentDepartmentId { get; set; }
    }
}
