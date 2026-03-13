using com.mymooo.workbench.cache.ElasticSearch.Model;
using com.mymooo.workbench.core.ElasticSearch;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using static com.mymooo.workbench.core.ElasticSearch.ElasticSearchEnum;

namespace com.mymooo.workbench.cache.ElasticSearch
{
    public static class ElasticSearchExtensions
    {
        /// <summary>
        /// 创建客户端
        /// </summary>
        /// <param name="index"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IElasticClient GetClient(string index, string url)
        {
            index = index.ToLower();
            var settings = new ConnectionSettings(new Uri(url)).DefaultIndex(index);
            var elasticClient = new ElasticClient(settings);
            ElasticSearchExtensions.CreateIndex(elasticClient, index);
            return elasticClient;
        }
        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="elasticClient"></param>
        public static CreateIndexResponse CreateIndex(this IElasticClient elasticClient, string indexName, int numberOfReplicas = 1, int numberOfShards = 5)
        {
            IIndexState indexState = new IndexState
            {
                Settings = new IndexSettings
                {
                    NumberOfReplicas = numberOfReplicas,
                    
                    NumberOfShards = numberOfShards
                }
            };
            Func<CreateIndexDescriptor, ICreateIndexRequest> func = x => x.InitializeUsing(indexState).Map(m => m.AutoMap());
            CreateIndexResponse response = elasticClient.Indices.Create(indexName, func);
            return response;
        }

        #region SDK所有方法
        /// <summary>
        /// 创建查询条件
        /// </summary>
        /// <param name="param"></param>
        /// <param name="queryType"></param>
        /// <returns></returns>
        public static QueryContainer[] CreateMatch(Dictionary<string, string> param, ElasticSearchConditionLogicType queryType)
        {
            List<QueryContainer> queryContainerList = new List<QueryContainer>();
            foreach (var item in param)
            {
                switch (queryType)
                {
                    case ElasticSearchConditionLogicType.And:
                        var andQuery = new TermQuery() { Field = item.Key, Value = item.Value };
                        queryContainerList.Add(andQuery);
                        break;
                    case ElasticSearchConditionLogicType.Or:
                        var orQuery = new MatchQuery() { Field = item.Key, Query = item.Value };
                        queryContainerList.Add(orQuery);
                        break;
                }
            }
            return queryContainerList.ToArray();
        }

        /// <summary>
        /// 创建查询器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="param"></param>
        /// <param name="queryType"></param>
        /// <returns></returns>
        public static QueryContainer CreateQueryContainer<T>(Dictionary<string, string> param, ElasticSearchConditionLogicType queryType) where T : class
        {
            return new QueryContainerDescriptor<T>().Bool(
                b => b.Must(
                    CreateMatch(param, queryType)));
        }


        #endregion

