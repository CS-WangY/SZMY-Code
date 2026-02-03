using Kingdee.BOS;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.Operation;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Contracts;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kingdee.Mymooo.App.Core
{
	public class MymoooBusinessDataService : IMymoooBusinessDataService
	{
		/// <summary>
		/// 保存并提交审核
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="businessInfo"></param>
		/// <param name="dynamicObjects"></param>
		/// <returns></returns>
		public IOperationResult SaveAndAuditBill(Context ctx, BusinessInfo businessInfo, DynamicObject[] dynamicObjects)
		{
			SaveService saveService = new SaveService();
			IOperationResult operationResult;
			var operateOption = OperateOption.Create();
			operateOption.SetIgnoreWarning(true);
			operationResult = saveService.SaveAndAudit(ctx, businessInfo, dynamicObjects, operateOption);
			return operationResult;
		}

		/// <summary>
		/// 保存单据
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="businessInfo"></param>
		/// <param name="dynamicObjects"></param>
		/// <returns></returns>
		public IOperationResult SaveBill(Context ctx, BusinessInfo businessInfo, DynamicObject[] dynamicObjects)
		{
			return SaveBill(ctx, businessInfo, false, dynamicObjects);
		}

		/// <summary>
		/// 保存单据
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="businessInfo"></param>
		/// <param name="dynamicObjects"></param>
		/// <returns></returns>
		public IOperationResult SaveBill(Context ctx, BusinessInfo businessInfo, bool isRemoveValidators, DynamicObject[] dynamicObjects)
		{
			SaveService saveService = new SaveService();
			IOperationResult operationResult;
			var operateOption = OperateOption.Create();
			if (isRemoveValidators)
			{
				operateOption.SetVariableValue("RemoveValidators", true);
			}
			operateOption.SetIgnoreWarning(true);
			operationResult = saveService.Save(ctx, businessInfo, dynamicObjects, operateOption);
			return operationResult;
		}


		public IOperationResult SubmitBill(Context ctx, BusinessInfo businessInfo, object[] ids)
		{
			SubmitService submiteService = new SubmitService();
			IOperationResult operationResult;
			var operateOption = OperateOption.Create();
			operateOption.SetIgnoreWarning(true);
			operationResult = submiteService.Submit(ctx, businessInfo, ids, FormOperationEnum.Submit.ToString(), operateOption);
			return operationResult;
		}


		/// <summary>
		/// 删除
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="businessInfo"></param>
		/// <param name="ids"></param>
		/// <returns></returns>
		public IOperationResult DeleteBill(Context ctx, BusinessInfo businessInfo, object[] ids)
		{
			DeleteService deleteService = new DeleteService();
			IOperationResult operationResult;
			var operateOption = OperateOption.Create();
			operateOption.SetIgnoreWarning(true);
			operationResult = deleteService.Delete(ctx, businessInfo, ids, operateOption);
			return operationResult;
		}

		/// <summary>
		/// 审核
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="businessInfo"></param>
		/// <param name="ids"></param>
		/// <returns></returns>
		public IOperationResult Audit(Context ctx, BusinessInfo businessInfo, object[] ids)
		{
			var operateOption = OperateOption.Create();
			operateOption.SetIgnoreWarning(true);
			List<object> list = new List<object>();
			list.Add("1");
			List<object> paras = list;
			List<KeyValuePair<object, object>> pkEntryIds = ids.Select((object x) => new KeyValuePair<object, object>(x, "")).ToList();
			SetStatusService setStatusService = new SetStatusService();
			return setStatusService.SetBillStatus(ctx, businessInfo, pkEntryIds, paras, FormOperationEnum.Audit.ToString(), operateOption);
		}

		/// <summary>
		/// 分配
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="allocateParameter"></param>
		/// <returns></returns>
		public IOperationResult Allocate(Context ctx, AllocateParameter allocateParameter)
		{
			AllocateService allocateService = new AllocateService();
			return allocateService.Allocate(ctx, allocateParameter);
		}

		/// <summary>
		/// 反审核
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="businessInfo"></param>
		/// <param name="ids"></param>
		/// <returns></returns>
		public IOperationResult UnAudit(Context ctx, BusinessInfo businessInfo, object[] ids)
		{
			var operateOption = OperateOption.Create();
			operateOption.SetIgnoreWarning(true);
			List<KeyValuePair<object, object>> pkEntryIds = ids.Select((object x) => new KeyValuePair<object, object>(x, "")).ToList();
			SetStatusService setStatusService = new SetStatusService();
			return setStatusService.SetBillStatus(ctx, businessInfo, pkEntryIds, null, FormOperationEnum.UnAudit.ToString(), operateOption);
		}

		/// <summary>
		/// 调用单据操作服务
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="businessInfo"></param>
		/// <param name="ids"></param>
		/// <param name="operationNumber"></param>
		/// <returns></returns>
		public IOperationResult SetBillStatus(Context ctx, BusinessInfo businessInfo, object[] ids, string operationNumber)
		{
			var operateOption = OperateOption.Create();
			operateOption.SetIgnoreWarning(true);
			List<KeyValuePair<object, object>> pkEntryIds = ids.Select((object x) => new KeyValuePair<object, object>(x, "")).ToList();
			SetStatusService setStatusService = new SetStatusService();
			return setStatusService.SetBillStatus(ctx, businessInfo, pkEntryIds, null, operationNumber, operateOption);
		}

		public ResponseMessage<long> AddRabbitMqMeaage(Context ctx, string action, string keyword, string data)
		{
			ResponseMessage<long> response = new ResponseMessage<long>();
			var sql = "/*dialect*/insert into RabbitMQScheduledMessage(FAction,FKeyword,FMessage,FCreateDate) values(@FAction,@FKeyword,@FMessage,@FCreateDate) SELECT SCOPE_IDENTITY()";
			List<SqlParam> sqlParams = new List<SqlParam>()
			{
				new SqlParam("@FAction", KDDbType.String, action),
				new SqlParam("@FKeyword", KDDbType.String, keyword),
				new SqlParam("@FMessage", KDDbType.String, data),
				new SqlParam("@FCreateDate", KDDbType.DateTime, DateTime.Now),
			};

			response.Data = DBUtils.ExecuteScalar<long>(ctx, sql, 0, sqlParams.ToArray());
			response.Code = ResponseCode.Success;
			return response;
		}

		public ResponseMessage<dynamic> AddRabbitMqMeaageResult(Context ctx, string action, string keyword, string data, bool isSucceed, string result)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			var sql = "insert into RabbitMQScheduledMessage(FAction,FKeyword,FMessage,FCreateDate,FIsExecute,FCompleteDate,FIsSucceed,FResult) values(@FAction,@FKeyword,@FMessage,@FCreateDate,@FIsExecute,@FCompleteDate,@FIsSucceed,@FResult)";
			List<SqlParam> sqlParams = new List<SqlParam>()
			{
				new SqlParam("@FAction", KDDbType.String, action),
				new SqlParam("@FKeyword", KDDbType.String, keyword),
				new SqlParam("@FMessage", KDDbType.String, data),
				new SqlParam("@FCreateDate", KDDbType.DateTime, DateTime.Now),
				new SqlParam("@FIsExecute", KDDbType.Boolean, true),
				new SqlParam("@FIsSucceed", KDDbType.Boolean, isSucceed),
				new SqlParam("@FResult", KDDbType.String, result),
				new SqlParam("@FCompleteDate", KDDbType.DateTime, DateTime.Now),
			};

			DBUtils.Execute(ctx, sql, sqlParams);
			response.Code = ResponseCode.Success;
			return response;
		}

		public void UpdateRabbitMqMeaage(Context ctx, long id, string result, bool isSucceed)
		{
			var sql = "update RabbitMQScheduledMessage set FIsExecute = @FIsExecute,FCompleteDate=@FCompleteDate,FIsSucceed=@FIsSucceed,FResult=@FResult where FId = @FId ";
			List<SqlParam> sqlParams = new List<SqlParam>()
			{
				new SqlParam("@FCompleteDate", KDDbType.DateTime, DateTime.Now),
				new SqlParam("@FIsExecute", KDDbType.Boolean, true),
				new SqlParam("@FIsSucceed", KDDbType.Boolean, isSucceed),
				new SqlParam("@FResult", KDDbType.String, result),
				new SqlParam("@FId", KDDbType.Int64, id),
			};

			DBUtils.Execute(ctx, sql, sqlParams);
		}
	}
}
