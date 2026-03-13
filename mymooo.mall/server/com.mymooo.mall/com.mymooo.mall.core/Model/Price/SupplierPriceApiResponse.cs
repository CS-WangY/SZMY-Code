using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Price
{
    public class SupplierPriceApiResponse
    {
            
            /// <summary>
            /// 产品Id
            /// </summary>
            public long ProductId { get; set; }

            /// <summary>
            /// 型号
            /// </summary>
            public string Number { get; set; } = string.Empty;

            /// <summary>
            /// 简易型号
            /// </summary>
            public string ShortNumber { get; set; } = string.Empty;

            /// <summary>
            /// 数量
            /// </summary>
            public decimal Qty { get; set; }
        public List<ProductPriceResponse> SuppliserPrice { get; set; } = [];
        
    }
    public class ProductPriceResponse
    {
        public string Version { get; set; } = string.Empty;

        public long SupplierId { get; set; }

        /// <summary>
        /// 供应商编码
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 供应商中文名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public int DeliveryDay { get; set; }

        public int MinOrderQuantity { get; set; }
        public int QuantityUpperLimit { get; set; }
    }


}
