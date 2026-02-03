using Kingdee.BOS.App;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Msg;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;

namespace Kingdee.Mymooo.Report.Plugin.SalesManagement
{
    [Description("销售订单跟踪查询报表"), HotUpdate]
    public class SalesOrderTrackQueryRpt : SysReportBaseService
    {
        public override void Initialize()
        {   //初始化
            base.Initialize();
            // 简单账表类型：普通、树形、分页
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            //this.IsCreateTempTableByPlugin = false;
            //是否分组汇总
            this.ReportProperty.IsGroupSummary = false;
        }

        /// <summary>
        /// 获取表头
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public override ReportTitles GetReportTitles(IRptParams filter)
        {
            //把过滤条件的内容，全部传入filter
            ReportTitles reportTitles = new ReportTitles();
            DynamicObject customFilter = filter.FilterParameter.CustomFilter;
            if (customFilter != null)
            {
                //var custName = this.GetBaseDataNameValue(customFilter["FCustCode"] as DynamicObjectCollection);
                reportTitles.AddTitle("F_PENY_SoNo", Convert.ToString(customFilter["FSoNo"]) == "" ? "全部" : customFilter["FSoNo"].ToString());
                reportTitles.AddTitle("F_PENY_CustName", customFilter["FCustCode"] == null ? "全部" : ((DynamicObject)customFilter["FCustCode"])["Name"].ToString());
                reportTitles.AddTitle("F_PENY_MATERIALName", Convert.ToString(customFilter["FMATERIALName"]) == "" ? "全部" : customFilter["FMATERIALName"].ToString());
                reportTitles.AddTitle("F_PENY_PoStatus", GetPoStatus(Convert.ToString(customFilter["FPoStatus"])));
                reportTitles.AddTitle("F_PENY_ProStatus", GetProStatus(Convert.ToString(customFilter["FProStatus"])));
                reportTitles.AddTitle("F_PENY_SoStatus", GetSoStatus(Convert.ToString(customFilter["FSoStatus"])));
                reportTitles.AddTitle("F_PENY_DeliveryStatus", GetDeliveryStatus(Convert.ToString(customFilter["FDeliveryStatus"])));
                reportTitles.AddTitle("F_PENY_MATERIALCode", customFilter["FMATERIALID"] == null ? "全部" : ((DynamicObject)customFilter["FMATERIALID"])["Number"].ToString());
                reportTitles.AddTitle("F_PENY_CustItemNo", Convert.ToString(customFilter["FCustItemNo"]) == "" ? "全部" : customFilter["FCustItemNo"].ToString());
                reportTitles.AddTitle("F_PENY_CustItemName", Convert.ToString(customFilter["FCustItemName"]) == "" ? "全部" : customFilter["FCustItemName"].ToString());
                reportTitles.AddTitle("F_PENY_CustPo", Convert.ToString(customFilter["FCustPo"]) == "" ? "全部" : customFilter["FCustPo"].ToString());
                reportTitles.AddTitle("F_PENY_SALERUserName", customFilter["FSALERUserId"] == null ? "全部" : ((DynamicObject)customFilter["FSALERUserId"])["Name"].ToString());
                reportTitles.AddTitle("F_PENY_StartDate", Convert.ToString(customFilter["FStartDate"]) == "" ? "全部" : DateTime.Parse(customFilter["FStartDate"].ToString()).ToString("yyyy-MM-dd"));
                reportTitles.AddTitle("F_PENY_EndDate", Convert.ToString(customFilter["FEndDate"]) == "" ? "全部" : DateTime.Parse(customFilter["FEndDate"].ToString()).ToString("yyyy-MM-dd"));
                reportTitles.AddTitle("F_PENY_LastDate", Convert.ToString(customFilter["FLastDate"]) == "" ? "全部" : DateTime.Parse(customFilter["FLastDate"].ToString()).ToString("yyyy-MM-dd"));
                reportTitles.AddTitle("F_PENY_ExpiryDay", Convert.ToString(customFilter["FExpiryDay"]) == "" ? "全部" : customFilter["FExpiryDay"].ToString());
                reportTitles.AddTitle("F_PENY_IsBarter", Convert.ToString(customFilter["FIsBarter"]).Equals("True") ? "是" : "否");
                reportTitles.AddTitle("F_PENY_IsCanDeliver", GetIsCanDeliverStatus(Convert.ToString(customFilter["FIsCanDeliver"])));
                reportTitles.AddTitle("F_PENY_CUSTMATERIALNO", Convert.ToString(customFilter["FCUSTMATERIALNO"]) == "" ? "全部" : customFilter["FCUSTMATERIALNO"].ToString());
                reportTitles.AddTitle("F_PENY_StartSalesOrderDate", Convert.ToString(customFilter["FSalesOrderDate1"]) == "" ? "全部" : DateTime.Parse(customFilter["FSalesOrderDate1"].ToString()).ToString("yyyy-MM-dd"));
                reportTitles.AddTitle("F_PENY_EndSalesOrderDate", Convert.ToString(customFilter["FSalesOrderDate2"]) == "" ? "全部" : DateTime.Parse(customFilter["FSalesOrderDate2"].ToString()).ToString("yyyy-MM-dd"));
                reportTitles.AddTitle("F_PENY_StartAuditTime", Convert.ToString(customFilter["FAuditTime1"]) == "" ? "全部" : DateTime.Parse(customFilter["FAuditTime1"].ToString()).ToString("yyyy-MM-dd"));
                reportTitles.AddTitle("F_PENY_EndAuditTime", Convert.ToString(customFilter["FAuditTime2"]) == "" ? "全部" : DateTime.Parse(customFilter["FAuditTime2"].ToString()).ToString("yyyy-MM-dd"));
                reportTitles.AddTitle("F_PENY_PlatCreatorId", Convert.ToString(customFilter["FPlatCreatorId"]) == "" ? "全部" : customFilter["FPlatCreatorId"].ToString());
                reportTitles.AddTitle("F_PENY_SupplyTargetOrgId", customFilter["FSupplyTargetOrgId"] == null ? "全部" : ((DynamicObject)customFilter["FSupplyTargetOrgId"])["Name"].ToString());
                reportTitles.AddTitle("F_PENY_ExistsReturn", Convert.ToString(customFilter["FExistsReturn"]).Equals("True") ? "是" : "否");
                reportTitles.AddTitle("F_PENY_UrgentDelivery", Convert.ToString(customFilter["FUrgentDelivery"]) == "" ? "全部" : Convert.ToString(customFilter["FUrgentDelivery"]).Equals("1") ? "是" : "否");
            }
            return reportTitles;
            //return base.GetReportTitles(filter);
        }

