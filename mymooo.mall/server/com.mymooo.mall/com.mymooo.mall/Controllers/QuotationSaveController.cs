using com.mymooo.mall.core.Model.InquiryOrder;
using com.mymooo.mall.core.Model.Product;
using com.mymooo.mall.core.Model.Quotation;
using com.mymooo.mall.core.Model.QuotationOrder;
using com.mymooo.mall.core.Model.ResolveOrder;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.CustomerInformation;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.ProductInformation;
using com.mymooo.mall.core.SqlSugarCore.SalesBusiness;
using com.mymooo.mall.core.SqlSugarCore.SystemManagement;
using Elastic.Clients.Elasticsearch.Core.Search;
using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Utils.JsonConverter;
using Org.BouncyCastle.Ocsp;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace com.mymooo.mall.Controllers
{
	/// <summary>
	/// 报价
	/// </summary>
	public partial class QuotationController : BaseController
	{
		/// <summary>
		/// 下单
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost]
		public ResponseMessage<dynamic> PlaceOrder([FromBody] PlaceQuotationOrderRequest request)
		{
			ResponseMessage<dynamic> response = new();
			int inquiryQuotationOrderId = 0;
			long inquiryOrderId = 0;

			if (request == null)
			{
				response.Code = ResponseCode.ModelError;
				response.ErrorMessage = "出错啦,提交的数据为空";
				return response;
			}

            // REDIS 缓存单号.避免重复提交 request.InquiryNumber
            if (!string.IsNullOrEmpty(request.InquiryNumber))
			{
                //var x =  _mymoooContext.RedisCache.HashExists(new InquiryPlaceOrderCache() { IKey = request.InquiryNumber });
                //var xx = _mymoooContext.RedisCache.KeyExists(new InquiryPlaceOrderCache() { IKey = request.InquiryNumber });

                bool hasKey = _mymoooContext.RedisCache.HashExists(new InquiryPlaceOrderCache() { IKey = request.InquiryNumber });
                if (hasKey)
				{
                    response.Code = ResponseCode.ModelError;
                    response.ErrorMessage = "检测到你刚已提交过相同单号的报价单.请不要重复提交";
                    return response;
                }
                InquiryPlaceOrderCache iquiryCache = new InquiryPlaceOrderCache()
				{
					IKey = request.InquiryNumber,
					IValue = request.InquiryNumber
				};

				_mymoooContext.RedisCache.HashSet(iquiryCache, TimeSpan.FromMinutes(10));
			}

            var customerId = request.Buyer.CustomerId;   // 客户
			var company = request.CompanyId == null ? null : _mymoooContext.SqlSugar.Queryable<Company>().Where(r => r.Id == request.CompanyId).First();   //公司客户

			var salesman = _quotationService.GetSaleMan(customerId, request.CompanyId);
			var weChatCode = _mymoooContext.User?.Code;

			CustomerAddress receive = _mymoooContext.SqlSugar.Queryable<CustomerAddress>().Where(r => r.Id == request.AddressId).First();
			//AddTask(new Task(() => receive = GetAddress(request.AddressId)));

			// customer 中需要的内容不一定全
			CustomerUser customer = _mymoooContext.SqlSugar.Queryable<CustomerUser>().Where(r => r.FUM_USER_ID == customerId).First();
			//AddTask(new Task(() => customer = GetCustomer(customerId)));

			// 业务员所在的最基层部门信息
			//Department department = null;
			//AddTask(new Task(() => department = GetDepartment(salesman.Id)));
			// 这个部门是蚂蚁后台组织架构中的部门,非企业微信组织架构.
			if (salesman == null)
			{
				response.Code = ResponseCode.ModelError;
				response.ErrorMessage = "未找到缺省的业务员";
				return response;
			}

			Department department = _quotationService.LowestSalesDepartment(salesman.UserId);


			var tempString = _mymoooContext.RedisCache.HashGet(new SystemProfile() { Key = "QUOTATION_VALIDITY" }, p => p.Value);
			int extendableToNDays = string.IsNullOrEmpty(tempString) ? 15 : Convert.ToInt32(tempString); //默认的报价单交期

			// 获取未来20天的工厂生产日历
			List<FacCalculate> facCalculate = GetFacCalculate();
			// 获取所有产品工程师部门
			var EngineerListDept = LowestPurchaseDepartmentUserList();
			//专门增加一步, 处理产品工程师分配.  返回产品型号-产品工程师-产品经理
			List<ProductCodeEnginer> productCodeEnginerList = PersistQuotationEnginer(request.QuotationItems);

			// 注意: 客户报价精度是单价精度. 这个看似是前端计算的值.直接保存的.后端似乎没有处理
			var inquiryOrder = NewInquiryOrder(request, company, customer, department,
				salesman, extendableToNDays, receive);

			var quotationOrder = NewQuotationOrder(request, inquiryOrder);   //构造报价单


			var inquiryItems = new List<InquiryItem>();
			var quotationItems = new List<QuotedItem>();
			foreach (var quotationItemModel in request.QuotationItems)
			{
				var inquiryItem = NewInquiryItem(quotationItemModel, productCodeEnginerList);
				inquiryItems.Add(inquiryItem);

				var quotationItem = NewQuotationItem(quotationItemModel, inquiryItem, quotationOrder, facCalculate, productCodeEnginerList);
				quotationItems.Add(quotationItem);
			}
			// 写库操作
			try
			{
				_mymoooContext.SqlSugar.BeginTran();

				List<InquiryOrderDetail> inquiryOrderDetailDals = [];
				InquiryOrder inquiryOrderDal = new InquiryOrder
				{
					UserId = inquiryOrder.Customer.FUM_USER_ID,
					Deadline = inquiryOrder.ExpiryDate, //截止报价时间
					DeliveryAddress = inquiryOrder.ReceiveAddress,
					DeliveryName = inquiryOrder.Receiver,
					DeliveryMobile = inquiryOrder.ReceiverMobile,
					ExpectedDeliveryDays = inquiryOrder.ExpectedDeliveryDays,
					ExpectedTotalWithTax = inquiryOrder.ExpectedTotalWithTax,
					PayName = inquiryOrder.Purchaser,
					CustomerPurchaseNumber = inquiryOrder.CustomerPurchaseNumber,
					FIM_SALES_DEF_CODE = inquiryOrder.SalesOrganizationCode,
					FIM_SALES_DEF_NAME = inquiryOrder.SalesOrganizationName,
					ReceiverCompany = inquiryOrder.ReceiveCompany,
					ReceiverDepartment = inquiryOrder.ReceiveDepartment,
					PurchaserCompany = inquiryOrder.PurchaseCompany,
					PurchaserAddress = inquiryOrder.PurchaserAddress,
					PurchaserDepartment = inquiryOrder.PurchaseDepartment,
					CompanyId = inquiryOrder.Company?.Id,
					PayMobile = inquiryOrder.PurchaserMobile,
					SalesmanId = inquiryOrder.Salesman?.UserId ?? 0,
					DepartmentId = inquiryOrder.DepartmentId,
					DataSources = (byte)inquiryOrder.OrderSouce,  // 订单来源 Web , IOS 等
					DeliveryId = inquiryOrder.ReceiveId,
					InputterId = inquiryOrder.Inputter?.UserId ?? 0,
					OrderType = (byte)inquiryOrder.OrderType,  //标准,非标,3D,样品
					PaymentMethodId = inquiryOrder.InqPaymentMethodId,
					SpecialPrice = inquiryOrder.SpecialPrice,
					IsInternal = inquiryOrder.IsInternal,
					Agent = inquiryOrder.Agent,
					InquiryNumber = request.InquiryNumber ?? "",
					IsNonStandard = request.IsNonStandard,
					UploadPath = request.UploadPath,
					SpecialVersion = "V1.0"
				};

				inquiryOrderId = _mymoooContext.SqlSugar.Insertable(inquiryOrderDal).ExecuteReturnIdentity();

				// 处理报价单 主从表
				InquiryQuotationOrder IQOM = new InquiryQuotationOrder
				{
					FileName = quotationOrder.FileName ?? string.Empty,
					QuotationTotalAmount = quotationOrder.TotalWithoutTax,  // 总计不含税价
					QuotationTaxAmount = quotationOrder.Tax,  // 税率
					FQM_FREIGHT_FEE = quotationOrder.Shipping,
					FQM_FREIGHT_DISCOUNT = 0, // 运费折扣, 没用上
					QuotationTotalTaxAmount = quotationOrder.TotalWithTax,   //总计含税价
					InquiryId = inquiryOrderId, // 询价单ID,
					Discount = quotationOrder.Discount,
					KnockOff = quotationOrder.KnockOff,  // 减价
					FreightToBeCollected = quotationOrder.FreightToBeCollected,   // 是否运费到付. false
					TaxRate = quotationOrder.Tax,
					ExpiryDate = inquiryOrder.ExpiryDate// 其实都一样, DateTime.Now.AddDays(extendableToNDays);
				};

				inquiryQuotationOrderId = _mymoooContext.SqlSugar.Insertable(IQOM).ExecuteReturnIdentity();

				// 处理询价单明细
				for (int i = 0; i < inquiryItems.Count; i++)
				{
					InquiryOrderDetail iOD = new InquiryOrderDetail
					{
						InquiryId = inquiryOrderId,  // FID_INQ_MSTR_ID    
						Seq = i + 1,
						ProductId = inquiryItems[i].ProductSeriesId,
						ProductNumber = inquiryItems[i].ProductCode,
						Qty = inquiryItems[i].Qty,
						CustItemNo = inquiryItems[i].CustItemNo,
						Status = (long)inquiryItems[i].Status,
						ProductEngineerId = inquiryItems[i].ProductEngineerId,
						ProductEngineerName = inquiryItems[i].ProductEngineerName,
						ProductManagerId = inquiryItems[i].ProductManagerId,
						ProductManagerName = inquiryItems[i].ProductManagerName,
						ProductName = inquiryItems[i].ProductName,
						BrandId = inquiryItems[i].BrandId,
						ParentId = 0,
						ProductType = inquiryItems[i].ProductType,
						ProductTypeId = inquiryItems[i].TypeId,
						Materials = inquiryItems[i].Materials,
						IsHistory = inquiryItems[i].IsHistory,
						CustomerProductName = inquiryItems[i].CustItemName,
						CustomerProductNumber = inquiryItems[i].CustomCode,
						BusinessDivisionId = inquiryItems[i].BusinessDivisionId,
						BusinessDivisionName = inquiryItems[i].BusinessDivisionName,
						SupplyOrgId = inquiryItems[i].SupplyOrgId,
						SupplyOrgName = inquiryItems[i].SupplyOrgName,
						ProjectNo = inquiryItems[i].ProjectNo,
						Storage = inquiryItems[i].Storage,
						ShortNumber = inquiryItems[i].ShortNumber,
						IsSpecial = inquiryItems[i].IsSpecial
					};
					inquiryOrderDetailDals.Add(iOD);
				}

				var inquiryOrderDetailIds = _mymoooContext.SqlSugar.Insertable(inquiryOrderDetailDals).ExecuteReturnPkList<long>();

				//把返回的批量自增Id,赋值给实体.后面要用.
				for (int i = 0; i < inquiryOrderDetailIds.Count; i++)
				{
					inquiryOrderDetailDals[i].InquiryDetailId = inquiryOrderDetailIds[i];
				}


				List<InquiryQuotationOrderDetail> IQODs = [];
				for (int i = 0; i < quotationItems.Count; i++)
				{
					InquiryQuotationOrderDetail IQOD = new()
					{
						ResolveDepartmentId = quotationItems[i].ResolveDepartmentId,
						OriginalUnitPriceWithTax = quotationItems[i].OriginalUnitPriceWithTax,
						Version = quotationItems[i].Version,
						AuditedBy = quotationItems[i].AuditedBy,
						SubtotalWithoutTax = quotationItems[i].SubtotalWithoutTax,
						SubtotalWithTax = quotationItems[i].SubtotalWithTax,
						ProjectNo = quotationItems[i].ProjectNo,
						CustItemNo = quotationItems[i].CustItemNo,
						StockFeatures = quotationItems[i].StockFeatures,
						BeforeWholeDiscountUnitPriceWithTax = quotationItems[i].BeforeWholeDiscountUnitPriceWithTax,
						RejectType = quotationItems[i].RejectType,
						ResolvedSubmitUserName = quotationItems[i].ResolvedSubmitUserName,
						RequirementDate = quotationItems[i].RequirementDate,
						RequirementDays = quotationItems[i].RequirementDays,
						DesireDeliveryDays = quotationItems[i].DesireDeliveryDays,  //期望交期
						DesirePrice = quotationItems[i].DesirePrice,   //期望价格

						Price = quotationItems[i].UnitPriceWithoutTax,
						TaxPrice = quotationItems[i].UnitPriceWithTax,
						DeliveDays = quotationItems[i].DispatchDays,
						InquiryDetailId = inquiryOrderDetailIds[i], // 这个事询价单,项目ID.  排序一定不能乱
						QuotationId = inquiryQuotationOrderId, // 报价单主表ID, 
						ResolveStatus = (byte)quotationItems[i].ResolveStatus,
						Quantity = quotationItems[i].Quantity,
						SupplierDispatchDays = quotationItems[i].SupplierDispatchDays,
						FirstCost = quotationItems[i].FirstCost,
						SupplierId = quotationItems[i].SupplierId,
						AuditStatus = (byte)quotationItems[i].AuditStatus,
						Memo = quotationItems[i].Remark,
						InsideRemark = quotationItems[i].InsideRemark,
						AskPrice = quotationItems[i].AskPrice,
						OriginalPrice = quotationItems[i].OriginalUnitPriceWithTax,
						QtyDiscount = quotationItems[i].QtyDiscount,
						LevelDiscount = quotationItems[i].LevelDiscount,
						CategoryType = (byte)quotationItems[i].CategoryType,
						ParentId = 0,  // 现在都没有子项了
						SmallClassId = quotationItems[i].SmallClassId,
						LargeClassId = quotationItems[i].LargeClassId,
						PriceSource = quotationItems[i].PriceSource == null ? null : (int?)(quotationItems[i].PriceSource),
						DeliverySource = (quotationItems[i].DeliverySource == null) ? null : (int?)quotationItems[i].DeliverySource,
						FQD_QUO_LPRICE = quotationItems[i].QuoLPrice,
						FQD_DEAL_LPRICE = quotationItems[i].DealLPrice,
						AttachmentUrl = quotationItems[i].AttachmentUrl,
						PurchaseRemark = quotationItems[i].PurchaseRemark,
					};
					IQOD.AutoDeliveryDays = IQOD.DeliveDays;
					IQOD.AutoDeliverySource = IQOD.DeliverySource;
					IQOD.AutoPriceSource = IQOD.PriceSource;
					IQOD.AutoUnitPrice = IQOD.FirstCost;
					IQODs.Add(IQOD);
				}
				_mymoooContext.SqlSugar.Insertable(IQODs).ExecuteCommand();


				// 开始处理分解单逻辑, 分解单数据大多来源于 询价单和报价单.不再单独定义BAL的MODEL

				// 一个报价单,可能对应多个分解单主表
				foreach (var allotedToEngineer in inquiryOrderDetailDals.GroupBy(item => item.ProductEngineerId).Select(item => item.First()))
				{

					List<ResolvedOrderItem> resolvedOrderItemList = [];
					List<ResolvedHistory> resolvedHistoryList = [];

					var EnginerrDept = EngineerListDept.Where(r => r.FUM_USER_ID == allotedToEngineer.ProductEngineerId).FirstOrDefault();
					Guid deptGuid = Guid.Empty;
					if (EnginerrDept != null)
					{
						deptGuid = EnginerrDept.Id;
					}
					else
					{
						throw new Exception("分配的产品工程师没有部门");   // 这种情况不大可能存在.
					}
					//分解单主表  , allotedToEngineer.InquiryItem.Id
					var resolvedOrder = PersistNewResolvedOrder(IQODs, inquiryOrderDetailDals, inquiryOrderId, deptGuid, allotedToEngineer.ProductEngineerId);

					// 分解单主表写入
					var resolvedOrderId = _mymoooContext.SqlSugar.Insertable(resolvedOrder).ExecuteReturnIdentity();
					// 分配给该产品工程师的项目
					var allotedEngineerQuotationItems = inquiryOrderDetailDals.Where(item => item.ProductEngineerId == allotedToEngineer.ProductEngineerId).ToList();
					foreach (var resolvingItemAllotedToEngineer in allotedEngineerQuotationItems)
					{
						var inquiryItem = inquiryOrderDetailDals.Where(r => r.InquiryDetailId == resolvingItemAllotedToEngineer.InquiryDetailId).FirstOrDefault();

						var quotedItem = IQODs.Where(r => r.InquiryDetailId == resolvingItemAllotedToEngineer.InquiryDetailId).FirstOrDefault();

						if (inquiryItem == null)
						{
							throw new Exception("Inquiry明细数据没找到");   // 这种情况不大可能存在.
						}
						if (quotedItem == null)
						{
							throw new Exception("quotedItem报价明细数据没找到");   // 这种情况不大可能存在.
						}

						var resolvedItem = NewResolvedItem(inquiryItem, quotedItem, resolvedOrder);
						if (resolvedItem.IsAutoQuote == 1)
						{
							resolvedHistoryList.Add(NewResolvedHistory(resolvedItem, inquiryItem, inquiryOrder));
						}
						resolvedOrderItemList.Add(resolvedItem);

					}
					var orderNumber = _mymoooContext.SqlSugar.Queryable<ResolvedOrder>().Where(it => it.Id == resolvedOrderId).Select(it => it.OrderNumber).Single();
					for (int i = 0; i < resolvedOrderItemList.Count; i++)
					{
						resolvedOrderItemList[i].ResolvedOrderNumber = orderNumber;
					}
					// 执行分解单明细表写入
					var resolvedOrdeDetailResult = _mymoooContext.SqlSugar.Insertable(resolvedOrderItemList).ExecuteReturnPkList<long>();

					for (int i = 0; i < resolvedOrdeDetailResult.Count; i++)
					{
						var ri = resolvedHistoryList.Where(r => r.InqId == resolvedOrderItemList[i].InquiryItemId).FirstOrDefault();
						if (ri != null)
						{    // 分解单历史写入
							ri.ResolvedItemId = (int)resolvedOrdeDetailResult[i];
							_mymoooContext.SqlSugar.Insertable(ri).ExecuteReturnIdentity();
						}
					}
					// 分解单历史表写入                    
				}
				// 结束处理分解单逻辑


				_mymoooContext.SqlSugar.CommitTran();

				_quotationService.SendRabbitMqPlaceQuotationOrder(inquiryOrderId);
				//throw new Exception("");
			}
			catch (Exception ex)
			{
				_mymoooContext.SqlSugar.RollbackTran();
				response.Code = ResponseCode.Exception;
				response.Data = ex.Data;
				response.ErrorMessage = ex.Message;
				return response;
			}




			response.Code = ResponseCode.Success;
			response.Data = new
			{
				QuotationOrderId = inquiryOrderId    // 这里是询价单Id 
			};

			return response;
		}


		private InquiryOrderBAL NewInquiryOrder(PlaceQuotationOrderRequest request, Company? company, CustomerUser customer,
			Department department, ManagementUser salesman, int extendableToNDays, CustomerAddress receive)
		{
			string companyAddress = string.Empty;
			string customerCompanyName = string.Empty;
			if (company != null)
			{
				//// 通过地区编码,查询内容.
				string[] regions = [];
				// 公司,省,市,区,都配置才拼查
				if (company.ProvinceId != null && company.ProvinceId != null && company.CityId != null)
				{
					regions = _mymoooContext.SqlSugar.Queryable<RegionDict>()
				   .Where(r => (company.ProvinceId == r.Id) || (company.CityId == r.Id) || (company.CityId == r.Id))
				   .Select(r => r.Name)
				   .ToArray();
				}

				companyAddress = string.Join("", regions);
				companyAddress = string.Concat(companyAddress, company.Address);
			}
			else
			{
				var cum = _mymoooContext.SqlSugar.Queryable<CustomerUserMstr>()
				.Where(r => r.FCM_CUST_ID == customer.FUM_CUST_ID)
				.Select(r => new { r.FCM_ADDR_CHI, r.FCM_NAME_CHI })  // 中文地址.名称
				.First();
				if (cum != null)
				{
					customerCompanyName = cum.FCM_NAME_CHI;
					companyAddress = cum.FCM_ADDR_CHI;
				}
			}

			var inquiryOrder = new InquiryOrderBAL
			{
				Company = company,
				Customer = customer,
				DepartmentId = department.Id,
				Salesman = salesman,
				Inputter = new ManagementUser
				{
					UserId = _mymoooContext.User?.MymoooUserId ?? 0
				},
				CustomerPurchaseNumber = request.CustomerPurchaseNumber ?? string.Empty,
				SalesOrganizationCode = request.SalesOrganizationCode,
				SalesOrganizationName = request.SalesOrganizationName,
				SpecialPrice = request.SpecialPrice,
				IsInternal = request.IsInternal,
				ExpiryDate = DateTime.Now.AddDays(extendableToNDays),
				PurchaseCompany = company == null ? customerCompanyName : company.Name,  // 个人名义采购,公司名也得填上.
				PurchaseDepartment = request.Buyer.Department,
				PurchaserAddress = companyAddress,
				PurchaserMobile = customer.FUM_USER_NO,   // 注册账号,  原是 Account
				Purchaser = request.Buyer.Name,
				ReceiverMobile = receive.Mobile,
				OrderSouce = request.Datasouces,
				ReceiveId = receive.Id,
				Receiver = receive.Receiver,
				ReceiveDepartment = receive.Department,
				ReceiveCompany = receive.Company,
				ReceiveAddress = receive.Province +
								 (string.IsNullOrEmpty(receive.City) ? string.Empty : receive.City) +
								 (string.IsNullOrEmpty(receive.District) ? string.Empty : receive.District) +
								 receive.Address,
				Agent = true,
				InqPaymentMethodId = request.Buyer.InqPaymentMethodId,
				IsNonStandard = request.IsNonStandard
			};
			return inquiryOrder;
		}


		private QuotedOrderBAL NewQuotationOrder(PlaceQuotationOrderRequest request, InquiryOrderBAL inquiryOrder)
		{
			var totalWithTax = CalculateTotalWithTax(request.QuotationItems, inquiryOrder.Company);
			var quotationOrder = new QuotedOrderBAL
			{
				Discount = null,
				Shipping = 0,              //运费
				ShippingDiscount = 0,      //运费折扣
				FreightToBeCollected = true,   //是否运费到付, 缺省为true 订购时再改
				InquiryOrder = inquiryOrder,
				KnockOff = 0,
				TotalWithoutTax = totalWithTax[1],
				Tax = Math.Round(totalWithTax[0] - totalWithTax[1], 2, MidpointRounding.AwayFromZero),
				FileName = request.FileName
			};
			return quotationOrder;
		}

		private decimal CalculateTotalWithoutTax(decimal totalWithTax)
		{
			var tempString = _mymoooContext.RedisCache.HashGet(new SystemProfile() { Key = "TAX_RATE" }, p => p.Value);
			decimal tax = string.IsNullOrEmpty(tempString) ? (decimal)13.0 : Convert.ToDecimal(tempString); //默认的税率
			return Math.Round(totalWithTax / (1 + tax / 100), 4, MidpointRounding.AwayFromZero);
		}


		private List<decimal> CalculateTotalWithTax(IEnumerable<ConfirmQuotationItemViewModel> quotationItemModels, Company? company)
		{
			var tempString = _mymoooContext.RedisCache.HashGet(new SystemProfile() { Key = "TAX_RATE" }, p => p.Value);
			decimal taxRate = string.IsNullOrEmpty(tempString) ? (decimal)13.0 : Convert.ToDecimal(tempString); //默认的税率

			List<decimal> list = [
				0,//含税
				0//未税
			];
			foreach (var quotationItemModel in quotationItemModels.
				Where(quotationItemModel => (quotationItemModel.InputUnitPriceWithTax != null ||
											 quotationItemModel.SystemUnitPriceWithTax != null)))
			{
				if (quotationItemModel.SystemUnitPriceWithTax != null &&
					quotationItemModel.SystemUnitPriceWithTax > 0 &&
					quotationItemModel.SelectedSystemUnitPriceWithTax)
				{
					var unitPriceWithTax = Math.Round((decimal)quotationItemModel.SystemUnitPriceWithTax, company == null ? 4 : company.DecimalPlacesOfUnitPrice, MidpointRounding.AwayFromZero);
					list[0] += Math.Round(quotationItemModel.Quantity * unitPriceWithTax, 2, MidpointRounding.AwayFromZero);



					list[1] += Math.Round(quotationItemModel.Quantity *
									Math.Round(unitPriceWithTax / (1 + taxRate / 100), company == null ? 4 : company.DecimalPlacesOfUnitPrice, MidpointRounding.AwayFromZero), 2, MidpointRounding.AwayFromZero);
					continue;
				}

				if (quotationItemModel.InputUnitPriceWithTax != null &&
					quotationItemModel.InputUnitPriceWithTax > 0)
				{
					var unitPriceWithTax = Math.Round((decimal)quotationItemModel.InputUnitPriceWithTax, company == null ? 4 : company.DecimalPlacesOfUnitPrice, MidpointRounding.AwayFromZero);
					list[0] += Math.Round(quotationItemModel.Quantity *
									Math.Round((decimal)quotationItemModel.InputUnitPriceWithTax, company == null ? 4 : company.DecimalPlacesOfUnitPrice, MidpointRounding.AwayFromZero), 2, MidpointRounding.AwayFromZero);
					list[1] += Math.Round(quotationItemModel.Quantity *
								  Math.Round(unitPriceWithTax / (1 + taxRate / 100), company == null ? 4 : company.DecimalPlacesOfUnitPrice, MidpointRounding.AwayFromZero), 2, MidpointRounding.AwayFromZero);
				}
			}
			return list;
		}




		private static InquiryItem NewInquiryItem(ConfirmQuotationItemViewModel quotationItemModel, List<ProductCodeEnginer> productCodeEnginers)
		{
			var productCodeEnginer = productCodeEnginers.Where(r => r.ProductCode == quotationItemModel.Product.Code).FirstOrDefault() ?? throw new Exception(quotationItemModel.Product.Code + "没有匹配到产品工程师");
			// 产品工程师部门Id 询价单没有,在报价单明细中有.
			var inquiryStatus = GetInquiryStatus(quotationItemModel);
			var inquiryItem = new InquiryItem
			{
				Qty = quotationItemModel.Quantity,
				ProductCode = quotationItemModel.Product.Code,
				ProductEngineerName = productCodeEnginer.EngineerName,
				ProductEngineerId = productCodeEnginer.EngineerId,
				ProductManagerId = productCodeEnginer.EngineerManagerId,
				ProductManagerName = productCodeEnginer.EngineerManagerName,
				CustomCode = quotationItemModel.CustomCode,
				CustItemName = quotationItemModel.CustItemName,
				ProductName = quotationItemModel.Product.Name,
				ProductSeriesId = quotationItemModel.Product.ProductId,
				TypeId = quotationItemModel.Product.TypeId,
				ProjectNo = quotationItemModel.ProjectNo,
				CustItemNo = quotationItemModel.CustItemNo,
				Status = inquiryStatus,
				// InquiryOrder = inquiryOrder,
				IsHistory = quotationItemModel.IsHistory,
				BrandId = quotationItemModel.Product.Brand?.BrandId, // 这个地方改为直接赋值GUID
				BusinessDivisionId = quotationItemModel.ProductSmallClass?.BusinessDivisionId ?? "",
				BusinessDivisionName = quotationItemModel.ProductSmallClass?.BusinessDivisionName ?? "",
				SupplyOrgId = quotationItemModel.SupplyOrgId,
				SupplyOrgName = quotationItemModel.SupplyOrgName ?? "",
				Storage = quotationItemModel.Storage,
				ShortNumber = quotationItemModel.ShortNumber,
				IsSpecial = false,         //inquiryOrder.SpecialPrice ? true : false, // 整单特价单,则每项是特价  -- 这个逻辑去掉
			};
			return inquiryItem;

		}

		private QuotedItem NewQuotationItem(ConfirmQuotationItemViewModel quotationItemModel, InquiryItem inquiryItem,
			QuotedOrderBAL quotationOrder, List<FacCalculate> FactoryCalendar, List<ProductCodeEnginer> productCodeEnginers)
		{
			var brandCode = quotationItemModel.Product.Brand != null ? quotationItemModel.Product.Brand.Code : string.Empty;
			var unitPriceWithTax = GetUnitPriceWithTax(quotationItemModel, quotationOrder.InquiryOrder.Company);
			var dispatchDays = GetDispatchDays(quotationItemModel);
			var tempString = _mymoooContext.RedisCache.HashGet(new SystemProfile() { Key = "TAX_RATE" }, p => p.Value);
			decimal taxRate = string.IsNullOrEmpty(tempString) ? (decimal)13.0 : Convert.ToDecimal(tempString); //默认的税率
			var enDept = productCodeEnginers.Where(r => r.EngineerId == inquiryItem.ProductEngineerId).FirstOrDefault();
			var quotationItem = new QuotedItem
			{
				DispatchDays = dispatchDays,
				RequirementDate = HandleCalendarDate(dispatchDays, FactoryCalendar),// _calendarService.Calculate(dispatchDays),  // 根据工厂日历,算交期.  挪循环外计算,默认不休息,再根据工厂日历延后几天.
				RequirementDays = dispatchDays,
				UnitPriceWithTax = unitPriceWithTax,
				UnitPriceWithoutTax = unitPriceWithTax <= 0
					? unitPriceWithTax
					: Math.Round(unitPriceWithTax / (1 + taxRate / 100), 4, MidpointRounding.AwayFromZero),
				InquiryItem = inquiryItem,
				QuotedOrder = quotationOrder,
				ProductEngineerId = inquiryItem.ProductEngineerId,
				ProductEngineerName = inquiryItem.ProductEngineerName,
				ProductManagerId = inquiryItem.ProductManagerId,
				ProductManagerName = inquiryItem.ProductManagerName,
				ResolveDepartmentId = enDept?.DepartId,   //  分解部门,待运算
				FirstCost = quotationItemModel.SystemUnitPriceWithTax > 0 ? (decimal)quotationItemModel.SystemUnitPriceWithTax : unitPriceWithTax,
				Remark = quotationItemModel.Remark ?? string.Empty,
				PurchaseRemark = quotationItemModel.PurchaseRemark ?? string.Empty,
				InsideRemark = quotationItemModel.InsideRemark,
				OriginalUnitPriceWithTax = quotationItemModel.OrgPrice,
				QtyDiscount = (quotationItemModel.QtyDiscount < 0.0000001M) ? 100 : quotationItemModel.QtyDiscount,
				LevelDiscount = quotationItemModel.LevelDiscount,
				ResolveStatus = ResolveOrderStatus.Did,
				AuditStatus = QuoteOrderAuditStatus.IsDraft,
				Quantity = quotationItemModel.Quantity,
				AskPrice = true,
				CategoryType = (CategoryType)quotationItemModel.CategoryType,
				SubtotalWithTax = decimal.Round((decimal)unitPriceWithTax * quotationItemModel.Quantity, 2, MidpointRounding.AwayFromZero),
				ProductSmallClass = quotationItemModel.ProductSmallClass,
				ProjectNo = quotationItemModel.ProjectNo,
				CustItemNo = quotationItemModel.CustItemNo,
				StockFeatures = quotationItemModel.StockFeatures,
				SmallClassId = quotationItemModel.ProductSmallClass?.Id,
				LargeClassId = quotationItemModel.ProductSmallClass?.ParentId,
				PriceSource = quotationItemModel.PriceSource,
				DeliverySource = quotationItemModel.DeliverySource,
				QuoLPrice = quotationItemModel.QuoLPrice,
				DealLPrice = quotationItemModel.DealLPrice,
				AttachmentUrl = string.IsNullOrEmpty(quotationItemModel.AttaFilesName) ? string.Empty : quotationItemModel.AttaFilesName,  //前端有可能传null
				DesirePrice = quotationItemModel.DesirePrice,
				DesireDeliveryDays = quotationItemModel.DesireDeliveryDays,

			};
			quotationItem.SubtotalWithoutTax = decimal.Round((decimal)quotationItem.UnitPriceWithoutTax * quotationItemModel.Quantity, 2, MidpointRounding.AwayFromZero);
			//如果是自动提交报价




			//if (_dbSystemSettingService.QuoteAutoSubmit == 1)
			//{
			if (quotationItem.UnitPriceWithoutTax > 0 && quotationItem.DispatchDays >= 0 && quotationItem.ProductSmallClass != null && quotationItem.ProductSmallClass.Id > 0 && !quotationItemModel.IsTempSmallClass && (quotationItemModel.Product.ProductId == 0 || quotationItemModel.Product.Published))
			{
				bool isAutoSubmit = true;

				// 如果属于,型材,非标,不自动审核.
				// 13个非标小类铝型材；铝型材框架；功能组件；CNC产线；原材料贸易；工业原材料               
				string? tString = _mymoooContext.RedisCache.HashGet(new SystemProfile() { Key = "NotAutoQuoteAuditSmallClassId" }, p => p.Value);
				if (string.IsNullOrEmpty(tString))
				{
					tString = "89";  //容错代码 非标轴.
				}

				if (tString.Split(',').Select(s => long.Parse(s)).ToArray().Contains(quotationItem.ProductSmallClass.Id))
				{
					isAutoSubmit = false;
					inquiryItem.Status = InquiryStatus.WaitingForQuote;  // 报价单状态置为待报价
				}

				if (isAutoSubmit)
				{
					quotationItem.AuditStatus = QuoteOrderAuditStatus.Submitted;
					quotationItem.SubmittedBy = _mymoooContext.User?.MymoooUserId ?? 0;
					quotationItem.SubmittedOn = DateTime.Now;
					quotationItem.ResolveStatus = ResolveOrderStatus.Cancel;
				}
			}
			//}
			return quotationItem;
		}


		private static int GetDispatchDays(ConfirmQuotationItemViewModel quotationItemModel)
		{
			if (quotationItemModel.SystemDispatchDays != null &&
				quotationItemModel.SystemDispatchDays >= 0 &&
				quotationItemModel.SelectedSystemDispatchDays)
			{
				return (int)quotationItemModel.SystemDispatchDays;
			}

			if (quotationItemModel.InputDispatchDays != null &&
				quotationItemModel.InputDispatchDays >= 0)
			{
				return (int)quotationItemModel.InputDispatchDays;
			}
			return -1;
		}

		private static DateTime? HandleCalendarDate(int dispatchDays, List<FacCalculate> FactoryCalendar)
		{

			DateTime initDate = DateTime.Now.AddDays(dispatchDays);
			if (dispatchDays == -1) return null;
			// 获取不带休息日交期,在List中的索引
			int iIndex;
			List<FacCalculate> subCalendar = [];
			for (int i = 0; i < FactoryCalendar.Count; i++)
			{
				FacCalculate facCalculate = FactoryCalendar[i];
				subCalendar.Add(facCalculate);
				if (FactoryCalendar[i].FCL_DAYID != 0 && FactoryCalendar[i].FCL_Date >= initDate)
				{
					iIndex = i;
					break;
				}
			}
			int addDays = subCalendar.Where(r => r.FCL_DAYID == 0).ToList().Count;
			return DateTime.Now.AddDays(addDays + dispatchDays);
		}
		private static decimal GetUnitPriceWithTax(ConfirmQuotationItemViewModel quotationItemModel, Company? company)
		{

			if (quotationItemModel.SystemUnitPriceWithTax != null &&
				quotationItemModel.SystemUnitPriceWithTax > 0 &&
				quotationItemModel.SelectedSystemUnitPriceWithTax)
			{
				return Math.Round((decimal)quotationItemModel.SystemUnitPriceWithTax, company == null ? 4 : company.DecimalPlacesOfUnitPrice, MidpointRounding.AwayFromZero);
			}

			if (quotationItemModel.InputUnitPriceWithTax != null &&
				quotationItemModel.InputUnitPriceWithTax > 0)
			{
				return Math.Round((decimal)quotationItemModel.InputUnitPriceWithTax, company == null ? 4 : company.DecimalPlacesOfUnitPrice, MidpointRounding.AwayFromZero);
			}
			return -1;
		}

		private static InquiryStatus GetInquiryStatus(ConfirmQuotationItemViewModel quotationItemModel)
		{
			if (quotationItemModel.SystemUnitPriceWithTax != null &&
				quotationItemModel.SystemUnitPriceWithTax > 0 &&
				quotationItemModel.SelectedSystemUnitPriceWithTax &&
				quotationItemModel.ProductSmallClass != null &&
				quotationItemModel.ProductSmallClass.Id > 0 &&
				!quotationItemModel.IsTempSmallClass &&
				(quotationItemModel.Product.ProductId == 0 || quotationItemModel.Product.Published) &&
				GetDispatchDays(quotationItemModel) >= 0)
			{
				return InquiryStatus.Quoted;
			}

			if (quotationItemModel.InputUnitPriceWithTax != null &&
				quotationItemModel.InputUnitPriceWithTax > 0 &&
				quotationItemModel.ProductSmallClass != null &&
				quotationItemModel.ProductSmallClass.Id > 0 &&
				(quotationItemModel.Product.ProductId == 0 || quotationItemModel.Product.Published) &&
				GetDispatchDays(quotationItemModel) >= 0 && !quotationItemModel.IsTempSmallClass)
			{
				return InquiryStatus.Quoted;
			}
			return InquiryStatus.WaitingForQuote;
		}
		/// <summary>
		/// 处理,产品工程师逻辑.
		/// </summary>
		/// <param name="QuotationItems"></param>
		/// <returns></returns>
		private List<ProductCodeEnginer> PersistQuotationEnginer(IEnumerable<ConfirmQuotationItemViewModel> QuotationItems)
		{
			List<ProductCodeEnginer> result = [];
			var productSmallClassList = QuotationItems.Where(r => r.ProductSmallClass != null).GroupBy(r => r.ProductSmallClass?.Id).ToList();
			var productCodeListNotSmallClass = QuotationItems.Where(r => r.ProductSmallClass == null).Select(r => r.Product.Code).Distinct().ToList();
			//var allSmalls = _mymoooContext.RedisCache.ListRange<ProductSmallClass>();
			List<EngineerDept> engineerDepts = LowestPurchaseDepartmentUserList(); // 所有产品工程师的基层部门

			// 系统配置中存不了那么长,需单独设置. 
			//var tempString = _mymoooContext.RedisCache.HashGetMainValue<SystemProfile>("DefaultEngineerIds");  // 系统配置的缺省产品工程师
			//if (tempString == null) {
			//    throw new Exception("系统未配置缺省产品工程师");
			//}           
			//var tempString = "[{\"enName\":\"胡亚平\",\"enId\":\"51324\",\"enMName\":\"李亮\",\"enMId\":\"50964\",\"deptId\":\"21a3374a-cb11-47ad-b99c-ea96f4ae4115\"},{\"enName\":\"李杰春\",\"enId\":\"51435\",\"enMName\":\"刘曙\",\"enMId\":\"50965\",\"deptId\":\"3a962a05-cfb8-47fe-856a-0a8226f9f529\"},{\"enName\":\"郑凤舞\",\"enId\":\"51462\",\"enMName\":\"陈俊杰\",\"enMId\":\"51215\",\"deptId\":\"90ee0852-9409-423a-b352-fa61f0ae9842\"},{\"enName\":\"谢克伟\",\"enId\":\"51213\",\"enMName\":\"陈生红\",\"enMId\":\"51051\",\"deptId\":\"5cfe4fcb-f2c4-421e-b00b-9be0c16ca7b4\"}]";

			var tempString = _mymoooContext.SqlSugar.Queryable<BusinessParamConfig>()
			  .Where(p => p.BKey == "DefaultEngineerIds")
			  .Select(p => p.BValue)
			  .First();
			if (string.IsNullOrEmpty(tempString))
			{
				throw new Exception("系统未配置缺省产品工程师");
			}
			List<ConfigeEngineer>? configeEngineers = JsonSerializerOptionsUtils.Deserialize<List<ConfigeEngineer>>(tempString) ?? throw new Exception("系统未配置缺省产品工程师或配置错误啦");
			if (configeEngineers.Count == 0)
			{
				throw new Exception("系统未配置缺省产品工程师或配置错误了");
			}

			Random rand = new Random();
			int index = rand.Next(configeEngineers.Count);

			var selectEngineer = configeEngineers[index];
			// 有小类的,按小类匹配产品工程师和产品经理.
			var produtListWithSmallClass = QuotationItems.Where(r => r.ProductSmallClass != null).ToList();
			ConcurrentDictionary<long, ProductSmallClass?> smalls = [];
			for (int i = 0; i < produtListWithSmallClass.Count; i++)
			{
				EngineerIdAndMangerId engineerbyClassId = new EngineerIdAndMangerId();
				var curSmallClass = produtListWithSmallClass[i].ProductSmallClass;

				if (curSmallClass == null)
				{
					continue;
				}
				var item = smalls.GetOrAdd(curSmallClass.Id, _mymoooContext.RedisCache.HashGet(new ProductSmallClass() { Id = curSmallClass.Id }));

				if (item == null)
				{
					continue;
				}

				if (item.ProductEngineerId == 0)
				{
					throw new Exception(curSmallClass.Name + "[" + curSmallClass.Id + "]-该类产品工程师未配置");
				}

				engineerbyClassId.ProductEngineerId = item.ProductEngineerId;
				engineerbyClassId.ProductManagerId = item.ProductManagerId ?? 0;
				var dept = engineerDepts.Where(r => r.FUM_USER_ID == item.ProductEngineerId).FirstOrDefault();
				if (dept == null)
				{
					throw new Exception(curSmallClass.Name + "-产品工程师没找到所属部门");
				}
				else
				{
					engineerbyClassId.DepartId = dept.Id;
				}

				result.Add(IdentifyEngineer(produtListWithSmallClass[i].Product.Code, engineerbyClassId));
			}

			// 没有小类的, 按型号 匹配 产品工程师.产品经理
			var productMatchingList = GetMatchStringList(productCodeListNotSmallClass);  // 获取需要匹配的字符串数组
			for (int i = 0; i < productMatchingList.Count; i++)
			{
				// 这个里面的SQL,似乎没有办法支持,批量查询,还是要加在循环里.
				EngineerIdAndMangerId? engineers = GetEngineerByMatchedProductCode(productMatchingList[i].ProductCodeMatchList);
				if (engineers == null)
				{
					// 没有匹配到. 用系统配置缺省的. 兜底的
					EngineerIdAndMangerId engineerdefault = new EngineerIdAndMangerId();
					engineerdefault.ProductEngineerId = selectEngineer.EnId;
					engineerdefault.ProductManagerId = selectEngineer.EnMId;
					engineerdefault.DepartId = selectEngineer.DeptId;
					result.Add(IdentifyEngineer(productMatchingList[i].ProductCode, engineerdefault));
					continue;
				}
				// 匹配到了, 用匹配的产品工程师
				result.Add(IdentifyEngineer(productMatchingList[i].ProductCode, engineers));
			}
			return result;
		}


		private ProductCodeEnginer IdentifyEngineer(string productCode, EngineerIdAndMangerId engineers)
		{
			//  var allManagerUser = _mymoooContext.RedisCache.ListRange<ManagementUser>();
			ProductCodeEnginer en = new ProductCodeEnginer();
			en.ProductCode = productCode;
			//  var eu = allManagerUser.Where(r => r.UserId == engineers.ProductEngineerId && r.UserStatus == UserStatus.D).FirstOrDefault();
			var eu = _mymoooContext.RedisCache.HashGet(new ManagementUser() { UserId = engineers.ProductEngineerId }); // 通过ID,查询NAME, 从REDIS        
																													   //var em = allManagerUser.Where(r => r.UserId == engineers.ProductManagerId  && r.UserStatus == UserStatus.D).FirstOrDefault();
			var em = _mymoooContext.RedisCache.HashGet(new ManagementUser() { UserId = engineers.ProductManagerId });
			if (eu == null || em == null)
			{
				throw new Exception("产品型号[" + productCode + "]未匹配到产品工程师,可能是用户缓存过期");
			}
			en.EngineerName = eu.UserName;
			en.EngineerId = eu.UserId;
			en.EngineerManagerId = em.UserId;
			en.EngineerManagerName = em.UserName;
			en.DepartId = engineers.DepartId;
			return en;
		}


		/// <summary>
		/// 根据匹配型号,确定产品工程师和产品经理.
		/// </summary>
		/// <param name="matchedList"></param>
		/// <returns></returns>
		private EngineerIdAndMangerId? GetEngineerByMatchedProductCode(List<string> matchedList)
		{
			if (matchedList.Count == 0) return null;
			string SelectSql = @"SELECT TOP 1 m.ProductEngineerId,m.ProductManagerId,m.DepartmentId as DepartId,(SELECT dbo.FN_GetDepartmentLevel(d.Id)) AS [DepartmentLevel]
					      FROM ThirdPartyProductType AS pt 
					      INNER JOIN Department_Catalog_Mapping AS m
						      ON CAST(pt.Id AS NVARCHAR(50)) = m.CatalogId AND 
						         m.[Type] = 2
					      INNER JOIN Department AS d
					          ON d.Id = m.DepartmentId
					      WHERE pt.Value IN (@MatchedList)
					      ORDER BY LEN(pt.Value) DESC, DepartmentLevel DESC,UpdatedOn desc";

			return _mymoooContext.SqlSugar.Ado.SqlQuery<EngineerIdAndMangerId>(SelectSql,
							new
							{
								MatchedList = matchedList
							}
						 ).FirstOrDefault();
		}

		private static List<ProductCodeMatch> GetMatchStringList(IEnumerable<string> matchingList)
		{
			List<ProductCodeMatch> result = [];
			List<string> splitString = ["-", ".", "*"];
			foreach (var matchingItem in matchingList)
			{
				ProductCodeMatch Pcm = new ProductCodeMatch();
				Pcm.ProductCode = matchingItem;
				string matched = matchingItem;
				while (matched.Length > 4)     // 只检索大于4个字符的匹配.
				{
					matched = matched[..^1];
					Pcm.ProductCodeMatchList.Add(matched);
				}
				// 如果字符第3-4项,有分隔符.则继续再往前匹配2位
				if (matched.Length >= 4 && splitString.Contains(matched[3].ToString()))
				{
					matched = matched[..^1];
					Pcm.ProductCodeMatchList.Add(matched);
					if (splitString.Contains(matched[2].ToString()))
					{
						matched = matched[..^1];
						Pcm.ProductCodeMatchList.Add(matched);
					}
				}
				result.Add(Pcm);    //如果整体型号少于4项会漏掉, 不过匹配查询也没有意义.
			}
			return result;
		}


		/// <summary>
		/// 获取未来20天的工厂工作日历. 超过20天或未配置取缺省值15
		/// </summary>
		/// <returns></returns>
		private List<FacCalculate> GetFacCalculate()
		{
			string SelectSql = @"SELECT FCL_DAYID,FCL_DATE
 FROM F_CALENDAR where  FCL_DATE > GETDATE() And FCL_DATE <= DATEADD(DAY,20,GETDATE()) And FCL_FAC_ID=-1  order by FCL_DATE ASC";

			return [.. _mymoooContext.SqlSugar.Ado.SqlQuery<FacCalculate>(SelectSql)];
		}


		// 获取产品经理最基层的部门ID, 此部门是蚂蚁后台维护的部门.仅仅做权限控制.  至于为什么 是 Type == 2 . 采购部门. 因为原来就是这逻辑.
		private List<EngineerDept> LowestPurchaseDepartmentUserList()
		{
			string SelectSql = @"WITH DepartmentTree(Id, Name, ParentId, [Level]) AS 
                                      (
	                                      SELECT d.Id, 
			                                     d.Name, 
			                                     d.ParentId, 
			                                     0 AS [Level]
	                                      FROM Department AS d
	                                      WHERE d.[Type] = 2
	                                      UNION ALL

	                                      SELECT d.Id, 
			                                     d.Name, 
			                                     d.ParentId, 
			                                     [Level] + 1 AS [Level]
	                                      FROM Department AS d, DepartmentTree AS t
	                                      WHERE d.ParentId = t.Id
                                      )
                                      SELECT distinct d.Id,FUM_USER_ID,t.Level
                                      FROM DepartmentTree AS t
                                      INNER JOIN Department AS d
	                                      ON t.Id = d.Id
                                      INNER JOIN Admin_Department_Mapping AS m
	                                      ON t.Id = m.DepartmentId
                                      INNER JOIN F_USER_MSTR AS u
	                                      ON u.FUM_USER_ID = m.AdminId";

			var SearchResult = _mymoooContext.SqlSugar.Ado.SqlQuery<EngineerDept>(SelectSql).ToList();


			// 只取一个最基层的,SQL实现困难. C#再处理下.
			List<EngineerDept> result = [];
			for (int i = 0; i < SearchResult.Count; i++)
			{
				var item = result.Where(r => r.FUM_USER_ID == SearchResult[i].FUM_USER_ID).FirstOrDefault();
				if (item == null)
				{
					result.Add(SearchResult[i]);
				}
				else if (item.Level < SearchResult[i].Level && item.Level < 3)  //工程师都在2, 有 业务部是3 
				{
					result.Remove(item);
					result.Add(SearchResult[i]);
				}
			}
			return result;
		}




		private ResolvedOrder PersistNewResolvedOrder(List<InquiryQuotationOrderDetail> quotedItems, List<InquiryOrderDetail> inquiryOrderDetails,
			long inquiryOrderId, Guid allotedToDepartmentId, long ProductEngineerId)
		{
			//long InquiryItemId,
			// // 这个分解单总金额.是 所有报价项目的总金额.  
			var totalWithTax = quotedItems.Sum(item =>
			{
				var unitPriceWithTax = GetUnitPriceWithTax(item) ?? 0;
				return item.Quantity * unitPriceWithTax;
			});

			var inquiryItem = inquiryOrderDetails.Where(item => item.ProductEngineerId == ProductEngineerId).FirstOrDefault() ?? throw new Exception("分解单项目没找到");
			var resolvedOrder = new ResolvedOrder
			{
				DepartmentId = allotedToDepartmentId,
				ProductEngineerId = inquiryItem.ProductEngineerId,
				RProductEngineerName = inquiryItem.ProductEngineerName,
				ProductManagerId = inquiryItem.ProductManagerId,
				RProductManagerName = inquiryItem.ProductManagerName,
				InquiryOrderId = inquiryOrderId,
				TotalWithTax = Math.Round(totalWithTax, 4, MidpointRounding.AwayFromZero)
			};
			return resolvedOrder;
		}



		//// 写分解历史表
		private ResolvedHistory NewResolvedHistory(ResolvedOrderItem resolvedOrderItem, InquiryOrderDetail inquiryItem, InquiryOrderBAL inquiryOrder)
		{
			string companyCode = string.Empty;
			if (inquiryOrder.Company != null)
			{
				companyCode = inquiryOrder.Company.Code;
			}

			return new ResolvedHistory
			{
				AuditTime = (DateTime?)resolvedOrderItem.AuditedOn,
				CustPrdCode = resolvedOrderItem.CustomerProductCode,
				SupplierPrdCode = resolvedOrderItem.SupplierProductCode,
				PrdCode = resolvedOrderItem.ProductCode,
				PrdName = resolvedOrderItem.ProductName,
				InqNum = resolvedOrderItem.Quantity,
				Brand = "",// resolvedOrderItem.Brand?.Name,  //不存品牌了吧.反正数据库也么有什么有效值.
				SupplierName = resolvedOrderItem.SupplierName,
				SupplierUnitPrice = resolvedOrderItem.SupplierUnitPrice == null ? 0 : (decimal)resolvedOrderItem.SupplierUnitPrice,
				IsTax = true,
				DispatchDays = resolvedOrderItem.DispatchDays == null ? 0 : (int)resolvedOrderItem.DispatchDays,
				CustomerCode = companyCode,
				SaleUnitPrice = (decimal?)resolvedOrderItem.UnitPriceWithTax,
				IsTurnover = false,
				ProductSmallClassId = resolvedOrderItem.SmallClassId,
				ResolvedItemId = resolvedOrderItem.Id,   // 这里现在是没有值的
				InqId = (int)inquiryItem.InquiryDetailId,
				Remark = "系统自动报价",
				ResolvedUnitPriceWithTax = resolvedOrderItem.UnitPriceWithTax == null ? 0 : (decimal)resolvedOrderItem.UnitPriceWithTax,
			};
		}


		//分解单项目
		private ResolvedOrderItem NewResolvedItem(InquiryOrderDetail inquiryItem, InquiryQuotationOrderDetail quotedItem,
			ResolvedOrder resolvedOrder)
		{

			QuoteOrderAuditStatus auditStatus = quotedItem.AuditStatus == (byte)QuoteOrderAuditStatus.Submitted ? QuoteOrderAuditStatus.Cancel : QuoteOrderAuditStatus.IsDraft;
			int isAutoQuote = quotedItem.AuditStatus == (byte)QuoteOrderAuditStatus.Submitted ? 1 : 0;
			decimal? supplierPrice = null;
			string supplierCode = "";
			string supplierName = "";
			long? smallClassId = quotedItem.SmallClassId;
			decimal? nAmount = null;    //产品小类超过此金额需要审核. 



			if (smallClassId != null)
			{
				nAmount = _mymoooContext.RedisCache.HashGet(new ProductSmallClass() { Id = smallClassId.Value }, p => p.Amount); // 从REDIS中获取  

				// 如果属于,型材,非标,不自动审核.
				// 13个非标小类铝型材；铝型材框架；功能组件；CNC产线；原材料贸易；工业原材料               
				string? tempString = _mymoooContext.RedisCache.HashGet(new SystemProfile() { Key = "NotAutoQuoteAuditSmallClassId" }, p => p.Value);
				if (string.IsNullOrEmpty(tempString))
				{
					tempString = "89";  //容错代码 非标轴.
				}

				if (tempString.Split(',').Select(s => long.Parse(s)).ToArray().Contains(smallClassId.Value))
				{
					isAutoQuote = 0;
				}
			}

			return new ResolvedOrderItem
			{
				ProductId = inquiryItem.ProductId,
				ProductTypeId = inquiryItem.ProductTypeId,
				ParentId = 0,
				IsHistory = inquiryItem.IsHistory,
				ProductCode = inquiryItem.ProductNumber,
				ProductName = inquiryItem.ProductName,
				CustomerProductCode = inquiryItem.CustomerProductNumber,
				SupplierUnitPriceWhetherIncludeTax = true,
				SupplierUnitPrice = supplierPrice,
				SupplierCode = supplierCode,
				SupplierName = supplierName,
				AskPrice = quotedItem.AskPrice,
				ProfitRate = 0.1M,   // 分解单利润率 原常量传参,去掉了
				DispatchDays = quotedItem.DeliveDays < 0 ? (int?)
					null : (int)quotedItem.DeliveDays,
				Status = (byte)inquiryItem.Status,  // 待校验这个状态值对不对
				UnitPriceWithTax = quotedItem.TaxPrice,
				OriginalUnitPriceWithTax = quotedItem.OriginalUnitPriceWithTax,
				QtyDiscount = quotedItem.QtyDiscount,
				LevelDiscount = quotedItem.LevelDiscount,
				ResolvedOrderNumber = resolvedOrder.OrderNumber, //这个是 要赋值,分解单主表 单号             
				InquiryItemId = inquiryItem.InquiryDetailId,
				AuditStatus = (byte)auditStatus,
				BrandId = inquiryItem.BrandId,
				InsideRemark = quotedItem.InsideRemark,
				Remark = quotedItem.Memo,
				AttachmentUrl = quotedItem.AttachmentUrl,
				Quantity = quotedItem.Quantity,
				CategoryType = (byte)quotedItem.CategoryType,
				IsAutoQuote = isAutoQuote,
				AuditedBy = isAutoQuote != 1 ? (long?)null : inquiryItem.ProductEngineerId,
				AuditedOn = isAutoQuote != 1 ? (DateTime?)null : DateTime.Now,
				SubmittedBy = isAutoQuote != 1 ? (long?)null : inquiryItem.ProductEngineerId,   //自动报价,提交人为产品工程师
				SubmittedUserName = isAutoQuote != 1 ? string.Empty : inquiryItem.ProductEngineerName,
				SubmittedOn = isAutoQuote != 1 ? (DateTime?)null : DateTime.Now,
				SmallClassId = quotedItem.SmallClassId,
				LargeClassId = quotedItem.LargeClassId,
				BusinessDivisionId = inquiryItem.BusinessDivisionId,
				BusinessDivisionName = inquiryItem.BusinessDivisionName,
				SupplyOrgId = inquiryItem.SupplyOrgId,
				SupplyOrgName = inquiryItem.SupplyOrgName,
				PriceSource = quotedItem.PriceSource == null ? null : (PriceSource)quotedItem.PriceSource,
				DeliverySource = quotedItem.DeliverySource == null ? null : (DeliverySource)quotedItem.DeliverySource,
				QuotationLowestPrice = quotedItem.FQD_QUO_LPRICE,
				DeliverySubmitBy = quotedItem.DeliverySubmitBy,
				DealLowestPrice = quotedItem.FQD_DEAL_LPRICE,
				DesireDeliveryDays = quotedItem.DesireDeliveryDays,
				DesirePrice = quotedItem.DesirePrice,
				Amount = nAmount
			};
		}


		private static InquiryStatus GetInquiryStatus(QuotedItem quotedItem)
		{
			if (quotedItem.DispatchDays < 0)
			{
				return InquiryStatus.WaitingForQuote;
			}

			if (quotedItem.InquiryItem.Status == InquiryStatus.WaitingForQuote)
			{
				return InquiryStatus.WaitingForQuote;
			}

			if (!quotedItem.AskPrice)
			{
				return InquiryStatus.Quoted;
			}

			if (quotedItem.FirstCost > 0)
			{
				return InquiryStatus.Quoted;
			}

			if (quotedItem.OriginalUnitPriceWithTax > 0)
			{
				return InquiryStatus.Quoted;
			}

			if (quotedItem.UnitPriceWithTax > 0)
			{
				return InquiryStatus.Quoted;
			}

			return InquiryStatus.WaitingForQuote;
		}


		private decimal? GetUnitPriceWithTax(InquiryQuotationOrderDetail quotedItem)
		{
			if (quotedItem.FirstCost > 0)
			{
				return quotedItem.FirstCost;
			}

			if (quotedItem.OriginalUnitPriceWithTax > 0)
			{
				return quotedItem.OriginalUnitPriceWithTax;
			}

			if (quotedItem.BeforeWholeDiscountUnitPriceWithTax > 0)
			{
				return quotedItem.BeforeWholeDiscountUnitPriceWithTax;
			}

			return null;
		}
	}
}