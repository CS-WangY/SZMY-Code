using com.mymooo.mall.business.Service;
using com.mymooo.mall.business.Service.SalesService;
using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Model.InquiryOrder;
using com.mymooo.mall.core.Model.Message;
using com.mymooo.mall.core.Model.Price;
using com.mymooo.mall.core.Model.Quotation;
using com.mymooo.mall.core.Model.Stock;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.CustomerInformation;
using com.mymooo.mall.core.SqlSugarCore.SalesBusiness;
using com.mymooo.mall.core.SqlSugarCore.SystemManagement;
using com.mymooo.mall.core.Srm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.core.Utils.JsonConverter;
using mymooo.core.Utils.Service;
using mymooo.weixinWork.SDK.Application;
using mymooo.weixinWork.SDK.WeixinWorkMessage.Model;



namespace com.mymooo.mall.Controllers
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="logger"></param>
	/// <param name="mymoooContext"></param>
	/// <param name="scmService"></param>
	/// <param name="workbenchService"></param>
	/// <param name="quotationService"></param>
	/// <param name="quotationCacheService"></param>
	/// <param name="salesOrderService"></param>
	/// <param name="httpService"></param>
	/// <param name="applicationServiceClient"></param>
	public partial class QuotationController(ILogger<QuotationController> logger, MallContext mymoooContext, ScmService scmService, WorkbenchService workbenchService,
		QuotationService quotationService, QuotationCacheService quotationCacheService, SalesOrderService salesOrderService, HttpService httpService, ApplicationServiceClient applicationServiceClient) : BaseController
	{

		private readonly ILogger<QuotationController> _logger = logger;
		private readonly MallContext _mymoooContext = mymoooContext;
		private readonly ScmService _scmService = scmService;
		private readonly WorkbenchService _workbenchService = workbenchService;
		private readonly HttpService _httpService = httpService;
		private readonly QuotationService _quotationService = quotationService;
		private readonly QuotationCacheService _quotationCacheService = quotationCacheService;
		private readonly SalesOrderService _salesOrderService = salesOrderService;
		private readonly ApplicationServiceClient _applicationServiceClient = applicationServiceClient;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost]
		public ResponseMessage<ConfirmQuotationViewModel> Confirm([FromBody] ConfirmQuotationRequest request)
		{
			ResponseMessage<ConfirmQuotationViewModel> response = new();

			if (request == null)
			{
				response.Code = ResponseCode.ModelError;
				response.ErrorMessage = "出错啦,没有提交的数据";
				return response;
			}

			// 加入一些服务器端校验.
			if (request.QuotationItems == null)
			{
				response.Code = ResponseCode.ModelError;
				response.ErrorMessage = "出错啦,没有报价项目";
				return response;
			}
			if (request.IsNonStandard && request.QuotationItems.Any(it => it.ProductSmallClassId == null))
			{
				response.Code = ResponseCode.ModelError;
				response.ErrorMessage = "非标件和标准件需分开下单";
				return response;
			}
			foreach (var it in request.QuotationItems)
			{
				if (it.Remark.Length > 250 || it.PurchaseRemark.Length > 250)
				{
					response.Code = ResponseCode.ModelError;
					response.ErrorMessage = "项目[" + it.Product.Code + "](" + it.Product.Name + ")的备注字符太多了,不要超过250个字,确有需要,可以上传附件文件,在附件文件中标注.";
					return response;
				}
				if (it.InsideRemark.Length > 500)
				{
					response.Code = ResponseCode.ModelError;
					response.ErrorMessage = "项目[" + it.Product.Code + "](" + it.Product.Name + ")的备注(供应链)字符太多了,确有需要,可以上传附件文件,在附件文件中标注.";
					return response;
				}
			}

			var weChatCode = _mymoooContext.User?.Code;

			if (request.CompanyId.HasValue)  //补充公司编码
			{
				var company = _mymoooContext.SqlSugar.Queryable<Company>().Where(r => r.Id == request.CompanyId).First();
				if (company != null)
				{
					request.CompanyCode = company.Code;
					if (request.Buyer != null)
					{
						request.Buyer.CompanyCode = company.Code;
					}
				}
			}
			var Custcode = _mymoooContext.SqlSugar.Ado.GetString(@"select a.FCM_CUST_CODE from F_CUST_MSTR a where  exists (
                                                                select 1 from F_CUST_USER b where b.FUM_CUST_ID = b.FUM_CUST_ID and b.FUM_USER_ID=@CustId
                                                                )", new { CustId = request?.Buyer?.CustomerId });
			if (!string.IsNullOrEmpty(Custcode) && request?.Buyer != null)
			{
				request.Buyer.CustomerCode = Custcode;
			}




#pragma warning disable CS8602 // 解引用可能出现空引用。
			List<ConfirmQuotationItemViewModel> quotationItemModels = _quotationService.PrepareQuotationItemModelV3(request.QuotationItems.ToList(), request.CompanyCode, request.Buyer != null ? (long?)request.Buyer.CustomerId : null, request.IsNonStandard);
#pragma warning restore CS8602 // 解引用可能出现空引用。

			var address = GetAddress(request.AddressId);


			for (int i = 0; i < request.QuotationItems.Count(); i++)
			{
				var item = quotationItemModels[i];
				var row = ((IList<InputQuotationItemViewModel>)request.QuotationItems)[i];
				// 2023 -5-16 客户型号输入啥就是啥, 
				item.CustItemName = row.CustomProductName;
				item.CustomCode = row.CustomProductCode;

				// 附件把路径拼上去
				if (!string.IsNullOrEmpty(row.AttaFilesName))
				{
					var files = row.AttaFilesName.Split('|');
					string[] nfs = new string[files.Length];
					for (int j = 0; j < files.Length; j++)
					{
						nfs[j] = row.AttaPath + files[j];
					}
					item.AttaFilesName = string.Join("|", nfs);
				}
			}
			//增加目录书判断, 
			if (quotationItemModels != null && quotationItemModels.Count > 1)
			{
				var bookProduct = quotationItemModels.FirstOrDefault(e => e.ProductSmallClass != null && e.ProductSmallClass.Id == 191);
				var otherProduct = quotationItemModels.FirstOrDefault(e => e.ProductSmallClass == null || (e.ProductSmallClass != null && e.ProductSmallClass.Id != 191));
				if (bookProduct != null && otherProduct != null)
				{
					response.Code = ResponseCode.Exception;
					response.ErrorMessage = "【" + bookProduct.Product.Code + "】 属于目录书产品类别,请单独下单!";
					return response;
				}
			}
			if (string.IsNullOrWhiteSpace(request.InquiryNumber))
			{
				request.InquiryNumber = _mymoooContext.SqlSugar.Ado.GetString("select NEXT VALUE FOR billseq");
			}
			var result = new ConfirmQuotationViewModel
			{
				Receive = address,
				QuotationItems = quotationItemModels,
				Buyer = request.Buyer,
				//把入参返回给前端, 这里去空格了,就为了,点返回按钮保持数据
				OriginalModel = JsonSerializerOptionsUtils.Serialize(request),
				CustomerPurchaseNumber = request.CustomerPurchaseNumber,
				SalesOrganizationCode = request.SalesOrganizationCode,
				SalesOrganizationName = request.SalesOrganizationName,
				SpecialPrice = request.SpecialPrice,
				FileName = request.FileName,
				IsInternal = request.IsInternal,
				InquiryNumber= request.InquiryNumber,
                IsNonStandard = request.IsNonStandard,
			    UploadPath=request.UploadPath
            };

			response.Code = ResponseCode.Success;
			response.Data = result;
			return response;
		}


		private CustomerAddress GetAddress(long addressId)
		{

			return _mymoooContext.SqlSugar.Queryable<CustomerAddress>()
			   .Where(p => p.Id == addressId)
			   .First();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[AllowAnonymous]
		[HttpPost]
		public ActionResult Test([FromBody] ReplaceModelQueryModelReplaceRequest request)
		{

			// _scmService.ReplaceModelQueryModelReplace(_mymoooContext, request);
			//var x = _mymoooContext.User;
			//var x = User.ToString();
			//string ss = " A     |\t\n\rB|\as| ";
			//var xx = Regex.Replace(ss, @"\s", "");
			//var x = User.ToString();
			//weixinwork
			//MessagesHelp<List<CustomerAddress>> message = MessagesHelp<List<CustomerAddress>>();
			//////根据企业编码拿到企业信息
			////var company = _companyService.GetCompanyForCode(companyCode);
			////if (company == null)
			////{
			////    message.Message = "企业信息不存在";
			////    return Json(message, JsonRequestBehavior.AllowGet);
			////}
			////var addressesOfCompany = _addressService.Multiple(company.Id);
			//message.IsSuccess = true;
			//message.Data = "";
			_quotationService.GetSaleMan(0, new Guid("DEE03314-9D3A-447A-873D-B6EF048E3377"));
			return Json("ok");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[AllowAnonymous]
		public ActionResult SetQuotationByCAPP([FromBody] FbQuotationCacheDto data)
		{

			ResponseMessage<dynamic> result = new();
			try
			{
				var isExit = _mymoooContext.SqlSugar.Queryable<InquiryOrder>().LeftJoin<InquiryOrderDetail>((a, b) => a.InquiryId == b.InquiryId).Where((a, b) => a.InquiryNumber == data.SalesOrderNo && b.ProductNumber == data.DrawingNumber).Any();
				if (isExit)
				{
					var isSuccess = _mymoooContext.RedisCache.HashSet(data);
					if (isSuccess)
					{
						result.Code = ResponseCode.Success;
						result.Message = "写入成功";
					}
					else
					{
						result.Code = ResponseCode.ThirdpartyError;
						result.Message = "写入失败";
					}
				}
				else
				{
					result.Code = ResponseCode.NoExistsData;
					result.Message = "单号/型号不存在";
				}

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "CAPP报价写入缓存:");
				result.Code = ResponseCode.ThirdpartyError;
				result.Message = "写入失败";
			}
			return Json(result);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[AllowAnonymous]
		public ActionResult UpdateCAPPQuoteInfo([FromBody] List<CappQuotationRequest> request)
		{
			MessagesHelp<dynamic>? response = new();
			if (request == null || request.Count <= 0)
			{
				response.Code = ResponseCode.Success;
				return Json(response);
			}
			foreach (var item in request)
			{
				var data = _mymoooContext.RedisCache.HashGet(new FbQuotationCacheDto() { SalesOrderNo = item.QuotationOrderNo, DrawingNumber = item.DrawingNumber ?? "" });
				var data2 = JsonSerializerOptionsUtils.Deserialize<CappQuotationOrderDto>(JsonSerializerOptionsUtils.Serialize(data));
				if (data2 != null)
				{

					data2.QuotationOrderNo = item.QuotationOrderNo;
					data2.QuotationOrderLineNo = item.QuotationOrderLineNo;
					var res = _httpService.InvokeWebService($"pdm/{_mymoooContext.ApigatewayConfig.EnvCode}/api/quotation-order/add-or-update-quote-info", JsonSerializerOptionsUtils.Serialize(data2));
					_logger.LogError($"回写CAPP:{JsonSerializerOptionsUtils.Serialize(data2)};返回结果:{res}");
					if (res != null)
					{
						response = JsonSerializerOptionsUtils.Deserialize<MessagesHelp<dynamic>>(res);
					}
				}
			}
			return Json(response);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[AllowAnonymous]
		[HttpPost]
		public ActionResult GetQuotationByCapp([FromBody] List<QueryCappCacheRequest> request)
		{

			ResponseMessage<List<FbQuotationCacheDto>> response = new()
			{
				Data = []
			};
			if (request == null || request.Count <= 0)
			{
				response.Code = ResponseCode.Success;
				response.Data = [];
				return Json(response);
			}
			foreach (var item in request)
			{
				var data = _mymoooContext.RedisCache.HashGet(new FbQuotationCacheDto() { SalesOrderNo = item.InquiryOrder, DrawingNumber = item.DrawingNumber ?? "" });
				if (data != null)
				{
					response.Data.Add(data);
				}
			}
			return Json(response);
		}

		/// <summary>
		/// MQ  回调任务, 计算供应商,和更新供应商安全库存.
		/// 入参为分解单明细型号 的产品Id,产品型号,产品数量.
		/// </summary>
		/// <param name="Quotation"></param>
		/// <returns></returns>
		public IActionResult CalSupplierAndsafetyStock([FromBody] QuotationMqReq Quotation)
		{
			ResponseMessage<QuotationMqReq> result = new()
			{
				Data = Quotation
			};
			string Sql = @" Select id.FID_DETAIL_ID resolvedItemId, id.ShortNumber,id.FID_NUM Qty, id.FID_PRD_ID ProductId,id.FID_PRD_CODE ProductCode From  F_CUST_INQUIRY_DETAIL as id
                           left Join F_CUST_INQUIRY_MSTR as im on im.FIM_INQ_ID = id.FID_INQ_MSTR_ID
                            where im.FIM_INQ_CODE = @qutationNumber ";
			var priceReqs = _mymoooContext.SqlSugar.Ado.SqlQuery<SupplierPriceReq>(Sql,
				new
				{
					qutationNumber = Quotation.quotationNumber
				}
				).ToList();

			List<SupplierPriceApiReq> apiDataBody = [];
			for (int i = 0; i < priceReqs.Count; i++)
			{
				SupplierPriceApiReq req = new()
				{
					ProductId = priceReqs[i].ProductId,
					Number = priceReqs[i].ProductCode,
					Qty = priceReqs[i].Qty,
					ShortNumber = priceReqs[i].ShortNumber,
					SupplierCode = string.Empty
				};
				var item = apiDataBody.Where(r => r.Number == req.Number).FirstOrDefault();
				if (item != null)
				{
					item.Qty += req.Qty;  // 同型号数量累加
				}
				else
				{
					apiDataBody.Add(req);
				}
			}

			var SupplierPriceList = _scmService.GetSuplierPrice(apiDataBody);

			List<ResolvedOrderItem> resolvedOrderItems = []; //当前采购单的所有分解项目
			if (SupplierPriceList.Count == 0)
			{
				result.Code = ResponseCode.Success;
				result.Message = "成功处理报价单供应商匹配和安全库存业务(供应商匹配Count==0 退出)";
				return Json(result);
			}
			else
			{
				// 目的是更新, 分解单明细的3 个字段.  				SupplierUnitPrice                 SupplierCode                 SupplierName 
				// 查询分解单号,下属项目
				Sql = @"Select  * From ResolvedOrderItem Where InquiryItemId in (
						Select id.FID_DETAIL_ID From  F_CUST_INQUIRY_DETAIL as id
                           left Join F_CUST_INQUIRY_MSTR as im on im.FIM_INQ_ID = id.FID_INQ_MSTR_ID
                            where im.FIM_INQ_CODE = @qutationNumber )";
				resolvedOrderItems = [.. _mymoooContext.SqlSugar.Ado.SqlQuery<ResolvedOrderItem>(Sql,
					new
					{
						qutationNumber = Quotation.quotationNumber
					}
					)];
				if (resolvedOrderItems != null)
				{
					List<ResolvedOrderItem> updateItems = [];
					foreach (var item in resolvedOrderItems)
					{
						var productCodePrice = SupplierPriceList.Where(r => r.ProductCode == item.ProductCode).OrderBy(r=> r.SupplierUnitPrice).FirstOrDefault();
						if (productCodePrice != null)
						{
							var currentItem = item;
							currentItem.SupplierUnitPrice = productCodePrice.SupplierUnitPrice;
							currentItem.SupplierCode = productCodePrice.SupplierCode;
							currentItem.SupplierName = productCodePrice.SupplierName;  // 供应商Id 不用了
							updateItems.Add(currentItem);
						}

						if (updateItems.Count > 0)  // 执行更新供应商匹配结果.
						{
							_mymoooContext.SqlSugar.Updateable(updateItems).UpdateColumns(it => new { it.SupplierName, it.SupplierCode, it.SupplierUnitPrice }).ExecuteCommand();

							// 更新报价单明细,字段报价部分字段值.
							var inqDetailIdList = updateItems.Select(r => r.InquiryItemId).Distinct().ToList();
							var quoList = _mymoooContext.SqlSugar.Queryable<InquiryQuotationOrderDetail>().Where(r => inqDetailIdList.Contains(r.InquiryDetailId)).ToList();
							foreach (var it in quoList)
							{
								var inq = updateItems.Where(r => r.InquiryItemId == it.InquiryDetailId).First();
								if (inq == null) { continue; }
								it.AutoUnitPrice = inq.SupplierUnitPrice;
								it.AutoPriceSource = (int?)inq.PriceSource;
							}
							_mymoooContext.SqlSugar.Updateable(quoList).UpdateColumns(it => new { it.AutoDeliveryDays, it.AutoDeliverySource }).ExecuteCommand();
						}
					}
				}
			}

			// 查到供应商安全库存,更新库存

			SafetyStockQueryReq safetyStockQuery = new()
			{
				ProductItemList = []
			};
			foreach (var it in SupplierPriceList)
			{
				safetyStockQuery.ProductItemList.Add(new SafetyStockQuerySimpleDto
				{
					ProductModel = it.ProductCode,
					SupplierCode = it.SupplierCode
				});
			}

			List<SafetyStockQuerySimpleDto> safeStockList = _scmService.SupplierSafetyStockQuery(safetyStockQuery);
			if (safeStockList.Count == 0)
			{
				result.Code = ResponseCode.Success;
				result.Message = "成功处理报价单供应商匹配和安全库存业务(安全库存Count==0 退出)";
				return Json(result);
			}

			int configStockRadio = 100; //库存比例(小于此比例可更新货期) 单位(%) 现值 100

			var tempString = _mymoooContext.RedisCache.HashGet(new SystemProfile() { Key = "StockRadio" }, p => p.Value);
			if (!string.IsNullOrEmpty(tempString))
			{
				configStockRadio = Convert.ToInt32(tempString);
			}

			if (resolvedOrderItems != null)
			{
				List<FacCalculate> facCalculate = GetFacCalculate(); // 获取未来20天的工厂生产日历
				List<ResolvedOrderItem> updateItems = [];
				foreach (var item in resolvedOrderItems)
				{
					if (item.DeliverySource == DeliverySource.Stock) { continue; }   //如果 有ERP 库存交期,不予处理
					var productCodeStock = safeStockList.Where(r => r.ProductModel == item.ProductCode).FirstOrDefault();
					if (productCodeStock == null) { continue; }  // 没有供应商库存,也不处理
					if (item.Quantity <= productCodeStock.StockQty * configStockRadio / 100) // 需求数量在安全库存之下 执行处理
					{
						var currentItem = item;
						currentItem.DispatchDays = productCodeStock.StockDays;
						currentItem.DeliverySource = DeliverySource.Suplier;
						updateItems.Add(currentItem);
					}
					//目的是更新分解单 明细的2个字段.和报价单明细的 几个字段
					if (updateItems.Count > 0)
					{
						_mymoooContext.SqlSugar.Updateable(updateItems).UpdateColumns(it => new { it.DispatchDays, it.DeliverySource }).ExecuteCommand();

						// 更新报价单明细,字段报价部分字段值.
						var inqDetailIdList = updateItems.Select(r => r.InquiryItemId).Distinct().ToList();
						var quoList = _mymoooContext.SqlSugar.Queryable<InquiryQuotationOrderDetail>().Where(r => inqDetailIdList.Contains(r.InquiryDetailId)).ToList();
						foreach (var it in quoList)
						{
							var inq = updateItems.Where(r => r.InquiryItemId == it.InquiryDetailId).First();
							if (inq == null) { continue; }
							it.AutoDeliveryDays = inq.DispatchDays;
							it.AutoDeliverySource = (int?)inq.DeliverySource;
							it.DeliveDays = (inq.DispatchDays == null) ? -1 : (decimal)inq.DispatchDays;
							it.DeliverySource = (int?)inq.DeliverySource;
							if (inq.DispatchDays != null)
							{
								it.RequirementDate = HandleCalendarDate((int)inq.DispatchDays, facCalculate);
								it.RequirementDays = inq.DispatchDays;
							}
						}
						_mymoooContext.SqlSugar.Updateable(quoList).UpdateColumns(it => new { it.AutoDeliveryDays, it.AutoDeliverySource, it.DeliveDays, it.DeliverySource, it.RequirementDate, it.RequirementDays }).ExecuteCommand();
					}
				}
			}

			result.Code = ResponseCode.Success;
			result.Message = "成功处理报价单供应商匹配和安全库存业务" + JsonSerializerOptionsUtils.Serialize(safeStockList);
			return Json(result);

		}



        /// <summary>
		/// 该方法仅仅用于测试价目表和安全库存. 不执行写入
		/// </summary>
		/// <param name="Quotation"></param>
		/// <returns></returns>
		// 
        // 入参为分解单明细型号 的产品Id,产品型号,产品数量.
        public IActionResult TestSupplierAndsafetyStock([FromBody] QuotationMqReq Quotation)
        {
            ResponseMessage<QuotationMqReq> result = new()
            {
                Data = Quotation
            };
            string Sql = @" Select id.FID_DETAIL_ID resolvedItemId, id.ShortNumber,id.FID_NUM Qty, id.FID_PRD_ID ProductId,id.FID_PRD_CODE ProductCode From  F_CUST_INQUIRY_DETAIL as id
                           left Join F_CUST_INQUIRY_MSTR as im on im.FIM_INQ_ID = id.FID_INQ_MSTR_ID
                            where im.FIM_INQ_CODE = @qutationNumber ";
            var priceReqs = _mymoooContext.SqlSugar.Ado.SqlQuery<SupplierPriceReq>(Sql,
                new
                {
                    qutationNumber = Quotation.quotationNumber
                }
                ).ToList();


            List<SupplierPriceApiReq> apiDataBody = [];
            for (int i = 0; i < priceReqs.Count; i++)
            {
                SupplierPriceApiReq req = new()
                {
                    ProductId = priceReqs[i].ProductId,
                    Number = priceReqs[i].ProductCode,
                    Qty = priceReqs[i].Qty,
                    ShortNumber = priceReqs[i].ShortNumber,
                    SupplierCode = string.Empty
                };
                var item = apiDataBody.Where(r => r.Number == req.Number).FirstOrDefault();
                if (item != null)
                {
                    item.Qty = item.Qty + req.Qty;  // 同型号数量累加
                }
                else
                {
                    apiDataBody.Add(req);
                }
            }

            var SupplierPriceList = _scmService.GetSuplierPrice(apiDataBody);

            List<ResolvedOrderItem> resolvedOrderItems = []; //当前采购单的所有分解项目
            if (SupplierPriceList.Count == 0)
            {
                result.Code = ResponseCode.Success;
                result.Message = "成功处理报价单供应商匹配和安全库存业务(供应商匹配Count==0 退出)";
                return Json(result);
            }
            else
            {
                // 目的是更新, 分解单明细的3 个字段.  				SupplierUnitPrice                 SupplierCode                 SupplierName 
                // 查询分解单号,下属项目
                Sql = @"Select  * From ResolvedOrderItem Where InquiryItemId in (
						Select id.FID_DETAIL_ID From  F_CUST_INQUIRY_DETAIL as id
                           left Join F_CUST_INQUIRY_MSTR as im on im.FIM_INQ_ID = id.FID_INQ_MSTR_ID
                            where im.FIM_INQ_CODE = @qutationNumber )";
                resolvedOrderItems = _mymoooContext.SqlSugar.Ado.SqlQuery<ResolvedOrderItem>(Sql,
                    new
                    {
                        qutationNumber = Quotation.quotationNumber
                    }
                    ).ToList();
                if (resolvedOrderItems != null)
                {
                    List<ResolvedOrderItem> updateItems = [];
                    foreach (var item in resolvedOrderItems)
                    {
                        var productCodePrice = SupplierPriceList.Where(r => r.ProductCode == item.ProductCode).OrderBy(r => r.SupplierUnitPrice).FirstOrDefault();
                        if (productCodePrice != null)
                        {
                            var currentItem = item;
                            currentItem.SupplierUnitPrice = productCodePrice.SupplierUnitPrice;
                            currentItem.SupplierCode = productCodePrice.SupplierCode;
                            currentItem.SupplierName = productCodePrice.SupplierName;  // 供应商Id 不用了
                            updateItems.Add(currentItem);
                        }

                        if (updateItems.Count > 0)  // 执行更新供应商匹配结果.
                        {
                          

                            // 更新报价单明细,字段报价部分字段值.
                            var inqDetailIdList = updateItems.Select(r => r.InquiryItemId).Distinct().ToList();
                            var quoList = _mymoooContext.SqlSugar.Queryable<InquiryQuotationOrderDetail>().Where(r => inqDetailIdList.Contains(r.InquiryDetailId)).ToList();
                            foreach (var it in quoList)
                            {
                                var inq = updateItems.Where(r => r.InquiryItemId == it.InquiryDetailId).First();
                                if (inq == null) { continue; }
                                it.AutoUnitPrice = inq.SupplierUnitPrice;
                                it.AutoPriceSource = (int?)inq.PriceSource;
                            }
                            
                        }
                    }
                }
            }

            // 查到供应商安全库存,更新库存

            SafetyStockQueryReq safetyStockQuery = new SafetyStockQueryReq();
            safetyStockQuery.ProductItemList = new List<SafetyStockQuerySimpleDto>();
            foreach (var it in SupplierPriceList)
            {
                safetyStockQuery.ProductItemList.Add(new SafetyStockQuerySimpleDto
                {
                    ProductModel = it.ProductCode,
                    SupplierCode = it.SupplierCode
                });
            }

            List<SafetyStockQuerySimpleDto> safeStockList = _scmService.SupplierSafetyStockQuery(safetyStockQuery);
            if (safeStockList.Count == 0)
            {
                result.Code = ResponseCode.Success;
                result.Message = "成功处理报价单供应商匹配和安全库存业务(安全库存Count==0 退出)";
                return Json(result);
            }

            int configStockRadio = 100; //库存比例(小于此比例可更新货期) 单位(%) 现值 100

            var tempString = _mymoooContext.RedisCache.HashGet(new SystemProfile() { Key = "StockRadio" }, p => p.Value);
            if (!string.IsNullOrEmpty(tempString))
            {
                configStockRadio = Convert.ToInt32(tempString);
            }

            if (resolvedOrderItems != null)
            {
                List<FacCalculate> facCalculate = GetFacCalculate(); // 获取未来20天的工厂生产日历
                List<ResolvedOrderItem> updateItems = [];
                foreach (var item in resolvedOrderItems)
                {
                    if (item.DeliverySource == DeliverySource.Stock) { continue; }   //如果 有ERP 库存交期,不予处理
                    var productCodeStock = safeStockList.Where(r => r.ProductModel == item.ProductCode).FirstOrDefault();
                    if (productCodeStock == null) { continue; }  // 没有供应商库存,也不处理
                    if (item.Quantity <= productCodeStock.StockQty * configStockRadio / 100) // 需求数量在安全库存之下 执行处理
                    {
                        var currentItem = item;
                        currentItem.DispatchDays = productCodeStock.StockDays;
                        currentItem.DeliverySource = DeliverySource.Suplier;
                        updateItems.Add(currentItem);
                    }
                    //目的是更新分解单 明细的2个字段.和报价单明细的 几个字段
                    if (updateItems.Count > 0)
                    {
                     

                        // 更新报价单明细,字段报价部分字段值.
                        var inqDetailIdList = updateItems.Select(r => r.InquiryItemId).Distinct().ToList();
                        var quoList = _mymoooContext.SqlSugar.Queryable<InquiryQuotationOrderDetail>().Where(r => inqDetailIdList.Contains(r.InquiryDetailId)).ToList();
                        foreach (var it in quoList)
                        {
                            var inq = updateItems.Where(r => r.InquiryItemId == it.InquiryDetailId).First();
                            if (inq == null) { continue; }
                            it.AutoDeliveryDays = inq.DispatchDays;
                            it.AutoDeliverySource = (int?)inq.DeliverySource;
                            it.DeliveDays = (inq.DispatchDays == null) ? -1 : (decimal)inq.DispatchDays;
                            it.DeliverySource = (int?)inq.DeliverySource;
                            if (inq.DispatchDays != null)
                            {
                                it.RequirementDate = HandleCalendarDate((int)inq.DispatchDays, facCalculate);
                                it.RequirementDays = inq.DispatchDays;
                            }
                        }
                      
                    }
                }
            }

            result.Code = ResponseCode.Success;
            result.Message = "成功测试处理报价单供应商匹配和安全库存业务" + JsonSerializerOptionsUtils.Serialize(safeStockList);
            return Json(result);

        }


		/// <summary>
		/// MQ  回调任务, 计算ERP库存.确认交期
		/// </summary>
		/// <param name="Quotation"></param>
		/// <returns></returns>
        
        public IActionResult CalErpStockDispatchDays([FromBody] QuotationMqReq Quotation)
		{
			ResponseMessage<QuotationMqReq> result = new()
			{
				Data = Quotation
			};

			string Sql = @" Select id.FID_DETAIL_ID resolvedItemId, id.ShortNumber,id.FID_NUM Qty, id.FID_PRD_ID ProductId,id.FID_PRD_CODE ProductCode From  F_CUST_INQUIRY_DETAIL as id
                           left Join F_CUST_INQUIRY_MSTR as im on im.FIM_INQ_ID = id.FID_INQ_MSTR_ID
                            where im.FIM_INQ_CODE = @qutationNumber ";
			_ = _mymoooContext.SqlSugar.Ado.SqlQuery<SupplierPriceReq>(Sql,
				new
				{
					qutationNumber = Quotation.quotationNumber
				}
				).ToList();


			//// 查询ERP接口 & 缓存 , 获取库存
			////List<KeyValue<string, string>> products = new List<KeyValue<string, string>>();
			////KeyValue<string, string> product = new KeyValue<string, string>();
			////product.Key = analyzedItem.PrdCode;
			////var supplyorg = _productSmallClassService.GetSupply(inputItem.SupplyOrgId);
			////product.Value = supplyorg?.SupplyOrgNumber;
			////products.Add(product);

			////HelpCommon help = new HelpCommon();
			////var stockresult = help.GetErpStockDataBySupplyOrg(products);
			//List<string> stockResultList = [];



			//         // 查到供应商安全库存,更新库存

			//         SafetyStockQueryReq safetyStockQuery = new SafetyStockQueryReq();
			//         safetyStockQuery.ProductItemList = new List<SafetyStockQuerySimpleDto>();
			//         foreach (var it in SupplierPriceList)
			//         {
			//             safetyStockQuery.ProductItemList.Add(new SafetyStockQuerySimpleDto
			//             {
			//                 ProductModel = it.ProductCode,
			//                 SupplierCode = it.SupplierCode
			//             });
			//         }

			//         List<SafetyStockQuerySimpleDto> safeStockList = _scmService.SupplierSafetyStockQuery(safetyStockQuery);
			//         if (safeStockList.Count == 0)
			//         {
			//             result.Code = ResponseCode.Success;
			//             result.Message = "成功处理报价单供应商匹配和安全库存业务(安全库存Count==0 退出)";
			//             return result;
			//         }

			//         int configStockRadio = 100; //库存比例(小于此比例可更新货期) 单位(%) 现值 100

			//         var tempString = _mymoooContext.RedisCache.HashGetMainValue<SystemProfile>("StockRadio");
			//         if (!string.IsNullOrEmpty(tempString))
			//         {
			//             configStockRadio = Convert.ToInt32(tempString);
			//         }

			//         if (resolvedOrderItems != null)
			//         {
			//             List<ResolvedOrderItem> updateItems = [];
			//             foreach (var item in resolvedOrderItems)
			//             {
			//                 if (item.DeliverySource == DeliverySource.Stock) { continue; }   //如果 有ERP 库存交期,不予处理
			//                 var productCodeStock = safeStockList.Where(r => r.ProductModel == item.ProductCode).FirstOrDefault();
			//                 if (productCodeStock == null) { continue; }  // 没有供应商库存,也不处理
			//                 if (item.Quantity <= productCodeStock.StockQty * configStockRadio / 100) // 需求数量在安全库存之下 执行处理
			//                 {
			//                     var currentItem = item;
			//                     currentItem.DispatchDays = productCodeStock.StockDays;
			//                     currentItem.DeliverySource = DeliverySource.Suplier;
			//                     updateItems.Add(currentItem);
			//                 }
			//                 //目的是更新分解单 明细的2个字段.和报价单明细的 几个字段
			//                 if (updateItems.Count > 0)
			//                 {
			//                     _mymoooContext.SqlSugar.Updateable(updateItems).UpdateColumns(it => new { it.DispatchDays, it.DeliverySource }).ExecuteCommand();

			//                     // 更新报价单明细,字段报价部分字段值.
			//                     var inqDetailIdList = updateItems.Select(r => r.InquiryItemId).Distinct().ToList();
			//                     var quoList = _mymoooContext.SqlSugar.Queryable<InquiryQuotationOrderDetail>().Where(r => inqDetailIdList.Contains(r.InquiryDetailId)).ToList();
			//                     foreach (var it in quoList)
			//                     {
			//                         var inq = updateItems.Where(r => r.InquiryItemId == it.InquiryDetailId).First();
			//                         if (inq == null) { continue; }
			//                         it.AutoDeliveryDays = inq.DispatchDays;
			//                         it.AutoDeliverySource = (int)inq.DeliverySource;
			//                         it.DeliveDays = (decimal)inq.DispatchDays;
			//                         it.DeliverySource = (int)inq.DeliverySource;
			//                     }
			//                     _mymoooContext.SqlSugar.Updateable(quoList).UpdateColumns(it => new { it.AutoDeliveryDays, it.AutoDeliverySource, it.DeliveDays, it.DeliverySource }).ExecuteCommand();
			//                 }
			//             }
			//         }

			result.Code = ResponseCode.Success;
			result.Message = "成功处理报价单ERP库存确定交期业务";
			return Json(result);

		}

		/// <summary>
		/// 检查并设置,报价单中的特价项目, 只处理公司客户.
		/// 逐项检查,该单中的项目是否为第一次订购,如果是, 则标志位设置为 true
		/// </summary>
		/// <param name="Quotation"></param>
		/// <returns></returns>
		public IActionResult CheckAndSetQuotationSpecialPriceItems([FromBody] QuotationMqReq Quotation)
		{
			ResponseMessage<QuotationMqReq> result = new();
			result.Data = Quotation;
			if (Quotation == null)
			{
				result.Code = ResponseCode.Exception;
				result.Message = "入参错误";
				return Json(result);
			}

			// 这个SpecialPrice, 这个是主表的是否特价单.
			// 13个非标小类铝型材；铝型材框架；功能组件；CNC产线；原材料贸易；工业原材料    
			long[] smallClassIdList = [];
			string? tempString = _mymoooContext.RedisCache.HashGet(new SystemProfile() { Key = "notSpecialPriceSmallClassId" }, p => p.Value);
			if (string.IsNullOrEmpty(tempString))
			{
				tempString = "89";  //容错代码
			}
			smallClassIdList = tempString.Split(',').Select(s => long.Parse(s)).ToArray();

			string Sql = @" Select FID_DETAIL_ID as InquiryItemId,FID_PRD_CODE as ProductCode,SpecialPrice as IsSpecialPrice,CompanyId  From  F_CUST_INQUIRY_DETAIL cid
                 inner Join F_CUST_INQUIRY_MSTR as im on im.FIM_INQ_ID=cid.FID_INQ_MSTR_ID 
				 inner Join F_CUST_QUOTATION_DETAIL as qd on cid.FID_DETAIL_ID = qd.FQD_INQ_DETAIL_ID 
                 where (qd.SmallClassId is null OR  qd.SmallClassId not in (@noSpecialPriceSmallClassId))  And im.FIM_INQ_CODE=@qutationNumber ";
			var PrdList = _mymoooContext.SqlSugar.Ado.SqlQuery<SpeciaPricePrd>(Sql,
				new
				{
					qutationNumber = Quotation.quotationNumber,
					noSpecialPriceSmallClassId = smallClassIdList
				}
				).ToList();

			if (PrdList.Count == 0)
			{
				result.Code = ResponseCode.Success;
				result.Message = "没有需要特价处理的项目,无需处理";
				return Json(result);
			}

			if (PrdList[0].CompanyId == null)
			{
				result.Code = ResponseCode.Success;
				result.Message = "非公司客户,无需处理";
				return Json(result);
			}


			if (PrdList[0].CompanyId == null)
			{
				result.Code = ResponseCode.Success;
				result.Message = "非公司客户,无需处理";
				return Json(result);
			}


			// 根据公司编码.型号编码, 查询历史报价单.且报价单状态为已审核..
			Sql = @"Select id.FID_PRD_CODE From  F_CUST_QUOTATION_DETAIL as qd 
                           inner Join F_CUST_INQUIRY_DETAIL as id on id.FID_DETAIL_ID = qd.FQD_INQ_DETAIL_ID 
                           inner Join F_CUST_INQUIRY_MSTR as im on im.FIM_INQ_ID=id.FID_INQ_MSTR_ID 						   
                            where im.CompanyId = @compnayId
							And im.FIM_INQ_CODE != @qutationNumber
							AND qd.AuditStatus = 2 And id.FID_PRD_CODE  in (@prdCodeList)";

			var ExistPrdCodeList = _mymoooContext.SqlSugar.Ado.SqlQuery<string>(Sql,
				new
				{
					prdCodeList = PrdList.Select(r => r.ProductCode).Distinct().ToList(),
					qutationNumber = Quotation.quotationNumber,
					compnayId = PrdList[0].CompanyId
				}
				).Distinct().ToList();

			// 默认所有进入特价. 然后把 已审核的排除掉.
			List<long> InquiryItemIdList = [];
			for (int i = 0; i < PrdList.Count; i++)
			{
				if (ExistPrdCodeList.Where(r => r == PrdList[i].ProductCode).Any())
				{
					continue;
				}
				InquiryItemIdList.Add(PrdList[i].InquiryItemId);
			}

			//更新数据表.
			List<InquiryOrderDetail> IODs = _mymoooContext.SqlSugar.Queryable<InquiryOrderDetail>().Where(r => InquiryItemIdList.Contains(r.InquiryDetailId)).ToList();
			for (int i = 0; i < IODs.Count; i++)
			{
				IODs[i].IsSpecial = true;
			}
			_mymoooContext.SqlSugar.Updateable(IODs).UpdateColumns(it => new { it.IsSpecial }).ExecuteCommand();

			result.Code = ResponseCode.Success;
			result.Message = "项目特价业务成功处理完毕";
			return Json(result);
		}

		/// <summary>
		/// 价格超3W提醒
		/// </summary>
		/// <param name="Quotation"></param>
		/// <returns></returns>
		public IActionResult SendQuotationNoticeWeChatText([FromBody] QuotationMqReq Quotation)
		{
			ResponseMessage<dynamic> result = new();
			result.Data = Quotation;
			string Sql = @"Select SUM(qd.SubtotalWithTax) as TotalPrice From  F_CUST_QUOTATION_DETAIL as qd 
                           inner Join F_CUST_INQUIRY_DETAIL as id on id.FID_DETAIL_ID = qd.FQD_INQ_DETAIL_ID 
                           inner Join F_CUST_INQUIRY_MSTR as im on im.FIM_INQ_ID=id.FID_INQ_MSTR_ID 
                            where im.FIM_INQ_CODE=@qutationNumber ";
			var totalWithTax = _mymoooContext.SqlSugar.Ado.SqlQuery<decimal>(Sql,
				new
				{
					qutationNumber = Quotation.quotationNumber
				}
				).FirstOrDefault();

			if (totalWithTax < 30000)
			{
				result.Code = ResponseCode.Success;
				result.Message = "该报价单总金额不够3万,无需通知";
				return Json(result);
			}
			totalWithTax = Math.Round(totalWithTax, 2);
			// 获取销售的企业微信, 制单员企业微信和名字, 和采购公司名
			Sql = @"Select fum1.WeChatCode , fum1.FUM_USER_NAME ,fum2.WeChatCode,fum2.FUM_USER_NAME,fcim.FPurchaserCompany From  F_CUST_INQUIRY_MSTR as fcim
					Left Join F_USER_MSTR as  fum1 on fcim.SalesmanId = fum1.FUM_USER_ID
					Left Join F_USER_MSTR as  fum2 on fcim.InputterId = fum2.FUM_USER_ID
					 Where FIM_INQ_CODE = @qutationNumber ";

			var userList = _mymoooContext.SqlSugar.Ado.SqlQuery<string[]>(Sql,
				new
				{
					qutationNumber = Quotation.quotationNumber
				}
				).FirstOrDefault();
			if (userList == null)
			{
				result.Code = ResponseCode.Success;
				result.Message = "订单资料缺失,不予处理";
				return Json(result);
			}
			// 得到上级上级可能多个.
			var userLeader = _workbenchService.GetHigherUps(userList[0]);
			var userAssistantT = _workbenchService.GetUserAssistantByWxCode(userList[0]);


			List<string> toUsers = new List<string>();
			List<string> toUserNames = new List<string>();
			for (int i = 0; i < userLeader.Count; i++)
			{
				toUsers.Add(userLeader[i].code);
				toUserNames.Add(userLeader[i].name);
			}
			var userAssistant = userAssistantT.Result;
            for (int i = 0; i < userAssistant.Count; i++)
            {
                if (!userAssistant[i].isDelete)
                {
                    toUsers.Add(userAssistant[i].assistantCode);
                    toUserNames.Add(userAssistant[i].assistantName);
                }
            }


            //单所有者(业务)
            toUsers.Add(userList[0]);
			toUserNames.Add(userList[1]);

			string inputerName = string.Empty;
			string PurchaserCompany = "个人客户";
			if (!string.IsNullOrEmpty(userList[4]))
			{
				PurchaserCompany = userList[4];
			}

			// 制单人
			if (!string.IsNullOrEmpty(userList[2]))
			{
				toUsers.Add(userList[2]);
				toUserNames.Add(userList[3]);
				inputerName = userList[3];
			}

			string toUserString = string.Join("|", toUsers.Distinct().ToArray());
			string toUserNameString = string.Join("，", toUserNames.Distinct().ToArray());
			string sNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
			SendTextMessageRequest send = new SendTextMessageRequest()
			{
				ToUser = toUserString,
				Text = new SendTextMessageRequest.TextMessage()
				{
					Content = $"----------报价通知-----------" + "\r\n" +
							  $"客户名称：{PurchaserCompany}\r\n" +
							  $"报价单号：{Quotation.quotationNumber}\r\n" +
							  $"报价日期：{sNow}\r\n" +
							  $"报价金额：{totalWithTax}\r\n" +
							  $"请及时跟进，并做客情汇报。\r\n" +
							  $"制单人：{inputerName}\r\n" +
							  $"通知接收人：{toUserNameString}"
				}
			};
			// 发送企业微信消息
			var sendResult = _applicationServiceClient.SendTextMessage(send);
			if (sendResult.Code == ResponseCode.Success)
			{
				result.Message = "发送成功.报价超30000元企业微信发送通知给相关业务";
			}
			else
			{
				result.Code = ResponseCode.Exception;
				result.ErrorMessage = "报价超30000元,企业微信发送给相关业务的通知发送失败";
			}
			return Json(result);
		}

		/// <summary>
		/// 通过订单号获取销售组织代码
		/// </summary>
		/// <param name="req"></param>
		/// <returns></returns>
		[HttpPost]
		public ResponseMessage<string> GetSalesOrgCodeByQuoOrderNumber([FromBody] QuotationMqReq req)
		{
			ResponseMessage<string> result = new()
			{
				Data = _quotationService.GetSalesOrgByQuoOrderNumber(req.quotationNumber),
				Code = ResponseCode.Success
			};
			return result;

		}



		/// <summary>
		/// 通过mq销售订单消息回调修改最近历史价
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public IActionResult UpdateCache([FromBody] QuotationMqReq request)
		{
			if (request == null)
			{
				return Json(new ResponseMessage<string>() { Code = ResponseCode.Success });
			}
			_quotationCacheService.UpdateCache(request.quotationNumber);
			return Json(new ResponseMessage<string>() { Code = ResponseCode.Success });
		}

		/// <summary>
		/// 初始化缓存
		/// </summary>
		/// <returns></returns>
		public IActionResult ReloadCache()
		{
			//var request = new SalesHistoryPrice()
			//{
			//	ProductNumber="test-01",
			//	 PriceSource=PriceSource.history,
			//	  CompanyCode="001"
			//};

			//         _mymoooContext.RedisCache.HashSet(request);

			//         request = new SalesHistoryPrice()
			//         {
			//             ProductNumber = "test-01",
			//             PriceSource = PriceSource.fhistory
			//         };

			//_mymoooContext.RedisCache.HashSet(request);
			_quotationCacheService.ReloadCache();
            _salesOrderService.ReloadCache();
            return Json(new ResponseMessage<string>() { Code = ResponseCode.Success });
        }
    }
}