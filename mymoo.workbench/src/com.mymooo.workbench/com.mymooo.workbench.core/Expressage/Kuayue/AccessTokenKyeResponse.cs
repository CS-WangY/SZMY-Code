using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.Kuayue
{
    public class AccessTokenKyeResponse
    {
        /// <summary>
        /// 响应代码
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 响应消息
        /// </summary>
        public string Msg { get; set; }
        /// <summary>
        /// 响应数据
        /// </summary>
        public KyeAccessToken Data { get; set; }
        /// <summary>
        /// 响应状态
        /// </summary>
        public bool Success { get; set; }
    }
}
