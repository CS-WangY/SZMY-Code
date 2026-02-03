using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    /// <summary>
    /// MES生产订单关闭参数
    /// </summary>
    public class MesProductionCloseRequest
    {
        /// <summary>
        /// 组织编码
        /// </summary>
        public string OrgNo { get; set; }
        /// <summary>
        /// 组织名称
        /// </summary>
        public string OrgName { get; set; }
        /// <summary>
        /// 生产单编号
        /// </summary>
        public string MakeNo { get; set; }
        /// <summary>
        /// 是否关闭
        /// </summary>
        public bool IsClose { get; set; } = true;
        /// <summary>
        /// 明细
        /// </summary>
        public List<MesProductionCloseEntity> Entity { get; set; }
    }

    /// <summary>
    /// MES生产订单关闭参数
    /// </summary>
    public class MesProductionCloseEntity
    {
        /// <summary>
        /// 生产单序号
        /// </summary>
        public int MakeSeq { get; set; }
        /// <summary>
        /// 车间编码
        /// </summary>
        public string WorksNo { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string DwgNo { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string DwgVer { get; set; }
        /// <summary>
        /// 生产数量
        /// </summary>
        public decimal Qty { get; set; }
        /// <summary>
        /// 入库数量
        /// </summary>
        public decimal InQty { get; set; }
        /// <summary>
        /// 关闭类型(空是没有关闭)
        //A 自动结案
        //B 手工结案
        //C 强制结案
        //D 拆分结案
        /// </summary>
        public string CloseType { get; set; }
        /// <summary>
        /// 关闭人
        /// </summary>
        public string CloseUserName { get; set; }

        /// <summary>
        /// 业务状态
        //1、计划
        //2、计划确认
        //3、下达
        //4、开工
        //5、完工
        //6、结案
        //7、结算
        /// </summary>
        public int Status { get; set; }
    }
}
