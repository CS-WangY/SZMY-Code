using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Util;
using Kingdee.BOS.Core;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Core.Validation;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.PLN.ParamOption;
using Kingdee.K3.MFG.App;
using Kingdee.K3.MFG.Contracts.PLN;
using Kingdee.K3.Core.MFG;
using Kingdee.K3.FIN.Core;
using System.ComponentModel;

namespace Kingdee.Mymooo.ServicePlugIn.MrpModel.MrpCalingValidator
{
    /// <summary>
    /// 是否正在MRP运算校验，调用本校验器的单据操作插件须加载组织，单据编号和单据类型字段
    /// </summary>
    public class IsMrpCalingValidator : AbstractValidator
    {
        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            if (dataEntities.IsEmpty()) return;
            //AppServiceContext在引用组件Kingdee.K3.MFG.App.dll后可用
            //如发现下面一句获取的正在计划运算信息有误，则应清除运算冲突，路径：计划管理》运算日志查询》冲突冲突

            MrpCalculatingInfo mrpCalculatingInfo =
                AppServiceContext.GetService<IMrpLogExtService>().GetMrpCalculatingInfo(ctx);
            foreach (ExtendedDataEntity DataItem in dataEntities)
            {
                DynamicObject billData = DataItem.DataEntity;
                long orgId = 0;
                switch (billData.GetValue<string>("FFormId").ToUpper())
                {
                    case "SAL_OUTSTOCK":
                        orgId = billData.GetDynamicValue<long>("SaleOrgId_Id");
                        break;
                    case "STK_TRANSFEROUT":
                    case "STK_TRANSFERIN":
                        orgId = billData.GetDynamicValue<long>("StockOrgID_Id");
                        break;
                }
                if (mrpCalculatingInfo.DemandOrgIds.Contains(orgId))
                { //如果只校验组织，则这里已经可以判断预测单不能通过本校验器了
                  //string billTypeId = billData.GetDynamicValue<string>("BillTypeID_Id");
                  //HashSet<string> billTypeIds;
                  //if (mrpCalculatingInfo.DemandFormIdBillTypeMaps.TryGetValue(MFGFormIdConst.SubSys_PLN.ForecastOrder, out billTypeIds)
                  //    && billTypeIds.Contains(billTypeId))
                  //{//校验本单的单据类型是参与运算的，则不能通过本校验器
                    string billNo = billData.GetDynamicValue<string>("BillNo");
                    string errMsg = string.Format("{0}订单审核失败，MRP正在运算中，请稍后再试!", billNo);
                    validateContext.AddError(billData,
                        new ValidationErrorInfo("",
                            billData.GetDynamicValue<string>("Id"),
                            DataItem.DataEntityIndex,
                            DataItem.RowIndex,
                            "MrpCalingError",
                            errMsg,
                            "MRPCalculating"));
                    //}
                }
            }
        }
    }
}
