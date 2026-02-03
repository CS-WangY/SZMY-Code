using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ApigatewayConfiguration
{
    /// <summary>
    /// 配置
    /// </summary>
    public class ApigatewayConfig : ConfigurationElement
    {
        /// <summary>
        /// 数据中心Id
        /// </summary>
        [ConfigurationProperty("url", IsRequired = true)]
        public string Url
        {
            get
            {
                return (string)base["url"];
            }
            set
            {
                base["url"] = value;
            }
        }

        /// <summary>
        /// 应用Id
        /// </summary>
        [ConfigurationProperty("appId", IsRequired = true)]
        public string AppId
        {
            get
            {
                return (string)base["appId"];
            }
            set
            {
                base["appId"] = value;
            }
        }

        /// <summary>
        /// Toekn
        /// </summary>
        [ConfigurationProperty("token", IsRequired = true)]
        public string Toekn
        {
            get
            {
                return (string)base["token"];
            }
            set
            {
                base["token"] = value;
            }
        }

        /// <summary>
        /// 用户名
        /// </summary>
        [ConfigurationProperty("nonce", IsRequired = true)]
        public string Nonce
        {
            get
            {
                return (string)base["nonce"];
            }
            set
            {
                base["nonce"] = value;
            }
        }

        /// <summary>
        /// 环境变量
        /// </summary>
        [ConfigurationProperty("envCode", IsRequired = true)]
        public string EnvCode
        {
            get
            {
                return (string)base["envCode"];
            }
            set
            {
                base["envCode"] = value;
            }
        }

        /// <summary>
        /// 虚拟路径
        /// </summary>
        public string VirtualHost { get; } = "/erp";
    }
}
