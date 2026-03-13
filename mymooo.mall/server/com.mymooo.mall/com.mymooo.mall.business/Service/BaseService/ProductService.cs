using com.mymooo.mall.business.Service.OldPlatformAdmin;
using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Model.Cache;
using com.mymooo.mall.core.Model.OldPlatformAdmin.Selection;
using com.mymooo.mall.core.Model.Product;
using com.mymooo.mall.core.Model.Quotation;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation;
using com.mymooo.mall.wcf.InquiryServices;
using mymooo.core;
using mymooo.core.Attributes;
using mymooo.core.Utils.JsonConverter;
using System.Collections.Concurrent;

namespace com.mymooo.mall.business.Service.BaseService
{
	[AutoInject(InJectType.Scope)]
	public class ProductService(MallContext mymoooContext, InquiryServiceClient inquiryServiceClient, OldWebApiService webApiService, CrmService crmService, GatewayService gatewayService, ScmService scmService)
	{
		private readonly MallContext _mymoooContext = mymoooContext;
		private readonly InquiryServiceClient _inquiryService = inquiryServiceClient;
		private readonly OldWebApiService _webApiService = webApiService;
		private readonly CrmService _crmService = crmService;
		private readonly GatewayService _gatewayService = gatewayService;
		private readonly ScmService _scmService = scmService;

		public void ReloadAllCache()
		{
			var timeStamp = _mymoooContext.SqlSugar.Ado.SqlQuery<byte[]>("select @@DBTS").First();
			var startTimeStamp = _mymoooContext.RedisCache.GetTimestamp<Product>();
			var filter = " [RowVersion] <= @EndTimeStamp";
			if (startTimeStamp != null)
			{
				filter += " and [RowVersion] > @StartTimeStamp ";
			}
			int pageIndex = 1;
			var products = _mymoooContext.SqlSugar.Queryable<Product>().Includes(p => p.ProductClass).Includes(p => p.ProductSmallClass,e=> e.ProductEngineer).Includes(p => p.ProductTypes).Where(filter, new { EndTimeStamp = timeStamp, StartTimeStamp = startTimeStamp }).OrderBy(p => p.ProductId).ToOffsetPage(pageIndex, 100);
			while (products.Count > 0)
			{
				foreach (var product in products)
				{
					_mymoooContext.RedisCache.HashSet(product);
				}
				products = _mymoooContext.SqlSugar.Queryable<Product>().Includes(p => p.ProductClass).Includes(p => p.ProductSmallClass).Includes(p => p.ProductTypes).Where(filter, new { EndTimeStamp = timeStamp, StartTimeStamp = startTimeStamp }).OrderBy(p => p.ProductId).ToOffsetPage(++pageIndex, 100);
			}
			_mymoooContext.RedisCache.SetTimestamp<Product>(timeStamp);
		}

		public async Task<List<ProductNumberQtyPriceResponse>> Test(int pageNumber = 2, string productNumber = "", string? companyCode = null, int qty = 0)
		{
			List<ProductNumberQtyPriceRequest> products = [];
			if (string.IsNullOrWhiteSpace(productNumber))
			{
				var smalls = _mymoooContext.SqlSugar.Queryable<ProductModelSmallClassMapping>().ToOffsetPage(pageNumber, 1000);
				smalls.ForEach(p => products.Add(new ProductNumberQtyPriceRequest() { ProductNumber = p.Model, Qty = qty }));
			}
			else
			{
				products.Add(new ProductNumberQtyPriceRequest() { ProductNumber = productNumber, Qty = qty });
			}

			var result = await GetProductNumberPrice(companyCode ?? "E10300", products);
			return result;
		}

