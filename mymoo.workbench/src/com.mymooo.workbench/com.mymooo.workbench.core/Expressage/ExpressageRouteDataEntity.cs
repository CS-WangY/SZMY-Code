using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage
{
    /// <summary>
    /// 路由信息实体类
    /// </summary>
    public class ExpressageRouteDataEntity
    {
        /// <summary>
        /// 路由节点描述 路由节点信息编号，每一个id代表一条不同的路由节点信息，Node路由节点=200 时，为空
        /// </summary>
        public string Desc { get; set; }
        /// <summary>
        /// 退回单号
        /// </summary>
        public string EcWaybillNumber { get; set; }
        /// <summary>
        /// 物流单号
        /// </summary>
        public string Mailno { get; set; }
        /// <summary>
        /// 寄件时间
        /// </summary>
        public string PickupTime { get; set; }
        /// <summary>
        /// 签收时间
        /// </summary>
        public string ReceiveTime { get; set; }
        /// <summary>
        /// 签收人
        /// </summary>
        public string Receiver { get; set; }
        /// <summary>
        /// 是否退货
        /// </summary>
        public string ReturnFlag { get; set; }
        /// <summary>
        /// 路由节点
        /// </summary>
        public int Node { get; set; }
        /// <summary>
        /// 路由步骤
        /// </summary>
        public string Step { get; set; }
        /// <summary>
        /// 路由时间
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// 取货人
        /// </summary>
        public string Packager { get; set; }
        /// <summary>
        /// 取货人联系方式
        /// </summary>
        public string PackagerTel { get; set; }

        /// <summary>
        /// 当前城市
        /// </summary>
        public string CurrCity { get; set; }
    }
}
