using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.Common
{
    public class MessageHelp
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 返回消息
        /// </summary>
        public string Message { get; set; }
        public object Data { get; set; }
    }
    public class MessageHelp<T>
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 返回消息
        /// </summary>
        public string Message { get; set; }
        public T Data { get; set; }
    }


    public class MessageHelpCloud
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool isSuccess { get; set; }
        /// <summary>
        /// 结果代码 200正确 888业务错误
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 消息，如果处理错误此条为对应的错误原因
        /// </summary>
        public string msg { get; set; }
    }

    public class MessageHelpMes
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 返回消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 返回错误消息
        /// </summary>
        public string errorMessage { get; set; }
        public string code { get; set; }
        public string result { get; set; }
        public string timestamp { get; set; }
    }

    public class MessageHelpProcessResultReturn
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool success { get; set; }
        /// <summary>
        /// 返回code（结果代码 200正确 888业务错误）
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 返回消息，如果处理错误此条为对应的错误原因
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 返回data
        /// </summary>
        public string data { get; set; }
    }

    /// <summary>
    /// 分页请求数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageResqust<T>
    {
        private int _pageIndex;
        private int _pageSize;

        /// <summary>
        /// 当前页,控制当前页不能小于1
        /// </summary>
        public int PageIndex
        {
            set
            {
                _pageIndex = value;
            }
            get
            {
                if (_pageIndex < 1)
                {
                    return 1;
                }
                return _pageIndex;
            }
        }

        /// <summary>
        /// 每页大小 每页大小不能小于1 默认50
        /// </summary>
        public int PageSize
        {
            set
            {
                _pageSize = value;
            }
            get
            {
                if (_pageSize < 1)
                {
                    return 50;
                }
                return _pageSize;
            }
        }

        /// <summary>
        /// 过滤条件
        /// </summary>
        public T Filter { get; set; }
    }

    /// <summary>
    /// 分页返回数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageReponse<T>
    {
        public List<T> Data { get; set; }

        public int Count { get; set; }
    }
}
