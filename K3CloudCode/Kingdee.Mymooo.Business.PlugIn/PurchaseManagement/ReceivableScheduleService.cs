using Kingdee.BOS;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.PurchaseManagement;
using Kingdee.Mymooo.ServiceHelper.SalesManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.PurchaseManagement
{
    /// <summary>
    /// 采购执行计划生成应付单
    /// </summary>
    public class ReceivableScheduleService : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            var context = LoginServiceUtils.BackgroundLogin(ctx);
            string sSql = $@"select t2.FBILLNO,t1.FID,t1.FENTRYID,t4.FNUMBER,t1.FCHANGEFLAG from T_PUR_POORDERENTRY t1
                            inner join T_PUR_POORDER t2 on t1.FID=t2.FID
                            inner join T_PUR_POORDERFIN rn on rn.FID=t2.FID
                            left join T_BD_RECCONDITION re on re.FID=rn.FPAYCONDITIONID
                            inner join T_ORG_ORGANIZATIONS t3 on t2.FPURCHASEORGID=t3.FORGID
                            inner join T_BD_MATERIAL t4 on t1.FMATERIALID=t4.FMATERIALID
                            where t3.FNUMBER='SZMYGC' and t2.FDOCUMENTSTATUS='C' and t1.FCHANGEFLAG<>'I' and re.FNUMBER='501'
                            and not exists(select FENTRYID from T_AP_PAYABLE_LK where FSBILLID=t1.FID and FSID=t1.FENTRYID)";
            var reader = DBServiceHelper.ExecuteDynamicObject(context, sSql);
            if (!(reader is null))
            {
                List<PurchaseOrderPushEntity> entry = new List<PurchaseOrderPushEntity>();
                foreach (var item in reader)
                {
                    entry.Add(new PurchaseOrderPushEntity
                    {
                        FID = Convert.ToInt64(item["FID"]),
                        FEntryID = Convert.ToInt64(item["FEntryID"])
                    });
                }
                PurchaseOrderServiceHelper.PurchaseToReceivable(context, entry, context.CurrentOrganizationInfo.ID);
            }
        }
    }
}
