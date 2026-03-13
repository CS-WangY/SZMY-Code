using com.mymooo.mall.core.Model.QuotationOrder;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation;
using Elastic.Clients.Elasticsearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.InquiryOrder
{

    public class InquiryItem 
    {
        public long Id { get; set; }

        public int Seq { get; set; }
      //  public InquiryOrderBAL InquiryOrder { get; set; } 

      //  public virtual QuotedItem QuotedItem { get; set; } 

        /// <summary>
        /// 产品Id。
        /// </summary>
        public long ProductSeriesId { get; set; }

        /// <summary>
        /// 类型Id
        /// </summary>
        public long TypeId { get; set; }

        /// <summary>
        /// 产品型号。
        /// </summary>
        public string ProductCode { get; set; } = string.Empty;

        /// <summary>
        /// 产品名称，存在直接报价情况，所以可以输入产品名称。
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// 产品工程师
        /// </summary>
        public long ProductEngineerId { get; set; }

        public string ProductEngineerName { get; set; } = string.Empty;

        public string ProductManagerName { get; set; } = string.Empty;

        public long ProductManagerId { get; set; }

        /// <summary>
        /// 产品数量。
        /// </summary>
        public int Qty { get; set; }

        /// <summary>
        /// 客户零件号/模号/料号。
        /// </summary>
        public string CustomCode { get; set; } = string.Empty;

        /// <summary>
        /// 客户产品名称
        /// </summary>
        public string CustItemName { get; set; } = string.Empty;

        /// <summary>
        /// 项目号
        /// </summary>
        public string ProjectNo { get; set; } = string.Empty;

        /// <summary>
        /// 客户料号
        /// </summary>
        public string CustItemNo { get; set; } = string.Empty;

        /// <summary>
        /// 库存管理特征
        /// </summary>
        public string StockFeatures { get; set; } = string.Empty;

        /// <summary>
        /// 状态。
        /// </summary>
        public InquiryStatus Status { get; set; }


        public Guid? BrandId { get; set; }

        // 似乎没啥用
        public byte ProductType { get; set; } = 0;
        
        public string Materials { get; set; } = string.Empty;
        public decimal BargainPriceWithTax { get; set; }
        public int BargainDispatchDays { get; set; }
        public DateTime SubmitDate { get; set; }
        public bool IsHistory { get; set; }

        public long ProductSmallClassId { get; set; }
        /// <summary>
        /// 事业部Id
        /// </summary>
        public string BusinessDivisionId { get; set; } = string.Empty;
        /// <summary>
        /// 事业部名称
        /// </summary>
        public string BusinessDivisionName { get; set; } = string.Empty;

        /// <summary>
        /// 供货组织Id
        /// </summary>
        public long SupplyOrgId { get; set; }

        /// <summary>
        /// 供货组织名称
        /// </summary>
        public string SupplyOrgName { get; set; } = string.Empty;

        public string Storage { get; set; } = string.Empty;

        public string ShortNumber {  get; set; } = string.Empty;
        // 是否特价项.   项目首次报价为特价项
        public bool IsSpecial { get; set; }

        /// <summary>
        /// 获取状态名称。
        /// </summary>
        /// <returns></returns>
        public string GetStatusName()
        {
            switch (Status)
            {
                case InquiryStatus.Invalid:
                    return "已失效";
                case InquiryStatus.Purchased:
                    return "已订购";
                case InquiryStatus.Quoted:
                    return "已报价";
                case InquiryStatus.WaitingForQuote:
                    return "待报价";
                case InquiryStatus.NoQuotation:
                    return "不报价";
                default:
                    return "";
            }
        }
    }
}
