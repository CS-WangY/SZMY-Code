using System;
using System.Collections.Generic;
using System.Text;

namespace com.mymooo.workbench.cache.ElasticSearch.Model
{
    public class ElasticSearchBulkGetRequest
    {
        public ElasticSearchBulkGetRequest()
        {
            docs = new List<DocItem>();
        }
        public List<DocItem> docs { get; set; }
    }
    public class DocItem
    {
        public string _index { get; set; }
        public string _id { get; set; }
    }
}
