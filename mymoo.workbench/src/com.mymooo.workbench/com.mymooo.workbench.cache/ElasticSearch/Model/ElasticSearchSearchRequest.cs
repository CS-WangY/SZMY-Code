using System;
using System.Collections.Generic;
using System.Text;

namespace com.mymooo.workbench.cache.ElasticSearch.Model
{
    public class ElasticSearchSearchRequest
    {
        public ElasticSearchSearchRequest()
        {
            query = new Query();
            sort = new List<Dictionary<string, Dictionary<string, string>>>();
        }
        /// <summary>
        /// 
        /// </summary>
        public Query query { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? size { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? @from { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<Dictionary<string,Dictionary<string,string>>> sort { get; set; }
    }
    

    public class BoolWrap
    {
        /// <summary>
        /// 
        /// </summary>
        public ChildBool must { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ChildBool must_not { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ChildBool> should { get; set; }
    }

    public class ChildBool
    {
        public ChildBool()
        {
            @bool = new BoolWraps();
        }
        public BoolWraps @bool { get; set; }
    }
    public class BoolWraps
    {
        /// <summary>
        /// 
        /// </summary>
        public List<object> must { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<object> must_not { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<object> should { get; set; }
    }

    public class TermWrap
    {
        /// <summary>
        /// 单个匹配
        /// </summary>
        public Dictionary<string,string> term { get; set; }

    }

    /// <summary>
    /// 范围查询
    /// </summary>
    public class RangeWrap
    {
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> range { get; set; }

    }
    public class TermsWrap
    {
        /// <summary>
        /// 多个匹配，类似 in查询
        /// </summary>
        public Dictionary<string, string[]> terms { get; set; }
    }
    public class Filter
    {
        public Filter()
        {
            @bool=new BoolWrap();
        }
        /// <summary>
        /// 
        /// </summary>
        public BoolWrap @bool { get; set; }
    }

    public class Constant_score
    {
        public Constant_score()
        {
            filter = new Filter();
        }
        /// <summary>
        /// 
        /// </summary>
        public Filter filter { get; set; }
    }

    public class Query
    {
        public Query()
        {
            constant_score = new Constant_score();
        }
        /// <summary>
        /// 
        /// </summary>
        public Constant_score constant_score { get; set; }
    }






}
