using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.mymooo.workbench.Models
{
    public class MenuItemGridModel
    {
        public long Id { get; set; }

        /// <summary>
        /// 菜单项
        /// </summary>
        public long MenuId { get; set; }

        /// <summary>
        /// 菜单路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 是否控制权限
        /// </summary>
        public bool ControlPrivilege { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 启用人
        /// </summary>
        public string EnableUser { get; set; }

        /// <summary>
        /// 启用日期
        /// </summary>
        public DateTime? EnableDate { get; set; }


        /// <summary>
        /// 第三方应用id
        /// </summary>
        public string AppId { get; set; }

        public string MenuTitle { get; set; }

        public string AppName { get; set; }
    }
}
