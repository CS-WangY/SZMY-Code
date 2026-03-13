using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Model.Product;
using mymooo.core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.business.Service.ProductServices
{
    [AutoInject(InJectType.Scope)]
    public class ProductModelMappingCacheService(MallContext mymoooContext)
    {
        private readonly MallContext _mymoooContext = mymoooContext;
       
        public void UpdateMappingCache(List<ProductModelMappingCacheDto> mappingList)
        {
            foreach (var item in mappingList)
            {
                _mymoooContext.RedisCache.HashSet(item);
            }
        }

        public void GetMappingCache(ProductModelMappingCacheDto mapping)
        {
            mapping =_mymoooContext.RedisCache.HashGet(mapping);
        }
    }
}
