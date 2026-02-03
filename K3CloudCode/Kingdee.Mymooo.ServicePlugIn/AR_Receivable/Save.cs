using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Util;
using System.ComponentModel;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS;
using Kingdee.Mymooo.Core.Utils;
using System.Xml.Linq;

namespace Kingdee.Mymooo.ServicePlugIn.AR_Receivable
{
    [Description("应收单保存插件"), HotUpdate]
    public class Save : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("F_PENY_ISAPPROVAL"); //是否需要审批
            e.FieldKeys.Add("F_PENY_DIFFALLAMOUNT");//价税差额
            e.FieldKeys.Add("F_PENY_ALLAMOUNT");//原价税合计
            e.FieldKeys.Add("FALLAMOUNTFOR_D");//价税合计
            e.FieldKeys.Add("FComment");//明细备注
            e.FieldKeys.Add("FTaxPrice");//含税单价
            e.FieldKeys.Add("FPriceQty");//计价数量
            e.FieldKeys.Add("F_PENY_TaxPrice");//原含税单价

        }
        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);

            if (this.Option.TryGetVariableValue<bool>("RemoveValidators", out bool validator) && validator)
            {
                e.Validators.Clear();
            }
            SaveValidator isPoValidator = new SaveValidator();
            isPoValidator.AlwaysValidate = true;
            isPoValidator.EntityKey = "FBillHead";
            e.Validators.Add(isPoValidator);
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            foreach (var item in e.DataEntitys)
            {
                //价税差额大于20，更新表头是否需要审批
                var fid = Convert.ToInt64(item["Id"]);
                string sSql = $"select top 1 FENTRYID from T_AR_RECEIVABLEENTRY where FID={fid} and F_PENY_DIFFALLAMOUNT>20";
                var entryId = DBUtils.ExecuteScalar<long>(this.Context, sSql, 0);
                if (entryId == 0)
                {
                    sSql = $"UPDATE t_AR_receivable SET F_PENY_ISAPPROVAL='0' WHERE FID={fid}";
                    DBUtils.Execute(this.Context, sSql);
                }
                else
                {
                    sSql = $"UPDATE t_AR_receivable SET F_PENY_ISAPPROVAL='1' WHERE FID={fid}";
                    DBUtils.Execute(this.Context, sSql);
                }
            }
        }
    }
}
