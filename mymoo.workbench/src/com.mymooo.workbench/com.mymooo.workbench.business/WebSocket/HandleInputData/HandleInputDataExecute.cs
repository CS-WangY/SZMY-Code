using com.mymooo.workbench.core;
using com.mymooo.workbench.core.Account;
using com.mymooo.workbench.core.SystemManage.Request;
using com.mymooo.workbench.core.Utils;
using com.mymooo.workbench.core.WebSocket.HandleData;
using com.mymooo.workbench.ef;
using com.mymooo.workbench.weixin.work.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Utils.JsonConverter;
using mymooo.weixinWork.SDK.AddressBook.Model;
using mymooo.weixinWork.SDK.Application;
using mymooo.weixinWork.SDK.Application.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace com.mymooo.workbench.business.WebSocket.HandleInputData
{
    [AutoInject(InJectType.Scope)]
    public class HandleInputDataExecute(WeixinDbContext weixinDbContext, WorkbenchContext workbenchContext, IOptions<K3CloudConfig> k3CloudConfig, WeixinWorkUtils weixinWorkUtils
        , ApplicationServiceClient applicationServiceClient, ILogger<HandleInputDataExecute> logger)
    {
        private readonly WeixinDbContext _weixinDbContext = weixinDbContext;
        private readonly WorkbenchContext _workbenchContext = workbenchContext;
        private readonly K3CloudConfig _k3CloudConfig = k3CloudConfig.Value;
        private readonly WeixinWorkUtils _weixinWorkUtils = weixinWorkUtils;
        private readonly ApplicationServiceClient _applicationServiceClient = applicationServiceClient;
        private readonly ILogger<HandleInputDataExecute> _logger = logger;

        public async Task HandleSynchronizeing(HubCallerContext context, Action<HandleDataCallBack<dynamic>> syncAddressBookCallBack = null)
        {
            var callback = new HandleDataCallBack<dynamic>();
            if (_workbenchContext.RedisCache.Lock<DepartmentListResponse>(new TimeSpan(0, 10, 0)))
            {
                using var tans = _weixinDbContext.Database.BeginTransaction();
                try
                {
                    ResponseMessage<DepartmentListResponse> result = null;
                    int Process = 0;
                    //将时间戳作为版本号
                    var versions = DateTime.Now.Ticks;
                    var applicationConfigList = _weixinDbContext.ThirdpartyApplicationConfig.Include(c => c.ThirdpartyApplicationDetail).Where(c => c.IsWeiXinWork).ToList();
                    foreach (var applicationConfig in applicationConfigList)
                    {
                        result = await _applicationServiceClient.GetDepartmentList<WorkbenchContext, User>(_workbenchContext);
                        if (!result.IsSuccess || result.Data.Errcode > 0)
                        {
                            callback.IsEnd = true;
                            callback.ErrorMessage = result.Data.ErrorMessage;
                            callback.Code = ResponseCode.Exception;
                            _workbenchContext.RedisCache.RemoveLock<DepartmentListResponse>();
                            return;
                        }
                        double count = 0, deptCount = result.Data.Department.Length;
                        if (deptCount == 0)
                        {
                            Process += 10;
                            callback.Progress = Process;
                            callback.Code = ResponseCode.Success;
                            syncAddressBookCallBack?.Invoke(callback);
                        }
                        //对返回的数据添加系统号，区分来源
                        foreach (var item in result.Data.Department)
                        {
                            var department = _weixinDbContext.Department.FirstOrDefault(c => c.AppId == applicationConfig.AppId && c.DepartmentId == item.Id);
                            if (department == null)
                            {
                                department = new ef.AccountContext.Department();
                                department.FunctionAttr = "";
                                _weixinDbContext.Department.Add(department);
                            }
                            department.AppId = applicationConfig.AppId;
                            department.DepartmentId = item.Id;
                            department.Name = item.Name;
                            department.NameEn = item.NameEn + "";
                            department.Order = item.Order;
                            department.ParentId = item.ParentId;
                            department.Versions = versions;
                            department.IsDelete = false;
                            count++;
                            if (count % 5 == 0 || count == deptCount)
                            {
                                if (count == deptCount)
                                {
                                    Process += 10;
                                    callback.Progress = Process;
                                }
                                else
                                {
                                    callback.Progress = Process + Convert.ToInt32(count / deptCount * 10);
                                }
                                callback.Code = ResponseCode.Success;
                                syncAddressBookCallBack?.Invoke(callback);
                            }
                        }
                        _weixinDbContext.SaveChanges();
                        var departmentResponse = await _applicationServiceClient.GetDepartmentUsers<WorkbenchContext, User>(_workbenchContext);
                        if (!departmentResponse.IsSuccess || departmentResponse.Data.Errcode > 0)
                        {
                            callback.IsEnd = true;
                            callback.ErrorMessage = result.Data.ErrorMessage;
                            callback.Code = ResponseCode.Exception;
                            _workbenchContext.RedisCache.RemoveLock<DepartmentListResponse>();
                            return;
                        }
                        double count2 = 0, usercount = departmentResponse.Data.UserList.Length;
                        if (usercount == 0)
                        {
                            Process += 37;
                            callback.Progress = Process;
                            callback.Code = ResponseCode.Success;
                            syncAddressBookCallBack?.Invoke(callback);
                        }
                        foreach (var userInfo in departmentResponse.Data.UserList)
                        {
                            var user = _weixinDbContext.User.Include(c => c.DepartmentUsers).FirstOrDefault(c => c.AppId == applicationConfig.AppId && c.UserId == userInfo.UserId);
                            if (user == null)
                            {
                                user = new ef.AccountContext.User();
                                _weixinDbContext.User.Add(user);
                            }
                            else
                            {
                                _weixinDbContext.DepartmentUser.RemoveRange(user.DepartmentUsers);
                                _weixinDbContext.SaveChanges();
                            }
                            user.Address = userInfo.Address + "";
                            user.Alias = userInfo.Alias + "";
                            user.AppId = applicationConfig.AppId;
                            user.Email = userInfo.Email;
                            user.Biz_Email = userInfo.Biz_Email;
                            user.ExternalPosition = userInfo.ExternalPosition + "";
                            user.Gender = userInfo.Gender;
                            user.MainDepartmentId = userInfo.Main_department;
                            user.Mobile = userInfo.Mobile;
                            user.Status = userInfo.Status;
                            user.Telephone = userInfo.Telephone;
                            user.QrCode = userInfo.QrCode + "";
                            user.UserId = userInfo.UserId;
                            user.Name = userInfo.Name;
                            user.OpenUserid = userInfo.OpenUserId + "";
                            user.Position = userInfo.Position + "";
                            user.Versions = versions;
                            user.DirectSupervisor = userInfo.DirectLeader?.FirstOrDefault();
                            user.Grade = GetExtattrValue<string>(userInfo.Extattr, "职级");
                            user.Education = GetExtattrValue<string>(userInfo.Extattr, "学历");
                            user.EntryDate = GetExtattrValue<DateTime?>(userInfo.Extattr, "入职时间");

                            user.IsDelete = false;
                            for (int i = 0; i < userInfo.Department.Length; i++)
                            {
                                var dept = _weixinDbContext.Department.FirstOrDefault(c => c.AppId == applicationConfig.AppId && c.DepartmentId == userInfo.Department[i]);
                                ef.AccountContext.DepartmentUser departmentUser = new()
                                {
                                    IsLeaderInDepartment = userInfo.IsLeaderInDepartment[i],
                                    Order = userInfo.Order[i],
                                    DepartmentId = dept.Id
                                };
                                user.DepartmentUsers.Add(departmentUser);
                            }
                            _weixinDbContext.SaveChanges();
                            count2++;
                            if (count2 % 5 == 0 || count2 == usercount)
                            {
                                if (count2 == usercount)
                                {
                                    Process += 37;
                                    callback.Progress = Process;
                                }
                                else
                                {
                                    callback.Progress = Process + Convert.ToInt32(count2 / usercount * 37); ;
                                }
                                callback.Code = ResponseCode.Success;
                                syncAddressBookCallBack?.Invoke(callback);
                            }
                        }
                    }
                    //将不是此时间戳的部门用户标记为删除
                    var depts = _weixinDbContext.Department.Where(c => c.Versions != versions).ToList();
                    double count3 = 0, deptscount = depts.Count;
                    if (deptscount == 0)
                    {
                        Process += 3;
                        callback.Progress = Process;
                        callback.Code = ResponseCode.Success;
                        syncAddressBookCallBack?.Invoke(callback);
                    }
                    foreach (var dept in depts)
                    {
                        dept.IsDelete = true;
                        count3++;
                        if (count3 % 5 == 0 || count3 == deptscount)
                        {
                            if (count3 == deptscount)
                            {
                                Process += 3;
                                callback.Progress = Process;
                            }
                            else
                            {
                                callback.Progress = Process + Convert.ToInt32(count3 / deptscount * 3); ;
                            }
                            callback.Code = ResponseCode.Success;
                            syncAddressBookCallBack?.Invoke(callback);
                        }
                    }

                    var users = _weixinDbContext.User.Where(c => c.Versions != versions).ToList();
                    count3 = 0;
                    deptscount = users.Count;
                    if (deptscount == 0)
                    {
                        Process += 3;
                        callback.Progress = Process;
                        callback.Code = ResponseCode.Success;
                        syncAddressBookCallBack?.Invoke(callback);
                    }
                    foreach (var user in users)
                    {
                        user.IsDelete = true;
                        user.Status = 5;
                        count3++;
                        if (count3 % 5 == 0 || count3 == deptscount)
                        {
                            if (count3 == deptscount)
                            {
                                Process += 3;
                                callback.Progress = Process;
                            }
                            else
                            {
                                callback.Progress = Process + Convert.ToInt32(count3 / deptscount * 3); ;
                            }
                            callback.Code = ResponseCode.Success;
                            syncAddressBookCallBack?.Invoke(callback);
                        }
                    }
                    _weixinDbContext.SaveChanges();
                    tans.Commit();
                    List<WXUserRequest> wXUsers = _weixinDbContext.User.Where(it => it.AppId == "weixinwork" && !it.IsDelete && it.Status == 1).Select(it => new WXUserRequest
                    {
                        UserId = it.UserId,
                        Address = it.Address,
                        Email = it.Email,
                        Mobile = it.Mobile,
                        Name = it.Name,
                        Telephone = it.Telephone,
                        Versions = it.Versions
                    }).ToList();

                    var res = K3CloudUtils.K3cloudWebService("Kingdee.Mymooo.WebApi.ServicesStub.CustomerManagermentService.SysDeptAndUserNews.common.kdsvc", JsonConvert.SerializeObject(wXUsers), _k3CloudConfig);
                    _logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}金蝶同步通讯录接口:{JsonConvert.SerializeObject(wXUsers)}返回:|{res}|");
                    var reobject = JsonConvert.DeserializeObject<ResponseMessage<dynamic>>(res);

                    callback.IsEnd = true;
                    callback.Code = ResponseCode.Success;
                    syncAddressBookCallBack?.Invoke(callback);
                    _workbenchContext.RedisCache.RemoveLock<DepartmentListResponse>();
                }
                catch (Exception ex)
                {
                    tans.Rollback();
                    callback.IsEnd = true;
                    callback.ErrorMessage = $"同步通讯录出错:{ex.Message}";
                    callback.Code = ResponseCode.Exception;
                    _workbenchContext.RedisCache.RemoveLock<DepartmentListResponse>();
                    syncAddressBookCallBack?.Invoke(callback);
                }
            }
            else
            {
                callback.IsEnd = true;
                callback.ErrorMessage = "通讯录正在同步中,请稍后再试";
                callback.Code = ResponseCode.Exception;
                syncAddressBookCallBack?.Invoke(callback);
            }
        }

        /// <summary>
        /// 获取扩展属性
        /// </summary>
        /// <param name="dynamic"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private T GetExtattrValue<T>(UserInfoResponse.WeiXinUserExtattr dynamic, string name)
        {
            var result = default(T);
            try
            {
                foreach (var attr in dynamic.ExtattrDetails)
                {
                    if (attr.Name == name)
                    {
                        if (IsNullableType(typeof(T)))
                        {
                            return (T)new NullableConverter(typeof(T)).ConvertFromString(GetStrNoCh(attr.Value));
                        }
                        return (T)Convert.ChangeType(attr.Value, typeof(T));
                    }
                }
            }
            catch (Exception)
            {
            }
            return result;
            bool IsNullableType(Type theType)
            {
                return (theType.IsGenericType && theType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
            }
        }

        private static string GetStrNoCh(string strSource)
        {
            return Regex.Replace(strSource, @"[\u4e00-\u9fa5]", "");
        }
    }
}
