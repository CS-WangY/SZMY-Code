using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Config;
using com.mymooo.mall.core.Model.Cache;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Options;
using mymooo.core.Attributes;

namespace com.mymooo.mall.core.Cache
{
    /// <summary>
    /// 产品型号缓存
    /// </summary>
    [AutoInject(InJectType.Single)]
    public class ProductNumberCacheService : ElasticsearchClient
    {
        private readonly ElasticsearchConfig _elasticsearchConfig;

        /// <summary>
        /// 产品型号缓存
        /// </summary>
        /// <param name="elasticsearch"></param>
        public ProductNumberCacheService(IOptions<ElasticsearchConfig> elasticsearch) : base(CreateElasticsearchClient(elasticsearch.Value))
        {
            _elasticsearchConfig = elasticsearch.Value;

            var exists = this.Indices.ExistsAsync(_elasticsearchConfig.ProductNumberIndx).Result;
            if (!exists.Exists)
            {
                var response = this.Indices.CreateAsync<ProductNumberIndex>(_elasticsearchConfig.ProductNumberIndx,
                    p => p.Mappings(c => c.Properties(d => d.Text(a => a.Id, config => config.Fields(f => f.Keyword("keyword", fc => fc.IgnoreAbove(256))))
                    .Text(a => a.ProductNumber, config => config.Fields(f => f.Keyword("keyword", fc => fc.IgnoreAbove(256))))
                    .Text(a => a.ProductName, config => config.Fields(f => f.Keyword("keyword", fc => fc.IgnoreAbove(256))))
                    .Text(a => a.SmallName, config => config.Fields(f => f.Keyword("keyword", fc => fc.IgnoreAbove(256))))
                    .Text(a => a.MymoooNumberId, config => config.Index(false)).Text(a => a.MymoooProductNumber, config => config.Index(false))
                    .Text(a => a.MymoooProductName, config => config.Fields(f => f.Keyword("keyword", fc => fc.IgnoreAbove(256))))
                    .Text(a => a.SmallCode, config => config.Index(false)).Text(a => a.ShortNumber, config => config.Index(false))
                    .LongNumber(a => a.ProductId).LongNumber(a => a.TypeId).ByteNumber(a => a.CategoryType).LongNumber(a => a.SmallId).IntegerNumber(a => a.DataSource)))).Result;
            }
        }

        private static ElasticsearchClientSettings CreateElasticsearchClient(ElasticsearchConfig elasticsearch)
        {
            ElasticsearchClientSettings elasticsearchClient = new ElasticsearchClientSettings(new Uri(elasticsearch.Url)).DefaultIndex(elasticsearch.ProductNumberIndx).PrettyJson().RequestTimeout(TimeSpan.FromMinutes(10));
            return elasticsearchClient;
        }

        /// <summary>
        /// 重新加载缓存
        /// </summary>
        /// <param name="mymoooContext"></param>
        public void ReloadCache(MallContext mymoooContext)
        {
            int pageNumber = 1;
            var products = mymoooContext.SqlSugar.Queryable<ProductModelSmallClassMapping>().Includes(p => p.ProductSmallClass).OrderBy(p => p.Model).ToOffsetPage(pageNumber, 10000);
            while (products.Count > 0)
            {
                List<ProductNumberIndex> indexs = [];
                foreach (var product in products)
                {
                    var id = product.Model.Replace("-", "").ToLower().Trim();
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        continue;
                    }
                    var index = new ProductNumberIndex()
                    {
                        ProductNumber = product.Model.Trim(),
                        ProductName = product.Name,
                        ShortNumber = product.ShortNumber,
                        ProductId = product.ProductId,
                        SmallId = product.ProductSmallClassId,
                        SmallCode = product.ProductSmallClass?.Code ?? string.Empty,
                        SmallName = product.ProductSmallClass?.Name ?? string.Empty
                    };
                    if (product.ProductId > 0)
                    {
                        index.MymoooProductNumber = index.ProductNumber;
                        index.MymoooProductName = product.Name;
                    }
                    indexs.Add(index);
                }

                if (indexs.Count > 0)
                {
                    this.IndexMany(indexs);
                }
                products = mymoooContext.SqlSugar.Queryable<ProductModelSmallClassMapping>().Includes(p => p.ProductSmallClass).OrderBy(p => p.Model).ToOffsetPage(++pageNumber, 10000);
            }
        }

        /// <summary>
        /// 获取全部缓存数据
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProductNumberIndex>> GetAll()
        {
            MatchAllQuery matchAllQuery = new MatchAllQuery();
            var result = await this.SearchAsync<ProductNumberIndex>(s => s.From(0).Size(10000).Query(matchAllQuery));
            if (result.IsValidResponse)
            {
                return [.. result.Documents];
            }

            return [];
        }

        /// <summary>
        /// 查询缓存中的数量
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<long> QureyCount(ProductNumberQueryRequest request)
        {
            TermQuery termQuery = new(new Field("ProductId"))
            {
                Value = request.ProductId
            };
            var result = await this.CountAsync<ProductNumberIndex>(s => s.Query(q => q.Term(termQuery)));
            if (result.IsValidResponse)
            {
                return result.Count;
            }

            return 0;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<ProductNumberIndex>> Qurey(ProductNumberQueryRequest request)
        {
            TermQuery termQuery = new(new Field("ProductId"))
            {
                Value = request.ProductId
            };
            var result = await this.SearchAsync<ProductNumberIndex>(s => s.From(request.PageNumber).Size(request.PageSize).Query(q => q.Term(termQuery)));
            if (result.IsValidResponse)
            {
                return [.. result.Documents];
            }

            return [];
        }

        /// <summary>
        /// 按多个型号查询
        /// </summary>
        /// <param name="productNumbers"></param>
        /// <returns></returns>
        public async Task<List<ProductNumberIndex>> QureyNumberKeys(List<string> productNumbers)
        {
            List<FieldValue> fieldValues = [];
            productNumbers.ForEach(productNumber => fieldValues.Add(productNumber.Replace("-", "").Trim().ToLower()));
            var result = await this.SearchAsync<ProductNumberIndex>(s => s.From(0).Size(productNumbers.Count).Query(q => q.Terms(t => t.Field("id.keyword").Terms(new TermsQueryField(fieldValues)))));

            if (result.IsValidResponse)
            {
                return [.. result.Documents];
            }

            return [];
        }

        /// <summary>
        /// 按型号删除
        /// </summary>
        /// <param name="productNumber"></param>
        /// <returns></returns>
        public async Task<Result> Delete(string productNumber)
        {
            DeleteRequest deleteRequest = new(_elasticsearchConfig.ProductNumberIndx, productNumber.Replace("-", "").Trim().ToLower());
            var result = await this.DeleteAsync(deleteRequest);

            if (result.IsValidResponse)
            {
                return result.Result;
            }
            return Result.NotFound;
        }

        /// <summary>
        /// 按产品Id删除
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public async Task<Result> Delete(long productId)
        {
            TermQuery termQuery = new(new Field("ProductId"))
            {
                Value = productId
            };
            var result = await this.SearchAsync<ProductNumberIndex>(s => s.Query(termQuery));
            if (result.IsValidResponse)
            {
                foreach (var item in result.Documents)
                {
                    DeleteRequest deleteRequest = new(_elasticsearchConfig.ProductNumberIndx, item.Id);
                    var deleteResponse = await this.DeleteAsync(deleteRequest);
                }
            }

            return Result.Deleted;
        }
    }
}
