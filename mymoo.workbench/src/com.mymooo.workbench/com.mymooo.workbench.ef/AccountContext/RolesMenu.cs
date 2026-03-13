using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace com.mymooo.workbench.ef.AccountContext
{
    /// <summary>
    /// 角色菜单
    /// </summary>
    public partial class RolesMenu
    {
        [Key] //主键 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  //设置自增
        public long Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public long RoleId { get; set; }

        [Required]
        public long MenuId { get; set; }

        /// <summary>
        /// 是否有权
        /// </summary>
        public bool IsRight { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [MaxLength(30)]
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 菜单
        /// </summary>
        [ForeignKey("MenuId")]
        public virtual Menu Menu { get; set; }

        [ForeignKey("RoleId")]
        public virtual Roles Role { get; set; }
    }
}
