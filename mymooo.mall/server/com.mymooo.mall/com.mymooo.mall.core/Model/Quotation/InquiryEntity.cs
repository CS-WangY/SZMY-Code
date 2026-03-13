
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mymooo.mall.core.Model.Message;

namespace com.mymooo.mall.core.Model.Quotation
{
    #region q前台

    /// <summary>
    /// 询价单对比信息
    /// </summary>
    public class InquiryContrastInfo
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public InquiryContrastInfo()
        {
            this.FactroyInfos = [];
            this.Entrys = [];
        }
        /// <summary>
        /// 询价单单号
        /// </summary>
        public required string InquiryCode { get; set; }

        /// <summary>
        /// 询价单日期
        /// </summary>
        public DateTime InquiryDate { get; set; }

        /// <summary>
        /// 供应商信息
        /// </summary>
        public List<InquiryFactoyyInfo> FactroyInfos { get; set; }

        /// <summary>
        /// 询价单明细信息
        /// </summary>
        public List<InquiryContrastEntry> Entrys { get; set; }
    }

    /// <summary>
    /// 询价对比的供应商信息
    /// </summary>
    public class InquiryFactoyyInfo
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public InquiryFactoyyInfo()
        {
            this.QuoteInfos = [];
        }

        /// <summary>
        /// 供应商内码
        /// </summary>
        public long FacId { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 供应商类别
        /// </summary>
        public string? FacTypeId { get; set; }

        /// <summary>
        /// 供应商类别
        /// </summary>
        public string? FacType { get; set; }

        /// <summary>
        /// 供应商地区
        /// </summary>
        public string? Area { get; set; }

        /// <summary>
        /// 报价项目数
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// 报价总金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 报价货期
        /// </summary>
        public decimal DeliveryTime { get; set; }

        /// <summary>
        /// 报价备注
        /// </summary>
        public string? Remarks { get; set; }

        /// <summary>
        /// 发票
        /// </summary>
        public string? Invoice { get; set; }

        /// <summary>
        /// 税率
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// 运费
        /// </summary>
        public decimal FreightRate { get; set; }

        /// <summary>
        /// 运费折扣
        /// </summary>
        public decimal FreightRateDiscount { get; set; }

        /// <summary>
        /// 供应商报价信息
        /// </summary>
        public List<FactoryQuoteInfo> QuoteInfos { get; set; }
    }

    /// <summary>
    /// 询价单对比明细信息
    /// </summary>
    public class InquiryContrastEntry
    {
        /// <summary>
        /// 询价单分录内码
        /// </summary>
        public long InquiryDetailId { get; set; }

        /// <summary>
        /// 产品内码
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// 产品图片路径
        /// </summary>
        public string? ProductUrl { get; set; }

        /// <summary>
        /// 品牌编码
        /// </summary>
        public string? BrandCode { get; set; }

        /// <summary>
        /// 品牌名称
        /// </summary>
        public string? BrandName { get; set; }

        /// <summary>
        /// 产品编码
        /// </summary>
        public string? ProductCode { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string? ProductName { get; set; }

        /// <summary>
        /// 订购数量
        /// </summary>
        public decimal Num { get; set; }

    }

    /// <summary>
    /// 供应商报价信息
    /// </summary>
    public class FactoryQuoteInfo
    {
        /// <summary>
        /// 询价单分录内码
        /// </summary>
        public long InquiryDetailId { get; set; }

        /// <summary>
        /// 产品内码
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// 主id
        /// </summary>
        public long MainId { get; set; }

        /// <summary>
        /// 报价单分录内码
        /// </summary>
        public long EntryId { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 状态 -1 无法报价 0 未报价 1 已报价
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 单价需要显示的信息
        /// </summary>
        public string? ShowPrice { get; set; }

        /// <summary>
        /// 货期
        /// </summary>
        public decimal DeliveDays { get; set; }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string? Tooltips { get; set; }
    }

    /// <summary>
    /// 询价单信息
    /// </summary>
    //public class InquiryInfo
    //{
    //    public InquiryInfo()
    //    {
    //        InqDetailList = new List<InqDetailInfo>();
    //        FacList = new List<FntFacInfo>();
    //    }

    //    #region 主单信息
    //    public bool Deleted { get; set; }

    //    /// <summary>
    //    /// 询价单ID
    //    /// </summary>
    //    public long InqId { get; set; }

    //    /// <summary>
    //    /// 询价单编号
    //    /// </summary>
    //    public string InqCode { get; set; }

    //    /// <summary>
    //    /// 用户ID
    //    /// </summary>
    //    public long UserId { get; set; }

    //    /// <summary>
    //    /// 客户ID
    //    /// </summary>
    //    public long CustId { get; set; }

    //    /// <summary>
    //    /// 询价单状态
    //    /// </summary>
    //    public long InqState { get; set; }

    //    /// <summary>
    //    /// 明细条数
    //    /// </summary>
    //    public long Items { get; set; }

    //    /// <summary>
    //    /// 批核标识
    //    /// </summary>
    //    public long InqInd { get; set; }

    //    /// <summary>
    //    /// 品牌Id
    //    /// </summary>
    //    public long BrandId { get; set; }

    //    /// <summary>
    //    /// 品牌名称
    //    /// </summary>
    //    public string BrandName { get; set; }

    //    /// <summary>
    //    /// 品牌名称
    //    /// </summary>
    //    public string BrandNameEn { get; set; }

    //    /// <summary>
    //    /// 提交日期(创建/修改日期)
    //    /// </summary>
    //    public DateTime SubmitDate { get; set; }

    //    /// <summary>
    //    /// 生成订单日期
    //    /// </summary>
    //    public DateTime GenerateDate { get; set; }

    //    /// <summary>
    //    /// 报价日期
    //    /// </summary>
    //    public DateTime CreateDate { get; set; }

    //    /// <summary>
    //    /// 截止报价时间
    //    /// </summary>
    //    public DateTime StopDate { get; set; }
    //    #endregion

    //    #region 联系人信息
    //    /// <summary>
    //    /// 联系人ID
    //    /// </summary>
    //    public long CustPayId { set; get; }

    //    /// <summary>
    //    /// 联系人名称
    //    /// </summary>
    //    public string Payer { get; set; }

    //    /// <summary>
    //    /// 联系人电话
    //    /// </summary>
    //    public string PayTel { get; set; }

    //    /// <summary>
    //    /// 手机
    //    /// </summary>
    //    public string PayerMobile { get; set; }

    //    public string PayerCompany { get; set; }

    //    public string PayerDepartment { get; set; }
    //    #endregion

    //    #region 收货人信息
    //    /// <summary>
    //    /// 收货ID
    //    /// </summary>
    //    public long CustAddrId { get; set; }

    //    /// <summary>
    //    /// 地址
    //    /// </summary>
    //    public string CustAddr { get; set; }

    //    /// <summary>
    //    /// 收货人
    //    /// </summary>
    //    public string Receiver { get; set; }

    //    /// <summary>
    //    /// 收货人电话
    //    /// </summary>
    //    public string Telephone { get; set; }

    //    /// <summary>
    //    /// 收货人手机
    //    /// </summary>
    //    public string ReceiverMobile { get; set; }

    //    public string ReceiverDepartment { get; set; }

    //    public string ReceiverCompany { get; set; }
    //    #endregion

    //    #region 其它信息
    //    /// <summary>
    //    /// 付款方式
    //    /// </summary>
    //    public string PayStyle { get; set; }

    //    /// <summary>
    //    /// 发货方式　0 分开发货 1一起发货 2指定发货
    //    /// </summary>
    //    public string DevliveryStyleId { get; set; }

    //    /// <summary>
    //    /// 客户采购单号
    //    /// </summary>
    //    public string CustDefCode { get; set; }

    //    /// <summary>
    //    /// 备注
    //    /// </summary>
    //    public string Memo { get; set; }

    //    /// <summary>
    //    /// 发票类型 2001无需发票  2002普通发票  2003增值税发票
    //    /// </summary>
    //    public string InvType { get; set; }
    //    #endregion

    //    /// <summary>
    //    /// 明细
    //    /// </summary>
    //    public List<InqDetailInfo> InqDetailList { get; set; }

    //    /// <summary>
    //    /// 供应商报价信息
    //    /// </summary>
    //    public List<FntFacInfo> FacList { get; set; }

    //    /// <summary>
    //    /// 是否直接通过 true 是
    //    /// </summary>
    //    public bool IsPass { get; set; }

    //    /// <summary>
    //    /// 客户编码
    //    /// </summary>
    //    public string CustName { get; set; }

    //    /// <summary>
    //    /// 客户名称
    //    /// </summary>
    //    public string CustCode { get; set; }

    //    public decimal Total { get; set; }

    //    public decimal BllTotal { get; set; }

    //    public decimal FeightDiscount { get; set; }

    //    public decimal Taxrate { get; set; }

    //    public decimal Tax { get; set; }

    //    public decimal FeightFee { get; set; }
    //}

    ///// <summary>
    ///// 明细
    ///// </summary>
    //public class InqDetailInfoScm
    //{
    //    public InqDetailInfoScm()
    //    {
    //        this.QtyDiscount = 100;
    //    }
    //    /// <summary>
    //    /// 明细ID
    //    /// </summary>
    //    public long DtlId { get; set; }

    //    /// <summary>
    //    /// 需求清单ID
    //    /// </summary>
    //    public long DemId { get; set; }

    //    /// <summary>
    //    /// 主单ID
    //    /// </summary>
    //    public long InqId { get; set; }

    //    /// <summary>
    //    /// 产品Id
    //    /// </summary>
    //    public long PrdId { get; set; }

    //    /// <summary>
    //    /// 类型Id
    //    /// </summary>
    //    public long TypeId { get; set; }

    //    /// <summary>
    //    /// 产品型号
    //    /// </summary>
    //    public string PrdCode { get; set; }

    //    /// <summary>
    //    /// 产品名称
    //    /// </summary>
    //    public string PrdName { get; set; }

    //    /// <summary>
    //    /// 供货组织
    //    /// </summary>
    //    public string SupplyOrgNumber { get; set; }

    //    /// <summary>
    //    /// 品牌ID
    //    /// </summary>
    //    public long BrandId { get; set; }

    //    public string BrandCode { get; set; }
    //    /// <summary>
    //    /// 订购数量
    //    /// </summary>
    //    public decimal Num { get; set; }

    //    public string ImageUrl { get; set; }

    //    /// <summary>
    //    ///  错误状态
    //    /// 0正常
    //    /// -1 型号错误  -2 超出数量范围
    //    /// </summary>
    //    public int ErrorState { get; set; }

    //    /// <summary>
    //    /// 错误描述
    //    /// </summary>
    //    public string ErrorDesc { get; set; }

    //    /// <summary>
    //    /// 给定的智能提示
    //    /// </summary>
    //    public string ErrorTip { get; set; }

    //    /// <summary>
    //    /// 客户物料
    //    /// </summary>
    //    public string CustItem { get; set; }

    //    /// <summary>
    //    /// 备注
    //    /// </summary>
    //    public string Memo { get; set; }

    //    /// <summary>
    //    /// 不含税金额
    //    /// </summary>
    //    public decimal SubTotal { get; set; }

    //    /// <summary>
    //    /// 含税金额
    //    /// </summary>
    //    public decimal SubTaxTotal { get; set; }

    //    /// <summary>
    //    /// 原价
    //    /// </summary>
    //    public decimal OrgPrice { get; set; }

    //    /// <summary>
    //    /// 不含税单价
    //    /// </summary>
    //    public decimal UnitPrice { get; set; }

    //    /// <summary>
    //    /// 含税单价
    //    /// </summary>
    //    public decimal UnitTaxPrice { get; set; }

    //    /// <summary>
    //    /// 税额
    //    /// </summary>
    //    public decimal Tax { get; set; }

    //    /// <summary>
    //    /// 发货天数
    //    /// </summary>
    //    public decimal DeliveryDays { get; set; }

    //    /// <summary>
    //    /// 询价单状态 0待报价  1报价中    2截止报价   3关闭 4 已报价 5 已订购
    //    /// </summary>
    //    public long InqState { get; set; }

    //    public bool Published { get; set; }

    //    public long CategoryId { get; set; }

    //    public bool ConfirmedPrice { get; set; }

    //    /// <summary>
    //    /// 结算价
    //    /// </summary>
    //    public decimal SupplyUnitPrice { get; set; }

    //    /// <summary>
    //    /// 0: Default;
    //    /// 1: 其他品牌
    //    /// </summary>
    //    public byte CategoryType { get; set; }

    //    /// <summary>
    //    /// 数量折扣
    //    /// </summary>
    //    public decimal QtyDiscount { get; set; }

    //    /// <summary>
    //    /// 产品小类
    //    /// </summary>
    //    public long SmallId { get; set; }

    //    /// <summary>
    //    /// 供应商信息
    //    /// </summary>
    //    public VendorPriceInfo VendorPriceInfo { get; set; }
    //    public PriceListDataType PriceListDataType { get; set; }
    //    /// <summary>
    //    /// 销售价目表数量折扣
    //    /// </summary>
    //    public decimal SalesPriceListQuantityDiscount { get; set; }

    //    public CalculateProductPriceResult PriceCalculateResult { get; set; }

    //    /// <summary>
    //    /// 数据来源
    //    /// 0.型号替换导入
    //    /// 1.供应商价目表
    //    /// 2.资料工程部门导入
    //    /// </summary>
    //    public int DataSource { get; set; }

    //}
    public class CalculateProductPriceResult
    {
        public CalculateProductPriceResult()
        {
            this.DiscountRate = 100;
        }
        public decimal DiscountRate { get; set; }
        public decimal PreferentialUnitPrice { get; set; }

        public decimal QtyDiscount { get; set; }

        public decimal OriginalUnitPrice { get; set; }

     //   public bool IsHistory { get; set; }

        // 是否CRM客户价目表
      //  public bool IsCrmPriceList { get; set; }
        public decimal DeliveryDays { get; set; }

        public PriceSource? PriceSource { get; set; } 

        public DeliverySource? DeliverySource { get; set; }

        /// <summary>
        /// 历史最低报价(最近一年)
        /// </summary>
        public decimal? QuoLPrice { get; set; }
        /// <summary>
        /// 历史最低成交价(最近一年)
        /// </summary>
        public decimal? DealLPrice { get; set; }
    }
    ///// <summary>
    ///// 询价单返回消息
    ///// </summary>
    //public class FntInqResult
    //{
    //    /// <summary>
    //    /// 消息
    //    /// </summary>
    //    public MessageInfo Message { get; set; }

    //    /// <summary>
    //    /// 错误对应信息
    //    /// </summary>
    //    public List<InqDetailInfo> InqDetailList { get; set; }
    //}

    /// <summary>
    /// 询价单信息
    /// </summary>
    public class InquiryMstr
    {
        /// <summary>
        /// 询价key
        /// </summary>
        public long InqId { get; set; }

        /// <summary>
        /// 询价单号
        /// </summary>
        public string? InqCode { get; set; }

        /// <summary>
        /// 询价日期
        /// </summary>
        public DateTime SubmitDate { get; set; }

        /// <summary>
        /// 截止日期
        /// </summary>
        public DateTime ExpirationDate { get; set; }

        /// <summary>
        /// 品牌Id
        /// </summary>
        public long BrandId { get; set; }

        /// <summary>
        /// 品牌名称
        /// </summary>
        public string? BrandName { get; set; }

        /// <summary>
        /// 品牌名称
        /// </summary>
        public string? BrandNameEn { get; set; }

        /// <summary>
        /// 项目数
        /// </summary>
        public long InqItems { get; set; }

        /// <summary>
        /// 收到的报价数
        /// </summary>
        public int InqNum { get; set; }

        /// <summary>
        /// 未读 数量
        /// </summary>
        public int ReadNum { get; set; }

        /// <summary>
        /// 询价单状态 0待报价   2已失效  4 已报价  5 已订购
        /// </summary>
        public long InqState { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 客户id
        /// </summary>
        public long CustID { get; set; }

        /// <summary>
        /// 客户类别
        /// </summary>
        public string? CustType { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string? CustName { get; set; }

        public decimal BllTotal { get; set; }

        /// <summary>
        /// 客户采购单号
        /// </summary>
    //    public string CustDefCode { get; set; }

     //   public List<InqDetailInfo> InqDetailList { get; set; }

        /// <summary>
        /// 供应商报价信息
        /// </summary>
    //    public List<FntFacInfo> FactoryList { get; set; }
    }
    #endregion
}
