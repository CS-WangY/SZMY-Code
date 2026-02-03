using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.core.Model.BussinessModel.K3Cloud;
using mymooo.core.Utils;
using mymooo.k3cloud.business.Services.Purchase;
using mymooo.k3cloud.business.Services.Sales;
using mymooo.k3cloud.core.Account;
using mymooo.k3cloud.core.Approver;
using mymooo.k3cloud.core.PurchaseModel;
using mymooo.weixinWork.SDK.AddressBook;
using mymooo.weixinWork.SDK.AddressBook.Model;
using mymooo.weixinWork.SDK.Approval;
using mymooo.weixinWork.SDK.Approval.Model;
using mymooo.weixinWork.SDK.WeixinWorkMessage.Model;
using Newtonsoft.Json.Linq;
using System.Net.Mime;

namespace mymooo.k3cloud.Controllers
{
    /// <summary>
    /// 企业微信SDK调用
    /// </summary>
    [Route("[controller]/[action]")]
    [ApiController]
    public class AddressBookController(AddressBookServiceClient addressBookServiceClient) : Controller
    {
        private readonly AddressBookServiceClient _addressBookServiceClient = addressBookServiceClient;

        /// <summary>
        /// 获取第几级父级用户信息
        /// </summary>
        /// <param name="usercode"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseMessage<ParentUserInfo>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetLeveParent(string usercode,int level)
        {
            var request = _addressBookServiceClient.GetLevelParent(usercode,level);
            return Json(request);
        }
    }
}
