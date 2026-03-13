using com.mymooo.mall.core.Cache;
using Microsoft.Extensions.Options;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Cache;
using mymooo.core.Config;
using mymooo.core.HostedService;
using mymooo.core.Model.SqlSugarCore;
using SqlSugar;

namespace com.mymooo.mall.core.Account
{
    /// <summary>
    /// mall 上下文
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="serviceProvider"></param>
    /// <param name="sqlSugar"></param>
    /// <param name="rsaConfig"></param>
    /// <param name="redisCache"></param>
    /// <param name="mallMainConfig"></param>
    /// <param name="apigatewayConfig"></param>
    /// <param name="gatewayRedisCache"></param>
    /// <param name="productNumberCacheService"></param>
    /// <param name="productSelectionCacheService"></param>
    /// <param name="rabbitMQService"></param>
    [AutoInject(InJectType.Scope)]
	public class MallContext(IServiceProvider serviceProvider, MymoooSqlSugar sqlSugar, IOptions<RsaConfig> rsaConfig, RedisCache redisCache
			, IOptions<MymoooMainConfig> mallMainConfig, IOptions<ApigatewayConfig> apigatewayConfig, GatewayRedisCache gatewayRedisCache
			, ProductNumberCacheService productNumberCacheService, ProductSelectionCacheService productSelectionCacheService, RabbitMQMessageService<RabbitMQMessage> rabbitMQService) 
		: MymoooContext<User, RabbitMQMessage>(serviceProvider, rsaConfig, mallMainConfig, apigatewayConfig, sqlSugar, gatewayRedisCache, redisCache, rabbitMQService)
	{

		/// <summary>
		/// 产品选型
		/// </summary>
		public ProductSelectionCacheService ProductSelectionCache { get; } = productSelectionCacheService;

		/// <summary>
		/// 产品型号缓存
		/// </summary>
		public ProductNumberCacheService ProductNumberCache { get; } = productNumberCacheService;
	}
}
