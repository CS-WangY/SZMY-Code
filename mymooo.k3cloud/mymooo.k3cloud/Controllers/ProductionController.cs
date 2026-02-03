using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.k3cloud.business.Services.Production;
using mymooo.k3cloud.business.Services.Stock;
using mymooo.k3cloud.core.ProductionModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace mymooo.k3cloud.Controllers
{
    /// <summary>
    /// 生产管理 控制器
    /// </summary>
    [Route("[controller]/[action]")]
    [ApiController]
    public class ProductionController(ProductionService productionService) : Controller
    {
        /// <summary>
        /// 同步轮项目 发送mes
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<MakeDispatchRequest>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> SendMakeDispatch([FromBody, Required] MakeDispatchRequest request)
        {
            return Json(await productionService.SendMakeDispatch(request));
        }
		/// <summary>
		/// mes获取原材料单价
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost]
		[ProducesResponseType(typeof(ResponseMessage<List<MesRawMaterialsRequest>>), StatusCodes.Status200OK)]
		[Produces(MediaTypeNames.Application.Json)]
		public IActionResult MesRawMaterialsPrice([FromBody, Required] List<MesRawMaterialsRequest> request)
		{
			return Json(productionService.GetMesRawMaterialsPrice(request));
		}
	}
}
