using com.mymooo.mall.core.Model.Message;
using com.mymooo.mall.core.Model.Product;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation;
using com.mymooo.mall.SDK.Model;
using Microsoft.Extensions.Options;
using mymooo.core;
using mymooo.core.Attributes;
using mymooo.core.Cache;
using mymooo.core.Config;
using mymooo.core.Utils.JsonConverter;
using mymooo.core.Utils.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.SDK
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpService"></param>
    /// <param name="apigatewayConfig"></param>
    [AutoInject(InJectType.Single)]
    public class ProductClassClient(HttpService httpService, IOptions<ApigatewayConfig> apigatewayConfig, RedisCache redisCache)
    {
        private readonly RedisCache _redisCache = redisCache;
        private readonly HttpService _httpService = httpService;
        private readonly ApigatewayConfig _apigatewayConfig = apigatewayConfig.Value;

        public async Task<ResponseMessage<List<ClassTreeModel>>> GetClassTree()
        {
            var result = await _httpService.InvokeWebServiceAsync($"platformAdmin/{_apigatewayConfig.EnvCode}/mallapi/ProductClass/GetClassTree", "");
            if (string.IsNullOrWhiteSpace(result))
            {
                return new ResponseMessage<List<ClassTreeModel>>();
            }
            else
            {
                return JsonSerializerOptionsUtils.Deserialize<ResponseMessage<List<ClassTreeModel>>>(result);
            }
        }


        /// <summary>
        /// 获取产品小类相关信息
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public ProductClassSimpleModel GetProductClass(long productId)
        {
            var classResult = new ProductClassSimpleModel();
            var product = new Product()
            {
                ProductId = productId,
            };
            var result = _redisCache.HashGet(product);
            if (result != null && result.ProductSmallClass != null)
            {
                classResult.ClassId = result.ProductSmallClass.ParentId;
                classResult.ClassName = result.ProductClass.ClassName;
                classResult.SmallClassId = result.ProductSmallClass.Id;
                classResult.SmallClassName = result.ProductSmallClass.Name;
                if (result.ProductSmallClass.ProductEngineer != null)
                {
                    classResult.ProductEngineerId = result.ProductSmallClass.ProductEngineerId;
                    classResult.ProductEngineerName = result.ProductSmallClass.ProductEngineer.UserName;
                }

            }
            return classResult;
        }
    }
}
