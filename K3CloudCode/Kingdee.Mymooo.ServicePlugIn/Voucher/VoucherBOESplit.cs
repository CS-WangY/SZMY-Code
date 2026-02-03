using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Kingdee.BOS;
using Kingdee.BOS.App.Core.Utils;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.Operation;
using Kingdee.BOS.Model.DynamicForm;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.Web.DynamicForm;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.FIN.Core;
using Kingdee.K3.FIN.Core.PlugIn;
using Kingdee.K3.FIN.Core.PlugIn.Arg;
using Kingdee.Mymooo.Core.Utils;

namespace Kingdee.Mymooo.ServicePlugIn.Voucher
{
    [Description("生成凭证-承兑汇票类型按背书拆分贷方明细"), HotUpdate]
    public class VoucherBOESplit : AbstractBuildVchSecondDevPlugIn
    {
        public override void AfterBookBuildVoucher(BuildVchSecondDevArgs e)
        {
            base.AfterBookBuildVoucher(e);
            if (e.GlVchIds.Count == 0)
            {
                return;
            }
            foreach (var vchid in e.GlVchIds)
            {
                // 获取付款单对应的应付票据和凭证号
                string sql = $@"SELECT t3.FBCONTACTUNIT,t3.FRETURNAMOUNT,t3.FBCONTACTUNITTYPE
                FROM T_GL_VOUCHER T0
                INNER JOIN T_BAS_VOUCHER T1 ON T1.FGLVOUCHERID=T0.FVOUCHERID
                INNER JOIN T_AP_PAYBILL T2 ON T2.FID=T1.FSOURCEBILLID
                LEFT JOIN T_CN_PAYBILLREC T3 ON T3.FID=T2.FID
                WHERE  T1.FSOURCEBILLKEY='AP_PAYBILL'
                AND T2.FPENYVoucherBOESplit=1
                AND T0.FVOUCHERID = {vchid}";
                DynamicObjectCollection dyc = DBUtils.ExecuteDynamicObject(e.Context, sql);
                if (dyc == null || dyc.Count == 0)
                {
                    return;
                }
                using (var cope = new KDTransactionScope(TransactionScopeOption.Required))
                {
                    var billView = FormMetadataUtils.CreateBillView(e.Context, "GL_VOUCHER", vchid);
                    //删除原单据贷方所有记录
                    var rowcount = billView.Model.GetEntryRowCount("FEntity");
                    var Newentity = billView.BillBusinessInfo.GetEntity("FEntity");
                    DynamicObject rowObj = null;
                    for (int i = rowcount; i >= 0; i--)
                    {
                        var id = Convert.ToInt64(billView.Model.GetEntryPKValue("FEntity", i));
                        var dc = billView.Model.GetValue<Int32>("FDC", i, 0);
                        if (id == 0)
                        {
                            billView.Model.DeleteEntryRow("FEntity", i);
                        }
                        if (dc == -1)
                        {
                            rowObj = (DynamicObject)billView.Model.GetEntityDataObject(Newentity, i).Clone(false, true);
                            billView.Model.DeleteEntryRow("FEntity", i);
                        }
                    }
                    rowcount = billView.Model.GetEntryRowCount("FEntity");
                    //decimal credittotal = 0;
                    foreach (var item in dyc)
                    {
                        billView.Model.CreateNewEntryRow(Newentity, rowcount);
                        decimal amount = Convert.ToDecimal(item["FRETURNAMOUNT"]);
                        long cutid = Convert.ToInt64(item["FBCONTACTUNIT"]);
                        string cuttype = Convert.ToString(item["FBCONTACTUNITTYPE"]);
                        long detailid = 0;
                        switch (cuttype)
                        { 
                            case "BD_Customer":
                                sql = $"SELECT FID FROM T_BD_FLEXITEMDETAILV WHERE FFLEX6={cutid} AND FFLEX4=0 AND FFLEX5=0";
                                detailid = DBUtils.ExecuteScalar<long>(e.Context, sql, 0);
                                break;
                            case "BD_Supplier":
                                sql = $"SELECT FID FROM T_BD_FLEXITEMDETAILV WHERE FFLEX4={cutid}";
                                detailid = DBUtils.ExecuteScalar<long>(e.Context, sql, 0);
                                break;
                        }
                        billView.Model.SetValue("FEXPLANATION", rowObj["FEXPLANATION"], rowcount);
                        billView.Model.SetItemValueByID("FACCOUNTID", rowObj["FACCOUNTID_Id"], rowcount);
                        billView.Model.SetItemValueByID("FCURRENCYID", rowObj["FCURRENCYID_Id"], rowcount);
                        billView.Model.SetItemValueByID("FEXCHANGERATETYPE", rowObj["EXCHANGERATETYPE_Id"], rowcount);
                        billView.Model.SetItemValueByID("FSettleTypeID", rowObj["SettleTypeID_Id"], rowcount);
                        billView.Model.SetValue("FCREDIT", amount, rowcount);
                        billView.Model.SetItemValueByID("FDETAILID", detailid, rowcount);
                        //billView.Model.SetItemValueByIDFromClient("$$FDETAILID__FFLEX6", 100015, rowcount);

                        rowcount++;
                    }
                    FormMetadataUtils.SaveBill(e.Context, billView);
                    billView.CommitNetworkCtrl();
                    billView.InvokeFormOperation(FormOperationEnum.Close);
                    billView.Close();

                    cope.Complete();
                }
            }
        }

        public static void SaveBill(Context ctx, IBillView billView)
        {
            var operateOption = OperateOption.Create();
            operateOption.SetIgnoreWarning(true);
            var oper = BusinessDataServiceHelper.Save(ctx, billView.BillBusinessInfo, billView.Model.DataObject, operateOption, "Save");
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
        }
    }

    [Description("生成凭证 -承兑汇票类型按背书拆分贷方明细"), HotUpdate]
    public class VoucherBOESplitLoad : AbstractBuildVoucherPlugIn
    {
        public override void AfterLoadBatchSourceData(BuildVoucherArgs e)
        {
            base.AfterLoadBatchSourceData(e);
        }
        public override void BeforeLoadBatchSourceData(BuildVoucherArgs e)
        {
            base.BeforeLoadBatchSourceData(e);
        }
    }
}
