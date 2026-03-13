using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace com.mymooo.workbench.ef.ThirdpartyApplication
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ThirdpartyApplicationDetail
    {
        public ThirdpartyApplicationDetail()
        {
            this.WeiXinMessage = [];
        }

        /// <summary>
        /// 第三方应用Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  //设置自增
        [Key]
        [SqlSugar.SugarColumn(IsPrimaryKey = true)]
        public long Id { get; set; }

        /// <summary>
        /// 第三方应用id
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string AppId { get; set; }

        /// <summary>
        /// 第三方应用编码
        /// </summary>
        [Required]
        [MaxLength(30)]
        [MinLength(2)]
        public string DetailCode { get; set; }

        /// <summary>
        /// 第三方应用名称
        /// </summary>
        [Required]
        [MaxLength(30)]
        [MinLength(2)]
        public string DetailName { get; set; }

        /// <summary>
        /// 企业Id
        /// </summary>
        [MaxLength(30)]
        public string Agentid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string AppSecret { get; set; }

        /// <summary>
        /// 第三方回调地址
        /// </summary>
        [MaxLength(500)]
        public string RedirectUri { get; set; }

        /// <summary>
        /// 第三方应用地址
        /// </summary>
        [MaxLength(500)]
        public string Token { get; set; }

        /// <summary>
        /// 第三方应用地址
        /// </summary>
        [MaxLength(500)]
        public string EncodingAESKey { get; set; }

        /// <summary>
        /// 第三方应用配置信息
        /// </summary>
        [ForeignKey("AppId")]
        [SqlSugar.SugarColumn(IsIgnore = true)]
        public ThirdpartyApplicationConfig ThirdpartyApplicationConfig { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Required]
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        [Required]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// token失效时间
        /// </summary>
        //[NotMapped]
        public DateTime? ExpiresTime { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        //[NotMapped]
        public string AccessToken { get; set; }

        [SqlSugar.SugarColumn(IsIgnore = true)]
        public virtual ICollection<WeiXinMessage> WeiXinMessage { get; set; }

        /// <summary>
        /// 接收消息处理器
        /// </summary>
        public string MessageExecute { get; set; }
    }
}
