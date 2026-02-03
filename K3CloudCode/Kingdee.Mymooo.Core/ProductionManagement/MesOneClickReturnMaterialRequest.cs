using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    /// <summary>
    /// Mes一键退料
    /// </summary>
    public class MesOneClickReturnMaterialRequest
    {
        /// <summary>
        /// 生产订单ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 生产单据编号
        /// </summary>
        public string BillNo { get; set; }

        /// <summary>
        /// 关闭类型1=执行至结案，2=强制结案
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 执行顺序(1补料退料，2领料退料，3结案)
        /// </summary>
        public int OrderId { get; set; } = 1;

        /// <summary>
        /// 生产订单明细（MoDetails）						
        /// </summary>
        public List<MesMoDetails> MoDetails { get; set; }

        /// <summary>
        /// 生产订单明细（MoDetails）						
        /// </summary>
        public class MesMoDetails
        {
            /// <summary>
            /// 生产订单明细ID
            /// </summary>
            public long EntryId { get; set; }

            /// <summary>
            /// 生产订单明细序号
            /// </summary>
            public int BillSeq { get; set; }

            /// <summary>
            /// 领料信息
            /// </summary>
            public MesMtrlInfo PickMtrlInfo { get; set; }

            /// <summary>
            /// 补料信息
            /// </summary>
            public MesMtrlInfo FeedMtrlInfo { get; set; }
        }
        /// <summary>
        /// 领料/补料的退料单号(PickMtrlInfo/FeedMtrlInfo)
        /// </summary>
        public class MesMtrlInfo
        {
            /// <summary>
            /// 退料单据编号
            /// </summary>
            public string ReturnBillNo { get; set; }
            public List<MesReturnBillNo> SourceDetails { get; set; }
        }
        /// <summary>
        /// 领料/补料单号(ReturnBillNo)						
        /// </summary>
        public class MesReturnBillNo
        {
            /// <summary>
            /// 退料来源单据编号
            /// </summary>
            public string ReturnSourceBillNo { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public List<MesProductionReturnMtrlDet> SubDetails { get; set; }
        }

        /// <summary>
        /// MES生产退料请求明细
        /// </summary>
        public class MesProductionReturnMtrlDet
        {
            /// <summary>
            /// 物料编号
            /// </summary>
            public string MaterialCode { get; set; }

            /// <summary>
            /// 物料名称
            /// </summary>
            public string MaterialName { get; set; }

            /// <summary>
            /// 实退数量
            /// </summary>
            public decimal Qty { get; set; }

            /// <summary>
            /// 退料仓库编码
            /// </summary>
            public string StockCode { get; set; }

            /// <summary>
            /// 退料仓库名称
            /// </summary>
            public string StockName { get; set; }

            /// <summary>
            /// 退料类型
            /// </summary>
            public string ReturnType { get; set; } = "";

            /// <summary>
            /// 退料原因
            /// </summary>
            public string ReturnReason { get; set; } = "";
        }


    }

}
