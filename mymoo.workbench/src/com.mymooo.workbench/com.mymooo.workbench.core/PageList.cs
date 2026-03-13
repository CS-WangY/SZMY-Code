using System;
using System.Collections.Generic;

namespace com.mymooo.workbench.core
{
    /// <summary>
    /// 分页后的数据集合
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class PageList<T>
    {

        /// <summary>
        /// 记录总条数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 分页数据
        /// </summary>
        public List<T> DataList { get; set; }
    }
}
