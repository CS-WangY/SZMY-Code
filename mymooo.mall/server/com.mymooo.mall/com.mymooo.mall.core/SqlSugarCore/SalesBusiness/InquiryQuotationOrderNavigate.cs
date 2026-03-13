using SqlSugar;

namespace com.mymooo.mall.core.SqlSugarCore.SalesBusiness
{
	public partial class InquiryQuotationOrder
	{
		[Navigate(NavigateType.OneToOne, nameof(InquiryId))]
		public InquiryOrder InquiryOrder { get; set; }


		[Navigate(NavigateType.OneToMany, nameof(QuotationId))]
		public List<InquiryQuotationOrderDetail> QuotationDetails { get; set; }

	}
}
