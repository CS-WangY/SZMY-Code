using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.k3cloud.business.Services.Production;
using mymooo.k3cloud.business.Services.Sub_SubreqOrder;
using mymooo.k3cloud.core.ProductionModel;
using mymooo.k3cloud.core.SubReqModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace mymooo.k3cloud.Controllers
{
    /// <summary>
    /// 委外管理 控制器
    /// </summary>
    [Route("[controller]/[action]")]
    [ApiController]
    public class Sub_SubreqController(Sub_SubreqService subSubreqService) : Controller
    {
        /// <summary>
        /// 轴承座项目 发送mes
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<SubReqOrderRequests>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> SendMakeDispatch([FromBody, Required] SubReqOrderRequests request)
        {
            return Json(await subSubreqService.SendMakeDispatch(request));
        }
    }
}
