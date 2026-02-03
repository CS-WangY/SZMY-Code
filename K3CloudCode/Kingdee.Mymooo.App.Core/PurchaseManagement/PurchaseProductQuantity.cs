using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core.StockManagement;

namespace Kingdee.Mymooo.App.Core.PurchaseManagement
{
    public class PurchaseProductQuantity
    {
        public List<QuantityResponse> GetPurchaseProductQuantity(Context ctx, QuantityRequest request)
        {
            var sSql = "";
            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sSql);
            var result = new List<QuantityResponse>();
            foreach (var item in datas)
            {
                result.Add(new QuantityResponse
                {
                    ProductModel = "",
                    TotalPurchaseQuantity = 1,
                    LastInventoryQuantity = 0,
                    LastInventoryDateTime = DateTime.Now,
                });
            }
            return result;
        }
    }
    public class QuantityRequest
    {
        public string CompanyId { get; set; }
        public List<string> ProductModel { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
    public class QuantityResponse
    {
        /// <summary>
        /// 产品型号
        /// </summary>
        public string ProductModel { get; set; }
        /// <summary>
        /// 总采购数量
        /// </summary>
        public int TotalPurchaseQuantity { get; set; }
        /// <summary>
        /// 总入库数量
        /// </summary>
        public int LastInventoryQuantity { get; set; }
        /// <summary>
        /// 最后入库时间
        /// </summary>
        public DateTime LastInventoryDateTime { get; set; }
    }
}
