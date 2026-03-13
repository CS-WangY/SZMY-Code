using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace com.mymooo.workbench.ef.ThirdpartyApplication
{
    /// <summary>
    /// 审批应用消息
    /// </summary>
    public partial class ApprovalMessage
    {
        /// <summary>
        /// Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  //设置自增
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 消息Id
        /// </summary>
        [Required]
        public long MessageId { get; set; }

        /// <summary>
        /// 审批单号
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string SpNo { get; set; }

        /// <summary>
        /// 审批模板名称
        /// </summary>
        [Required]
        public string SpName { get; set; }

        /// <summary>
        /// 关键字1
        /// </summary>
        [MaxLength(500)]
        public string Keyword1 { get; set; }

        /// <summary>
        /// 关键字2
        /// </summary>
        [MaxLength(500)]
        public string Keyword2 { get; set; }

        /// <summary>
        /// 关键字3
        /// </summary>
        [MaxLength(500)]
        public string Keyword3 { get; set; }

        /// <summary>
        /// 审批状态 申请单状态：1-审批中；2-已通过；3-已驳回；4-已撤销；6-通过后撤销；7-已删除；10-已支付
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string SpStatus { get; set; }

        /// <summary>
        /// 申请人
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string ApplyUser { get; set; }

        /// <summary>
        /// 申请时间
        /// </summary>
        [Required]
        public DateTime? ApplyTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string MessageDetail { get; set; }

        /// <summary>
        /// 模板Id
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string TemplateId { get; set; }

        /// <summary>
        /// 审核人
        /// </summary>
        [MaxLength(30)]
        public string ApprovalUser { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? ApprovalTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateDate { get; set; }

        public virtual WeiXinMessage Message { get; set; }
    }
}
