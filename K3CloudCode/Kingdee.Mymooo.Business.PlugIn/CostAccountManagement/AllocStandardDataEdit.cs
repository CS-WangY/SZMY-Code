using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.CostAccountManagement
{
    [Description("自定义费用分配标准值维护单据插件"), HotUpdate]
    public class AllocStandardDataEdit : AbstractBillPlugIn
    {
        public override void EntryBarItemClick(BarItemClickEventArgs e)
        {
            base.EntryBarItemClick(e);

            if (e.BarItemKey.EqualsIgnoreCase("PENY_tbGetSalesAmount"))
            {
                var year = this.View.Model.GetValue<int>("FYEAR", 0, 0);
                var period = this.View.Model.GetValue<int>("FPERIOD", 0, 0);
                if (year == 0 || period == 0)
                {
                    this.View.ShowMessage("请先选择需要获取的会计期间!");
                    return;
                }
                var accOrg = this.View.Model.GetValue("FACCTGORGID") as DynamicObject;
                if (accOrg == null)
                {
                    this.View.ShowMessage("请先选择核算组织!");
                    return;
                }
                var accSystem = this.View.Model.GetValue("FACCTGSYSTEMID") as DynamicObject;
                if (accSystem == null)
                {
                    this.View.ShowMessage("请先选择核算体系!");
                    return;
                }

                var sql = @"SELECT t0.FNUMBER fnumber, t0.FSEQ fseq, t0.FDATE fdate, t0.FPRODUCTID fproductid_id, t0.FMATERIALID fmaterialid_id, t0.FUNITID funitid_id, t0.FWORKSHOPID fworkshopid_id, t0.FCOSTCENTERID fcostcenterid_id, t0.FCREATEORGID fcreateorgid_id, t0.FUSEORGID fuseorgid_id, t0.FDOCUMENTSTATUS 
fdocumentstatus, t0.FPROORDERTYPE fproordertype, t0.FFORBIDSTATUS fforbidstatus, t0.FENTRYID fentryid,isnull(oe.FALLAMOUNT,0) FAmount
FROM T_CB_PROORDERTYPE t0
	inner join T_PRD_MOENTRY e on t0.FPROORDERENTRYID = e.FENTRYID
	inner join T_ORG_ACCTSYSDETAIL accd on t0.FUSEORGID = accd.FSUBORGID
	inner join T_ORG_ACCTSYSENTRY acce on accd.FENTRYID = acce.FENTRYID and acce.FMAINORGID = @FMAINORGID and acce.FACCTSYSTEMID = @FACCTSYSTEMID
	left join T_SAL_ORDERENTRY_F oe on e.FSALEORDERENTRYID = oe.FENTRYID
WHERE t0.FPROORDERTYPE = 'PO' 
AND EXISTS (SELECT 1 FROM (
				SELECT orderTy.FEntryID 
				FROM T_CB_PROORDERTYPE orderTy 
				WHERE orderTy.FPROORDERTYPE = 'PO' AND orderTy.FDate >= @FStartDate AND orderTy.FDate < @FEndDate
				UNION ALL 
				SELECT orderTy.FEntryID 
				FROM T_CB_PROORDERINFO t1 
					INNER JOIN T_CB_PROORDERDIME t2 ON t1.fproductdimeid = t2.fproductdimeid AND t1.fendinitkey = 0 AND t1.facctgid = @FMAINORGID
					INNER JOIN t_bd_Material MAL ON MAL.FMASTERID = t2.fproductid 
					INNER JOIN T_CB_PROORDERTYPE orderTy ON orderTy.FPRODUCTID = MAL.FMATERIALID AND orderTy.FPROORDERTYPE = t2.FPROORDERTYPE AND orderTy.FCOSTCENTERID = t2.FCOSTCENTERID AND orderTy.FNUMBER = t2.fproductno AND orderTy.FSEQ = t2.fbillseq
				WHERE t1.FBUSINESSSTATUS NOT IN ('6', '7') AND orderTy.FPROORDERTYPE IN ('PO')) tmp 
			WHERE TMP.FEntryID = t0.FENTRYID)";

                var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sql, paramList: new SqlParam[]
                {
                    new SqlParam("@FMAINORGID", KDDbType.Int64, accOrg["Id"]),
                    new SqlParam("@FACCTSYSTEMID", KDDbType.Int64, accSystem["Id"]),
                    new SqlParam("@FStartDate", KDDbType.Date, new DateTime(year, period, 1)),
                    new SqlParam("@FEndDate", KDDbType.Date, new DateTime(year, period, 1).AddMonths(1)),
                });

                this.View.Model.DeleteEntryData("FEntity");
                int rowIndex = 0;
                foreach (var data in datas)
                {
                    this.View.Model.CreateNewEntryRow("FEntity");
                    this.View.Model.SetItemValueByNumber("FSTANDARDID", "CPXSE", rowIndex);
                    this.View.InvokeFieldUpdateService("FSTANDARDID", rowIndex);
                    this.View.Model.SetValue("FCOSTCENTERID", data["fcostcenterid_id"], rowIndex);
                    this.View.Model.SetValue("FPROORDERTYPE", "PO", rowIndex);
                    this.View.Model.SetValue("FPROORDERID", data["fentryid"], rowIndex);
                    this.View.InvokeFieldUpdateService("FPROORDERID", rowIndex);
                    this.View.Model.SetValue("FQTY", data["FAmount"], rowIndex);
                    rowIndex++;
                }
                this.View.UpdateView("FEntity");
            }
        }
    }
}
