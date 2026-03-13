using com.mymooo.mall.core.Model.InquiryOrder;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.mall.core.Model.Quotation
{
    public class PlaceQuotationOrderRequest
    {
        public required long AddressId { get; set; }

        public Guid? CompanyId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public OrderType OrderType { get; set; }

        public required InputQuotationBuyerViewModel Buyer { get; set; }

        public string? CustomerPurchaseNumber { get; set; }

        /// <summary>
        /// 销售组织编码
        /// </summary>
        public string SalesOrganizationCode { get; set; } = string.Empty;

        /// <summary>
        /// 特价单
        /// </summary>
        public bool SpecialPrice { get; set; }

        /// <summary>
        /// 内部订单
        /// </summary>
        public bool IsInternal { get; set; }

        /// <summary>
        /// 销售组织名称
        /// </summary>
        public string SalesOrganizationName { get; set; } = string.Empty;

        public required IEnumerable<ConfirmQuotationItemViewModel> QuotationItems { get; set; }

        /// <summary>
        /// 订单来源
        /// </summary>
        public InquiryDataSouce Datasouces { get; set; }

        /// <summary>
        /// 报价单号
        /// </summary>
        public string? InquiryNumber { get; set; }


        /// <summary>
        /// 是否非标单
        /// </summary>
        public bool IsNonStandard { get; set; }

        /// <summary>
        /// 上传附件的路径
        /// </summary>
        public string UploadPath { get; set; }
    }
}
