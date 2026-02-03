using Kingdee.BOS.Core.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.DirectSaleManagement
{
    /// <summary>
    /// 根据销售订单明细获取采购订单和直发数量
    /// </summary>
    public class reqSoPoDirQtyFilter
    {
        /// <summary>
        /// 销售单号
        /// </summary>
        public string SoNo { get; set; }

        /// <summary>
        ///平台销售单明细ID 1001|1002|1003(多笔明细，按|符号隔开)
        /// </summary>
        public string FbdDetIdList { get; set; }
    }

    /// <summary>
    /// 据销售订单表头返回参数
    /// </summary>
    public class SoDirEntity
    {
        /// <summary>
        /// 销售单号Fid
        /// </summary>
        public long SoFId { get; set; }

        /// <summary>
        /// 销售单号
        /// </summary>
        public string SoNo { get; set; }

        /// <summary>
        /// 快递单号
        /// </summary>
        public string TranCode { get; set; } = "";

        /// <summary>
        /// 直发单号
        /// </summary>
        public string DirNo { get; set; } = "";

        /// <summary>
        /// 是否担保直发
        /// </summary>
        public bool IsWarrant { get; set; } = false;

        /// <summary>
        /// 是否新增预留数量
        /// </summary>
        public bool IsAddReServeQty { get; set; } = false;

        /// <summary>
        /// 销售订单明细
        /// </summary>
        public List<SoDetDirEntity> SoDet { get; set; }

        /// <summary>
        /// 收料通知单ID组
        /// </summary>
        public List<long> ReceiveMaterials { get; set; } = new List<long>();

        /// <summary>
        /// 发货通知单信息
        /// </summary>
        public DirDnEntity dnEntity { get; set; }

        /// <summary>
        /// 直发仓库ID（销售组织外发仓ID）
        /// </summary>
        public long DirStockId { get; set; }

        /// <summary>
        /// 收货方地址
        /// </summary>
        public string ReceiveAddress { get; set; }

        /// <summary>
        /// 收货方联系人
        /// </summary>
        public string LinkMan { get; set; }

        /// <summary>
        /// 收货方联系电话
        /// </summary>
        public string LinkPhone { get; set; }

        /// <summary>
        /// 创建用户名称
        /// </summary>
        public string CreateUserName { get; set; }

        /// <summary>
        /// 创建用户编码
        /// </summary>
        public string CreateUserCode { get; set; }

        /// <summary>
        /// 预计回款时间
        /// </summary>
        public DateTime? ExpectedPaymentDate { get; set; } = null;

    }

    /// <summary>
    /// 据销售订单明细返回参数
    /// </summary>
    public class SoDetDirEntity
    {
        /// <summary>
        /// 销售明细ID
        /// </summary>
        public long SoEntryId { get; set; }

        /// <summary>
        /// 销售单序号
        /// </summary>
        public int SoSeqNo { get; set; }

        /// <summary>
        /// 平台销售单明细ID
        /// </summary>
        public long FbdDetId { get; set; }

        /// <summary>
        /// 销售单物料号
        /// </summary>
        public string ItemNo { get; set; }

        /// <summary>
        /// 供货组织Id
        /// </summary>
        public long SupplyOrgId { get; set; }

        /// <summary>
        /// 直发仓库ID
        /// </summary>
        public long DirStockId { get; set; }

        /// <summary>
        /// 销售订单可出数量
        /// </summary>
        public decimal FBaseCanOutQty { get; set; }

        /// <summary>
        /// 采购明细
        /// </summary>
        public List<PoDirQtyDet> PoDet { get; set; }

    }

    /// <summary>
    /// 据销售订单明细获取采购订单和直发数量请求返回参数
    /// </summary>
    public class PoDirQtyDet
    {
        /// <summary>
        /// 采购单号Fid
        /// </summary>
        public long PoFId { get; set; }

        /// <summary>
        /// 采购明细FEntryId
        /// </summary>
        public long PoEntryId { get; set; }

        /// <summary>
        /// 采购单号
        /// </summary>
        public string PoNo { get; set; }

        /// <summary>
        /// 采购单序号
        /// </summary>
        public int PoSeqNo { get; set; }

        /// <summary>
        /// 可直发数量
        /// </summary>
        public decimal DirQty { get; set; }

        /// <summary>
        /// 需要直发数量
        /// </summary>
        public decimal ReqDirQty { get; set; }

        /// <summary>
        /// 采购组织Id
        /// </summary>
        public long PoOrgID { get; set; }

        /// <summary>
        /// 直发仓库ID
        /// </summary>
        public long DirStockId { get; set; }

    }


    /// <summary>
    /// 获取直发的采购订单数据
    /// </summary>
    public class GetDirPoEntity
    {
        /// <summary>
        /// 采购单号Fid
        /// </summary>
        public long PoFID { get; set; }

        /// <summary>
        /// 采购单号
        /// </summary>
        public string PoNo { get; set; }

        /// <summary>
        /// 采购明细ID
        /// </summary>
        public long PoEntryId { get; set; }

        /// <summary>
        /// 采购单序号
        /// </summary>
        public int PoSeqNo { get; set; }

        /// <summary>
        /// 采购订单剩余未入库数量汇总
        /// </summary>
        public decimal PoSumSyQty { get; set; }


    }

    /// <summary>
    /// 发货通知单
    /// </summary>
    public class DirDnEntity
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
        /// 明细
        /// </summary>
        public List<DirDnDetEntity> Det { get; set; }

    }

    /// <summary>
    /// 发货通知单明细
    /// </summary>
    public class DirDnDetEntity
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
        /// 发货通知销售单位
        /// </summary>
        public long SalUnitID { get; set; }


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
        /// 销售订单号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 销售订单行号
        /// </summary>
        public string OrderSeq { get; set; }

    }


    public class ReplaceVal
    {
        public EntityType EntityType { get; set; }
        public string EntityKey { get; set; }
        public string EntityValue { get; set; }
        public TargetValueType valueType { get; set; }
        public object Val { get; set; }
    }
    public enum EntityType { BillHand, BillEntry }
    public enum TargetValueType { Value, Object }

}