        private string GetPoStatus(string values)
        {
            string PoStatus = "全部";
            if (values.Equals("0"))
            {
                PoStatus = "未采购";
            }
            else if (values.Equals("1"))
            {
                PoStatus = "已采购";
            }
            return PoStatus;
        }

        private string GetProStatus(string values)
        {
            string ProStatus = "全部";
            if (values.Equals("0"))
            {
                ProStatus = "未生产";
            }
            else if (values.Equals("1"))
            {
                ProStatus = "已生产";
            }
            return ProStatus;
        }

        private string GetSoStatus(string values)
        {
            string SoStatus = "全部";
            if (values.Equals("A"))
            {
                SoStatus = "未关闭";
            }
            else if (values.Equals("B"))
            {
                SoStatus = "已关闭";
            }
            return SoStatus;
        }

        private string GetDeliveryStatus(string values)
        {
            string DeliveryStatus = "全部";
            switch (values)
            {
                case "0":
                    DeliveryStatus = "已完成";
                    break;
                case "1":
                    DeliveryStatus = "未完成";
                    break;
                case "2":
                    DeliveryStatus = "未完成未逾期";
                    break;
                case "3":
                    DeliveryStatus = "未完成已逾期";
                    break;
                case "4":
                    DeliveryStatus = "送货但未批核";
                    break;
                default:
                    break;
            }
            return DeliveryStatus;
        }

        private string GetIsCanDeliverStatus(string values)
        {
            string CanDeliverStatus = "全部";
            if (values.Equals("1"))
            {
                CanDeliverStatus = "满足";
            }
            else if (values.Equals("0"))
            {
                CanDeliverStatus = "不满足";
            }
            return CanDeliverStatus;
        }

        //基础资料名称（多选）
        private string GetBaseDataNameValue(DynamicObjectCollection dyobj)
        {
            string name = "";
            if (dyobj != null)
            {
                foreach (DynamicObject dynbj in dyobj)//把数据全部读取出来,转成文本，放到表头
                {
                    if (dynbj != null || !dynbj.DynamicObjectType.Properties.Contains("Name"))
                    {
                        DynamicObject dynbj2 = (DynamicObject)dynbj[2];
                        name = name + ",'" + dynbj2["Name"].ToString() + "'";
                    }
                }
                if (name.Length > 0)
                {
                    name = name.Substring(1, name.Length - 1);
                }
            }
            return name;
        }

        //创建临时报表
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            DynamicObject customFilter = filter.FilterParameter.CustomFilter;
            int expiryDay = 0;
            if (customFilter["FExpiryDay"] != null)
            {
                expiryDay = Int32.Parse(customFilter["FExpiryDay"].ToString());
            }

            //权限
            var userinfo = Kingdee.Mymooo.ServiceHelper.BaseManagement.UserServiceHelper.GetUserInfoForUserID(this.Context, this.Context.UserId);
            long userId = userinfo.FEMPID;

            //优先条件，可以先查询的条件。
            string priorityWhere = $" where T.FSALEORGID={this.Context.CurrentOrganizationInfo.ID} and T.FCANCELSTATUS='A'  ";
            string qxSql = $@"select 1 from T_SEC_USERROLEMAP t1
                        left join T_SEC_USERORG t2 on t1.FENTITYID=t2.FENTITYID
                        left join T_SEC_ROLE t3 on t1.FROLEID=t3.FROLEID
                        where t2.FUSERID={userinfo.FUSERID}  and t3.FNUMBER = 'KHGLY' and t2.FORGID={this.Context.CurrentOrganizationInfo.ID} ";
            if (!DBServiceHelper.ExecuteScalar<bool>(this.Context, qxSql, false))
            {
                qxSql = $@" 
                   and exists(
                        select 1
                        from T_SAL_SCSALERCUST sg
	                        inner join T_BD_OPERATORDETAILS g on sg.FSALERGROUPID = g.FOPERATORGROUPID
	                        inner join T_BD_OPERATORENTRY e on g.FENTRYID = e.FENTRYID and e.FOPERATORTYPE = 'XSY'
	                        inner join T_BD_STAFF staff on e.FSTAFFID = staff.FSTAFFID
                        where staff.FEmpInfoId = {userId} and T.FCUSTID=sg.FCUSTOMERID and T.FSALEORGID=sg.FSALEORGID)";
            }
            else
            {
                qxSql = "";
            }
            //单据编号
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FSoNo"])))
            {
                priorityWhere += $" and T.FBILLNO='{Convert.ToString(customFilter["FSoNo"])}' ";
            }
            //客户
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FCustCode"])))
            {
                priorityWhere += $" and CUS.FNUMBER='{((DynamicObject)customFilter["FCustCode"])["Number"]}' ";
            }
            //销售员
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FSALERUserId"])))
            {
                priorityWhere += $" and SALM.FNUMBER='{((DynamicObject)customFilter["FSALERUserId"])["Number"]}' ";
            }
            //物料Code
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FMATERIALID"])))
            {
                priorityWhere += $" and BDM.FNUMBER='{((DynamicObject)customFilter["FMATERIALID"])["Number"]}' ";
            }
            //物料名称
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FMATERIALName"])))
            {
                priorityWhere += $" and BDML.FNAME like '%{Convert.ToString(customFilter["FMATERIALName"])}%' ";
            }
            //客户采购单编号
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FCustPo"])))
            {
                priorityWhere += $" and T.FCustPurchaseNo like '%{Convert.ToString(customFilter["FCustPo"])}%' ";
            }
            //客户物料编码
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FCustItemNo"])))
            {
                priorityWhere += $" and TE.FCustItemNo like '%{Convert.ToString(customFilter["FCustItemNo"])}%' ";
            }
            //客户物料描述
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FCustItemName"])))
            {
                priorityWhere += $" and TE.FCustItemName like '%{Convert.ToString(customFilter["FCustItemName"])}%' ";
            }
            //客户料号
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FCUSTMATERIALNO"])))
            {
                priorityWhere += $" and te.FCUSTMATERIALNO like '%{Convert.ToString(customFilter["FCUSTMATERIALNO"])}%' ";
            }

            //平台同步日期开始日期
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FStartDate"])))
            {
                priorityWhere += $" and T.FCreateDate>='{Convert.ToString(customFilter["FStartDate"])}' ";
            }

