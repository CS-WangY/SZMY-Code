//using com.mymooo.mall.core.Model.Address;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.CustomerInformation;

namespace com.mymooo.mall.core.Model.Quotation
{
    public class ConfirmQuotationViewModel
    {
        public CustomerAddress? Receive { get; set; } 

        public string CustomerPurchaseNumber { get; set; } = string.Empty;

        /// <summary>
        /// 销售组织编码
        /// </summary>
        public string SalesOrganizationCode { get; set; } = string.Empty;

        /// <summary>
        /// 销售组织名称
        /// </summary>
        public string SalesOrganizationName { get; set; } = string.Empty;

        /// <summary>
        /// 特价单
        /// </summary>
        public bool SpecialPrice { get; set; }

        public InputQuotationBuyerViewModel? Buyer { get; set; }

        public IList<ConfirmQuotationItemViewModel>? QuotationItems { get; set; }

        public string OriginalModel { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 内部订单
        /// </summary>
        public bool IsInternal { get; set; }

        public string InquiryNumber { get; set; } = string.Empty;

        /// <summary>
        /// 是否非标单
        /// </summary>
        public bool IsNonStandard { get; set; }

        /// <summary>
        ///  上传附件的路径
        /// </summary>
        public string UploadPath { get; set; }
    }
}