        /// <summary>
        /// 创建查询条件
        /// </summary>
        /// <param name="filterEntity"></param>
        public static ElasticSearchSearchRequest CreateSearchCondition(ESSearchRequest filterEntity)
        {
            ElasticSearchSearchRequest request = new ElasticSearchSearchRequest();
            if (filterEntity.MustCondition != null && filterEntity.MustCondition.Count > 0)
            {
                request.query.constant_score.filter.@bool.must = new ChildBool();
                request.query.constant_score.filter.@bool.must.@bool = CreateFilterCondition(filterEntity.MustCondition);
            }

            //if (filterEntity.NotMustCondition != null && filterEntity.NotMustCondition.Count > 0)
            //{
            //TODO
            //request.query.constant_score.filter.@bool.must = new ChildBool();
            //request.query.constant_score.filter.@bool.must.@bool = CreateFilterCondition(filterEntity.MustCondition);
            //}

            if (filterEntity.ShouldCondition != null && filterEntity.ShouldCondition.Count > 0)
            {
                request.query.constant_score.filter.@bool.should = new List<ChildBool>();
                foreach (var item in filterEntity.ShouldCondition)
                {
                    var childBool = new ChildBool();
                    childBool.@bool = CreateFilterCondition(item.ConditionList);
                    request.query.constant_score.filter.@bool.should.Add(childBool);
                }
            }

            if (filterEntity.Sort != null)
            {
                foreach (var item in filterEntity.Sort)
                {
                    var sortDic = new Dictionary<string, Dictionary<string, string>>();
                    var sortTypeDic = new Dictionary<string, string>();
                    sortTypeDic.Add("Order", item.SortType.ToString());
                    sortDic.Add(item.SortField, sortTypeDic);
                    request.sort.Add(sortDic);
                }
            }
            if (filterEntity.Page > 0)
            {
                request.size = filterEntity.PageSize;
                request.from = (filterEntity.Page - 1) * filterEntity.PageSize;
            }
            else
            {
                request.size = 1000;
                request.from = 0;
            }

            return request;
        }
        /// <summary>
        /// 创建查询条件
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static BoolWraps CreateFilterCondition(List<ElasticSearchCondition> condition)
        {
            var boolCondition = new BoolWraps();
            boolCondition.must = new List<object>();
            foreach (var item in condition)
            {
                if (item.Field.Contains("numberLimit"))
                {
                    //应急代码 需要修改 2022.07.14
                    var shouldWrap = new Model.Filter();
                    shouldWrap.@bool = new BoolWrap();
                    shouldWrap.@bool.should = new List<ChildBool>();
                    var filter1 = new ChildBool();
                    filter1.@bool.must = new List<object>();
                    var filter2 = new ChildBool();
                    filter2.@bool.must = new List<object>();

                    //限购为0 或者限购大于查询数量
                    var term = new TermWrap();
                    term.term = new Dictionary<string, string>();
                    term.term.Add(item.Field, "0");
                    filter1.@bool.must.Add(term);

                    var range = new RangeWrap();
                    range.range = new Dictionary<string, Dictionary<string, string>>();
                    var rangeFilter = new Dictionary<string, string>();
                    rangeFilter.Add("gte", item.Value);
                    range.range.Add(item.Field, rangeFilter);
                    filter2.@bool.must.Add(range);

                    shouldWrap.@bool.should.Add(filter1);
                    shouldWrap.@bool.should.Add(filter2);


                    boolCondition.must.Add(shouldWrap);
                    continue;
                }
                switch (item.ConditionType)
                {
                    case ElasticSearchEnum.ElasticSearchConditionType.Equal:
                        var term = new TermWrap();
                        term.term = new Dictionary<string, string>();
                        term.term.Add(item.Field, item.Value);
                        boolCondition.must.Add(term);
                        break;
                    case ElasticSearchEnum.ElasticSearchConditionType.Contian:
                        var terms = new TermsWrap();
                        terms.terms = new Dictionary<string, string[]>();
                        terms.terms.Add(item.Field, item.Value.Split(','));
                        boolCondition.must.Add(terms);
                        break;
                    case ElasticSearchConditionType.LessThanOrEqual:
                        var range = new RangeWrap();
                        range.range = new Dictionary<string,Dictionary<string,string>>();
                        var rangeFilter = new Dictionary<string, string>();
                        rangeFilter.Add("lte", item.Value);
                        range.range.Add(item.Field, rangeFilter);
                        boolCondition.must.Add(range);
                        break;
                    case ElasticSearchConditionType.GreaterThanOrEquan:
                        range = new RangeWrap();
                        range.range = new Dictionary<string, Dictionary<string, string>>();
                        rangeFilter = new Dictionary<string, string>();
                        rangeFilter.Add("gte", item.Value);
                        range.range.Add(item.Field, rangeFilter);
                        boolCondition.must.Add(range);
                        break;

                }
            }
            return boolCondition;
        }


        //public static void CreateSearchCondition(ElasticSearchSearchRequest request, List<ElasticSearchCondition> condition, int page=0, int pageSize=0, List<ElasticSearchSort> sort = null)
        //{
        //    foreach (var item in condition)
        //    {
        //        switch (item.ConditionType)
        //        {
        //            case ElasticSearchEnum.ElasticSearchConditionType.Equal:
        //                if (request.query.constant_score.filter.@bool.must == null)
        //                {
        //                    request.query.constant_score.filter.@bool.must = new List<object>();
        //                }
        //                var term = new TermWrap();
        //                term.term = new Dictionary<string, string>();
        //                term.term.Add(item.Field, item.Value);
        //                request.query.constant_score.filter.@bool.must.Add(term);
        //                break;
        //            case ElasticSearchEnum.ElasticSearchConditionType.Contian:
        //                if (request.query.constant_score.filter.@bool.must == null)
        //                {
        //                    request.query.constant_score.filter.@bool.must = new List<object>();
        //                }
        //                var terms = new TermsWrap();
        //                terms.terms = new Dictionary<string, string[]>();
        //                terms.terms.Add(item.Field, item.Value.Split(','));
        //                request.query.constant_score.filter.@bool.must.Add(terms);
        //                break;
        //            case ElasticSearchConditionType.LessThanOrEqual:
        //                break;
        //            case ElasticSearchConditionType.MoreThanOrEquan:
        //                break;

        //        }
        //    }
        //    if (sort != null)
        //    {
        //        foreach (var item in sort)
        //        {
        //            var sortDic = new Dictionary<string, Dictionary<string, string>>();
        //            var sortTypeDic = new Dictionary<string, string>();
        //            sortTypeDic.Add("Order", item.SortType.ToString());
        //            sortDic.Add(item.SortField, sortTypeDic);
        //            request.sort.Add(sortDic);
        //        }
        //    }
        //    if (page > 0)
        //    {
        //        request.size = pageSize;
        //        request.from = (page - 1) * pageSize;
        //    }
        //}

    }
}
