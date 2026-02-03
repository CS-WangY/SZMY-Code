using mymooo.core.Attributes.Redis;
using SqlSugar;
namespace mymooo.weixinWork.SDK.SqlSugarCore
{
	///<summary>
	///审批模板
	///</summary>
	[RedisKey("mymooo-Approval-Template", 14, false)]
	public partial class ApprovalTemplate
	{
		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
		[SugarColumn(IsPrimaryKey = true)]
		[RedisPrimaryKey]
		public string TemplateId { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
		public string TemplateName { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
		public string CreateUser { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:True
		/// </summary>           
		public DateTime? CreateDate { get; set; }

	}
}