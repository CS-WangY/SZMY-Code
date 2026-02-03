using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.core.Model.SqlSugarCore;
using mymooo.k3cloud.core.Account;
using System.Net.Mime;

namespace mymooo.k3cloud.Controllers
{
    /// <summary>
    /// MQ相关
    /// </summary>
    /// <param name="kingdeeContent">上下文</param>
    [Route("[controller]/[action]")]
    [ApiController]
    public class RabbitMQController(KingdeeContent kingdeeContent) : Controller
    {
        private readonly KingdeeContent _kingdeeContent = kingdeeContent;

        /// <summary>
        /// 获取MessageId
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseMessage<List<long>>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetMessageId(int count = 1)
        {
            ResponseMessage<List<long>> response = new() { Data = [] };
            for (int i = 0; i < count; i++)
            {
                response.Data.Add(_kingdeeContent.GatewayRedisCache.StringIncrement<RabbitMQMessage>());
            }

            return Json(response);
        }

        /// <summary>
        /// 发送mq消息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult SendMqMessage(CancellationToken cancellationToken = default)
        {
            _kingdeeContent.RabbitMQService.SendMessage(cancellationToken);
            return Ok();
        }
    }
}
