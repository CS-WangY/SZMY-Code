using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.Approval.Model
{
	/// <summary>
	/// 摘要行信息，用于定义某一行摘要显示的内容
	/// </summary>
	public class SummaryInfo
	{
		/// <summary>
		/// 摘要行显示文字，用于记录列表和消息通知的显示，不要超过20个字符
		/// </summary>
		[JsonPropertyName("summary_info")]
		public List<ControlTitle> SummaryInfos { get; set; } = [];
	}
}