            //平台同步日期结束日期
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FEndDate"])))
            {
                priorityWhere += $" and T.FCreateDate<'{DateTime.Parse(Convert.ToString(customFilter["FEndDate"])).AddDays(1).ToString("yyyy-MM-dd")}' ";
            }
            //到期日期
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FLastDate"])))
            {
                priorityWhere += $" and TED.FDELIVERYDATE <='{DateTime.Parse(Convert.ToString(customFilter["FLastDate"])).ToString("yyyy-MM-dd")}' ";
            }

            //平台单据日期开始日期
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FSalesOrderDate1"])))
            {
                priorityWhere += $" and T.FSalesOrderDate>='{Convert.ToString(customFilter["FSalesOrderDate1"])}' ";
            }

            //平台单据日期结束日期
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FSalesOrderDate2"])))
            {
                priorityWhere += $" and T.FSalesOrderDate<'{DateTime.Parse(Convert.ToString(customFilter["FSalesOrderDate2"])).AddDays(1).ToString("yyyy-MM-dd")}' ";
            }

            //平台审批日期开始日期
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FAuditTime1"])))
            {
                priorityWhere += $" and T.FAuditTime>='{Convert.ToString(customFilter["FAuditTime1"])}' ";
            }

            //平台审批日期结束日期
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FAuditTime2"])))
            {
                priorityWhere += $" and T.FAuditTime<'{DateTime.Parse(Convert.ToString(customFilter["FAuditTime2"])).AddDays(1).ToString("yyyy-MM-dd")}' ";
            }
            //销售单状态(A未关闭，B关闭)
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FSoStatus"])))
            {
                if (Convert.ToString(customFilter["FSoStatus"]).Equals("A"))
                {
                    priorityWhere += $" and (case when T.FCLOSESTATUS='B' then '已关闭' else (case when TE.FMrpCloseStatus='B' then '已关闭' else '未关闭' end) end)='未关闭' ";
                }
                else
                {
                    priorityWhere += $" and (case when T.FCLOSESTATUS='B' then '已关闭' else (case when TE.FMrpCloseStatus='B' then '已关闭' else '未关闭' end) end)='已关闭' ";
                }
            }
            //制单人
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FPlatCreatorId"])))
            {
                priorityWhere += $" and T.FPlatCreatorId like '%{Convert.ToString(customFilter["FPlatCreatorId"])}%' ";
            }

            //供货组织
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FSupplyTargetOrgId"])))
            {
                priorityWhere += $" and TE.FSupplyTargetOrgId='{((DynamicObject)customFilter["FSupplyTargetOrgId"])["ID"]}' ";
            }

            //包含退货订单
            if (Convert.ToString(customFilter["FExistsReturn"]).EqualsIgnoreCase("True"))
            {
                //包含退货，不含负数项(只含补货)
                priorityWhere += @" and TE.FQTY>0 ";
            }
            else
            {
                priorityWhere += @" and T.FBillTypeID!='a300e2620037435492aed9842875b451' ";
            }

            //是否加急收货
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FUrgentDelivery"])))
            {
                priorityWhere += $" and TE.FUrgentDelivery='{Convert.ToString(customFilter["FUrgentDelivery"])}' ";
            }

