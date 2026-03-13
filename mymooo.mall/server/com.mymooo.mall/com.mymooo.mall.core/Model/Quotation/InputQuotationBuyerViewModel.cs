namespace com.mymooo.mall.core.Model.Quotation
{
    public class InputQuotationBuyerViewModel
    {
        public long CustomerId { get; set; }

        /// <summary>
        /// 个人客户编码
        /// </summary>
        public string CustomerCode { get; set; } = string.Empty;
        /// <summary>
        /// 企业客户编码
        /// </summary>
        public string CompanyCode { get; set; } = string.Empty;

        public string Company { get; set; } = string.Empty;

        public string ParentCompany { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string PaymentMethodName { get; set; } = string.Empty;
        public string Leave { get; set; } = string.Empty;

        public string Credit { get; set; } = string.Empty;

        public Guid? InqPaymentMethodId { get; set; }
    }
}
