using com.mymooo.workbench.ef.AccountContext;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace com.mymooo.workbench.ef.ThirdpartyApplication
{
	/// <summary>
	/// 第三方应用配置信息
	/// </summary>
	public partial class ThirdpartyApplicationConfig
    {
        public ThirdpartyApplicationConfig()
        {
            ThirdpartyApplicationDetail = [];
            Menu = [];
            Departments = [];
            MymoooUsers = [];
        }

        /// <summary>
        /// 第三方应用Id
        /// </summary>
        [Required]
        [MaxLength(30)]
        [MinLength(5)]
        [Key]
        public string AppId { get; set; }

        /// <summary>
        /// 第三方应用名称
        /// </summary>
        [Required]
        [MaxLength(30)]
        [MinLength(2)]
        public string AppName { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Token { get; set; }

        /// <summary>
        /// Nonce
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Nonce { get; set; }

        /// <summary>
        /// 第三方应用地址
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Url { get; set; }

		/// <summary>
		/// 第三方应用金蝶数据中心
		/// </summary>
		[MaxLength(100)]
		public string DataCenterNumber { get; set; }

		/// <summary>
		/// 第三方应用 账号
		/// </summary>
		[MaxLength(100)]
		public string Username { get; set; }

		/// <summary>
		/// 回调加密key
		/// </summary>
		public string EncodingAESKey { get; set; }

        /// <summary>
        /// 单点的登录地址
        /// </summary>
        [MaxLength(100)]
        public string SignLoginUrl { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        public int Validity { get; set; }

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

        public bool IsWeiXinWork { get; set; }

        public virtual ICollection<Menu> Menu { get; set; }
        public virtual ICollection<ThirdpartyApplicationDetail> ThirdpartyApplicationDetail { get; set; }

        public virtual ICollection<Department> Departments { get; set; }
        public virtual ICollection<User> MymoooUsers { get; set; }

        public bool IsProduction { get; set; }
    }
}
