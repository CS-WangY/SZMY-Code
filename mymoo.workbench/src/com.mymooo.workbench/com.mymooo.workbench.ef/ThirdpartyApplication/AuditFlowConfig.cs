using System;
using System.Collections.Generic;

#nullable disable

namespace com.mymooo.workbench.ef.ThirdpartyApplication
{
    public partial class AuditFlowConfig
    {
        public AuditFlowConfig()
        {
            AuditFlowConfigDetails = new HashSet<AuditFlowConfigDetail>();
        }

        public long Id { get; set; }
        
        public string AppId { get; set; }
        
        /// <summary>
        /// 模板Id
        /// </summary>
        public string TemplateId { get; set; }
        /// <summary>
        /// 抄送方式 1-提单时抄送； 2-单据通过后抄送；3-提单和单据通过后抄送
        /// </summary>
        public int NotifyType { get; set; }

        /// <summary>
        /// 审核模式:0-通过接口指定审批人、抄送人,1-使用此模板在管理后台设置的审批流程，支持条件审批。
        /// </summary>
        public int ApprovalMode { get; set; }

        public DateTime? CreateTime { get; set; }
        public string CreateUserCode { get; set; }

        public virtual ICollection<AuditFlowConfigDetail> AuditFlowConfigDetails { get; set; }
    }
}
