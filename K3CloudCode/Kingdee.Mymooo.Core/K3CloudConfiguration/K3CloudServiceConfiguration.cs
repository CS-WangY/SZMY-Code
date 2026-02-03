using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.K3CloudConfiguration
{
    public class K3CloudServiceConfiguration : ConfigurationSection
    {
        /// <summary>
        /// 金蝶配置
        /// </summary>
        [ConfigurationProperty("K3CloudConfig")]
        public K3CloudConfig K3CloudConfig
        {
            get
            {
                return (K3CloudConfig)base["K3CloudConfig"];
            }
            set
            {
                base["K3CloudConfig"] = value;
            }
        }
    }
}
