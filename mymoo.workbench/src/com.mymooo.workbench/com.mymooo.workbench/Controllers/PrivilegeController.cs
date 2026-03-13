using com.mymooo.workbench.business.Account;
using com.mymooo.workbench.core.Account;
using com.mymooo.workbench.core.SystemManage;
using com.mymooo.workbench.ef;
using com.mymooo.workbench.ef.AccountContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mymooo.core;
using mymooo.core.Model.Gateway;
using mymooo.core.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.mymooo.workbench.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="workbenchDbContext"></param>
    /// <param name="tokenService"></param>
    /// <param name="workbenchContext"></param>
    public class PrivilegeController(WorkbenchDbContext workbenchDbContext, TokenService tokenService, WorkbenchContext workbenchContext) : Controller
    {
        private readonly WorkbenchDbContext _workbenchDbContext = workbenchDbContext;
        private readonly TokenService _tokenService = tokenService;
        private readonly WorkbenchContext _workbenchContext = workbenchContext;

        #region 菜单权限接口
        /// <summary>
        /// 获取菜单权限
        /// </summary>
        /// <returns></returns>
        public IActionResult GetMenu(string systemCode = "workbench")
        {
            var user = _workbenchContext.User;
            var meuns = _workbenchDbContext.UserRoles.Where(u => u.UserId == user.UserId)
                .Join(_workbenchDbContext.RolesMenu, u => u.RoleId, ur => ur.RoleId, (u, ur) => ur).Where(p => p.IsRight)
                .Join(_workbenchDbContext.Menu, u => u.MenuId, ur => ur.Id, (u, ur) => ur).Where(m => m.IsPublish && m.AppId == systemCode)
                .Distinct().ToList();
            ResponseMessage<List<MenuModel>> message = new()
            {
                Data = GetMenuTree(meuns, 0),
                Code = ResponseCode.Success
            };
            return Json(message);
        }

        private List<MenuModel> GetMenuTree(List<Menu> menus, long pid)
        {
            List<MenuModel> tree = [];
            var appIds = _workbenchDbContext.ThirdpartyApplicationConfig.Select(p => p.AppId).ToList();
            var children = menus.Where(p => p.ParentId == pid).ToList();
            var envCode = _workbenchContext.ApigatewayConfig.EnvCode.Equals("production", StringComparison.OrdinalIgnoreCase) ? "" : _workbenchContext.ApigatewayConfig.EnvCode;

            if (children.Count > 0)
            {
                foreach (var child in children)
                {
                    MenuModel itemMenu = new();
                    if (appIds.Contains(child.Name))
                    {
                        var app = _workbenchContext.GatewayRedisCache.HashGet(new SystemEnvironmentConfig()
                        {
                            SystemConfig = new SystemConfig() { SystemCode = child.Name },
                            EnvironmentConfig = new EnvironmentConfig() { EnvCode = _workbenchContext.ApigatewayConfig.EnvCode }
                        });
                        if (app == null || _workbenchContext.User.Token == null)
                        {
                            continue;
                        }
                        itemMenu.Path = $"{app.Url}Account/SignLogin?titck={HttpUtility.UrlEncode(RSACryptography.RsaKeyEncrypt(_workbenchContext.RsaConfig.PublicKey, _workbenchContext.User.Token))}";
                    }
                    else
                    {
                        itemMenu.Path = child.Path;
                    }
                    itemMenu.Sort = child.Sort;
                    itemMenu.Component = child.Component;
                    itemMenu.Name = child.Name;
                    itemMenu.Meta = new Meta
                    {
                        Title = child.Title,
                        Icon = child.Icon
                    };
                    itemMenu.children = GetMenuTree(menus, child.Id);
                    tree.Add(itemMenu);
                }
            }
            return tree;
        }

        #endregion


        #region 按钮权限接口

        /// <summary>
        /// 判断是否有权限
        /// </summary>
        /// <returns></returns>
        public IActionResult GetFunction(string path, string systemCode = "workbench")
        {
            ResponseMessage<List<MenuModel>> message = new ResponseMessage<List<MenuModel>>();
            //查询功能有没有加入功能权限
            var meunItem = _workbenchDbContext.MenuItem.FirstOrDefault(o => o.Path == path);
            if (meunItem == null)
            {
                message.Code = ResponseCode.Success;
                return Json(message);
            }
            else
            {
                if (meunItem.ControlPrivilege)
                {
                    //先验证一票否决
                    meunItem = _workbenchDbContext.UserRoles.Where(u => u.UserId == _workbenchContext.User.UserId)
                        .Join(_workbenchDbContext.RolesMenuItem, u => u.RoleId, ur => ur.RoleId, (u, ur) => ur)
                        .Where(p => p.IsRight == false)
                        .Join(_workbenchDbContext.MenuItem, u => u.MenuItemId, ur => ur.Id, (u, ur) => ur)
                        .Where(o => o.Path == path && o.AppId == systemCode)
                        .FirstOrDefault();

                    if (meunItem != null)
                    {
                        message.Code = ResponseCode.PrivilegeReject;
                        message.ErrorMessage = $"{meunItem.Title}功能被一票否决,无权调用!";
                        return Json(message);
                    }
                    meunItem = _workbenchDbContext.UserRoles.Where(u => u.UserId == _workbenchContext.User.UserId)
                        .Join(_workbenchDbContext.RolesMenuItem, u => u.RoleId, ur => ur.RoleId, (u, ur) => ur)
                        .Where(p => p.IsRight)
                        .Join(_workbenchDbContext.MenuItem, u => u.MenuItemId, ur => ur.Id, (u, ur) => ur)
                        .Where(o => o.Path == path && o.AppId == systemCode)
                        .FirstOrDefault();

                    if (meunItem == null)
                    {
                        message.Code = ResponseCode.PrivilegeReject;
                        message.ErrorMessage = $"用户无权限!";
                        return Json(message);
                    }
                }
            }
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        #endregion

        /// <summary>
        /// 获取该用户有权限的用户
        /// </summary>
        /// <returns></returns>
        public IActionResult GetAuthorityUserList()
        {
            ResponseMessage<List<AuthorityUser>> message = new() { Data = [] };
            var user = _workbenchContext.User;
            if (!user.IsAdmin && !user.IsKeeponAttache)
            {
                List<AuthorityUser> authorityUserLsit = _tokenService.GetAuthorityUserList(user);
                message.Data = authorityUserLsit;
            }
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 获取指定wechatCode 的权限
        /// </summary>
        /// <param name="wechatCode"></param>
        /// <returns></returns>
        public IActionResult GetAuthorityUserListByWechatCode(string wechatCode)
        {
            ResponseMessage<List<AuthorityUser>> message = new() { Data = [] };
            var user = _workbenchContext.User;
            var mymoooUser =  _workbenchDbContext.User.FirstOrDefault(e => e.UserId == wechatCode && !e.IsDelete && e.AppId == "weixinwork" && e.Status == 1);
            List<AuthorityUser> authorityUserLsit = _tokenService.GetAuthorityUserList(user, mymoooUser);
            message.Data = authorityUserLsit;
            message.Code = ResponseCode.Success;
            return Json(message);
        }


        /// <summary>
        /// 获取某员工的下属团队成员
        /// </summary>
        /// <param name="wechatCode">微信Code</param>
        /// <param name="IncludeResignations">是否包括2年内离职人员,默认不包括</param>
        /// <returns></returns>
        public IActionResult GetTeamUserList( string wechatCode, bool IncludeResignations = false)
        {
            ResponseMessage<List<AuthorityUser>> message = new() { Data = [] };
            if (string.IsNullOrEmpty(wechatCode))
            {
                message.Data = [];
                message.Code = ResponseCode.Success;
                return Json(message);
            }
            List<AuthorityUser> authorityUserLsit = _tokenService.GetTeamUserList(wechatCode, IncludeResignations);
            message.Data = authorityUserLsit;
            message.Code = ResponseCode.Success;
            return Json(message);
        }


        /// <summary>
        /// 判断是否是管理员
        /// </summary>
        /// <returns></returns>
        public IActionResult IsAdmin()
        {
            ResponseMessage<dynamic> message = new();
            var role = _workbenchDbContext.UserRoles.Include(p => p.Role).FirstOrDefault(p => p.UserId == _workbenchContext.User.UserId && p.Role.IsAdmin);
            message.Data = role != null;
            message.Code = ResponseCode.Success;
            return Json(message);
        }


        /// <summary>
        /// 判断某用户是否管理员或所有数据权限
        /// </summary>
        /// <param name="wechatCode"></param>
        /// <returns></returns>
        public IActionResult IsAdminOrAllDataPrivilege(string wechatCode)
        {
            bool isAllPrivilege = false;
            ResponseMessage<dynamic> message = new();
            List<string> roleCode = new List<string>()
            {
                "admin",
                "BusinessData"
            };

            var user = _workbenchDbContext.User.FirstOrDefault(e => e.UserId == wechatCode && !e.IsDelete && e.AppId == "weixinwork" && e.Status == 1);
            var roles = _workbenchDbContext.Roles.Where(e => roleCode.Contains(e.Code)).Select(e => e.Id).ToList();
            if (user != null && roles != null)
            {
                isAllPrivilege = _workbenchDbContext.UserRoles.Any(e => e.UserId == user.Id && roles.Contains(e.RoleId));
            }

            message.Data = isAllPrivilege;
            message.Code = ResponseCode.Success;
            return Json(message);
        }


        /// <summary>
        /// 判断是否是销售员
        /// </summary>
        /// <returns></returns>
        public IActionResult IsSalesman()
        {
            ResponseMessage<dynamic> message = new();
            var role = _workbenchDbContext.UserRoles.Include(p => p.Role).FirstOrDefault(p => p.UserId == _workbenchContext.User.UserId && p.Role.Code == "salesman");
            message.Data = role != null;
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 获取用户角色
        /// </summary>
        /// <returns></returns>
        public IActionResult GetUserRoles()
        {
            ResponseMessage<dynamic> message = new();
            var user = _workbenchContext.User;
            var linq = from a in _workbenchDbContext.User
                       join b in _workbenchDbContext.UserRoles on a.Id equals b.UserId into urTemp
                       from ur in urTemp.DefaultIfEmpty()
                       join c in _workbenchDbContext.Roles on ur.RoleId equals c.Id into rTemp
                       from r in rTemp.DefaultIfEmpty()
                       where a.Id == user.UserId
                       select (new
                       {
                           Code = r.Code ?? "",
                           Name = r.Name ?? ""
                       });
            message.Data = linq.ToList();
            message.Code = ResponseCode.Success;
            return Json(message);
        }
    }
}
