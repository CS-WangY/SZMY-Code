using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
 using Kingdee.BOS.App.Data;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Kingdee.Mymooo.ServicePlugIn.SalesBill
{
	[Description("发货通知单提交按钮插件（更新月结客户的信用额度）")]
    [Kingdee.BOS.Util.HotUpdate]
    public class DeliveryNoticeSubmit : AbstractOperationServicePlugIn
    {
        /// <summary>
        /// 事务外 操作前
        /// </summary>
        /// <param name="e"></param>
        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            base.BeforeExecuteOperationTransaction(e);
            foreach (var data in e.SelectedRows)
            {
                //获取月结订单
                List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FBILLNO", KDDbType.String, data["BillNo"]) };
                var sql = $@"select c.FNUMBER,d.FBILLALLAMOUNT from T_SAL_DELIVERYNOTICE a
                            inner join T_BD_RecCondition b on a.FRECEIPTCONDITIONID=b.FID
                            inner join T_BD_CUSTOMER c on a.FCUSTOMERID=c.FCUSTID
                            inner join T_SAL_DELIVERYNOTICEFIN d on a.FID=d.FID
                            where a.FBILLNO=@FBILLNO and b.FRECMETHOD=1 ";
                var datas = DBUtils.ExecuteDynamicObject(this.Context, sql, paramList: pars.ToArray());
                //获取客户Code
                var customerCode = "";
                //价税合计
                decimal billAllAmount = 0;
                foreach (var item in datas)
                {
                    customerCode = item["FNUMBER"].ToString();
                    billAllAmount = Convert.ToDecimal(item["FBILLALLAMOUNT"]);
                }
                //月结客户才需要判断额度,并且金额大于0
                if (!string.IsNullOrEmpty(customerCode) && billAllAmount > 0)
                {
                    var request = new
                    {
                        CustId = customerCode,
                        Amount = billAllAmount
                    };
                    var response = ApigatewayUtils.InvokePostWebService($"credit/{ApigatewayUtils.ApigatewayConfig.EnvCode}/credit/query", JsonConvertUtils.SerializeObject(request));

                    var returninfo = JsonConvertUtils.DeserializeObject<MessageHelpForCredit>(response);
                    if (!returninfo.IsSuccess)
                    {
                        throw new Exception("获取信用接口出错：" + returninfo.ErrorMessage + returninfo.Message);
                    }
                    else
                    {
                        pars = new List<SqlParam>() {
                            new SqlParam("@ID", KDDbType.Int32, data["Id"]),
                            new SqlParam("@FCreditLine", KDDbType.Decimal, returninfo.Data.CreditLine),
                            new SqlParam("@FAvailableCredit", KDDbType.Decimal, returninfo.Data.AvailableCredit),
                            new SqlParam("@FOccupyCredit", KDDbType.Decimal, returninfo.Data.OccupyCredit),
                            new SqlParam("@FExpiryDay", KDDbType.Int32, returninfo.Data.ExpiryDay),
                            new SqlParam("@FExpiryAmount", KDDbType.Decimal, returninfo.Data.ExpiryAmount),
                        };
                        var updateSql = $@"update T_SAL_DELIVERYNOTICE set FCreditLine=@FCreditLine,FAvailableCredit=@FAvailableCredit,FOccupyCredit=@FOccupyCredit,FExpiryDay=@FExpiryDay,FExpiryAmount=@FExpiryAmount where FID=@ID";
                        DBUtils.Execute(this.Context, updateSql, pars);

                    }
                }
            }
        }
    }
}
