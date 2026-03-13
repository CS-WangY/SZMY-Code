using System;
using System.ComponentModel.DataAnnotations;

namespace com.mymooo.workbench.ef.AccountContext
{
    public class UserToken
    {
        /// <summary>
        /// 用户唯一id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        [Required]
        [MaxLength(36)]
        [MinLength(5)]
        [Key]
        public string Token { get; set; }

        /// <summary>
        /// 用户编码
        /// </summary>
        [MaxLength(30)]
        [Required]
        public string UserCode { get; set; }

        /// <summary>
        /// 登录时间
        /// </summary>
        [Required]
        public DateTime LoginDate { get; set; }

        /// <summary>
        /// 第三方应用Id
        /// </summary>
        [Required]
        [MaxLength(30)]
        [MinLength(5)]
        public string AppId { get; set; }

        /// <summary>
        /// 有效期(分钟)
        /// </summary>
        [Required]
        [Range(10, 10000)]
        public int Validity { get; set; }

        /// <summary>
        /// 失效日期
        /// </summary>
        public DateTime FailureDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Ip { get; set; }

        public string MymoooCompany { get; set; }
    }
}
