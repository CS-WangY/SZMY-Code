using System;
using System.Collections.Generic;
using System.Text;

namespace com.mymooo.workbench.core.ElasticSearch
{
    public static class ElasticSearchEnum
    {
        public enum ElasticSearchConditionLogicType
        {
            And = 0,
            Or = 1
        }

        public enum ElasticSearchConditionType
        {
            /// <summary>
            /// 等于
            /// </summary>
            Equal=0,
            /// <summary>
            /// 不等于
            /// </summary>
            NotEqual=1,
            /// <summary>
            /// 包含 like"%%"
            /// </summary>
            Contian=2,
            /// <summary>
            /// 不包含
            /// </summary>
            NotContian=3,
            /// <summary>
            /// 小于
            /// </summary>
            LessThan=4,
            /// <summary>
            /// 大于
            /// </summary>
            MoreThan=5,
            /// <summary>
            /// 小于等于
            /// </summary>
            LessThanOrEqual= 6,
            /// <summary>
            /// 大于等于
            /// </summary>
            GreaterThanOrEquan = 7
        }

        /// <summary>
        /// 排序方式
        /// </summary>
        public enum SortType
        {
            ASC=0,
            DESC=1
        }
    }
}
