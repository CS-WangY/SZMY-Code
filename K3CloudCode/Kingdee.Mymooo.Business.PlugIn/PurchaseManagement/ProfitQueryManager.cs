using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace Kingdee.Mymooo.Business.PlugIn.PurchaseManagement
{
    [Description("毛利查询-产品经理-动态表单"), HotUpdate]
    public class ProfitQueryManager : AbstractDynamicFormPlugIn
    {
        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);
            if (e.Key.Equals("FQueryButton", StringComparison.OrdinalIgnoreCase))
            {
                string managerNumber = this.Model.GetValue("FManagerNumber") == null ? "" : this.Model.GetValue("FManagerNumber").ToString();
                string managerName = this.Model.GetValue("FManagerName") == null ? "" : this.Model.GetValue("FManagerName").ToString();
                string engineerNumber = this.Model.GetValue("FEngineerNumber") == null ? "" : this.Model.GetValue("FEngineerNumber").ToString();
                string engineerName = this.Model.GetValue("FEngineerName") == null ? "" : this.Model.GetValue("FEngineerName").ToString();
                string sDate = this.Model.GetValue("FSDate") == null ? "" : this.Model.GetValue("FSDate").ToString();
                string eDate = this.Model.GetValue("FEDate") == null ? "" : ((DateTime)this.Model.GetValue("FEDate")).AddDays(1).AddSeconds(-1).ToString();
                var sSql = $@"select mhe.FID as FManagerid,
                            sum(soef.FAMOUNT) as FAmount,
                            sum(soef.FALLAMOUNT) as FAllAmount,
                            sum(ISNULL(t5.FALLAMOUNT,0)) as FCostAmount,
							sum(soef.FAMOUNT)-sum(ISNULL(t5.FALLAMOUNT,0)) as FGPAmount,
							sum(soef.FALLAMOUNT)-sum(ISNULL(t5.FALLAMOUNT,0)) as FGPAllAmount
                            from T_SAL_ORDERENTRY_F soef
                            left join T_SAL_ORDERENTRY soe on soe.FENTRYID=soef.FENTRYID
                            left join T_HR_EMPINFO mhe on soe.FPRODUCTMANAGERID=mhe.FID
                            left join T_HR_EMPINFO_L mhel on mhe.FID=mhel.FID
                            left join T_SAL_ORDER so on soe.FID=so.FID
							left join T_PUR_POORDERENTRY_R t3 on so.FBILLNO=t3.FDEMANDBILLNO and soe.FENTRYID=t3.FDEMANDBILLENTRYID
							left join T_PUR_POORDERENTRY t4 on t3.FENTRYID=t4.FENTRYID
							left join T_PUR_POORDERENTRY_F t5 on t4.FENTRYID=t5.FENTRYID
                            where
                            so.FAPPROVEDATE between '{sDate}' and '{eDate}'";

                //mhe.FNUMBER like '%{managerNumber}%'
                //and mhel.FNAME like '%{managerName}%'
                if (managerNumber != "")
                {
                    sSql += " and mhe.FNUMBER like '%{managerNumber}%'";
                }
                if (managerName != "")
                {
                    sSql += " and mhel.FNAME like '%{managerName}%'";
                }
                sSql += " group by mhe.FID";
                var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);

                this.Model.DeleteEntryData("FManagerEntity");
                foreach (var item in datas)
                {
                    int rowcount = this.Model.GetEntryRowCount("FManagerEntity");
                    this.Model.CreateNewEntryRow("FManagerEntity");
                    this.Model.SetValue("FManagerid", item["FManagerid"].ToString(), rowcount);
                    this.Model.SetValue("FAmount", item["FAmount"].ToString(), rowcount);
                    this.Model.SetValue("FAllamount", item["FAllamount"].ToString(), rowcount);
                    this.Model.SetValue("FCostAmount", item["FCostAmount"].ToString(), rowcount);
                    this.Model.SetValue("FGPAmount", item["FGPAmount"].ToString(), rowcount);
                    this.Model.SetValue("FGPAllAmount", item["FGPAllAmount"].ToString(), rowcount);
                }
                this.View.UpdateView("FManagerEntity");
            }
        }
        public override void EntityRowClick(EntityRowClickEventArgs e)
        {
            base.EntityRowClick(e);
            string sDate = this.Model.GetValue("FSDate") == null ? "" : this.Model.GetValue("FSDate").ToString();
            string eDate = this.Model.GetValue("FEDate") == null ? "" : ((DateTime)this.Model.GetValue("FEDate")).AddDays(1).AddSeconds(-1).ToString();
            if (e.Key.Equals("FManagerEntity", StringComparison.OrdinalIgnoreCase))
            {
                var manid = this.Model.GetValue("FManagerid", e.Row) as DynamicObject;
                var sSql = $@"select mhe.FID as FEngineerid,
                        sum(soef.FAMOUNT) as FEAmount,
                        sum(soef.FALLAMOUNT) as FEAllAmount,
                        sum(ISNULL(t5.FALLAMOUNT,0)) as FECostAmount,
						sum(soef.FAMOUNT)-sum(ISNULL(t5.FALLAMOUNT,0)) as FEGPAmount,
						sum(soef.FALLAMOUNT)-sum(ISNULL(t5.FALLAMOUNT,0)) as FEGPAllAmount
                        from T_SAL_ORDERENTRY_F soef
                        left join T_SAL_ORDERENTRY soe on soe.FENTRYID=soef.FENTRYID
                        left join T_HR_EMPINFO mhe on soe.FPRODUCTENGINEERID=mhe.FID
                        left join T_HR_EMPINFO_L mhel on mhe.FID=mhel.FID
                        left join T_SAL_ORDER so on soe.FID=so.FID
						left join T_PUR_POORDERENTRY_R t3 on so.FBILLNO=t3.FDEMANDBILLNO and soe.FENTRYID=t3.FDEMANDBILLENTRYID
                        left join T_PUR_POORDERENTRY t4 on t3.FENTRYID=t4.FENTRYID
						left join T_PUR_POORDERENTRY_F t5 on t4.FENTRYID=t5.FENTRYID
                        where
                        so.FAPPROVEDATE between '{sDate}' and '{eDate}'";
                if (manid is null)
                {
                    sSql += $" and soe.FPRODUCTMANAGERID=0";
                }
                else
                {
                    sSql += $" and soe.FPRODUCTMANAGERID={manid["Id"].ToString()}";
                }
                sSql += " group by mhe.FID";
                var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);

                this.Model.DeleteEntryData("FEngineerEntity");
                foreach (var item in datas)
                {
                    int rowcount = this.Model.GetEntryRowCount("FEngineerEntity");
                    this.Model.CreateNewEntryRow("FEngineerEntity");
                    this.Model.SetValue("FEngineerid", item["FEngineerid"].ToString(), rowcount);
                    this.Model.SetValue("FEAmount", item["FEAmount"].ToString(), rowcount);
                    this.Model.SetValue("FEAllAmount", item["FEAllAmount"].ToString(), rowcount);
                    this.Model.SetValue("FECostAmount", item["FECostAmount"].ToString(), rowcount);
                    this.Model.SetValue("FEGPAmount", item["FEGPAmount"].ToString(), rowcount);
                    this.Model.SetValue("FEGPAllAmount", item["FEGPAllAmount"].ToString(), rowcount);
                }
                this.View.UpdateView("FEngineerEntity");
            }
            if (e.Key.Equals("FEngineerEntity", StringComparison.OrdinalIgnoreCase))
            {
                var manrow = this.Model.GetEntryCurrentRowIndex("FManagerEntity");
                var manid = this.Model.GetValue("FManagerid", manrow) as DynamicObject;
                var engid = this.Model.GetValue("FEngineerid", e.Row) as DynamicObject;

                var sSql = $@"select t2.FBILLNO,t1.FSEQ,t1.FMATERIALID,
                        t1.FQTY,ISNULL(t4.FQTY,0) as FCKQTY,
                        t1f.FAMOUNT as FXSWS,t1f.FALLAMOUNT as FXSHS,
                        t5.FALLAMOUNT as FCost,
                        ISNULL(t1f.FAMOUNT,0)-ISNULL(t5.FALLAMOUNT,0) as FGP,
                        ISNULL(t1f.FALLAMOUNT,0)-ISNULL(t5.FALLAMOUNT,0) as FGPAll
                        from T_SAL_ORDERENTRY t1
                        left join T_SAL_ORDERENTRY_F t1f on t1.FENTRYID=t1f.FENTRYID
                        left join T_SAL_ORDER t2 on t1.FID=t2.FID
                        left join T_PUR_POORDERENTRY_R t3 on t2.FBILLNO=t3.FDEMANDBILLNO and t1.FENTRYID=t3.FDEMANDBILLENTRYID
                        left join T_PUR_POORDERENTRY t4 on t3.FENTRYID=t4.FENTRYID
						left join T_PUR_POORDERENTRY_F t5 on t4.FENTRYID=t5.FENTRYID
                        where 
                        t2.FAPPROVEDATE between '{sDate}' and '{eDate}'";
                if (manid is null)
                {
                    sSql += $" and t1.FPRODUCTMANAGERID=0";
                }
                else
                {
                    sSql += $" and t1.FPRODUCTMANAGERID={manid["Id"].ToString()}";
                }
                if (engid is null)
                {
                    sSql += $" and t1.FPRODUCTENGINEERID=0";
                }
                else
                {
                    sSql += $" and t1.FPRODUCTENGINEERID={engid["Id"].ToString()}";
                }
                var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);

                this.Model.DeleteEntryData("FOrderEntity");
                foreach (var item in datas)
                {
                    int rowcount = this.Model.GetEntryRowCount("FOrderEntity");
                    this.Model.CreateNewEntryRow("FOrderEntity");
                    this.Model.SetValue("FBILLNO", item["FBILLNO"].ToString(), rowcount);
                    this.Model.SetValue("FSEQ", item["FSEQ"].ToString(), rowcount);
                    this.Model.SetValue("FMATERIALID", item["FMATERIALID"].ToString(), rowcount);
                    this.Model.SetValue("FQTY", item["FQTY"].ToString(), rowcount);
                    this.Model.SetValue("FCKQTY", item["FCKQTY"].ToString(), rowcount);
                    this.Model.SetValue("FXSWS", item["FXSWS"].ToString(), rowcount);
                    this.Model.SetValue("FXSHS", item["FXSHS"].ToString(), rowcount);
                    this.Model.SetValue("FCost", item["FCost"].ToString(), rowcount);
                    this.Model.SetValue("FGP", item["FGP"].ToString(), rowcount);
                    this.Model.SetValue("FGPAll", item["FGPAll"].ToString(), rowcount);
                }
                this.View.UpdateView("FOrderEntity");
            }


        }
    }
}
