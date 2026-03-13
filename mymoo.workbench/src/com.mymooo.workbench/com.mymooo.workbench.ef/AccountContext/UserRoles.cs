using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace com.mymooo.workbench.ef.AccountContext
{
    /// <summary>
    /// 用户和角色关系表
    /// </summary>
    public partial class UserRoles
    {
        [Key] //主键 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  //设置自增
        public long Id { get; set; }

        /// <summary>
        /// 用户id唯一标识
        /// </summary>
        [Required]
        public long UserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public long RoleId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [MaxLength(30)]
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateDate { get; set; }

        [ForeignKey("RoleId")]
        public virtual Roles Role { get; set; }
    }
}
