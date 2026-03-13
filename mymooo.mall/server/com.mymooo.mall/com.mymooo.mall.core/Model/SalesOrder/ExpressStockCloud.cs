using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mymooo.core.Attributes.Redis;
using SqlSugar;

namespace com.mymooo.mall.core.Model.SalesOrder
{

    /// <summary>
    /// 云仓存 的物流信息Model
    /// </summary>

    public class ExpressStockCloud
    {

        /// <summary>
        /// 路由节点描述
        /// </summary>
        public string Desc { get; set; } = string.Empty;
        /// <summary>
        /// 退回单号
        /// </summary>
        public string EcWaybillNumber { get; set; } = string.Empty;
        /// <summary>
        /// 运单号
        /// </summary>
        public string Mailno { get; set; } = string.Empty;

        /// <summary>
        /// 路由节点信息编号，每一个id代表一条不同的路由节点信息，Node路由节点=200 时，为空
        /// </summary>
        public int Node { get; set; }
        /// <summary>
        /// 寄件时间
        /// </summary>
        public string PickupTime { get; set; } = string.Empty;
        /// <summary>
        /// 签收时间
        /// </summary>
        public string ReceiveTime { get; set; } = string.Empty;
        /// <summary>
        /// 签收人
        /// </summary>
        public string Receiver { get; set; } = string.Empty;
        /// <summary>
        /// 是否退货 Node 路由节点=100 时，返回值“10-是”
        /// </summary>
        public string ReturnFlag { get; set; } = string.Empty;
        /// <summary>
        /// 路由步骤  注: node 对应数字，step对应文字-1:派送异常6:包车发件0:出车取货7:提货完毕1:取货签到8:交接扫描(派)2:揽件完毕90:中转卸货
       // (派)3:交接扫描(取) 98:准备派送4:中转卸货(取) 99:派送签到5:机场发件100:签收完毕
        /// </summary>
        public string Step { get; set; } = string.Empty;
        /// <summary>
        /// 路由时间
        /// </summary>
        public string Time { get; set; } = string.Empty;
        /// <summary>
        /// 取货人
        /// </summary>
        public string Packager { get; set; } = string.Empty;
        /// <summary>
        /// 取货人联系方式
        /// </summary>
        public string PackagerTel { get; set; } = string.Empty;


    }

    /// <summary>
    /// 原快递100,物流详情每项MODEL
    /// </summary>
    public class Express100
    {
        /// <summary>
        /// 
        /// </summary>
        public string Time { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string FTime { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string Context { get; set; } = string.Empty;

    }

}
