using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.k3cloud.business.Services.Sales;
using mymooo.k3cloud.business.Services.Stock;
using mymooo.k3cloud.core.Inventory;
using mymooo.k3cloud.core.StockModel;
using mymooo.weixinWork.SDK.WeixinWorkMessage.Model;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace mymooo.k3cloud.Controllers
{
	/// <summary>
	/// 仓库应用
	/// </summary>
	[Route("[controller]/[action]")]
	[ApiController]
	public class StockController(StockService stockService) : Controller
	{
		private readonly StockService _stockService = stockService;
		/// <summary>
		/// MES根据型材，材质，长宽高模糊查询总库存
		/// </summary>
		[HttpPost]
		[ProducesResponseType(typeof(ResponseMessage<dynamic>), StatusCodes.Status200OK)]
		[Produces(MediaTypeNames.Application.Json)]
		public IActionResult MesFuzzyQueryStockBaseQty([FromBody, Required] FuzzyQueryStockBaseQtyRequest request)
		{
			return Json(_stockService.GetMesFuzzyQueryStockBaseQty(request));
		}

		/// <summary>
		/// 获取即时库存(MES专用)
		/// </summary>
		[HttpPost]
		[ProducesResponseType(typeof(ResponseMessage<dynamic>), StatusCodes.Status200OK)]
		[Produces(MediaTypeNames.Application.Json)]
		public IActionResult MesStockQuantity([FromBody, Required] List<string> itemNos)
		{
			return Json(_stockService.GetMesStockQuantity(itemNos));
		}
	}
}
