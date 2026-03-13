using System;
using System.Collections.Generic;
using System.Text;
using static com.mymooo.workbench.core.ElasticSearch.ElasticSearchEnum;

namespace com.mymooo.workbench.core.ElasticSearch
{
    public class ElasticSearchSort
    {
        public string SortField { get; set; }
        public SortType SortType { get; set; }
    }
}
