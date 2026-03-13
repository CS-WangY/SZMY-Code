using com.mymooo.mall.business.Service;
using com.mymooo.mall.business.Service.BaseService;
using com.mymooo.mall.business.Service.SalesService;
using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Model.Cache;
using com.mymooo.mall.core.Model.Quotation;
using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.core.Account;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace com.mymooo.mall.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userService"></param>
    /// <param name="mallContext"></param>
	public class LogUserActionController(UserService userService, MallContext mallContext) : Controller
	{
		private readonly UserService _userService = userService;
        private readonly MallContext _mymoooContext = mallContext;




        [HttpPost]
        public ResponseMessage<string> Log([FromBody] SysLogReq req)
        {
            ResponseMessage<string> result = new();
            if (req == null)
            {
                return result;
            }

            req.Ip = string.IsNullOrEmpty(_mymoooContext.Ip) ? "" : _mymoooContext.Ip;
            if (_mymoooContext.User != null)
            {
                req.UserName = _mymoooContext.User.Name;
                req.UserId = _mymoooContext.User.UserId;
                req.HostName = _mymoooContext.HostName;
            }

            req.MainParam = req.MainParam.Length > 1000 ? req.MainParam.Substring(req.MainParam.Length - 1000) : req.MainParam;   // 最多只存1000个字符

            req.NTime  = (decimal)Math.Round((req.EndDate - req.StartDate).TotalSeconds,3);  //精确到小数点后三位

            result.Data = _userService.SysLog(req);
            result.Code = ResponseCode.Success;
            return result;
        }


    }
}
