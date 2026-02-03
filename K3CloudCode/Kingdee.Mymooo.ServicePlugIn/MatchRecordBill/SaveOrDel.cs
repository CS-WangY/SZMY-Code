using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.ServiceHelper.SalesManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Util;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using System.Web.UI.WebControls.WebParts;
using Kingdee.BOS.App.Data;
using Kingdee.BOS;

namespace Kingdee.Mymooo.ServicePlugIn.MatchRecordBill
{
	[Description("应收收款核销记录保存和删除更新应收单收款信息"), HotUpdate]
	public class SaveOrDel : AbstractOperationServicePlugIn
	{
		//应收的Fid集合
		List<long> recFidList = new List<long>();
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FALLAMOUNTFOR_D");//价税合计
			e.FieldKeys.Add("FIsGuarantee");//担保发货
			e.FieldKeys.Add("FReceivableAmount");//收款金额
			e.FieldKeys.Add("FReceivableStatus");//收款状态(0未收款/1部分收款/2完全收款)
		}

		public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
		{
			base.BeginOperationTransaction(e);
			foreach (var item in e.DataEntitys)
			{
				string sSql = $@"select distinct t2.FSRCBILLID from T_AR_RECMacthLog t1
                inner join T_AR_RECMacthLogENTRY t2 on  t1.FID=t2.FID
                where  t1.FISCLEARINNER='F' and t1.FMATCHMETHODID<>30   
                and t2.FSOURCETYPE ='180ecd4afd5d44b5be78a6efe4a7e041'
                and t1.FID={Convert.ToInt64(item["Id"])} ";
				var FidDynamic = DBUtils.ExecuteDynamicObject(this.Context, sSql);
				foreach (var items in FidDynamic)
				{
					recFidList.Add(Convert.ToInt64(items["FSRCBILLID"]));
				}
			}
		}

