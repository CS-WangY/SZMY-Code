using mymooo.weixinWork.SDK.Approval.Model.Enum;
using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.Approval.Model
{
	/// <summary>
	/// 审批流用户配置
	/// </summary>
	public class ApproverUserConfig
	{
		/// <summary>
		/// Id
		/// </summary>
		public string? Id { get; set; } 

		/// <summary>
		/// 类型
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public ApproverSpecificPrincipal Iden { get; set; }

		/// <summary>
		/// 名称
		/// </summary>
		public string Name { get; set; } = string.Empty;
	}

}
