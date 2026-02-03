using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.Approval.Model
{
	/// <summary>
	/// 发起审批响应结果
	/// </summary>
	public class ApplyeventResponse
    {
        /// <summary>
        /// 出错返回码，为0表示成功，非0表示调用失败
        /// </summary>
        public int Errcode { get; set; }

		/// <summary>
		/// 返回码提示语
		/// </summary>
		[JsonPropertyName("errmsg")]
		public string? ErrorMessage { get; set; }

		/// <summary>
		/// 审批单号
		/// </summary>
		[JsonPropertyName("sp_no")]
        public string SpNo { get; set; } = string.Empty;
    }
}
