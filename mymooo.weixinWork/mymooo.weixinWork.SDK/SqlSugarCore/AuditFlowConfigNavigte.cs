using SqlSugar;

namespace mymooo.weixinWork.SDK.SqlSugarCore
{
	public partial class AuditFlowConfig
	{
		/// <summary>
		/// 审批流配置
		/// </summary>
		[Navigate(NavigateType.OneToOne, nameof(TemplateId))]
		public ApprovalTemplate? ApprovalTemplate { get; set; }

		/// <summary>
		/// 审批流明细
		/// </summary>
		[Navigate(NavigateType.OneToMany, nameof(AuditFlowConfigDetail.AuditFlowConfigId))]
		public List<AuditFlowConfigDetail>? AuditFlowConfigDetails { get; set; }
	}
}
