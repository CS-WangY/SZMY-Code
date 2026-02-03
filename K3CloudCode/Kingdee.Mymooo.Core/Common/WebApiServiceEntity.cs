using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.Common
{
    /// <summary>
    /// 接口返回值
    /// </summary>
    public class ResponseStatus
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<Errors> Errors { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<SuccessEntitys> SuccessEntitys { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> SuccessMessages { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int MsgCode { get; set; }
    }

    public class Result
    {
        /// <summary>
        /// 
        /// </summary>
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class WebApiServiceEntity
    {
        /// <summary>
        /// 
        /// </summary>
        public Result Result { get; set; }
    }

    public class Errors
    {
        /// <summary>
        /// 
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int DIndex { get; set; }
    }
    public class SuccessEntitys
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int DIndex { get; set; }
    }
}
