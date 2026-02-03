using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.k3cloud.business.Services.Material;
using mymooo.k3cloud.business.Services.Sales;
using mymooo.k3cloud.core.MaterialModel;
using mymooo.k3cloud.core.Sales;

namespace mymooo.k3cloud.Controllers
{
	/// <summary>
	/// 物料服务
	/// </summary>
	[Route("[controller]/[action]")]
	[ApiController]
	public class MaterialController(MaterialServices materialService) : Controller
	{
		/// <summary>
		/// 物料更新
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost]
		[ProducesResponseType(typeof(ResponseMessage<MaterialInfoRequest>), StatusCodes.Status200OK)]
		[Produces(MediaTypeNames.Application.Json)]
		public IActionResult UpdateMaterials(MaterialInfoRequest request)
		{
			return Json(materialService.UpdateMaterials(request));
		}
	}
}
