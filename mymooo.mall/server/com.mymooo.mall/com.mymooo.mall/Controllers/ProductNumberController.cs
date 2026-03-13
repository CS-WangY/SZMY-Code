using com.mymooo.mall.business.Service.BaseService;
using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Model.Cache;
using com.mymooo.mall.core.Model.Product;
using com.mymooo.mall.core.Model.Stock;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation;
using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.k3cloud.core.Stock;
using mymooo.k3cloud.SDK.Inventory;

namespace com.mymooo.mall.Controllers
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="productService"></param>
	/// <param name="mymoooContext"></param>
	/// <param name="inventoryService"></param>
	public class ProductNumberController(ProductService productService, MallContext mymoooContext, InventoryServiceClient inventoryService) : BaseController
	{
		private readonly ProductService _productService = productService;
		private readonly MallContext _mymoooContext = mymoooContext;
		private readonly InventoryServiceClient _inventoryService = inventoryService;

		/// <summary>
		/// 重新加载缓存
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public async Task<IActionResult> ReloadCache([FromBody] List<ProductNumberIndex> request)
		{
			if (request == null)
			{
				ResponseMessage<dynamic> response = new()
				{
					Code = ResponseCode.Warning,
					ErrorMessage = "入参为空"
				};
				return Json(response);
			}
			var result = await this.ModelVerify(request);
			if (!result.IsSuccess)
			{
				result.Code = ResponseCode.Warning;
				return Json(result);
			}
			return Json(await _productService.ReloadCache(request));
		}

		/// <summary>
		/// 获取物料缓存数据测试
		/// </summary>
		/// <returns></returns>
		public IActionResult GetMaterialInventory()
		{
			var s1 = _inventoryService.GetInventory("ctbr-d5-l8");
			var s2 = _inventoryService.GetInventoryTotal("ctbr-d5-l8");
			var s3 = _inventoryService.GetInventory("ctbr-d5-l8", "hnth");
			var s4 = _inventoryService.GetInventoryTotal("ctbr-d5-l8", "hnth");
			var s5 = _inventoryService.GetInventory(new InventoryInfo() { MaterialNumber = "ctbr-d5-l8", StockOrgNumber = "hnth", StockNumber = "xjpj" });
			var s6 = _inventoryService.GetInventoryTotal(new InventoryInfo() { MaterialNumber = "ctbr-d5-l8", StockOrgNumber = "hnth", StockNumber = "xjpj" });
			return Ok("ok");
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="stockReq"></param>
		/// <returns></returns>
		public IActionResult GetStockSummary([FromBody] List<SupplyOrgStockQueryReq> stockReq)
        {
            List<SupplyOrgStockReponse> Itemstock = new List<SupplyOrgStockReponse>();

			if (stockReq == null)
			{
                return ReturnJson(Itemstock);
            }

            // 前端没有供货组织代码,得再查一下。没几条记录
            var supplyOrgs = _mymoooContext.SqlSugar.Queryable<BusinessDivisionSupplyOrg>()
				                  .Select(r=>new {r.SupplyOrgId,r.SupplyOrgNumber}).ToList();

			stockReq.ForEach(r =>
			{
				if (r.OrgId != 0 )
				{
					string orgNumber = supplyOrgs.Where(so => so.SupplyOrgId == r.OrgId).First().SupplyOrgNumber;
					var stockData = _inventoryService.GetInventoryTotal(r.ProductCode, orgNumber);
					SupplyOrgStockReponse m = new SupplyOrgStockReponse();
					OrgStockInfo s = new OrgStockInfo();
					m.ItemNo = r.ProductCode.ToUpper();
					//可用库存
					s.UsableQty = FormatStockNumber(stockData?.UsableQty ?? 0);
					//待出库量
					s.UnQtyShipdSum = FormatStockNumber(stockData?.UnQtyShipdSum ?? 0);
					// 采购在途
					s.OnOrderQty = FormatStockNumber(stockData?.OnOrderQty ?? 0);
					// 品检数量
					s.QtyInsp = FormatStockNumber(stockData?.InspQty ?? 0);
					m.StockInfo = s;
					Itemstock.Add(m);
				}
			});
			return ReturnJson(Itemstock);
        }


		/// <summary>
		///  这个是各供货组织,仓,所有都列出来
		/// </summary>
		/// <param name="ProductReq"></param>
		/// <returns></returns>
		public IActionResult GetStockInfo([FromBody] List<string> ProductReq)
        {
            List<SupplyOrgStockReponse> Itemstock = new List<SupplyOrgStockReponse>();

            if (ProductReq == null)
            {
                return ReturnJson(Itemstock);
            }


//            [
//  {
//                "fMaterialId": 11509753,
//    "fOrgId": 224428,
//    "orgNum": "SZMYGC",
//    "orGName": "深圳蚂蚁工场科技有限公司",
//    "stoId": 7223622,
//    "stoNum": null,
//    "stoName": "大岭山外发仓",
//    "materialNum": "TEST-B",
//    "materialName": "TEST-B",
//    "fBaseQty": 0,
//    "fAvbQty": 0,
//    "fAvbQtyL": 0,
//    "fLockQty": 0,
//    "unitNum": "Pcs",
//    "uNitName": "个",
//    "fIsOutSourceStock": "1",
//    "unQtyShipdSum": 60.0000000000,
//    "usableQty": 0.0,
//    "onOrderQTY": 0.0,
//    "qtyInsp": 0.0,
//    "fOutSourceStockLoc": "大岭山"
//  }
//]

            //// 前端没有供货组织代码,得再查一下。没几条记录
            //var supplyOrgs = _mymoooContext.SqlSugar.Queryable<BusinessDivisionSupplyOrg>()
            //                      .Select(r => new { r.SupplyOrgId, r.SupplyOrgNumber }).ToList();

            //ProductReq.ForEach(r =>
            //{
            //    if (r.OrgId != 0)
            //    {
            //        string orgNumber = supplyOrgs.Where(so => so.SupplyOrgId == r.OrgId).First().SupplyOrgNumber;
            //        var stockData = _inventoryService.GetInventoryTotal(r.ProductCode, orgNumber);
            //        SupplyOrgStockReponse m = new SupplyOrgStockReponse();
            //        OrgStockInfo s = new OrgStockInfo();
            //        m.ItemNo = r.ProductCode.ToUpper();
            //        //可用库存
            //        s.UsableQty = FormatStockNumber(stockData?.UsableQty ?? 0);
            //        //待出库量
            //        s.UnQtyShipdSum = FormatStockNumber(stockData?.UnQtyShipdSum ?? 0);
            //        // 采购在途
            //        s.OnOrderQty = FormatStockNumber(stockData?.OnOrderQty ?? 0);
            //        // 品检数量
            //        s.QtyInsp = FormatStockNumber(stockData?.InspQty ?? 0);
            //        m.StockInfo = s;
            //        Itemstock.Add(m);
            //    }
            //});
            return ReturnJson(Itemstock);
        }

        private decimal FormatStockNumber(decimal num)
        {
			decimal r;
			if (Math.Round(num * 100, 0, MidpointRounding.AwayFromZero) == num * 100)
			{
				r = Math.Round(num, 0, MidpointRounding.AwayFromZero);
            }
			else
			{
                r = Math.Round(num, 2, MidpointRounding.AwayFromZero);
            }
            return r;
		}

		/// <summary>
		/// 获取产品参数
		/// </summary>
		/// <param name="productId"></param>
		/// <param name="number"></param>
		/// <param name="supplierProducts"></param>
		/// <returns></returns>
        public IActionResult GetParameterValue(long productId, string number, [FromBody] List<ShortNumberselectionRequest> supplierProducts)
		{
			ResponseMessage<dynamic> response = new();
			if (string.IsNullOrWhiteSpace(number))
			{
				response.Code = ResponseCode.ModelError;
				response.ErrorMessage = "产品型号不能为空!";
				return Json(response);
			}
			return Json(_productService.GetParameterValue(productId, number, supplierProducts ?? []));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pageNumber"></param>
		/// <param name="productNumber"></param>
		/// <param name="companyCode"></param>
		/// <param name="qty"></param>
		/// <returns></returns>
		public async Task<IActionResult> Test(int pageNumber = 2, string productNumber = "", string? companyCode = null, int qty = 1)
		{
			return Json(await _productService.Test(pageNumber, productNumber, companyCode, qty));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		public IActionResult DeleteProductNumber(string number)
		{
			return Json(_mymoooContext.ProductNumberCache.Delete(number));
		}

		/// <summary>
		/// 删除产品缓存
		/// </summary>
		/// <param name="productId"></param>
		/// <returns></returns>
		public IActionResult DeleteProductId(long productId)
		{
			return Json(_mymoooContext.ProductNumberCache.Delete(productId));
		}

		/// <summary>
		/// 重新执行缓存
		/// </summary>
		/// <returns></returns>
		public IActionResult ReloadExecuteCache()
		{
			_productService.ReloadExecuteCache();
			return Ok("ok");
		}
	}
}
