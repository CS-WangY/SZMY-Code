using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.ElasticSearch
{
    public class ElasticSearchSearchResponseV2
    {
        /// <summary>
        /// 
        /// </summary>
        public int? took { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Hits hits { get; set; }
    }

    public class _shards
    {
        /// <summary>
        /// 
        /// </summary>
        public int? total { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? successful { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? skipped { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? failed { get; set; }
    }

    public class Total
    {
        /// <summary>
        /// 
        /// </summary>
        public int? value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string relation { get; set; }
    }



    public class HitsItem
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
        public decimal? _score { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public dynamic _source { get; set; }
    }

    public class Hits
    {
        /// <summary>
        /// 
        /// </summary>
        public Total total { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? max_score { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<HitsItem> hits { get; set; }
    }
}
