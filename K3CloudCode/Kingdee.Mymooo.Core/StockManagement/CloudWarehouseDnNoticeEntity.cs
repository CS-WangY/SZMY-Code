using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.StockManagement
{
    /// <summary>
    /// 云仓储发货通知单回传金蝶
    /// </summary>
    public class CloudWarehouseDnNoticeEntity
    {
        /// <summary>
        /// FID
        /// </summary>
        public long FId { get; set; }

        /// <summary>
        /// 发货通知单号
        /// </summary>
        public string BillNo { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }

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
        public List<DeliveryNoticeDetEntity> Det { get; set; }

        /// <summary>
        /// 随机码，用于记录和重新执行
        /// </summary>
        public string Giud { get; set; }

        /// <summary>
        /// 创建人的微信Code
        /// </summary>
        public string CreateUserWxCode { get; set; }

        /// <summary>
        /// 业务员的微信Code
        /// </summary>
        public string SalUserWxCode { get; set; }

        /// <summary>
        /// 是否已经结束
        /// </summary>
        public bool IsEnd { get; set; } = false;

        /// <summary>
        /// 创建人ID
        /// </summary>
        public long CreatorId { get; set; }

        /// <summary>
        /// 是否本次作废，本次作废的，需要更新预留
        /// </summary>
        public bool IsThisCancel { get; set; } = false;
        /// <summary>
        /// 分步式调拨ID组
        /// </summary>
        public List<long> DeliveryTransfers { get; set; } = new List<long>();

        /// <summary>
        /// 云存储回调时间
        /// </summary>
        public DateTime? CallbackDate { get; set; }
    }

    /// <summary>
    /// 发货通知单明细
    /// </summary>
    public class DeliveryNoticeDetEntity
    {
        /// <summary>
        /// 明细ID
        /// </summary>
        public long EntryId { get; set; }

        /// <summary>
        /// 发货通知单序号
        /// </summary>
        public int Seq { get; set; }

        /// <summary>
        /// 物料ID
        /// </summary>
        public long MaterialId { get; set; }
        /// <summary>
        /// 物料的mster码
        /// </summary>
        public long MsterID { get; set; }

        /// <summary>
        /// 物料编号
        /// </summary>
        public string ItemNo { get; set; }

        /// <summary>
        /// 应发数量
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// 已送货数量
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// 发货通知销售单位
        /// </summary>
        public long SalUnitID { get; set; }

        /// <summary>
        /// 可特结数量，用于调拨单入数量更改问题
        /// </summary>
        public decimal TjQty { get; set; }

        /// <summary>
        /// 0:正常，1:特结，2:关闭
        /// </summary>
        public int Status { get; set; }

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

        /// <summary>
        /// 供货商组织ID
        /// </summary>
        public long SupplyOrgId { get; set; }

        /// <summary>
        /// 出货仓库ID
        /// </summary>
        public long StockId { get; set; }

        /// <summary>
        /// 销售订单ID
        /// </summary>
        public string SBillId { get; set; }

        /// <summary>
        /// 销售订单明细ID
        /// </summary>
        public string SID { get; set; }

        /// <summary>
        /// 特结原因
        /// </summary>
        public string SpecialReason { get; set; }

        /// <summary>
        /// 销售订单号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 销售订单行号
        /// </summary>
        public string OrderSeq { get; set; }

        /// <summary>
        /// 供货组织是否云仓储回调下架
        /// </summary>
        public bool IsCloudStockCallBack { get; set; } = false;

    }

    //特结或者关闭数量，根据物料和供货组织汇总
    public class SupplyItemSumQty
    {
        /// <summary>
        /// 物料号
        /// </summary>
        public string ItemNo { get; set; }
        /// <summary>
        /// 仓库ID
        /// </summary>
        public long StockId { get; set; }
        /// <summary>
        /// 供货组织ID
        /// </summary>
        public long SupplyOrgId { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }

    }


    //特结或者关闭数量，红字调回明细行
    public class SupplyItemDet
    {
        /// <summary>
        /// 物料号
        /// </summary>
        public string ItemNo { get; set; }

        /// <summary>
        /// 供货组织ID
        /// </summary>
        public long SupplyOrgId { get; set; }

        /// <summary>
        /// 特结数量
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// 发货通知单序号
        /// </summary>
        public int Seq { get; set; }

    }

    /// <summary>
    /// 红字调回的消息集合
    /// </summary>
    public class RedDeliveryTransferInMsg
    {
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Msg { get; set; }
        /// <summary>
        /// 接收消息的人
        /// </summary>
        public string UserList { get; set; }

    }
}
