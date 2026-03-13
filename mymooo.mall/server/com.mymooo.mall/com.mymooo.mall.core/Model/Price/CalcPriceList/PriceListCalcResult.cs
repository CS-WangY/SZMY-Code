using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mymooo.mall.core.Model.Quotation;

namespace com.mymooo.mall.core.Model.Price.CalcPriceList
{
    public class PriceListCalcResult
    {/// <summary>
     /// 产品Id
     /// </summary>
        public long ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

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
        /// <summary>
        /// 小类Id
        /// </summary>
        public long SmallId { get; set; }
        /// <summary>
        /// 小类名称
        /// </summary>
        public string SmallName { get; set; } = string.Empty;

        public List<PriceListProductPrice>? Prices { get; set; }
    }

    public class PriceListProductPrice
    {
        public PriceListDataType? PriceListDataType { get; set; }
        public PriceListType? PriceListType { get; set; }
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
        /// <summary>
        /// 数量上限
        /// </summary>
        public int NumberLimit { get; set; }
        /// <summary>
        /// 最小起批量
        /// </summary>
        public int Moq { get; set; }


        /// <summary>
        /// 价目表Id
        /// </summary>
        public long PriceListId { get; set; }

        /// <summary>
        /// 更改记录Id
        /// </summary>
        public long ChangeId { get; set; }

        /// <summary>
        /// 简易型号 
        /// </summary>
        public string ShortNumber { get; set; } = string.Empty;
        /// <summary>
        /// 企业编码
        /// </summary>
        public string[] CompanyCodes { get; set; }  = new string[0];

        /// <summary>
        /// 价目表审核状态 0未审核，1通过审核
        /// </summary>
        public int AuditStatus { get; set; }
        /// <summary>
        /// 是否启用，只有当前使用中版本才会启用，历史版本是禁用
        /// </summary>
        public bool IsEnable { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// 数量折扣
        /// </summary>
        public decimal QuantityDiscount { get; set; }
    }

    
}