		public ResponseMessage<ShortNumberselectionResponse> GetParameterValue(long productId, string number, List<ShortNumberselectionRequest> supplierProducts)
		{
			ResponseMessage<ShortNumberselectionResponse> response = new() { Data = new ShortNumberselectionResponse() { Number = number } };

			var parameterValues = _mymoooContext.RedisCache.HashGet(response.Data, p => p.ProductSelectParameterValues);
			if (parameterValues == null)
			{
				response.Data = _webApiService.GetProductParameterValue(productId, number, supplierProducts).Result;
				if (response.Data != null)
				{
					_mymoooContext.RedisCache.HashSet(response.Data);
					response.Data.ShortNumbers.ForEach(p => _mymoooContext.RedisCache.HashSet(p));
				}
			}
			else
			{
				var cacheNumber = _mymoooContext.ProductNumberCache.QureyNumberKeys([number]).Result.FirstOrDefault();
				if (cacheNumber != null)
				{
					response.Data.ShortNumber = cacheNumber.ShortNumber;
					response.Data.Number = cacheNumber.ProductNumber;
					response.Data.ProductId = cacheNumber.ProductId;
				}
				response.Data.ProductSelectParameterValues = parameterValues;
				List<ShortNumberselectionRequest> notChaches = [];
				foreach (var supplierProduct in supplierProducts)
				{
					var shortNumber = _mymoooContext.RedisCache.HashGet(new ShortNumberselectionResponse.SpecialShortNumber() { Number = number, Code = supplierProduct.Code });
					if (shortNumber == null)
					{
						notChaches.Add(supplierProduct);
					}
					else
					{
						response.Data.ShortNumbers.Add(shortNumber);
					}
				}

				if (notChaches.Count > 0)
				{
					var notCacheValues = _webApiService.GetProductParameterValue(productId, number, supplierProducts).Result;
					if (notCacheValues != null)
					{
						foreach (var shortNumber in notCacheValues.ShortNumbers)
						{
							_mymoooContext.RedisCache.HashSet(shortNumber);
							response.Data.ShortNumbers.Add(shortNumber);
						}
					}
				}
			}

			return response;
		}

		public async void ReloadExecuteCache()
		{
			await ReloadCache(await _mymoooContext.ProductNumberCache.GetAll());
			//await ReloadCache(_mymoooContext.ProductNumberCache.QureyNumberKeys(["QTMA-S5M150-25-E-d10"]));
		}

		public async Task<ResponseMessage<dynamic>> ReloadCache(List<ProductNumberIndex> request)
		{
			ResponseMessage<dynamic> response = new();
			var priceResponse = await _crmService.GetFullPriceList(request.Select(p => p.MymoooProductNumber).ToList());
			foreach (var number in request)
			{
				ProductSelectionPriceIndex? priceIndex = null;
				if (number.ProductId > 0)
				{
					priceIndex = await SelectionCache(number);
				}

				if (priceResponse.TryGetValue(number.MymoooProductNumber, out List<ProductSelectionPriceIndex>? productPrices))
				{
					foreach (var price in productPrices)
					{
						price.CompanyCode = price.CompanyCode.Replace("-", "").Trim();
						//price.TargetPriceSource = price.PriceSource;
						price.DeliverySource = price.PriceSource == PriceSource.Customer ? DeliverySource.Customer : DeliverySource.Common;
						_mymoooContext.RedisCache.HashSet(price);
						//price.TargetPriceSource = price.PriceSource == PriceSource.Customer ? PriceSource.currentCustomer : PriceSource.currentCommon;
						//price.PriceSource = price.PriceSource == PriceSource.Customer ? PriceSource.cacheCustomer : PriceSource.cacheCommon;
						//price.DeliverySource = price.PriceSource == PriceSource.cacheCustomer ? DeliverySource.cacheCustomer : DeliverySource.cacheCommon;
						//_mymoooContext.RedisCache.HashSet(price);
					}
				}
				//if (priceIndex != null)
				//{
				//	priceIndex.TargetPriceSource = PriceSource.currentCommon;
				//	priceIndex.PriceSource = PriceSource.cacheImport;
				//	if (!_mymoooContext.RedisCache.HashExists(priceIndex))
				//	{
				//		_mymoooContext.RedisCache.HashSet(priceIndex);
				//	}
				//	else
				//	{
				//		var cache = _mymoooContext.RedisCache.HashGet(priceIndex);
				//		if (cache?.PriceSource == PriceSource.cacheImport)
				//		{
				//			_mymoooContext.RedisCache.HashSet(priceIndex);
				//		}
				//	}
				//}
				_mymoooContext.ProductNumberCache.Index(number);
				if (number.Id != number.MymoooNumberId)
				{
					number.ProductNumber = number.MymoooProductNumber;
					number.ProductName = number.MymoooProductName;
					_mymoooContext.ProductNumberCache.Index(number);
				}
			}
			response.Code = ResponseCode.Success;
			return response;
		}

