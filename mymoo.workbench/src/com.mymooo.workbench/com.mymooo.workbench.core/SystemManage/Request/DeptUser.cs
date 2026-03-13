using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.SystemManage.Request
{
    
    public class WXUserRequest
    {
        /// <summary>
        /// 企业微信用户code
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 企业微信名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 邮箱，第三方仅通讯录应用可获取；对于非第三方创建的成员，第三方通讯录应用也不可获取
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 邮箱，第三方仅通讯录应用可获取；对于非第三方创建的成员，第三方通讯录应用也不可获取
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 座机。第三方仅通讯录应用可获取；对于非第三方创建的成员，第三方通讯录应用也不可获取
        /// </summary>
        public string Telephone { get; set; }

        /// <summary>
        /// 地址。第三方仅通讯录应用可获取；对于非第三方创建的成员，第三方通讯录应用也不可获取
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        public long Versions { get; set; }
     
    }
}
