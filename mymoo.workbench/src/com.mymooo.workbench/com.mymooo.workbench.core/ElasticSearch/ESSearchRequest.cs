using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.ElasticSearch
{
    public class ESSearchRequest
    {
        public string IndexName { get; set; }
        /// <summary>
        /// 开始行数
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 结束行数
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// and语句
        /// </summary>
        public List<ElasticSearchCondition> MustCondition { get; set; }
        /// <summary>
        /// and 不等于条件
        /// </summary>
        public List<ElasticSearchCondition> NotMustCondition { get; set; }
        /// <summary>
        /// or语句
        /// </summary>
        public List<ConditionWrap> ShouldCondition { get; set; }
        public List<ElasticSearchSort> Sort { get; set; }
    }

    public class ConditionWrap
    {
        public ConditionWrap(string productNumber, List<ElasticSearchCondition> conditionList)
        {
            ProductNumber = productNumber;
            ConditionList = conditionList;
        }

        public string ProductNumber { get; set; }
        public List<ElasticSearchCondition> ConditionList { get; set; }
    }
}
