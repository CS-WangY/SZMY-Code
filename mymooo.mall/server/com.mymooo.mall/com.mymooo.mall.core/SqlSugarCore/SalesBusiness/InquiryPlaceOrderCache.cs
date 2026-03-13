using System;
using System.Linq;
using System.Text;
using mymooo.core.Attributes.Redis;
using SqlSugar;
namespace com.mymooo.mall.core.SqlSugarCore.SystemManagement
{
    ///<summary>
    ///
    ///</summary>
    [RedisKey("mymooo-inquiryplaceorder", isSaveMain: true)]
    public partial class InquiryPlaceOrderCache
    {
        /// <summary>
        /// 
        /// </summary>
       
        [RedisPrimaryKey(1, "-")]
        public string IKey { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [RedisValue]
        public string IValue { get; set; } = string.Empty;


    }
}