using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.cache.ElasticSearch.Model
{

    public class Error
    {
        /// <summary>
        /// 
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string reason { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string index_uuid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string shard { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string index { get; set; }
    }

    public class Create
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
        public int status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Error error { get; set; }
    }

    public class ItemsItem
    {
        /// <summary>
        /// 
        /// </summary>
        public Create create { get; set; }
    }

    public class ElasticSearchBulkAddResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public int took { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool errors { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ItemsItem> items { get; set; }
    }

}
