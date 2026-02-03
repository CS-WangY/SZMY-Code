using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.K3CloudConfiguration
{
	/// <summary>
	/// 金蝶配置
	/// </summary>
	public class K3CloudConfig : ConfigurationElement
	{
		/// <summary>
		/// 数据中心Id
		/// </summary>
		[ConfigurationProperty("url")]
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
		/// 数据中心Id
		/// </summary>
		[ConfigurationProperty("dataCenterNumber", IsRequired = true)]
		public string DataCenterNumber
		{
			get
			{
				return (string)base["dataCenterNumber"];
			}
			set
			{
				base["dataCenterNumber"] = value;
			}
		}

		/// <summary>
		/// 用户名
		/// </summary>
		[ConfigurationProperty("username", IsRequired = true)]
		public string Username
		{
			get
			{
				return (string)base["username"];
			}
			set
			{
				base["username"] = value;
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
		/// 应用密钥
		/// </summary>
		[ConfigurationProperty("appSecret", IsRequired = true)]
		public string AppSecret
		{
			get
			{
				return (string)base["appSecret"];
			}
			set
			{
				base["appSecret"] = value;
			}
		}
	}
}