		public override void EndOperationTransaction(EndOperationTransactionArgs e)
		{
			base.EndOperationTransaction(e);
			foreach (var item in e.DataEntitys)
			{
				string sSql = $@"select distinct t2.FSRCBILLID from T_AR_RECMacthLog t1
                inner join T_AR_RECMacthLogENTRY t2 on  t1.FID=t2.FID
                where  t1.FISCLEARINNER='F' and t1.FMATCHMETHODID<>30   
                and t2.FSOURCETYPE ='180ecd4afd5d44b5be78a6efe4a7e041'
                and t1.FID={Convert.ToInt64(item["Id"])} ";
				var FidDynamic = DBUtils.ExecuteDynamicObject(this.Context, sSql);
				foreach (var items in FidDynamic)
				{
					recFidList.Add(Convert.ToInt64(items["FSRCBILLID"]));
				}
			}
			foreach (var fid in recFidList.Distinct())
			{
				//先清空记录
				string sEmptySql = $@"update T_AR_RECEIVABLEENTRY set FReceivableAmount=0,FReceivableStatus=0 where FID ={fid} ";
				DBUtils.Execute(this.Context, sEmptySql);

				//获取应收核销金额
				string sSql = $@"select t2.FSRCBILLID,SUM(t2.FCURWRITTENOFFAMOUNTFOR) AMOUNT from T_AR_RECMacthLog t1
                                inner join T_AR_RECMacthLogENTRY t2 on  t1.FID=t2.FID
                                where  t1.FISCLEARINNER='F' and t1.FMATCHMETHODID<>30   
                                and t2.FSOURCETYPE ='180ecd4afd5d44b5be78a6efe4a7e041'
                                and t2.FSRCBILLID={fid}
								group by t2.FSRCBILLID";
				var AmountDynamic = DBUtils.ExecuteDynamicObject(this.Context, sSql);
				if (AmountDynamic.Count > 0)
				{
					//先正负抵扣
					string sAmountSql = $@"select t1.FID,
                    isnull((select SUM(FALLAMOUNTFOR) from T_AR_RECEIVABLEENTRY t2 where t1.FID=t2.FID and t2.FALLAMOUNTFOR>0),0) AMOUNT1,
                    isnull((select SUM(FALLAMOUNTFOR) from T_AR_RECEIVABLEENTRY t2 where t1.FID=t2.FID and t2.FALLAMOUNTFOR<0),0) AMOUNT2
                    from t_AR_receivable t1 where t1.FID={fid} ";
					var amountDynamic = DBUtils.ExecuteDynamicObject(this.Context, sAmountSql);
					//正数金额
					decimal positiveAmount = Convert.ToDecimal(amountDynamic[0]["AMOUNT1"]);
					//负数金额
					decimal negativeAmount = Math.Abs(Convert.ToDecimal(amountDynamic[0]["AMOUNT2"]));
					//更新应收单信息
					var view = FormMetadataUtils.CreateBillView(this.Context, "AR_receivable", fid);
					DynamicObjectCollection entrys = view.Model.GetEntityDataObject(view.BusinessInfo.GetEntity("FEntityDetail"));

					//核销金额
					var sumAmount = Convert.ToDecimal(AmountDynamic[0]["AMOUNT"]);
					//是否正数
					var IsInt = true;
					if (sumAmount < 0)
					{
						IsInt = false;
						sumAmount = Math.Abs(sumAmount);
					}
					//先处理金额为0的数据
					foreach (var entry in entrys.Where(x => (Convert.ToDecimal(x["FALLAMOUNTFOR_D"]) == 0)))
					{
						var rowIndex = entrys.IndexOf(entry);
						//收款状态(0待收退款/1部分收款/2完全收款/3部分退款/4完全退款)
						if (IsInt)
						{
							view.Model.SetValue("FReceivableStatus", "2", rowIndex);
						}
						else
						{
							view.Model.SetValue("FReceivableStatus", "4", rowIndex);
						}
					}

					if (positiveAmount > 0 && negativeAmount > 0)
					{
						//正数比负数大，以正数为主循环
						if (positiveAmount > negativeAmount)
						{
							foreach (var entry in entrys.Where(x => Convert.ToDecimal(x["FALLAMOUNTFOR_D"]) > 0))
							{
								//全部抵扣已完成
								if (negativeAmount == 0)
								{
									continue;
								}

								var rowIndex = entrys.IndexOf(entry);
								//应收金额
								decimal ysAmount = Convert.ToDecimal(entry["FALLAMOUNTFOR_D"]);
								//累计抵扣金额
								decimal sumDkAmount = 0;
								foreach (var entry2 in entrys.Where(x => (Convert.ToDecimal(x["FALLAMOUNTFOR_D"]) < 0) && (Convert.ToDecimal(x["FALLAMOUNTFOR_D"]) < Convert.ToDecimal(x["FReceivableAmount"]))))
								{
									//本次累计抵扣已完成
									if (ysAmount == sumDkAmount)
									{
										break;
									}
									var rowIndex2 = entrys.IndexOf(entry2);
									//本次抵扣金额
									decimal dkAmount = Math.Abs(Convert.ToDecimal(entry2["FALLAMOUNTFOR_D"])) - Math.Abs(Convert.ToDecimal(entry2["FReceivableAmount"]));
									//已收款金额
									decimal yskAmount = Math.Abs(Convert.ToDecimal(entry2["FReceivableAmount"]));
									//剩余抵扣金额
									decimal syAmount = ysAmount - sumDkAmount;
									if (syAmount >= dkAmount)
									{
										view.Model.SetValue("FReceivableAmount", -Math.Abs(dkAmount + yskAmount), rowIndex2);
										//收款状态(0待收退款/1部分收款/2完全收款/3部分退款/4完全退款)
										view.Model.SetValue("FReceivableStatus", "4", rowIndex2);
										sumDkAmount += dkAmount;
									}
									else if (syAmount < dkAmount)
									{
										view.Model.SetValue("FReceivableAmount", -Math.Abs(syAmount + yskAmount), rowIndex2);
										//收款状态(0待收退款/1部分收款/2完全收款/3部分退款/4完全退款)
										view.Model.SetValue("FReceivableStatus", "3", rowIndex2);
										sumDkAmount += syAmount;
									}
								}
								//本次完全抵扣
								if (ysAmount == sumDkAmount)
								{
									view.Model.SetValue("FReceivableAmount", sumDkAmount, rowIndex);
									//收款状态(0待收退款/1部分收款/2完全收款/3部分退款/4完全退款)
									view.Model.SetValue("FReceivableStatus", "2", rowIndex);
									//本次抵扣都不够，说明已经扣完
									negativeAmount -= sumDkAmount;
								}
								else
								{
									//本次部分抵扣
									view.Model.SetValue("FReceivableAmount", sumDkAmount, rowIndex);
									//收款状态(0待收退款/1部分收款/2完全收款/3部分退款/4完全退款)
									view.Model.SetValue("FReceivableStatus", "1", rowIndex);
									//本次抵扣都不够，说明已经扣完
									negativeAmount -= sumDkAmount;
									continue;
								}
							}
						}
						else
						{
							foreach (var entry in entrys.Where(x => Convert.ToDecimal(x["FALLAMOUNTFOR_D"]) < 0))
							{
								//全部抵扣已完成
								if (positiveAmount == 0)
								{
									continue;
								}

								var rowIndex = entrys.IndexOf(entry);
								//应收金额
								decimal ysAmount = Math.Abs(Convert.ToDecimal(entry["FALLAMOUNTFOR_D"]));
								//累计抵扣金额
								decimal sumDkAmount = 0;
								foreach (var entry2 in entrys.Where(x => (Convert.ToDecimal(x["FALLAMOUNTFOR_D"]) > 0) && (Convert.ToDecimal(x["FALLAMOUNTFOR_D"]) > Convert.ToDecimal(x["FReceivableAmount"]))))
								{
									//本次累计抵扣已完成
									if (ysAmount == sumDkAmount)
									{
										break;
									}
									var rowIndex2 = entrys.IndexOf(entry2);
									//本次抵扣金额
									decimal dkAmount = Math.Abs(Convert.ToDecimal(entry2["FALLAMOUNTFOR_D"])) - Math.Abs(Convert.ToDecimal(entry2["FReceivableAmount"]));
									//已收款金额
									decimal yskAmount = Math.Abs(Convert.ToDecimal(entry2["FReceivableAmount"]));
									//剩余抵扣金额
									decimal syAmount = ysAmount - sumDkAmount;
									if (syAmount >= dkAmount)
									{
										view.Model.SetValue("FReceivableAmount", dkAmount + yskAmount, rowIndex2);
										//收款状态(0待收退款/1部分收款/2完全收款/3部分退款/4完全退款)
										view.Model.SetValue("FReceivableStatus", "2", rowIndex2);
										sumDkAmount += dkAmount;
									}
									else if (syAmount < dkAmount)
									{
										view.Model.SetValue("FReceivableAmount", syAmount + yskAmount, rowIndex2);
										//收款状态(0待收退款/1部分收款/2完全收款/3部分退款/4完全退款)
										view.Model.SetValue("FReceivableStatus", "1", rowIndex2);
										sumDkAmount += syAmount;
									}
								}
								//本次完全抵扣
								if (ysAmount == sumDkAmount)
								{
									view.Model.SetValue("FReceivableAmount", -Math.Abs(sumDkAmount), rowIndex);
									//收款状态(0待收退款/1部分收款/2完全收款/3部分退款/4完全退款)
									view.Model.SetValue("FReceivableStatus", "4", rowIndex);
									//本次抵扣都不够，说明已经扣完
									positiveAmount -= sumDkAmount;
								}
								else
								{
									//本次部分抵扣
									view.Model.SetValue("FReceivableAmount", -Math.Abs(sumDkAmount), rowIndex);
									//收款状态(0待收退款/1部分收款/2完全收款/3部分退款/4完全退款)
									view.Model.SetValue("FReceivableStatus", "3", rowIndex);
									//本次抵扣都不够，说明已经扣完
									positiveAmount -= sumDkAmount;
									continue;
								}
							}

						}
					}

					//核销金额不为空才需要继续抵扣
					if (sumAmount != 0)
					{
						//抵扣剩余核销金额
						foreach (var entry in entrys.Where(x => (Convert.ToDecimal(x["FALLAMOUNTFOR_D"]) != Convert.ToDecimal(x["FReceivableAmount"]))).OrderByDescending(o => Convert.ToBoolean(o["FIsGuarantee"]) == true ? 1 : 0).ThenBy(o => Convert.ToInt32(o["seq"])).ToList())
						{
							var rowIndex = entrys.IndexOf(entry);
							//应收金额
							var amount = Math.Abs(decimal.Parse(entry["FALLAMOUNTFOR_D"].ToString()));
							//已付款金额
							var recAmount = Math.Abs(decimal.Parse(entry["FReceivableAmount"].ToString()));
							//本次抵扣金额
							decimal dkAmount = amount - recAmount;

							if (sumAmount >= dkAmount)
							{
								//收款状态(0待收退款/1部分收款/2完全收款/3部分退款/4完全退款)
								if (IsInt)
								{
									view.Model.SetValue("FReceivableAmount", recAmount + dkAmount, rowIndex);
									view.Model.SetValue("FReceivableStatus", "2", rowIndex);
								}
								else
								{
									view.Model.SetValue("FReceivableAmount", -Math.Abs(recAmount + dkAmount), rowIndex);
									view.Model.SetValue("FReceivableStatus", "4", rowIndex);
								}

								sumAmount -= dkAmount;
							}
							else if (dkAmount > sumAmount)
							{

								//收款状态(0待收退款/1部分收款/2完全收款/3部分退款/4完全退款)
								if (IsInt)
								{
									view.Model.SetValue("FReceivableAmount", recAmount + sumAmount, rowIndex);
									view.Model.SetValue("FReceivableStatus", "1", rowIndex);
								}
								else
								{
									view.Model.SetValue("FReceivableAmount", -Math.Abs(recAmount + sumAmount), rowIndex);
									view.Model.SetValue("FReceivableStatus", "3", rowIndex);
								}

								sumAmount = 0;
							}
							if (sumAmount == 0)
							{
								break;
							}
						}

					}
					//批量修改分摊金额
					List<SqlObject> list = new List<SqlObject>();
					foreach (var entry in entrys)
					{
						long FENTRYID = Convert.ToInt64(entry["id"]);
						decimal ReceivableAmount = Convert.ToDecimal(entry["FReceivableAmount"]);
						int ReceivableStatus = Convert.ToInt32(entry["FReceivableStatus"]);
						list.Add(new SqlObject("/*dialect*/update T_AR_RECEIVABLEENTRY set FReceivableAmount=@ReceivableAmount,FReceivableStatus=@ReceivableStatus where FENTRYID =@FENTRYID ",
							 new List<SqlParam>(){
							 new SqlParam("@ReceivableAmount", KDDbType.Decimal, ReceivableAmount)
							,new SqlParam("@ReceivableStatus", KDDbType.String, ReceivableStatus)
							,new SqlParam("@FENTRYID", KDDbType.Int64, FENTRYID)}));

					}

					DBUtils.ExecuteBatch(this.Context, list);

					//var oper = MymoooBusinessDataServiceHelper.SaveBill(this.Context, view.BusinessInfo, true, new DynamicObject[] { view.Model.DataObject });
					//if (!oper.IsSuccess)
					//{
					//    if (oper.ValidationErrors.Count > 0)
					//    {
					//        throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
					//    }
					//    else
					//    {
					//        throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
					//    }
					//}
					//清除释放网控
					view.CommitNetworkCtrl();
					view.InvokeFormOperation(FormOperationEnum.Close);
					view.Close();
				}
			}
		}

