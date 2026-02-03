using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Core.Report.PlugIn;
using Kingdee.BOS.Core.Report.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.PurchaseManagement.Report;
using Kingdee.Mymooo.Core.Utils;
using Newtonsoft.Json;
using NPOI.SS.Formula.PTG;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Kingdee.Mymooo.Business.PlugIn.SalesManagement.Report
{

    [Description("销售订单跟踪加急收货报表插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class SalesOrderTrackDeliveryBusiness : AbstractSysReportPlugIn
    {
        /// <summary>
        /// 初始化事件
        /// </summary>
        /// <param name="e"></param>
        public override void OnInitialize(InitializeEventArgs e)
        {
            base.OnInitialize(e);
        }
        /// <summary>
        /// 菜单点击事件
        /// </summary>
        /// <param name="e"></param>
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);
            //加急收货检验消息提醒
            //标题：你有收货单需要加急收货：
            //采购单号 + 序号 + 物料号 + 物料名称 + 快递公司 + 快递单号
            //申请人 + 申请时间
            if (e.BarItemKey == "PENY_tbUrgentDeliveryMsg")
            {
                var selectedDataRows = this.SysReportView.SelectedDataRows;
                if (selectedDataRows == null || selectedDataRows.Length == 0)
                {
                    this.View.ShowMessage("未选择数据", MessageBoxType.Error);
                    return;
                }
                List<T_PUR_UrgentDeliveryLog> list = new List<T_PUR_UrgentDeliveryLog>();

                var wxContent = "你有收货单需要加急收货" + "\r\n";
                var orgId = Convert.ToInt64(selectedDataRows[0]["FSupplyTargetOrgId"]);
                foreach (DataRow dr in selectedDataRows)
                {
                    list.Add(new T_PUR_UrgentDeliveryLog
                    {
                        FSoId = Convert.ToInt64(dr["FSoId"]),
                        FSoEntryId = Convert.ToInt64(dr["FSoEntryId"]),
                        FSoNo = Convert.ToString(dr["FSoNo"]),
                        FSoSeq = Convert.ToInt32(dr["FSoSeq"]),
                        FSupplyTargetOrgId = Convert.ToInt64(dr["FSupplyTargetOrgId"]),
                        FQty = Convert.ToDecimal(dr["FQty"]),
                        FMaterialCode = Convert.ToString(dr["FMaterialCode"]),
                        FMaterialName = Convert.ToString(dr["FMaterialName"]),
                        FPoId = Convert.ToInt64(dr["FPoId"]),
                        FPoEntryId = Convert.ToInt64(dr["FPoEntryId"]),
                        FPoNo = Convert.ToString(dr["FPoNo"]),
                        FPoSeq = Convert.ToInt32(dr["FPoSeq"]),
                        FBaseQty = Convert.ToDecimal(dr["FBaseQty"]),
                        FBuyWechatCode = Convert.ToString(dr["FBuyWechatCode"]),
                        FBuyName = Convert.ToString(dr["FBuyName"]),
                        FTrackingCode = Convert.ToString(dr["FTrackingCode"]),
                        FTrackingName = Convert.ToString(dr["FTrackingName"]),
                        FTrackingNumber = Convert.ToString(dr["FTrackingNumber"]),
                        FSupplierDescriptions = Convert.ToString(dr["FSupplierDescriptions"]),
                        FCreateDate = DateTime.Now,
                        FCreateUser = this.Context.UserName,
                    });
                }
                //去重复再发送消息
                foreach (var item in list.GroupBy(g => new { g.FPoNo, g.FPoSeq, g.FMaterialCode, g.FMaterialName, g.FTrackingName, g.FTrackingNumber })
                .Select(s => new
                {
                    FPoNo = s.Key.FPoNo,
                    FPoSeq = s.Key.FPoSeq,
                    FMaterialCode = s.Key.FMaterialCode,
                    FMaterialName = s.Key.FMaterialName,
                    FTrackingName = s.Key.FTrackingName,
                    FTrackingNumber = s.Key.FTrackingNumber
                }).ToList())
                {
                    wxContent += $"采购订单【{item.FPoNo}-{item.FPoSeq}】、物料【{item.FMaterialCode}-{item.FMaterialName}】、快递【{item.FTrackingName}-{item.FTrackingNumber}】" + "\r\n";
                }

                wxContent += $"申请人【{this.Context.UserName}】";
                List<string> userList = new List<string>();
                string sql = $@"select FWECHATCODE from T_WF_Role t1
                                inner join T_WF_ROLEEMP t2 on t1.FID=t2.FID
                                inner join T_HR_EMPINFO t3 on t2.F_EMP_EMP=t3.FID
                                where t1.FNUMBER='SLJJ' and F_EMP_ORG={orgId} ";
                var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sql);
                if (datas != null)
                {
                    foreach (var item in datas)
                    {
                        userList.Add(Convert.ToString(item["FWECHATCODE"]));
                    }
                }
                if (userList.Count() == 0)
                {
                    this.View.ShowMessage($"供货组织没有配置工作流角色通知对应的人。", MessageBoxType.Error);
                    return;
                }

                //更新对应的销售订单
                sql = $@"/*dialect*/update T_SAL_ORDERENTRY set FUrgentDelivery='1' where FENTRYID in ({string.Join(",", list.Select(x => x.FSoEntryId))})";
                DBServiceHelper.Execute(this.Context, sql);
                //更新对应的采购订单
                sql = $@"/*dialect*/update T_PUR_POORDERENTRY set FUrgentDelivery='1' where FENTRYID in ({string.Join(",", list.Select(x => x.FPoEntryId).Distinct())})";
                DBServiceHelper.Execute(this.Context, sql);
                //更新订单的收料通知单
                sql = $@"/*dialect*/update rece set rece.FUrgentDelivery='1' from T_PUR_RECEIVEENTRY rece 
	                inner join T_PUR_Receive rec on rece.FID=rec.FID and rec.FCANCELSTATUS='A'
	                inner join T_PUR_RECEIVEENTRY_S  recs on recs.FENTRYID=rece.FENTRYID
	                inner join T_PUR_RECEIVEENTRY_LK relk on rece.FENTRYID = relk.FENTRYID  and relk.FSTABLENAME = 'T_PUR_POORDERENTRY'
	                inner join T_PUR_POORDERENTRY e on relk.FSID = e.FENTRYID
	                where recs.FINSTOCKQTY=0 and e.FENTRYID in ({string.Join(",", list.Select(x => x.FPoEntryId).Distinct())})";
                DBServiceHelper.Execute(this.Context, sql);
                //更新订单的收料通知单对于的检验单
                sql = $@"/*dialect*/update ie set ie.FUrgentInspection='1' from T_PUR_RECEIVEENTRY rece 
	                inner join T_PUR_Receive rec on rece.FID=rec.FID and rec.FCANCELSTATUS='A'
	                inner join T_PUR_RECEIVEENTRY_S  recs on recs.FENTRYID=rece.FENTRYID
	                inner join T_PUR_RECEIVEENTRY_LK relk on rece.FENTRYID = relk.FENTRYID  and relk.FSTABLENAME = 'T_PUR_POORDERENTRY'
	                inner join T_PUR_POORDERENTRY e on relk.FSID = e.FENTRYID
					inner join T_QM_INSPECTBILLENTRY_LK ilk on rece.FENTRYID = ilk.FSID and ilk.FSTABLENAME = 'T_PUR_RECEIVEENTRY'
				    inner join T_QM_INSPECTBILLENTRY ie on ilk.FENTRYID = ie.FENTRYID
					inner join T_QM_INSPECTBILL ii on ie.FID = ii.FID and ii.FCANCELSTATUS='A' and ii.FDOCUMENTSTATUS<>'C'
	                where recs.FINSTOCKQTY=0 and e.FENTRYID in ({string.Join(",", list.Select(x => x.FPoEntryId).Distinct())})";
                DBServiceHelper.Execute(this.Context, sql);
                //写入日志表
                foreach (var item in list)
                {
                    List<SqlParam> pars = new List<SqlParam>();
                    pars.Add(new SqlParam("@FSoId", KDDbType.Int64, item.FSoId));
                    pars.Add(new SqlParam("@FSoEntryId", KDDbType.Int64, item.FSoEntryId));
                    pars.Add(new SqlParam("@FSupplyTargetOrgId", KDDbType.Int64, item.FSupplyTargetOrgId));
                    pars.Add(new SqlParam("@FQty", KDDbType.Decimal, item.FQty));
                    pars.Add(new SqlParam("@FSoNo", KDDbType.String, item.FSoNo));
                    pars.Add(new SqlParam("@FSoSeq", KDDbType.Int32, item.FSoSeq));
                    pars.Add(new SqlParam("@FMaterialCode", KDDbType.String, item.FMaterialCode));
                    pars.Add(new SqlParam("@FMaterialName", KDDbType.String, item.FMaterialName));
                    pars.Add(new SqlParam("@FPoId", KDDbType.Int64, item.FPoId));
                    pars.Add(new SqlParam("@FPoEntryId", KDDbType.Int64, item.FPoEntryId));
                    pars.Add(new SqlParam("@FPoNo", KDDbType.String, item.FPoNo));
                    pars.Add(new SqlParam("@FPoSeq", KDDbType.Int32, item.FPoSeq));
                    pars.Add(new SqlParam("@FBaseQty", KDDbType.Decimal, item.FBaseQty));
                    pars.Add(new SqlParam("@FBuyName", KDDbType.String, item.FBuyName));
                    pars.Add(new SqlParam("@FTrackingCode", KDDbType.String, item.FTrackingCode));
                    pars.Add(new SqlParam("@FTrackingName", KDDbType.String, item.FTrackingName));
                    pars.Add(new SqlParam("@FTrackingNumber", KDDbType.String, item.FTrackingNumber));
                    pars.Add(new SqlParam("@FSupplierDescriptions", KDDbType.String, item.FSupplierDescriptions));
                    pars.Add(new SqlParam("@FCreateDate", KDDbType.DateTime, item.FCreateDate));
                    pars.Add(new SqlParam("@FCreateUser", KDDbType.String, item.FCreateUser));
                    pars.Add(new SqlParam("@FNotifyer", KDDbType.String, string.Join("|", userList)));
                    pars.Add(new SqlParam("@FRemarks", KDDbType.String, wxContent));
                    sql = @"INSERT INTO T_PUR_UrgentDeliveryLog
                                       (FSoId,FSoEntryId,FSoNo,FSoSeq
                                       ,FSupplyTargetOrgId,FQty
                                       ,FMaterialCode,FMaterialName
                                       ,FPoId,FPoEntryId,FPoNo,FPoSeq,FBaseQty
                                       ,FBuyName,FTrackingCode,FTrackingName
                                       ,FTrackingNumber,FSupplierDescriptions
                                       ,FCreateDate,FCreateUser,FNotifyer,FRemarks)
                                 VALUES
                                       (@FSoId,@FSoEntryId,@FSoNo,@FSoSeq
                                       ,@FSupplyTargetOrgId,@FQty
                                       ,@FMaterialCode,@FMaterialName
                                       ,@FPoId,@FPoEntryId,@FPoNo,@FPoSeq,@FBaseQty
                                       ,@FBuyName,@FTrackingCode,@FTrackingName
                                       ,@FTrackingNumber,@FSupplierDescriptions
                                       ,@FCreateDate,@FCreateUser,@FNotifyer,@FRemarks) ";
                    DBServiceHelper.Execute(this.Context, sql, pars);
                }

                //发送消息
                SendTextMessageUtils.SendTextMessage(string.Join("|", userList), wxContent);
                this.View.ShowMessage($"加急收货提醒成功。");
                return;
            }

        }
    }

}
