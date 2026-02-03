using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Contracts.PayableManagement;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.Mymooo.Core.PayableManagement;
using Kingdee.BOS.Core.DynamicForm;

namespace Kingdee.Mymooo.App.Core.PayableManagement
{
	/// <summary>
	/// 应付服务
	/// </summary>
	public class PayableOrderService : IPayableOrderService
	{
		/// <summary>
		/// MES费用采购下推费用应付
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> MesCostPurGenerateCostPayableService(Context ctx, MesCostPurGenerateCostPayableRequest request)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			//0没有折扣，1整单优惠，2明细优惠
			int discountType = 0;
			foreach (var det in request.Details)
			{
				if (det.SubDetails.Where(x => x.EntryDiscountRate != 0).Count() > 0)
				{
					discountType = 2;
					break;
				}
			}
			if (discountType == 0)
			{
				if (request.OrderDiscountAmountFor != 0)
				{
					discountType = 1;
				}
			}
			//如果已经生成单据，则跳过
			string sql = $@"select top 1 FBILLNO from T_AP_PAYABLE where FBILLNO='{request.PayBillNo}' ";
			if (!string.IsNullOrWhiteSpace(DBServiceHelper.ExecuteScalar<string>(ctx, sql, "")))
			{
				response.Code = ResponseCode.Success;
				response.Message = "已生成费用应付单";
				return response;
			}
			foreach (var det in request.Details)
			{
				sql = $@"select top 1 FID from T_PUR_POORDER where FBILLNO='{det.BillNo}' ";
				long Id = DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0);
				if (Id == 0)
				{
					response.Code = ResponseCode.ModelError;
					response.Message = $"不存在费用采购订单[{det.BillNo}]";
					return response;
				}
				det.FID = Id;
				foreach (var subDet in det.SubDetails)
				{
					sql = $@"select top 1 FENTRYID from t_PUR_POOrderEntry where FID={Id} and FMesEntryId='{subDet.MesEntryId}' ";
					long entryId = DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0);
					if (entryId == 0)
					{
						response.Code = ResponseCode.ModelError;
						response.Message = $"MES的费用采购订单ID[{subDet.MesEntryId}]不存在";
						return response;
					}
					subDet.FENTRYID = entryId;
				}
			}

			MymoooBusinessDataService service = new MymoooBusinessDataService();
			//处理数据
			try
			{
				using (var cope = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
				{
					List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
					foreach (var det in request.Details)
					{
						foreach (var subDet in det.SubDetails)
						{
							selectedRows.Add(new ListSelectedRow(det.FID.ToString(), subDet.FENTRYID.ToString(), 0, "PUR_PurchaseOrder") { EntryEntityKey = "FPOOrderEntry" });
						}
					}

					var rules = ConvertServiceHelper.GetConvertRules(ctx, "PUR_PurchaseOrder", "AP_Payable");
					var rule = rules.FirstOrDefault(t => t.IsDefault);
					if (rule == null)
					{
						throw new Exception("没有从费用采购到费用应付的转换关系");
					}
					//有数据才需要下推
					if (selectedRows.Count > 0)
					{
						PushArgs pushArgs = new PushArgs(rule, selectedRows.ToArray())
						{
							TargetBillTypeId = "3c6f819d78ac4d5981891956c4595b20",     // 请设定目标单据单据类型
							TargetOrgId = 0,            // 请设定目标单据主业务组织
														//CustomParams = ,     // 可以传递额外附加的参数给单据转换插件
						};
						//执行下推操作，并获取下推结果
						var operationResult = ConvertServiceHelper.Push(ctx, pushArgs, null);
						if (operationResult.IsSuccess)
						{
							var view = FormMetadataUtils.CreateBillView(ctx, "AP_Payable");
							foreach (var item in operationResult.TargetDataEntities)
							{
								view.Model.DataObject = item.DataEntity;
								view.Model.SetValue("FBillNo", request.PayBillNo);
								view.Model.SetValue("FDate", request.Date);
								if (discountType == 1)
								{
									view.Model.SetValue("FOrderDiscountAmountFor", request.OrderDiscountAmountFor);
									view.InvokeFieldUpdateService("FOrderDiscountAmountFor", 0);
								}
								if (discountType == 2)
								{
									var entrys = view.Model.GetEntityDataObject(view.BusinessInfo.GetEntity("FEntityDetail"));
									foreach (var entry in entrys)
									{
										//获取当前的折扣率
										decimal discountRate = request.Details.FirstOrDefault(x => x.BillNo == Convert.ToString(entry["FORDERNUMBER"])).SubDetails.FirstOrDefault(d => d.MesEntryId == Convert.ToString(entry["FMesEntryId"])).EntryDiscountRate;
										if (discountRate != 0)
										{
											var rowIndex = entrys.IndexOf(entry);
											view.Model.SetValue("FEntryDiscountRate", discountRate, rowIndex);
											view.InvokeFieldUpdateService("FEntryDiscountRate", rowIndex);
										}
									}
								}
							}
							//保存批核
							var opers = service.SaveAndAuditBill(ctx, view.BusinessInfo, new DynamicObject[] { view.Model.DataObject }.ToArray());
							if (opers.IsSuccess)
							{
								response.Code = ResponseCode.Success;
								response.Message = string.Join(";", opers.OperateResult.Select(p => p.Message));
								//清除释放网控
								view.CommitNetworkCtrl();
								view.InvokeFormOperation(FormOperationEnum.Close);
								view.Close();
								cope.Complete();
							}
							else
							{
								if (opers.ValidationErrors.Count > 0)
								{
									throw new Exception(string.Join(";", opers.ValidationErrors.Select(p => p.Message)));
								}
								else
								{
									throw new Exception(string.Join(";", opers.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				response.Code = ResponseCode.Exception;
				response.Message = ex.Message;
			}
			return response;
		}
	}
}
