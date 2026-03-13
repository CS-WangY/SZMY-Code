using System;
using System.Collections.Generic;
using System.Text;
using static com.mymooo.workbench.core.ElasticSearch.ElasticSearchEnum;

namespace com.mymooo.workbench.core.ElasticSearch
{
    public class ElasticSearchCondition
    {
        public string Field { get; set; }
        public string Value { get; set; }
        public ElasticSearchConditionType ConditionType { get; set; }
    }
}