		private async Task<ProductSelectionPriceIndex?> SelectionCache(ProductNumberIndex number)
		{
			ProductSelectionPriceIndex? priceIndex = null;
			var parameterResponse = _webApiService.GetProductParameterValue(number.ProductId, number.MymoooProductNumber, []);
			ProductionSelectionResponse.Querys querys = new()
			{
				ProductId = number.ProductId
			};
			var selectionResponse = _webApiService.ProductionSelection(querys, number.MymoooProductNumber);
			//var product = _mymoooContext.RedisCache.HashGet<Product, long>(request.ProductId);
			var parameters = await parameterResponse;
			if (parameters == null || parameters.ProductId == 0)
			{
				return null;
			}
			_mymoooContext.RedisCache.HashSet(parameters);
			parameters?.ShortNumbers.ForEach(p => _mymoooContext.RedisCache.HashSet(p));

			var selectionPrice = await selectionResponse;
			if (selectionPrice != null && selectionPrice.Sales != null && selectionPrice.IsDisplayPrice && selectionPrice.IsMethodComplete)
			{
				var spec = selectionPrice.ProductSpecParamList.First();
				priceIndex = new()
				{
					PorductNumber = number.MymoooProductNumber,
					PriceSource = PriceSource.import,
					SmallId = number.SmallId,
					//TargetPriceSource = PriceSource.import,
					DeliverySource = DeliverySource.import
				};
				var ladderPrices = selectionPrice.Sales.SalesPrices.Where(s => s.ProductTypeId == number.TypeId).ToArray();
				if (ladderPrices.Length == 0)
				{
					priceIndex.LadderPrices.Add(new ProductSelectionLadderPrice()
					{
						Price = selectionPrice.UnitPriceWithTax,
						DeliveryDay = selectionPrice.DispatchDays,
						NumberLimit = 99999,
						QuantityDiscount = 100,
						SalesPrice = selectionPrice.UnitPriceWithTax
					});
				}
				else
				{
					foreach (var salesPrcie in selectionPrice.Sales.SalesPrices.Where(s => s.ProductTypeId == number.TypeId))
					{
						priceIndex.LadderPrices.Add(new ProductSelectionLadderPrice()
						{
							Price = spec.OriginalPrcie,
							DeliveryDay = spec.OriginalDelivery + salesPrcie.AppendDays,
							NumberLimit = salesPrcie.Qty,
							QuantityDiscount = 100 - salesPrcie.DiscountRate,
							SalesPrice = Math.Round(spec.OriginalPrcie * (1 - salesPrcie.DiscountRate / 100), 6, MidpointRounding.AwayFromZero)
						});
					}
				}
				_mymoooContext.RedisCache.HashSet(priceIndex);
			}

			return priceIndex;
		}

		public List<ProdctModelSmallClassId> GetSmallClassByPrdCodeList(List<string> prdCodList)
		{
			return _mymoooContext.SqlSugar.Queryable<ProductModelSmallClassMapping>()
				.Where(p => prdCodList.Contains(p.Model))
				.Select(p => new ProdctModelSmallClassId { Model = p.Model, SmallClassId = p.ProductSmallClassId })
				.ToList();
		}

		public async Task<List<ProductNumberQtyPriceResponse>> GetProductNumberPrice(string companyCode, List<ProductNumberQtyPriceRequest> productNumbers)
		{
			//查看是否在缓存中
			var cacheNumbers = await _mymoooContext.ProductNumberCache.QureyNumberKeys(productNumbers.Select(p => p.Id).ToList());
			var replaceTasks = _scmService.QueryModelReplace(companyCode, productNumbers.Select(p => p.ProductNumber).ToList());
			//不在缓存中的,需要跑解析.
			var notCacheNumber = productNumbers.Where(p => !cacheNumbers.Exists(c => c.Id == p.Id)).ToList();
			var analyseProductNumbers = AnalyseProductNumber(companyCode, notCacheNumber, replaceTasks);
			List<ProductNumberQtyPriceResponse> products = await GetCachePorductPrice(companyCode, productNumbers, cacheNumbers, replaceTasks);
			var analyseNumbers = await analyseProductNumbers;
			products.AddRange(analyseNumbers);
			GetProductNumberSmall(products);
			SendRabbitMqAnalyse(analyseNumbers);
			return products;
		}

