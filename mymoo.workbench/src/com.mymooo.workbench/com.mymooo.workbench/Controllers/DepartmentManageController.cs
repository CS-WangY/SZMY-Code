using com.mymooo.workbench.core;
using com.mymooo.workbench.core.Account;
using com.mymooo.workbench.core.Utils;
using com.mymooo.workbench.ef;
using com.mymooo.workbench.ef.AccountContext;
using com.mymooo.workbench.Filter;
using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.mymooo.workbench.Controllers
{
    /// <summary>
    /// 部门管理
    /// </summary>
    [ServiceFilter(typeof(TokenAuthorityAttribute))]
    public class DepartmentManageController(WorkbenchDbContext workbenchDbContext, WorkbenchContext workbenchContext) : Controller
    {
        private readonly WorkbenchDbContext _workbenchDbContext = workbenchDbContext;
        private readonly WorkbenchContext _workbenchContext = workbenchContext;

		/// <summary>
		/// 获得职能属性列表
		/// </summary>
		/// <returns></returns>
		public IActionResult GetFunctionAttrLsit()
        {
            ResponseMessage<List<SystemParam>> message = new ResponseMessage<List<SystemParam>>();
            var functionAttr = _workbenchDbContext.SystemParam.Where(c => c.GroupId == 0).ToList();
            message.Code = ResponseCode.Success;
            message.Data = functionAttr;
            return Json(message);
        }

        /// <summary>
        /// 修改部门职能属性
        /// </summary>
        /// <returns></returns>
        public IActionResult UpdateDepartmentFunctionAttr(long Id, string FunctionAttr)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();

            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            var old = _workbenchDbContext.Department.FirstOrDefault(p => p.Id == Id);
            old.FunctionAttr = FunctionAttr;
            _workbenchDbContext.SaveChanges();

            message.Data = old;
            message.Code = ResponseCode.Success;
            return Json(message);
        }
    }
}
