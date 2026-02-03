using SqlSugar;
using System.Reflection;
using System.Text.Json.Serialization;

namespace mymooo.weixinWork.SDK.SqlSugarCore
{
	public partial class ApprovalTemplateField
	{
		/// <summary>
		/// 审批流配置
		/// </summary>
		[Navigate(NavigateType.OneToOne, nameof(TemplateId))]
		public ApprovalTemplate? ApprovalTemplate { get; set; }

		/// <summary>
		/// 属性
		/// </summary>
		[SugarColumn(IsIgnore = true)]
		[JsonIgnore]
		public PropertyInfo? PropertyInfo { get; internal set; }
	}
}
