using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    public class MesPrd_PPBOMRequest
    {
        /// <summary>
        /// 生产订单Id
        /// </summary>
        public long MoId { get; set; }

        /// <summary>
        /// 生产订单编号
        /// </summary>
        public string MoBillNo { get; set; }

        public List<PPBomEntry> Entry { get; set; }

    }
    public class PPBomEntry
    {
        /// <summary>
        /// 生产订单行Id
        /// </summary>
        public long MoEntryId { get; set; }
        /// <summary>
        /// 生产订单行号
        /// </summary>
        public int MoSeq { get; set; }
        /// <summary>
        /// 分子
        /// </summary>
        public decimal Numerator { get; set; }
        /// <summary>
        /// 分母
        /// </summary>
        public decimal Denominator { get; set; }
        /// <summary>
        /// 固定损耗
        /// </summary>
        public decimal FFixScrapQty { get; set; }
        /// <summary>
        /// 变动损耗
        /// </summary>
        public decimal ScrapRate { get; set; }
    }
}
