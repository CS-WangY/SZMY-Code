using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ApigatewayConfiguration
{
	/// <summary>
	/// 网关配置
	/// </summary>
	public class ApigatewayServiceConfiguration : ConfigurationSection
	{
		/// <summary>
		/// 网关配置
		/// </summary>
		[ConfigurationProperty("ApigatewayConfig")]
		public ApigatewayConfig ApigatewayConfig
		{
			get
			{
				return (ApigatewayConfig)base["ApigatewayConfig"];
			}
			set
			{
				base["ApigatewayConfig"] = value;
			}
		}
	}
}
