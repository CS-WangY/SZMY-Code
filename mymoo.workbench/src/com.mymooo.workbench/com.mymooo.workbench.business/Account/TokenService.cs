using com.mymooo.workbench.core.SystemManage;
using com.mymooo.workbench.ef;
using com.mymooo.workbench.weixin.work.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Attributes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace com.mymooo.workbench.business.Account
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="workbenchDbContext"></param>
    /// <param name="weixinWorkUtils"></param>
    [AutoInject(InJectType.Scope)]
	public class TokenService(WorkbenchDbContext workbenchDbContext, WeixinWorkUtils weixinWorkUtils, IConfiguration configuration)
	{
		private readonly WorkbenchDbContext _workbenchDbContext = workbenchDbContext;
		private readonly WeixinWorkUtils _weixinWorkUtils = weixinWorkUtils;
		private readonly IConfiguration _configuration = configuration;

		public ResponseMessage<User> GetUserInfo(string userCode, string mymoooCompany = "weixinwork")
		{
			ResponseMessage<User> responseMessage = new ResponseMessage<User>();
			var employeInfo = _weixinWorkUtils.GetUserInfo(userCode, mymoooCompany);
			if (employeInfo == null || employeInfo.Errcode != 0)
			{
				responseMessage.Code = ResponseCode.NoExistsData;
				responseMessage.ErrorMessage = "不存在该用户信息";
				return responseMessage;
			}
			else if (employeInfo.Status != 1)
			{
				responseMessage.Code = ResponseCode.UserStatusException;
				responseMessage.ErrorMessage = "用户状态不是激活状态";
				return responseMessage;
			}
			else
			{
				//判断用户是否是管理员
				var role = _workbenchDbContext.UserRoles.Include(p => p.Role).FirstOrDefault(p => p.UserId == employeInfo.Id && p.Role.IsAdmin);
				// 判断是否是备案专员
				var keeponAttache = _workbenchDbContext.UserRoles.Include(p => p.Role).FirstOrDefault(p => p.UserId == employeInfo.Id && p.Role.Code == "BusinessData");
				//产品总监,拥有供应链端,供应商全部权限
				var productInspector = _workbenchDbContext.UserRoles.Include(p => p.Role).FirstOrDefault(p => p.UserId == employeInfo.Id && p.Role.Code == "ProductInspector");

				var user = new User()
				{
					Code = employeInfo.UserId,
					Name = employeInfo.Name,
					Email = employeInfo.Email,
					Mobile = employeInfo.Mobile,
					Address = employeInfo.Address,
					MainDepartment = employeInfo.Main_department,
					ExternalPosition = employeInfo.ExternalPosition,
					Gender = employeInfo.Gender,
					OpenUserId = employeInfo.OpenUserId,
					QrCode = employeInfo.QrCode,
					MymoooCompany = mymoooCompany,
					IsAdmin = role != null,
					IsKeeponAttache = keeponAttache != null,
					IsProductInspector = productInspector != null,
					UserId = employeInfo.Id
				};
				responseMessage.Code = ResponseCode.Success;
				responseMessage.Data = user;
				return responseMessage;
			}
		}

		public ResponseMessage<User> VerifyAndLogin(string token)
		{
			ResponseMessage<User> message = new();
			var result = Verify(token);
			if (result.Code == ResponseCode.Success)
			{
				//调用企业微信,查询用户信息
				var employeInfo = _weixinWorkUtils.GetUserInfo(result.Data.UserCode, result.Data.MymoooCompany);
				//判断用户是否是管理员
				var role = _workbenchDbContext.UserRoles.Include(p => p.Role).FirstOrDefault(p => p.UserId == employeInfo.Id && p.Role.IsAdmin);
				// 判断是否是备案专员
				var keeponAttache = _workbenchDbContext.UserRoles.Include(p => p.Role).FirstOrDefault(p => p.UserId == employeInfo.Id && p.Role.Code == "BusinessData");
				//产品总监,拥有供应链端,供应商全部权限
				var productInspector = _workbenchDbContext.UserRoles.Include(p => p.Role).FirstOrDefault(p => p.UserId == employeInfo.Id && p.Role.Code == "ProductInspector");

				if (employeInfo.Errcode != 0)
				{
					message.Code = ResponseCode.NoExistsData;
					message.ErrorMessage = employeInfo.ErrorMessage;
				}
				else if (employeInfo.Status != 1)
				{
					message.Code = ResponseCode.UserStatusException;
					message.ErrorMessage = "用户状态不是激活状态";
				}
				else
				{
					message.Data = new User()
					{
						Token = token,
						Code = employeInfo.UserId,
						Name = employeInfo.Name,
						Email = employeInfo.Email,
						Mobile = employeInfo.Mobile,
						Address = employeInfo.Address,
						MainDepartment = employeInfo.Main_department,
						ExternalPosition = employeInfo.ExternalPosition,
						Gender = employeInfo.Gender,
						OpenUserId = employeInfo.OpenUserId,
						QrCode = employeInfo.QrCode,
						AppId = result.Data.AppId,
						MymoooCompany = result.Data.MymoooCompany,
						IsAdmin = role != null,
						IsKeeponAttache = keeponAttache != null,
						IsProductInspector = productInspector != null,
						UserId = employeInfo.Id
					};
					message.Code = ResponseCode.Success;
				}
			}
			else
			{
				message.Code = result.Code;
				message.ErrorMessage = result.ErrorMessage;
			}
			return message;
		}

		public ResponseMessage<ef.AccountContext.UserToken> Verify(string token)
		{
			ResponseMessage<ef.AccountContext.UserToken> message = new ResponseMessage<ef.AccountContext.UserToken>();
			if (string.IsNullOrWhiteSpace(token))
			{
				message.Code = ResponseCode.TokenInvalid;
				message.ErrorMessage = "无效的token";
				return message;
			}
			var tokenInfo = _workbenchDbContext.UserToken.FirstOrDefault(c => c.Token == token.ToString());
			if (tokenInfo == null)
			{
				message.Code = ResponseCode.TokenInvalid;
				message.ErrorMessage = "无效的token";
				return message;
			}
			else if (tokenInfo.FailureDate < DateTime.Now)
			{
				message.Code = ResponseCode.TokenInvalid;
				message.ErrorMessage = "失效的token";
				return message;
			}
			else
			{
				tokenInfo.FailureDate = DateTime.Now.AddMinutes(tokenInfo.Validity);
				_workbenchDbContext.SaveChanges();
				message.Code = ResponseCode.Success;
				message.Data = tokenInfo;
			}
			return message;
		}

		public ResponseMessage<object> MenuItemPrivilege(User user, long menuId, string path)
		{
			ResponseMessage<object> message = new ResponseMessage<object>();
			//
			var meun = _workbenchDbContext.Menu.FirstOrDefault(c => c.Id == menuId);
			if (meun != null)
			{
				var meunItem = _workbenchDbContext.MenuItem.FirstOrDefault(c => c.MenuId == menuId && c.Path == path);
				if (meunItem == null)
				{
					meunItem = new ef.AccountContext.MenuItem()
					{
						MenuId = menuId,
						CreateUser = user.Name,
						ControlPrivilege = false,
						CreateDate = DateTime.Now,
						Path = path,
						Title = path,
						Description = ""
					};
					_workbenchDbContext.MenuItem.Add(meunItem);
					_workbenchDbContext.SaveChanges();
				}
				else
				{
					if (meunItem.ControlPrivilege)
					{
						//先验证一票否决
						var meunFunction = _workbenchDbContext.UserRoles.Where(u => u.UserId == user.UserId)
							.Join(_workbenchDbContext.RolesMenuItem, u => u.RoleId, ur => ur.RoleId, (u, ur) => ur)
							.Where(p => p.IsRight == false)
							.Join(_workbenchDbContext.MenuItem, u => u.MenuItemId, ur => ur.Id, (u, ur) => ur)
							.Where(o => o.MenuId == menuId && o.Path == path)
							.FirstOrDefault();

						if (meunFunction != null)
						{
							message.Code = ResponseCode.PrivilegeReject;
							message.ErrorMessage = $"{meunItem.Title}功能被一票否决,无权调用!";
							return message;
						}
						meunFunction = _workbenchDbContext.UserRoles.Where(u => u.UserId == user.UserId)
							.Join(_workbenchDbContext.RolesMenuItem, u => u.RoleId, ur => ur.RoleId, (u, ur) => ur)
							.Where(p => p.IsRight)
							.Join(_workbenchDbContext.MenuItem, u => u.MenuItemId, ur => ur.Id, (u, ur) => ur)
							.Where(o => o.MenuId == menuId && o.Path == path)
							.FirstOrDefault();

						if (meunFunction == null)
						{
							message.Code = ResponseCode.PrivilegeReject;
							message.ErrorMessage = $"用户无{meunItem.Title}权限!";
							return message;
						}
					}
				}
			}
			message.Code = ResponseCode.Success;
			return message;
		}

		public ResponseMessage<object> MenuItemPrivilege(User user, string path)
		{
			ResponseMessage<object> message = new();
			var meunItem = _workbenchDbContext.MenuItem.FirstOrDefault(c => c.Path == path);
			if (meunItem == null)
			{
				message.Code = ResponseCode.Success;
				return message;
			}
			else
			{
				if (meunItem.ControlPrivilege)
				{
					var meunFunction = _workbenchDbContext.UserRoles.Where(u => u.UserId == user.UserId)
						.Join(_workbenchDbContext.RolesMenuItem, u => u.RoleId, ur => ur.RoleId, (u, ur) => ur)
						.Where(p => p.IsRight == false)
						.Join(_workbenchDbContext.MenuItem, u => u.MenuItemId, ur => ur.Id, (u, ur) => ur)
						.Where(o => o.Path == path)
						.FirstOrDefault();

					if (meunFunction != null)
					{
						message.Code = ResponseCode.PrivilegeReject;
						message.ErrorMessage = $"{meunItem.Title}功能被一票否决,无权调用!";
						return message;
					}
					meunFunction = _workbenchDbContext.UserRoles.Where(u => u.UserId == user.UserId)
						.Join(_workbenchDbContext.RolesMenuItem, u => u.RoleId, ur => ur.RoleId, (u, ur) => ur)
						.Where(p => p.IsRight)
						.Join(_workbenchDbContext.MenuItem, u => u.MenuItemId, ur => ur.Id, (u, ur) => ur)
						.Where(o => o.Path == path)
						.FirstOrDefault();

					if (meunFunction == null)
					{
						message.Code = ResponseCode.PrivilegeReject;
						message.ErrorMessage = $"用户无{meunItem.Title}权限!";
						return message;
					}
				}
			}
			message.Code = ResponseCode.Success;
			return message;
		}

		/// <summary>
		/// 获取该用户有权限的用户
		/// </summary>
		/// <returns></returns>
		public List<AuthorityUser> GetAuthorityUserList(User user, ef.AccountContext.User otherUser = null)
		{
			long oUserId = user.UserId;
			string oUserWechatCode = user.Code;

			if (otherUser != null)
			{
				oUserId = otherUser.Id;
				oUserWechatCode = otherUser.UserId;
			}

			List<AuthorityUser> authorityUser = new();

			// 部门主管权限判断
			var sql = @"with dept as
(
	select d.Id,d.DepartmentId,d.ParentId
	from DepartmentUser du
		inner join Department d on du.DepartmentId = d.Id
	where UserId = @UserId and du.IsLeaderInDepartment = 1
	union all
	select d.Id,d.DepartmentId,d.ParentId
	from Department d
		inner join dept t on d.ParentId = t.DepartmentId
)

select u.Id,u.UserId
from MymoooUser u
where exists (select 1 from dept d inner join DepartmentUser du on d.Id = du.DepartmentId where u.Id = du.UserId)";
			using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("SqlServerConnection")))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand(sql, conn);
				cmd.Parameters.Add(new SqlParameter("@UserId", System.Data.SqlDbType.BigInt) { Value = oUserId });
				using (var dr = cmd.ExecuteReader())
				{
					while (dr.Read())
					{
						authorityUser.Add(new AuthorityUser
						{
							UserId = Convert.ToInt64(dr["Id"]),
							UserCode = Convert.ToString(dr["UserId"])
						});
					}
				}
			}
			// 助理权限判断
			var isAssistant = _workbenchDbContext.UserPosition.Include(c => c.Position).Any(o => o.UserId == oUserId && o.Position.IsAssistant == true && o.Position.IsForbidden == false);
			if (isAssistant)
			{
				var assistant = _workbenchDbContext.AssistantUsers.Where(p => p.AssistantId == oUserId)
							.Join(_workbenchDbContext.User, au => au.UserId, u => u.Id, (au, u) => new AuthorityUser
							{
								UserId = u.Id,
								UserCode = u.UserId,
								UserName = u.Name
							}).Distinct();
				authorityUser = authorityUser.Union(assistant).ToList();
			}
			if (!authorityUser.Any(c => c.UserId == oUserId))
			{
				authorityUser.Add(new AuthorityUser
				{
					UserId = oUserId,
					UserCode = oUserWechatCode
                });
			}
			return authorityUser;
		}


		public List<AuthorityUser> GetTeamUserList(string wechatCode, bool IncludeResignations)
        {
            List<AuthorityUser> authorityUser = new();
            // 1,判断是否是领导. 不是部门领导. 只返回个人资料

            List<AuthorityUserAndDept> UserAndDept = new List<AuthorityUserAndDept>();
            List<AuthorityUserAndDept> NextLevelLeaderUserAndDept = new List<AuthorityUserAndDept>();

            string Sql = @"Select mu.UserId UserCode, mu.Id UserId, mu.Name UserName, du.IsLeaderInDepartment,d.DepartmentId  From MymoooUser mu
	            Inner Join DepartmentUser du On  mu.Id = du.UserId
				Inner Join Department d ON du.DepartmentId = d.Id
				Where mu.UserId = @wechatCode";

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("SqlServerConnection")))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand(Sql, conn);
				cmd.Parameters.Add(new SqlParameter("@wechatCode", System.Data.SqlDbType.NVarChar) { Value = wechatCode });
				using (var dr = cmd.ExecuteReader())
				{
					while (dr.Read())
					{
						UserAndDept.Add(new AuthorityUserAndDept
						{
							UserId = Convert.ToInt64(dr["UserId"]),
							UserCode = Convert.ToString(dr["UserCode"]),
							UserName = Convert.ToString(dr["UserName"]),
							IsLeaderInDepartment = Convert.ToBoolean(dr["IsLeaderInDepartment"]),
							DepartmentId = Convert.ToInt64(dr["DepartmentId"])
						});
					}
				}
			}
            NextLevelLeaderUserAndDept = new List<AuthorityUserAndDept>(UserAndDept);
            // 2, 如果是领导, 获取自己和下属. 如果有最多循环3次
			for (int i = 0; i< 3 ;i++ )  // 上面已经查了一级, 向下再最多查3级. 总共4级组织结构.实际只有3级.
			{
                if (NextLevelLeaderUserAndDept.Where(r => r.IsLeaderInDepartment).Any())
                {
                    string deptIds = string.Join(",", NextLevelLeaderUserAndDept.Where(r => r.IsLeaderInDepartment).Select(r => r.DepartmentId).ToList());
                    NextLevelLeaderUserAndDept.Clear();

                    // 下属部门只获取,自己是领导的部门
                    Sql = @$"Select mu.UserId UserCode, mu.Id UserId, mu.Name UserName, du.IsLeaderInDepartment,d.DepartmentId From  DepartmentUser du
						LEFT Join Department d ON du.DepartmentId=d.Id
						Left Join  MymoooUser mu ON du.UserId = mu.Id
						Where d.IsDelete=0  And (du.DepartmentId in (Select Id From Department Where IsDelete=0 And DepartmentId in ({deptIds})) OR d.DepartmentId in (
						Select DepartmentId From Department Where IsDelete=0  And ParentId in ({deptIds})
						)) ";
                    if (!IncludeResignations)
                    {
                        Sql += " And mu.IsDelete = 0 ";
                    }

                    using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("SqlServerConnection")))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(Sql, conn);
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var item = new AuthorityUserAndDept
                                {
                                    UserId = Convert.ToInt64(dr["UserId"]),
                                    UserCode = Convert.ToString(dr["UserCode"]),
                                    UserName = Convert.ToString(dr["UserName"]),
                                    IsLeaderInDepartment = Convert.ToBoolean(dr["IsLeaderInDepartment"]),
                                    DepartmentId = Convert.ToInt64(dr["DepartmentId"])
                                };
								if (!UserAndDept.Where(r=>r.UserId == item.UserId).Any())
								{
									UserAndDept.Add(item);
									if (item.IsLeaderInDepartment)
									{
										NextLevelLeaderUserAndDept.Add(item);
									}
								}
                            }
                        }
                    }
                }
            }

            authorityUser = UserAndDept.Select(r => new AuthorityUser { UserCode = r.UserCode, UserId = r.UserId, UserName = r.UserName })
				.ToList().Distinct().ToList();
			 
            return authorityUser;
        }
    }

}
