using mymooo.core.Attributes.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage
{
    [RedisKey("mymooo-expressage", 6, isSaveMain: false)]
    public class ExpressageRedisCache
    {
        /// <summary>
        /// 物流单号
        /// </summary>
        [RedisPrimaryKey]
        public string ExpressageNumber { get; set; }
        /// <summary>
        /// 回单图片链接
        /// </summary>
        [RedisValue]
        public string Receiptpicturl { get; set; }
        /// <summary>
        /// 签单图片链接
        /// </summary>
        [RedisValue]
        public string Signforpicturl { get; set; }
        /// <summary>
        /// 路由信息
        /// </summary>
        [RedisValue(isJson:true)]
        public List<ExpressageRouteDataEntity> Routedata { get; set; }

    }
}