            //销售订单跟踪查询数据
            string sql = $@"/*dialect*/select {KSQL_SEQ},FSalesOrderType,FOrgName,FID,FXSENTRYID,FCreateDate,FSalesOrderDate,FAuditTime,FSalesOrderNo,FSEQ,FCustomerCode,FCustomerName,FSALERCODE,FSALERNAME
                            ,FMATERIALID,FMATERIALCode,FMATERIALName,FCustItemNo,FCustItemName,FCUSTMATERIALNO,FQTY,FSTOCKOUTQTY
                            ,FREMAINOUTQTY,FREMAINOUTQTYS,FBASEQTYStr,FAVBQTYStr,FRSEQTYStr,FREMAINSTOCKINQTY,FINSTOCKJOINQTY,FDeliQty,FReturnQty,FActualShipmentQty,FTAXPRICE,FAllAmount,FDISCOUNT,FCustPo,FDELIVERYDATE
                            ,FSoStatus,FCancelQty,FCancelStatus,FCGENTRYID,FCGBILLNO,FBuyerName,FJHRQ,FPOSTATUSNAME,FProENTRYID,FProBILLNO,FProManagerName,FFINISHDATE,FProSTATUSNAME,FSumProTransitQTY
                            ,FITEMGRP,FGROUPDESC,FITEMTYPE,FTYPEDESC,FNOTE,FSUPPLIERREPLYDATE,FSUPPLIERDESCRIPTIONS,FPROJECTNO,FRECCONDITIONID,FRECCONDITIONNAME
                            ,case when convert(nvarchar(20),DATEADD(DAY,-{expiryDay},FDELIVERYDATE),23)<=convert(nvarchar(20),getdate(),23) then 1 else 0 end IsExpiry,FSupplyTargetOrgName,FBASEQTY,FPlatCreatorId
                            ,FSupplyTargetOrgId,FStockOrgId,FMASTERID,FLEADERNAME,FUrgentDelivery
                             into {tableName}
                             from (
                             select orgl2.FNAME FOrgName,T.FSALEORGID,TE.FID as FID,TE.FENTRYID as FXSENTRYID
                            ,typel.FNAME FSalesOrderType --单据类型
							,T.FCreateDate --平台同步日期
                            ,T.FSalesOrderDate --平台单据日期
                            ,T.FAuditTime --平台审批日期
                            ,T.FBILLNO FSalesOrderNo,TE.FSEQ
							,CUS.FNUMBER  FCustomerCode,CUSL.FNAME FCustomerName --客户信息
							,SALM.FNUMBER FSALERCODE,SALML.FNAME FSALERNAME --销售员
							,TE.FMATERIALID,BDM.FNUMBER FMATERIALCode,BDML.FNAME FMATERIALName --物料
							,TE.FCustItemNo,TE.FCustItemName,te.FCUSTMATERIALNO --客户物料
							,TE.FQTY --销售数量
                            ,TER.FSTOCKOUTQTY --已出库数量
                            ,TER.FRemainOutQty --待出库数量
                            ,convert(decimal(23,10),0) as FREMAINOUTQTYS --待出库合计
                            ,convert(decimal(23,10),0) as FREMAINSTOCKINQTY --总采购在途
                            ,convert(decimal(23,10),0) as FSumProTransitQTY --总生产在途
                            ,convert(decimal(23,10),0) as FINSTOCKJOINQTY--总检验中数量
							,TE.FQTY-TER.FCANOUTQTY FDeliQty --发货通知单数量
                            ,TER.FReturnQty --累计退货数量
                            ,(TER.FStockOutQty-TER.FReturnQty) FActualShipmentQty --实际发货数量(销售订单的累计出库数-累计退货数)
                            ,TEF.FTAXPRICE --含税单价
                            ,TEF.FAllAmount --含税总金额
                            ,TEF.FDISCOUNT  --折扣额
							,T.FCustPurchaseNo FCustPo --客户采购单编号
                            ,TED.FDELIVERYDATE --销售要货日期
							,case when T.FCLOSESTATUS='B' then '已关闭' else (case when TE.FMrpCloseStatus='B' then '已关闭' else '未关闭' end) end FSoStatus --销售单行状态(已关闭和未关闭)
							,case when (T.FCLOSESTATUS='B' or TE.FMrpCloseStatus='B') and TER.FBaseDeliQty=0 then '全部取消' 
							 when (T.FCLOSESTATUS='B' or TE.FMrpCloseStatus='B') and TER.FBaseDeliQty>0 and TE.FQTY > TER.FBaseDeliQty then '部分取消' 
							 else '正常' end FCancelStatus   --取消状态
							 ,case when (T.FCLOSESTATUS='B' or TE.FMrpCloseStatus='B') and TE.FQTY>TER.FBaseDeliQty then TE.FQTY-TER.FBaseDeliQty else 0 end FCancelQty --取消数量(订购数量-累计发货通知数量)
                            ,convert(nvarchar(200),'') as FCGENTRYID
							,convert(nvarchar(200),'') as FCGBILLNO
							,convert(nvarchar(200),'') as FBuyerName--采购员
							,convert(nvarchar(200),'') as FJHRQ--采购交货日期
	                        ,convert(nvarchar(200),'') as FPOSTATUSNAME--采购状态(A:新建 Z:暂存 B: 审核中 C: 已审核 D:重新审核)
                            ,convert(nvarchar(200),'') as FProENTRYID
							,convert(nvarchar(200),'') as FProBILLNO
							,convert(nvarchar(200),'') as FProManagerName--生产主管
							,convert(nvarchar(200),'') as FFINISHDATE--生产完工日期
	                        ,convert(nvarchar(200),'') as FProSTATUSNAME--生产状态(A:新建 Z:暂存 B: 审核中 C: 已审核 D:重新审核)
                            ,TE.FPARENTSMALLID FITEMGRP--产品大类
							,gll.FNAME FGROUPDESC
							,TE.FSMALLID FITEMTYPE--产品小类
							,gl.FNAME FTYPEDESC
							,TE.FNOTE,TE.FSUPPLIERREPLYDATE,TE.FSUPPLIERDESCRIPTIONS
                            ,orgl.FNAME FSupplyTargetOrgName --供货组织
                            ,TE.FPROJECTNO,TF.FRECCONDITIONID,ConL.FNAME FRECCONDITIONNAME--收款条件
		                    ,convert(nvarchar(200),'') as FRSEQTYStr --库存预留数
							,convert(nvarchar(200),'') as FBASEQTYStr --即时库存字符串
                            ,convert(decimal(23,10),0) as FBASEQTY --即时库存
							,convert(nvarchar(200),'') as FAVBQTYStr, --可用库存字符串
                            T.FPlatCreatorId --销售订单创建人
                            ,TE.FSupplyTargetOrgId --供货组织
                            ,TE.FStockOrgId,BDM.FMASTERID
                            ,convert(nvarchar(200),'') as FLEADERNAME --组织间需求单的负责人
                            ,case when TE.FUrgentDelivery='1' then '是' else '否' end FUrgentDelivery
                            from T_SAL_ORDERENTRY TE
                            left join T_SAL_ORDERENTRY_R TER on TE.FENTRYID=TER.FENTRYID
                            left join T_SAL_ORDERENTRY_F TEF on TE.FENTRYID=TEF.FENTRYID
                            left join T_SAL_ORDERENTRY_D TED on TE.FENTRYID=TED.FENTRYID
                            left join T_SAL_ORDER T on TE.FID=T.FID
                            left join T_SAL_ORDERFIN TF on T.FID=TF.FID
                            left join T_BD_MATERIAL BDM on TE.FMATERIALID=BDM.FMATERIALID
                            left join T_BD_MATERIAL_L BDML on TE.FMATERIALID=BDML.FMATERIALID and BDML.FLOCALEID={this.Context.UserLocale.LCID}
                            left join T_BD_CUSTOMER CUS on T.FCUSTID=CUS.FCUSTID
							left join T_BD_CUSTOMER_L CUSL on CUSL.FCUSTID=CUS.FCUSTID AND CUSL.FLOCALEID={this.Context.UserLocale.LCID}
                            left join V_BD_SALESMAN SALM on T.FSALERID=SALM.fid
							left join V_BD_SALESMAN_L SALML on SALM.fid=SALML.fid and SALML.FLOCALEID={this.Context.UserLocale.LCID}
							left join T_BD_RecCondition_L ConL  on ConL.FID=TF.FRECCONDITIONID
							left join T_BD_MATERIALGROUP_L gl on TE.FSMALLID = gl.FID and gl.FLOCALEID = {this.Context.UserLocale.LCID}
							left join T_BD_MATERIALGROUP_L gll on TE.FPARENTSMALLID = gll.FID and gll.FLOCALEID = {this.Context.UserLocale.LCID}
                            left join T_ORG_ORGANIZATIONS_L orgl on orgl.FORGID=TE.FSupplyTargetOrgId and orgl.FLOCALEID={this.Context.UserLocale.LCID}
                            left join T_ORG_ORGANIZATIONS_L orgl2 on orgl2.FORGID=T.FSALEORGID and orgl2.FLOCALEID={this.Context.UserLocale.LCID}
                            left join T_BAS_BILLTYPE_L typel on T.FBILLTYPEID=typel.FBILLTYPEID and typel.FLOCALEID = {this.Context.UserLocale.LCID}
                            {priorityWhere} {qxSql}
							) t1  where 1=1  ";

