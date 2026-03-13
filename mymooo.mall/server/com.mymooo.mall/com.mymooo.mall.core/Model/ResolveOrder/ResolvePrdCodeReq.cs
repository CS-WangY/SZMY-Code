using com.mymooo.mall.core.Utils.JsonConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json.Serialization;
namespace com.mymooo.mall.core.Model.ResolveOrder
{

    /// <summary>
    /// 分解单产品型号请求
    /// </summary>
    public class ResolveItemsReq
    {
      

        public string CompanyCode {  get; set; } = string.Empty;
        [JsonConverter(typeof(ProductModelConverter))]
        public string SearchPrdCode { get; set;} = string.Empty;


        public DateTime? AuditDateStart { get; set; }
        public DateTime? AuditDateEnd { get; set; }

        public string ReplaceType { get; set; } = string.Empty;
        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 50;
        
    }

    public class ResolveProductItem
    {
        public string CustPrdCode { get; set; } = string.Empty;
        public string PrdCode { get; set; } = string.Empty;
    }

    public class ResoleMapResp
    {
        public string  AuditTime { get; set; } = string.Empty;
        public string ResolvedOrderNumber { get; set; } = string.Empty;
        public long   ProductId { get; set; }
        public string PrdCode { get; set;} = string.Empty;
        public string CustPrdCode { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        public string CustomerCode { get; set; } = string.Empty;

        public string ReplaceType { get; set; } = string.Empty;

        public string Brand {  get; set; } = string.Empty;
    }

    public class ProductResolveMappingDto
    {
        public DateTime? AuditTime { get; set; }
        public string ResolvedOrderNumber { get; set; } = string.Empty;
        public long ProductId { get; set; }
        public string PrdCode { get; set; } = string.Empty;
        public string CustPrdCode { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        public string CustomerCode { get; set; } = string.Empty;

        public string ReplaceType { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;
    }
}
