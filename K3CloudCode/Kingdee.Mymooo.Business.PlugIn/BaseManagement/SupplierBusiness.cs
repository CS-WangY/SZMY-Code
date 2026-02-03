using Kingdee.BOS;
using Kingdee.BOS.BusinessEntity.InformationCenter.PushPlatform.Dto;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls.WebParts;

namespace Kingdee.Mymooo.Business.PlugIn.BaseManagement
{
	/// <summary>
	/// 同步供应商信息
	/// </summary>
	public class SupplierBusiness : IMessageExecute
	{
		public ResponseMessage<dynamic> Execute(Context ctx, string message)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			var request = JsonConvert.DeserializeObject<SupplierRequest>(message);
			if (request == null || string.IsNullOrWhiteSpace(request?.Code))
			{
				response.Message = "参数信息不能为空";
				response.Code = ResponseCode.ModelError;
				return response;
			}
			return BasicDataSyncServiceHelper.AddOrMotifySupplier(ctx, request);
		}
		//供应商供应产品小类
		public ResponseMessage<dynamic> SupplierSmall(Context ctx, string message)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			var requests = JsonConvert.DeserializeObject<SupplierSmallRequest[]>(message);
			if (requests.Count() == 0)
			{
				response.Message = "参数信息不能为空";
				response.Code = ResponseCode.ModelError;
				return response;
			}
			return SupplyOrgServiceHelper.SupplierSmallService(ctx, requests);

		}
		/// <summary>
		/// 供应商供应产品小类新增供应商+小类检验评分
		/// </summary>
		/// <param name="request"></param>
		public ResponseMessage<dynamic> AddSupplierClassInspectScore(Context ctx)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			var sql = @"/*dialect*/insert into PENY_T_SupplierClassInspectScore(FORGID,FSUPPLIERID,FParentSmallId,FSmallId,FFraction,FStringencyId,FMODIFYDATE,FCREATEDATE)
						select t2.FSupplyOrgId,t3.FSUPPLIERID,g.FPARENTID ParentSmallId,t1.FMATERIALGROUP SmallId,0,2,getdate(),getdate() from T_BD_SupplierSmall t1
						inner join T_BD_MasterialGroupTaxCode t2 on t1.FMATERIALGROUP=t2.FMATERIALGROUP
						inner join t_BD_Supplier t3 on t1.FSUPPLIERID=t3.FMASTERID and t3.FUSEORGID=t2.FSupplyOrgId
						inner join T_BD_MATERIALGROUP g on t1.FMATERIALGROUP = g.FID and g.FPARENTID>0
						where t1.FDOCUMENTSTATUS='C' and t2.FDOCUMENTSTATUS='C' and t3.FUSEORGID in (7401780,7401781)
						and not exists (
						select top 1 FID from PENY_T_SupplierClassInspectScore t4 where t4.FORGID=t2.FSupplyOrgId and t4.FSUPPLIERID=t3.FSUPPLIERID and t4.FSmallId=t1.FMATERIALGROUP
						)";
			DBServiceHelper.Execute(ctx, sql);
			response.Code = ResponseCode.Success;
			response.Message = "新增成功";
			return response;

		}
	}
}
