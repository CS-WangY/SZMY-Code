using System;
using System.Collections.Generic;
using System.Text;

namespace com.mymooo.workbench.cache.ElasticSearch.Model
{

    public class Script
    {
        /// <summary>
        /// 
        /// </summary>
        public string source { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string lang { get; set; }
        public Dictionary<string,string> @params { get; set; }
    }

    public class ElasticSearchUpdateRequest
    {
        public ElasticSearchUpdateRequest()
        {
            script = new Script();
        }
        /// <summary>
        /// 
        /// </summary>
        public Script script { get; set; }
    }

}
