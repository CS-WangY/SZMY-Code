using mymooo.weixinWork.SDK.Approval.Model.Enum;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

#nullable disable

namespace com.mymooo.workbench.ef.ThirdpartyApplication
{
    /// <summary>
    /// 审批模板对应的字段
    /// </summary>
    public partial class ApprovalTemplateField
    {
        /// <summary>
        /// Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  //设置自增
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 模板Id
        /// </summary>
        [Required]
        public string TemplateId { get; set; }

        /// <summary>
        /// 字段code
        /// </summary>
        [Required]
        public string FieldNumber { get; set; }

        /// <summary>
        /// 字段名称
        /// </summary>
        [Required]
        public string FieldName { get; set; }

        /// <summary>
        /// 企业微信模板的字段Id
        /// </summary>
        [Required]
        public string FieldId { get; set; }

        /// <summary>
        /// 字段类型
        /// </summary>
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ApproverFieldType FieldType { get; set; }

        /// <summary>
        /// 是否关键字
        /// </summary>
        ///[Required]
        public int KeywordSeq { get; set; }

        /// <summary>
        /// 创建用户
        /// </summary>
        ///[Required]
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        ///[Required]
        public DateTime? CreateDate { get; set; }

        public string SelectOptionJson { get; set; }

        /// <summary>
        /// 审批序号
        /// </summary>
        public int? ApprovalSeq { get; set; }

        public virtual ApprovalTemplate Template { get; set; }
    }
}
