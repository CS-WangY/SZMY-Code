using System;
using System.Collections.Generic;
using System.Text;

namespace com.mymooo.workbench.cache.ElasticSearch.Model
{
    public class ElasticSearchBulkGetResponse
    {
        public List<ResponseDocument> docs { get; set; }
    }
    public class ResponseDocument
    {
        /// <summary>
        /// 
        /// </summary>
        public string _index { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string _id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int _version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int _seq_no { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int _primary_term { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string found { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string _source { get; set; }
    }

}
