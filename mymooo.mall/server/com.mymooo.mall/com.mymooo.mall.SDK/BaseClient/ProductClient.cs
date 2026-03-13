using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation;
using com.mymooo.mall.SDK.Model;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.SDK.BaseClient
{
    /// <summary>
    /// 产品相关对外方法
    /// </summary>
    /// <param name="redisCache"></param>
    [AutoInject(InJectType.Single)]
    public class ProductClient(RedisCache redisCache)
    {
        private readonly RedisCache _redisCache = redisCache;

    }
}
