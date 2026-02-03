using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.SalesManagement
{
    /// <summary>
    /// MES下推生成销售出库单
    /// </summary>

    public class MesGenerateOutStockRequest
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 发货通知单号
        /// </summary>
        public string BillNo { get; set; }

        /// <summary>
        /// 拣货完成时间
        /// </summary>
        public DateTime? PickingCompleteDate { get; set; }

        /// <summary>
        /// 包装完成时间
        /// </summary>
        public DateTime? PackagingCompleteDate { get; set; }

        /// <summary>
        /// 物流单号
        /// </summary>
        public string TrackingNumber { get; set; }

        /// <summary>
        /// 物流公司
        /// </summary>
        public string TrackingName { get; set; }

        /// <summary>
        /// 物流时间
        /// </summary>
        public DateTime? TrackingDate { get; set; }

        /// <summary>
        /// 物流下单员
        /// </summary>
        public string TrackingUser { get; set; }

        /// <summary>
        /// 明细
        /// </summary>
        public List<MesGenerateOutStockDetEntity> Details { get; set; }

    }

    /// <summary>
    /// 发货通知单明细
    /// </summary>
    public class MesGenerateOutStockDetEntity
    {
        /// <summary>
        /// 明细ID
        /// </summary>
        public long EntryId { get; set; }

        /// <summary>
        /// 发货通知单序号
        /// </summary>
        public int BillSeq { get; set; }

        /// <summary>
        /// 应发数量
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// 已送货数量
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// 0:正常，1:特结，2:关闭
        /// </summary>
        public int Status { get; set; } = 0;

        /// <summary>
        /// 装箱员
        /// </summary>
        public string PackagingUser { get; set; }

        /// <summary>
        /// 装箱时间
        /// </summary>
        public DateTime? PackagingDate { get; set; }

        /// <summary>
        /// 拣货员
        /// </summary>
        public string PickingUser { get; set; }

        /// <summary>
        /// 拣货时间
        /// </summary>
        public DateTime? PickingDate { get; set; }

    }
}
