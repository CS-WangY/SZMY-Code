using com.mymooo.workbench.core.Account;
using com.mymooo.workbench.core.Utils;
using com.mymooo.workbench.ef;
using com.mymooo.workbench.ef.ThirdpartyApplication;
using Microsoft.EntityFrameworkCore;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.weixinWork.SDK.Application;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute
{
    /// <summary>
    /// 通讯录消息处理
    /// </summary>
    [AutoInject(InJectType.Scope)]
    public class AddressBookMessageExecute(WorkbenchContext workbenchContext, WorkbenchDbContext workbenchDbContext, ApplicationServiceClient applicationServiceClient)
    {
        private readonly WorkbenchContext _workbenchContext = workbenchContext;
        private readonly WorkbenchDbContext _workbenchDbContext = workbenchDbContext;
        private readonly ApplicationServiceClient _applicationServiceClient = applicationServiceClient;

        /// <summary>
        /// 通讯录消息处理
        /// </summary>
        /// <param name="message"></param>
        public async Task Execute(WeiXinMessage message)
        {
            dynamic messageInfo = XmlUtils.GetAnonymousType(message.Message);
            string mymoooCompany = message.ApplicationDetail.AppId;
            var versions = DateTime.Now.Ticks;

            if (messageInfo.MsgType.Equals("event", StringComparison.OrdinalIgnoreCase))
            {
                string operType = null;
                string operPartyOrUser = null;
                if (!string.IsNullOrWhiteSpace(messageInfo.ChangeType))
                {
                    string[] changeTypeArr = messageInfo.ChangeType.Split("_");
                    operType = changeTypeArr[0];
                    operPartyOrUser = changeTypeArr[1];
                }
                if (operPartyOrUser != null && "user".Equals(operPartyOrUser))
                {
                    string userId = messageInfo.UserID;
                    if (((System.Collections.Generic.IDictionary<String, Object>)messageInfo).ContainsKey("NewUserID"))
                    {
                        userId = messageInfo.NewUserID;
                    }
                    string selectUserId = messageInfo.UserID;
                    var user = _workbenchDbContext.User.Include(c => c.DepartmentUsers).FirstOrDefault(c => c.AppId == mymoooCompany && c.UserId == selectUserId);
                    if (user == null && operType == "update")
                    {
                        operType = "create";
                    }
                    if (user != null && operType == "create")
                    {
                        operType = "update";
                    }
                    switch (operType)
                    {
                        case "create":
                            if (user != null)
                            {
                                _workbenchDbContext.DepartmentUser.RemoveRange(user.DepartmentUsers);
                                _workbenchDbContext.SaveChanges();
                                await CreateOrUpdateUser(user, userId, mymoooCompany);
                            }
                            else
                            {
                                user = new ef.AccountContext.User();
                                await CreateOrUpdateUser(user, userId, mymoooCompany);
                                _workbenchDbContext.User.Add(user);
                            }
                            break;
                        case "update":
                            _workbenchDbContext.DepartmentUser.RemoveRange(user.DepartmentUsers);
                            _workbenchDbContext.SaveChanges();
                            await CreateOrUpdateUser(user, userId, mymoooCompany);
                            break;
                        case "delete":
                            if (user != null)
                            {
                                user.IsDelete = true;
                                user.Status = 5;
                                user.UpdateTime = DateTime.Now;
                            }
                            break;
                    }

                }
                if (operPartyOrUser != null && "party".Equals(operPartyOrUser))
                {
                    long departMentId = long.Parse(messageInfo.Id);
                    var departMent = _workbenchDbContext.Department.FirstOrDefault(c => c.AppId == mymoooCompany && c.DepartmentId == departMentId);
                    if (departMent == null && operType == "update")
                    {
                        operType = "create";
                    }
                    if (departMent != null && operType == "create")
                    {
                        operType = "update";
                    }
                    switch (operType)
                    {
                        case "create":
                            if (departMent != null)
                            {
                                _workbenchDbContext.Department.Remove(departMent);
                                _workbenchDbContext.SaveChanges();
                            }
                            departMent = new ef.AccountContext.Department();
                            await CreateOrUpdateDepartment(departMent, departMentId, mymoooCompany);
                            _workbenchDbContext.Department.Add(departMent);
                            break;
                        case "update":
                            departMent ??= new ef.AccountContext.Department();
                            await CreateOrUpdateDepartment(departMent, departMentId, mymoooCompany);
                            break;
                        case "delete":
                            if (departMent != null)
                            {
                                departMent.IsDelete = true;
                            }
                            break;
                    }
                }
                _workbenchDbContext.SaveChanges();
            }
            message.IsComplete = true;
            message.CompleteDate = DateTime.Now;
            message.Result = "";
            message.StackTrace = "";
        }

        private async Task CreateOrUpdateUser(ef.AccountContext.User user, string userId, string mymoooCompany)
        {
            var versions = DateTime.Now.Ticks;
            var userData = await _applicationServiceClient.GetUserInfo<WorkbenchContext, User>(_workbenchContext, userId);
            if (userData.IsSuccess && userData.Data.Errcode == 0)
            {
                user.Address = userData.Data.Address + "";
                user.Alias = userData.Data.Alias + "";
                user.AppId = mymoooCompany;
                user.Email = userData.Data.Email + "";
                user.ExternalPosition = userData.Data.ExternalPosition + "";
                user.Gender = userData.Data.Gender;
                user.MainDepartmentId = userData.Data.Main_department;
                user.Mobile = userData.Data.Mobile + "";
                user.Status = userData.Data.Status;
                user.Telephone = userData.Data.Telephone + "";
                user.QrCode = userData.Data.QrCode + "";
                user.UserId = userData.Data.UserId;
                user.Name = userData.Data.Name;
                user.OpenUserid = userData.Data.OpenUserId + "";
                user.Position = userData.Data.Position + "";
                user.Versions = versions;
                user.UpdateTime = DateTime.Now;

                user.DirectSupervisor = userData.Data.DirectLeader?.FirstOrDefault();
                user.Grade = GetExtattrValue<string>(userData.Data.Extattr, "职级");
                user.Education = GetExtattrValue<string>(userData.Data.Extattr, "学历");
                DateTime date = GetExtattrValue<DateTime>(userData.Data.Extattr, "入职时间");
                if (date == default)
                {
                    user.EntryDate = null;
                }
                else
                {
                    user.EntryDate = date;
                }
                user.IsDelete = false;
                for (int i = 0; i < userData.Data.Department.Length; i++)
                {
                    var dept = _workbenchDbContext.Department.FirstOrDefault(c => c.AppId == mymoooCompany && c.DepartmentId == userData.Data.Department[i]);
                    if (dept != null)
                    {
                        ef.AccountContext.DepartmentUser departmentUser = new()
                        {
                            IsLeaderInDepartment = userData.Data.IsLeaderInDepartment[i],
                            Order = userData.Data.Order[i],
                            DepartmentId = dept.Id
                        };
                        user.DepartmentUsers.Add(departmentUser);
                    }
                }
            }
        }

        /// <summary>
        /// 获取扩展属性
        /// </summary>
        /// <param name="dynamic"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private T GetExtattrValue<T>(dynamic dynamic, string name)
        {
            var result = default(T);
            try
            {
                foreach (var attr in dynamic.attrs)
                {
                    if (attr.name.Value == name)
                    {
                        return (T)Convert.ChangeType(attr.value.Value, typeof(T));
                    }
                }
            }
            catch (Exception)
            {
            }
            return result;
        }

        private async Task CreateOrUpdateDepartment(ef.AccountContext.Department department, long departMentId, string mymoooCompany)
        {
            var versions = DateTime.Now.Ticks;
            var departmentData = await _applicationServiceClient.GetDepartmentList<WorkbenchContext, User>(_workbenchContext, departMentId);
            if (departmentData.IsSuccess && departmentData.Data.Errcode == 0)
            {
                foreach (var item in departmentData.Data.Department)
                {
                    department.AppId = mymoooCompany;
                    department.DepartmentId = item.Id;
                    department.Name = item.Name;
                    department.NameEn = item.NameEn + "";
                    department.Order = item.Order;
                    department.ParentId = item.ParentId;
                    department.Versions = versions;
                    department.IsDelete = false;
                }
            }
        }
    }
}
