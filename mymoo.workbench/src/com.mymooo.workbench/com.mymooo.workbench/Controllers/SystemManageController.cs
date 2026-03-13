using com.mymooo.api.gateway.SDK;
using com.mymooo.workbench.business.Account;
using com.mymooo.workbench.core.Account;
using com.mymooo.workbench.core.SystemManage;
using com.mymooo.workbench.core.SystemManage.Filter;
using com.mymooo.workbench.core.Utils;
using com.mymooo.workbench.ef;
using com.mymooo.workbench.Filter;
using com.mymooo.workbench.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Utils.JsonConverter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace com.mymooo.workbench.Controllers
{
    /// <summary>
    /// 系统管理
    /// </summary>
    [ServiceFilter(typeof(TokenAuthorityAttribute))]
    public class SystemManageController(WorkbenchDbContext workbenchDbContext, HttpUtils httpUtils, TokenService tokenService, WorkbenchContext workbenchContext, RabbitMQServiceClient rabbitMQServiceClient) : Controller
    {
        private readonly WorkbenchDbContext _workbenchDbContext = workbenchDbContext;
        private readonly HttpUtils _httpUtils = httpUtils;
        private readonly TokenService _tokenService = tokenService;
        private readonly WorkbenchContext _workbenchContext = workbenchContext;
        private readonly RabbitMQServiceClient _rabbitMQServiceClient = rabbitMQServiceClient;

        public IActionResult GetMenu()
        {
            var meuns = _workbenchDbContext.ThirdpartyApplicationConfig.Include(c => c.Menu).ToList();
            return View();
        }

        public IActionResult Menu()
        {
            return View();
        }

        #region 菜单管理
        /// <summary>
        /// 获取菜单列表
        /// </summary>
        /// <returns></returns>
        public IActionResult GetMenuList()
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();

            var query = _workbenchDbContext.Menu.Join(_workbenchDbContext.ThirdpartyApplicationConfig, m => m.AppId, t => t.AppId, (m, t) => new
            {
                m.Id,
                m.ParentId,
                m.AppId,
                t.AppName,//系统编码转系统名称显示
                m.Component,
                m.CreateDate,
                m.CreateUser,
                m.Description,
                m.Icon,
                m.IsPublish,
                m.MenuItem,
                m.Name,
                m.Path,
                m.PublishDate,
                m.PublishUser,
                m.RolesMenu,
                m.Sort,
                m.Title
            }).ToList();
            message.Data = query;
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 获取菜单列表（分页）
        /// </summary>
        /// <returns></returns>
        public IActionResult GetMenuListByFilter([FromBody] PageRequest<MenuGridFilter> filter)
        {
            ResponseMessage<PageResponse<dynamic>> message = new();
            int total = 0;

            if (filter == null)
            {
                filter = new PageRequest<MenuGridFilter>();
            }
            if (filter.Filter != null && !string.IsNullOrWhiteSpace(filter.Filter.AppId))
            {
                string appId = filter.Filter.AppId;
                string[] appIdArr = appId.Split(",");
                var query = _workbenchDbContext.Menu.Where(c => appIdArr.Contains(c.AppId)).Join(_workbenchDbContext.ThirdpartyApplicationConfig, m => m.AppId, t => t.AppId, (m, t) => new
                {
                    m.Id,
                    m.ParentId,
                    m.AppId,
                    t.AppName,//系统编码转系统名称显示
                    m.Component,
                    m.CreateDate,
                    m.CreateUser,
                    m.Description,
                    m.Icon,
                    m.IsPublish,
                    m.MenuItem,
                    m.Name,
                    m.Path,
                    m.PublishDate,
                    m.PublishUser,
                    m.RolesMenu,
                    m.Sort,
                    m.Title
                }).ToList();
                total = query.Count;
                message.Data = new PageResponse<dynamic>(filter.PageIndex, filter.PageSize, total);
                var skip = (filter.PageIndex - 1) * filter.PageSize;
                if (total <= skip)
                {
                    message.Code = ResponseCode.Success;
                    return Json(message);
                }
                var result = query.Skip(skip).Take(filter.PageSize).ToList();
                result.ForEach(c =>
                    message.Data.Rows.Add(new
                    {
                        c.Id,
                        c.ParentId,
                        c.AppId,
                        c.AppName,
                        c.Component,
                        c.CreateDate,
                        c.CreateUser,
                        c.Description,
                        c.Icon,
                        c.IsPublish,
                        c.MenuItem,
                        c.Name,
                        c.Path,
                        c.PublishDate,
                        c.PublishUser,
                        c.RolesMenu,
                        c.Sort,
                        c.Title
                    })
                );
            }
            else
            {
                var query = _workbenchDbContext.Menu.Join(_workbenchDbContext.ThirdpartyApplicationConfig, m => m.AppId, t => t.AppId, (m, t) => new
                {
                    m.Id,
                    m.ParentId,
                    m.AppId,
                    t.AppName,//系统编码转系统名称显示
                    m.Component,
                    m.CreateDate,
                    m.CreateUser,
                    m.Description,
                    m.Icon,
                    m.IsPublish,
                    m.MenuItem,
                    m.Name,
                    m.Path,
                    m.PublishDate,
                    m.PublishUser,
                    m.RolesMenu,
                    m.Sort,
                    m.Title
                }).ToList();
                total = query.Count();
                message.Data = new PageResponse<dynamic>(filter.PageIndex, filter.PageSize, total);
                var skip = (filter.PageIndex - 1) * filter.PageSize;
                if (total <= skip)
                {
                    message.Code = ResponseCode.Success;
                    return Json(message);
                }
                var result = query.Skip(skip).Take(filter.PageSize).ToList();
                result.ForEach(c =>
                    message.Data.Rows.Add(new
                    {
                        c.Id,
                        c.ParentId,
                        c.AppId,
                        c.AppName,
                        c.Component,
                        c.CreateDate,
                        c.CreateUser,
                        c.Description,
                        c.Icon,
                        c.IsPublish,
                        c.MenuItem,
                        c.Name,
                        c.Path,
                        c.PublishDate,
                        c.PublishUser,
                        c.RolesMenu,
                        c.Sort,
                        c.Title
                    })
                );
            }

            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 获取系统列表
        /// </summary>
        /// <returns></returns>
        public IActionResult GetAppList()
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            //企业微信的系统不展示
            var query = _workbenchDbContext.ThirdpartyApplicationConfig.Where(c => c.IsProduction).ToList();
            message.Data = query;
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 根据系统获取菜单
        /// </summary>
        /// <returns></returns>
        public IActionResult GetMenuByAppId(string appId, bool IsShowChildMenu = false)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();

            if (!string.IsNullOrWhiteSpace(appId))
            {
                string[] appIdArr = appId.Split(",");
                if (!IsShowChildMenu)
                {
                    var query2 = _workbenchDbContext.Menu.Where(c => appIdArr.Contains(c.AppId));

                    var query = query2.Where(it => query2.Any(i => i.ParentId == it.Id)).Join(_workbenchDbContext.ThirdpartyApplicationConfig, m => m.AppId, t => t.AppId, (m, t) => new
                    {
                        m.Id,
                        m.ParentId,
                        m.AppId,
                        t.AppName,//系统编码转系统名称显示
                        m.Component,
                        m.CreateDate,
                        m.CreateUser,
                        m.Description,
                        m.Icon,
                        m.IsPublish,
                        m.MenuItem,
                        m.Name,
                        m.Path,
                        m.PublishDate,
                        m.PublishUser,
                        m.RolesMenu,
                        m.Sort,
                        m.Title
                    });
                    message.Data = query.ToList();
                }
                else
                {
                    var query = _workbenchDbContext.Menu.Where(c => appIdArr.Contains(c.AppId)).Join(_workbenchDbContext.ThirdpartyApplicationConfig, m => m.AppId, t => t.AppId, (m, t) => new
                    {
                        m.Id,
                        m.ParentId,
                        m.AppId,
                        t.AppName,//系统编码转系统名称显示
                        m.Component,
                        m.CreateDate,
                        m.CreateUser,
                        m.Description,
                        m.Icon,
                        m.IsPublish,
                        m.MenuItem,
                        m.Name,
                        m.Path,
                        m.PublishDate,
                        m.PublishUser,
                        m.RolesMenu,
                        m.Sort,
                        m.Title
                    });
                    message.Data = query.ToList();
                }
                message.Code = ResponseCode.Success;
                return Json(message);
            }
            else
            {
                var query = GetMenuList();
                return query;
            }


        }

        /// <summary>
        /// 新增菜单
        /// </summary>
        /// <returns></returns>
        public IActionResult InsertMenu([FromBody] ef.AccountContext.Menu Menu)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (this.TryValidateModel(Menu))
            {
                if (!_workbenchContext.User.IsAdmin)
                {
                    message.Code = ResponseCode.PrivilegeReject;
                    message.ErrorMessage = "当前用户没有权限!";
                    return Json(message);
                }
                if (string.IsNullOrWhiteSpace(Menu.Title))
                {
                    message.ErrorMessage = "菜单名称不能为空!";
                    return Json(message);
                }
                if (string.IsNullOrWhiteSpace(Menu.Path))
                {
                    message.ErrorMessage = "菜单路径不能为空!";
                    return Json(message);
                }
                if (string.IsNullOrWhiteSpace(Menu.Component))
                {
                    message.ErrorMessage = "组件路径不能为空!";
                    return Json(message);
                }
                User user = _workbenchContext.User;
                Menu.CreateUser = user.Code;
                Menu.CreateDate = DateTime.Now;
                Menu.IsPublish = false;
                _workbenchDbContext.Menu.Add(Menu);
                _workbenchDbContext.SaveChanges();
                message.Data = Menu;
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
        /// 修改菜单
        /// </summary>
        /// <returns></returns>
        public IActionResult UpdateMenu([FromBody] ef.AccountContext.Menu Menu)
        {
            ResponseMessage<dynamic> message = new();
            if (this.TryValidateModel(Menu))
            {
                if (!_workbenchContext.User.IsAdmin)
                {
                    message.Code = ResponseCode.PrivilegeReject;
                    message.ErrorMessage = "当前用户没有权限!";
                    return Json(message);
                }
                if (string.IsNullOrWhiteSpace(Menu.Title))
                {
                    message.ErrorMessage = "菜单名称不能为空!";
                    return Json(message);
                }
                if (string.IsNullOrWhiteSpace(Menu.Path))
                {
                    message.ErrorMessage = "菜单路径不能为空!";
                    return Json(message);
                }
                if (string.IsNullOrWhiteSpace(Menu.Component))
                {
                    message.ErrorMessage = "组件路径不能为空!";
                    return Json(message);
                }
                var old = _workbenchDbContext.Menu.FirstOrDefault(p => p.Id == Menu.Id);
                old.ParentId = Menu.ParentId;
                old.Path = Menu.Path;
                old.Title = Menu.Title;
                old.Icon = Menu.Icon;
                old.Sort = Menu.Sort;
                old.Description = Menu.Description;
                old.Component = Menu.Component;
                old.Name = Menu.Name;
                Menu = old;
                _workbenchDbContext.SaveChanges();
                message.Data = Menu;
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
        /// 删除菜单
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public IActionResult DeleteMenu(int Id)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            var Item = _workbenchDbContext.Menu.FirstOrDefault(c => c.Id == Id);
            var Child = _workbenchDbContext.Menu.FirstOrDefault(c => c.ParentId == Id);
            if (Item == null)
            {
                message.ErrorMessage = "菜单不存在!";
                return Json(message);
            }
            if (Item.ParentId == 0 && Child != null)
            {
                message.ErrorMessage = "根菜单且有子菜单不能删除!";
                return Json(message);
            }
            if (Item.IsPublish)
            {
                message.ErrorMessage = "菜单已发布不能删除!";
                return Json(message);
            }
            var isQuoted = _workbenchDbContext.RolesMenu.FirstOrDefault(c => c.Id == Id);
            if (isQuoted != null)
            {
                message.Code = ResponseCode.Success;
                message.Data = new
                {
                    IsQuoted = true
                };
                return Json(message);
            }
            else
            {
                _workbenchDbContext.Menu.Remove(Item);
                _workbenchDbContext.SaveChanges();
                message.Code = ResponseCode.Success;
                return Json(message);
            }
        }

        /// <summary>
        /// 删除菜单引用
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public IActionResult DeleteMenuQuote(int Id)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            var Item = _workbenchDbContext.Menu.FirstOrDefault(c => c.Id == Id);
            var Child = _workbenchDbContext.Menu.FirstOrDefault(c => c.ParentId == Id);
            if (Item == null)
            {
                message.ErrorMessage = "菜单不存在!";
                return Json(message);
            }
            if (Item.ParentId == 0 && Child != null)
            {
                message.ErrorMessage = "根菜单且有子菜单不能删除!";
                return Json(message);
            }
            if (Item.IsPublish)
            {
                message.ErrorMessage = "菜单已发布不能删除!";
                return Json(message);
            }
            using (TransactionScope scope = new TransactionScope())
            {
                var s = _workbenchDbContext.RolesMenu.Where(c => c.MenuId == Id);
                foreach (var i in s)
                {
                    _workbenchDbContext.RolesMenu.Remove(i);
                }
                _workbenchDbContext.Menu.Remove(Item);
                _workbenchDbContext.SaveChanges();
                scope.Complete();
            }
            message.Code = ResponseCode.Success;
            return Json(message);
        }


        /// <summary>
        /// 发布菜单
        /// </summary>
        /// <param name="Id">菜单id</param>
        /// <returns></returns>
        public IActionResult PublishMenu(int Id)
        {
            ResponseMessage<dynamic> message = new();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            var ReportMenu = _workbenchDbContext.Menu.FirstOrDefault(p => p.Id == Id);
            if (ReportMenu == null)
            {
                message.ErrorMessage = "菜单不存在!";
                return Json(message);
            }
            if (ReportMenu.IsPublish)
            {
                message.ErrorMessage = "菜单已发布不能重复发布!";
                return Json(message);
            }
            User user = _workbenchContext.User;
            ReportMenu.PublishUser = user.Code;
            ReportMenu.PublishDate = DateTime.Now;
            ReportMenu.IsPublish = true;
            _workbenchDbContext.SaveChanges();
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 下架菜单
        /// </summary>
        /// <param name="Id">菜单id</param>
        /// <returns></returns>
        public IActionResult OffMenu(int Id)
        {
            ResponseMessage<dynamic> message = new();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            var ReportMenu = _workbenchDbContext.Menu.FirstOrDefault(p => p.Id == Id);
            if (ReportMenu == null)
            {
                message.ErrorMessage = "菜单不存在!";
                return Json(message);
            }
            if (!ReportMenu.IsPublish)
            {
                message.ErrorMessage = "菜单已下架不能重复下架!";
                return Json(message);
            }
            ReportMenu.PublishUser = "";
            ReportMenu.PublishDate = null;
            ReportMenu.IsPublish = false;
            _workbenchDbContext.SaveChanges();
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        #endregion

        #region 角色管理
        /// <summary>
        /// 获取角色列表
        /// </summary>
        /// <returns></returns>
        public IActionResult GetRoleList()
        {
            ResponseMessage<dynamic> message = new();

            var query = _workbenchDbContext.Roles.ToList();
            message.Data = query;
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 获取角色列表(分页)
        /// </summary>
        /// <returns></returns>
        public IActionResult GetRoleListByFilter([FromBody] PageRequest<RolesFilter> filter)
        {
            ResponseMessage<PageResponse<dynamic>> message = new ResponseMessage<PageResponse<dynamic>>();

            if (filter == null)
            {
                filter = new PageRequest<RolesFilter>();
            }
            var query = _workbenchDbContext.Roles.ToList();
            var total = query.Count;
            message.Data = new PageResponse<dynamic>(filter.PageIndex, filter.PageSize, total);
            //计算需要跳过是数量
            var skip = (filter.PageIndex - 1) * filter.PageSize;
            if (total <= skip)
            {
                //总数小于跳过的数量,直接返回
                message.Code = ResponseCode.Success;
                return Json(message);
            }
            var result = query.Skip(skip).Take(filter.PageSize).ToList();
            result.ForEach(c =>
                message.Data.Rows.Add(new
                {
                    c.Code,
                    c.CreateDate,
                    c.CreateUser,
                    c.Description,
                    c.ForbiddenDate,
                    c.ForbiddenUser,
                    c.Id,
                    c.IsAdmin,
                    c.IsForbidden,
                    c.Name,
                    c.RolesMenu,
                    c.UserRoles
                })
            );
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 新增角色
        /// </summary>
        /// <returns></returns>
        public IActionResult InsertRole([FromBody] ef.AccountContext.Roles Roles)
        {
            ResponseMessage<dynamic> message = new();
            if (this.TryValidateModel(Roles))
            {
                if (!_workbenchContext.User.IsAdmin)
                {
                    message.Code = ResponseCode.PrivilegeReject;
                    message.ErrorMessage = "当前用户没有权限!";
                    return Json(message);
                }
                if (string.IsNullOrWhiteSpace(Roles.Code))
                {
                    message.ErrorMessage = "角色编码不能为空!";
                    return Json(message);
                }
                if (string.IsNullOrWhiteSpace(Roles.Name))
                {
                    message.ErrorMessage = "角色名称不能为空!";
                    return Json(message);
                }
                User user = _workbenchContext.User;
                Roles.CreateUser = user.Code;
                Roles.CreateDate = DateTime.Now;
                _workbenchDbContext.Roles.Add(Roles);
                _workbenchDbContext.SaveChanges();
                message.Data = Roles;
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
        /// 修改角色
        /// </summary>
        /// <returns></returns>
        public IActionResult UpdateRole([FromBody] ef.AccountContext.Roles Roles)
        {
            ResponseMessage<dynamic> message = new();
            if (this.TryValidateModel(Roles))
            {
                if (!_workbenchContext.User.IsAdmin)
                {
                    message.Code = ResponseCode.PrivilegeReject;
                    message.ErrorMessage = "当前用户没有权限!";
                    return Json(message);
                }
                if (string.IsNullOrWhiteSpace(Roles.Code))
                {
                    message.ErrorMessage = "角色编码不能为空!";
                    return Json(message);
                }
                if (string.IsNullOrWhiteSpace(Roles.Name))
                {
                    message.ErrorMessage = "角色名称不能为空!";
                    return Json(message);
                }
                var old = _workbenchDbContext.Roles.FirstOrDefault(p => p.Id == Roles.Id);
                old.Code = Roles.Code;
                old.Name = Roles.Name;
                old.Description = Roles.Description;
                Roles = old;
                _workbenchDbContext.SaveChanges();
                message.Data = Roles;
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
        /// 删除角色
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public IActionResult DeleteRole(int Id)
        {
            ResponseMessage<dynamic> message = new();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            var role = _workbenchDbContext.Roles.FirstOrDefault(p => p.Id == Id);
            if (role == null)
            {
                message.ErrorMessage = "角色不存在!";
                return Json(message);
            }
            if (!role.IsForbidden)
            {
                message.ErrorMessage = "角色已经启用不能删除!";
                return Json(message);
            }
            var isQuotedUser = _workbenchDbContext.UserRoles.FirstOrDefault(c => c.RoleId == Id);
            var isQuotedMenu = _workbenchDbContext.RolesMenu.FirstOrDefault(c => c.RoleId == Id);
            if (isQuotedUser != null || isQuotedMenu != null)
            {
                message.Code = ResponseCode.Success;
                message.Data = new
                {
                    IsQuoted = true
                };
                return Json(message);
            }
            else
            {
                _workbenchDbContext.Roles.Remove(role);
                _workbenchDbContext.SaveChanges();
                message.Code = ResponseCode.Success;
                return Json(message);
            }
        }

        /// <summary>
        /// 删除角色引用
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public IActionResult DeleteRoleQuoted(int Id)
        {
            ResponseMessage<dynamic> message = new();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            var role = _workbenchDbContext.Roles.FirstOrDefault(p => p.Id == Id);
            if (role == null)
            {
                message.ErrorMessage = "角色不存在!";
                return Json(message);
            }
            if (!role.IsForbidden)
            {
                message.ErrorMessage = "角色已经启用不能删除!";
                return Json(message);
            }
            using (TransactionScope scope = new TransactionScope())
            {
                var s = _workbenchDbContext.UserRoles.Where(c => c.RoleId == Id);
                var r = _workbenchDbContext.RolesMenu.Where(c => c.RoleId == Id);
                foreach (var i in s)
                {
                    _workbenchDbContext.UserRoles.Remove(i);
                }
                foreach (var j in r)
                {
                    _workbenchDbContext.RolesMenu.Remove(j);
                }
                _workbenchDbContext.Roles.Remove(role);
                _workbenchDbContext.SaveChanges();
                scope.Complete();
            }
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 给角色设置管理员
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public IActionResult SetRoleAdmin(int Id)
        {
            ResponseMessage<dynamic> message = new();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            var setAdminRole = _workbenchDbContext.Roles.FirstOrDefault(p => p.Id == Id);
            var noSetAdminRoles = _workbenchDbContext.Roles.Where(p => p.Id != Id);
            if (setAdminRole == null)
            {
                message.ErrorMessage = "角色不存在!";
                return Json(message);
            }
            setAdminRole.IsAdmin = true;
            foreach (var i in noSetAdminRoles)
            {
                i.IsAdmin = false;
            }
            _workbenchDbContext.SaveChanges();
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 禁用角色
        /// </summary>
        /// <param name="Id">菜单id</param>
        /// <returns></returns>
        public IActionResult ForbiddenRole(int Id)
        {
            ResponseMessage<dynamic> message = new();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            var role = _workbenchDbContext.Roles.FirstOrDefault(p => p.Id == Id);
            if (role == null)
            {
                message.ErrorMessage = "角色不存在!";
                return Json(message);
            }
            if (role.IsForbidden)
            {
                message.ErrorMessage = "角色已经禁用不能重复禁用!";
                return Json(message);
            }
            User user = _workbenchContext.User;
            role.ForbiddenUser = user.Code;
            role.ForbiddenDate = DateTime.Now;
            role.IsForbidden = true;
            _workbenchDbContext.SaveChanges();
            message.Code = ResponseCode.Success;
            return Json(message);
        }


        /// <summary>
        /// 启用角色
        /// </summary>
        /// <param name="Id">菜单id</param>
        /// <returns></returns>
        public IActionResult EnableRole(int Id)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            var role = _workbenchDbContext.Roles.FirstOrDefault(p => p.Id == Id);
            if (role == null)
            {
                message.ErrorMessage = "角色不存在!";
                return Json(message);
            }
            if (!role.IsForbidden)
            {
                message.ErrorMessage = "角色已经启用不能重复启用!";
                return Json(message);
            }
            role.ForbiddenUser = "";
            role.ForbiddenDate = null;
            role.IsForbidden = false;
            _workbenchDbContext.SaveChanges();
            message.Code = ResponseCode.Success;
            return Json(message);
        }
        #endregion


        #region 角色菜单管理
        /// <summary>
        /// 根据角色id获取菜单列表
        /// </summary>
        /// <returns></returns>
        public IActionResult GetRolesMenu(int RoleId, string appId)
        {
            ResponseMessage<dynamic> message = new();

            var menuIdList = _workbenchDbContext.Menu.Where(c => c.AppId == appId).ToList();
            ArrayList arrayList = new ArrayList();
            foreach (var item in menuIdList)
            {
                arrayList.Add(item.Id);
            }
            var query = _workbenchDbContext.RolesMenu.Where(c => c.RoleId == RoleId && arrayList.Contains(c.MenuId));

            //var query = _workbenchDbContext.RolesMenu.Where(c => c.RoleId == RoleId);
            message.Data = query.ToList();
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 给角色分配菜单
        /// </summary>
        /// <returns></returns>
        public IActionResult AssignMenu([FromBody] AssignReportMenuFilter filter)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            User user = _workbenchContext.User;
            //1、根据appid查有哪些menuId
            var menuIdList = _workbenchDbContext.Menu.Where(c => c.AppId == filter.appId).ToList();
            ArrayList arrayList = new ArrayList();
            foreach (var item in menuIdList)
            {
                arrayList.Add(item.Id);
            }
            //2、根据RoleId和menuId查询出所有记录
            var role = _workbenchDbContext.RolesMenu.Where(c => c.RoleId == filter.roleId && arrayList.Contains(c.MenuId)).ToList();
            using (TransactionScope scope = new TransactionScope())
            {
                //3、删除相关记录
                foreach (var r in role)
                {
                    _workbenchDbContext.RolesMenu.Remove(r);
                }
                //4、重新插入新记录
                foreach (var m in filter.menuId)
                {
                    ef.AccountContext.RolesMenu rolesMenu = new()
                    {
                        RoleId = filter.roleId,
                        MenuId = m,
                        IsRight = true,
                        CreateUser = user.Code,
                        CreateDate = DateTime.Now
                    };
                    _workbenchDbContext.RolesMenu.Add(rolesMenu);
                }
                _workbenchDbContext.SaveChanges();
                scope.Complete();
            }
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 角色取消分配菜单
        /// </summary>
        /// <returns></returns>
        public IActionResult CancelMenu([FromBody] AssignReportMenuFilter filter)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            User user = _workbenchContext.User;
            var role = _workbenchDbContext.RolesMenu.Where(c => c.RoleId == filter.roleId && filter.menuId.Contains(c.MenuId)).ToList();
            using (TransactionScope scope = new TransactionScope())
            {
                foreach (var r in role)
                {
                    _workbenchDbContext.RolesMenu.Remove(r);
                }
                _workbenchDbContext.SaveChanges();
                scope.Complete();
            }
            message.Code = ResponseCode.Success;
            return Json(message);
        }
        #endregion


        #region 用户管理


        /// <summary>
        /// 根据用户id获取角色列表
        /// </summary>
        /// <returns></returns>
        public IActionResult GetUserRoles(long userId)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();

            var query = _workbenchDbContext.UserRoles.Where(c => c.UserId == userId).ToList();
            foreach (var item in query)
            {
                item.Role = (ef.AccountContext.Roles)_workbenchDbContext.Roles.FirstOrDefault(c => c.Id == item.RoleId);
            }
            message.Data = query;
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 给用户分配角色
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IActionResult AssignRole([FromBody] AssignReportRoleFilter filter)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            User user = _workbenchContext.User;
            var users = _workbenchDbContext.UserRoles.Where(c => c.UserId == filter.userId);
            using (TransactionScope scope = new TransactionScope())
            {
                foreach (var u in users)
                {
                    _workbenchDbContext.UserRoles.Remove(u);
                }
                foreach (var r in filter.roleId)
                {
                    ef.AccountContext.UserRoles userRoles = new()
                    {
                        UserId = filter.userId,
                        RoleId = r,
                        CreateUser = user.Code,
                        CreateDate = DateTime.Now
                    };
                    _workbenchDbContext.UserRoles.Add(userRoles);
                }
                _workbenchDbContext.SaveChanges();
                scope.Complete();
            }

            // 正式环境中清除一下可能存在的服务器端缓存, 不重要
            try
            {
                var userWechatCode = _workbenchDbContext.User.Where(c => c.Id == filter.userId).First().UserId;
                ClearPrivilegeCache(userWechatCode);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }



            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 根据用户id获取岗位列表
        /// </summary>
        /// <returns></returns>
        public IActionResult GetUserPosition(long userId)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();

            var query = _workbenchDbContext.UserPosition.Where(c => c.UserId == userId);
            message.Data = query.ToList();
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 给用户分派岗位
        /// </summary>
        /// <returns></returns>
        public IActionResult AssignPostion([FromBody] AssignPositionFilter filter)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            User user = _workbenchContext.User;
            var users = _workbenchDbContext.UserPosition.Where(c => c.UserId == filter.userId);
            using (TransactionScope scope = new TransactionScope())
            {
                foreach (var u in users)
                {
                    _workbenchDbContext.UserPosition.Remove(u);
                }
                foreach (var r in filter.positionId)
                {
                    ef.AccountContext.UserPosition userPosition = new()
                    {
                        UserId = filter.userId,
                        PositionId = r,
                        CreateUser = user.Code,
                        CreateDate = DateTime.Now
                    };
                    _workbenchDbContext.UserPosition.Add(userPosition);
                }
                _workbenchDbContext.SaveChanges();
                scope.Complete();
            }
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 给用户分配助理
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IActionResult AssignAssistant([FromBody] AssignAssistantFilter filter)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            var assistantCode = _workbenchDbContext.User.FirstOrDefault(c => c.Id == filter.assistantId).UserId;
            var isAssistant = _workbenchDbContext.UserPosition.Where(c => c.UserId == filter.assistantId).Include(c => c.Position).Any(o => o.Position.IsAssistant != null && o.Position.IsAssistant == true && !o.Position.IsForbidden);
            if (!isAssistant)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = $"{assistantCode}不是助理岗!";
                return Json(message);
            }
            User user = _workbenchContext.User;
            using (TransactionScope scope = new TransactionScope())
            {
                foreach (var r in filter.userId)
                {
                    if (!_workbenchDbContext.AssistantUsers.Any(c => c.UserId == r && c.AssistantId == filter.assistantId))
                    {
                        //var flag = _workbenchDbContext.UserPosition.Where(c => c.UserId == r).Include(c => c.Position).Any(o => o.Position.IsAssistant != null && o.Position.IsAssistant == true && !o.Position.IsForbidden);
                        //if (flag)
                        //{
                        //    message.Code = ResponseCode.PrivilegeReject;
                        //    message.ErrorMessage = $"{assistantCode}已经是助理不能在分配助理!";
                        //    return Json(message);
                        //}
                        if (filter.assistantId == r)
                        {
                            message.Code = ResponseCode.PrivilegeReject;
                            message.ErrorMessage = $"{assistantCode}不能分配给{assistantCode}";
                            return Json(message);
                        }
                        ef.AccountContext.AssistantUser assistantUsers = new()
                        {
                            AssistantId = filter.assistantId,
                            UserId = r,
                            CreateUser = user.Code,
                            CreateDate = DateTime.Now
                        };
                        _workbenchDbContext.AssistantUsers.Add(assistantUsers);
                    }
                }
                _workbenchDbContext.SaveChanges();
                scope.Complete();

                // 正式环境中清除一下可能存在的服务器端缓存, 不重要, 也会N小时后自动过期
                try
                {                   
                    ClearPrivilegeCache(assistantCode);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 获取用户的助理/获取助理负责的用户
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="IsAssistant">true为查助理列表</param>
        /// <returns></returns>
        public ActionResult GetUserAssistant(long UserId, bool IsAssistant)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();

            if (IsAssistant)
            {
                var query = _workbenchDbContext.User.Where(c => c.Id == UserId)
                .Join(_workbenchDbContext.AssistantUsers, u => u.Id, au => au.UserId, (u, au) => new
                {
                    assistantCode = _workbenchDbContext.User.FirstOrDefault(c => c.Id == au.AssistantId).UserId,
                    assistantName = _workbenchDbContext.User.FirstOrDefault(c => c.Id == au.AssistantId).Name,
                    assistantId = au.AssistantId,
                    mymoooCompany = u.AppId
                }).Distinct();
                message.Data = query.ToList();
            }
            else
            {
                var query = _workbenchDbContext.User.Where(c => c.Id == UserId)
                .Join(_workbenchDbContext.AssistantUsers, u => u.Id, au => au.AssistantId, (u, au) => new
                {
                    UserCode = _workbenchDbContext.User.FirstOrDefault(c => c.Id == au.UserId).UserId,
                    UserName = _workbenchDbContext.User.FirstOrDefault(c => c.Id == au.UserId).Name,
                    UserId = au.UserId,
                    mymoooCompany = u.AppId
                }).Distinct();
                message.Data = query.ToList();
            }
            message.Code = ResponseCode.Success;
            return Json(message);
        }


        /// <summary>
        /// 通过用户企业微信账号获取用户的助理/获取助理负责的用户
        /// </summary>
        /// <param name="UserWxCode">用户企业微信的账号</param>
        /// <param name="IsAssistant">true为查助理列表</param>
        /// <returns></returns>
        public ActionResult GetUserAssistantByWxCode(string UserWxCode, bool IsAssistant)
        {
            ResponseMessage<dynamic> message = new();

            if (IsAssistant)
            {
                var query = _workbenchDbContext.User.Where(c => c.UserId == UserWxCode)
                .Join(_workbenchDbContext.AssistantUsers, u => u.Id, au => au.UserId, (u, au) => new
                {
                    assistantCode = _workbenchDbContext.User.FirstOrDefault(c => c.Id == au.AssistantId).UserId,
                    assistantName = _workbenchDbContext.User.FirstOrDefault(c => c.Id == au.AssistantId).Name,
                    isDelete = _workbenchDbContext.User.FirstOrDefault(c => c.Id == au.AssistantId).IsDelete,
                    assistantId = au.AssistantId,
                    mymoooCompany = u.AppId
                }).Distinct();
                message.Data = query.ToList();
            }
            else
            {
                var query = _workbenchDbContext.User.Where(c => c.UserId == UserWxCode)
                .Join(_workbenchDbContext.AssistantUsers, u => u.Id, au => au.AssistantId, (u, au) => new
                {
                    UserCode = _workbenchDbContext.User.FirstOrDefault(c => c.Id == au.UserId).UserId,
                    UserName = _workbenchDbContext.User.FirstOrDefault(c => c.Id == au.UserId).Name,
                    isDelete = _workbenchDbContext.User.FirstOrDefault(c => c.Id == au.AssistantId).IsDelete,
                    au.UserId,
                    mymoooCompany = u.AppId
                }).Distinct();
                message.Data = query.ToList();
            }
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 给用户删除助理
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="AssistantId"></param>
        /// <returns></returns>
        public ActionResult DeleteAssistant(long UserId, long AssistantId)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            var item = _workbenchDbContext.AssistantUsers.FirstOrDefault(c => c.UserId == UserId && c.AssistantId == AssistantId);
            _workbenchDbContext.AssistantUsers.Remove(item);
            _workbenchDbContext.SaveChanges();
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        #endregion

        #region 功能管理
        /// <summary>
        /// 查询所有按钮权限配置
        /// </summary>
        /// <returns></returns>
        public IActionResult GetMenuItemList()
        {
            ResponseMessage<dynamic> message = new();

            var query = _workbenchDbContext.MenuItem
                .Join(_workbenchDbContext.ThirdpartyApplicationConfig, mi => mi.AppId, t => t.AppId, (mi, t) => mi)
                .Join(_workbenchDbContext.Menu, mi => mi.MenuId, m => m.Id, (mi, m) => new MenuItemGridModel
                {
                    Id = mi.Id,
                    MenuId = mi.MenuId,
                    MenuTitle = m.Title,
                    AppId = mi.AppId,
                    AppName = m.ThirdpartyApplicationConfig.AppName,//系统编码转系统名称显示
                    ControlPrivilege = mi.ControlPrivilege,
                    CreateDate = mi.CreateDate,
                    CreateUser = mi.CreateUser,
                    Description = mi.Description,
                    Path = mi.Path,
                    Title = mi.Title,
                    EnableDate = mi.EnableDate,
                    EnableUser = mi.EnableUser
                }).ToList();
            message.Data = query;
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 根据系统号查询按钮权限配置
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public IActionResult GetMenuItemByAppId(string appId)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();

            if (!string.IsNullOrWhiteSpace(appId))
            {
                string[] appIdArr = appId.Split(",");
                var query = _workbenchDbContext.MenuItem.Where(c => appIdArr.Contains(c.AppId))
                .Join(_workbenchDbContext.ThirdpartyApplicationConfig, mi => mi.AppId, t => t.AppId, (mi, t) => mi)
                .Join(_workbenchDbContext.Menu, mi => mi.MenuId, m => m.Id, (mi, m) => new MenuItemGridModel
                {
                    Id = mi.Id,
                    MenuId = mi.MenuId,
                    MenuTitle = m.Title,
                    AppId = mi.AppId,
                    AppName = m.ThirdpartyApplicationConfig.AppName,//系统编码转系统名称显示
                    ControlPrivilege = mi.ControlPrivilege,
                    CreateDate = mi.CreateDate,
                    CreateUser = mi.CreateUser,
                    Description = mi.Description,
                    Path = mi.Path,
                    Title = mi.Title,
                    EnableDate = mi.EnableDate,
                    EnableUser = mi.EnableUser
                }).ToList();
                message.Data = query;
                message.Code = ResponseCode.Success;
                return Json(message);
            }
            else
            {
                var query = GetMenuItemList();
                return query;
            }
        }

        /// <summary>
        /// 查询按钮权限（分页）
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IActionResult GetMenuItemListByFilter([FromBody] PageRequest<MenuItemGridFilter> filter)
        {
            ResponseMessage<PageResponse<dynamic>> message = new();
            List<MenuItemGridModel> query = [];

            if (filter == null)
            {
                filter = new PageRequest<MenuItemGridFilter>();
            }
            if (filter.Filter != null && !string.IsNullOrWhiteSpace(filter.Filter.AppId))
            {
                string appId = filter.Filter.AppId;
                string[] appIdArr = appId.Split(",");
                query = [.. _workbenchDbContext.MenuItem.Where(c => appIdArr.Contains(c.AppId))
                .Join(_workbenchDbContext.ThirdpartyApplicationConfig, mi => mi.AppId, t => t.AppId, (mi, t) => mi)
                .Join(_workbenchDbContext.Menu, mi => mi.MenuId, m => m.Id, (mi, m) => new MenuItemGridModel
                {
                    Id = mi.Id,
                    MenuId = mi.MenuId,
                    MenuTitle = m.Title,
                    AppId = mi.AppId,
                    AppName = m.ThirdpartyApplicationConfig.AppName,//系统编码转系统名称显示
                    ControlPrivilege = mi.ControlPrivilege,
                    CreateDate = mi.CreateDate,
                    CreateUser = mi.CreateUser,
                    Description = mi.Description,
                    Path = mi.Path,
                    Title = mi.Title,
                    EnableDate = mi.EnableDate,
                    EnableUser = mi.EnableUser
                })];
            }
            else
            {
                query = [.. _workbenchDbContext.MenuItem
                .Join(_workbenchDbContext.ThirdpartyApplicationConfig, mi => mi.AppId, t => t.AppId, (mi, t) => mi)
                .Join(_workbenchDbContext.Menu, mi => mi.MenuId, m => m.Id, (mi, m) => new MenuItemGridModel
                {
                    Id = mi.Id,
                    MenuId = mi.MenuId,
                    MenuTitle = m.Title,
                    AppId = mi.AppId,
                    AppName = m.ThirdpartyApplicationConfig.AppName,//系统编码转系统名称显示
                    ControlPrivilege = mi.ControlPrivilege,
                    CreateDate = mi.CreateDate,
                    CreateUser = mi.CreateUser,
                    Description = mi.Description,
                    Path = mi.Path,
                    Title = mi.Title,
                    EnableDate = mi.EnableDate,
                    EnableUser = mi.EnableUser
                })];
            }
            var total = query.Count();
            message.Data = new PageResponse<dynamic>(filter.PageIndex, filter.PageSize, total);
            var skip = (filter.PageIndex - 1) * filter.PageSize;
            if (total <= skip)
            {
                message.Code = ResponseCode.Success;
                return Json(message);
            }
            var result = query.Skip(skip).Take(filter.PageSize).ToList();
            result.ForEach(c =>
                message.Data.Rows.Add(new
                {
                    c.Id,
                    c.MenuId,
                    c.MenuTitle,
                    c.AppId,
                    c.AppName,
                    c.ControlPrivilege,
                    c.CreateDate,
                    c.CreateUser,
                    c.Description,
                    c.Path,
                    c.Title,
                    c.EnableDate,
                    c.EnableUser
                })
            );
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 新增按钮权限配置
        /// </summary>
        /// <param name="MenuItem"></param>
        /// <returns></returns>
        public IActionResult InsertMenuItem([FromBody] ef.AccountContext.MenuItem MenuItem)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (this.TryValidateModel(MenuItem))
            {
                if (!_workbenchContext.User.IsAdmin)
                {
                    message.Code = ResponseCode.PrivilegeReject;
                    message.ErrorMessage = "当前用户没有权限!";
                    return Json(message);
                }
                if (string.IsNullOrWhiteSpace(MenuItem.MenuId.ToString()))
                {
                    message.ErrorMessage = "所属页面不能为空!";
                    return Json(message);
                }
                if (string.IsNullOrWhiteSpace(MenuItem.Path))
                {
                    message.ErrorMessage = "路径不能为空!";
                    return Json(message);
                }
                if (string.IsNullOrWhiteSpace(MenuItem.Title))
                {
                    message.ErrorMessage = "功能按钮名称不能为空!";
                    return Json(message);
                }
                User user = _workbenchContext.User;
                MenuItem.CreateUser = user.Code;
                MenuItem.CreateDate = DateTime.Now;
                _workbenchDbContext.MenuItem.Add(MenuItem);
                _workbenchDbContext.SaveChanges();
                message.Data = MenuItem;
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
        /// 编辑按钮权限配置
        /// </summary>
        /// <returns></returns>
        public IActionResult UpdateMenuItem([FromBody] ef.AccountContext.MenuItem MenuItem)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (this.TryValidateModel(MenuItem))
            {
                if (!_workbenchContext.User.IsAdmin)
                {
                    message.Code = ResponseCode.PrivilegeReject;
                    message.ErrorMessage = "当前用户没有权限!";
                    return Json(message);
                }
                if (string.IsNullOrWhiteSpace(MenuItem.MenuId.ToString()))
                {
                    message.ErrorMessage = "所属页面不能为空!";
                    return Json(message);
                }
                if (string.IsNullOrWhiteSpace(MenuItem.Path))
                {
                    message.ErrorMessage = "路径不能为空!";
                    return Json(message);
                }
                if (string.IsNullOrWhiteSpace(MenuItem.Title))
                {
                    message.ErrorMessage = "功能按钮名称不能为空!";
                    return Json(message);
                }
                var old = _workbenchDbContext.MenuItem.FirstOrDefault(p => p.Id == MenuItem.Id);
                old.AppId = MenuItem.AppId;
                old.MenuId = MenuItem.MenuId;
                old.Path = MenuItem.Path;
                old.Title = MenuItem.Title;
                old.Description = MenuItem.Description;
                MenuItem = old;
                _workbenchDbContext.SaveChanges();
                message.Data = MenuItem;
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
        /// 删除按钮权限配置
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public IActionResult DeleteMenuItem(int Id)
        {
            ResponseMessage<dynamic> message = new();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            var Item = _workbenchDbContext.MenuItem.FirstOrDefault(c => c.Id == Id);
            if (Item == null)
            {
                message.ErrorMessage = "功能按钮不存在!";
                return Json(message);
            }
            var isQuoted = _workbenchDbContext.RolesMenuItem.FirstOrDefault(c => c.Id == Id);
            if (isQuoted != null)
            {
                message.Code = ResponseCode.Success;
                message.Data = new
                {
                    IsQuoted = true
                };
                return Json(message);
            }
            else
            {
                _workbenchDbContext.MenuItem.Remove(Item);
                _workbenchDbContext.SaveChanges();
                message.Code = ResponseCode.Success;
                return Json(message);
            }
        }

        /// <summary>
        /// 删除按钮权限引用
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public IActionResult DeleteMenuItemQuote(int Id)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            var Item = _workbenchDbContext.MenuItem.FirstOrDefault(c => c.Id == Id);
            if (Item == null)
            {
                message.ErrorMessage = "按钮配置不存在!";
                return Json(message);
            }
            using (TransactionScope scope = new TransactionScope())
            {
                var s = _workbenchDbContext.RolesMenuItem.Where(c => c.MenuItemId == Id);
                foreach (var i in s)
                {
                    _workbenchDbContext.RolesMenuItem.Remove(i);
                }
                _workbenchDbContext.MenuItem.Remove(Item);
                _workbenchDbContext.SaveChanges();
                scope.Complete();
            }
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 根据菜单id获取按钮配置
        /// </summary>
        /// <param name="menuId"></param>
        /// <returns></returns>
        public IActionResult GetBottonByMenuId(long menuId)
        {
            ResponseMessage<dynamic> message = new();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            var query = _workbenchDbContext.MenuItem.Where(c => c.MenuId == menuId).ToList();
            message.Data = query;
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 根据appId获取按钮配置
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public IActionResult GetBottonByAppId(string appId)
        {
            ResponseMessage<dynamic> message = new();

            var query = _workbenchDbContext.MenuItem.Where(c => c.AppId == appId).ToList();
            message.Data = query;
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 根据roleId获取按钮配置
        /// </summary>
        /// <returns></returns>
        public IActionResult GetRolesMenuItemByRoleId(long roleId)
        {
            ResponseMessage<dynamic> message = new();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            var query = _workbenchDbContext.RolesMenuItem.Where(c => c.RoleId == roleId).ToList();
            message.Data = query;
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="RoleId"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public IActionResult GetRolesMenuItem(int RoleId, string appId)
        {
            ResponseMessage<dynamic> message = new();

            var menuItemList = _workbenchDbContext.MenuItem.Where(c => c.AppId == appId).ToList();
            ArrayList arrayList = new ArrayList();
            foreach (var item in menuItemList)
            {
                arrayList.Add(item.Id);
            }
            var query = _workbenchDbContext.RolesMenuItem.Where(c => c.RoleId == RoleId && arrayList.Contains(c.MenuItemId));

            message.Data = query.ToList();
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 给角色分配按钮权限
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IActionResult AssignBottonSave([FromBody] AssignReportMenuFilter filter)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            User user = _workbenchContext.User;
            //1、根据appid查有哪些menuItme
            var menuIdList = _workbenchDbContext.MenuItem.Where(c => c.AppId == filter.appId).ToList();
            ArrayList arrayList = new ArrayList();
            foreach (var item in menuIdList)
            {
                arrayList.Add(item.Id);
            }
            //2、根据RoleId和menuItem查询出所有记录
            var role = _workbenchDbContext.RolesMenuItem.Where(c => c.RoleId == filter.roleId && arrayList.Contains(c.MenuItemId)).ToList();
            using (TransactionScope scope = new TransactionScope())
            {
                //3、删除相关记录
                foreach (var r in role)
                {
                    _workbenchDbContext.RolesMenuItem.Remove(r);
                }
                //4、重新插入新记录
                foreach (var m in filter.menuId)
                {
                    ef.AccountContext.RolesMenuItem RolesMenuItem = new()
                    {
                        RoleId = filter.roleId,
                        MenuItemId = m,
                        IsRight = true,
                        CreateUser = user.Code,
                        CreateDate = DateTime.Now
                    };
                    _workbenchDbContext.RolesMenuItem.Add(RolesMenuItem);
                }
                _workbenchDbContext.SaveChanges();
                scope.Complete();
            }
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 启用控制权限
        /// </summary>
        /// <param name="Id">菜单id</param>
        /// <returns></returns>
        public IActionResult EnableMenuItem(int Id)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            var menuItem = _workbenchDbContext.MenuItem.FirstOrDefault(p => p.Id == Id);
            if (menuItem == null)
            {
                message.ErrorMessage = "功能不存在!";
                return Json(message);
            }
            if (menuItem.ControlPrivilege)
            {
                message.ErrorMessage = "功能已启用不能重复启用!";
                return Json(message);
            }
            User user = _workbenchContext.User;
            menuItem.ControlPrivilege = true;
            menuItem.EnableUser = user.Code;
            menuItem.EnableDate = DateTime.Now;
            _workbenchDbContext.SaveChanges();
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 禁用功能
        /// </summary>
        /// <param name="Id">菜单id</param>
        /// <returns></returns>
        public IActionResult ForbiddenMenuItem(int Id)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            var menuItem = _workbenchDbContext.MenuItem.FirstOrDefault(p => p.Id == Id);
            if (menuItem == null)
            {
                message.ErrorMessage = "功能不存在!";
                return Json(message);
            }
            if (!menuItem.ControlPrivilege)
            {
                message.ErrorMessage = "功能已禁用不能重复禁用!";
                return Json(message);
            }
            menuItem.ControlPrivilege = false;
            User user = _workbenchContext.User;
            menuItem.EnableUser = "";
            menuItem.EnableDate = null;
            _workbenchDbContext.SaveChanges();
            message.Code = ResponseCode.Success;
            return Json(message);
        }




        #endregion

        #region 用户管理

        /// <summary>
        /// 获取部门详情
        /// </summary>
        /// <returns></returns>
        public IActionResult GetDepartment()
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();

            var list = new List<ef.AccountContext.Department>();
            //string mymoooCompany = GetCurrentMymoooCompany();

            var query = list.Union(_workbenchDbContext.Department.Where(p => !p.IsDelete).ToList()).OrderBy(o => o.DepartmentId).GroupBy(c => c.AppId);

            //var query = _workbenchDbContext.Department.Where(c=>c.AppId== "DGweixinwork").ToList().OrderBy(o => o.DepartmentId);
            //foreach (var item in query)
            //{
            //    int idIndex = 1;
            //    foreach (var department in item)
            //    {
            //        department.Id = department.DepartmentId;
            //        idIndex++;
            //    }
            //}

            message.Data = query.ToList();
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        public IActionResult GetDepartmentByCrm()
        {

            var query = _workbenchDbContext.Department.Where(p => !p.IsDelete).ToList();
            return Json(query);
        }

        public IActionResult GetMainDepartMentByUserId(long id)
        {
            ResponseMessage<string> message = new ResponseMessage<string>();
            var query = from a in _workbenchDbContext.User
                        join b in _workbenchDbContext.Department on a.MainDepartmentId equals b.DepartmentId into b1
                        from b2 in b1.DefaultIfEmpty()
                        where !a.IsDelete && a.Status == 1 && !b2.IsDelete && a.Id == id
                        select b2.Name;
            message.Code = ResponseCode.Success;
            message.Data = query.FirstOrDefault();
            return Json(message);
        }

        /// <summary>
        /// 获取部门成员详情
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IActionResult GetDepartmentUserList([FromBody] PageRequest<DepartmentUserFilter> filter)
        {
            ResponseMessage<PageResponse<dynamic>> message = new ResponseMessage<PageResponse<dynamic>>();

            if (filter == null)
            {
                filter = new PageRequest<DepartmentUserFilter>();
            }
            if (filter.Filter == null)
            {
                filter.Filter = new DepartmentUserFilter() { DepartmentIdList = new List<long>() };
            }
            IQueryable<DepartmentListResponse.Userlist> query = null;
            if (filter.Filter.DepartmentIdList.Count < 1)
            {
                query = from u in _workbenchDbContext.User
                        where u.IsDelete == false && u.Status == 1
                        select new DepartmentListResponse.Userlist
                        {
                            email = u.Email,
                            gender = u.Gender,
                            mobile = u.Mobile,
                            name = u.Name,
                            position = u.Position,
                            userid = u.UserId,
                            id = u.Id,
                            isAssistant = _workbenchDbContext.UserPosition.Where(c => c.UserId == u.Id).Include(c => c.Position).Any(o => o.Position.IsAssistant != null && o.Position.IsAssistant == true && !o.Position.IsForbidden),
                            IsPurchaser = _workbenchDbContext.UserPosition.Where(c => c.UserId == u.Id).Include(c => c.Position).Any(o => "purchaser" == o.Position.Code && !o.Position.IsForbidden),
                            IsManager = _workbenchDbContext.UserPosition.Where(c => c.UserId == u.Id).Include(c => c.Position).Any(o => "manager" == o.Position.Code && !o.Position.IsForbidden)
                        };
            }
            else
            {

                for (int i = 0; i < filter.Filter.DepartmentIdList.Count; i++)
                {
                    List<long> selectDepartmentIdChild = new List<long>();
                    List<long> temptree = new List<long>();
                    IQueryable<DepartmentListResponse.Userlist> userlist = null;
                    long id = filter.Filter.DepartmentIdList[i];
                    string mymoooCompany = filter.Filter.MymoooCompany[i];
                    if (string.IsNullOrWhiteSpace(mymoooCompany))
                    {
                        mymoooCompany = _workbenchContext.User.MymoooCompany;
                    }
                    var departmentIdList = _workbenchDbContext.Department.Where(c => c.AppId == mymoooCompany);
                    //根据ID和系统码拿部门ID
                    var departmentId = departmentIdList.Where(c => c.Id == id).FirstOrDefault().DepartmentId;
                    //递归查询所有子部门
                    //tree.Clear();
                    SelectDepartmentIdChild(departmentIdList, departmentId, temptree, out selectDepartmentIdChild);

                    //如果不存在子部门则查询当前部门
                    //if (selectDepartmentIdChild.Count() < 1)
                    //{
                    //    var selectDepartment = departmentIdList.Where(c => c.DepartmentId == departmentId).FirstOrDefault();
                    //    selectDepartmentIdChild.Add(selectDepartment.Id);
                    //}
                    var selectDepartment = departmentIdList.Where(c => c.DepartmentId == departmentId).FirstOrDefault();
                    selectDepartmentIdChild.Add(selectDepartment.Id);
                    //查询用户信息
                    userlist = _workbenchDbContext.DepartmentUser.Where(p => selectDepartmentIdChild.Contains(p.DepartmentId))
                        .Join(_workbenchDbContext.User.Where(w => w.IsDelete == false && w.Status == 1), d => d.UserId, c => c.Id, (d, c) => new DepartmentListResponse.Userlist
                        {
                            email = c.Email,
                            gender = c.Gender,
                            mobile = c.Mobile,
                            name = c.Name,
                            position = c.Position,
                            userid = c.UserId,
                            id = c.Id,
                            isAssistant = _workbenchDbContext.UserPosition.Where(up => up.UserId == c.Id).Include(c => c.Position).Any(o => o.Position.IsAssistant != null && o.Position.IsAssistant == true && !o.Position.IsForbidden),
                            IsPurchaser = _workbenchDbContext.UserPosition.Where(up => up.UserId == c.Id).Include(c => c.Position).Any(o => "purchaser" == o.Position.Code && !o.Position.IsForbidden),
                            IsManager = _workbenchDbContext.UserPosition.Where(up => up.UserId == c.Id).Include(c => c.Position).Any(o => "manager" == o.Position.Code && !o.Position.IsForbidden)
                        }).Distinct();
                    if (userlist == null)
                    {
                        message.Code = ResponseCode.NoExistsData;
                        return Json(message);
                    }
                    if (i == 0)
                    {
                        query = userlist;
                    }
                    else
                    {
                        query = query.Union(userlist);
                    }
                }
            }

            if (filter.Filter.UserId != 0)
            {
                query = query.Where(c => c.id == filter.Filter.UserId);
            }
            if (filter.Filter.IsAssistant != null)
            {
                query = query.Where(c => c.isAssistant == filter.Filter.IsAssistant);
            }
            if (filter.Filter.IsPurchaser != null)
            {
                query = query.Where(c => c.IsPurchaser == filter.Filter.IsPurchaser);
            }
            if (filter.Filter.IsManager != null)
            {
                query = query.Where(c => c.IsManager == filter.Filter.IsManager);
            }
            if (filter.Filter.Post != 0)
            {
                query = query.Where(c => _workbenchDbContext.UserPosition.Where(up => up.UserId == c.id).Include(c => c.Position).Any(o => o.Position.Id == filter.Filter.Post && !o.Position.IsForbidden));
            }
            if (!string.IsNullOrWhiteSpace(filter.Filter.Position))
            {
                query = query.Where(c => c.position == filter.Filter.Position);
            }
            var total = query.Count();
            message.Data = new PageResponse<dynamic>(filter.PageIndex, filter.PageSize, total);
            //计算需要跳过是数量
            var skip = (filter.PageIndex - 1) * filter.PageSize;
            if (total <= skip)
            {
                //总数小于跳过的数量,直接返回
                message.Code = ResponseCode.Success;
                return Json(message);
            }
            var result = query.Skip(skip).Take(filter.PageSize).ToList();
            result.ForEach(c =>
                message.Data.Rows.Add(new
                {
                    c.userid,
                    c.id,
                    c.name,
                    c.department,
                    c.position,
                    c.mobile,
                    c.gender,
                    c.email,
                    c.isAssistant,
                    c.IsPurchaser,
                    c.IsManager
                })
            );
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        private void SelectDepartmentIdChild(IQueryable<ef.AccountContext.Department> department, long pid, List<long> temptree, out List<long> tree)
        {
            var children = department.Where(p => p.ParentId == pid).ToList();
            if (children.Count() > 0)
            {
                foreach (var child in children)
                {
                    SelectDepartmentIdChild(department, child.DepartmentId, temptree, out tree);
                    temptree.Add(child.Id);
                }
            }
            tree = temptree;
        }

        /// <summary>
        /// 根据员工获取最上层的部门
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public IActionResult GetUserTopDepartMent(long Id)
        {
            ResponseMessage<string> message = new ResponseMessage<string>();
            string sql = string.Format(@"with temp as (
                                        select a.Name,a.ParentId,a.DepartmentId,1 inx from Department a
                                        left join MymoooUser b on a.DepartmentId = b.MainDepartmentId
                                        where b.id=@Id and b.AppId='weixinwork' and b.IsDelete = 0 and b.Status =1 and a.AppId='weixinwork' and a.IsDelete = 0
                                        union all
                                        select a.Name,a.ParentId,a.DepartmentId,temp.inx+1 from Department a inner join temp on a.DepartmentId = temp.ParentId
                                        where a.IsDelete = 0 and a.AppId='weixinwork'
                                        )
                                        select top 1 * from temp where ParentId!=0 order by inx desc");
            var conn = _workbenchDbContext.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            using (var cmd = conn.CreateCommand())
            {

                var param = cmd.CreateParameter();
                param.ParameterName = "@Id";
                param.DbType = DbType.Int64;
                param.Value = Id;
                cmd.Parameters.Add(param);

                cmd.CommandText = sql;
                using (var reader = cmd.ExecuteReader())
                {

                    if (reader.Read())
                    {
                        message.Data = reader["Name"].ToString();
                    }
                    if (conn.State != ConnectionState.Closed)
                    {
                        conn.Close();
                    }
                    message.Code = ResponseCode.Success;
                    return Json(message);

                }
            }
        }

        public IActionResult GetUsersByDepatMent([FromBody] PageRequest<DepartmentUserFilter> filter)
        {
            ResponseMessage<PageResponse<dynamic>> message = new ResponseMessage<PageResponse<dynamic>>();
            string sql = string.Format(@"with temp as (select * from Department where Id=@DepartId and IsDelete =0 
                union  all
                select t.* 
                from temp
	                inner join Department t on temp.DepartmentId = t.ParentId and t.IsDelete=0 and temp.AppId=t.AppId )
                select * from (
                select a.UserId,a.Id,a.Name,Position,Mobile,Gender,Email,
                sum(case when p.IsAssistant = 1 then 1 else 0 end) as IsAssistant,
                sum(case when p.Code = 'purchaser' then 1 else 0 end) as IsPurchaser,
                sum(case when p.Code = 'manager' then 1 else 0 end) as IsManager,
                isnull(pu.IsLeaderInDepartment, 0) as IsLeaderInDept 
                from MymoooUser a
                 left join DepartmentUser pu on a.Id =pu.UserId and pu.DepartmentId = @DepartId
	                left join UserPosition up on a.Id = up.UserId
	                left join Position p on up.PositionId = p.Id and p.IsForbidden = 0
                where a.IsDelete = 0 and a.Status=1 and exists (select 1 from temp inner join DepartmentUser du on temp.Id = du.DepartmentId where a.Id = du.UserId)
                {0} 
                group by a.UserId,a.Id,a.Name,Position,Mobile,Gender,Email,isnull(pu.IsLeaderInDepartment, 0)
                ) a
                where 1=1 {1} {2} {3} {4} {5}
                 order by IsLeaderInDept desc
offset ((@pageIndex-1)*@pageSize) rows
fetch next @pageSize rows only ", filter.Filter.Post == 0 ? "" : " and p.Id=@Post",
   filter.Filter.IsAssistant == null ? "" : " and IsAssistant=@IsAssistant",
   filter.Filter.IsPurchaser == null ? "" : " and IsPurchaser=@IsPurchaser",
   filter.Filter.IsManager == null ? "" : " and IsManager=@IsManager",
   string.IsNullOrWhiteSpace(filter.Filter.Position) ? "" : "and Position=@Position",
   string.IsNullOrWhiteSpace(filter.Filter.UserName) ? "" : " and Name=@UserName");


            string countSql = string.Format(@"with temp as (select * from Department where Id=@DepartId and IsDelete =0 
                union  all
                select t.* 
                from temp
	                inner join Department t on temp.DepartmentId = t.ParentId and t.IsDelete=0 and temp.AppId=t.AppId )
                select count(*) from (
                select a.UserId,a.Id,a.Name,Position,Mobile,Gender,Email,
                sum(case when p.IsAssistant = 1 then 1 else 0 end) as IsAssistant,
                sum(case when p.Code = 'purchaser' then 1 else 0 end) as IsPurchaser,
                sum(case when p.Code = 'manager' then 1 else 0 end) as IsManager,
                isnull(pu.IsLeaderInDepartment, 0) as IsLeaderInDept 
                from MymoooUser a
                 left join DepartmentUser pu on a.Id =pu.UserId and pu.DepartmentId = @DepartId
	                left join UserPosition up on a.Id = up.UserId
	                left join Position p on up.PositionId = p.Id and p.IsForbidden = 0
                where a.IsDelete = 0 and a.Status=1 and exists (select 1 from temp inner join DepartmentUser du on temp.Id = du.DepartmentId where a.Id = du.UserId)
                {0} 
                group by a.UserId,a.Id,a.Name,Position,Mobile,Gender,Email,isnull(pu.IsLeaderInDepartment, 0)
                ) a
                where 1=1 {1} {2} {3} {4} {5} ", filter.Filter.Post == 0 ? "" : " and p.Id=@Post",
  filter.Filter.IsAssistant == null ? "" : " and IsAssistant=@IsAssistant",
  filter.Filter.IsPurchaser == null ? "" : " and IsPurchaser=@IsPurchaser",
  filter.Filter.IsManager == null ? "" : " and IsManager=@IsManager",
  string.IsNullOrWhiteSpace(filter.Filter.Position) ? "" : "and Position=@Position",
  string.IsNullOrWhiteSpace(filter.Filter.UserName) ? "" : " and Name=@UserName");

            var conn = _workbenchDbContext.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            using (var cmd = conn.CreateCommand())
            {

                if (filter.Filter.Post != 0)
                {
                    var parm1 = cmd.CreateParameter();
                    parm1.ParameterName = "@Post";
                    parm1.DbType = DbType.Int64;
                    parm1.Value = filter.Filter.Post;
                    cmd.Parameters.Add(parm1);
                }

                if (filter.Filter.IsAssistant != null)
                {
                    var parm2 = cmd.CreateParameter();
                    parm2.ParameterName = "@IsAssistant";
                    parm2.DbType = DbType.Boolean;
                    parm2.Value = filter.Filter.IsAssistant;
                    cmd.Parameters.Add(parm2);
                }

                if (filter.Filter.IsPurchaser != null)
                {

                    var parm3 = cmd.CreateParameter();
                    parm3.ParameterName = "@IsPurchaser";
                    parm3.DbType = DbType.Boolean;
                    parm3.Value = filter.Filter.IsPurchaser;
                    cmd.Parameters.Add(parm3);
                }
                if (filter.Filter.IsManager != null)
                {
                    var parm4 = cmd.CreateParameter();
                    parm4.ParameterName = "@IsManager";
                    parm4.DbType = DbType.Boolean;
                    parm4.Value = filter.Filter.IsManager;
                    cmd.Parameters.Add(parm4);
                }

                if (!string.IsNullOrWhiteSpace(filter.Filter.Position))
                {
                    var parm5 = cmd.CreateParameter();
                    parm5.ParameterName = "@Position";
                    parm5.DbType = DbType.String;
                    parm5.Value = filter.Filter.Position;
                    cmd.Parameters.Add(parm5);
                }

                if (!string.IsNullOrWhiteSpace(filter.Filter.UserName))
                {
                    var parm6 = cmd.CreateParameter();
                    parm6.ParameterName = "@UserName";
                    parm6.DbType = DbType.String;
                    parm6.Value = filter.Filter.UserName;
                    cmd.Parameters.Add(parm6);
                }

                if (filter.Filter.DepartmentIdList.Count > 0)
                {
                    var parm7 = cmd.CreateParameter();
                    parm7.ParameterName = "@DepartId";
                    parm7.DbType = DbType.Int64;
                    parm7.Value = filter.Filter.DepartmentIdList.FirstOrDefault();
                    cmd.Parameters.Add(parm7);
                }
                var param8 = cmd.CreateParameter();
                param8.ParameterName = "@pageIndex";
                param8.DbType = DbType.Int32;
                param8.Value = filter.PageIndex;
                cmd.Parameters.Add(param8);

                var param9 = cmd.CreateParameter();
                param9.ParameterName = "@pageSize";
                param9.DbType = DbType.Int32;
                param9.Value = filter.PageSize;
                cmd.Parameters.Add(param9);

                cmd.CommandText = countSql;

                int total = int.Parse(cmd.ExecuteScalar().ToString());
                message.Data = new PageResponse<dynamic>(filter.PageIndex, filter.PageSize, total);
                cmd.CommandText = sql;
                using (var reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        message.Data.Rows.Add(new
                        {
                            Userid = reader["UserId"].ToString(),
                            Id = Convert.ToUInt64(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Position = reader["Position"].ToString(),
                            Mobile = reader["Mobile"].ToString(),
                            Gender = reader["Gender"].ToString(),
                            Email = reader["Email"].ToString(),
                            isAssistant = Convert.ToBoolean(reader["IsAssistant"]),
                            IsPurchaser = Convert.ToBoolean(reader["IsPurchaser"]),
                            IsManager = Convert.ToBoolean(reader["IsManager"]),
                            IsLeaderInDept = Convert.ToBoolean(reader["IsLeaderInDept"])
                        });
                    }
                }
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
                message.Code = ResponseCode.Success;
                return Json(message);

            }
        }

        public IActionResult GetDeptByUser([FromBody] DepartmentUserFilter filter)
        {
            ResponseMessage<List<dynamic>> message = new ResponseMessage<List<dynamic>>();
            string sql = string.Format(@"with temp as(
            select d.ParentId,d.Name,d.AppId,de.UserId,d.id as DeptId,d.DepartmentId,case when d.DepartmentId=u.MainDepartmentId then 1 else 0 end IsMainDepartMent, 
			ROW_NUMBER() OVER(ORDER BY ParentId)  as number,lev = 0
			from Department d 
			inner join DepartmentUser de on d.Id=de.DepartmentId
			left join MymoooUser as u on de.UserId=u.Id
            where de.UserId=@UserId and d.IsDelete=0
            union all
            select t.ParentId,t.Name,t.AppId,temp.UserId,temp.DeptId,t.DepartmentId,temp.IsMainDepartMent,temp.number,lev = temp.lev - 1 from temp
			inner join Department t on temp.ParentId=t.DepartmentId and t.AppId=temp.AppId and t.IsDelete=0
            )
            select 
                    stuff((select '→'+Name from  temp  
                        where c.number=number  order by lev
                        for xml path('')),1,1,'') as Name,d.IsLeaderInDepartment,c.IsMainDepartMent
             from  temp  c  
			   inner join DepartmentUser d on c.UserId=d.UserId and c.DeptId=d.DepartmentId
             group by c.number,c.IsMainDepartMent,d.IsLeaderInDepartment");
            var conn = _workbenchDbContext.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            using (var cmd = conn.CreateCommand())
            {


                var param8 = cmd.CreateParameter();
                param8.ParameterName = "@UserId";
                param8.DbType = DbType.Int64;
                param8.Value = filter.UserId;
                cmd.Parameters.Add(param8);
                cmd.CommandText = sql;
                message.Data = new List<dynamic>();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        message.Data.Add(new
                        {
                            Name = reader["Name"].ToString(),
                            IsLeaderInDepartment = Convert.ToInt32(reader["IsLeaderInDepartment"]),
                            IsMainDepartMent = Convert.ToInt32(reader["IsMainDepartMent"])
                        });
                    }
                }
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
                message.Code = ResponseCode.Success;
                return Json(message);

            }
        }

        /// <summary>
        /// 获取所有下级
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IActionResult Getsubordinate(long userId)
        {
            ResponseMessage<List<long>> message = new ResponseMessage<List<long>>();
            message.Data = new List<long>();
            string sql = @$"with temp as
                        (
                        select DepartmentId,ParentId,Id from Department where IsDelete =0 and  exists (
                        select 1 from DepartmentUser d where Department.Id = d.DepartmentId and exists(
                        select 1 from MymoooUser c where id= @Id
                         and d.UserId = c.Id and c.IsDelete =0
                        ) and IsLeaderInDepartment =1
                        )
                        union all
                        select a.DepartmentId,a.ParentId,a.Id from Department a inner join temp  on a.ParentId = temp.DepartmentId and IsDelete = 0
                        )
                        select distinct m.Id,m.Name from temp
                        left join DepartmentUser d on temp.Id = d.DepartmentId
                        left join MymoooUser m on d.UserId = m.Id
                        where m.IsDelete =0
                        union
                        select id,name from MymoooUser where id =@Id";
            var conn = _workbenchDbContext.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            using (var cmd = conn.CreateCommand())
            {
                var parameter = cmd.CreateParameter();
                parameter.ParameterName = "@Id";
                parameter.DbType = DbType.Int64;
                parameter.Value = userId;
                cmd.Parameters.Add(parameter);

                cmd.CommandText = sql;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        message.Data.Add(Convert.ToInt64(reader["Id"]));
                    }
                }
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
                message.Code = ResponseCode.Success;
                return Json(message);
            }
        }

        public IActionResult CheckIsSaleMan(string name)
        {
            var re = from a in _workbenchDbContext.User
                     join b in _workbenchDbContext.UserRoles on a.Id equals b.UserId into b1
                     from b2 in b1.DefaultIfEmpty()
                     join c in _workbenchDbContext.Roles on b2.RoleId equals c.Id into c1
                     from c2 in c1.DefaultIfEmpty()
                     where c2.Code == "salesman" && a.Name == name
                     select a.Id;
            ResponseMessage<long> message = new ResponseMessage<long>();
            var result = re.FirstOrDefault();
            message.Data = result;
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        public IActionResult GetDeptByUserByCrm([FromBody] List<string> listCode)
        {
            ResponseMessage<List<DeptByCodeDto>> message = new ResponseMessage<List<DeptByCodeDto>>();
            var result = from a in _workbenchDbContext.User.Where(it => listCode.Any(a => a == it.UserId))
                         join b in _workbenchDbContext.DepartmentUser on a.Id equals b.UserId into b1
                         from b2 in b1.DefaultIfEmpty()
                         join c in _workbenchDbContext.Department on b2.DepartmentId equals c.Id into c1
                         from c2 in c1.DefaultIfEmpty()
                         select new DeptByCodeDto
                         {
                             DeptName = c2.Name,
                             UserCode = a.UserId
                         };
            message.Data = result.ToList();
            message.Code = ResponseCode.Success;
            return Json(message);
        }
        /// <summary>
        /// 获取职位列表
        /// </summary>
        /// <returns></returns>
        public IActionResult GetWeiXinPositionList()
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            var query = _workbenchDbContext.User.Where(c => c.Position != "").ToList().GroupBy(r => r.Position).Select(r => new
            {
                Key = r.First().Position,
                Value = r.First().Position
            });
            message.Data = query;
            message.Code = ResponseCode.Success;
            return Json(message);
        }
        /// <summary>
        /// 模糊查询员工信息
        /// </summary>
        /// <returns></returns>
        public ActionResult FuzzyQuery(string code, int count = 5, bool isAssistant = false, bool isMymo = false)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            List<dynamic> result = new List<dynamic>();
            string mymoooCompany = _workbenchContext.User.MymoooCompany;
            var userData = _workbenchDbContext.User.Where(c => (c.UserId.Contains(code) || c.Name.Contains(code)) && c.Status == 1 && c.IsDelete == false && (!isMymo || c.AppId == "weixinwork"));
            if (isAssistant)
            {
                userData = userData.Join(_workbenchDbContext.UserPosition.Include(c => c.Position).Where(o => o.Position.IsAssistant != null && o.Position.IsAssistant == true && !o.Position.IsForbidden), u => u.Id, up => up.UserId, (u, up) => u);
            }
            foreach (var item in userData.Take(count).ToList())
            {
                result.Add(new
                {
                    Code = item.UserId.ToString(),
                    Name = item.Name.ToString(),
                    UserId = item.Id,
                    MymoooCompany = item.AppId
                });
            }
            message.Data = result;
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        private void SelectDepartmentIdChild(IQueryable<ef.AccountContext.Department> department, long pid, List<long> temptree, out List<long> tree, string mymoooCompany)
        {
            var children = department.Where(p => p.ParentId == pid && !p.IsDelete && p.AppId == mymoooCompany).ToList();
            if (children.Count() > 0)
            {
                foreach (var child in children)
                {
                    SelectDepartmentIdChild(department, child.DepartmentId, temptree, out tree, mymoooCompany);
                    temptree.Add(child.Id);
                }
            }
            tree = temptree;
        }
        #endregion

        #region 系统参数
        /// <summary>
        /// 获取系统参数
        /// </summary>
        /// <returns></returns>
        public IActionResult GetSystemParam()
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();

            var query = _workbenchDbContext.SystemParam.AsQueryable().ToList();
            message.Data = query;
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 新增系统参数
        /// </summary>
        /// <param name="systemParam"></param>
        /// <returns></returns>
        public IActionResult InsertSystemParam([FromBody] ef.AccountContext.SystemParam systemParam)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (this.TryValidateModel(systemParam))
            {
                if (!_workbenchContext.User.IsAdmin)
                {
                    message.Code = ResponseCode.PrivilegeReject;
                    message.ErrorMessage = "当前用户没有权限!";
                    return Json(message);
                }
                var query = _workbenchDbContext.SystemParam.FirstOrDefault(p => p.SystemParamKey == systemParam.SystemParamKey);
                if (query != null)
                {
                    message.ErrorMessage = "系统参数key不能重复!";
                    return Json(message);
                }
                _workbenchDbContext.SystemParam.Add(systemParam);
                _workbenchDbContext.SaveChanges();
                message.Data = systemParam;
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
        /// 修改系统参数
        /// </summary>
        /// <param name="systemParam"></param>
        /// <returns></returns>
        public IActionResult UpdateSystemParam([FromBody] ef.AccountContext.SystemParam systemParam)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (this.TryValidateModel(systemParam))
            {
                if (!_workbenchContext.User.IsAdmin)
                {
                    message.Code = ResponseCode.PrivilegeReject;
                    message.ErrorMessage = "当前用户没有权限!";
                    return Json(message);
                }
                var query = _workbenchDbContext.SystemParam.FirstOrDefault(p => p.SystemParamKey == systemParam.SystemParamKey && p.Id != systemParam.Id);
                if (query != null)
                {
                    message.ErrorMessage = "系统参数key不能重复!";
                    return Json(message);
                }
                var old = _workbenchDbContext.SystemParam.FirstOrDefault(p => p.Id == systemParam.Id);
                old.GroupId = systemParam.GroupId;
                old.SystemParamKey = systemParam.SystemParamKey;
                old.SystemParamValue = systemParam.SystemParamValue;
                old.SystemParamDesc = systemParam.SystemParamDesc;
                systemParam = old;
                _workbenchDbContext.SaveChanges();
                message.Data = systemParam;
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
        /// 删除系统参数
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public IActionResult DeleteSystemParam(int Id)
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            if (!_workbenchContext.User.IsAdmin)
            {
                message.Code = ResponseCode.PrivilegeReject;
                message.ErrorMessage = "当前用户没有权限!";
                return Json(message);
            }
            var s1 = from s in _workbenchDbContext.SystemParam
                     where s.Id == Id
                     select s;
            _workbenchDbContext.SystemParam.Remove(s1.FirstOrDefault());
            _workbenchDbContext.SaveChanges();
            message.Code = ResponseCode.Success;
            return Json(message);
        }
        #endregion

        /// <summary>
        /// 配置审批流界面获取全部用户信息  忽略下响应body日志
        /// </summary>
        /// <returns></returns>
        [IgnoreLog(ignoreResponseBody: true)]
        public IActionResult GetUserList()
        {
            var userList = _workbenchDbContext.User.Where(c => c.Status == 1 && c.AppId == "weixinwork").ToList();
			ResponseMessage<List<ef.AccountContext.User>> message = new()
			{
				Data = userList,
				Code = ResponseCode.Success
			};
			return Json(message);
        }

        public IActionResult GetUserListByCrm()
        {
            var userList = _workbenchDbContext.User.Where(it => it.Status == 1 && it.AppId == "weixinwork").ToList();
			ResponseMessage<List<ef.AccountContext.User>> message = new()
			{
				Data = userList,
				Code = ResponseCode.Success
			};
			return Json(message);
        }

        public IActionResult GetPaymentMethod()
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            var mymoooCompany = _workbenchContext.User.MymoooCompany;
            var platformAdmin = _workbenchContext.ApigatewayConfig.EnvCode.Equals("production", StringComparison.OrdinalIgnoreCase) ? "platformAdmin" : _workbenchContext.ApigatewayConfig.EnvCode + "platformAdmin";
            var applicationConfig = _workbenchDbContext.ThirdpartyApplicationConfig.Include(c => c.ThirdpartyApplicationDetail).FirstOrDefault(c => c.AppId == platformAdmin);
            //var applicationConfig = _workbenchDbContext.ThirdpartyApplicationConfig.Include(c => c.ThirdpartyApplicationDetail).FirstOrDefault(c => c.AppId == "testplatformAdmin");
            var result = _httpUtils.SignatureInvokeGetPlatformWebService(applicationConfig.Token, applicationConfig.Nonce, applicationConfig.Url + "Company/GetPaymentMethodList?salesCode=''");
            message.Code = ResponseCode.Success;
            message.Data = result;
            return Json(message);
        }

        /// <summary>
        /// 获取深圳蚂蚁所有用户的编码和部门名称
        /// </summary>
        /// <returns></returns>
        public IActionResult GetUserAndDepartmentList()
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            var query = _workbenchDbContext.User.Where(c => c.Status == 1 && !c.IsDelete && c.AppId == "weixinwork")
                .Join(_workbenchDbContext.DepartmentUser, u => u.Id, du => du.UserId, (u, du) => du)
                .Join(_workbenchDbContext.Department, du => du.DepartmentId, d => d.Id, (du, d) => new
                {
                    DepartmentName = d.Name,
                    WeChatCode = du.User.UserId
                }).ToList();
            message.Data = query;
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 获取用户与部门的对应关系
        /// </summary>
        /// <returns></returns>
        public IActionResult GetUserAndDept()
        {
            ResponseMessage<List<UserAndDeptDto>> response = new ResponseMessage<List<UserAndDeptDto>>();
            var query = from a in _workbenchDbContext.DepartmentUser
                        join b in _workbenchDbContext.User on a.UserId equals b.Id into b1
                        from b2 in b1.DefaultIfEmpty()
                        join c in _workbenchDbContext.Department on a.DepartmentId equals c.Id into c1
                        from c2 in c1.DefaultIfEmpty()
                        where !b2.IsDelete && b2.Status == 1 && !c2.IsDelete && b2.AppId == "weixinwork"
                        select new UserAndDeptDto
                        {
                            DepatId = c2.DepartmentId,
                            DeptName = c2.Name,
                            UserCode = b2.UserId,
                            UserName = b2.Name,
                            UserId = b2.Id,
                            IsMain = c2.DepartmentId == b2.MainDepartmentId
                        };
            response.Data = [.. query];
            response.Code = ResponseCode.Success;
            return Json(response);
        }

        /// <summary>
        /// 获取深圳蚂蚁所有用户的基本信息和部门名称,包括禁用的, 此方法用于填充扩展信息数据
        /// </summary>
        /// <returns></returns>
        public IActionResult GetAllUserAndDepartmentList()
        {
            ResponseMessage<dynamic> message = new ResponseMessage<dynamic>();
            var query = _workbenchDbContext.User.Where(c => c.AppId == "weixinwork")
                .Join(_workbenchDbContext.DepartmentUser, u => u.Id, du => du.UserId, (u, du) => du)
                .Join(_workbenchDbContext.Department, du => du.DepartmentId, d => d.Id, (du, d) => new
                {
                    UserId = du.UserId,
                    UserName = du.User.Name,
                    Position = du.User.Position,
                    DepartmentName = d.Name,
                    WeChatCode = du.User.UserId
                }).ToList();
            message.Data = query;
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 获取深圳蚂蚁所有可用的业务助理
        /// </summary>
        /// <returns></returns>
        public IActionResult GetAllSalesAssistantList()
        {
            ResponseMessage<dynamic> message = new();

            var subQuery1 = _workbenchDbContext.Position
                .Where(p => p.Code == "YWZL")
                .Select(p => p.Id)
                .FirstOrDefault(); // Subquery result  

            var subQuery2 = _workbenchDbContext.UserPosition
                .Where(x => x.PositionId == subQuery1)
                .Select(x => x.UserId)
                .ToList(); // 执行查询并获取结果列表


            var query = _workbenchDbContext.User.Where(c => c.Status == 1 && !c.IsDelete && c.AppId == "weixinwork" && subQuery2.Contains(c.Id))
                .Join(_workbenchDbContext.DepartmentUser, u => u.Id, du => du.UserId, (u, du) => du)
                .Join(_workbenchDbContext.Department, du => du.DepartmentId, d => d.Id, (du, d) => new
                {
                    du.UserId,
                    UserName = du.User.Name,
                    du.User.Position,
                    DepartmentName = d.Name,
                    WeChatCode = du.User.UserId
                })
                .Distinct()
                .ToList();
            message.Data = query;
            message.Code = ResponseCode.Success;
            return Json(message);
        }
        public IActionResult GetMobileById(long Id)
        {
            var user = _workbenchContext.User;
            var re = _workbenchDbContext.SystemParam.Where(it => it.SystemParamKey == "System_QueryTimes");
            int queryNums = 6;
            if (re.Any())
            {
                queryNums = Convert.ToInt32(re.FirstOrDefault().SystemParamValue);
            }
            ResponseMessage<dynamic> message = new();
            var query = _workbenchDbContext.User.Where(c => c.Status == 1 && !c.IsDelete && c.AppId == "weixinwork" && c.Id == Id).Select(it => new
            {
                it.Mobile,
                it.Name
            }).ToList();
            int nums = 0;
            if (query.Count != 0)
            {
                nums = _workbenchDbContext.QueryMobileHistories.Where(it => it.QueryByName == user.Name && it.CreateTime.Date == DateTime.Now.Date).Select(it => it.Mobile).Distinct().Count();
                if (nums >= queryNums)
                {
                    if (!_workbenchDbContext.QueryMobileHistories.Where(it => it.Mobile == query.FirstOrDefault().Mobile && it.QueryByName == user.Name && it.CreateTime.Date == DateTime.Now.Date).Any())
                    {
                        message.Code = ResponseCode.UpperLimit;
                        message.Message = "超过每日查询次数上限,请联系行政";
                        message.Data = null;
                        return Json(message);
                    }

                }
                foreach (var item in query)
                {
                    ef.AccountContext.QueryMobileHistory queryMobileHistory = new()
                    {
                        Name = item.Name,
                        Mobile = item.Mobile,
                        QueryByName = user.Name,
                        CreateTime = DateTime.Now
                    };
                    _workbenchDbContext.QueryMobileHistories.Add(queryMobileHistory);
                    _workbenchDbContext.SaveChanges();
                }
            }
            message.Code = ResponseCode.Success;
            message.Data = query;
            return Json(message);
        }

        public IActionResult QueryMoblieTimes([FromBody] PageRequest<QueryMobileFilter> request)
        {

            string name = string.Empty;
            if (request != null && request.Filter != null)
            {
                name = _workbenchDbContext.User.FirstOrDefault(it => it.Id == request.Filter.Id)?.Name;
            }
            DateTime? start = null;
            DateTime? end = null;


            if (request != null && request.Filter != null && request.Filter.StartDate != null)
            {
                start = ((DateTime)request.Filter.StartDate).Date;
            }

            if (request != null && request.Filter != null && request.Filter.EndDate != null)
            {

                end = ((DateTime)request.Filter.EndDate?.AddDays(1)).Date;
            }

            ResponseMessage<PageResponse<KeyValuePair<string, int>>> message = new ResponseMessage<PageResponse<KeyValuePair<string, int>>>();
            var result = _workbenchDbContext.QueryMobileHistories.Where(it => (string.IsNullOrWhiteSpace(name) || it.QueryByName == name) && (start == null || it.CreateTime >= start) && (end == null || it.CreateTime < end)).GroupBy(it => it.QueryByName).OrderByDescending(it => it.Count()).Select(
                 it => new KeyValuePair<string, int>(it.Key, it.Count()));
            var total = result.Count();
            message.Data = new PageResponse<KeyValuePair<string, int>>(request.PageIndex, request.PageSize, total);
            var skip = (request.PageIndex - 1) * request.PageSize;
            if (total <= skip)
            {
                message.Code = ResponseCode.Success;
                return Json(message);
            }
            message.Code = ResponseCode.Success;
            message.Data.Rows = result.Skip(skip).Take(request.PageSize).ToList();
            return Json(message);

        }

        public IActionResult QueryMobileHistory([FromBody] PageRequest<QueryMobileFilter> request)
        {
            DateTime? start = null;
            DateTime? end = null;
            if (request.Filter.StartDate != null)
            {
                start = ((DateTime)request.Filter.StartDate).Date;
            }

            if (request.Filter.EndDate != null)
            {
                end = ((DateTime)request.Filter.EndDate?.AddDays(1)).Date;
            }

            ResponseMessage<PageResponse<dynamic>> message = new ResponseMessage<PageResponse<dynamic>>();
            var result = from a in _workbenchDbContext.QueryMobileHistories.Where(it => it.QueryByName == request.Filter.Name && (start == null || it.CreateTime >= start) && (end == null || it.CreateTime < end))
                         join b in _workbenchDbContext.User on a.Name equals b.Name into b1
                         from b2 in b1.DefaultIfEmpty()
                         join c in _workbenchDbContext.Department on b2.MainDepartmentId equals c.DepartmentId into c1
                         from c2 in c1.DefaultIfEmpty()
                         where b2.AppId == "weixinwork" && c2.AppId == "weixinwork"
                         orderby a.CreateTime descending
                         select new
                         {
                             a.Name,
                             DeptName = c2.Name,
                             a.Mobile,
                             a.QueryByName,
                             a.CreateTime
                         };
            int total = result.Count();
            message.Data = new PageResponse<dynamic>(request.PageIndex, request.PageSize, total);
            var skip = (request.PageIndex - 1) * request.PageSize;
            if (total <= skip)
            {
                message.Code = ResponseCode.Success;
                return Json(message);
            }
            result.Skip(skip).Take(request.PageSize).ToList().ForEach(it => message.Data.Rows.Add(it));
            message.Code = ResponseCode.Success;
            return Json(message);
        }

        /// <summary>
        /// 获取所有销售员和销售主管
        /// </summary>
        /// <param name="userCode">当前登录用户，管理员和业务数据员可以查看所有业务员，主管只能看自己和部门内</param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public IActionResult GetSalesmanList(string userCode, string userName)
        {
            var response = new ResponseMessage<List<AuthorityUser>>();
            //var userList = _workbenchDbContext.UserRoles.Where(e => e.RoleId == 2 || e.RoleId == 40)
            //    .Join(_workbenchDbContext.User.Where(e=> e.IsDelete && e.Status==1 && e.AppId== "weixinwork" && !string.IsNullOrEmpty(userName)?e.Name.Contains(userName):true ), a => a.UserId, b => b.Id, (a, b) => 
            //    new AuthorityUser {
            //        UserId=b.Id, 
            //        UserCode =b.UserId,
            //        UserName=b.Name
            //    }).ToList();

            var _userList = from a in _workbenchDbContext.UserRoles.Where(e => e.RoleId == 2 || e.RoleId == 40)
                            join b in _workbenchDbContext.User.Where(e => !string.IsNullOrEmpty(userName) ? e.Name.Contains(userName) : true) on a.UserId equals b.Id
                            where !b.IsDelete && b.Status == 1 && b.AppId == "weixinwork"
                            select new AuthorityUser
                            {
                                UserId = b.Id,
                                UserCode = b.UserId,
                                UserName = b.Name
                            };
            var userList = _userList.ToList();
            if (!CheckUserRole(userCode, new List<string>() { "admin", "BusinessData" }))
            {
                //数据权限限制
                var user = _workbenchDbContext.User.FirstOrDefault(e => e.UserId == userCode && !e.IsDelete && e.AppId == "weixinwork" && e.Status==1);
                List<AuthorityUser> authorityUserList = _tokenService.GetAuthorityUserList(new User() { UserId = user.Id });
                if (authorityUserList != null)
                {
                    var userIdList = authorityUserList.Select(e => e.UserId).ToList();
                    response.Data = userList.Where(e => userIdList.Contains(e.UserId)).Distinct().OrderBy(it => it.UserCode).ToList();
                }
            }
            else
            {
                //管理员和业务数据源可以查看所有数据
                response.Data = userList.Distinct().OrderBy(it => it.UserCode).ToList();
            }
            return Json(response);
        }

        /// <summary>
        /// 验证用户是否有某个角色
        /// </summary>
        /// <returns></returns>
        private bool CheckUserRole(string userCode, List<string> roleCode)
        {
            var user = _workbenchDbContext.User.FirstOrDefault(e => e.UserId == userCode && !e.IsDelete && e.AppId=="weixinwork" && e.Status == 1);
            var roles = _workbenchDbContext.Roles.Where(e => roleCode.Contains(e.Code)).Select(e => e.Id).ToList();
            if (user != null && roles != null)
            {
                return _workbenchDbContext.UserRoles.Any(e => e.UserId == user.Id && roles.Contains(e.RoleId));
            }
            else
            {
                return false;
            }
        }


        public async Task<IActionResult> SynchronizeToCapp()
        {
            var response = new ResponseMessage<List<CappSyncUserResponse>>
            {
                Code = ResponseCode.Success
            };
            long versionId = DateTime.Now.Ticks;
            var userList = _workbenchDbContext.User.Where(r => !r.IsDelete).Select(r => new CappSyncUserResponse
            { UserId = r.UserId, Name = r.Name, Mobile = r.Mobile, Email = r.Email, Telephone = r.Telephone, Address = r.Address, Versions = versionId }
            ).ToList();
            //.Skip(200).Take(5)
            //   var res = apigatewayUtils.InvokeWebServicePost("pdm", $"/api/user/wechat-user-sync", JsonConvert.SerializeObject(userList));


            //Console.WriteLine(res);

            var result = await _rabbitMQServiceClient.SendEnvMessage<WorkbenchContext, User>(_workbenchContext, "workbench_Addressbook_ToCapp_", JsonSerializerOptionsUtils.Serialize(userList));
            if (result.IsSuccess)
            {
                response.Data = userList;
                response.Message = "同步通讯录到CAPP成功";
                return Json(response);
            }
            return Json(result);
        }




        /// <summary>
        /// 获取部门和成员到MES
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<IActionResult> SynchronizeToMes()
        {
            ResponseMessage<string> message = new ResponseMessage<string>();
            string mymoooCompany = "weixinwork"; 
            var allDeptList = _workbenchDbContext.Department.Where(c => c.AppId == mymoooCompany && !c.IsDelete).ToList();
            var userListDepart = _workbenchDbContext.DepartmentUser.ToList();
            DateTime startDate = new DateTime(2024, 9, 10);
            // 获取 2024,9,10 日在职,且 2024年9-10日后有变更的员工
            var userList = _workbenchDbContext.User.Where(u => (!u.IsDelete && u.UpdateTime == null)
               || u.UpdateTime != null && u.UpdateTime > startDate).ToList();


            List<DeptInfoSync> allDeptSync = [];
            foreach (var d in allDeptList)
            {
                DeptInfoSync dS = new DeptInfoSync()
                {
                    DeptId = d.DepartmentId,
                    DeptName = d.Name,
                    ParentId = d.ParentId,
                };
                allDeptSync.Add(dS);
            }
            // 人, 部门
            List<UserSync> allUserList = [];
            foreach (var u in userList)
            {
                UserSync uS = new UserSync()
                {
                    Address = u.Address,
                    Alias = u.Alias,
                    Email = u.Email,
                    EntryDate = u.EntryDate,
                    Gender = u.Gender,
                    IsDelete = u.IsDelete,
                    Mobile = u.Mobile,
                    UserName = u.Name,
                    Position = u.Position,
                    Telephone = u.Telephone,
                    UserId = u.UserId
                };
                var mId = u.MainDepartmentId;
                var uDeptList = userListDepart.Where(c => c.UserId == u.Id).Select(r => r.DepartmentId).ToList();
                var dDeptList = allDeptList.Where(a => !a.IsDelete && uDeptList.Contains(a.Id)).ToList();
                List<DeptSync> uD = [];
                foreach (var d in dDeptList)
                {
                    DeptSync deptSync = new DeptSync()
                    {
                        DeptId = d.DepartmentId,
                        ParentId = d.ParentId,
                        DeptName = d.Name
                    };
                    if (mId == d.DepartmentId)
                    {
                        deptSync.IsMain = true;
                    }

                    uD.Add(deptSync);
                }
                uS.DeptList = uD;
                allUserList.Add(uS);
            }


            DeptAndUserSyncResponse deptAndUserSyncResponse = new DeptAndUserSyncResponse()
            {
                 AllDept = allDeptSync,
                 UserList = allUserList
            };

            var result = await _rabbitMQServiceClient.SendEnvMessage<WorkbenchContext, User>(_workbenchContext, "workbench_Addressbook_ToMes_", JsonSerializerOptionsUtils.Serialize(deptAndUserSyncResponse));            
            if (result.IsSuccess)
            {
                message.Data = "ok";
                message.Code = ResponseCode.Success;
            }
            else
            {
                message.Data = "fail:" + result.ErrorMessage;
                message.Code = ResponseCode.Exception;
            }

            return Json(message);
        }



        private void ClearPrivilegeCache(string WechatCode)
        {
            if (_workbenchContext.ApigatewayConfig.EnvCode.Equals("production", StringComparison.OrdinalIgnoreCase))
            {
                _httpUtils.InvokePostWebService($"https://admin.mymooo.com/SalesOrder/RemoveCache?WechatCode={WechatCode}", "", "text/html");
            }
        }

        /// <summary>
        ///  按角色,获取所有的产品工程师列表
        /// </summary>
        /// <returns></returns>
        public IActionResult GetAllEngineerList()
        {
            ResponseMessage<dynamic> message = new();

            string querySql = @" Select distinct mu.UserId code,mu.Name name From UserRoles as ur
Left Join MymoooUser mu on ur.UserId = mu.Id
Where RoleId = (Select top 1 Id From Roles Where Code = 'PM')
  And Status = 1 Order By mu.UserId"
            ;
            var result = _workbenchContext.SqlSugar.Ado.SqlQuery<dynamic> (querySql).ToList();
            message.Code = ResponseCode.Success;
            message.Data = result;
            return Json(message);
        }

        

    }
}

