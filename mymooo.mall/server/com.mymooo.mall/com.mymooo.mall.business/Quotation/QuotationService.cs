using com.mymooo.mall.business.Service.BaseService;
using com.mymooo.mall.business.Service.ProductServices;
using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Model.Cache;
using com.mymooo.mall.core.Model.Price.CalcPriceList;
using com.mymooo.mall.core.Model.Product;
using com.mymooo.mall.core.Model.Quotation;
using com.mymooo.mall.core.Model.SalesMan;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.CustomerInformation;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation;
using com.mymooo.mall.core.SqlSugarCore.SalesBusiness;
using com.mymooo.mall.core.SqlSugarCore.SystemManagement;
using com.mymooo.mall.wcf.InquiryServices;
using mymooo.core.Attributes;
using mymooo.core.Utils.JsonConverter;
using mymooo.k3cloud.SDK.Inventory;
using SqlSugar;
using System.Text.RegularExpressions;
using System.Web;

namespace com.mymooo.mall.business.Service
{


    [AutoInject(InJectType.Scope)]
	public partial class QuotationService(MallContext mymoooContext, ProductService productService, ErpService erpService, CrmService crmService, GatewayService gatewayService, 
		InventoryServiceClient inventoryService, ProductModelMappingCacheService productModelMappingCacheService)
	{
		private readonly MallContext _mymoooContext = mymoooContext;
		private readonly ProductService _productService = productService;
		private readonly ErpService _erpService = erpService;
		private readonly CrmService _crmService = crmService;
		private readonly GatewayService _gatewayService = gatewayService;
        private readonly InventoryServiceClient _inventoryService = inventoryService;
		private readonly ProductModelMappingCacheService _productModelMappingCacheService = productModelMappingCacheService;
        // private object qutationNumber;

        // 主要是计算价格和交期
        public List<ConfirmQuotationItemViewModel> PrepareQuotationItemModelV3(
	   List<InputQuotationItemViewModel> inputQuotationItemModels, string companyCode, long? customerId, bool IsNonStandard)
		{

			var viewModelList = new List<ConfirmQuotationItemViewModel>();
			List<ConfirmQuotationItemViewModel> models = new List<ConfirmQuotationItemViewModel>();
			var productsOrg = new List<Tuple<string, string>>();  //蚂蚁型号,供货组织

            //List<InqDetailInfo> analyzedItems = [];
            //         if (IsNonStandard)  // 非标单不走解析
            //{
            //	analyzedItems = CalculateNonStandard(inputQuotationItemModels);
            //         }
            //else
            //{
            //	analyzedItems = CalculateWithPriceListMult(inputQuotationItemModels,  companyCode, customerId);
            //}

			// 都要走解析
            List<InqDetailInfo> analyzedItems = CalculateWithPriceListMult(inputQuotationItemModels, companyCode, customerId);

            // 处理供货组织. 小类勿需处理了. 前面 在统一方法里都处理过了.
            for (int i = 0; i < analyzedItems.Count; i++)
			{
				var supplyOrgNumber = string.Empty;
				var model = new ConfirmQuotationItemViewModel();
				var analyzedItem = analyzedItems[i];

				if (analyzedItem.SmallId > 0)
				{
					model.ProductSmallClass = _mymoooContext.RedisCache.HashGet(new ProductSmallClass() { Id = analyzedItem.SmallId });
				}

				// 优先用系统解析的小类, 如果没有解析到, 就用模板里面填的小类(也可能没有)
				var item2 = inputQuotationItemModels.ToList()[i];
				if (analyzedItem.SmallId <= 0 && item2.ProductSmallClassId != null)
				{
					model.ProductSmallClass = _mymoooContext.RedisCache.HashGet(new ProductSmallClass() { Id = item2.ProductSmallClassId.Value });
					model.IsTempSmallClass = true;
				}

                // 系统产品且有小类,   历史曾经 一小类对应过多供货组织，现在业务需求只一个。
                if (model.ProductSmallClass != null)
				{
					var result = _mymoooContext.RedisCache.HashGet<ProductSmallClass>(new ProductSmallClass() { Id = model.ProductSmallClass.Id});
					if (result != null && result.SupplyOrgs.Count > 0)
					{
						model.SupplyOrgId = result.SupplyOrgs.Where(r => r.IsDefault).First().SupplyOrgId;
						model.SupplyOrgName = result.SupplyOrgs.Where(r => r.IsDefault).First().SupplyOrgName;
						supplyOrgNumber = result.SupplyOrgs.Where(r => r.IsDefault).First().SupplyOrgNumber;

						// 如果有客户-供货组织的映射关系,则用客户的
						var cmso = _mymoooContext.RedisCache.HashGet(new CompanyMapSupplyOrg() { BusinessDivisionNumber = result.BusinessDivisionNumber , CompanyCode = companyCode });
						if (cmso != null)
						{
                            model.SupplyOrgId = cmso.SupplyOrgId;
                            model.SupplyOrgName = cmso.SupplyOrgName;
                            supplyOrgNumber = cmso.SupplyOrgCode;
                        }
                    }
				}
				else
				{
					model.SupplyOrgId = 0;
					model.SupplyOrgName = string.Empty;
				}

				if (model.SupplyOrgId > 0)
				{

					if (!productsOrg.Any(it => it.Item1 == analyzedItem.PrdCode && it.Item2 == supplyOrgNumber))
					{
						var product = new Tuple<string, string>(analyzedItem.PrdCode, supplyOrgNumber);
						analyzedItem.SupplyOrgNumber = supplyOrgNumber;
						productsOrg.Add(product);
					}
				}
                models.Add(model);


                var inputItem = inputQuotationItemModels[i];
				inputItem.Product.Code = analyzedItem.PrdCode; // 用户输入的蚂蚁型号,可能改变(替换),重新赋值一下,下面用到
            }

            // 查询ERP库存,确定ERP库存交期.
            int configStockRadio = 100; //库存比例(小于此比例可更新货期) 单位(%) 现值 100
            var tempString = _mymoooContext.RedisCache.HashGet(new SystemProfile() { Key = "StockRadio" }, p => p.Value);            
            if (!string.IsNullOrEmpty(tempString))
            {
                configStockRadio = Convert.ToInt32(tempString);
            }
			decimal stockDelivery = 1M;   // 有库存设定的交货天数
            tempString = _mymoooContext.RedisCache.HashGet(new SystemProfile() { Key = "StockDelivery" }, p => p.Value);
            if (!string.IsNullOrEmpty(tempString))
            {
                stockDelivery = Convert.ToDecimal(tempString);
            }

            // 根据ERP库存,确定交期
            for (int i = 0; i < analyzedItems.Count; i++)
			{
                var analyzedItem = analyzedItems[i];

				var supplyOrgNumber = analyzedItem.SupplyOrgNumber ?? string.Empty;

                var stockResult = _inventoryService.GetInventoryTotal(analyzedItem.PrdCode, supplyOrgNumber);
                
                if  (stockResult != null)
				{
					// 计算相同型号的总数 ,再算.
					var PrdNumTotal = inputQuotationItemModels.Where(r => r.Product.Code == analyzedItem.PrdCode).Sum(r => r.Quantity);

                    bool isAvailable = stockResult.UsableQty * configStockRadio / 100 >= PrdNumTotal;
					if (isAvailable)
					{
						analyzedItem.PriceCalculateResult.DeliverySource = DeliverySource.Stock;
						analyzedItem.PriceCalculateResult.DeliveryDays = stockDelivery;       // 需求日期,在保存的时候会算.		
						analyzedItem.DeliveryDays = stockDelivery;
                    }
				}
            }


            // 处理返回
            for (int i = 0; i < analyzedItems.Count; i++)
			{
				var model2 = models[i];
				var analyzedItem = analyzedItems[i];
				var inputItem = inputQuotationItemModels[i];
				var qtyDiscount = analyzedItem.QtyDiscount;

				var preferentialUnitPrice = analyzedItem.PriceCalculateResult.PreferentialUnitPrice;
				var levalDiscount = analyzedItem.PriceCalculateResult.DiscountRate;
				var model = new ConfirmQuotationItemViewModel
				{
					IsHistory = analyzedItem.PriceCalculateResult.PriceSource == PriceSource.history,
					OrgPrice = analyzedItem.OrgPrice,
					CategoryType = analyzedItem.CategoryType,
					Product = PrepareProductModel(analyzedItem, inputItem),
					CustomCode = inputItem.CustomProductCode,
					InputUnitPriceWithTax = inputItem.UnitPriceWithTax,
					SystemUnitPriceWithTax = preferentialUnitPrice <= 0 ? (decimal?)null : preferentialUnitPrice,
					//如果从前端传过来的价格是null，而且解析的价格>0，那么选择系统价(包括历史价)为true
					SelectedSystemUnitPriceWithTax = inputItem.UnitPriceWithTax == null && preferentialUnitPrice > 0,
					InputDispatchDays = inputItem.DispatchDays,
					SystemDispatchDays = (int)analyzedItem.DeliveryDays,// 这里逻辑先去掉看看 (analyzedItem.PriceCalculateResult.IsHistory || analyzedItem.PriceCalculateResult.IsCrmPriceList) ? (int)analyzedItem.DeliveryDays : (analyzedItem.ErrorState < 0 || preferentialUnitPrice <= 0 || analyzedItem.DeliveryDays <= 0 ? (int?)null : (int)analyzedItem.DeliveryDays),
																		//如果从前端传过来的发货天数是null，而且解析的价格>0 and 解析天数>=0，那么 选择系统天数为true
					SelectedSystemDispatchDays = inputItem.DispatchDays == null  && analyzedItem.DeliveryDays > 0,   //&& preferentialUnitPrice > 0  
					Quantity = inputItem.Quantity,
					Remark = inputItem.Remark + analyzedItem.Memo,
					InsideRemark = HttpUtility.UrlDecode(inputItem.InsideRemark),
					PurchaseRemark = inputItem.PurchaseRemark,
					QtyDiscount = (qtyDiscount < 0.000001M) ? 100 : qtyDiscount,
					LevelDiscount = (levalDiscount < 0.0000001M) ? 100 : levalDiscount,
					SupplyUnitPriceWithTax = null,  // 供应商单价在这里没有
					ProjectNo = MyRegex().Replace(inputItem.ProjectNo, ""),
					CustItemNo = MyRegex().Replace(inputItem.CustItemNo, ""),
					StockFeatures = MyRegex().Replace(inputItem.StockFeatures, ""),
					PriceSourceName = analyzedItem.PriceCalculateResult.PriceSource == null ? "" : analyzedItem.PriceCalculateResult.PriceSource.GetDescription(),
					PriceSource = analyzedItem.PriceCalculateResult.PriceSource,
					DeliverySourceName = analyzedItem.PriceCalculateResult.DeliverySource == null ? "" : analyzedItem.PriceCalculateResult.DeliverySource.GetDescription(),
					DeliverySource = analyzedItem.PriceCalculateResult.DeliverySource,
					QuoLPrice = analyzedItem.PriceCalculateResult.QuoLPrice,
					DealLPrice = analyzedItem.PriceCalculateResult.DealLPrice,
					AttaFilesName = inputItem.AttaFilesName,
					Storage = inputItem.Storage,
					ShortNumber = analyzedItem.ShortNumber,  // 增加简易型号的传参和存储
					DesirePrice = inputItem.DesirePrice,
					DesireDeliveryDays = inputItem.DesireDeliveryDays,


					// 这里的逻辑是, 如果系统产品解析到了.用系统产品解析的. 没有解析到.用模板的. 小类,供货组织.  前面一个循环已处理了
					//SupplyOrgId = inputItem.SupplyOrgId,
					//SupplyOrgName = inputItem.SupplyOrgName,
					IsTempSmallClass = model2.IsTempSmallClass,
					SupplyOrgId = model2.SupplyOrgId,
					SupplyOrgName = model2.SupplyOrgName,
					ProductSmallClass = model2.ProductSmallClass,

				};

				viewModelList.Add(model);
			}

			return viewModelList;
		}



		/// <summary>
		/// 批量计算价格和折扣
		/// </summary>
		/// <param name="analyzedItems"></param>
		/// <param name="companyCode"> 公司客户的代码</param>
		/// <param name="customerId">个人客户的Id</param>
		public List<InqDetailInfo> CalculateWithPriceListMult(List<InputQuotationItemViewModel> inputQuotationItemModels, string companyCode, long? customerId)
		{
			List<InqDetailInfo> analyzedItems = [];
			var priceResult = new List<CalculateProductDiscountUnitPriceResponse>();
			var priceListCalcRequest = new PriceListCalcRequest();

			Guid? customerCompanyId = null;
			Guid? customerParentCompanyId = null;
			string parentCompanyCode = string.Empty;
			int pricePrecision = 4;// TempConfig.PricePrecision;   价格精度. 缺省精度
			List<string> companyRelationIdList = [];   // 当前公司的关联公司IDs
			List<string> companyRelationCodesList = []; // 当前关联公司的Codes 用于查询历史报价.

			//查询集团公司, 母公司,子公司
			if (!string.IsNullOrEmpty(companyCode))
			{
				// 获取价格精度.
				var company = _mymoooContext.SqlSugar.Queryable<Company>()
					.Where(c => c.Code == companyCode)
					.Select(r => new
					{
						r.DecimalPlacesOfUnitPrice,
						CompanyId = r.Id
					}).First();
				pricePrecision = company.DecimalPlacesOfUnitPrice;
				customerCompanyId = company.CompanyId;


				// 获取 父公司ID.
				var companyParent = _mymoooContext.SqlSugar.Queryable<SubAndParentCompany>()
						.Where(c => c.CompanyId == company.CompanyId && c.IsValid == true)
						.Select(r => new { r.ParentCompanyId, r.UnifiedCreditLine })
						.First();


				companyRelationIdList.Add(company.CompanyId.ToString().ToUpper());

				// 获取父公司代码
				//统一信用额度, 才和父公司有关联. CRM客户价目表可以用集团公司的, 但历史价目表不受此限制.
				if (companyParent != null && companyParent.UnifiedCreditLine)  // 沿用父公司额度
				{
					var parentCompany = _mymoooContext.SqlSugar.Queryable<Company>()
									.Where(c => c.Id == companyParent.ParentCompanyId)
									.Select(r => new { r.Code, r.Id }).First();
					if (parentCompany != null)
					{
						parentCompanyCode = parentCompany.Code;
						customerParentCompanyId = parentCompany.Id;
						companyRelationIdList.Add(parentCompany.Id.ToString().ToUpper());
					}
				}

				// 获取当前公司的关联公司Ids 用于查询最近历史价
				var companyRelationIdTemp = _mymoooContext.SqlSugar.Queryable<SubAndParentCompany>()
					.Where(c => (c.ParentCompanyId == company.CompanyId || c.CompanyId == company.CompanyId) && c.IsValid == true)
					.Select(r => r.CompanyId.ToString().ToUpper())
					.ToList();
				if (companyRelationIdTemp.Count > 0)
				{
					companyRelationIdList = new List<string>(companyRelationIdList.Union(companyRelationIdTemp));
				}

				// 获取当前公司的子公司 代码
				priceListCalcRequest.CompanyCode = companyCode;

				if (companyRelationIdList != null)
				{
					// 查询Codes
					companyRelationCodesList = _mymoooContext.SqlSugar.Queryable<Company>()
										.Where(r => companyRelationIdList.Contains(r.Id.ToString().ToUpper()))
										.Select(r => r.Code).ToList();
				}
			}
			#region 客户型号映射
			var mappingCompanyCode = companyCode;
			if (!string.IsNullOrEmpty(parentCompanyCode))
			{
				mappingCompanyCode = parentCompanyCode;
            }
			if (!string.IsNullOrEmpty(mappingCompanyCode) )
			{
				foreach (var it in inputQuotationItemModels)
				{
					if (!string.IsNullOrEmpty(it.CustomProductCode))
					{
						var result = _mymoooContext.RedisCache.HashGet<ProductModelMappingCacheDto>(new ProductModelMappingCacheDto() { CompanyCode= mappingCompanyCode,CustomerModel=it.CustomProductCode }, "material-match");
						if (result != null&&!string.IsNullOrEmpty(result.ProductModel))
						{
							it.Product.Code = result.ProductModel;
						}
					}
				}
			}


            #endregion

            // 如果有客户价目表,客户价目表优先 CRM通用价目表,和历史价, 历史价优先.

            int configStockDelivery = 1; // 可用库存交期. 缺省值 1
            int configStockRadio = 100; //库存比例(小于此比例可更新货期) 单位(%) 现值 100

			var tempString = _mymoooContext.RedisCache.HashGet(new SystemProfile() { Key = "StockRadio" }, p => p.Value);
			if (!string.IsNullOrEmpty(tempString))
			{
				configStockRadio = Convert.ToInt32(tempString);
			}

			tempString = _mymoooContext.RedisCache.HashGet(new SystemProfile() { Key = "stockDelivery" }, p => p.Value);
			if (!string.IsNullOrEmpty(tempString))
			{
				configStockDelivery = Convert.ToInt32(tempString);
			}

            // 客户价目表,历史价目表,通用价目表逻辑.均封装在一起了.
            var itemListReq = inputQuotationItemModels.Select(e => new ProductNumberQtyPriceRequest
            {
                ProductNumber = e.Product.Code,
                Qty = e.Quantity
            }).ToList();

			foreach (var it in inputQuotationItemModels)  //如果客户型号和蚂蚁型号不同,也添加,一并查询.
			{
				var ci = itemListReq.Where(r => r.ProductNumber == it.CustomProductCode).FirstOrDefault();
				if (ci == null)
				{
					itemListReq.Add(new ProductNumberQtyPriceRequest
					{
						ProductNumber = it.CustomProductCode,
						Qty = it.Quantity
					});
				}
			}

			itemListReq = itemListReq.GroupBy(r => r.ProductNumber)
				.Select(grp => new ProductNumberQtyPriceRequest
				{
					ProductNumber = grp.Key,
					Qty = grp.Sum(req => req.Qty)
				}).ToList();


			List<ProductNumberQtyPriceResponse> priceListResult = [];   // 查询客户节目表,历史价,通用价目表,的结果  个人客户,可以应用通用价目表,不应用历史价.
			if (itemListReq.Count > 0)
			{
				// 非常重要的方法, 调用获取产品解析和替换型号和价格
				priceListResult = _productService.GetProductNumberPrice(string.IsNullOrEmpty(parentCompanyCode) ? companyCode : parentCompanyCode, itemListReq).Result;
			}


            // 处理报价逻辑
            foreach (var item in inputQuotationItemModels)
			{
				InqDetailInfo analyzedItem = new InqDetailInfo();
				analyzedItem.PrdCode = item.Product.Code;
				analyzedItem.PrdName = item.Product.Name;
				var itemPrice = priceListResult.Where(r => r.ProductNumber == item.Product.Code).FirstOrDefault();  // 查询价格结果
				if (itemPrice == null)
				{   //如果蚂蚁型号没查到.用客户自定义型号
					itemPrice = priceListResult.Where(r => r.ProductNumber == item.CustomProductCode).FirstOrDefault();
					if (itemPrice == null)
					{
						analyzedItems.Add(analyzedItem);
						continue;
					}
				}
				analyzedItem.PrdCode = itemPrice.MymoooProductNumber;
				analyzedItem.PrdName = itemPrice.MymoooProductName;

                analyzedItem.PriceCalculateResult = new CalculateProductPriceResult()
                {
                    QtyDiscount = (itemPrice.QtyDiscount < 0.000001M) ? 100 : itemPrice.QtyDiscount,
                    PreferentialUnitPrice = Math.Round(itemPrice.SalesPrice, pricePrecision, MidpointRounding.AwayFromZero), // 优先价=销售价.  SetPricePrecision(itemPrice.SalesPrice, pricePrecision),
                    OriginalUnitPrice = Math.Round(itemPrice.OrgPrice, pricePrecision, MidpointRounding.AwayFromZero),                    
                    DeliveryDays = itemPrice.DeliveryDays,
                    PriceSource = itemPrice.PriceSource,
                };
                if (itemPrice.SmallId != 0)  // 如果解析到小类, 用解析的小类, 没有解析的小类,用价目表(历史价)小类
                {
                    analyzedItem.SmallId = itemPrice.SmallId;
                    analyzedItem.SupplyOrgNumber = itemPrice.SupplyOrgNumber;  // 其实后面有另处理.

				}
				analyzedItem.ImageUrl = itemPrice.CatalogUrl;
				analyzedItem.Published = itemPrice.IsRelease;
				analyzedItem.TypeId = itemPrice.TypeId;
                analyzedItem.PrdId = itemPrice.ProductId;
                analyzedItem.OrgPrice = itemPrice.OrgPrice;
				analyzedItem.CategoryType = itemPrice.CategoryType;
				analyzedItem.CategoryId = itemPrice.CategoryId;
				analyzedItem.Memo = string.IsNullOrEmpty(itemPrice.Memo) ? string.Empty : "系统备注:" + itemPrice.Memo;

				analyzedItem.PriceCalculateResult.DeliverySource = itemPrice.DeliverySource;  //货期来源
				analyzedItem.DeliveryDays = itemPrice.DeliveryDays;  //货期

				analyzedItem.ShortNumber = itemPrice.ShortNumber;
				analyzedItem.QtyDiscount = (itemPrice.QtyDiscount < 0.000001M) ? 100 : itemPrice.QtyDiscount;
				if (itemPrice.ProductNumber != itemPrice.MymoooProductNumber)  // 如果有型号替换.
				{
					analyzedItem.PrdCode = itemPrice.MymoooProductNumber;
					analyzedItem.PrdName = itemPrice.MymoooProductName;
				}
				analyzedItems.Add(analyzedItem);
			}
			// 处理最低价
			List<HistoryLowPrice> OnYearLowPrice = [];
			if (!string.IsNullOrEmpty(companyCode))
			{
				// 需要检索历史报价和历史成交最低价的条件
				HistoryLowPriceListRequest hisLowReq = new HistoryLowPriceListRequest
				{
					CompanyIdList = companyRelationIdList == null ? [] : companyRelationIdList,
					ProductCodeList = analyzedItems.Select(r => r.PrdCode).Distinct().ToList()
				};
				OnYearLowPrice = GetHistoryLowPrice(hisLowReq);   // 一年,公司用户最低价成交价,和报价,查询.
			}

			foreach (var item in analyzedItems)
			{
				var itemLowPrice = OnYearLowPrice.Where(r => r.ProductCode == item.PrdCode).FirstOrDefault();

				item.PriceCalculateResult.QuoLPrice = itemLowPrice != null ? itemLowPrice.QuoLPrice : null;
				item.PriceCalculateResult.DealLPrice = itemLowPrice != null ? itemLowPrice.DealLPrice : null;

			}


            // 处理批量查询该公司对应的等级折扣信息. 
            var gradDiscoutList = handleDiscoutPrice((customerParentCompanyId == null) ? customerCompanyId : customerParentCompanyId, analyzedItems);  //这里返回类别和折扣.
            foreach (var item in analyzedItems)
            {
                // 只历史,指定客户价目表不处理折扣
                if (item.CategoryId == 0 ) { continue; }   // 没有产品CategoryId不处理  这个 CategoryId 还不等同于小类.
                 if (item.PriceCalculateResult.PriceSource == PriceSource.history) { continue; }
                else if (item.PriceCalculateResult.PriceSource == PriceSource.Customer) { continue; }
                else if (item.PriceCalculateResult.PriceSource == PriceSource.cacheCustomer) { continue; }               

                
                var gradDiscountItem = gradDiscoutList.Where(r => r.ProductTypeId == item.TypeId).FirstOrDefault();
                if (gradDiscountItem == null)
                {
                    gradDiscountItem = gradDiscoutList.Where(r => r.ProductId == item.PrdId && r.ProductId > 0).FirstOrDefault();
                    if (gradDiscountItem == null)
                    {
                        gradDiscountItem = gradDiscoutList.Where(r => r.ProductCategoryId == item.CategoryId).FirstOrDefault();
                    }
                }
                
                if (gradDiscountItem != null)
                {
                    item.PriceCalculateResult.DiscountRate = gradDiscountItem.Discount;  // 等级折扣.
                    item.PriceCalculateResult.PreferentialUnitPrice = Math.Round(item.PriceCalculateResult.PreferentialUnitPrice * gradDiscountItem.Discount / 100, pricePrecision, MidpointRounding.AwayFromZero);
                }
             }
            
            return analyzedItems;
        }



        /// <summary>
        /// 非标单不走解析,赋值一些初始化值
        /// </summary>
        /// <param name="analyzedItems"></param>
        public List<InqDetailInfo> CalculateNonStandard(List<InputQuotationItemViewModel> inputQuotationItemModels)
        {
            List<InqDetailInfo> analyzedItems = [];

            // 处理报价逻辑
            foreach (var item in inputQuotationItemModels)
            {
                InqDetailInfo analyzedItem = new InqDetailInfo();
                analyzedItem.PrdCode = item.Product.Code;
                analyzedItem.PrdName = item.Product.Name;

                analyzedItem.PriceCalculateResult = new CalculateProductPriceResult()
                {
                    QtyDiscount = 100,
                    PreferentialUnitPrice = -1, 
                    OriginalUnitPrice = -1,
                    DeliveryDays = -1,
                    PriceSource = PriceSource.none,
                };

                analyzedItem.ImageUrl = "";
                analyzedItem.Published = true;
                analyzedItem.TypeId = 0;
                analyzedItem.OrgPrice = -1;
                analyzedItem.CategoryType = 0;
                analyzedItem.CategoryId = 0;
                analyzedItem.Memo =  string.Empty ;
                analyzedItem.PriceCalculateResult.DeliverySource = DeliverySource.none;  //货期来源
                analyzedItem.DeliveryDays = -1;
                analyzedItem.ShortNumber = "";
                analyzedItem.QtyDiscount =  100;

                analyzedItems.Add(analyzedItem);
            }    
            return analyzedItems;
        }


        private List<HistoryLowPrice> GetHistoryLowPrice(HistoryLowPriceListRequest req)
        {

			List<HistoryLowPrice> result = new List<HistoryLowPrice>();
			// 个人客户不查询展示
			if (req.CompanyIdList.Count == 0)
			{
				return result;
			}
			if (req.ProductCodeList.Count == 0)
			{
				return result;
			}

			//    计算一年内,最低历史报价 
			string Sql = @"Select Min(qd.FQD_UNIT_TAX_PRICE) as Price,id.FID_PRD_CODE PrdCode From  F_CUST_QUOTATION_DETAIL as qd 
                           inner Join F_CUST_INQUIRY_DETAIL as id on id.FID_DETAIL_ID = qd.FQD_INQ_DETAIL_ID 
                           inner Join F_CUST_INQUIRY_MSTR as im on im.FIM_INQ_ID=id.FID_INQ_MSTR_ID 
                            where im.CompanyId  in (@companyList) AND im.FIM_CREATE_DATE > @FIM_CREATE_DATE and  id.FID_PRD_CODE in (@prdCodeList) 
                            And qd.FQD_UNIT_TAX_PRICE > 0 Group By id.FID_PRD_CODE ";
			var PrdQLPriceData = _mymoooContext.SqlSugar.Ado.SqlQuery<PrdCodePrice>(Sql,
				new
				{
					prdCodeList = req.ProductCodeList,
					companyList = req.CompanyIdList,
					FIM_CREATE_DATE = DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd")
				}
				).ToList();
			for (int i = 0; i < PrdQLPriceData.Count; i++)
			{
				HistoryLowPrice priceItem = new HistoryLowPrice();
				priceItem.ProductCode = PrdQLPriceData[i].PrdCode;
				priceItem.QuoLPrice = PrdQLPriceData[i].Price;
				result.Add(priceItem);
			}

            
            //  最低成交价 1年内成交 最低价, 历史价无一年限制, 但这里有1年限制, 这里只是展示给产品经理看的.
            Sql = @"select Min(d.FBD_UNIT_TAX_PRICE) as Price , d.FBD_PRD_CODE as PrdCode
            from F_CUST_BOOK_MSTR b
            	inner join F_CUST_BOOK_DETAIL d on b.FBM_BOOK_ID = d.FBD_BOOK_MSTR_ID
            	left join F_CUST_QUOTATION_DETAIL id on d.InquiryItemId = id.FQD_INQ_DETAIL_ID and d.InquiryItemId > 0
            where b.CompanyId in (@companyList) And id.SubmittedOn >@SubmitDate And d.FBD_PRD_CODE in (@prdCodeList) and b.FBM_BOOK_STATE not in (3,8,9)  and d.FBD_UNIT_TAX_PRICE > 0
                Group By d.FBD_PRD_CODE
               ";
			var PrdDLPriceData = _mymoooContext.SqlSugar.Ado.SqlQuery<PrdCodePrice>(Sql,
				new
				{
					prdCodeList = req.ProductCodeList,
					companyList = req.CompanyIdList,
					SubmitDate = DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd")
				}
				).ToList();
			for (int i = 0; i < PrdDLPriceData.Count; i++)
			{
				var Item = result.Where(x => x.ProductCode == PrdDLPriceData[i].PrdCode).ToList();
				if (Item != null)
				{
					for (int j = 0; j < Item.Count; j++)  //其实也就一个.
					{
						Item[j].DealLPrice = PrdDLPriceData[i].Price;
					}
				}
				else
				{
					HistoryLowPrice priceItem = new HistoryLowPrice();
					priceItem.ProductCode = PrdDLPriceData[i].PrdCode;
					priceItem.DealLPrice = PrdDLPriceData[i].Price;
					result.Add(priceItem);
				}

			}
			//     Debug.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
			return result;
		}

		private List<CompanyGradeDiscountResponse> handleDiscoutPrice(Guid? customerCompanyId, List<InqDetailInfo> analyzedItems)
		{
			List<CompanyGradeDiscountResponse> gradDiscoutResult = new List<CompanyGradeDiscountResponse>();
			if (customerCompanyId == null)
			{
				return gradDiscoutResult;
			}
			var CategoryIdList = analyzedItems.Where(r => r.CategoryId > 0).GroupBy(r => r.CategoryId).Distinct().Select(r => r.Key).ToList();

			var TypeIdList = analyzedItems.Where(r => r.TypeId > 0).GroupBy(r => r.TypeId).Distinct().Select(r => r.Key).ToList();

			var PrdIdList = analyzedItems.Where(r => r.PrdId > 0).GroupBy(r => r.PrdId).Distinct().Select(r => r.Key).ToList();

			//Debug.WriteLine(JsonConvert.SerializeObject(CategoryIdList));

			// 查询该公司的公司折扣, 公司等级折扣对应公司的,沿用原逻辑. 

			// 具体公司等级折扣,查Type==2 的...  单实际数据没有2
			var companyDiscoutList = _mymoooContext.SqlSugar.Queryable<CompanyGradeDiscount>().
				Where(r => r.CompanyId == customerCompanyId && TypeIdList.Contains(r.ProductCategoryId) && r.Type == 2).ToList();

			for (int i = 0; i < companyDiscoutList.Count; i++)
			{
				CompanyGradeDiscountResponse c = new CompanyGradeDiscountResponse();
				c.ProductTypeId = (int)companyDiscoutList[i].ProductCategoryId;
				c.Discount = companyDiscoutList[i].Discount;
				gradDiscoutResult.Add(c);
			}


			// 公司等级折扣,查Type= 1的
			companyDiscoutList = _mymoooContext.SqlSugar.Queryable<CompanyGradeDiscount>().
				Where(r => r.CompanyId == customerCompanyId && PrdIdList.Contains(r.ProductCategoryId) && r.Type == 1).ToList();
			for (int i = 0; i < companyDiscoutList.Count; i++)
			{
				CompanyGradeDiscountResponse c = new CompanyGradeDiscountResponse();
				c.ProductId = (int)companyDiscoutList[i].ProductCategoryId;
				c.Discount = companyDiscoutList[i].Discount;
				if (gradDiscoutResult.Where(r => r.ProductCategoryId == c.ProductCategoryId).ToList().Count == 0)
				{
					gradDiscoutResult.Add(c);
				}
			}


			// 具体公司的等级折扣,根据ProductCategoryId 递归 查Type=0           
			string Sql = @"WITH ProductCategoryTree(Id, [Name], ParentId, [Level])
                                        AS
                                        (
                                                SELECT FPC_CLASS_ID AS Id,
                                                           FPC_CLASS_NAME AS[Name],
                                                           FPC_PARENT_CLASS_ID AS ParentId,
                                                           1 AS[Level]
                                                FROM F_PRD_CLASS
                                                WHERE FPC_CLASS_ID in (@CategoryIds)

                                                UNION ALL

                                                SELECT FPC_CLASS_ID AS Id,
                                                           FPC_CLASS_NAME AS[Name],
                                                           FPC_PARENT_CLASS_ID AS ParentId,
                                                           [Level] + 1 AS[Level]
                                                FROM F_PRD_CLASS AS c
                                                INNER JOIN ProductCategoryTree AS t
                                                        ON c.FPC_CLASS_ID = t.ParentId

                                        )
                                        SELECT  d.ProductCategoryId,d.Discount,t.Level
                                        FROM ProductCategoryTree AS t
                                        INNER JOIN CompanyGradeDiscount AS d
                                                ON t.Id = d.ProductCategoryId
                                        WHERE d.CompanyId = @CompanyId and d.Type = 0
                                        ORDER BY d.ProductCategoryId,t.[Level] ASC
               ";
			var DiscountList = _mymoooContext.SqlSugar.Ado.SqlQuery<GradePriceDiscout>(Sql,
				new
				{
					CategoryIds = CategoryIdList,
					CompanyId = customerCompanyId
				}
			 ).ToList();

			for (int i = 0; i < DiscountList.Count; i++)
			{
				CompanyGradeDiscountResponse c = new CompanyGradeDiscountResponse();
				c.ProductCategoryId = (int)DiscountList[i].ProductCategoryId;
				c.Discount = DiscountList[i].Discount;
				if (gradDiscoutResult.Where(r => r.ProductCategoryId == c.ProductCategoryId).ToList().Count == 0)
				{
					gradDiscoutResult.Add(c);
				}
			}

			// 以上是具体公司级别的等级折扣.
			//以下是通用的公司等级折扣.

			// 通用等级折扣,查Type==2 的
			var gradList = _mymoooContext.SqlSugar.Queryable<GradeDiscount>()
				.InnerJoin<Company>((g, c) => g.GradeId == c.GradeLevel)
				.Where((g, c) => c.Id == customerCompanyId && TypeIdList.Contains(g.ProductCategoryId) && g.Type == 2)
				.Select(g => new { ProductCategoryId = (int)g.ProductCategoryId, g.Discount })
				.ToList();

			for (int i = 0; i < gradList.Count; i++)
			{
				CompanyGradeDiscountResponse c = new CompanyGradeDiscountResponse();
				c.ProductTypeId = (int)gradList[i].ProductCategoryId;
				c.Discount = gradList[i].Discount;
				//   if (gradDiscoutResult.Where(r => r.ProductCategoryId == c.ProductCategoryId).ToList().Count == 0)
				//  {
				gradDiscoutResult.Add(c);
				//  }
			}


			// 通用等级折扣,查Type= 1的
			gradList = _mymoooContext.SqlSugar.Queryable<GradeDiscount>()
		   .InnerJoin<Company>((g, c) => g.GradeId == c.GradeLevel)
		   .Where((g, c) => c.Id == customerCompanyId && PrdIdList.Contains(g.ProductCategoryId) && g.Type == 1)
		   .Select(g => new { ProductCategoryId = (int)g.ProductCategoryId, g.Discount })
		   .ToList();

			for (int i = 0; i < gradList.Count; i++)
			{
				CompanyGradeDiscountResponse c = new CompanyGradeDiscountResponse();
				c.ProductId = (int)gradList[i].ProductCategoryId;
				c.Discount = gradList[i].Discount;
				//  if (gradDiscoutResult.Where(r => r.ProductCategoryId == c.ProductCategoryId).ToList().Count == 0)
				//  {
				gradDiscoutResult.Add(c);
				//  }
			}

			// 通用等级折扣,根据ProductCategoryId 递归 查Type=0           
			Sql = @"WITH ProductCategoryTree(Id, [Name], ParentId, [Level])
                                        AS
                                        (
                                                SELECT FPC_CLASS_ID AS Id,
                                                           FPC_CLASS_NAME AS[Name],
                                                           FPC_PARENT_CLASS_ID AS ParentId,
                                                           1 AS[Level]
                                                FROM F_PRD_CLASS
                                                WHERE FPC_CLASS_ID in (@CategoryIds)

                                                UNION ALL

                                                SELECT FPC_CLASS_ID AS Id,
                                                           FPC_CLASS_NAME AS[Name],
                                                           FPC_PARENT_CLASS_ID AS ParentId,
                                                           [Level] + 1 AS[Level]
                                                FROM F_PRD_CLASS AS c
                                                INNER JOIN ProductCategoryTree AS t
                                                        ON c.FPC_CLASS_ID = t.ParentId

                                        )
                                        SELECT  d.ProductCategoryId,d.Discount,t.Level
                                        FROM ProductCategoryTree AS t
                                        INNER JOIN GradeDiscount AS d
                                                ON t.Id = d.ProductCategoryId
                    inner join Company c on c.GradeLevel = d.GradeId
                                        WHERE c.Id = @CompanyId and d.Type = 0
                                        ORDER BY d.ProductCategoryId,t.[Level] ASC
               ";
			DiscountList = _mymoooContext.SqlSugar.Ado.SqlQuery<GradePriceDiscout>(Sql,
			   new
			   {
				   CategoryIds = CategoryIdList,
				   CompanyId = customerCompanyId
			   }
			).ToList();

			for (int i = 0; i < DiscountList.Count; i++)
			{
				CompanyGradeDiscountResponse c = new CompanyGradeDiscountResponse();
				c.ProductCategoryId = (int)DiscountList[i].ProductCategoryId;
				c.Discount = DiscountList[i].Discount;
				// if (gradDiscoutResult.Where(r => r.ProductCategoryId == c.ProductCategoryId).ToList().Count == 0)
				// {
				gradDiscoutResult.Add(c);
				// }
			}

			return gradDiscoutResult;
		}




        private ProductSeriesViewModel PrepareProductModel(InqDetailInfo analyzedItem, InputQuotationItemViewModel inputItem)
        {
            var found = analyzedItem.PrdId > 0;
            var analyedProductCode = analyzedItem.PrdCode;
            var analyedCustomCode = analyzedItem.CustItem == null ? string.Empty: analyzedItem.CustItem;
            var analyedProductName = analyzedItem.PrdName;
            var typedProductName = inputItem.Product.Name;
            var productCode = string.IsNullOrWhiteSpace(analyedProductCode) &&
                              new Regex("^[a-zA-Z0-9\\.*\\/\\-\\+_]+$").IsMatch(analyedCustomCode)
                ? analyedCustomCode
                : analyedProductCode;


			return new ProductSeriesViewModel
			{
				ProductId = analyzedItem.PrdId,
				TypeId = analyzedItem.TypeId,
				Code = productCode,
				Name = string.IsNullOrWhiteSpace(typedProductName) || found ? analyedProductName : typedProductName,
				Brand = inputItem.Product.Brand,
				ImageUrl =  analyzedItem.ImageUrl,
                Published = analyzedItem.Published
			};
		}



		public ManagementUser GetSaleMan(long customerId, Guid? companyId)
		{
			ManagementUser result = new ManagementUser();

			long salesManId = 0;
			// 个人。
			if (companyId == null || companyId.Equals(Guid.Empty))
			{
				salesManId = _mymoooContext.SqlSugar.Queryable<Customer_Salesman_Bound>()
					.Where(r => r.CustomerId == customerId && r.Type == (byte)SalesBindingType.Personal)
					.Select(r => r.SalesmanId)
					.First();
			}
			else // 企业。
			{
				salesManId = _mymoooContext.SqlSugar.Queryable<Customer_Salesman_Bound>()
					.Where(r => r.CustomerId == customerId && r.ThingId == companyId && r.Type == (byte)SalesBindingType.Company)
					.Select(r => r.SalesmanId)
					.First();
				if (salesManId == 0)
				{
					salesManId = _mymoooContext.SqlSugar.Queryable<Customer_Salesman_Bound>()
					.Where(r => r.ThingId == companyId && r.Type == (byte)SalesBindingType.Company)
					.Select(r => r.SalesmanId)
					.First();
				}
			}

			if (salesManId == 0)
			{
				var defaultSalemanId = _mymoooContext.RedisCache.HashGet(new SystemProfile() { Key = "SALESMAN" }, p => p.Value);
				salesManId = (defaultSalemanId == null) ? 0L : Convert.ToInt64(defaultSalemanId);// var defaultSalesmanId = 11111; // 从REDIS中获取 _dbSystemSettingService.DefaultSalesmanId;
			}
			result = _mymoooContext.SqlSugar.Queryable<ManagementUser>().Where(r => r.UserId == salesManId).First();
			return result;
		}





		/// <summary>
		/// 获取业务员最基层的部门信息.
		/// </summary>
		/// <param name="salesmanId"></param>
		/// <returns></returns>
		public Department LowestSalesDepartment(long salesmanId)
		{
			const string selectSql = @"WITH DepartmentTree(Id, Name, ParentId, [Level]) AS 
                                      (
	                                      SELECT d.Id, 
			                                     d.Name, 
			                                     d.ParentId, 
			                                     0 AS [Level]
	                                      FROM Department AS d
	                                      WHERE d.[Type] = @Type

	                                      UNION ALL

	                                      SELECT d.Id, 
			                                     d.Name, 
			                                     d.ParentId, 
			                                     [Level] + 1 AS [Level]
	                                      FROM Department AS d, DepartmentTree AS t
	                                      WHERE d.ParentId = t.Id
                                      )
                                      SELECT TOP 1 d.*
                                      FROM DepartmentTree AS t
                                      INNER JOIN Department AS d
	                                      ON t.Id = d.Id
                                      INNER JOIN Admin_Department_Mapping AS m
	                                      ON t.Id = m.DepartmentId
                                      INNER JOIN F_USER_MSTR AS u
	                                      ON u.FUM_USER_ID = m.AdminId
                                      WHERE u.FUM_USER_ID = @AdminId
                                      ORDER BY t.[Level] DESC";



			return _mymoooContext.SqlSugar.Ado.SqlQuery<Department>(selectSql,
						new
						{
							AdminId = salesmanId,
							Type = (byte)CompanyStructureType.SalesDepartment
						}).First();
		}



		public async void SendRabbitMqPlaceQuotationOrder(long inquiryQuotationOrderId)
		{
			var inqOrder = _mymoooContext.SqlSugar.Queryable<InquiryOrder>().Where(r => r.InquiryId == inquiryQuotationOrderId).First();
			var quotationNumber = inqOrder.InquiryNumber;
			await _gatewayService.SendMessage("place_quotation_order_", JsonSerializerOptionsUtils.Serialize(new { quotationNumber }));
		}


        [GeneratedRegex(@"\s")]
        private static partial Regex MyRegex();





        public  string GetSalesOrgByQuoOrderNumber(string orderNumber)
        {
			if (string.IsNullOrEmpty(orderNumber))
			{
				return string.Empty;
			}
            var result = _mymoooContext.SqlSugar.Queryable<InquiryOrder>()
				.Where(r => r.InquiryNumber == orderNumber).Select(r=>r.FIM_SALES_DEF_CODE).First();
			if (result == null)
			{
				return string.Empty;
			}
			return result;
            
        }


    }

}
