using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Core.Report.PlugIn;
using Kingdee.BOS.Core.Report.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Core.PurchaseManagement.Report;
using Kingdee.Mymooo.Core.Utils;
using Newtonsoft.Json;
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

    [Description("销售订单跟踪查询报表插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class SalesOrderTrackQueryBusiness : AbstractSysReportPlugIn
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
        /// 数据绑定完成后
        /// </summary>
        /// <param name="e"></param>
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            var grid = this.View.GetControl<EntryGrid>("FList");
            grid.SetFrozen("FSalesOrderDate".ToUpperInvariant(), "");
            grid.SetFrozen("FSalesOrderNo".ToUpperInvariant(), "");
            grid.SetFrozen("FSEQ".ToUpperInvariant(), "");
            grid.SetFrozen("FCustomerName".ToUpperInvariant(), "");
            grid.SetFrozen("FSALERNAME".ToUpperInvariant(), "");
            grid.SetFrozen("FMATERIALCode".ToUpperInvariant(), "");
            grid.SetFrozen("FMATERIALName".ToUpperInvariant(), "");
            grid.SetFrozen("FCustItemNo".ToUpperInvariant(), "");
            grid.SetFrozen("FCustItemName".ToUpperInvariant(), "");
            grid.SetFrozen("FCUSTMATERIALNO".ToUpperInvariant(), "");
            grid.SetFrozen("FQTY".ToUpperInvariant(), "");
            grid.SetFrozen("FSTOCKOUTQTY".ToUpperInvariant(), "");

        }


        //双击
        //单元格,双击事件
        public override void CellDbClick(CellEventArgs Args)
        {
            base.CellDbClick(Args);
            if (Args.Header.FieldName == "FSalesOrderNo")//销售单号，查询销售订单跟踪销退明细报表
            {
                string fId = Convert.ToString(((ISysReportViewService)this.View).GetCurrentRowValue("FID"));
                string fEntryId = Convert.ToString(((ISysReportViewService)this.View).GetCurrentRowValue("FXSENTRYID"));
                if (!string.IsNullOrWhiteSpace(fId) && !string.IsNullOrWhiteSpace(fEntryId))
                {
                    SysReportShowParameter para = new SysReportShowParameter();
                    para.OpenStyle.ShowType = ShowType.Modal;
                    //唯一标识
                    para.FormId = "PENY_SAL_OrderTrackToSRRpt";
                    para.IsShowFilter = false;
                    para.CustomParams["FId"] = fId;
                    para.CustomParams["FEntryId"] = fEntryId;
                    this.View.ShowForm(para);
                }
            }
            else if (Args.Header.FieldName == "FCGBILLNO")//采购单号，查询销售订单跟踪出入库明细报表
            {
                string fEntryId = Convert.ToString(((ISysReportViewService)this.View).GetCurrentRowValue("FCGENTRYID"));
                if (!string.IsNullOrWhiteSpace(fEntryId))
                {
                    SysReportShowParameter para = new SysReportShowParameter();
                    para.OpenStyle.ShowType = ShowType.Modal;
                    //唯一标识
                    para.FormId = "PENY_SAL_OrderTrackToIORpt";
                    para.IsShowFilter = false;
                    para.CustomParams["FEntryId"] = fEntryId;
                    this.View.ShowForm(para);
                }
            }
            else if (Args.Header.FieldName == "FSUPPLIERREPLYDATE")//供应商回复发货日期，查询销售订单跟踪供应商回复报表
            {
                string fEntryId = Convert.ToString(((ISysReportViewService)this.View).GetCurrentRowValue("FCGENTRYID"));
                if (!string.IsNullOrWhiteSpace(fEntryId))
                {
                    SysReportShowParameter para = new SysReportShowParameter();
                    para.OpenStyle.ShowType = ShowType.Modal;
                    //唯一标识
                    para.FormId = "PENY_SAL_OrderTrackToSuppReplyRpt";
                    para.IsShowFilter = false;
                    para.CustomParams["FEntryId"] = fEntryId;
                    this.View.ShowForm(para);
                }
            }
            else if (Args.Header.FieldName == "FUrgentDelivery")//是否加急收货，查询加急收货明细报表
            {
                string fId = Convert.ToString(((ISysReportViewService)this.View).GetCurrentRowValue("FID"));
                string fEntryId = Convert.ToString(((ISysReportViewService)this.View).GetCurrentRowValue("FXSENTRYID"));
                if (!string.IsNullOrWhiteSpace(fId) && !string.IsNullOrWhiteSpace(fEntryId))
                {
                    SysReportShowParameter para = new SysReportShowParameter();
                    para.OpenStyle.ShowType = ShowType.Modal;
                    //唯一标识
                    para.FormId = "PENY_PUR_UrgentDeliveryLog";
                    para.IsShowFilter = false;
                    para.CustomParams["FEntryId"] = fEntryId;
                    this.View.ShowForm(para);
                }
            }
        }


        public override void EntityRowDoubleClick(BOS.Core.DynamicForm.PlugIn.Args.EntityRowClickEventArgs e)
        {
            base.EntityRowDoubleClick(e);
        }

        //设置行的颜色 OnFormatRowConditions
        public override void OnFormatRowConditions(ReportFormatConditionArgs args)
        {
            base.OnFormatRowConditions(args);

            if (Convert.ToInt32(args.DataRow["IsExpiry"]) == 1)
            {
                FormatCondition FRow_FC = new FormatCondition()
                {   //红色
                    BackColor = "red",
                    Key = "FDELIVERYDATE"
                };
                //加载颜色FRow_FC
                args.FormatConditions.Add(FRow_FC);
            }
        }

        /// <summary>
        /// 菜单点击事件
        /// </summary>
        /// <param name="e"></param>
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);
            //加急收货检验
            //判断规则：
            //1、不同供货组织，不能一起申请。
            //2、销售订单行未关闭
            //3、判断是否存在采购订单预留
            //4、采购订单的快递公司是否存在，（货拉拉和其他的不需要判断快递单号，另外的需要调用快递100查询是否已经签收）
            //4.1、如果存在正常的快递公司，未签收，提示快递未到货，不能申请加急。
            if (e.BarItemKey == "PENY_tbUrgentDelivery")
            {
                var selectedDataRows = this.SysReportView.SelectedDataRows;
                if (selectedDataRows == null || selectedDataRows.Length == 0)
                {
                    this.View.ShowMessage("未选择数据", MessageBoxType.Error);
                    return;
                }
                List<long> longs = new List<long>();
                foreach (var selectedRow in selectedDataRows)
                {
                    longs.Add(Convert.ToInt64(selectedRow["FXSENTRYID"]));
                }
                var errMsg = "";
                List<T_PUR_UrgentDeliveryLog> list = new List<T_PUR_UrgentDeliveryLog>();
                var sql = $@"/*dialect*/select TE.FID as FSoId,TE.FENTRYID as FSoEntryId
                 ,T.FBILLNO FSoNo,TE.FSEQ FSoSeq
                 ,TE.FSupplyTargetOrgId
                 ,TE.FQTY FQty --销售数量
                 ,BDM.FNUMBER FMaterialCode,BDML.FNAME FMaterialName --物料
                ,case when T.FCLOSESTATUS='B' then '已关闭' else (case when TE.FMrpCloseStatus='B' then '已关闭' else '未关闭' end) end FSoStatus --销售单行状态(已关闭和未关闭)
                 ,po.FID FPoId
                 ,TranmitStock.FSUPPLYENTRYID FPoEntryId --采购订单明细ID
                 ,po.FBILLNO FPoNo
                 ,poe.FSEQ FPoSeq
                 ,buy.FWECHATCODE FBuyWechatCode
                 ,asse.FNUMBER FTrackingCode,assel.FDATAVALUE FTrackingName,poe.FTrackingNumber,poe.FSupplierDescriptions
                 from T_SAL_ORDERENTRY TE
                 inner join T_SAL_ORDERENTRY_R TER on TE.FENTRYID=TER.FENTRYID
                 inner join T_SAL_ORDER T on TE.FID=T.FID
                 left join T_BD_MATERIAL BDM on TE.FMATERIALID=BDM.FMATERIALID
                 left join T_BD_MATERIAL_L BDML on TE.FMATERIALID=BDML.FMATERIALID and BDML.FLOCALEID=2052
                 left join v_TranmitStockAll TranmitStock on TE.FENTRYID=TranmitStock.FSRCENTRYID and TranmitStock.FSUPPLYFORMID='PUR_PurchaseOrder'
                 left join T_PUR_POORDERENTRY poe on TranmitStock.FSUPPLYENTRYID=poe.FENTRYID
                 left join T_PUR_POORDER po on po.FID=poe.FID
                 left join V_BD_BUYER buy on po.FPURCHASERID=buy.fid
                 left join T_BAS_ASSISTANTDATAENTRY asse on asse.FENTRYID=poe.FTrackingID
                 left join T_BAS_ASSISTANTDATAENTRY_L assel on asse.FENTRYID=assel.FENTRYID and assel.FLOCALEID=2052
                 where TE.FENTRYID in ({string.Join(",", longs)}) 
                order by T.FCreateDate,T.FBILLNO,TE.FSEQ ";
                var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sql);
                foreach (var data in datas)
                {
                    list.Add(new T_PUR_UrgentDeliveryLog
                    {
                        FSoId = Convert.ToInt64(data["FSoId"]),
                        FSoEntryId = Convert.ToInt64(data["FSoEntryId"]),
                        FSoNo = Convert.ToString(data["FSoNo"]),
                        FSoSeq = Convert.ToInt32(data["FSoSeq"]),
                        FSupplyTargetOrgId = Convert.ToInt64(data["FSupplyTargetOrgId"]),
                        FQty = Convert.ToDecimal(data["FQty"]),
                        FMaterialCode = Convert.ToString(data["FMaterialCode"]),
                        FMaterialName = Convert.ToString(data["FMaterialName"]),
                        FSoStatus = Convert.ToString(data["FSoStatus"]),
                        FPoId = Convert.ToInt64(data["FPoId"]),
                        FPoEntryId = Convert.ToInt64(data["FPoEntryId"]),
                        FPoNo = Convert.ToString(data["FPoNo"]),
                        FPoSeq = Convert.ToInt32(data["FPoSeq"]),
                        FBuyWechatCode = Convert.ToString(data["FBuyWechatCode"]),
                        FTrackingCode = Convert.ToString(data["FTrackingCode"]),
                        FTrackingName = Convert.ToString(data["FTrackingName"]),
                        FTrackingNumber = Convert.ToString(data["FTrackingNumber"]),
                        FSupplierDescriptions = Convert.ToString(data["FSupplierDescriptions"]),
                        FCreateDate = DateTime.Now,
                        FCreateUser = this.Context.UserName
                    });
                }
                //1、不同供货组织，不能一起申请。
                var orgList = list.Select(o => o.FSupplyTargetOrgId).Distinct().ToList();
                if (orgList.Count() > 1)
                {
                    this.View.ShowMessage($"不同供货组织，不能一起申请加急。", MessageBoxType.Error);
                    return;
                }
                //2、销售订单行未关闭
                if (list.Where(x => x.FSoStatus.Equals("已关闭")).Count() > 0)
                {
                    this.View.ShowMessage($"已关闭的销售订单，不能申请加急。", MessageBoxType.Error);
                    return;
                }
                //3、判断是否存在采购订单预留
                foreach (var item in list)
                {
                    if (string.IsNullOrEmpty(item.FPoNo))
                    {
                        errMsg += $"{item.FSoNo}-{item.FSoSeq}；";
                    }
                }
                if (!string.IsNullOrWhiteSpace(errMsg))
                {
                    this.View.ShowMessage($"销售订单[{errMsg}]不存在采购预留。", MessageBoxType.Error);
                    return;
                }
                //通知采购人员填写快递单号
                foreach (var item in list.Where(x => string.IsNullOrEmpty(x.FTrackingCode)).GroupBy(g => new { g.FBuyWechatCode, g.FPoNo, g.FPoSeq })
                    .Select(s => new { FBuyWechatCode = s.Key.FBuyWechatCode, FPoNo = s.Key.FPoNo, FPoSeq = s.Key.FPoSeq }).ToList())
                {
                    var buyWechatCode = item.FBuyWechatCode;
                    //业务申请此单需加急收货，请跟进供应商，填写快递单号。提醒人+提醒时间
                    if (!string.IsNullOrEmpty(buyWechatCode))
                    {
                        var wxContent = $"业务申请此单[{item.FPoNo}-{item.FPoSeq}]需加急收货，请跟进供应商，填写快递单号。【{this.Context.UserName}】";
                        SendTextMessageUtils.SendTextMessage(buyWechatCode, wxContent);
                    }
                    //errMsg += $"{item.FPoNo}-{item.FPoSeq}；";
                }
                //if (!string.IsNullOrWhiteSpace(errMsg))
                //{
                //    this.View.ShowMessage($"采购订单[{errMsg}]未填写快递信息，无法确认是否到货，已提醒采购员填写快递信息。", MessageBoxType.Error);
                //    return;
                //}

                SysReportShowParameter para = new SysReportShowParameter();
                para.OpenStyle.ShowType = ShowType.Modal;
                //唯一标识
                para.FormId = "PENY_PUR_UrgentRecInspRpt";
                para.IsShowFilter = false;
                para.CustomParams["FEntryIds"] = string.Join(",", longs);
                this.View.ShowForm(para);
            }
        }
    }
}
