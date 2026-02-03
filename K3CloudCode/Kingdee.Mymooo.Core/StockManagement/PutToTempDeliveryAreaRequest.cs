using Kingdee.BOS.Orm.DataEntity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kingdee.Mymooo.Core.StockManagement
{
    /// <summary>
    /// 云仓储出库单参数实体
    /// </summary>
    public class PutToTempDeliveryAreaRequest
    {
        /// <summary>
        /// 出库单号和序号(DN123456-1)
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// 出库单号
        /// </summary>
        public string ExWarehouseOrderNumber { get; set; }

        /// <summary>
        /// 销售单号
        /// </summary>
        public string SalesOrderNumber { get; set; }

        /// <summary>
        /// 客户信息
        /// </summary>
        public DeliverGoodsToCustomerModel Customer { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 客户编码
        /// </summary>
        public string CustomerCoding { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string ModelNumber { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 物料规格
        /// </summary>
        public string Specification { get; set; }

        /// <summary>
        /// 待出库数量
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// 剩余待出库数量
        /// </summary>
        public decimal SurplusQuantity => Quantity;

        /// <summary>
        /// 单位
        /// </summary>
        public NewUnitModel Unit { get; set; }

        /// <summary>
        /// 出库时间
        /// </summary>
        public DateTime ExWarehouseOnUtc { get; set; }

        /// <summary>
        /// 出库类型
        /// </summary>
        public ExternalTypeModel Type { get; set; }

        /// <summary>
        /// 大类
        /// </summary>
        public ParentSmallModel ParentSmall { get; set; }

        /// <summary>
        /// 小类
        /// </summary>
        public SmallModel Small { get; set; }

        /// <summary>
        /// 仓库
        /// </summary>
        public string LocCode { get; set; }
        /// <summary>
        /// 是否直发仓
        /// </summary>
        public bool IsDirectDeliveryStock { get; set; } = false;

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 客户采购单号
        /// </summary>
        public string CustPo { get; set; }

        /// <summary>
        /// 客户料号
        /// </summary>
        public string CustItemNo { get; set; }

        /// <summary>
        /// 客户物料号
        /// </summary>
        public string CustMaterialNo { get; set; }

        /// <summary>
        /// 客户物料名称
        /// </summary>
        public string CustItemName { get; set; }

        /// <summary>
        /// 项目号
        /// </summary>
        public string ProjectNo { get; set; }

        /// <summary>
        /// 送货地址
        /// </summary>
        public string DnAdd { get; set; }

        /// <summary>
        /// 送货单备注
        /// </summary>
        public string DnRemark { get; set; }

        /// <summary>
        /// 销售单备注
        /// </summary>
        public string SoRemark { get; set; }

        /// <summary>
        /// 包装台编号
        /// </summary>
        public string MachineNo { get; set; }

        /// <summary>
        /// 包装台名称
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// 库存管理特征
        /// </summary>
        public string StockFeatures { get; set; }

        //以下伯恩标签使用

        /// <summary>
        /// 条码内容
        /// </summary>
        public string BarCode { get; set; }

        /// <summary>
        /// 客户仓库名
        /// </summary>
        public string Warehouse { get; set; }

        /// <summary>
        /// 采购项次
        /// </summary>
        public string PoItemNumber { get; set; }

        /// <summary>
        /// 物料规格
        /// </summary>
        public string ItemSpec { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        public string BatchNo { get; set; }

        /// <summary>
        /// 辅助重量
        /// </summary>
        public string AuxWeight { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        public string ProductioDate { get; set; }

        /// <summary>
        /// 存储方式
        /// </summary>
        public string StorageMode { get; set; }
        /// <summary>
        /// 包装规格
        /// </summary>
        public decimal PackingSize { get; set; }

        /// <summary>
        /// 标签数(小包)
        /// </summary>
        public int TagNumber { get; set; }

        /// <summary>
        /// 分公司单号
        /// </summary>
        public string RefDnNo { get; set; }

        /// <summary>
        /// 分公司名称
        /// </summary>
        public string CompName { get; set; }

        /// <summary>
        /// 事业部编码
        /// </summary>
        public string BusinessDivisionId { get; set; }

        /// <summary>
        /// 组织编码
        /// </summary>
        public string DeliveryDetOrgCode { get; set; }
        /// <summary>
        /// 事业部
        /// </summary>
        public string BusinessDivisionid { get; set; }

        /// <summary>
        /// 销售组织编码
        /// </summary>
        public string CompCode { get; set; }

        /// <summary>
        /// 组织名称
        /// </summary>
        public string DeliveryDetOrgName { get; set; }
        /// <summary>
        /// 销售订单序号
        /// </summary>
        public int SalesOrderSeq { get; set; }
        /// <summary>
        /// 销售订单数量
        /// </summary>
        public decimal SalesOrderQty { get; set; }

        /// <summary>
        /// 累计发货数量(出库)
        /// </summary>
        public decimal DeliveryTotalQty { get; set; }

        /// <summary>
        /// 累计退货数量
        /// </summary>
        public decimal ReturnsTotalQty { get; set; }

        /// <summary>
        /// 发货通知单表头Id
        /// </summary>
        public long FId { get; set; }

        /// <summary>
        /// 发货通知单明细ID
        /// </summary>
        public long EntryId { get; set; }

        /// <summary>
        /// 地址-蚂蚁
        /// </summary>
        public string MyAddress { get; set; }

        /// <summary>
        /// 传真-蚂蚁
        /// </summary>
        public string MyFax { get; set; }

        /// <summary>
        /// 电话-蚂蚁
        /// </summary>
        public string MyTel { get; set; }

        /// <summary>
        /// 运费
        /// </summary>
        public decimal TrackingCharge { get; set; }

        /// <summary>
        /// 要求交货日期
        /// </summary>
        public DateTime Rtd { get; set; }

        /// <summary>
        /// 仓库对应的仓库发货区域
        /// </summary>
        public string DeliveryplaceCode { get; set; }

        /// <summary>
        /// 是否自动处理
        /// </summary>
        public bool IsAutoHandle { get; set; }

        /// <summary>
        /// 包装详情
        /// </summary>
        public List<PackedSpecEntity> Packdn { get; set; }

        /// <summary>
        /// 制单人
        /// </summary>
        public string ReportMaker { get; set; }
        /// <summary>
        /// 制单人移动电话
        /// </summary>
        public string ReportMakerPhone { get; set; }

        /// <summary>
        /// 含税单价
        /// </summary>
        public string VatPrice { get; set; }
    }

    public class PackedSpecEntity
    {
        /// <summary>
        /// 包装序号
        /// </summary>
        public decimal PackSeq { get; set; }
        /// <summary>
        /// 包装袋子总数
        /// </summary>
        public decimal PackTotal { get; set; }
        /// <summary>
        /// 数量(多少个一包装)
        /// </summary>
        public decimal Qty { get; set; }
    }

    /// <summary>
    /// 出库类型
    /// </summary>
    public class ExternalTypeModel
    {
        public string Value { get; set; }

        public string Description { get; set; }
    }

    /// <summary>
    /// 大类
    /// </summary>
    public class ParentSmallModel
    {
        public string Code { get; set; }

        public string Name { get; set; }
    }


    /// <summary>
    /// 小类
    /// </summary>
    public class SmallModel
    {
        public string Code { get; set; }

        public string Name { get; set; }
    }

    /// <summary>
    /// 单位
    /// </summary>
    public class NewUnitModel
    {
        public long Id { get; set; }

        public string Name { get; set; }
    }

    /// <summary>
    /// 客户信息
    /// </summary>
    public class DeliverGoodsToCustomerModel
    {
        public string Name { get; set; }

        public string Coding { get; set; }

        public string Phone { get; set; }

        public string LinkMan { get; set; }

        public string ReceiveAddress { get; set; }

        /// <summary>
        /// 特殊发货
        /// </summary>
        public bool SpecialDelivery { get; set; } = false;

        /// <summary>
        /// 包装要求
        /// </summary>
        public string PackagingReq { get; set; }
    }
}