		private async Task<List<ProductNumberQtyPriceResponse>> GetCachePorductPrice(string companyCode, List<ProductNumberQtyPriceRequest> productNumbers, List<ProductNumberIndex> cacheNumbers, Task<List<ReplaceModelModel>> replaceTasks)
		{
			var replaceNumbers = await replaceTasks;
			List<ProductNumberQtyPriceResponse> products = [];
			foreach (var productNumber in productNumbers)
			{
				var cacheNumber = cacheNumbers.FirstOrDefault(p => p.Id == productNumber.Id);
				if (cacheNumber == null)
				{
					continue;
				}
				var product = new ProductNumberQtyPriceResponse()
				{
					CompanyCode = companyCode,
					ProductId = cacheNumber.ProductId,
					SmallCode = cacheNumber.SmallCode,
					SmallId = cacheNumber.SmallId,
					SmallName = cacheNumber.SmallName,
					ShortNumber = cacheNumber.ShortNumber,
					Qty = productNumber.Qty,
					ProductName = cacheNumber.ProductName,
					ProductNumber = productNumber.ProductNumber,
					MymoooProductNumber = cacheNumber.MymoooProductNumber,
					MymoooProductName = cacheNumber.MymoooProductName,
					TypeId = cacheNumber.TypeId,
					CategoryType = cacheNumber.CategoryType,
					DataSource = cacheNumber.DataSource
				};

				if (product.TypeId == 0)
				{
					var replaceNumber = replaceNumbers.FirstOrDefault(p => p.ReplaceModelId == product.Id);
					if (replaceNumber != null)
					{
						product.MymoooProductNumber = replaceNumber.AntModel;
						product.MymoooProductName = replaceNumber.AntModel;
						product.Memo = replaceNumber.DifferenceRemark ?? string.Empty;
						product.DataSource = replaceNumber.DataSource;
						product.ProductId = replaceNumber.ProductId;
					}
				}
				products.Add(product);
				if (cacheNumber.ProductId > 0)
				{
					var cahceProduct = _mymoooContext.RedisCache.HashGet(new Product() { ProductId = cacheNumber.ProductId });
					if (cahceProduct != null)
					{
						product.IsRelease = cahceProduct.IsRelease;
						product.CatalogUrl = cahceProduct.CatalogUrl;
						product.CategoryId = cahceProduct.ClassId;
						product.SmallId = cahceProduct.SmallId;
					}
				}
			}
			var priceNumbers = await _crmService.GetFullCompanyPriceList(companyCode, products.Select(p => p.MymoooProductNumber).ToList());
			AnalysePorductNumberPirce(companyCode, products, priceNumbers);
			return products;
		}

		private void GetProductNumberSmall(List<ProductNumberQtyPriceResponse> products)
		{
			ConcurrentDictionary<long, ProductSmallClass?> smalls = [];
			foreach (var detail in products)
			{
				if (detail.SmallId > 0)
				{
					var small = smalls.GetOrAdd(detail.SmallId, _mymoooContext.RedisCache.HashGet(new ProductSmallClass() { Id = detail.SmallId }));
					//产品小类已发布 已启用 并且是叶子节点
					if (small != null && small.IsPublish && small.IsEnable && small.IsLeaf)
					{
						detail.SmallCode = small.Code;
						detail.SmallName = small.Name;
						detail.ProductEngineerId = small.ProductEngineerId;
						detail.ProductEngineerWeChatCode = small.ProductEngineer?.WeChatCode;
						detail.ProductEngineerName = small.ProductEngineer?.UserName;
						detail.ProductManagerId = small.ProductManagerId;
						detail.ProductManagerWeChatCode = small.ProductManager?.WeChatCode;
						detail.ProductManagerName = small.ProductManager?.UserName;
						detail.BusinessDivisionId = small.BusinessDivisionId;
						detail.BusinessDivisionNumber = small.BusinessDivisionNumber;
						detail.BusinessDivisionName = small.BusinessDivisionName;
						var supplyOrg = small.SupplyOrgs.FirstOrDefault(small => small.IsDefault);
						if (supplyOrg != null)
						{
							detail.SupplyOrgId = supplyOrg.SupplyOrgId;
							detail.SupplyOrgNumber = supplyOrg.SupplyOrgNumber;
							detail.SupplyOrgName = supplyOrg.SupplyOrgName;
						}
						if (string.IsNullOrWhiteSpace(detail.ProductName))
						{
							detail.ProductName = small.Name;
						}
						if (string.IsNullOrWhiteSpace(detail.MymoooProductName))
						{
							detail.MymoooProductName = small.Name;
						}
					}
					else
					{
						detail.SmallId = 0;
					}
				}
			}
		}

