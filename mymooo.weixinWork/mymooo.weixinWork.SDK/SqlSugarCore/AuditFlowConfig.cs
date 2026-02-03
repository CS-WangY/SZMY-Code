using mymooo.core.Attributes.Redis;
using mymooo.weixinWork.SDK.Approval.Model.Enum;
using SqlSugar;
namespace mymooo.weixinWork.SDK.SqlSugarCore
{
    ///<summary>
    ///
    ///</summary>
	[RedisKey("mymooo-Approval-Template", 14)]
    public partial class AuditFlowConfig
	{
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [RedisMainField]
        public string EnvCode { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [RedisPrimaryKey]
        public string TemplateId { get; set; } = string.Empty;

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>           
		public TemplateApproverModel ApprovalMode { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public NotifyType NotifyType { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CreateUserCode { get; set; } = string.Empty;

    }
}