		public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
		{
			base.AfterExecuteOperationTransaction(e);
			foreach (var item in e.DataEntitys)
			{
				string sSql = $@"select distinct t2.FSRCBILLID from T_AR_RECMacthLog t1
                inner join T_AR_RECMacthLogENTRY t2 on  t1.FID=t2.FID
                where  t1.FISCLEARINNER='F' and t1.FMATCHMETHODID<>30   
                and t2.FSOURCETYPE ='180ecd4afd5d44b5be78a6efe4a7e041'
                and t1.FID={Convert.ToInt64(item["Id"])} ";
				var FidDynamic = DBUtils.ExecuteDynamicObject(this.Context, sSql);
				foreach (var items in FidDynamic)
				{
					recFidList.Add(Convert.ToInt64(items["FSRCBILLID"]));
				}
			}
			foreach (var fid in recFidList.Distinct())
			{
				//先清空记录
				string sEmptySql = $@"/*dialect*/UPDATE t1 SET t1.FReceivableStatus=t3.FReceivableStatus
FROM T_SAL_OUTSTOCKENTRY t1
LEFT JOIN T_AR_RECEIVABLEENTRY_LK t2 ON t1.FENTRYID=t2.FSID AND t2.FRULEID='AR_OutStockToReceivableMap'
INNER JOIN T_AR_RECEIVABLEENTRY t3 ON t2.FENTRYID=t3.FENTRYID
WHERE t3.FID={fid} ";
				DBUtils.Execute(this.Context, sEmptySql);
			}
		}
	}
}