            //替换补货
            if (Convert.ToString(customFilter["FIsBarter"]).EqualsIgnoreCase("True"))
            {
                sql += @" and FREMAINOUTQTY>0 and FSoStatus='未关闭' and exists(
								select top 1 t2.FENTRYID from T_SAL_RETURNNOTICEENTRY t2 
								inner join T_SAL_RETURNNOTICE t3 on t2.FID=t3.FID and t3.FCancelStatus<>'B' and t3.FCLOSESTATUS<>'B' and t2.FCLOSESTATUS<>'B'
								where t2.FSOENTRYID=t1.FXSENTRYID and FRMTYPE='b9349cf911cd4fdbbf18bec028ee7fe1'
							 ) ";
            }

            //送货状态(0已完成/1未完成/2未完成未逾期/3未完成已逾期/4送货但未批核)
            if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FDeliveryStatus"])))
            {
                switch (Convert.ToString(customFilter["FDeliveryStatus"]))
                {
                    case "0":
                        sql += " and FREMAINOUTQTY = 0 ";
                        break;
                    case "1":
                        sql += " and FREMAINOUTQTY > 0 ";
                        break;
                    case "2":
                        //未完成已逾期：当前日期+逾期天数>要求交货日，没有累计发货通知数量
                        sql += $" and FREMAINOUTQTY > 0 and DATEADD(d,{expiryDay},GETDATE()) > FDELIVERYDATE ";
                        break;
                    case "3":
                        //未完成未逾期：当前日期+逾期天数<=要求交货日，没有累计发货通知数量
                        sql += $" and FREMAINOUTQTY > 0 and DATEADD(d,{expiryDay},GETDATE()) <= FDELIVERYDATE ";
                        break;
                    case "4":
                        //送货但未批核：做了发货通知单，未批核的
                        sql += @" and exists (
                                                 select LK.FSID from
                                                 T_SAL_DELIVERYNOTICEENTRY_LK LK
                                                 left join T_SAL_DELIVERYNOTICEENTRY DELE on DELE.FENTRYID=LK.FENTRYID
                                                 left join T_SAL_DELIVERYNOTICE DEL on DELE.FID=DEL.FID
                                                 where DEL.FDOCUMENTSTATUS <> 'C' and t1.FID=LK.FSBILLID and t1.FXSENTRYID=LK.FSID
                                                 ) ";
                        break;
                }
            }

            using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
            {
                //排序
                string dataSort = Convert.ToString(filter.FilterParameter.SortString);
                if (dataSort != "")
                {
                    sql = string.Format(sql, dataSort);
                }
                else
                {
                    sql = string.Format(sql, " FCreateDate,FSalesOrderNo,FSEQ ");
                }
                DBServiceHelper.Execute(this.Context, sql);

                //更新采购订单的关联数据
                sql = $@"/*dialect*/update t1 set t1.FCGENTRYID=t2.FCGENTRYID,t1.FCGBILLNO=t2.FCGBILLNO,t1.FBuyerName=t2.FBuyerName,t1.FJHRQ=t2.FJHRQ,t1.FPOSTATUSNAME=t2.FPOSTATUSNAME
                    from {tableName} t1,(select FDEMANDBILLENTRYID, STRING_AGG(FENTRYID, ',') AS FCGENTRYID,STRING_AGG(FBILLNO, '、') AS FCGBILLNO,STRING_AGG(FBuyerName, '、') AS FBuyerName
                        ,STRING_AGG(FDELIVERYDATE, '、') AS FJHRQ,STRING_AGG(FDOCUMENTSTATUS, '、') AS FPOSTATUSNAME 
                        from (
							select t1.FENTRYID,t4.FBILLNO +'-' +convert(nvarchar(5),t1.FSEQ) FBILLNO,isnull(buyl.FNAME,'') FBuyerName,convert(nvarchar(20),t3.FDELIVERYDATE,23) FDELIVERYDATE,
							case when t4.FDOCUMENTSTATUS='A' then '新建' 
							 when t4.FDOCUMENTSTATUS='Z' then '暂存' 
							 when t4.FDOCUMENTSTATUS='B' then '审核中'
							 when t4.FDOCUMENTSTATUS='C' then '已审核'
							 when t4.FDOCUMENTSTATUS='D' then '重新审核'
							 else '' end FDOCUMENTSTATUS
                            ,t2.FDEMANDBILLENTRYID,t5.FNUMBER ItemNo
                            from
                            T_PUR_POORDERENTRY t1
                            left join T_PUR_POORDERENTRY_R t2 on t1.FENTRYID=t2.FENTRYID
                            left join T_PUR_POORDERENTRY_D t3 on t1.FENTRYID=t3.FENTRYID
                            left join T_PUR_POORDER t4 on t1.FID=t4.FID
							left join V_BD_BUYER_L buyl on t4.FPURCHASERID=buyl.fid
                            left join T_BD_MATERIAL t5 on t5.FMATERIALID=t1.FMATERIALID
						    where t2.FDEMANDBILLENTRYID>0  and t4.FCANCELSTATUS='A' 
                            and exists (select top 1 FID from {tableName} temp where  t2.FDEMANDBILLENTRYID=temp.FXSENTRYID and t5.FNUMBER=temp.FMATERIALCode) 
							union
							select t1.FENTRYID,t4.FBILLNO +'-' +convert(nvarchar(5),t1.FSEQ) FBILLNO,isnull(buyl.FNAME,'')  FBuyerName,convert(nvarchar(20),t3.FDELIVERYDATE,23) FDELIVERYDATE,
							case when t4.FDOCUMENTSTATUS='A' then '新建' 
							 when t4.FDOCUMENTSTATUS='Z' then '暂存' 
							 when t4.FDOCUMENTSTATUS='B' then '审核中'
							 when t4.FDOCUMENTSTATUS='C' then '已审核'
							 when t4.FDOCUMENTSTATUS='D' then '重新审核'
							 else '' end FDOCUMENTSTATUS
                            ,t8.FSALEORDERENTRYID FDEMANDBILLENTRYID,t9.FNUMBER ItemNo
                            from T_PUR_POORDERENTRY t1
                            left join T_PUR_POORDERENTRY_R t2 on t1.FENTRYID=t2.FENTRYID
                            left join T_PUR_POORDERENTRY_D t3 on t1.FENTRYID=t3.FENTRYID
                            left join T_PUR_POORDER t4 on t1.FID=t4.FID
							left join T_PUR_POORDERENTRY_LK t5 on t1.FENTRYID=t5.FENTRYID
							left join V_BD_BUYER_L buyl on t4.FPURCHASERID=buyl.fid
							left join T_PUR_REQENTRY_LK t6 on t6.FENTRYID=t5.FSID
							left join T_PLN_PLANORDER_LK t7 on t6.FSID=t7.FID
                            left JOIN T_PLN_PLANORDER_B t8 ON t7.FSBILLID=t8.FID
                            left join T_BD_MATERIAL t9 on t9.FMATERIALID=t1.FMATERIALID
							where t8.FSALEORDERENTRYID>0  and t4.FCANCELSTATUS='A'
                            and exists (select top 1 FID from {tableName} temp where  t8.FSALEORDERENTRYID=temp.FXSENTRYID and t9.FNUMBER=temp.FMATERIALCode)
                            ) t1 
                            GROUP BY FDEMANDBILLENTRYID) t2 where t1.FXSENTRYID=t2.FDEMANDBILLENTRYID ";
                DBServiceHelper.Execute(this.Context, sql);

                //更新生产订单的关联数据
                sql = $@"/*dialect*/update t1 set t1.FProENTRYID=t2.FProENTRYID,t1.FProBILLNO=t2.FProBILLNO,t1.FProManagerName=t2.FProManagerName,t1.FFINISHDATE=t2.FFINISHDATE,t1.FProSTATUSNAME=t2.FProSTATUSNAME
                         from {tableName} t1,(select FDEMANDBILLENTRYID,STRING_AGG(FENTRYID, ',') AS FProENTRYID,STRING_AGG(FBILLNO, '、') AS FProBILLNO
                        ,STRING_AGG(FProManagerName, '、') AS FProManagerName
                        ,STRING_AGG(FPLANFINISHDATE, '、') AS FFINISHDATE,STRING_AGG(FProSTATUSNAME, '、') AS FProSTATUSNAME      
						from (    
						   select t2.FENTRYID,t1.FBILLNO +'-' +convert(nvarchar(5),t2.FSEQ) FBILLNO,isnull(t9.FNAME,'') FProManagerName,
							convert(nvarchar(20),t2.FPLANFINISHDATE,23) FPLANFINISHDATE,
			 				case when t1.FDOCUMENTSTATUS='A' then '新建' 
							 when t1.FDOCUMENTSTATUS='Z' then '暂存' 
							 when t1.FDOCUMENTSTATUS='B' then '审核中'
							 when t1.FDOCUMENTSTATUS='C' then '已审核'
							 when t1.FDOCUMENTSTATUS='D' then '重新审核'
							 else '' end FProSTATUSNAME
                            ,t2.FSALEORDERENTRYID FDEMANDBILLENTRYID,t3.FNUMBER ItemNo
							from T_PRD_MO t1
                            inner join T_PRD_MOENTRY t2 on t1.FID=t2.FID
                            left join T_BD_MATERIAL t3 on t3.FMATERIALID=t2.FMATERIALID
							left join T_BD_DEPARTMENT t7 on t2.FWORKSHOPID=t7.FDEPTID
							left join T_BD_STAFF t8 on t7.FPENYLEADER=t8.FSTAFFID
							left join T_BD_STAFF_L t9 on t9.FSTAFFID=t8.FSTAFFID
                            where t1.FCANCELSTATUS='A'  and FSALEORDERENTRYID>0
                            and exists (select top 1 FID from {tableName} temp where  t2.FSALEORDERENTRYID=temp.FXSENTRYID and t3.FNUMBER=temp.FMATERIALCode) 
                            union 
                            select t2.FENTRYID,
							t1.FBILLNO +'-' +convert(nvarchar(5),t2.FSEQ) FBILLNO,
							isnull(t9.FNAME,'') FProManagerName,convert(nvarchar(20),t2.FPLANFINISHDATE,23) FPLANFINISHDATE,
							case when t1.FDOCUMENTSTATUS='A' then '新建' 
							 when t1.FDOCUMENTSTATUS='Z' then '暂存' 
							 when t1.FDOCUMENTSTATUS='B' then '审核中'
							 when t1.FDOCUMENTSTATUS='C' then '已审核'
							 when t1.FDOCUMENTSTATUS='D' then '重新审核'
							 else '' end FProSTATUSNAME
                            ,t6.FSALEORDERENTRYID FDEMANDBILLENTRYID,t3.FNUMBER ItemNo
							from T_PRD_MO t1
                            inner join T_PRD_MOENTRY t2 on t1.FID=t2.FID
                            left join T_BD_MATERIAL t3 on t3.FMATERIALID=t2.FMATERIALID
                            inner join T_PLN_PLANORDER t4 on t4.FBILLNO=t2.FSRCBILLNO
                            inner join T_PLN_PLANORDER_LK t5 on t5.FID=t4.FID
                            inner join T_PLN_PLANORDER_B t6 on t6.FID=t5.FSBILLID
							left join T_BD_DEPARTMENT t7 on t2.FWORKSHOPID=t7.FDEPTID
							left join T_BD_STAFF t8 on t7.FPENYLEADER=t8.FSTAFFID
							left join T_BD_STAFF_L t9 on t9.FSTAFFID=t8.FSTAFFID
                            where t1.FCANCELSTATUS='A'  and t2.FSALEORDERENTRYID=0 and t6.FSALEORDERENTRYID>0
                            and exists (select top 1 FID from {tableName} temp where  t6.FSALEORDERENTRYID=temp.FXSENTRYID and t3.FNUMBER=temp.FMATERIALCode) 
							) t1 
							GROUP BY FDEMANDBILLENTRYID) t2 where t1.FXSENTRYID=t2.FDEMANDBILLENTRYID ";
                DBServiceHelper.Execute(this.Context, sql);

                //采购状态（1已采购/0未采购）
                if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FPoStatus"])))
                {
                    //已采购
                    if (Convert.ToString(customFilter["FPoStatus"]).Equals("1"))
                    {
                        sql = $@"/*dialect*/delete from {tableName} where FCGENTRYID='' ";
                        DBServiceHelper.Execute(this.Context, sql);
                    }
                    else
                    {
                        //未采购
                        sql = $@"/*dialect*/delete from {tableName} where FCGENTRYID<>'' ";
                        DBServiceHelper.Execute(this.Context, sql);
                    }
                }

                //生产状态（1已生产/0未生产）
                if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FProStatus"])))
                {
                    //已生产
                    if (Convert.ToString(customFilter["FProStatus"]).Equals("1"))
                    {
                        sql = $@"/*dialect*/delete from {tableName} where FProENTRYID='' ";
                        DBServiceHelper.Execute(this.Context, sql);
                    }
                    else
                    {
                        //未生产
                        sql = $@"/*dialect*/delete from {tableName} where FProENTRYID<>'' ";
                        DBServiceHelper.Execute(this.Context, sql);
                    }
                }

                //更新待出库数累计
                sql = $@"/*dialect*/update t1 set t1.FREMAINOUTQTYS=t2.FREMAINOUTQTYS
                         from {tableName} t1,(
						  select
							 SUM(FREMAINOUTQTY) FREMAINOUTQTYS,--待出库数累计
							 t2.FSupplyTargetOrgId,
							 t4.FNUMBER FMATERIALCode
							 from
							 T_SAL_ORDERENTRY_R t1
							 inner join T_SAL_ORDERENTRY t2 on t1.FENTRYID = t2.FENTRYID
							 inner join T_SAL_ORDER t3 on t1.FID = t3.FID
							inner join T_BD_MATERIAL t4 on t2.FMATERIALID=t4.FMATERIALID
							 where
							 t3.FDOCUMENTSTATUS = 'C'  and t3.FCANCELSTATUS='A'  and t3.FCLOSESTATUS='A' and FREMAINOUTQTY>0
							and exists (select top 1 FID from {tableName} temp where  t2.FSupplyTargetOrgId=temp.FSupplyTargetOrgId and t4.FNUMBER=temp.FMATERIALCode)
							group by t4.FNUMBER,t2.FSupplyTargetOrgId
						 ) t2 where t1.FMATERIALCode=t2.FMATERIALCode and t1.FSupplyTargetOrgId=t2.FSupplyTargetOrgId ";
                DBServiceHelper.Execute(this.Context, sql);

                //更新采购订单剩余收料数量(总采购在途)
                sql = $@"/*dialect*/update t1 set t1.FREMAINSTOCKINQTY=t2.FREMAINSTOCKINQTY
                         from {tableName} t1,(
							select
                            SUM(FREMAINRECEIVEQTY) FREMAINSTOCKINQTY,--取采购订单剩余收料数量(总采购在途)
                            t3.FPURCHASEORGID,
                            t4.FNUMBER FMATERIALCode
                            from
                            T_PUR_POORDERENTRY_R t1
                            inner join T_PUR_POORDERENTRY t2 on t1.FENTRYID = t2.FENTRYID
                            inner join T_PUR_POORDER t3 on t1.FID = t3.FID
							inner join T_BD_MATERIAL t4 on t2.FMATERIALID=t4.FMATERIALID
                            where t3.FDOCUMENTSTATUS = 'C' and t3.FCANCELSTATUS='A' and t3.FCLOSESTATUS='A' and t2.FMRPCLOSESTATUS='A' and t2.FMRPTERMINATESTATUS='A' and FREMAINSTOCKINQTY>0
							and exists (select top 1 FID from {tableName} temp where  t3.FPURCHASEORGID=temp.FSupplyTargetOrgId and t4.FNUMBER=temp.FMATERIALCode)
                            group by t3.FPURCHASEORGID,t4.FNUMBER
						 ) t2 where t1.FMATERIALCode=t2.FMATERIALCode and t1.FSupplyTargetOrgId=t2.FPURCHASEORGID ";
                DBServiceHelper.Execute(this.Context, sql);

                //更新生产订单未入库数量(总生产在途)
                sql = $@"/*dialect*/update t1 set t1.FSumProTransitQTY=t2.FNOSTOCKINQTY
                         from {tableName} t1,(
							select
                            SUM(t5.FNOSTOCKINQTY) FNOSTOCKINQTY,--取生产订单未入库数量(总生产在途)
                            t2.FPRDORGID,
                            t3.FNUMBER FMATERIALCode
                            from
                            T_PRD_MOENTRY t1
                            inner join T_PRD_MO t2 on t1.FID = t2.FID
							inner join T_BD_MATERIAL t3 on t1.FMATERIALID=t3.FMATERIALID
							inner join T_PRD_MOENTRY_A  t4 on t1.FENTRYID=t4.FENTRYID
							inner join T_PRD_MOENTRY_Q  t5 on t1.FENTRYID=t5.FENTRYID
                            where t2.FDOCUMENTSTATUS = 'C' and t5.FNOSTOCKINQTY>0 and t2.FCANCELSTATUS='A' and t4.FSTATUS in ('1','2','3','4','5')
							and exists (select top 1 FID from {tableName} temp where  t2.FPRDORGID=temp.FSupplyTargetOrgId and t3.FNUMBER=temp.FMATERIALCode)
                            group by t2.FPRDORGID, t3.FNUMBER
							) t2 where t1.FMATERIALCode=t2.FMATERIALCode and t1.FSupplyTargetOrgId=t2.FPRDORGID ";
                DBServiceHelper.Execute(this.Context, sql);

                //更新总检验中数量
                sql = $@"/*dialect*/update t1 set t1.FINSTOCKJOINQTY=t2.FINSTOCKJOINQTY
							from {tableName} t1,(
			              select SUM(t2.FACTRECEIVEQTY) - SUM(t1.FINSTOCKBASEQTY) - SUM(t1.FRETURNBASEQTY) FINSTOCKJOINQTY,--总检验中数量
                            t3.FSTOCKORGID,
                            t4.FNUMBER FMATERIALCode
                            from
                            T_PUR_RECEIVEENTRY_S t1
                            inner join T_PUR_ReceiveEntry t2 on t1.FENTRYID = t2.FENTRYID
                            inner join T_PUR_Receive t3 on t1.FID = t3.FID
							inner join T_BD_MATERIAL t4 on t2.FMATERIALID=t4.FMATERIALID
                            where
                            t3.FDOCUMENTSTATUS = 'C' and t3.FCANCELSTATUS='A'
                            and t2.FCHECKINCOMING = 1
							and exists (select top 1 FID from {tableName} temp where t3.FSTOCKORGID=temp.FSupplyTargetOrgId and t4.FNUMBER=temp.FMATERIALCode)
                            group by t3.FSTOCKORGID, t4.FNUMBER
							) t2 where t1.FMATERIALCode=t2.FMATERIALCode and t1.FSupplyTargetOrgId=t2.FSTOCKORGID  ";
                DBServiceHelper.Execute(this.Context, sql);

                //更新预留数
                sql = $@"/*dialect*/update t1 set t1.FRSEQTYStr=t2.FRSEQTYStr
							from {tableName} t1,(
							select FSRCENTRYID,STRING_AGG(FRSEQTYStr, '、') AS FRSEQTYStr from (
							select FSRCENTRYID,STOCKNAME+'：'+convert(nvarchar(10),convert(float,FBASEQTY)) FRSEQTYStr  from v_ReservedStockAll t1
							where exists (select top 1 FID from {tableName} temp 
							where  FSRCENTRYID=convert(nvarchar(50),temp.FXSENTRYID))
							) datas group by FSRCENTRYID
						) t2 where t1.FXSENTRYID=t2.FSRCENTRYID  ";
                DBServiceHelper.Execute(this.Context, sql);

                //库存临时表
                sql = $@"/*dialect*/SELECT convert(float,t1.FBASEQTY) FBASEQTY--即时库存
                        ,convert(float,t1.FAVBQTY) FAVBQTY --可用库存
                        ,t4.FNAME,t1.FSTOCKORGID,t1.FMATERIALID 
						into #stockTemp
						FROM V_STK_INVENTORY_CUS t1
                        LEFT JOIN T_BD_STOCK_L t4 on t1.FSTOCKID=t4.FSTOCKID and t4.FLOCALEID={this.Context.UserLocale.LCID}
                        LEFT JOIN T_ORG_ORGANIZATIONS t5 ON t1.FSTOCKORGID=t5.FORGID
                        WHERE  
						exists (select top 1 FID from {tableName} temp 
						where  t1.FMATERIALID=temp.FMASTERID and (t1.FSTOCKORGID=temp.FSupplyTargetOrgId or t1.FSTOCKORGID = temp.FStockOrgId))  ";
                DBServiceHelper.Execute(this.Context, sql);

                //即时库存拼接字符串
                sql = $@"/*dialect*/update t1 set t1.FBASEQTYStr=t2.NewFBASEQTY
                        from {tableName} t1,(
                        select t1.FXSENTRYID,(select STRING_AGG(FNAME+'：'+convert(nvarchar(10),convert(float,FBASEQTY)), '、') AS FBASEQTYStr  
                        from #stockTemp t2 where t2.FBASEQTY>0 and  t1.FMASTERID=t2.FMATERIALID 
                        and (t2.FSTOCKORGID = t1.FSupplyTargetOrgId or t2.FSTOCKORGID = t1.FStockOrgId)) NewFBASEQTY
                        from {tableName} t1
                        ) t2 where t1.FXSENTRYID=t2.FXSENTRYID ";
                DBServiceHelper.Execute(this.Context, sql);

                //可用库存拼接字符串
                sql = $@"/*dialect*/update t1 set t1.FAVBQTYStr=t2.NewFAVBQTYStr
                        from {tableName} t1,(
                        select t1.FXSENTRYID,(select STRING_AGG(FNAME+'：'+convert(nvarchar(10),convert(float,FAVBQTY)), '、') AS FAVBQTYStr  
                        from #stockTemp t2 where t2.FAVBQTY>0 and  t1.FMASTERID=t2.FMATERIALID 
                        and (t2.FSTOCKORGID = t1.FSupplyTargetOrgId or t2.FSTOCKORGID = t1.FStockOrgId)) NewFAVBQTYStr
                        from {tableName} t1
                        ) t2 where t1.FXSENTRYID=t2.FXSENTRYID ";
                DBServiceHelper.Execute(this.Context, sql);

                //即时库存汇总
                sql = $@"/*dialect*/update t1 set t1.FBASEQTY=t2.NewFBASEQTY
                        from {tableName} t1,(
                        select t1.FXSENTRYID,(select sum(FBASEQTY)  from #stockTemp t2 where t2.FBASEQTY>0 and  t1.FMASTERID=t2.FMATERIALID 
                        and (t2.FSTOCKORGID = t1.FSupplyTargetOrgId or t2.FSTOCKORGID = t1.FStockOrgId)) NewFBASEQTY
                        from {tableName} t1 
                        ) t2 where t1.FXSENTRYID=t2.FXSENTRYID ";
                DBServiceHelper.Execute(this.Context, sql);

                //组织间需求负责人
                sql = $@"/*dialect*/update {tableName} set FLEADERNAME=isnull((select top 1 t2.FNAME from T_PLN_REQUIREMENTORDER t1
                        left join T_BD_STAFF_L t2 on t2.FSTAFFID=t1.FPENYLEADER
                        where t1.FSALEORDERID={tableName}.FID and t1.FSALEORDERENTRYID={tableName}.FXSENTRYID),'')  ";
                DBServiceHelper.Execute(this.Context, sql);

                //满足发货(1满足，0不满足)
                if (!string.IsNullOrWhiteSpace(Convert.ToString(customFilter["FIsCanDeliver"])))
                {
                    //不满足 可出数量(未推发货通知单)>0  && 即时库存<=0 
                    if (Convert.ToString(customFilter["FIsCanDeliver"]).Equals("0"))
                    {
                        //  sql += $" and (FQTY-FDeliQty)>0 and FBASEQTY<=0 ";
                        sql = $@"/*dialect*/delete from {tableName} where (FQTY-FDeliQty)<=0 or isnull(FBASEQTY,0)>0  ";
                        DBServiceHelper.Execute(this.Context, sql);
                    }
                    else if (Convert.ToString(customFilter["FIsCanDeliver"]).Equals("1"))
                    {
                        //满足 可出数量(未推发货通知单)>0  && 即时库存>0  
                        // sql += $" and (FQTY-FDeliQty)>0 and FBASEQTY>0 ";

                        sql = $@"/*dialect*/delete from {tableName} where (FQTY-FDeliQty)<=0 or isnull(FBASEQTY,0)<=0  ";
                        DBServiceHelper.Execute(this.Context, sql);
                    }
                }

                //重新排序
                sql = $@"/*dialect*/update  t1 set t1.FIDENTITYID=t2.i from {tableName} t1,
                    (select ROW_NUMBER() OVER (ORDER BY FIDENTITYID) i,FIDENTITYID from {tableName}) t2 
                    where t1.FIDENTITYID=t2.FIDENTITYID ";
                DBServiceHelper.Execute(this.Context, sql);

                cope.Complete();
            }
        }

        //设置汇总列信息
        public override List<SummaryField> GetSummaryColumnInfo(IRptParams filter)
        {
            var result = base.GetSummaryColumnInfo(filter);
            result.Add(new SummaryField("FSEQ", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.COUNT));
            result.Add(new SummaryField("FQTY", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FSTOCKOUTQTY", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FREMAINOUTQTY", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FAllAmount", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            return result;
        }

    }
}
