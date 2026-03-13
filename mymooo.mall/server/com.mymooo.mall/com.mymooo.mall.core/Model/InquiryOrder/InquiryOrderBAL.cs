using com.mymooo.mall.core.Model.Quotation;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.CustomerInformation;

namespace com.mymooo.mall.core.Model.InquiryOrder
{
    /// <summary>
    /// 
    /// </summary>
	public class InquiryOrderBAL
    {

            public long Id { get; set; }

            /// <summary>
            /// 单号。
            /// </summary>
            public string Number { get; set; } = string.Empty;

            /// <summary>
            /// 买家。用户
            /// </summary>
            public virtual CustomerUser Customer { get; set; } = new CustomerUser();

            public virtual Company? Company { get; set; }

            /// <summary>
            /// 客户采购单号。
            /// </summary>
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

            public bool IsInternal { get; set; }

            /// <summary>
            /// 有效期。
            /// </summary>
            public DateTime ExpiryDate { get; set; }

            /// <summary>
            /// 提交日期。
            /// </summary>
            public DateTime QuotedOn { get; set; }

            #region Receive information.
            /// <summary>
            /// 收货公司。
            /// </summary>
            public string ReceiveCompany { get; set; } = string.Empty;

            /// <summary>
            /// 收货人手机号码。
            /// </summary>
            public string ReceiverMobile { get; set; } = string.Empty;

            /// <summary>
            /// 收货地址ID
            /// </summary>
            public long ReceiveId { get; set; }

        /// <summary>
        /// 收货地址。
        /// </summary>
        public string ReceiveAddress { get; set; } = string.Empty;

            /// <summary>
            /// 收货人。
            /// </summary>
            public string Receiver { get; set; } = string.Empty;

            /// <summary>
            /// 收货部门。
            /// </summary>
            public string ReceiveDepartment { get; set; } = string.Empty;
        #endregion

        #region Purchaser information.
        /// <summary>
        /// 订购人。
        /// </summary>
        public string Purchaser { get; set; } = string.Empty;

            /// <summary>
            /// 订购企业。
            /// </summary>
            public string PurchaseCompany { get; set; } = string.Empty;

            /// <summary>
            /// 订购部门。
            /// </summary>
            public string PurchaseDepartment { get; set; } = string.Empty;

        public string PurchaserMobile { get; set; } = string.Empty;

            public string PurchaserAddress { get; set; } = string.Empty;
            #endregion


            /// <summary>
            /// 是否删除。
            /// </summary>
            public bool Deleted { get; set; }

            /// <summary>
            /// 是否是客户自己删除的。
            /// </summary>
            public bool DeletedByCustomer { get; set; }

            /// <summary>
            /// 删除人Id。
            /// </summary>
            public long? DeletedBy { get; set; }

            /// <summary>
            /// 处理此单的业务员。
            /// </summary>
            public virtual ManagementUser? Salesman { get; set; }

            /// <summary>
            /// 替客户下单的操作员。
            /// </summary>
            public virtual ManagementUser? Inputter { get; set; }

            /// <summary>
            /// 处理此单的部门。
            /// </summary>
            public Guid DepartmentId { get; set; }

            public decimal ExpectedTotalWithTax { get; set; }

            public int ExpectedDeliveryDays { get; set; }

            /// <summary>
            /// 订单
            /// </summary>
            public InquiryDataSouce OrderSouce { get; set; }

            /// <summary>
            /// 是否为代理下单。
            /// </summary>
            public bool Agent { get; set; }

            /// <summary>
            /// 订单来源
            /// </summary>
            public InquiryDataSouce DataSources { get; set; }

            public OrderType OrderType { get; set; }



            /// <summary>
            /// 报价结算方式Id
            /// </summary>
            public Guid? InqPaymentMethodId { get; set; }

        /// <summary>
        /// 报价结算方式名称
        /// </summary>
        public string InqPaymentName { get; set; } = string.Empty;
      
        /// <summary>
        /// 是否非标单
        /// </summary>
        public bool IsNonStandard { get; set; }
    }
}
