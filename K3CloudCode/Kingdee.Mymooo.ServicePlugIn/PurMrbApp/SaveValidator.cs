using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Core;
using Kingdee.Mymooo.ServicePlugIn.SalDeliveryNotice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.Mymooo.ServiceHelper.StockManagement;
using Kingdee.BOS.Util;
 using Kingdee.BOS.App.Data;
using Kingdee.BOS.App.Core;

namespace Kingdee.Mymooo.ServicePlugIn.PurMrbApp
{
	public class SaveValidator : AbstractValidator
	{
		public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
		{
			foreach (var data in dataEntities)
			{
				var dynamicObject = data.DataEntity as DynamicObject;
				var entrys = data["PUR_MRAPPENTRY"] as DynamicObjectCollection;
				var index = 1;
				foreach (var item in entrys)
				{
					if (Convert.ToString(item["SRCBILLTYPEID"]).EqualsIgnoreCase("STK_InStock"))
					{
						var srcBillNO = Convert.ToString(item["SRCBILLNO"].ToString());
						var srcSeq = Convert.ToInt32(item["SRCSEQ"].ToString());
						decimal qty = GetReturnQty(this.Context, srcBillNO, srcSeq);
						if (Convert.ToDecimal(item["MRAPPQTY"].ToString()) > qty)
						{
							validateContext.AddError(data.DataEntity, new ValidationErrorInfo(
							string.Empty,
											  data.DataEntity["Id"].ToString(),
											  data.DataEntityIndex,
											  data.RowIndex,
											  item["Id"].ToString(),
											  $"第{index++}行物料编码[{Convert.ToString(((DynamicObject)item["MaterialID"])["Number"])}]可退数量不足[{qty.ToString("0.##")}]",
											  $"可退数量验证",
											  ErrorLevel.FatalError));
						}
					}
				}
			}

		}

		/// <summary>
		/// 根据采购入库单号和序号获取可退数量
		/// </summary>
		/// <returns></returns>
		private decimal GetReturnQty(Context ctx, string billNo, int seq)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@BillNo", KDDbType.String, billNo) ,
				new SqlParam("@Seq", KDDbType.Int32, seq) };
			var sql = $@"/*dialect*/select FREALQTY-ISNULL(Qty,0) qty from T_STK_INSTOCK t1
						inner join T_STK_INSTOCKENTRY t2 on t1.FID=t2.FID
						left join (select FBILLNO,FSEQ,SUM(qty) Qty from (
						--获取采购退料的数据（来源采购入库）
						select t3.FBILLNO,t4.FSEQ,t2.FRMREALQTY qty from T_PUR_MRBENTRY_LK t1
						inner join T_PUR_MRBENTRY t2 on t1.FENTRYID=t2.FENTRYID
						inner join T_STK_INSTOCK t3 on t1.FSBILLID=t3.FID
						inner join T_STK_INSTOCKENTRY t4 on t1.FSBILLID=t4.FID and t1.FSID=t4.FENTRYID
						inner join T_PUR_MRB t5 on t2.FID=t5.FID
						where t1.FSTABLENAME='T_STK_INSTOCKENTRY' and t3.FBILLNO=@BillNo and t4.FSEQ=@Seq  and t5.FCancelStatus='A'
						union all
						--获取采购退料的数据（来源退料申请）
						select t3.FSRCBILLNO,t3.FSRCSEQ,t2.FRMREALQTY qty from T_PUR_MRBENTRY_LK t1
						inner join T_PUR_MRBENTRY t2 on t1.FENTRYID=t2.FENTRYID
						inner join T_PUR_MRAPPENTRY t3 on t1.FSBILLID=t3.FID and t1.FSID=t3.FENTRYID
						inner join T_PUR_MRAPP t4 on t3.FID=t4.FID
						where t1.FSTABLENAME='T_PUR_MRAPPENTRY' and t3.FSRCBILLNO=@BillNo and t3.FSRCSEQ=@Seq  and t4.FCancelStatus='A'
						union all
						--来源退料申请（没有下推退料订单）
						select t1.FSRCBILLNO,t1.FSRCSEQ,t1.FMRAPPQTY qty from T_PUR_MRAPPENTRY t1
						inner join T_PUR_MRAPP t2 on t1.FID=t2.FID
						where t1.FSRCBILLNO=@BillNo and t1.FSRCSEQ=@Seq  and t2.FCancelStatus='A'
						and not exists(select top 1 FENTRYID from T_PUR_MRBENTRY_LK t3 
						where t3.FSTABLENAME='T_PUR_MRAPPENTRY'  and t3.FSBILLID=t1.FID and t3.FSID=t1.FENTRYID) 
						) datas group by FBILLNO,FSEQ) t3 on t3.FBILLNO=t1.FBILLNO and t3.FSEQ=t2.FSEQ
						where t1.FBILLNO=@BillNo and t2.FSEQ=@Seq ";
			return DBUtils.ExecuteScalar<decimal>(ctx, sql, 0, paramList: pars.ToArray());
		}
	}
}
