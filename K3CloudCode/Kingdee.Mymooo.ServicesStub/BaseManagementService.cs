using Kingdee.BOS.Core.CommonFilter.ConditionVariableAnalysis;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.Mymooo.App.Core.BaseManagement;
using Kingdee.Mymooo.Business.PlugIn.BaseManagement;
using Kingdee.Mymooo.Business.PlugIn.PurchaseManagement;
using Kingdee.Mymooo.Business.PlugIn.ReceivablesManagement;
using Kingdee.Mymooo.Business.PlugIn.SalesManagement;
using Kingdee.Mymooo.Business.PlugIn.StockManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MaterialService = Kingdee.Mymooo.Business.PlugIn.BaseManagement.MaterialService;

namespace Kingdee.Mymooo.WebApi.ServicesStub
{
	public class BaseManagementService : KDBaseService
	{
		public BaseManagementService(KDServiceContext context) : base(context)
		{
		}

		public string TryGetOrAddMaterial()
		{
			string data = string.Empty;
			ResponseMessage<MaterialInfo> response = new ResponseMessage<MaterialInfo>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				MaterialService materialService = new MaterialService();
				var request = JsonConvertUtils.DeserializeObject<MaterialInfo>(data);
				response.Data = MaterialServiceHelper.TryGetOrAdd(ctx, request, new List<long>() { 0 });
				response.Code = ResponseCode.Success;

			}
			catch (Exception ex)
			{
				response.Code = ResponseCode.Exception;
				response.Message = ex.Message;
			}
			var settiongs = new JsonSerializerSettings()
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			};
			return JsonConvert.SerializeObject(response, Formatting.None, settiongs);
		}
		public string TryBomGetOrAddMaterial()
		{
			string data = string.Empty;
			ResponseMessage<MaterialInfo> response = new ResponseMessage<MaterialInfo>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				MaterialService materialService = new MaterialService();
				var request = JsonConvertUtils.DeserializeObject<MaterialInfo>(data);
				response.Data = MaterialServiceHelper.TryBomGetOrAdd(ctx, request);
				response.Code = ResponseCode.Success;

			}
			catch (Exception ex)
			{
				response.Code = ResponseCode.Exception;
				response.Message = ex.Message;
			}
			var settiongs = new JsonSerializerSettings()
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			};
			return JsonConvert.SerializeObject(response, Formatting.None, settiongs);
		}

		public string MaterialGroup()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				var datas = JsonConvert.DeserializeObject<List<SalesOrderBillRequest.Productsmallclass>>(data);
				response = MaterialServiceHelper.GroupSave(ctx, datas);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			var settiongs = new JsonSerializerSettings()
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			};
			return JsonConvert.SerializeObject(response, Formatting.None, settiongs);
		}

		public string DeleteRepeatMaterial()
		{
			ResponseMessage<dynamic> response;
			try
			{
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				MaterialService materialService = new MaterialService();
				response = materialService.DeleteRepeatMaterial(ctx);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		public string SaveMaterial()
		{
			ResponseMessage<dynamic> response;
			try
			{
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				MaterialService materialService = new MaterialService();
				response = materialService.SaveMaterial(ctx);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		public string SyncPurchaseSmall()
		{
			string data = string.Empty;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				PurchaseBusiness purchaseBusiness = new PurchaseBusiness();
				return JsonConvertUtils.SerializeObject(purchaseBusiness.PurchaseSmall(ctx, data));
			}
			catch (Exception ex)
			{
				return JsonConvertUtils.SerializeObject(new ResponseMessage<dynamic>() { Code = ResponseCode.Exception, ErrorMessage = ex.Message });
			}
		}

		/// <summary>
		/// 获取事业部列表
		/// </summary>
		/// <returns></returns>
		public string GetBusinessDivisionList()
		{
			ResponseMessage<dynamic> response;
			string data = string.Empty;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				BaseDataBusiness business = new BaseDataBusiness();
				response = business.GetBusinessDivisionList(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 获取大类列表
		/// </summary>
		/// <returns></returns>
		public string GetItemGrpList()
		{
			ResponseMessage<dynamic> response;
			try
			{
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				MaterialService business = new MaterialService();
				response = business.GetItemGrpList(ctx);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 获取仓库列表
		/// </summary>
		/// <returns></returns>
		public string GetWarehouseList()
		{
			ResponseMessage<dynamic> response;
			try
			{
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				StockBusiness business = new StockBusiness();
				response = business.GetWarehouseList(ctx);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 获取组织列表
		/// </summary>
		/// <returns></returns>
		public string GetOrgList()
		{
			ResponseMessage<dynamic> response;
			try
			{
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				BaseDataBusiness business = new BaseDataBusiness();
				response = business.GetOrgList(ctx);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 获取基础资料控制策略
		/// </summary>
		/// <returns></returns>
		public string GetBDCtrlPolicy()
		{
			ResponseMessage<dynamic> response;
			string data = string.Empty;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				BaseDataBusiness business = new BaseDataBusiness();
				response = business.GetBDCtrlPolicy(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}


		/// <summary>
		/// 获取收款条件列表
		/// </summary>
		/// <returns></returns>
		public string GetPaymentInfo()
		{
			ResponseMessage<dynamic> response;
			string data = string.Empty;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				BaseDataBusiness business = new BaseDataBusiness();
				response = business.GetPaymentInfo(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 获取货币信息列表
		/// </summary>
		/// <returns></returns>
		public string GetCurrencyInfo()
		{
			ResponseMessage<dynamic> response;
			string data = string.Empty;
			try
			{
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				BaseDataBusiness business = new BaseDataBusiness();
				response = business.GetCurrencyInfo(ctx);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 获取税率信息列表
		/// </summary>
		/// <returns></returns>
		public string GetVatList()
		{
			ResponseMessage<dynamic> response;
			string data = string.Empty;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				BaseDataBusiness business = new BaseDataBusiness();
				response = business.GetVatList(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}


		/// <summary>
		/// 查询是否同源供应商
		/// </summary>
		/// <returns></returns>
		public string CheckSameSupplier()
		{
			ResponseMessage<dynamic> response;
			string data = string.Empty;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				BaseDataBusiness business = new BaseDataBusiness();
				response = business.CheckSameSupplier(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 更新物料体积
		/// </summary>
		/// <returns></returns>
		public string UpItemVolume()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				BaseDataBusiness business = new BaseDataBusiness();
				response = business.UpItemVolume(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		public string MaterialAllocate()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				//MaterialService materialService = new MaterialService();
				var request = JsonConvertUtils.DeserializeObject<List<ENGBomInfo>>(data);
				response = ENGBomServiceHelper.MaterialAllocate(ctx, request);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvertUtils.SerializeObject(response);
		}
		public string BomAllocate()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				//MaterialService materialService = new MaterialService();
				var request = JsonConvertUtils.DeserializeObject<List<ENGBomInfo>>(data);
				response = ENGBomServiceHelper.BomAllocate(ctx, request);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvertUtils.SerializeObject(response);
		}

		/// <summary>
		/// CRM同步客诉到金蝶
		/// </summary>
		/// <returns></returns>
		public string SynCompanyComplaint()
		{
			ResponseMessage<dynamic> response;
			string data = string.Empty;
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				BaseDataBusiness business = new BaseDataBusiness();
				response = business.SynCompanyComplaint(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		/// <summary>
		/// 供应商小类检验评分-周合格率统计
		/// </summary>
		/// <returns></returns>
		public string WeeklyPassRateStatistics()
		{
			ResponseMessage<dynamic> response;
			string data = string.Empty;
			try
			{
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				BaseDataBusiness business = new BaseDataBusiness();
				response = business.WeeklyPassRateStatistics(ctx);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvert.SerializeObject(response);
		}

		public string MaterialAllocateV2()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				MaterialService materialService = new MaterialService();
				var request = JsonConvertUtils.DeserializeObject<List<long>>(data);
				response = MaterialServiceHelper.MaterialAllocate(ctx, request);
			}
			catch (Exception ex)
			{
				response.Code = ResponseCode.Exception;
				response.Message = ex.Message;
			}
			return JsonConvertUtils.SerializeObject(response);
		}

		public string SyncReceivableOrser()
		{
			string data = string.Empty;

			using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
			{
				var task = reader.ReadToEndAsync();
				data = task.Result;
			}
			var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);

			ReceivableOrderBusiness purOrder = new ReceivableOrderBusiness();
			var response = purOrder.SyncReceivableOrser(ctx);

			return JsonConvertUtils.SerializeObject(response);
		}
	}
}
