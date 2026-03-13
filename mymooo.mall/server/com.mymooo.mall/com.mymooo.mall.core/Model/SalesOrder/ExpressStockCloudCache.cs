using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mymooo.core.Attributes.Redis;
using SqlSugar;

namespace com.mymooo.mall.core.Model.SalesOrder
{

    /// <summary>
    /// 云仓存 的物流信息Model
    /// </summary>
    [RedisKey("mymooo-expressage", databaseId: 6)]
    public class ExpressStockCloudCache
    {

        /// <summary>
        /// 
        /// </summary>
        [RedisPrimaryKey]
        public string Mailno { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        [RedisValue]
        public string Routedata { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [RedisValue]
        public string Signforpicturl { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [RedisValue]
        public string Receiptpicturl { get; set; } = string.Empty;

  
    }
}
