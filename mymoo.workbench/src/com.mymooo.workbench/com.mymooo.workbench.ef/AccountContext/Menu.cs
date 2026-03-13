using com.mymooo.workbench.ef.ThirdpartyApplication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace com.mymooo.workbench.ef.AccountContext
{
    public partial class Menu
    {
        public Menu()
        {
            RolesMenu = new HashSet<RolesMenu>();
            MenuItem = new HashSet<MenuItem>();
        }

        [Key] //主键 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  //设置自增
        public long Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long ParentId { get; set; }

        /// <summary>
        /// 第三方应用id
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string AppId { get; set; }

        /// <summary>
        /// 菜单路径
        /// </summary>
        [MaxLength(30)]
        [Required]
        public string Path { get; set; }

        /// <summary>
        /// 组件路径
        /// </summary>
        [MaxLength(500)]
        [Required]
        public string Component { get; set; }

        /// <summary>
        /// 组件名称
        /// </summary>
        [MaxLength(30)]
        //[Required]
        public string Name { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [MaxLength(30)]
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        [MaxLength(30)]
        public string Icon { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 是否发布
        /// </summary>
        public bool IsPublish { get; set; }

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
        public string PublishUser { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime? PublishDate { get; set; }

        /// <summary>
        /// 角色菜单
        /// </summary>
        public virtual ICollection<RolesMenu> RolesMenu { get; set; }

        /// <summary>
        /// 菜单项
        /// </summary>
        public virtual ICollection<MenuItem> MenuItem { get; set; }

        /// <summary>
        /// 第三方应用消息
        /// </summary>
        [ForeignKey("AppId")]
        public virtual ThirdpartyApplicationConfig ThirdpartyApplicationConfig { get; set; }
    }
}
