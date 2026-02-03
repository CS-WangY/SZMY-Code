using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.ComponentModel;
using System.Text;

namespace Kingdee.Mymooo.Business.PlugIn.SalesManagement
{
    [Description("销售订单跟踪查询插件"), HotUpdate]
    public class SalesOrderStrackingQuery : AbstractDynamicFormPlugIn
    {
        public static bool isFinsh;
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            this.Model.SetValue("F_PENY_Date", System.DateTime.Now.AddMonths(-1).ToShortDateString());
            this.Model.SetValue("F_PENY_Date1", System.DateTime.Now.ToShortDateString());
            this.View.UpdateView("F_PENY_Date");
            this.View.UpdateView("F_PENY_Date1");
        }
        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);
            if (e.Key.EqualsIgnoreCase("F_PENY_Button"))
            {
                isFinsh = false;
                StringBuilder spWhere = new StringBuilder();
                if (!(this.Model.GetValue("F_PENY_Text") is null))
                {
                    spWhere.AppendLine($"and T.FBILLNO like '%{this.Model.GetValue("F_PENY_Text").ToString()}%'");
                }
                //物料
                if (!(this.Model.GetValue("F_PENY_Base2") is null))
                {
                    var marterail = this.Model.GetValue("F_PENY_Base2") as DynamicObject;
                    spWhere.AppendLine($"and BDM.FNUMBER like '%{marterail["Number"].ToString()}%'");
                }
                if (!(this.Model.GetValue("F_PENY_Text11") is null))
                {
                    spWhere.AppendLine($"and BDML.FDESCRIPTION like '%{this.Model.GetValue("F_PENY_Text11").ToString()}%'");
                }
                //客户
                if (!(this.Model.GetValue("F_PENY_Base3") is null))
                {
                    var cus = this.Model.GetValue("F_PENY_Base3") as DynamicObject;
                    spWhere.AppendLine($"and CUS.FNUMBER like '%{cus["Number"].ToString()}%'");
                }
                if (!(this.Model.GetValue("F_PENY_Text3") is null))
                {
                    spWhere.AppendLine($"and cg.FBILLNO like '%{this.Model.GetValue("F_PENY_Text3").ToString()}%'");
                }
                if (!(this.Model.GetValue("F_PENY_Text4") is null))
                {
                    spWhere.AppendLine($"and TE.FCustItemNo like '%{this.Model.GetValue("F_PENY_Text4").ToString()}%'");
                }
                if (!(this.Model.GetValue("F_PENY_Text41") is null))
                {
                    spWhere.AppendLine($"and TE.FCustItemName like '%{this.Model.GetValue("F_PENY_Text41").ToString()}%'");
                }
                if (!string.IsNullOrWhiteSpace(this.Model.GetValue("F_PENY_Combo").ToString()))
                {
                    switch (this.Model.GetValue("F_PENY_Combo").ToString())
                    {
                        case "0":
                            spWhere.AppendLine($"and cg.FBILLNO is null");
                            break;
                        case "1":
                            spWhere.AppendLine($"and cg.FBILLNO is not null");
                            break;
                    }
                }
                if (!string.IsNullOrWhiteSpace(this.Model.GetValue("F_PENY_Combo1").ToString()))
                {
                    switch (this.Model.GetValue("F_PENY_Combo1").ToString())
                    {
                        //已完成
                        case "0":
                            spWhere.AppendLine("and TER.FREMAINOUTQTY = 0");
                            break;
                        //未完成
                        case "1":
                            spWhere.AppendLine("and TER.FREMAINOUTQTY > 0");
                            break;
                        //未完成未逾期
                        case "2":
                            spWhere.AppendLine("and TER.FREMAINOUTQTY > 0 and YQ.max_date < TED.FDELIVERYDATE");
                            break;
                        //未完成已逾期
                        case "3":
                            spWhere.AppendLine("and TER.FREMAINOUTQTY > 0 and YQ.max_date > TED.FDELIVERYDATE");
                            break;
                        //送货未审核
                        case "4":
                            spWhere.AppendLine(@"and exists
                                            (
                                            select LK.FSID from
                                            T_SAL_DELIVERYNOTICEENTRY_LK LK
                                            left join T_SAL_DELIVERYNOTICEENTRY t1 on t1.FENTRYID=LK.FENTRYID
                                            left join T_SAL_DELIVERYNOTICE t2 on t1.FID=t2.FID
                                            where t2.FDOCUMENTSTATUS <> 'C' and T.FID=LK.FSBILLID and TE.FENTRYID=LK.FSID
                                            )");
                            break;
                    }

                }
                if (!string.IsNullOrWhiteSpace(this.Model.GetValue("F_PENY_Combo2").ToString()))
                {
                    spWhere.AppendLine($"and T.FDOCUMENTSTATUS = '{this.Model.GetValue("F_PENY_Combo2").ToString()}'");
                }
                //销售员
                if (!(this.Model.GetValue("F_PENY_Base4") is null))
                {
                    var salm = this.Model.GetValue("F_PENY_Base4") as DynamicObject;
                    spWhere.AppendLine($"and SALM.FNUMBER like '%{salm["Number"].ToString()}%'");
                }
                if (!(this.Model.GetValue("F_PENY_Date") is null) && !(this.Model.GetValue("F_PENY_Date1") is null))
                {
                    spWhere.AppendLine($"and T.FDATE between '{this.Model.GetValue("F_PENY_Date").ToString()}' and '{this.Model.GetValue("F_PENY_Date1").ToString()}'");
                }

                var sSql = $@"select
                            TE.FID as FXSID,TE.FENTRYID as FXSENTRYID,
                            T.FDATE,T.FBILLNO,T.FCUSTID,T.FSALERID,TE.FMATERIALID,te.FMapId,te.FCUSTMATERIALNO
                            ,te.FQTY--销售数量
                            ,TER.FSTOCKOUTQTY--已出库数量
                            ,TER.FREMAINOUTQTY--待出库数量
                            ,NC.FREMAINOUTQTY as FREMAINOUTQTYS--待出库合计
                            ,KKC.FAVBQTYL--可用库存
                            ,CR.FREMAINSTOCKINQTY--采购在途
                            ,PJ.FINSTOCKJOINQTY--品检数量
                            ,TEF.FTAXPRICE--含税单价
                            ,TF.FBILLALLAMOUNT--含税总金额
                            ,TED.FDELIVERYDATE--要货日期
                            ,T.FDOCUMENTSTATUS
                            ,CG.FID as FCGID,CG.FENTRYID as FCGENTRYID
							,CG.FBILLNO as FCGBILLNO,CG.FSEQ,CG.FPURCHASERID,CG.FDELIVERYDATE as FJHRQ
                            ,TE.FPARENTSMALLID,TE.FSMALLID,TE.FNOTE,TE.FSUPPLIERREPLYDATE,TE.FSUPPLIERDESCRIPTIONS
                            ,CG.FDOCUMENTSTATUS as FCGDOC
                            ,TE.FPROJECTNO,TF.FRECCONDITIONID
                            from T_SAL_ORDERENTRY TE
                            left join T_SAL_ORDERENTRY_R TER on TE.FENTRYID=TER.FENTRYID
                            left join T_SAL_ORDERENTRY_F TEF on TE.FENTRYID=TEF.FENTRYID
                            left join T_SAL_ORDERENTRY_D TED on TE.FENTRYID=TED.FENTRYID
                            left join T_SAL_ORDER T on TE.FID=T.FID
                            left join T_SAL_ORDERFIN TF on T.FID=TF.FID
                            left join T_BD_MATERIAL BDM on TE.FMATERIALID=BDM.FMATERIALID
                            left join T_BD_MATERIAL_L BDML on TE.FMATERIALID=BDML.FMATERIALID
                            left join T_BD_CUSTOMER CUS on T.FCUSTID=CUS.FCUSTID
                            left join V_BD_SALESMAN SALM on T.FSALERID=SALM.fid
                            left join (
                            select m.FMATERIALID,org.FORGID
                            ,Sum(t1.FBASEQTY) FBASEQTY--库存量
                            ,SUM(t1.FAVBQTY) FAVBQTY --可用库存
                            ,ISNULL(SUM(t1.FAVBQTY),0)+ISNULL(SUM(t2.FBASELOCKQTY),0) FAVBQTYL --可用库存+预留量
                            from V_STK_INVENTORY_CUS t1
                            inner join T_ORG_ORGANIZATIONS org on t1.FSTOCKORGID=org.FORGID
                            inner join T_BD_MATERIAL m on t1.FMATERIALID=m.FMASTERID
                            LEFT JOIN 
                            (
                            SELECT  TKE.FMATERIALID ,TKE.FSTOCKID,TKE.FSUPPLYORGID,
                            SUM(ISNULL(TKE.FBASEQTY, 0)) AS FBASELOCKQTY
                            FROM    T_PLN_RESERVELINKENTRY TKE
                            left join T_PLN_RESERVELINK TK ON TKE.FID=TK.FID
                            WHERE   TK.FRESERVETYPE=1
                            AND TKE.FBASEQTY > 0
                            GROUP BY TKE.FMATERIALID ,TKE.FSTOCKID,TKE.FSUPPLYORGID
                            ) t2 on m.FMATERIALID=t2.FMATERIALID and t1.FSTOCKID=t2.FSTOCKID and t1.FSTOCKORGID=t2.FSUPPLYORGID
                            group by m.FMATERIALID,org.FORGID
                            ) KKC on t.FSALEORGID=kkc.FORGID and te.FMATERIALID=kkc.FMATERIALID
                            LEFT JOIN (
                            select
                            SUM(FREMAINOUTQTY) FREMAINOUTQTY,
                            t3.FSALEORGID,
                            t2.FMATERIALID
                            from
                            T_SAL_ORDERENTRY_R t1
                            inner join T_SAL_ORDERENTRY t2 on t1.FENTRYID = t2.FENTRYID
                            inner join T_SAL_ORDER t3 on t1.FID = t3.FID
                            where
                            t3.FDOCUMENTSTATUS = 'C'
                            group by
                            FSALEORGID,
                            t2.FMATERIALID
                            ) NC ON NC.FSALEORGID = T.FSALEORGID
                            and NC.FMATERIALID = TE.FMATERIALID
                            LEFT JOIN (
                            select
                            SUM(FREMAINSTOCKINQTY) FREMAINSTOCKINQTY,
                            t3.FPURCHASEORGID,
                            t2.FMATERIALID
                            from
                            T_PUR_POORDERENTRY_R t1
                            inner join T_PUR_POORDERENTRY t2 on t1.FENTRYID = t2.FENTRYID
                            inner join T_PUR_POORDER t3 on t1.FID = t3.FID
                            where
                            t3.FDOCUMENTSTATUS = 'C'
                            group by
                            t3.FPURCHASEORGID,
                            t2.FMATERIALID
                            ) CR ON CR.FPURCHASEORGID = T.FSALEORGID
                            and CR.FMATERIALID = TE.FMATERIALID
                            LEFT JOIN (
                            select
                            SUM(t2.FACTRECEIVEQTY) - SUM(t1.FINSTOCKJOINQTY) FINSTOCKJOINQTY,
                            t3.FSTOCKORGID,
                            t2.FMATERIALID
                            from
                            T_PUR_RECEIVEENTRY_S t1
                            inner join T_PUR_ReceiveEntry t2 on t1.FENTRYID = t2.FENTRYID
                            inner join T_PUR_Receive t3 on t1.FID = t3.FID
                            where
                            t3.FDOCUMENTSTATUS = 'C'
                            and t2.FCHECKINCOMING = 1
                            group by
                            t3.FSTOCKORGID,
                            t2.FMATERIALID
                            ) PJ ON PJ.FSTOCKORGID = T.FSALEORGID
                            and PJ.FMATERIALID = TE.FMATERIALID
                            LEFT JOIN (
                            select t1.FID,t1.FENTRYID,t4.FBILLNO,t1.FSEQ,t4.FPURCHASERID,t3.FDELIVERYDATE,t4.FDOCUMENTSTATUS
                            ,t2.FDEMANDBILLENTRYID
                            from
                            T_PUR_POORDERENTRY t1
                            left join T_PUR_POORDERENTRY_R t2 on t1.FENTRYID=t2.FENTRYID
                            left join T_PUR_POORDERENTRY_D t3 on t1.FENTRYID=t3.FENTRYID
                            left join T_PUR_POORDER t4 on t1.FID=t4.FID
                            ) CG on TE.FENTRYID=CG.FDEMANDBILLENTRYID
                            LEFT JOIN (
							SELECT MAX(t3.FAPPROVEDATE) max_date,t2.FSBILLID,t2.FSID FROM T_SAL_DELIVERYNOTICEENTRY t1
							LEFT JOIN T_SAL_DELIVERYNOTICEENTRY_LK  t2 on t1.FENTRYID=t2.FENTRYID
							LEFT JOIN T_SAL_DELIVERYNOTICE t3 on t1.FID=t3.FID
							GROUP BY t2.FSBILLID,t2.FSID
							) YQ on T.FID=YQ.FSBILLID and TE.FENTRYID=YQ.FSID
                            where T.FSALEORGID={this.Context.CurrentOrganizationInfo.ID}
                            {spWhere}
                            order by T.FDATE,T.FBILLNO,TE.FSEQ
                            ";
                var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                this.Model.BeginIniti();
                this.Model.DeleteEntryData("FSalesEntity");
                foreach (var item in datas)
                {
                    int rowcount = this.Model.GetEntryRowCount("FSalesEntity");
                    this.Model.CreateNewEntryRow("FSalesEntity");
                    this.Model.SetValue("FSalesOrderDate", item["FDATE"], rowcount);
                    this.Model.SetValue("FSalesOrderNo", item["FBILLNO"], rowcount);
                    this.Model.SetItemValueByID("FCustomerID", item["FCUSTID"], rowcount);
                    this.Model.SetItemValueByID("FSALERID", item["FSALERID"], rowcount);
                    this.Model.SetItemValueByID("FMATERIALID", item["FMATERIALID"], rowcount);
                    this.Model.SetItemValueByID("FMapId", item["FMapId"], rowcount);
                    this.Model.SetValue("FCUSTMATERIALNO", item["FCUSTMATERIALNO"], rowcount);
                    this.Model.SetValue("FQTY", item["FQTY"], rowcount);
                    this.Model.SetValue("FSTOCKOUTQTY", item["FSTOCKOUTQTY"], rowcount);
                    this.Model.SetValue("FREMAINOUTQTY", item["FREMAINOUTQTY"], rowcount);
                    this.Model.SetValue("FREMAINOUTQTYS", item["FREMAINOUTQTYS"], rowcount);
                    this.Model.SetValue("FAVBQTYL", item["FAVBQTYL"], rowcount);
                    this.Model.SetValue("FREMAINSTOCKINQTY", item["FREMAINSTOCKINQTY"], rowcount);
                    this.Model.SetValue("FINSTOCKJOINQTY", item["FINSTOCKJOINQTY"], rowcount);
                    this.Model.SetValue("FTAXPRICE", item["FTAXPRICE"], rowcount);
                    this.Model.SetValue("FBILLALLAMOUNT", item["FBILLALLAMOUNT"], rowcount);
                    this.Model.SetValue("FDELIVERYDATE", item["FDELIVERYDATE"], rowcount);
                    this.Model.SetValue("FDOCUMENTSTATUS", item["FDOCUMENTSTATUS"], rowcount);
                    this.Model.SetValue("FBILLNO", item["FCGBILLNO"], rowcount);
                    this.Model.SetValue("FSEQ", item["FSEQ"], rowcount);
                    this.Model.SetItemValueByID("FPURCHASERID", item["FPURCHASERID"], rowcount);
                    this.Model.SetValue("FJHRQ", item["FJHRQ"], rowcount);
                    this.Model.SetItemValueByID("FPARENTSMALLID", item["FPARENTSMALLID"], rowcount);
                    this.Model.SetItemValueByID("FSMALLID", item["FSMALLID"], rowcount);
                    this.Model.SetValue("FNOTE", item["FNOTE"], rowcount);
                    this.Model.SetValue("FSUPPLIERREPLYDATE", item["FSUPPLIERREPLYDATE"], rowcount);
                    this.Model.SetValue("FSUPPLIERDESCRIPTIONS", item["FSUPPLIERDESCRIPTIONS"], rowcount);
                    this.Model.SetValue("FCGDOC", item["FCGDOC"], rowcount);
                    this.Model.SetValue("FPROJECTNO", item["FPROJECTNO"], rowcount);
                    this.Model.SetItemValueByID("FRECCONDITIONID", item["FRECCONDITIONID"], rowcount);
                    this.Model.SetValue("FCGID", item["FCGID"], rowcount);
                    this.Model.SetValue("FCGENTRYID", item["FCGENTRYID"], rowcount);
                    this.Model.SetValue("FXSID", item["FXSID"], rowcount);
                    this.Model.SetValue("FXSENTRYID", item["FXSENTRYID"], rowcount);
                }
                this.View.UpdateView("FSalesEntity");
                this.Model.EndIniti();
                isFinsh = true;
            }
        }
        public override void EntityRowClick(EntityRowClickEventArgs e)
        {
            base.EntityRowClick(e);
            if (e.Key.EqualsIgnoreCase("FSALESENTITY"))
            {
                if (!isFinsh) return;
                var BillEntityRowIndex = this.View.Model.GetEntryCurrentRowIndex("FSALESENTITY");

                DynamicObject selectedEntityObj = (this.View.Model.DataObject["FSALESENTITY"] as DynamicObjectCollection)[BillEntityRowIndex];
                var FCGID = Convert.ToString(selectedEntityObj["FCGID"]);
                var FCGENTRYID = Convert.ToString(selectedEntityObj["FCGENTRYID"]);

                string sSql = $@"SELECT
	                                t3.FDATE,t2.FSTOCKID,t3.FBUSINESSTYPE,t2.FREALQTY
                                FROM
	                                T_STK_INSTOCKENTRY_LK t1
	                                LEFT JOIN T_STK_INSTOCKENTRY t2 ON t1.FENTRYID= t2.FENTRYID 
	                                LEFT JOIN T_STK_INSTOCK t3 ON t2.FID=t3.FID
                                where
	                                t1.FSBILLID= {FCGID} 
	                                AND t1.FSID={FCGENTRYID}";
                var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                this.Model.DeleteEntryData("FRKEntity");
                if (datas != null)
                {
                    foreach (var item in datas)
                    {
                        int rowcount = this.Model.GetEntryRowCount("FRKEntity");
                        this.Model.CreateNewEntryRow("FRKEntity");
                        this.Model.SetValue("F_PENY_Date3", item["FDATE"].ToString(), rowcount);
                        this.Model.SetValue("F_PENY_Base", item["FSTOCKID"].ToString(), rowcount);
                        this.Model.SetValue("F_PENY_Combo3", item["FBUSINESSTYPE"].ToString(), rowcount);
                        this.Model.SetValue("F_PENY_Decimal", item["FREALQTY"].ToString(), rowcount);
                    }
                    this.View.UpdateView("FRKEntity");
                }

                var FXSID = Convert.ToString(selectedEntityObj["FXSID"]);
                var FXSENTRYID = Convert.ToString(selectedEntityObj["FXSENTRYID"]);
                sSql = $@"SELECT
	                                t5.FBILLNO,
	                                t4.FSEQ,
	                                t5.FBILLTYPEID,
	                                t4.FREALQTY,
	                                t5.FAPPROVEDATE,
	                                t5.FDOCUMENTSTATUS,
	                                t6.FLOGCOMID,
	                                t6.FCARRYBILLNO,
	                                t4.FNOTE,
	                                t5.FPICKINGCOMPLETEDATE,
	                                t5.FPACKAGINGCOMPLETEDATE,
	                                t5.FLOGISTICSRECEIVINGDATE,
	                                t5.FCUSTRECEIVINGDATE 
                                FROM
	                                T_SAL_DELIVERYNOTICEENTRY_LK t1
	                                INNER JOIN T_SAL_DELIVERYNOTICEENTRY t2 ON t1.FENTRYID= t2.FENTRYID
	                                INNER JOIN T_SAL_OUTSTOCKENTRY_LK t3 ON t2.FID= t3.FSBILLID 
	                                AND t2.FENTRYID= t3.FSID
	                                INNER JOIN T_SAL_OUTSTOCKENTRY t4 ON t3.FENTRYID= t4.FENTRYID
	                                INNER JOIN T_SAL_OUTSTOCK t5 ON t4.FID= t5.FID
	                                LEFT JOIN T_SAL_OUTSTOCKTRACE t6 ON t5.FID= t6.FID 
                                WHERE
	                                t1.FSBILLID= {FXSID} 
	                                AND t1.FSID= {FXSENTRYID}";
                datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                this.Model.DeleteEntryData("FCKEntity");
                if (datas != null)
                {
                    foreach (var item in datas)
                    {
                        int rowcount = this.Model.GetEntryRowCount("FCKEntity");
                        this.Model.CreateNewEntryRow("FCKEntity");
                        this.Model.SetValue("F_PENY_Text6", item["FBILLNO"].ToString(), rowcount);
                        this.Model.SetValue("F_PENY_Text7", item["FSEQ"].ToString(), rowcount);
                        this.Model.SetValue("F_PENY_Text8", item["FBILLTYPEID"].ToString(), rowcount);
                        this.Model.SetValue("F_PENY_Decimal1", item["FREALQTY"].ToString(), rowcount);

                        this.Model.SetValue("F_PENY_Date4", item["FAPPROVEDATE"].ToString(), rowcount);
                        this.Model.SetValue("F_PENY_Text9", item["FDOCUMENTSTATUS"].ToString(), rowcount);
                        this.Model.SetItemValueByID("F_PENY_Base1", item["FLOGCOMID"].ToString(), rowcount);
                        this.Model.SetValue("F_PENY_Text10", item["FCARRYBILLNO"] is null ? "" : item["FCARRYBILLNO"].ToString(), rowcount);
                        this.Model.SetValue("F_PENY_Text12", item["FNOTE"].ToString(), rowcount);
                        this.Model.SetValue("F_PENY_Datetime", item["FPICKINGCOMPLETEDATE"].ToString(), rowcount);
                        this.Model.SetValue("F_PENY_Datetime1", item["FPACKAGINGCOMPLETEDATE"].ToString(), rowcount);
                        this.Model.SetValue("F_PENY_Datetime2", item["FLOGISTICSRECEIVINGDATE"].ToString(), rowcount);
                        this.Model.SetValue("F_PENY_Datetime3", item["FCUSTRECEIVINGDATE"].ToString(), rowcount);
                    }
                    this.View.UpdateView("FCKEntity");
                }

            }
        }
    }
}
