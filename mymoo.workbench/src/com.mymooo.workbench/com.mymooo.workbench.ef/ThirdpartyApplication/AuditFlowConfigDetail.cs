using mymooo.weixinWork.SDK.Approval.Model.Enum;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

#nullable disable

namespace com.mymooo.workbench.ef.ThirdpartyApplication
{
	public partial class AuditFlowConfigDetail
    {
        public long Id { get; set; }

        /// <summary>
        /// 外键
        /// </summary>
        public long AuditFlowConfigId { get; set; }

        /// <summary>
        /// 类型：0---审批人，1---抄送人,2---创建人，3---条件;
        /// </summary>
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AuditFlowDetailType Type { get; set; }

        /// <summary>
        /// 审批方式：0-或签，1-会签；
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ApproverNodeModel Sptype { get; set; }

        public string UserCode { get; set; }
        public string CreateUserCode { get; set; }
        public DateTime? CreateDateTime { get; set; }
        public string ConditionName { get; set; }
        public string Formal { get; set; }

        public int ParentId { get; set; }

        public int SonId { get; set; }

        public int Seq { get; set; }

        public virtual AuditFlowConfig AuditFlowConfig { get; set; }
    }
}
