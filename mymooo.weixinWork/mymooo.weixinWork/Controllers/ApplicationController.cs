using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.core.Account;
using mymooo.weixinWork.Models;
using mymooo.weixinWork.SDK;
using mymooo.weixinWork.SDK.Application;
using mymooo.weixinWork.SDK.Application.Model.ExternalContact;
using System.Net.Mime;

namespace mymooo.weixinWork.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ApplicationController(ApplicationServiceClient applicationServiceClient, WeixinWorkContext weixinWorkContext) : Controller
    {
        /// <summary>
        /// 获取企业已配置的「联系我」列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<ExternalContactListResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> GetExternalContactList([FromBody] ExternalContactListRequest request)
        {
            return Json(await applicationServiceClient.GetExternalContactList<WeixinWorkContext, User>(weixinWorkContext, request));
        }

        /// <summary>
        /// 获取企业已配置的「联系我」方式
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<ExternalContactDetailResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> GetExternalContactDetail([FromBody] ExternalContactListResponse.ExternalContactId request)
        {
            return Json(await applicationServiceClient.GetExternalContactDetail<WeixinWorkContext, User>(weixinWorkContext, request));
        }

        /// <summary>
        /// 更新企业已配置的「联系我」方式
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<WeiXinMessageResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> UpdateExternalContactDetail([FromBody] ExternalContactDetailResponse.ExternalContactDetail request)
        {
            return Json(await applicationServiceClient.UpdateExternalContactDetail<WeixinWorkContext, User>(weixinWorkContext, request));
        }
    }
}
