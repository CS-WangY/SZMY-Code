using com.mymooo.workbench.core.Account;
using com.mymooo.workbench.ef;
using com.mymooo.workbench.Filter;
using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.core.Account;
using System;
using System.Linq;

namespace com.mymooo.workbench.Controllers
{
	/// <summary>
	/// 岗位管理
	/// </summary>
	[ServiceFilter(typeof(TokenAuthorityAttribute))]
    public class PositionManageController(WorkbenchDbContext workbenchDbContext, WorkbenchContext workbenchContext) : Controller
    {
        private readonly WorkbenchDbContext _workbenchDbContext = workbenchDbContext;
        private readonly WorkbenchContext _workbenchContext = workbenchContext;

		/// <summary>
		/// 获取岗位列表
		/// </summary>
		/// <returns></returns>
		public IActionResult GetPositionList()
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            var query = _workbenchDbContext.Position.ToList();
            message.Data = query;
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 新增岗位
        /// </summary>
        /// <param name="Position"></param>
        /// <returns></returns>
        public IActionResult InsertPosition([FromBody] ef.AccountContext.Position Position)
        {
            ResponseMessage<dynamic> message = new();
            if (this.TryValidateModel(Position))
            {
                if (!_workbenchContext.User.IsAdmin)
                {
                    message.Code = ResponseCode.PrivilegeReject;
                    message.ErrorMessage = "当前用户没有权限!";
                    return Json(message);
                }
                if (string.IsNullOrWhiteSpace(Position.Code))
                {
                    message.ErrorMessage = "岗位编码不能为空!";
                    return Json(message);
                }
                if (string.IsNullOrWhiteSpace(Position.Name))
                {
                    message.ErrorMessage = "岗位名称不能为空!";
                    return Json(message);
                }
                User user = _workbenchContext.User;
                Position.CreateUser = user.Code;
                Position.CreateDate = DateTime.Now;
                _workbenchDbContext.Position.Add(Position);
                _workbenchDbContext.SaveChanges();
                message.Data = Position;
                message.Code = ResponseCode.Success;
            }
            else
            {
                message.Code = ResponseCode.ModelError;
                message.ErrorMessage = string.Join("\n", ModelState.Values.Select(p => string.Join(";", p.Errors.Select(c => c.ErrorMessage))));
            }
            return Json(message);
        }

        /// <summary>
        /// 修改岗位
        /// </summary>
        /// <param name="Position"></param>
        /// <returns></returns>
        public IActionResult UpdatePosition([FromBody] ef.AccountContext.Position Position)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (this.TryValidateModel(Position))            {
                if (!_workbenchContext.User.IsAdmin)
                {
                    message.Code = ResponseCode.PrivilegeReject;
                    message.ErrorMessage = "当前用户没有权限!";
                    return Json(message);
                }
                if (string.IsNullOrWhiteSpace(Position.Code))
                {
                    message.ErrorMessage = "岗位编码不能为空!";
                    return Json(message);
                }
                if (string.IsNullOrWhiteSpace(Position.Name))
                {
                    message.ErrorMessage = "岗位名称不能为空!";
                    return Json(message);
                }
                var old = _workbenchDbContext.Position.FirstOrDefault(p => p.Id == Position.Id);
                old.Code = Position.Code;
                old.Name = Position.Name;
                old.IsAssistant = Position.IsAssistant;
                old.Description = Position.Description;
                Position = old;
                _workbenchDbContext.SaveChanges();
                message.Data = Position;
                message.Code = ResponseCode.Success;
            }
            else
            {
                message.Code = ResponseCode.ModelError;
                message.ErrorMessage = string.Join("\n", ModelState.Values.Select(p => string.Join(";", p.Errors.Select(c => c.ErrorMessage))));
            }
            return Json(message);
        }

        /// <summary>
        /// 禁用岗位
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public IActionResult ForbiddenPosition(int Id)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            var position = _workbenchDbContext.Position.FirstOrDefault(p => p.Id == Id);
            if (position == null)
            {
                message.ErrorMessage = "岗位不存在!";
                return Json(message);
            }
            if (position.IsForbidden)
            {
                message.ErrorMessage = "岗位已经禁用不能重复禁用!";
                return Json(message);
            }
            User user = _workbenchContext.User;
            position.ForbiddenUser = user.Code;
            position.ForbiddenDate = DateTime.Now;
            position.IsForbidden = true;
            _workbenchDbContext.SaveChanges();
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 启用岗位
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public IActionResult EnablePosition(int Id)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            var position = _workbenchDbContext.Position.FirstOrDefault(p => p.Id == Id);
            if (position == null)
            {
                message.ErrorMessage = "岗位不存在!";
                return Json(message);
            }
            if (!position.IsForbidden)
            {
                message.ErrorMessage = "岗位已经启用不能重复启用!";
                return Json(message);
            }
            position.ForbiddenUser = "";
            position.ForbiddenDate = null;
            position.IsForbidden = false;
            _workbenchDbContext.SaveChanges();
            message.Code = ResponseCode.Success;
            return Json(message);
        }
    }
}
