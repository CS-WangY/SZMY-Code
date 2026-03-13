using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace com.mymooo.workbench.ef.ThirdpartyApplication
{
    /// <summary>
    /// 审批模板
    /// </summary>
    public partial class ApprovalTemplate
    {
        public ApprovalTemplate()
        {
            ApprovalTemplateFields = [];
        }
        /// <summary>
        /// Id
        /// </summary>
        [Key]
        public string TemplateId { get; set; }

        /// <summary>
        /// 配置Id
        /// </summary>
        [Required]
        public string AppId { get; set; }

        /// <summary>
        /// 模板名称
        /// </summary>
        [Required]
        public string TemplateName { get; set; }

        /// <summary>
        /// 模板执行分析的类
        /// </summary>
        [Required]
        public string MessageExecute { get; set; }

        /// <summary>
        /// 回调地址
        /// </summary>
        [MaxLength(500)]
        public string CallbackUrl { get; set; }

        /// <summary>
        /// 创建用户
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Required]
        public DateTime? CreateDate { get; set; }

        [ForeignKey("AppId")]
        public virtual ThirdpartyApplicationConfig ThirdpartyApplicationConfig { get; set; }
        public virtual ICollection<ApprovalTemplateField> ApprovalTemplateFields { get; set; }

    }
}
