using Kingdee.BOS.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.PurchaseManagement
{
    /// <summary>
    /// 获取Pr转Po左侧供应商信息
    /// </summary>
    public class PrToPoLeftSupplierEntity
    {
        public string SupplierCode { get; set; } = "";
        public string SupplierName { get; set; } = "";
        public string ProductSmallClassName { get; set; } = "";
    }

    public class PrToPoAuthoritySetting
    {
        public string SupplierStr { get; set; } = "";
        public string SmallClassStr { get; set; } = "";
        public List<long> SmallClassList { get; set; }
    }

    public class ProductSmallClass
    {
        /// <summary>
        /// 产品小类id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 产品小类父级id
        /// </summary>
        public long ParentId { get; set; }

        /// <summary>
        /// 产品小类父级名称
        /// </summary>
        public string ParentName { get; set; }

        /// <summary>
        /// 产品小类名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 图片url
        /// </summary>
        public string ImageUrl { get; set; }
        /// <summary>
        /// 是否发布
        /// </summary>
        public bool IsPublish { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long CreateUser { get; set; }

        /// <summary>
        /// 创建人姓名
        /// </summary>
        public string CreateUserName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateDate { get; set; }

        /// <summary>
        /// 发布人
        /// </summary>
        public long? PublishUser { get; set; }

        /// <summary>
        /// 发布人姓名
        /// </summary>
        public string PublishUserName { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public string PublishDate { get; set; }

        /// <summary>
        /// 编辑回显上传的地址
        /// </summary>
        public string ImageHttpUrl { get; set; }
    }

    /// <summary>
    /// Pr转Po右侧需求单信息
    /// </summary>
    public class PrToPoPurchaseRequireEntity
    {
        /// <summary>
        /// 选择
        /// </summary>
        public bool Selected { get; set; } = false;

        /// <summary>
        /// 需求单号
        /// </summary>
        public string PrNo { get; set; }

        /// <summary>
        /// 需求单号ID
        /// </summary>
        public long FId { get; set; }

        /// <summary>
        /// 需求单序号
        /// </summary>
        public int PrSeq { get; set; }

        /// <summary>
        /// 需求单序号ID
        /// </summary>
        public long FentryId { get; set; }

        /// <summary>
        /// 销售单号
        /// </summary>
        public string SoNo { get; set; }
        /// <summary>
        /// 销售序号
        /// </summary>
        public int SoSeq { get; set; }
        /// <summary>
        /// 销售下单日期
        /// </summary>
        public string SoActualDate { get; set; }
        /// <summary>
        /// 销售发货日期
        /// </summary>
        public string SoRtd { get; set; }
        /// <summary>
        /// 物料ID
        /// </summary>
        public long ItemID { get; set; }
        /// <summary>
        /// 物料编号
        /// </summary>
        public string ItemNo { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string ItemDesc { get; set; }

        /// <summary>
        /// 产品小类名称
        /// </summary>
        public string SmallClass { get; set; }
        /// <summary>
        /// 产品小类ID
        /// </summary>
        public string SmallClassId { get; set; }

        /// <summary>
        /// 物料单位Id
        /// </summary>
        public int UomId { get; set; }
        /// <summary>
        /// 物料单位名称
        /// </summary>
        public string Uom { get; set; }
        /// <summary>
        /// 物料品牌
        /// </summary>
        public string ItemBrand { get; set; }

        /// <summary>
        /// 需求数量
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// 要采购数量
        /// </summary>
        public decimal PurchaseNum { get; set; }

        /// <summary>
        /// PR-PO 直接获取PR的要求交货日期-1
        /// </summary>
        public string RequiredDeliveryDate { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 供应商ID
        /// </summary>
        public long VendorId { get; set; }
        /// <summary>
        /// 供应商编号
        /// </summary>
        public string VendorCode { get; set; }
        /// <summary>
        /// 供应商名称
        /// </summary>
        public string VendorName { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 最小起订量
        /// </summary>
        public int MinOrderQuantity { get; set; }
        /// <summary>
        /// 下同
        /// </summary>
        public bool Simili { get; set; } = false;
        /// <summary>
        /// 发货天数
        /// </summary>
        public int DeliveryDays { get; set; }
        /// <summary>
        /// 供应商规格型号
        /// </summary>
        public string SupplierProductCode { get; set; }

        /// <summary>
        /// 行情价
        /// </summary>
        public decimal RawMaterialPrice { get; set; } = 0;

        /// <summary>
        /// 加工费
        /// </summary>
        public decimal ProcessFee { get; set; } = 0;

        /// <summary>
        /// 米重
        /// </summary>
        public decimal WeightRate { get; set; } = 0;
        /// <summary>
        /// 价格来源
        /// </summary>
        public string PriceSource { get; set; } = "";

        /// <summary>
        /// 推荐供应商ID
        /// </summary>
        public long SuggestSupplierId { get; set; }

        /// <summary>
        /// 推荐供应商价格
        /// </summary>
        public decimal SuggestSupplierPrice { get; set; }

        /// <summary>
        /// 单据类型ID
        /// </summary>
        public string BillTypeId { get; set; } = "";

        /// <summary>
        /// 单据类型名称
        /// </summary>
        public string BillTypeName { get; set; }

    }

    public class MatchSupplierAndPriceResponse
    {
        public string Code { get; set; }
        public string ErrorMessge { get; set; }
        /// <summary>
        /// 匹配类型(0匹配供应商和价格/1选定供应商)
        /// </summary>
        public int MatchType { get; set; }

        public List<PrToPoPurchaseRequireEntity> Result { get; set; }

        /// <summary>
        /// 非同源比较列表
        /// </summary>
        //public List<NotSameSourceCompare> NotSameSourceCompareList { get; set; }

        /// <summary>
        /// 更低价格供应商比较列表
        /// </summary>
        public List<LowerPriceCompare> LowerPriceCompareList { get; set; }



    }

    public class NotSameSourceCompare
    {
        /// <summary>
        /// 物料编号
        /// </summary>
        public string ItemNo { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string ItemDesc { get; set; }

        public string SameSourceVendorCode { get; set; }
        public string SameSourceVendorName { get; set; }
        public decimal SameSourcePrice { get; set; }
        public int SameSourceMinOrderQuantity { get; set; }

        public string NotSameSourceVendorCode { get; set; }
        public string NotSameSourceVendorName { get; set; }
        public decimal NotSameSourcePrice { get; set; }
        public int NotSameSourceMinOrderQuantity { get; set; }

        /// <summary>
        /// 需求单号
        /// </summary>
        public string PrNo { get; set; }
        /// <summary>
        /// 需求单序号
        /// </summary>
        public int PrSeq { get; set; }
    }

    public class LowerPriceCompare
    {
        /// <summary>
        /// 物料编号
        /// </summary>
        public string ItemNo { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string ItemDesc { get; set; }

        /// <summary>
        /// 选择供应商
        /// </summary>
        public long SelectVendorId { get; set; }
        /// <summary>
        /// 选择供应商
        /// </summary>
        public string SelectVendorCode { get; set; }

        /// <summary>
        /// 选择供应商名称
        /// </summary>
        public string SelectVendorName { get; set; }

        public decimal SelectPrice { get; set; }

        public int SelectMinOrderQuantity { get; set; }

        /// <summary>
        /// 选择价格来源
        /// </summary>
        public string SelectPriceSource { get; set; }
        /// <summary>
        /// 更低价格供应商
        /// </summary>
        public long LowerPriceVendorId { get; set; }

        /// <summary>
        /// 更低价格供应商
        /// </summary>
        public string LowerPriceVendorCode { get; set; }

        /// <summary>
        /// 更低价格供应商名称
        /// </summary>
        public string LowerPriceVendorName { get; set; }
        public decimal LowerPrice { get; set; }
        public int LowerPriceMinOrderQuantity { get; set; }

        /// <summary>
        /// 更低价格来源
        /// </summary>
        public string LowerPriceSource { get; set; }

        /// <summary>
        /// 需求单号
        /// </summary>
        public string PrNo { get; set; }
        /// <summary>
        /// 需求单序号
        /// </summary>
        public int PrSeq { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }
    }

    public class PurchaseRequireModelFilter
    {
        /// <summary>
        /// 产品型号/产品名称
        /// </summary>
        public string Product { get; set; }

        public string SmallClass { get; set; }

        public string CompanyId { get; set; }

    }

    public class CompanyProductPriceResponse<T>
    {
        public CompanyProductPriceResponse()
        {
            this.SuppliserPrice = new List<T>();
        }

        /// <summary>
        /// 产品Id
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// 型号
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// 简易型号
        /// </summary>
        public string ShortNumber { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }

        public List<T> SuppliserPrice { get; set; }
    }

    public class PriceListEntity
    {
        public long VendorId { get; set; }
        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public string ItemNo { get; set; }
        public decimal Price { get; set; }
        public int MinOrderQuantity { get; set; }
        public int QuantityUpperLimit { get; set; }
        public int DeliveryDays { get; set; }

        /// <summary>
        /// 加工费
        /// </summary>
        public decimal ProcessFee { get; set; }

        /// <summary>
        /// 米重
        /// </summary>
        public decimal WeightRate { get; set; }

        /// <summary>
        /// 行情价
        /// </summary>
        public decimal RawMaterialPrice { get; set; }

        /// <summary>
        /// 价格来源
        /// </summary>
        public string PriceSource { get; set; }
    }

    public class MatrixProductPriceResponse
    {
        public string Version { get; set; }

        public long SupplierId { get; set; }

        /// <summary>
        /// 供应商编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 供应商中文名称
        /// </summary>
        public string Name { get; set; }

        public decimal Price { get; set; }
        public int DeliveryDay { get; set; }

        public int Moq { get; set; }
        public int NumberLimit { get; set; }

        /// <summary>
        /// 加工费
        /// </summary>
        public decimal ProcessFee { get; set; }

        /// <summary>
        /// 米重
        /// </summary>
        public decimal WeightRate { get; set; }

        /// <summary>
        /// 行情价
        /// </summary>
        public decimal RawMaterialPrice { get; set; }

    }

    public class PrToPoItemEntity
    {
        /// <summary>
        /// 物料ID
        /// </summary>
        public long ItemID { get; set; }
        /// <summary>
        /// 物料编号
        /// </summary>
        public string ItemNo { get; set; }
        /// <summary>
        /// 简易型号
        /// </summary>
        public string ShortNumber { get; set; }

        /// <summary>
        /// 产品ID
        /// </summary>
        public long ProductId { get; set; }
    }

    public class MatchSupplierPriceRequest
    {
        public string supplierCode { get; set; }
        public List<PrToPoPurchaseRequireEntity> choiceList { get; set; }
    }



    public class PrToPoSupplierEntity
    {
        public long SupplierId { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string ProductSmallClassName { get; set; }
    }

    public class SmallClassModelEntity
    {
        public string SmallClassId { get; set; }
        public string SmallClassName { get; set; }
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
    }

    public class SmallClassRobotMapping
    {
        public long Id { get; set; }
        public string ProductSmallClassId { get; set; }
        public string ProductSmallClassName { get; set; }
        public string RobotCode { get; set; }
        public string RobotName { get; set; }
        public string RobotUrl { get; set; }
        public long CreateUser { get; set; }
        public string CreateUserName { get; set; }
        public DateTime? CreateDate { get; set; }
        public long? UpdateUser { get; set; }
        public string UpdateUserName { get; set; }
        public DateTime? UpdateDate { get; set; }
    }

    /// <summary>
    /// 请求匹配行情价
    /// </summary>
    public class MatchRawMaterialPriceRequest
    {
        /// <summary>
        /// 公司编号
        /// </summary>
        public string CompanyCode { get; set; }
        /// <summary>
        /// 物料编号
        /// </summary>
        public string ProductModel { get; set; }
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }
    }

    /// <summary>
    /// 匹配行情价
    /// </summary>
    public class MatchRawMaterialPriceEntity
    {

        /// <summary>
        /// 供应商编码
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// 供应商中文名称
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// 物料编号
        /// </summary>
        public string ProductModel { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// 加工费
        /// </summary>
        public decimal ProcessFee { get; set; }

        /// <summary>
        /// 米重
        /// </summary>
        public decimal WeightRate { get; set; }

        /// <summary>
        /// 行情价
        /// </summary>
        public decimal RawMaterialPrice { get; set; }
    }
}
