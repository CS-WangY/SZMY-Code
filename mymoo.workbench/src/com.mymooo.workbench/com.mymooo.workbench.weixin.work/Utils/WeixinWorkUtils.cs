using com.mymooo.workbench.core.Account;
using com.mymooo.workbench.core.Minio;
using com.mymooo.workbench.core.SystemManage;
using com.mymooo.workbench.core.Utils;
using com.mymooo.workbench.core.WeiXinWork.Account;
using com.mymooo.workbench.ef;
using com.mymooo.workbench.ef.ThirdpartyApplication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Utils.JsonConverter;
using mymooo.weixinWork.SDK.Application;
using mymooo.weixinWork.SDK.Application.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace com.mymooo.workbench.weixin.work.Utils
{
    [AutoInject(InJectType.Scope)]
    public class WeixinWorkUtils(ILogger<WeixinWorkUtils> logger, HttpUtils httpUtils, IConfiguration configuration, MinioService minioService, WorkbenchContext workbenchContext, ApplicationServiceClient applicationServiceClient)
    {
        private readonly ILogger<WeixinWorkUtils> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly HttpUtils _httpUtils = httpUtils;
        private readonly MinioService _minioService = minioService;
        private readonly WorkbenchContext _workbenchContext = workbenchContext;
        private readonly ApplicationServiceClient _applicationServiceClient = applicationServiceClient;

        private void UseWework(WeixinDbContext weixinDbContext, ThirdpartyApplicationDetail applicationDetial, Action action)
        {
            if (string.IsNullOrWhiteSpace(applicationDetial.AccessToken) || applicationDetial.ExpiresTime < DateTime.Now)
            {
                GetAccessToken(weixinDbContext, applicationDetial);
            }

            action();
        }
        private WeixinDbContext GetDbContext()
        {
            var sqlConnection = _configuration.GetConnectionString("SqlServerConnection");
            DbContextOptions<WeixinDbContext> dbContextOption = new DbContextOptions<WeixinDbContext>();
            DbContextOptionsBuilder<WeixinDbContext> dbContextOptionBuilder = new DbContextOptionsBuilder<WeixinDbContext>(dbContextOption);
            WeixinDbContext workbenchDbContext = new(dbContextOptionBuilder.UseSqlServer(sqlConnection).Options);
            return workbenchDbContext;
        }

        private void GetAccessToken(WeixinDbContext weixinDbContext, ThirdpartyApplicationDetail applicationDetial)
        {
            string url = string.Format(applicationDetial.ThirdpartyApplicationConfig.Url + WeWorkPath.GetAccessToken, applicationDetial.ThirdpartyApplicationConfig.Token, applicationDetial.AppSecret);
            var result = _httpUtils.InvokeGetWebService<AccessTokenResponse>(url);
            if (result.errcode == 0)
            {
                applicationDetial.AccessToken = result.access_token;
                applicationDetial.ExpiresTime = DateTime.Now.AddSeconds(result.expires_in - 10);
                weixinDbContext.SaveChanges();
            }
            else
            {
                throw new Exception(result.errmsg);
            }
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="mymoooCompany"></param>
        /// <returns></returns>
        public UserInfoResponse GetUserInfo(string userId, string mymoooCompany = "weixinwork")
        {
            //从数据库获取
            var weixinDbContext = GetDbContext();
            var result = weixinDbContext.User.Where(c => c.UserId == userId && c.AppId == mymoooCompany).Select(s => new UserInfoResponse
            {
                Errcode = 0,
                ErrorMessage = "ok",
                UserId = s.UserId,
                Name = s.Name,
                Position = s.Position,
                Mobile = s.Mobile,
                Gender = s.Gender,
                Email = s.Email,
                Telephone = s.Telephone,
                Alias = s.Alias,
                Address = s.Address,
                OpenUserId = s.OpenUserid,
                Main_department = (int)s.MainDepartmentId,
                Status = s.Status,
                QrCode = s.QrCode,
                Id = s.Id,
                Extattr = new UserInfoResponse.WeiXinUserExtattr()
                {
                    ExtattrDetails = new UserInfoResponse.UserExtattrDetail[]
                    {
                       new() { Name = "学历",Value =  s.Education },
                       new() { Name = "入职时间",Value =  s.EntryDate == null ? string.Empty :s.EntryDate.Value.ToString("yyyy-MM-dd")  },
                       new() { Name = "直属上级",Value =  s.DirectSupervisor },
                       new() { Name = "职级",Value =  s.Grade }
                    }
                }
            }).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// 根据AccessToken获取TIcket
        /// </summary>
        /// <param name="appCode"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public AccessTicketResponse GetTicketByAccessToken(string appCode, string appId)
        {
            var weixinDbContext = GetDbContext();
            var applicationConfig = weixinDbContext.ThirdpartyApplicationConfig.Include(c => c.ThirdpartyApplicationDetail).FirstOrDefault(c => c.AppId == appId);
            var applicationDetial = applicationConfig.ThirdpartyApplicationDetail.FirstOrDefault(p => p.DetailCode == appCode);
            AccessTicketResponse response = null;
            UseWework(weixinDbContext, applicationDetial, () =>
            {
                string url = string.Format(applicationConfig.Url + WeWorkPath.GetTicket, applicationDetial.AccessToken);
                response = _httpUtils.InvokeGetWebService<AccessTicketResponse>(url);
                if (response != null)
                {
                    response.CropId = applicationConfig.Token;  // 这个名称有歧义不过值是这么用的
                    response.AppId = applicationDetial.AppId;
                    response.AgentId = applicationDetial.Agentid;
                }
            });
            _logger.LogInformation($"GetTicketByAccessToken：返回{JsonSerializerOptionsUtils.Serialize(response)}");
            return response;
        }

        public async Task<string> DownloadByMediaId(string appId, string mediaId, string pathFile, bool isHttps)
        {
            byte[] fileData = await _applicationServiceClient.DownLoadMediaAsync<WorkbenchContext, User>(_workbenchContext, mediaId);
            string? fileUrl = null;
            if (fileData.Length > 0)
            {
                try
                {
                    using (Stream stream = new MemoryStream(fileData))
                    {
                        fileUrl = _minioService.UploadFile(stream, pathFile, isHttps);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogInformation($"DownloadByMediaId-Minio ：{appId},mediaId={mediaId},pathFile={pathFile},错误:{e.Message}");
                }

            }
            return fileUrl;
        }

        /// <summary>
        /// 获取企业微信部门详情
        /// </summary>
        /// <param name="mymoooCompany"></param>
        /// <param name="department_id"></param>
        /// <returns></returns>
        public List<DepartmentListResponse.Userlist[]> GetDepartmentDetials(long department_id = 0)
        {
            var weixinDbContext = GetDbContext();
            List<DepartmentListResponse.Userlist[]> list = new List<DepartmentListResponse.Userlist[]>();
            DepartmentListResponse result = null;
            var applicationConfigList = weixinDbContext.ThirdpartyApplicationConfig.Include(c => c.ThirdpartyApplicationDetail).Where(c => c.IsWeiXinWork == true).ToList();
            foreach (var applicationConfig in applicationConfigList)
            {
                var applicationDetial = applicationConfig.ThirdpartyApplicationDetail.FirstOrDefault(p => p.DetailCode == "Application");
                //通过 通讯录的secret来生成token，以获取全部的用户：对应于微信的[获取部门的用户]
                UseWework(weixinDbContext, applicationDetial, () =>
                {
                    string url = string.Format(applicationConfig.Url + WeWorkPath.GetDepartmentDetail, applicationDetial.AccessToken, department_id > 0 ? department_id.ToString() : "");
                    result = _httpUtils.InvokeGetWebService<DepartmentListResponse>(url);
                });

                //深圳企业微信排第一位
                if ("weixinwork".Equals(applicationConfig.AppId))
                {
                    list.Insert(0, result.userlist);
                }
                else
                {
                    list.Add(result.userlist);
                }
            }
            return list;
        }

        /// <summary>
        /// 获取部门成员详情
        /// </summary>
        /// <param name="mymoooCompany"></param>
        /// <param name="department_id">部门Id</param>
        /// <param name="fetch_child">是否递归子部门</param>
        /// <returns></returns>
        public DepartmentListResponse GetDepartmentList(string mymoooCompany, long department_id, int fetch_child = 0)
        {
            //从企业微信获取
            //var applicationConfig = _weixinDbContext.ThirdpartyApplicationConfig.Include(c => c.ThirdpartyApplicationDetail).FirstOrDefault(c => c.AppId == mymoooCompany);
            //var applicationDetial = applicationConfig.ThirdpartyApplicationDetail.FirstOrDefault(p => p.DetailCode == WeixinWorkAppName.AddressBook);
            //DepartmentListResponse result = null;
            ////通过 通讯录的secret来生成token，以获取全部的用户：对应于微信的[获取部门的用户]
            //UseWework(applicationDetial, () =>
            //{
            //    string url = string.Format(applicationConfig.Url + WeWorkPath.GetDepartmentList, applicationDetial.AccessToken, department_id, fetch_child);
            //    var response = HttpUtils.InvokeWebService(url);
            //    result = JsonConvert.DeserializeObject<DepartmentListResponse>(response);
            //});

            //return result;


            //从数据库获取    
            //根据ID和系统码拿部门ID
            var weixinDbContext = GetDbContext();
            var departmentIdList = weixinDbContext.Department.Where(c => c.AppId == mymoooCompany).ToList();
            //递归查询所有子部门
            List<long> departmentSelectList = new List<long>();
            List<ef.AccountContext.Department> departmentIdList1 = departmentIdList;
            var selectDepartmentIdChild = SelectDepartmentIdChild(departmentIdList, department_id, departmentSelectList);
            //如果不存在子部门则查询当前部门
            //if (selectDepartmentIdChild.Count < 1)
            //{
            //    var selectDepartment = departmentIdList.Where(c => c.DepartmentId == department_id).FirstOrDefault();
            //    selectDepartmentIdChild.Add(selectDepartment.Id);
            //}
            var selectDepartment = departmentIdList.Where(c => c.DepartmentId == department_id).FirstOrDefault();
            selectDepartmentIdChild.Add(selectDepartment.Id);
            //查询用户信息
            var userlist = weixinDbContext.DepartmentUser.Where(p => selectDepartmentIdChild.Contains(p.DepartmentId)).Join(weixinDbContext.User, d => d.UserId, c => c.Id, (d, c) => new DepartmentListResponse.Userlist
            {
                email = c.Email,
                gender = c.Gender,
                mobile = c.Mobile,
                name = c.Name,
                position = c.Position,
                userid = c.UserId,
                id = c.Id
            }).ToList();
            var result = weixinDbContext.User.Where(c => c.AppId == mymoooCompany).Include(u => u.DepartmentUsers).Select(s => new DepartmentListResponse
            {
                errcode = 0,
                errmsg = "ok",
                userlist = userlist.ToArray()
            }).FirstOrDefault();
            return result;
        }

        private List<long> SelectDepartmentIdChild(List<ef.AccountContext.Department> department, long pid, List<long> departmentSelectList)
        {
            var children = department.Where(p => p.ParentId == pid).ToList();
            if (children.Count > 0)
            {
                foreach (var child in children)
                {
                    SelectDepartmentIdChild(department, child.DepartmentId, departmentSelectList);
                    departmentSelectList.Add(child.Id);
                }
            }
            return departmentSelectList;
        }


        /// <summary>
        /// 根据业务员和助理的ID,获取其微信和它领导的微信
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public List<string> GetSalesAndLeaderWechatList(List<string> userIds)
        {
            var result = new List<string>();
            var weixinDbContext = GetDbContext();
            var departmentIdList = weixinDbContext.User.Where(c => c.Status == 1 && userIds.Contains(c.Id.ToString()))
                .Select(c => c.MainDepartmentId).ToList();

            // 获取业务员的部门
            var deptIds = weixinDbContext.Department.Where(c => departmentIdList.Contains(c.DepartmentId)).Select(c => c.Id).ToList();

            // 获取领导
            var LeaderIds = weixinDbContext.DepartmentUser.Where(c => c.IsLeaderInDepartment == 1 && deptIds.Contains(c.DepartmentId))
                .Select(c => c.UserId.ToString())
                .ToList();

            userIds = userIds.Concat(LeaderIds).Distinct().ToList();

            result = [.. weixinDbContext.User.Where(c => c.Status == 1 && userIds.Contains(c.Id.ToString())).Select(c => c.UserId)];


            return result;
        }
    }
}