		private async Task<List<ProductNumberQtyPriceResponse>> AnalyseProductNumber(string companyCode, List<ProductNumberQtyPriceRequest> productNumbers, Task<List<ReplaceModelModel>> replaceTasks)
		{
			List<ProductNumberQtyPriceResponse> productNumberQtyPrices = [];
			if (productNumbers.Count == 0)
			{
				return productNumberQtyPrices;
			}

			var pageCount = productNumbers.Count / 20 + (productNumbers.Count % 20 > 0 ? 1 : 0);
			var tasks = new List<Task<List<ProductNumberQtyPriceResponse>>>();
			for (var i = 0; i < pageCount; i++)
			{
				var quotationItems = productNumbers.Skip(i * 20).Take(20).ToList();
				tasks.Add(AnalyseBatchProductNumber(companyCode, quotationItems, replaceTasks));
			}
			foreach (var item in tasks)
			{
				productNumberQtyPrices.AddRange(await item);
			}

			var noSmalls = productNumberQtyPrices.Where(p => p.SmallId == 0).ToList();
			if (noSmalls.Count > 0)
			{
				var noSmallNumbers = noSmalls.Select(p => p.MymoooProductNumber).ToList();
				var smalls = _mymoooContext.SqlSugar.Queryable<ProductModelSmallClassMapping>().Where(p => noSmallNumbers.Contains(p.Model)).ToList();
				foreach (var small in smalls)
				{
					var noSmall = noSmalls.FirstOrDefault(p => p.MymoooProductNumber.Equals(small.Model, StringComparison.OrdinalIgnoreCase));
					if (noSmall != null)
					{
						noSmall.SmallId = small.ProductSmallClassId;
					}
				}
			}
			return productNumberQtyPrices;
		}

