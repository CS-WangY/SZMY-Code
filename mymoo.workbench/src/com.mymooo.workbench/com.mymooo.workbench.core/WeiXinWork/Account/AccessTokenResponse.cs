using System;
using System.Collections.Generic;
using System.Text;

namespace com.mymooo.workbench.core.WeiXinWork.Account
{
    /// <summary>
    /// 获取token响应
    /// </summary>
    public class AccessTokenResponse
    {
        /// <summary>
        /// 出错返回码，为0表示成功，非0表示调用失败
        /// </summary>
        public int errcode { get; set; }

        /// <summary>
        /// 返回码提示语
        /// </summary>
        public string errmsg { get; set; }

        /// <summary>
        /// 获取到的凭证，最长为512字节
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// 凭证的有效时间（秒）
        /// </summary>
        public int expires_in { get; set; }
    }
}
