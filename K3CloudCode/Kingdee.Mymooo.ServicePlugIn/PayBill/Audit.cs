using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.App.Core;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.PayBill
{
    [Description("付款单审核插件"), HotUpdate]
    public class Audit : AbstractOperationServicePlugIn
    {
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            foreach (var headEntity in e.DataEntitys)
            {
                foreach (var item in headEntity["BILLRECEIVABLEENTRY"] as DynamicObjectCollection)
                {
                    var receivebleid = Convert.ToInt64(item["ReceivebleBillId_Id"]);
                    MymoooBusinessDataService service = new MymoooBusinessDataService();
                    var billView = FormMetadataUtils.CreateBillView(this.Context, "CN_BILLRECEIVABLE", receivebleid);
                    billView.Model.CreateNewEntryRow("FEndorseEntity");
                    var entitycount = billView.Model.GetEntryRowCount("FEndorseEntity") - 1;
                    var orsedate = headEntity["DATE"];
                    var drawer = billView.Model.GetValue("FDRAWER");
                    billView.Model.SetValue("FENDORSEDATE", orsedate, entitycount);
                    billView.Model.SetValue("FENDORSER", drawer, entitycount);
                    List<DynamicObject> dynamicObjects = new List<DynamicObject>();
                    dynamicObjects.Add(billView.Model.DataObject);
                    var oper = service.SaveBill(this.Context, billView.BusinessInfo, dynamicObjects.ToArray());
                    //清除释放网控
                    billView.CommitNetworkCtrl();
                    billView.InvokeFormOperation(FormOperationEnum.Close);
                    billView.Close();
                    if (!oper.IsSuccess)
                    {
                        if (oper.ValidationErrors.Count > 0)
                        {
                            throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
                        }
                        else
                        {
                            throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                        }
                    }

                    //throw new Exception("1");
                }
            }
        }
    }
}
