using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.k3cloud.business.Services;
using mymooo.k3cloud.core.Inventory;
using mymooo.weixinWork.SDK.Approval.Model;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace mymooo.k3cloud.Controllers
{
    /// <summary>
    /// 库存服务
    /// </summary>
    [Route("[controller]/[action]")]
    [ApiController]
    public class InventoryController(InventoryService inventoryService) : Controller
    {
        private readonly InventoryService _inventoryService = inventoryService;

        /// <summary>
        /// 更新库存缓存
        /// </summary>
        /// <param name="inventory"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<dynamic>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult UpdateInventory([FromBody, Required] InventoryBillInfo inventory)
        {
            return Json(_inventoryService.UpdateInventory(inventory));
        }
    }
}
