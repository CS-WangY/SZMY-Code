using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core
{

    /// <summary>
    /// 响应消息
    /// </summary>
    public class ResponseMessage<T>
    {
        /// <summary>
        /// code码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 返回消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 返回数据包
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess
        {
            get
            {
                return Code == ResponseCode.Success;
            }
        }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    public class ResponseCloudWarehouseMessage
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// 云仓储错误消息
    /// </summary>
    public class ResponseCloudWarehouseErrMessage
    {
        public List<ResponseCloudWarehouseErrDec> errors { get; set; }
    }
    /// <summary>
    /// 错误明细
    /// </summary>
    public class ResponseCloudWarehouseErrDec
    {
        public string field { get; set; }
        public string message { get; set; }
    }

    /// <summary>
    /// 授权用户
    /// </summary>
    public class AuthorityRoles
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public class AuthorityUser
    {
        public string UserCode { get; set; }
        public long UserId { get; set; }
    }
}
