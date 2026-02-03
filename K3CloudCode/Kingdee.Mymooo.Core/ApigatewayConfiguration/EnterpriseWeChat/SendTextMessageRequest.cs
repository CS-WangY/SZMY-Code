using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ApigatewayConfiguration.EnterpriseWeChat
{
	public class SendTextMessageRequest
	{
		/// <summary>
		/// 指定接收消息的成员，成员ID列表（多个接收者用‘|’分隔，最多支持1000个）。
		/// 特殊情况：指定为”@all”，则向该企业应用的全部成员发送
		/// </summary>
		public string touser { get; set; }

		/// <summary>
		/// 指定接收消息的部门，部门ID列表，多个接收者用‘|’分隔，最多支持100个。
		/// 当touser为”@all”时忽略本参数
		/// </summary>
		public string toparty { get; set; }

		/// <summary>
		/// 指定接收消息的标签，标签ID列表，多个接收者用‘|’分隔，最多支持100个。
		/// 当touser为”@all”时忽略本参数
		/// </summary>
		public string totag { get; set; }

		/// <summary>
		/// 消息内容，最长不超过2048个字节，超过将截断（支持id转译）
		/// </summary>
		public Text text { get; set; }

		/// <summary>
		/// 消息内容，最长不超过2048个字节，超过将截断（支持id转译）
		/// </summary>
		public class Text
		{
			/// <summary>
			/// 消息内容，最长不超过2048个字节，超过将截断（支持id转译）
			/// </summary>
			public string content { get; set; }
		}
	}
}
