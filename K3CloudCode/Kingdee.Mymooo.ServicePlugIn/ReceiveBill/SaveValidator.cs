using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Core;
using Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.App.Data;
using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System.Security.Cryptography;
using System.Web.UI.WebControls.WebParts;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;

namespace Kingdee.Mymooo.ServicePlugIn.ReceiveBill
{
    public class SaveValidator : AbstractValidator
    {
        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            StockValidator stockValidator = new StockValidator();
            foreach (var headEntity in dataEntities)
            {
                foreach (var entitem in headEntity["PUR_ReceiveEntry"] as DynamicObjectCollection)
                {
                    var billNo = headEntity.BillNo;
                    //行号
                    var seq = Convert.ToInt64(entitem["Seq"]);
                    //采购单明细ID
                    var srcEntryId = Convert.ToInt64(entitem["SrcEntryId"]);
                    //交货数量
                    var actReceiveQty = Convert.ToDecimal(entitem["ActReceiveQty"]);
                    //获取直发预留数量
                    var dirReServeQty = GetDirReServeQty(srcEntryId);
                    if (dirReServeQty > 0)
                    {
                        //收料单数量累计
                        var sumActReceiveQty = GetSumActReceiveQty(srcEntryId, billNo);
                        //采购订单数量累计(采购数量+退料数量)
                        var poQty = GetPoQty(srcEntryId);
                        //最大可交货数量
                        var maxAactReceiveQty = poQty - sumActReceiveQty - dirReServeQty;
                        if (actReceiveQty > maxAactReceiveQty)
                        {
                            validateContext.AddError(headEntity, new ValidationErrorInfo(
                                  string.Empty,
                                  headEntity["Id"].ToString(),
                                  headEntity.DataEntityIndex,
                                  headEntity.RowIndex,
                                  headEntity["Id"].ToString(),
                                  string.Format("第[{0}]行的数量不足，存在直发预留数量[{1}]，最大交货数量[{2}]，请修改数量。", seq, dirReServeQty.ToString("F"), maxAactReceiveQty.ToString("F")),
                                  $"收料通知单保存[{headEntity["BillNo"]}]",
                                  ErrorLevel.FatalError));
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 获取直发预留数量
        /// </summary>
        /// <returns></returns>
        private decimal GetDirReServeQty(long srcEntryId)
        {
            var sql = $@"select SUM(FDirReServeQty) from t_PUR_POOrderDirReServeQty  where FENTRYID=@srcEntryId ";
            return DBUtils.ExecuteScalar<decimal>(this.Context, sql, 0, new SqlParam("@srcEntryId", KDDbType.Int64, srcEntryId));
        }

        /// <summary>
        /// 获取累计收料单数量(不含当前订单)
        /// </summary>
        /// <returns></returns>
        private decimal GetSumActReceiveQty(long srcEntryId, string billNo)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@srcEntryId", KDDbType.Int64, srcEntryId) };
            var where = "";
            if (!string.IsNullOrEmpty(billNo))
            {
                pars.Add(new SqlParam("@billNo", KDDbType.String, billNo));
                where += $" and t1.FBILLNO!=@billNo ";
            }
            var sql = $@"/*dialect*/select SUM(FACTRECEIVEQTY) ACTRECEIVEQTY from T_PUR_RECEIVE t1
                inner join T_PUR_RECEIVEENTRY t2 on t1.FID=t2.FID
                inner join T_PUR_RECEIVEENTRY_LK t3 on t2.FENTRYID=t3.FENTRYID
                where t3.FSID=@srcEntryId and t1.FCANCELSTATUS='A' {where} ";
            return DBUtils.ExecuteScalar<decimal>(this.Context, sql, 0, paramList: pars.ToArray());
        }


        /// <summary>
        /// 获取采购单数量(采购+退料)
        /// </summary>
        /// <returns></returns>
        private decimal GetPoQty(long srcEntryId)
        {
            var sql = $@"/*dialect*/select FQTY+FBASEMRBQTY from t_PUR_POOrderEntry t1
                        inner join T_PUR_POORDERENTRY_R t2 on t1.FENTRYID=t2.FENTRYID
                        where t1.FENTRYID=@srcEntryId ";
            return DBUtils.ExecuteScalar<decimal>(this.Context, sql, 0, new SqlParam("@srcEntryId", KDDbType.Int64, srcEntryId));
        }
    }
}
