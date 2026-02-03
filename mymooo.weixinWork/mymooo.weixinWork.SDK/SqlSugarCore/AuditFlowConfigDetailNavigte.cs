using SqlSugar;

namespace mymooo.weixinWork.SDK.SqlSugarCore
{
	public partial class AuditFlowConfigDetail
	{
		/// <summary>
		/// 审批模板字段信息
		/// </summary>
		[Navigate(NavigateType.OneToMany, nameof(Id))]
		public AuditFlowConfig? AuditFlowConfig { get; set; }
	}
}
