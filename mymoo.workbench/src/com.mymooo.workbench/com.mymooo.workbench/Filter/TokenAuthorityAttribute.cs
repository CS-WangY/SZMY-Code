using com.mymooo.workbench.business.Account;
using com.mymooo.workbench.core.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using mymooo.core;
using mymooo.core.Attributes;

namespace com.mymooo.workbench.Filter
{
	/// <summary>
	/// 
	/// </summary>
    [AutoInject(InJectType.Scope)]
	public class TokenAuthorityAttribute(TokenService tokenService, WorkbenchContext workbenchContext) : ActionFilterAttribute
    {
        private readonly TokenService _tokenService = tokenService;
        private readonly WorkbenchContext _workbenchContext = workbenchContext;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		public override void OnActionExecuting(ActionExecutingContext context)
        {
			ResponseMessage<object> message = new ResponseMessage<object>();
            if (context.HttpContext.User == null || context.HttpContext.User.Identity == null || string.IsNullOrWhiteSpace(context.HttpContext.User.Identity.Name))
            {
                message.Code = ResponseCode.NoLogin;
                message.ErrorMessage = "用户未登录";
                context.Result = new JsonResult(message);
                return;
            }
            string path = context.HttpContext.Request.Path.Value;
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }
            var result = _tokenService.MenuItemPrivilege(_workbenchContext.User, path);
            if (result.Code == ResponseCode.Success)
            {
                return;
            }
            else
            {
                message.Code = result.Code;
                message.ErrorMessage = result.ErrorMessage;
                context.Result = new JsonResult(message);
                return;
            }
            

            //string meunId = context.HttpContext.Request.Headers["appId"];
            //if (!string.IsNullOrWhiteSpace(meunId))
            //{
            //    message = _tokenService.MenuItemPrivilege(context.HttpContext.CurrentUser(), Convert.ToInt64(meunId), context.HttpContext.Request.Path.Value);
            //    if (message.Code != ResponseCode.Success)
            //    {
            //        context.Result = new JsonResult(message);
            //    }
            //}
        }
    }
}
