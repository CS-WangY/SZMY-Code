using Kingdee.BOS;
using Kingdee.BOS.BusinessEntity.CloudPlatform;
using Kingdee.BOS.ServiceHelper;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Kingdee.Mymooo.ServiceHelper.PurchaseManagement;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Kingdee.Mymooo.Business.PlugIn.BaseManagement
{

	public class BaseDataBusiness
	{
		/// <summary>
		/// 获取事业部列表
		/// </summary>
		/// <param name="ctx"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> GetBusinessDivisionList(Context ctx, string message)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();

			List<BaseDataEntity> entityList = new List<BaseDataEntity>();
			var sql = $@"/*dialect*/select e.FENTRYID,el.FDATAVALUE from T_BAS_ASSISTANTDATAENTRY e  
                            inner join T_BAS_ASSISTANTDATAENTRY_L el on e.FENTRYID = el.FENTRYID 
                            where e.FID = '638822d83cb6e4' and e.FDOCUMENTSTATUS='C' order by FNUMBER ";
			if (!string.IsNullOrWhiteSpace(message))
			{
				sql += $@" AND el.FDATAVALUE like '%{message}%' ";
			}
			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
			foreach (var data in datas)
			{
				entityList.Add(new BaseDataEntity
				{
					BusinessDivisionId = Convert.ToString(data["FENTRYID"]),
					BusinessDivisionName = Convert.ToString(data["FDATAVALUE"]),
				});
			}
			response.Message = "获取成功";
			response.Data = entityList;
			response.Code = ResponseCode.Success;
			return response;
		}


		/// <summary>
		/// 获取组织列表
		/// </summary>
		/// <param name="ctx"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> GetOrgList(Context ctx)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();

			List<OrgEntity> entityList = new List<OrgEntity>();
			var sql = $@"/*dialect*/select org.FORGID,org.FNUMBER,orgl.FNAME from T_ORG_ORGANIZATIONS org 
            inner join T_ORG_ORGANIZATIONS_L orgl on org.FORGID=orgl.FORGID and orgl.FLOCALEID=2052
            where org.FDOCUMENTSTATUS='C' and org.FFORBIDSTATUS='A' and org.FORGID<>'4093663'
            --and org.FORGID not in(1,3886615,4093663,1043841)
            order by FORGID asc";

			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
			foreach (var data in datas)
			{
				entityList.Add(new OrgEntity
				{
					OrgId = Convert.ToString(data["FORGID"]),
					OrgCode = Convert.ToString(data["FNUMBER"]),
					OrgName = Convert.ToString(data["FNAME"]),
				});
			}
			response.Message = "获取成功";
			response.Data = entityList;
			response.Code = ResponseCode.Success;
			return response;
		}

		/// <summary>
		/// 获取基础资料控制策略
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message（BD_Supplier：供应商）"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> GetBDCtrlPolicy(Context ctx, string message)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();

			if (string.IsNullOrWhiteSpace(message))
			{
				response.Message = "获取失败，参数不能为空";
				response.Code = ResponseCode.ModelError;
				return response;
			}
			List<OrgEntity> entityList = new List<OrgEntity>();
			var sql = $@"/*dialect*/select e.FTARGETORGID,org.FNUMBER,ol.FNAME
                            from T_ORG_BDCtrlPolicy o
	                        inner join T_ORG_BDCtrlTOrgEntry e on o.FPOLICYID = e.FPOLICYID
	                        inner join T_ORG_ORGANIZATIONS org  on e.FTARGETORGID=org.FORGID
	                        inner join T_ORG_ORGANIZATIONS_L ol on e.FTARGETORGID = ol.FORGID and ol.FLOCALEID = 2052
                            where  org.FFORBIDSTATUS='A' and o.FCREATEORGID=1  and org.FPARENTID<>org.FORGID ";
			if (!string.IsNullOrWhiteSpace(message))
			{
				sql += $@" AND o.FBASEDATATYPEID = '{message}' ";
			}
			sql += " order by e.FTARGETORGID ";
			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
			foreach (var data in datas)
			{
				entityList.Add(new OrgEntity
				{
					OrgId = Convert.ToString(data["FTARGETORGID"]),
					OrgCode = Convert.ToString(data["FNUMBER"]),
					OrgName = Convert.ToString(data["FNAME"]),
				});
			}
			response.Message = "获取成功";
			response.Data = entityList;
			response.Code = ResponseCode.Success;
			return response;
		}

		/// <summary>
		/// 获取收款条件列表
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> GetPaymentInfo(Context ctx, string message)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();

			List<PaymentEntity> entityList = new List<PaymentEntity>();
			var sql = $@"/*dialect*/select t1.FNUMBER,t2.FNAME from T_BD_RecCondition t1
            inner join T_BD_RecCondition_L t2 on t1.FID=t2.FID
            where t1.FFORBIDSTATUS='A' and t1.FDOCUMENTSTATUS='C' ";
			if (!string.IsNullOrWhiteSpace(message))
			{
				sql += $@" and t1.FNUMBER='{message}' ";
			}
			sql += " order by t1.FNUMBER ";
			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
			foreach (var data in datas)
			{
				entityList.Add(new PaymentEntity
				{
					TermCode = Convert.ToString(data["FNUMBER"]),
					TermDesc = Convert.ToString(data["FNAME"]),
				});
			}
			response.Message = "获取成功";
			response.Data = entityList;
			response.Code = ResponseCode.Success;
			return response;
		}

		/// <summary>
		/// 获取货币信息列表
		/// </summary>
		/// <param name="ctx"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> GetCurrencyInfo(Context ctx)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();

			List<CurrencyEntity> entityList = new List<CurrencyEntity>();
			var sql = $@"/*dialect*/select t1.FNUMBER,t2.FNAME from T_BD_CURRENCY t1
            inner join T_BD_CURRENCY_L t2 on t1.FCURRENCYID=t2.FCURRENCYID
            where t1.FFORBIDSTATUS='A' and t1.FDOCUMENTSTATUS='C' ";
			sql += " order by t1.FNUMBER ";
			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
			foreach (var data in datas)
			{
				entityList.Add(new CurrencyEntity
				{
					Code = Convert.ToString(data["FNUMBER"]),
					Name = Convert.ToString(data["FNAME"]),
				});
			}
			response.Message = "获取成功";
			response.Data = entityList;
			response.Code = ResponseCode.Success;
			return response;
		}

		/// <summary>
		/// 获取税率信息列表
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> GetVatList(Context ctx, string message)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();

			List<VatEntity> entityList = new List<VatEntity>();
			var sql = $@"/*dialect*/select t1.FNUMBER,t2.FNAME,t1.FTAXRATE from T_BD_TAXRATE t1
            inner join T_BD_TAXRATE_L t2 on t1.FID=t2.FID
            where t1.FFORBIDSTATUS='A' and t1.FDOCUMENTSTATUS='C' ";
			if (!string.IsNullOrWhiteSpace(message))
			{
				sql += $@" and t1.FNUMBER='{message}' ";
			}
			sql += " order by t1.FNUMBER ";
			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
			foreach (var data in datas)
			{
				entityList.Add(new VatEntity
				{
					Code = Convert.ToString(data["FNUMBER"]),
					Desc = Convert.ToString(data["FNAME"]),
					VatRate = decimal.Parse(Convert.ToString(data["FTAXRATE"])),
				});
			}
			response.Message = "获取成功";
			response.Data = entityList;
			response.Code = ResponseCode.Success;
			return response;
		}

		/// <summary>
		/// 查询是否同源供应商
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> CheckSameSupplier(Context ctx, string message)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			CheckSameSupplierFilter request = JsonConvertUtils.DeserializeObject<CheckSameSupplierFilter>(message);
			SameSupplierDto entity = new SameSupplierDto();

			var pars = new List<SqlParam>() {
				new SqlParam("@CUST_CODE", KDDbType.String, request.CustomerCode),
				new SqlParam("@ITEM_NO", KDDbType.String, request.ItemNumber),
				new SqlParam("@PurchaseDate", KDDbType.DateTime, request.PurchaseDate),
				new SqlParam("@CompanyId", KDDbType.String, request.CompanyId)
			};

			var sql = $@"/*dialect*/select top 1 * from (
                    select  pod.VAT_PRICE ,DATEDIFF(DAY,po.PO_DATE,pod.FTD) AS DeliveryDay,po.VDR_CODE,so.SO_DATE 
					from M_SO_MSTR so
					inner join T_BD_JDCODECONVERTERPCODE jd on jd.ERPCODE=so.COMP_CODE
					inner join M_SOD_DET sod on so.New_SO_NO=sod.New_SO_NO
					inner join M_POD_DET pod on sod.SO_NO=pod.SO_NO and pod.SO_SEQ=sod.SEQ_NO and pod.COMP_CODE=sod.COMP_CODE
					inner join M_PO_MSTR po on pod.New_PO_NO =po.New_PO_NO
					where jd.JDCODE=@CompanyId and  so.CUST_CODE=@CUST_CODE and sod.ITEM_NO=@ITEM_NO  and po.PO_DATE<@PurchaseDate
                    union all
                    select t6.FTAXPRICE VAT_PRICE ,t6.DeliveryDay,t6.VDR_CODE,t2.FDATE SO_DATE  from T_SAL_ORDERENTRY t1
                    inner join T_SAL_ORDER t2 on t1.FID=t2.FID
                    inner join T_BD_MATERIAL t3 on t1.FMATERIALID=t3.FMATERIALID
                    inner join T_BD_CUSTOMER t4 on t2.FCUSTID=t4.FCUSTID
                    inner join T_ORG_ORGANIZATIONS t5 on t1.FSupplyTargetOrgId=t5.FORGID
                    inner JOIN (
			                    select t6.FTAXPRICE,DATEDIFF(DAY,t4.FDATE,t3.FDELIVERYDATE) AS DeliveryDay,t7.FNUMBER VDR_CODE
			                    ,t2.FDEMANDBILLENTRYID,t5.FNUMBER ItemNo
			                    from
			                    T_PUR_POORDERENTRY t1
			                    left join T_PUR_POORDERENTRY_R t2 on t1.FENTRYID=t2.FENTRYID
			                    left join T_PUR_POORDERENTRY_D t3 on t1.FENTRYID=t3.FENTRYID
			                    left join T_PUR_POORDER t4 on t1.FID=t4.FID
			                    left join T_BD_MATERIAL t5 on t5.FMATERIALID=t1.FMATERIALID
			                    left join T_PUR_POORDERENTRY_F t6 on t1.FENTRYID=t6.FENTRYID
			                    left join t_BD_Supplier t7 on t4.FSupplierId=t7.FSupplierId
			                    where t2.FDEMANDBILLENTRYID>0 and t4.FDATE<@PurchaseDate
			                    union
			                    select t10.FTAXPRICE,DATEDIFF(DAY,t4.FDATE,t3.FDELIVERYDATE) AS DeliveryDay,t11.FNUMBER VDR_CODE
			                    ,t8.FSALEORDERENTRYID FDEMANDBILLENTRYID,t9.FNUMBER ItemNo
			                    from T_PUR_POORDERENTRY t1
			                    left join T_PUR_POORDERENTRY_R t2 on t1.FENTRYID=t2.FENTRYID
			                    left join T_PUR_POORDERENTRY_D t3 on t1.FENTRYID=t3.FENTRYID
			                    left join T_PUR_POORDER t4 on t1.FID=t4.FID
			                    left join T_PUR_POORDERENTRY_LK t5 on t1.FENTRYID=t5.FENTRYID
			                    left join T_PUR_REQENTRY_LK t6 on t6.FENTRYID=t5.FSID
			                    left join T_PLN_PLANORDER_LK t7 on t6.FSID=t7.FID
			                    left JOIN T_PLN_PLANORDER_B t8 ON t7.FSBILLID=t8.FID
			                    left join T_BD_MATERIAL t9 on t9.FMATERIALID=t1.FMATERIALID
			                    left join T_PUR_POORDERENTRY_F t10 on t1.FENTRYID=t10.FENTRYID
			                    left join t_BD_Supplier t11 on t4.FSupplierId=t11.FSupplierId
			                    where t8.FSALEORDERENTRYID>0 and t4.FDATE<@PurchaseDate
			                    ) t6 on t1.FENTRYID=t6.FDEMANDBILLENTRYID and t3.FNUMBER=t6.ItemNo
                    where t5.FNUMBER=@CompanyId and t4.FNUMBER =@CUST_CODE and t3.FNUMBER=@ITEM_NO
                    ) datas 
                    order by SO_DATE desc ";

			var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
			foreach (var data in datas)
			{
				var supplierCode = data["VDR_CODE"].ToString();
				if (supplierCode == request.SupplierCode)
				{
					entity.IsSameSupplier = true;
					entity.PurchasePrice = Convert.ToDecimal(data["VAT_PRICE"]);
					entity.DeliveryDay = Convert.ToInt32(data["DeliveryDay"]);
				}
			}
			response.Message = "获取成功";
			response.Data = entity;
			response.Code = ResponseCode.Success;
			return response;
		}

		/// <summary>
		/// 更新物料体积
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> UpItemVolume(Context ctx, string message)
		{

			ItemVolumeEntity entity = JsonConvertUtils.DeserializeObject<ItemVolumeEntity>(message);
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				if (entity != null)
				{
					var sql = $@"/*dialect*/update t1 set FVOLUME={entity.Volume} from t_BD_MaterialBase t1,t_BD_Material t2 where t1.FMATERIALID=t2.FMATERIALID and FNUMBER='{entity.ItemNo}' ";
					DBServiceHelper.Execute(ctx, sql);
				}
				response.Message = "更新成功";
				response.Code = ResponseCode.Success;

			}
			catch (Exception ex)
			{

				response.Message = "更新失败：" + ex.Message;
				response.Code = ResponseCode.Exception;
			}
			return response;
		}


		/// <summary>
		/// CRM同步客诉到金蝶
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> SynCompanyComplaint(Context ctx, string message)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			if (string.IsNullOrWhiteSpace(message))
			{
				response.Message = "参数信息不能为空";
				response.Code = ResponseCode.ModelError;
				return response;
			}
			CompanyComplaintRequest request = JsonConvertUtils.DeserializeObject<CompanyComplaintRequest>(message);
			if (string.IsNullOrWhiteSpace(request.ComplaintNo))
			{
				response.Message = "单据编号不能为空";
				response.Code = ResponseCode.ModelError;
				return response;
			}
			return BasicDataSyncServiceHelper.SynCompanyComplaintService(ctx, request);
		}

		/// <summary>
		/// 周合格率统计
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public ResponseMessage<dynamic> WeeklyPassRateStatistics(Context ctx)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			return BasicDataSyncServiceHelper.WeeklyPassRateStatisticsService(ctx);
		}

	}

}
