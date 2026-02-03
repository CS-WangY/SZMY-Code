using mymooo.weixinWork.SDK.Approval.Model.Enum;
using SqlSugar;
using SqlSugar.DbConvert;
using System.Text.Json.Serialization;
namespace mymooo.weixinWork.SDK.SqlSugarCore
{
	///<summary>
	///
	///</summary>
	public partial class AuditFlowConfigDetail
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
        public long AuditFlowConfigId { get; set; }

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:False
		/// </summary>  
		[JsonConverter(typeof(JsonStringEnumConverter))]
		[SugarColumn(SqlParameterDbType = typeof(EnumToStringConvert))]
		public AuditFlowDetailType Type { get; set; }

		/// <summary>
		/// Desc:
		/// Default:
		/// Nullable:True
		/// </summary>      
		[JsonConverter(typeof(JsonStringEnumConverter))]
		[SugarColumn(SqlParameterDbType = typeof(EnumToStringConvert))]
		public ApproverNodeModel SPType { get; set; } 

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string UserCode { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CreateUserCode { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? CreateDateTime { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ConditionName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Formal { get; set; } = string.Empty;

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int ParentId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int SonId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public int Seq { get; set; }

    }
}