using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.SalesManagement
{
    /// <summary>
    /// 发货通知单到MES
    /// </summary>
    public class DeliveryToMesEntity
    {
		/// <summary>
		/// 表单ID
		/// </summary>
		public string FormId { get; set; }
		/// <summary>
		/// 操作类型
		/// </summary>
		public string OperationNumber { get; set; }
		/// <summary>
		/// 订单ID
		/// </summary>
		public long Id { get; set; }
        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNo { get; set; }
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// 审核日期
        /// </summary>
        public DateTime ApproveDate { get; set; }
        /// <summary>
        /// 发货组织编码
        /// </summary>
        public string DeliveryOrgCode { get; set; }
        /// <summary>
        /// 发货组织名称
        /// </summary>
        public string DeliveryOrgName { get; set; }
        /// <summary>
        /// 客户ID
        /// </summary>
        public long CustomerId { get; set; }
        /// <summary>
        /// 客户编码
        /// </summary>
        public string CustomerCode { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// 销售员编码
        /// </summary>
        public string SalesManCode { get; set; }
        /// <summary>
        /// 销售员名称
        /// </summary>
        public string SalesManName { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string PenyNote { get; set; }
        /// <summary>
        /// 收货人姓名
        /// </summary>
        public string LinkMan { get; set; }
        /// <summary>
        /// 收货人姓名
        /// </summary>
        public string LinkPhone { get; set; }
        /// <summary>
        /// 收货方地址
        /// </summary>
        public string ReceiveAddress { get; set; }
        /// <summary>
        /// 明细
        /// </summary>
        public List<DeliveryToMesDetEntity> Details { get; set; }
    }
    /// <summary>
    /// 明细
    /// </summary>
    public class DeliveryToMesDetEntity
    {
        /// <summary>
        /// 明细ID
        /// </summary>
        public long EntryId { get; set; }
        /// <summary>
        /// 明细序号
        /// </summary>
        public int BillSeq { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string MaterialName { get; set; }
        /// <summary>
        /// 客户物料编码
        /// </summary>
        public string CustItemNo { get; set; }
        /// <summary>
        /// 客户物料名称
        /// </summary>
        public string CustItemName { get; set; }
        /// <summary>
        /// 客户ERP物料编码
        /// </summary>
        public string CustMaterialNo { get; set; }
        /// <summary>
        /// 客户采购单号
        /// </summary>
        public string CustPurchaseNo { get; set; }
        /// <summary>
        /// 产品大类ID
        /// </summary>
        public int ParentSmallId { get; set; }
        /// <summary>
        /// 产品大类编码
        /// </summary>
        public string ParentSmallCode { get; set; }
        /// <summary>
        /// 产品大类名称
        /// </summary>
        public string ParentSmallName { get; set; }
        /// <summary>
        /// 产品小类ID
        /// </summary>
        public int SmallId { get; set; }
        /// <summary>
        /// 产品小类编码
        /// </summary>
        public string SmallCode { get; set; }
        /// <summary>
        /// 产品小类名称
        /// </summary>
        public string SmallName { get; set; }
        /// <summary>
        /// 销售数量
        /// </summary>
        public decimal Qty { get; set; }
        /// <summary>
        /// 要货日期
        /// </summary>
        public DateTime DeliveryDate { get; set; }
        /// <summary>
        /// 出货仓库编码
        /// </summary>
        public string StockCode { get; set; }
        /// <summary>
        /// 出货仓库名称
        /// </summary>
        public string StockName { get; set; }
        /// <summary>
        /// 供货组织编码
        /// </summary>
        public string SupplyTargetOrgCode { get; set; }
        /// <summary>
        /// 供货组织名称
        /// </summary>
        public string SupplyTargetOrgName { get; set; }
        /// <summary>
        /// 采购备注
        /// </summary>
        public string InsideRemark { get; set; }
        /// <summary>
        /// 项目号
        /// </summary>
        public string ProjectNo { get; set; }
        /// <summary>
        /// 库存管理特征
        /// </summary>
        public string StockFeatures { get; set; }
        /// <summary>
        /// 库位/工厂
        /// </summary>
        public string LocFactory { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string NoteEntry { get; set; }
        /// <summary>
        /// 销售订单ID
        /// </summary>
        public long SaleOrderId { get; set; }
        /// <summary>
        /// 销售订单单号
        /// </summary>
        public string SaleOrderNo { get; set; }
        /// <summary>
        /// 销售订单明细ID
        /// </summary>
        public long OrderEntryId { get; set; }
        /// <summary>
        /// 销售订单明细序号
        /// </summary>
        public int OrderEntrySeq { get; set; }
    }
}
