namespace mymooo.weixinWork.SDK.Approval.Attributes
{
	/// <summary>
	/// 审批模板
	/// </summary>
	/// <param name="templateId">审批模板Id</param>
	[AttributeUsage(AttributeTargets.Class)]
	public class ApprovalTemplateAttribute(string templateId) : Attribute
	{
		/// <summary>
		/// 审批模板Id
		/// </summary>
		public string TemplateId { get; } = templateId;
	}
}
