using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.core.Account;
using mymooo.k3cloud.core.Account;
using mymooo.weixinWork.SDK.Application;
using mymooo.weixinWork.SDK.Application.Model;
using mymooo.weixinWork.SDK.WeixinWorkMessage.Model;
using System.Net.Mime;

namespace mymooo.k3cloud.Controllers
{
    /// <summary>
    /// 企业微信应用
    /// </summary>
    /// <param name="applicationServiceClient"></param>
    /// <param name="kingdeeContent"></param>
    [Route("[controller]/[action]")]
    [ApiController]
    public class ApplicationController(ApplicationServiceClient applicationServiceClient, KingdeeContent kingdeeContent) : Controller
    {
        private readonly ApplicationServiceClient _applicationServiceClient = applicationServiceClient;
        private readonly KingdeeContent _kingdeeContent = kingdeeContent;

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<SendMessageResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> SendTextMessage([FromBody] SendTextMessageRequest request)
        {
            return Json(await _applicationServiceClient.SendTextMessage<KingdeeContent, User>(_kingdeeContent, request));
        }

        /// <summary>
        /// 发送 Markdown 消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<SendMessageResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> SendMarkdownMessage([FromBody] SendMarkdownMessageRequest request)
        {
            return Json(await _applicationServiceClient.SendMarkDownMessage<KingdeeContent, User>(_kingdeeContent, request));
        }

        /// <summary>
        /// 发送 卡片 消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<SendMessageResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> SendtextcardMessage([FromBody] SendTextCardMessageRequest request)
        {
            return Json(await _applicationServiceClient.SendTextcardMessage<KingdeeContent, User>(_kingdeeContent, request));
        }

        /// <summary>
        /// 创建群组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<SendMessageResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request)
        {
            return Json(await _applicationServiceClient.CreateChat<KingdeeContent, User>(_kingdeeContent, request));
        }
        /// <summary>
        /// 获取群组信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseMessage<SendMessageResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> GetChat(string request)
        {
            return Json(await _applicationServiceClient.GetChat<KingdeeContent, User>(_kingdeeContent, request));
        }
        /// <summary>
        /// 发送群组 Markdown 消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<SendMessageResponse>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> SendChatMarkDownMessage([FromBody] SendMarkdownMessageRequest request)
        {
            return Json(await _applicationServiceClient.SendChatMarkDownMessage<KingdeeContent, User>(_kingdeeContent, request));
        }
    }
}
