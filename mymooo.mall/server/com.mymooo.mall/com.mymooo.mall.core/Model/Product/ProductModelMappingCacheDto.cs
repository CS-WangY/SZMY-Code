using mymooo.core.Attributes.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Product
{
    [RedisKey("mymooo-company", 2)]
    public class ProductModelMappingCacheDto
    {
        /// <summary>
        /// 
        /// </summary>
        [RedisPrimaryKey]
        public string CompanyCode { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;
        /// <summary>
        /// 客户物料号
        /// </summary>
        public string CustomerNumber { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;
        /// <summary>
        /// 客户型号
        /// </summary>
        [RedisMainField(seq: 1, groupId: "material-match")]
        public string CustomerModel { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public long SmallClassId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SmallClassName { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string ProductModel { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string ProductName { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string DiffRemark { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string CustomerRemark { get; set; } = string.Empty;
    }
}
