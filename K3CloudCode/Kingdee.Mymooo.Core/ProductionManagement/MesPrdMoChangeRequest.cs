using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    public class PrdMoChangeRequest
    {
        /// <summary>
        /// 生产订单Id
        /// </summary>
        public long MoId { get; set; }
        /// <summary>
        /// 生产订单行Id
        /// </summary>
        public long MoEntryId { get; set; }
        /// <summary>
        /// 生产订单编号
        /// </summary>
        public string MoBillNo { get; set; }
        /// <summary>
        /// 生产订单行号
        /// </summary>
        public int MoSeq { get; set; }
        /// <summary>
        /// 变更后数量
        /// </summary>
        public decimal ChangeQty { get; set; }
        /// <summary>
        /// 计划完工时间
        /// </summary>
        public DateTime PlanFinishDate { get; set; }
    }
    public class PrdMoChangeWorkShopRequest
    {
        /// <summary>
        /// 生产订单Id
        /// </summary>
        public long MoId { get; set; }
        /// <summary>
        /// 生产订单行Id
        /// </summary>
        public long MoEntryId { get; set; }
        /// <summary>
        /// 生产订单编号
        /// </summary>
        public string MoBillNo { get; set; }
        /// <summary>
        /// 生产订单行号
        /// </summary>
        public int MoSeq { get; set; }
        /// <summary>
        /// 车间编码
        /// </summary>
        public string WorkShopNumber { get; set; }
    }
}
