using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.Kuayue
{
    public class KyeAccessToken
    {
        /// <summary>
        /// token
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 刷新TOKEN
        /// </summary>
        public string Refresh_token { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public long Expire_time { get; set; }

        public DateTime ExpiresTime { get;  set; }
    }
}
