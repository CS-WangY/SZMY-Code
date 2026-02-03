using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Cache;
using mymooo.core.Config;
using mymooo.core.Model.SqlSugarCore;

namespace mymooo.weixinWork.Models
{

    [AutoInject(InJectType.Scope)]
	public class WeixinWorkContext(IServiceProvider serviceProvider, IOptions<RsaConfig> rsaConfig, IOptions<MymoooMainConfig> mainConfig, IOptions<ApigatewayConfig> apigatewayConfig, MymoooSqlSugar sqlSugar, GatewayRedisCache gatewayRedisCache, WorkbenchRedisCache workbenchRedisCache, RedisCache redisCache, IMemoryCache memoryCache) 
		: 		MymoooContext<User>(serviceProvider, rsaConfig, mainConfig, apigatewayConfig, sqlSugar, gatewayRedisCache, workbenchRedisCache, redisCache, memoryCache)
	{
	}
}
