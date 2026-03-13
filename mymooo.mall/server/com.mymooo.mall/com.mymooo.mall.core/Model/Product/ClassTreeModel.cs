using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Product
{
    public class ClassTreeModel
    {

        /// <summary>
        /// Desc:分类KEY
        /// Default:
        /// Nullable:False
        /// </summary>           
        public long ClassId { get; set; }

        /// <summary>
        /// Desc:父分类KEY
        /// Default:-1
        /// Nullable:False
        /// </summary>           
        public long ParentClassId { get; set; }

        /// <summary>
        /// Desc:分类名称
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string ClassName { get; set; } = string.Empty;

        /// <summary>
        /// Desc:分类节点排序
        /// Default:-1
        /// Nullable:False
        /// </summary>           
        public int Seq { get; set; }

        public List<ClassTreeModel> Childs { get; set; }

    }
}