		private async Task<List<ProductNumberQtyPriceResponse>> AnalyseBatchProductNumber(string companyCode, List<ProductNumberQtyPriceRequest> productNumbers, Task<List<ReplaceModelModel>> replaceTasks)
		{
			var validateRequest = new InquiryInfo
			{
				IsPass = true,
				FacList =
				[
					new FntFacInfo()
				],
				InqDetailList = productNumbers.Select(p => new InqDetailInfo
				{
					PrdCode = p.ProductNumber,
					BrandId = 10499,
					BrandCode = string.Empty,
					Num = p.Qty
				}).ToList()
			};
			var analyseResult = await _inquiryService.ValidateInquiryAsync(validateRequest);
			var replaceNumbers = await replaceTasks;

			int rowIndex = 0;
			List<ProductNumberQtyPriceResponse> responseNumbers = [];
			foreach (var productNumber in productNumbers)
			{
				responseNumbers.Add(CreateProductNumberQtyPrice(companyCode, replaceNumbers, productNumber, analyseResult.InqDetailList[rowIndex++]));
			}
			var priceNumbers = await _crmService.GetFullCompanyPriceList(companyCode, responseNumbers.Select(p => p.MymoooProductNumber).ToList());
			AnalysePorductNumberPirce(companyCode, responseNumbers, priceNumbers);

			return responseNumbers;
		}

#pragma warning disable CA1822 // 将成员标记为 static
		private ProductNumberQtyPriceResponse CreateProductNumberQtyPrice(string companyCode, List<ReplaceModelModel> replaceNumbers, ProductNumberQtyPriceRequest productNumber, InqDetailInfo detail)
#pragma warning restore CA1822 // 将成员标记为 static
		{
			var price = new ProductNumberQtyPriceResponse()
			{
				CompanyCode = companyCode,
				Qty = productNumber.Qty,
				ProductNumber = productNumber.ProductNumber
			};
			if (detail.IsAudit || detail.Published)
			{
				price.ProductId = detail.PrdId;
				price.SmallId = detail.SmallId;
				price.TypeId = detail.TypeId;
				price.MymoooProductNumber = detail.PrdCode;
				price.ShortNumber = detail.ShortNumber;
				price.ProductName = detail.PrdName;
				price.MymoooProductName = detail.PrdName;
				price.Memo = detail.Memo;
				price.CategoryId = detail.CategoryId;
				price.CatalogUrl = detail.ImageUrl;
				price.CategoryType = detail.CategoryType;
				price.IsRelease = detail.Published;
				if (detail.UnitTaxPrice > 0)
				{
					price.PriceSource = PriceSource.import;
					price.DeliverySource = DeliverySource.import;
					price.SalesPrice = detail.UnitTaxPrice;
					price.OrgPrice = detail.OrgPrice;
					price.QtyDiscount = detail.QtyDiscount;
					price.DeliveryDays = (int)detail.DeliveryDays;
				}
			}
			else
			{
				price.MymoooProductNumber = productNumber.ProductNumber;
			}
			if (detail.PrdId == 0)
			{
				var replaceNumber = replaceNumbers.FirstOrDefault(p => p.ReplaceModelId == productNumber.Id);
				if (replaceNumber != null)
				{
					price.MymoooProductNumber = replaceNumber.AntModel;
					price.MymoooProductName = replaceNumber.AntModel;
					price.Memo = replaceNumber.DifferenceRemark ?? string.Empty;
					price.DataSource = replaceNumber.DataSource;
					price.ProductId = replaceNumber.ProductId;
				}
			}
			else
			{
				price.DataSource = 2;
			}

			return price;
		}

		private void AnalysePorductNumberPirce(string companyCode, List<ProductNumberQtyPriceResponse> productNumbers, ConcurrentDictionary<string, ProductSelectionPriceIndex> priceNumbers)
		{
			foreach (var productNumber in productNumbers)
			{
				var exists = priceNumbers.TryGetValue(productNumber.MymoooProductNumber, out ProductSelectionPriceIndex? priceLists);
				if (exists && priceLists != null && priceLists.PriceSource == PriceSource.Customer)
				{
					TransformPriceList(productNumber, priceLists);
				}
				else
				{
					//获取历史价格
					var historyPrice = _mymoooContext.RedisCache.HashGet(new SalesHistoryPrice() { ProductNumber = productNumber.MymoooProductNumberId, CompanyCode = companyCode });
					if (historyPrice != null)
					{
						if (historyPrice.AuditTime.AddYears(1) > DateTime.Now)
						{
							TransformHistoryList(productNumber, historyPrice);
						}
						else
						{
							_mymoooContext.RedisCache.HashDelete(new SalesHistoryPrice() { ProductNumber = productNumber.MymoooProductNumberId, CompanyCode = companyCode });
						}
					}

					if (exists && priceLists != null && priceLists.PriceSource == PriceSource.common)
					{
						TransformPriceList(productNumber, priceLists);
					}

					var import = _mymoooContext.RedisCache.HashGet(new ProductSelectionPriceIndex() { Id = productNumber.MymoooProductNumberId, PriceSource = PriceSource.import });
					if (import != null)
					{
						TransformPriceList(productNumber, import);
					}
				}

			}
		}

#pragma warning disable CA1822 // 将成员标记为 static
		private void TransformHistoryList(ProductNumberQtyPriceResponse price, SalesHistoryPrice historyPrice)
#pragma warning restore CA1822 // 将成员标记为 static
		{
			price.PriceSource = PriceSource.history;
			if (price.SmallId == 0)
			{
				price.SmallId = historyPrice.SmallId;
			}
			price.OrgPrice = historyPrice.OriginalPrice;
			price.QtyDiscount = historyPrice.QtyDiscount;
			price.LevelDiscount = historyPrice.LevelDiscount;
			price.SalesPrice = historyPrice.TaxPrice;
			if (historyPrice.DeliverySource == DeliverySource.Common || historyPrice.DeliverySource == DeliverySource.cacheCommon || historyPrice.DeliverySource == DeliverySource.import)
			{
				price.DeliverySource = historyPrice.DeliverySource;
				price.DeliveryDays = historyPrice.DeliveDays;
			}
			if (!historyPrice.IsFa)
			{
				price.DeliverySource = historyPrice.DeliverySource;
				price.DeliveryDays = historyPrice.DeliveDays;
			}
		}

#pragma warning disable CA1822 // 将成员标记为 static
		private void TransformPriceList(ProductNumberQtyPriceResponse price, ProductSelectionPriceIndex pricelist)
#pragma warning restore CA1822 // 将成员标记为 static
		{
			var ladderPrice = pricelist.LadderPrices.OrderBy(p => p.NumberLimit).FirstOrDefault(p => p.NumberLimit >= price.Qty);
			if (price.PriceSource == PriceSource.none)
			{
				price.PriceSource = pricelist.PriceSource;
				if (price.SmallId == 0)
				{
					price.SmallId = pricelist.SmallId;
				}
				if (ladderPrice != null)
				{
					price.OrgPrice = ladderPrice.Price;
					price.QtyDiscount = ladderPrice.QuantityDiscount;
					price.SalesPrice = ladderPrice.SalesPrice;
					price.DeliveryDays = ladderPrice.DeliveryDay;
				}
			}
			if (price.DeliverySource == DeliverySource.none)
			{
				price.DeliverySource = pricelist.PriceSource == PriceSource.Customer ? DeliverySource.Customer : pricelist.PriceSource == PriceSource.common ? DeliverySource.Common : DeliverySource.import;
				if (ladderPrice != null)
				{
					price.DeliveryDays = ladderPrice.DeliveryDay;
				}
			}
		}

