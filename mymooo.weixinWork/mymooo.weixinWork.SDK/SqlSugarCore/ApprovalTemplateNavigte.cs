using mymooo.core.Attributes.Redis;
using SqlSugar;

namespace mymooo.weixinWork.SDK.SqlSugarCore
{
	public partial class ApprovalTemplate
	{
		/// <summary>
		/// 审批模板字段信息
		/// </summary>
		[Navigate(NavigateType.OneToMany, nameof(TemplateId))]
		[RedisValue(isJson: true)]
		public List<ApprovalTemplateField>? ApprovalTemplateFields { get; set; }

		/// <summary>
		/// 审批流配置
		/// </summary>
		[Navigate(NavigateType.OneToMany, nameof(TemplateId))]
		public List<AuditFlowConfig>? AuditFlowConfigs { get; set; }
	}
}
