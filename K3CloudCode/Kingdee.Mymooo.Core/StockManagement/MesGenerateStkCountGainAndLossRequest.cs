using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.StockManagement
{
    /// <summary>
    /// MES生成盘盈单
    /// </summary>
    public class MesGenerateStkCountGainAndLossRequest
    {
        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNo { get; set; }

        /// <summary>
        /// 库存组织Id
        /// </summary>
        public long StockOrgId { get; set; }

        /// <summary>
        /// 库存组织编码
        /// </summary>
        public string StockOrgCode { get; set; }

        /// <summary>
        /// 库存组织名称
        /// </summary>
        public string StockOrgName { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string NoteHead { get; set; }

        /// <summary>
        /// 明细
        /// </summary>
        public List<MesGenerateStkCountGainAndLossDetEntity> Details { get; set; }
    }


    /// <summary>
    /// 明细
    /// </summary>
    public class MesGenerateStkCountGainAndLossDetEntity
    {
        /// <summary>
        /// 物料ID
        /// </summary>
        public long MaterialId { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 账面数量
        /// </summary>
        public decimal AcctQty { get; set; }

        /// <summary>
        /// 盘点数量
        /// </summary>
        public decimal CountQty { get; set; }

        /// <summary>
        /// 仓库编码
        /// </summary>
        public string StockCode { get; set; }

        /// <summary>
        /// 仓库名称
        /// </summary>
        public string StockName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Note { get; set; }

    }
}
