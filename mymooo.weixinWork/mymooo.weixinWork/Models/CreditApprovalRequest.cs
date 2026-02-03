using mymooo.weixinWork.SDK.Approval.Attributes;
using mymooo.weixinWork.SDK.Approval.Model;
using mymooo.weixinWork.SDK.Media.Model;

namespace mymooo.weixinWork.Models
{
    /// <summary>
    /// 
    /// </summary>
    [ApprovalTemplate("3TkaHCQ5mvgfS9WWqdPqRRxrk3KekYSzKxTgFYYs")]
	public class CreditApprovalRequest : ApprovalRequest
	{
		/// <summary>
		/// 发货日期
		/// </summary>
		public DateTime Date { get; set; }

		/// <summary>
		/// 客户编码
		/// </summary>
		public string CustNumber { get; set; } = string.Empty;

		/// <summary>
		/// 客户名称
		/// </summary>
		public string CustName { get; set; } = string.Empty;

		/// <summary>
		/// 发起申请人
		/// </summary>
		public string ApplicantCode { get; set; } = string.Empty;

		/// <summary>
		/// 发起人名称
		/// </summary>
		public string ApplicantName { get; set; } = string.Empty;

		/// <summary>
		/// 结算方式
		/// </summary>
		public string SettleType { get; set; } = string.Empty;

		/// <summary>
		/// 申请信用额度
		/// </summary>
		public decimal ApprovalCredit { get; set; }

		/// <summary>
		/// 申请理由
		/// </summary>
		public string ApprovalReason { get; set; } = string.Empty;

		/// <summary>
		/// 文件
		/// </summary>
		public List<MediaInfo>? MediaInfos { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string SalesName { get; set; } = string.Empty;	

		/// <summary>
		/// 
		/// </summary>
		public long SalesId { get; set; }
	}
}
