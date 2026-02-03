using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.Approval.Model
{
	/// <summary>
	/// 获取审批模板详情
	/// </summary>
	public class GetApprovalDetailRequest
	{
		/// <summary>
		/// 审批单号
		/// </summary>
		[JsonPropertyName("sp_no")]
		[Required(ErrorMessage = "审批单号必须录入!")]
		public string SpNo { get; set; } = string.Empty;
	}
}
