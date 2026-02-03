using mymooo.core.Account;
using mymooo.core.Cache;
using mymooo.core.Config;
using mymooo.core.Model.SqlSugarCore;
using mymooo.core.Utils.Service;

namespace mymooo.k3cloud.core.Account
{
    /// <summary>
    /// 金蝶上下文
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="serviceProvider">注册器</param>
    /// <param name="rsaConfig">加密解密密钥</param>
    /// <param name="mainConfig">main配置</param>
    /// <param name="apigatewayConfig">网关配置</param>
    /// <param name="sqlSugar">数据库</param>
    /// <param name="gatewayRedisCache">缓存配置</param>
    /// <param name="workbenchRedisCache"></param>
    /// <param name="redisCache">缓存</param>
    /// <param name="memoryCache"></param>
    /// <param name="rabbitMQService"></param>
    public class KingdeeContent(IServiceProvider serviceProvider, RsaConfig rsaConfig,MymoooMainConfig mainConfig, ApigatewayConfig apigatewayConfig, MymoooSqlSugar sqlSugar, GatewayRedisCache gatewayRedisCache, WorkbenchRedisCache workbenchRedisCache, RedisCache redisCache, MemoryCacheClient memoryCache, KafkaSendService<KingdeeContent, User> rabbitMQService)
        : MymoooContext<User>(serviceProvider, rsaConfig, mainConfig, apigatewayConfig, sqlSugar, gatewayRedisCache, workbenchRedisCache, redisCache, memoryCache)
    {
        /// <summary>
        /// 发送消息服务
        /// </summary>
        public KafkaSendService<KingdeeContent, User> RabbitMQService { get; } = rabbitMQService;
    }
}
