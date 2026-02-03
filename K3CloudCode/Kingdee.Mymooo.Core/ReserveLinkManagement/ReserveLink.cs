using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ReserveLinkManagement
{
    public class ReserveLink
    {
        /// <summary>
        /// 需求来源单号
        /// </summary>
        public string FDEMANDBILLNO { get; set; }
        /// <summary>
        /// 顶级需求来源单号
        /// </summary>
        public string FSRCBILLNO { get; set; }
        /// <summary>
        /// 父项需求来源单号
        /// </summary>
        public string FPARENTBILLNO { get; set; }
        /// <summary>
        /// 创建者标识
        /// </summary>
        public string FGenerateId0 { get; set; }
        /// <summary>
        /// 基本需求单位
        /// </summary>
        public long FBASEDEMANDUNITID { get; set; }
        /// <summary>
        /// 顶级需求来源类型
        /// </summary>
        public string FSRCFORMID { get; set; }
        /// <summary>
        /// 预留类型
        /// </summary>
        public int FRESERVETYPE { get; set; }
        /// <summary>
        /// 基本单位需求数量
        /// </summary>
        public decimal FBASEDEMANDQTY { get; set; }
        /// <summary>
        /// 变动损耗率
        /// </summary>
        public decimal FScrapRate { get; set; }
        /// <summary>
        /// 用量类型
        /// </summary>
        public int FDosageType { get; set; } = 2;
        /// <summary>
        /// 货源比例
        /// </summary>
        public decimal FSupRate { get; set; } = 100;
        /// <summary>
        /// 基本单位分子
        /// </summary>
        public decimal FBASENUMERATOR { get; set; }
        /// <summary>
        /// 基本单位分母
        /// </summary>
        public decimal FBASEDENOMINATOR { get; set; }
        /// <summary>
        /// 基本单位固定损耗
        /// </summary>
        public decimal FBaseFixScrapQty { get; set; }
        /// <summary>
        /// 顶级需求来源分录号
        /// </summary>
        public string FSRCENTRYID { get; set; }
        /// <summary>
        /// 父项需求来源分录号
        /// </summary>
        public string FPARENTENTRYID { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public long FMATERIALID { get; set; }
        /// <summary>
        /// 需求日期
        /// </summary>
        public DateTime FDEMANDDATE { get; set; }
        /// <summary>
        /// 需求组织
        /// </summary>
        public long FDEMANDORGID { get; set; }
        /// <summary>
        /// 需求来源类型
        /// </summary>
        public string FDEMANDFORMID { get; set; }
        /// <summary>
        /// 父项需求来源单内码
        /// </summary>
        public string FPARENTINTERID { get; set; }
        /// <summary>
        /// 需求单分录号
        /// </summary>
        public string FDemandENTRYID { get; set; }
        /// <summary>
        /// 需求来源单内码
        /// </summary>
        public string FDEMANDINTERID { get; set; }
        /// <summary>
        /// 顶级需求来源单内码
        /// </summary>
        public string FSRCINTERID { get; set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public int FPRIORITY { get; set; }
        /// <summary>
        /// 父项需求来源类型
        /// </summary>
        public string FPARENTFORMID { get; set; }
        /// <summary>
        /// BOM分录内码
        /// </summary>
        public long FBOMENTRYID { get; set; }
        /// <summary>
        /// 根需求单据类型优先级
        /// </summary>
        public long FSrcFomIdPriority { get; set; }
        /// <summary>
        /// 需求来源内码(整形)
        /// </summary>
        public long FINTDEMANDID { get; set; } = 0;
        /// <summary>
        /// 父项需求来源内码(整形)
        /// </summary>
        public long FINTPARENTID { get; set; } = 0;
        /// <summary>
        /// 顶级需求来源内码(整形)
        /// </summary>
        public long FINTSRCID { get; set; } = 0;
        /// <summary>
        /// 需求来源分录内码(整形)
        /// </summary>
        public long FINTDEMANDENTRYID { get; set; } = 0;
        /// <summary>
        /// 父项需求来源分录内码(整形)
        /// </summary>
        public long FINTPARENTENTRYID { get; set; } = 0;
        /// <summary>
        /// 顶级需求来源分录内码(整形)
        /// </summary>
        public long FINTSRCENTRYID { get; set; } = 0;
        /// <summary>
        /// 备注
        /// </summary>
        public string FRemarks { get; set; }
        public List<ReserveLinkEntry> Entry { get; set; }
    }

    public class ReserveLinkEntry
    {
        /// <summary>
        /// 供给类型
        /// </summary>
        public string FSUPPLYFORMID { get; set; }
        /// <summary>
        /// 供给单据内码
        /// </summary>
        public string FSUPPLYINTERID { get; set; }
        /// <summary>
        /// 供给单据号
        /// </summary>
        public string FSUPPLYBILLNO { get; set; }
        /// <summary>
        /// 供给单分录内码
        /// </summary>
        public string FSUPPLYENTRYID { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public long FMATERIALID { get; set; }
        /// <summary>
        /// 供应组织
        /// </summary>
        public long FSUPPLYORGID { get; set; }
        /// <summary>
        /// 供给日期
        /// </summary>
        public DateTime FSUPPLYDATE { get; set; }
        /// <summary>
        /// 仓库
        /// </summary>
        public long FSUPPLYSTOCKID { get; set; }
        /// <summary>
        /// 仓位
        /// </summary>
        public long FSUPPLYSTOCKLOCID { get; set; }
        /// <summary>
        /// BOM版本
        /// </summary>
        public long FSUPPLYBOMID { get; set; }
        /// <summary>
        /// 批号
        /// </summary>
        public int FSUPPLYLOT { get; set; } = 0;
        /// <summary>
        /// 基本计量单位
        /// </summary>
        public long FBASESUPPLYUNITID { get; set; }
        /// <summary>
        /// 库存单据类型
        /// </summary>
        public string FSTOCKFORMID { get; set; }
        /// <summary>
        /// 基本单位申请数量
        /// </summary>
        public decimal FBASEQTY { get; set; }
        /// <summary>
        /// 库存单据内码
        /// </summary>
        public long FSTOCKINTERID { get; set; }
        /// <summary>
        /// 库存单据分录内码
        /// </summary>
        public long FSTOCKENTRYID { get; set; }
        /// <summary>
        /// 供应优先级
        /// </summary>
        public int FSUPPLYPRIORITY { get; set; }
        /// <summary>
        /// 是否MTO
        /// </summary>
        public int FISMTO { get; set; } = 0;
        /// <summary>
        /// 成品率
        /// </summary>
        public decimal FYieldRate { get; set; }
        /// <summary>
        /// 预留产生类型
        /// </summary>
        public int FLINKTYPE { get; set; } = 0;
        /// <summary>
        /// 内码
        /// </summary>
        public string FEntryPkId { get; set; }
        /// <summary>
        /// 消耗优先级
        /// </summary>
        public int FCONSUMPRIORITY { get; set; } = 0;
        /// <summary>
        /// 库存辅单位
        /// </summary>
        public long FSecUnitId { get; set; } = 0;
        /// <summary>
        /// 标识号
        /// </summary>
        public string FGenerateId { get; set; }
        /// <summary>
        /// 库存量(辅单位)
        /// </summary>
        public decimal FSecQty { get; set; } = 0;
        /// <summary>
        /// 供给单内码(整形)
        /// </summary>
        public long FINTSUPPLYID { get; set; } = 0;
        /// <summary>
        /// 供给单分录内码(整形)
        /// </summary>
        public long FINTSUPPLYENTRYID { get; set; } = 0;
        /// <summary>
        /// 锁库日期
        /// </summary>
        public DateTime? FReserveDate { get; set; }
        /// <summary>
        /// 锁库天数
        /// </summary>
        public int FReserveDays { get; set; }
        /// <summary>
        /// 预计解锁日期
        /// </summary>
        public DateTime? FReleaseDate { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string FSupplyRemarks { get; set; }
    }
}
