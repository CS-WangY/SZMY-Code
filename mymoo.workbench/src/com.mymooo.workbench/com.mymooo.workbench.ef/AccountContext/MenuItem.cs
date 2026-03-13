using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace com.mymooo.workbench.ef.AccountContext
{
    /// <summary>
    /// 菜单项权限
    /// </summary>
    public partial class MenuItem
    {
        public MenuItem()
        {
            RolesMenuItem = new HashSet<RolesMenuItem>();
        }

        [Key] //主键 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  //设置自增
        public long Id { get; set; }

        /// <summary>
        /// 菜单项
        /// </summary>
        [Required]
        public long MenuId { get; set; }

        /// <summary>
        /// 菜单路径
        /// </summary>
        [MaxLength(255)]
        [Required]
        public string Path { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [MaxLength(255)]
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// 是否控制权限
        /// </summary>
        public bool ControlPrivilege { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; }

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
        public string EnableUser { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime? EnableDate { get; set; }

        /// <summary>
        /// 菜单
        /// </summary>
        [ForeignKey("MenuId")]
        public virtual Menu Menu { get; set; }

        /// <summary>
        /// 角色与菜单项配置项
        /// </summary>
        public virtual ICollection<RolesMenuItem> RolesMenuItem { get; set; }

        /// <summary>
        /// 第三方应用id
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string AppId { get; set; }
    }
}
