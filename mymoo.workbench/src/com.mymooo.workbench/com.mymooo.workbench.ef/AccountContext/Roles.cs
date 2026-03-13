using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace com.mymooo.workbench.ef.AccountContext
{
    /// <summary>
    /// 角色
    /// </summary>
    public partial class Roles
    {
        public Roles()
        {
            RolesMenu = new HashSet<RolesMenu>();
            UserRoles = new HashSet<UserRoles>();
        }

        [Key] //主键 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  //设置自增
        public long Id { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        [MaxLength(30)]
        [Required]
        public string Code { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [MaxLength(30)]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool IsForbidden { get; set; }

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
        /// 
        /// </summary>
        [MaxLength(30)]
        public string ForbiddenUser { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime? ForbiddenDate { get; set; }

        public bool IsAdmin { get; set; }

        public virtual ICollection<RolesMenu> RolesMenu { get; set; }
        public virtual ICollection<UserRoles> UserRoles { get; set; }
    }
}