		private async void SendRabbitMqAnalyse(List<ProductNumberQtyPriceResponse> prices)
		{
			prices = prices.Where(p => p.SmallId > 0).ToList();
			if (prices.Count > 0)
			{
				await _gatewayService.SendMessage("reload_product_price_", JsonSerializerOptionsUtils.Serialize(prices));
			}
		}

		public async Task ReloadSelectionCache()
		{
			var request = new ProductNumberQueryRequest() { ProductId = 301920 };
			var totalCount = await _mymoooContext.ProductNumberCache.QureyCount(request);
			request.PageSize = 10000;
			var page = totalCount / request.PageSize + (totalCount % request.PageSize > 0 ? 1 : 0);
			for (int i = 0; i < page; i++)
			{
				request.PageNumber = i * request.PageSize;
				var numbers = await _mymoooContext.ProductNumberCache.Qurey(request);
				foreach (var number in numbers)
				{
					ProductionSelectionResponse.Querys querys = new()
					{
						ProductId = number.ProductId
					};
					var result = _webApiService.ProductionSelection(querys, number.MymoooProductNumber).Result;
					if (result != null)
					{
						if (result.IsDisplayPrice && result.IsMethodComplete)
						{
							var productReslut = result.ProductSpecParamList[0];
							ProductSelectionIndex index = new()
							{
								Id = number.Id,
								ProductId = number.ProductId,
								Number = productReslut.Number,
								Name = number.ProductName,
								//ShortNumber = productReslut.ShortNumber,
								//ProductQueryParamList = productReslut.Querys.ProductQueryParamList,
								//ProductTypeId = productReslut.Querys.TypeList[0],
								//SmallId = number.SmallId,
								//SmallCode = number.SmallCode,
								//SmallName = number.SmallName
							};
							var productTypeId = productReslut.Querys.TypeList.First();
							index.Patameters["Type"] = result.DisplayParamData?.ProductTypeList.First(p => p.TypeID == productTypeId).TypeCode ?? "";
							foreach (var item in productReslut.Querys.ProductQueryParamList)
							{
								index.Patameters[item.ParamName] = item.SelectData;
							}
							_mymoooContext.ProductSelectionCache.Index(index);
						}
					}
				}
			}
		}
	}
}
