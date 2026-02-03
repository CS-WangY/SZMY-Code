using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.Web.Bill;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ProductionManagement;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.ProductionManagement
{
	/// <summary>
	/// MES生产补料
	/// </summary>
	public class MesProductionFeedMtrlBusiness : IMessageExecute
	{
		public ResponseMessage<dynamic> Execute(Context ctx, string message)
		{
			MesProductionFeedMtrlRequest request = JsonConvertUtils.DeserializeObject<MesProductionFeedMtrlRequest>(message);

			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();

			try
			{
				if (string.IsNullOrWhiteSpace(request.FeedBillNo))
				{
					response.Code = ResponseCode.ModelError;
					response.Message = "补料单号不能为空！";
					return response;
				}
				if (request.Details.Count == 0)
				{
					response.Code = ResponseCode.ModelError;
					response.Message = "生产订单明细不能为空！";
					return response;
				}
				if (IsExistsBillNo(ctx, request.FeedBillNo))
				{
					response.Code = ResponseCode.ModelError;
					response.Message = $"补料单号[{request.FeedBillNo}]已存在！";
					return response;
				}
				List<MesProductionFeedMtrlEntity> list = new List<MesProductionFeedMtrlEntity>();

				foreach (var items in request.Details)
				{
					List<SqlParam> pars = new List<SqlParam>() {
				new SqlParam("@FMOID", KDDbType.Int64, request.Id),
				new SqlParam("@FMOENTRYID", KDDbType.Int64, items.EntryId)
				};
					int i = 1;
					string param = string.Empty;
					foreach (var item in items.SubDetails)
					{
						if (i == 1)
							param = "@ItemNo" + i;
						else
							param += ",@ItemNo" + i;
						pars.Add(new SqlParam("@ItemNo" + i++, KDDbType.String, item.MaterialCode));
					}
					var sql = $@"/*dialect*/select t1.FPRDORGID,t1.FBILLNO,t2.FID,t2.FENTRYID,t1.FDOCUMENTSTATUS,t3.FNUMBER,t4.FNOPICKEDQTY from T_PRD_PPBOM t1
                        inner join T_PRD_PPBOMENTRY t2 on t1.FID=t2.FID
                        inner join T_BD_MATERIAL t3 on t2.FMATERIALID=t3.FMATERIALID
                        inner join T_PRD_PPBOMENTRY_Q  t4 on t2.FENTRYID=t4.FENTRYID
                        where  t2.FMOID=@FMOID and t2.FMOENTRYID=@FMOENTRYID and t3.FNUMBER in ({param}) ";
					var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
					if (datas.Count == 0)
					{
						response.Code = ResponseCode.ModelError;
						response.Message = $"生产订单[{request.BillNo}-{items.BillSeq}]不存在生产用料清单！";
						return response;
					}
					if (!datas[0]["FDOCUMENTSTATUS"].ToString().EqualsIgnoreCase("C"))
					{
						response.Code = ResponseCode.ModelError;
						response.Message = $"生产用料清单[{datas[0]["FBILLNO"].ToString()}]不是已批核状态！";
						return response;
					}
					//验证物料和数量是否足够
					foreach (var item in items.SubDetails)
					{
						if (datas.Where(x => x["FNUMBER"].Equals(item.MaterialCode)).Count() == 0)
						{
							response.Code = ResponseCode.ModelError;
							response.Message = $"生产用料清单[{datas[0]["FBILLNO"].ToString()}]不存在物料[{item.MaterialCode}]";
							return response;
						}
					}
					//重构数据
					foreach (var item in datas)
					{
						list.Add(new MesProductionFeedMtrlEntity
						{
							FPRDORGID = Convert.ToInt64(item["FPRDORGID"]),
							FID = Convert.ToInt64(item["FID"]),
							FENTRYID = Convert.ToInt64(item["FENTRYID"]),
							FQTY = items.SubDetails.Where(x => x.MaterialCode.Equals(item["FNUMBER"].ToString())).Select(s => s.ActualQty).FirstOrDefault(),
							FSTOCKCODE = items.SubDetails.Where(x => x.MaterialCode.Equals(item["FNUMBER"].ToString())).Select(s => s.StockCode).FirstOrDefault(),
							FFeedReasonId = items.SubDetails.Where(x => x.MaterialCode.Equals(item["FNUMBER"].ToString())).Select(s => s.FeedReasonId).FirstOrDefault()
						});
					}
				}

				List<ListSelectedRow> selectedRows = new List<ListSelectedRow>();
				foreach (var item in list)
				{
					selectedRows.Add(new ListSelectedRow(Convert.ToString(item.FID), Convert.ToString(item.FENTRYID), 0, "PRD_PPBOM") { EntryEntityKey = "FEntity" });
				}
				// 生产用料清单下推生产补料单
				var rules = ConvertServiceHelper.GetConvertRules(ctx, "PRD_PPBOM", "PRD_FeedMtrl");
				var rule = rules.Where(x => x.Key.EqualsIgnoreCase("PRDPPBOMRulePRDFeed")).FirstOrDefault();
				if (rule == null)
				{
					throw new Exception("没有从生产用料清单下推生产补料单的转换关系");
				}

				PushArgs pushArgs = new PushArgs(rule, selectedRows.ToArray())
				{
					TargetBillTypeId = "ca103ed148d0492dbe81d89f54d9ef85", // 请设定目标单据单据类型
					TargetOrgId = 0,            // 请设定目标单据主业务组织
				};
				//执行下推操作，并获取下推结果
				var operationResult = ConvertServiceHelper.Push(ctx, pushArgs, null);
				if (operationResult.IsSuccess)
				{
					var view = FormMetadataUtils.CreateBillView(ctx, "PRD_FeedMtrl");
					foreach (var item in operationResult.TargetDataEntities)
					{
						view.Model.DataObject = item.DataEntity;
						view.Model.SetValue("FBillNo", request.FeedBillNo);
						var entrys = view.Model.GetEntityDataObject(view.BusinessInfo.GetEntity("FEntity"));
						var rowIndex = 0;
						foreach (var entry in entrys)
						{
							var thisList = list.Where(x => x.FENTRYID.Equals(Convert.ToInt64(((entry["FEntity_Link"] as DynamicObjectCollection)[0] as DynamicObject)["SId"])))
							.FirstOrDefault();
							view.Model.SetValue("FAppQty", thisList.FQTY, rowIndex);
							view.InvokeFieldUpdateService("FAppQty", rowIndex);
							view.Model.SetValue("FActualQty", thisList.FQTY, rowIndex);
							view.InvokeFieldUpdateService("FActualQty", rowIndex);
							view.Model.SetItemValueByNumber("FStockId", thisList.FSTOCKCODE, rowIndex);
							view.InvokeFieldUpdateService("FStockId", rowIndex);
							view.Model.SetItemValueByNumber("FFeedReasonId", thisList.FFeedReasonId, rowIndex);
							rowIndex++;
						}
					}
					//保存批核
					var opers = MymoooBusinessDataServiceHelper.SaveAndAuditBill(ctx, view.BusinessInfo, new DynamicObject[] { view.Model.DataObject }.ToArray());
					if (opers.IsSuccess)
					{
						//清除释放网控
						view.CommitNetworkCtrl();
						view.InvokeFormOperation(FormOperationEnum.Close);
						view.Close();
						response.Code = ResponseCode.Success;
						response.Message = string.Join(";", opers.OperateResult.Select(p => p.Message));
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
				else
				{
					if (operationResult.ValidationErrors.Count > 0)
					{
						throw new Exception(string.Join(";", operationResult.ValidationErrors.Select(p => p.Message)));
					}
					else
					{
						throw new Exception(string.Join(";", operationResult.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
					}
				}

			}
			catch (Exception ex)
			{
				response.Code = ResponseCode.ModelError;
				response.Message = ex.Message;
			}
			return response;
		}



		/// <summary>
		/// 验证生产补料单是否存在
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="orgCode"></param>
		/// <returns></returns>
		private bool IsExistsBillNo(Context ctx, string BillNo)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@BillNo", KDDbType.String, BillNo) };
			var sql = $@"select count(1) from T_PRD_FEEDMTRL where FBILLNO=@BillNo ";
			return DBServiceHelper.ExecuteScalar<int>(ctx, sql, 0, paramList: pars.ToArray()) > 0 ? true : false;
		}
	}
}
