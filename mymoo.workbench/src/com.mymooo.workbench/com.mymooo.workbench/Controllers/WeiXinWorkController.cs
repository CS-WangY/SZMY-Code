using com.mymooo.workbench.business.QuartzTask.WeiXinWork;
using com.mymooo.workbench.business.WeixinWork.AddressBook;
using com.mymooo.workbench.business.WeixinWork.Approval;
using com.mymooo.workbench.core.Account;
using com.mymooo.workbench.core.SystemManage.Filter;
using com.mymooo.workbench.core.Utils;
using com.mymooo.workbench.core.WeiXinWork.Account;
using com.mymooo.workbench.ef;
using com.mymooo.workbench.weixin.work.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Model.Gateway;
using mymooo.core.Utils.JsonConverter;
using mymooo.weixinWork.SDK.AddressBook;
using mymooo.weixinWork.SDK.Application;
using mymooo.weixinWork.SDK.Approval;
using mymooo.weixinWork.SDK.Approval.Model;
using mymooo.weixinWork.SDK.Approval.Model.Enum;
using mymooo.weixinWork.SDK.SqlSugarCore;
using mymooo.weixinWork.SDK.WeixinWorkMessage.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace com.mymooo.workbench.Controllers
{
    /// <summary>
    /// 企业微信
    /// </summary>
    public class WeiXinWorkController(WeixinWorkUtils weixinWorkUtils, WeiXinWorkMessage weiXinWorkMessage, ApplicationServiceClient applicationServiceClient
        , WorkbenchDbContext workbenchDbContext, MessageScheduled messageScheduled, AddressBookServiceClient addressBookServiceClient
        , HttpUtils httpUtils, WorkbenchContext workbenchContext, ApprovalServiceClient approvalServiceClient, AddressBookService addressBookService, ApprovalService approvalService, ILogger<WeiXinWorkController> logger) : Controller
    {
        private readonly WeixinWorkUtils _weixinWorkUtils = weixinWorkUtils;
        private readonly WeiXinWorkMessage _weiXinWorkMessage = weiXinWorkMessage;
        private readonly WorkbenchDbContext _workbenchDbContext = workbenchDbContext;
        private readonly HttpUtils _httpUtils = httpUtils;
        private readonly WorkbenchContext _workbenchContext = workbenchContext;
        private readonly ApprovalServiceClient _approvalServiceClient = approvalServiceClient;
        private readonly AddressBookService _addressBookService = addressBookService;
        private readonly MessageScheduled _messageScheduled = messageScheduled;
        private readonly AddressBookServiceClient _addressBookServiceClient = addressBookServiceClient;
        private readonly ApprovalService _approvalService = approvalService;
        private readonly ApplicationServiceClient _applicationServiceClient = applicationServiceClient;
        private readonly ILogger<WeiXinWorkController> _logger = logger;

        /// <summary>
        /// 加载用户缓存
        /// </summary>
        /// <returns></returns>
        public IActionResult ReloadUserCache()
        {
            ResponseMessage<dynamic> response = new();
            _addressBookService.ReloadCache();
            return Json(response);
        }

        /// <summary>
        /// 重新执行
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Execute(long id)
        {
            ResponseMessage<dynamic> response = new();
            var weixinMessage = _workbenchDbContext.WeiXinMessage.Include(c => c.ApplicationDetail).FirstOrDefault(c => c.Id == id);
            if (weixinMessage == null)
            {
                response.ErrorMessage = "没有该审批记录";
                response.Code = ResponseCode.NoExistsData;
            }
            else
            {
                await _messageScheduled.ExecWeiXinMessage(weixinMessage, _workbenchContext);
            }
            return Json(response);
        }

        /// <summary>
        /// 修复
        /// </summary>
        /// <param name="spno"></param>
        /// <returns></returns>
        public async Task<IActionResult> RepairSp(string spno)
        {
            ResponseMessage<dynamic> response = new();
            var result = await _approvalServiceClient.GetApprovalDetail<WorkbenchContext, User>(_workbenchContext, new GetApprovalDetailRequest() { SpNo = spno }, true);
            if (!result.IsSuccess)
            {
                return Json(result);
            }
            var data = result.Data;
            if (data.Info == null)
            {
                response.ErrorMessage = data.ErrorMessage;
                response.Code = ResponseCode.NoExistsData;
                return Json(response);
            }

            if (data.Info.SpStatus != 2 && data.Info.SpStatus != 3)
            {
                response.ErrorMessage = "该审批还未审核通过,不能执行该操作";
                response.Code = ResponseCode.NoExistsData;
                return Json(response);
            }
            //else if (_workbenchDbContext.WeiXinMessage.Any(it => it.Spno == spno && (it.Status == 2 || it.Status == 3)))
            //{
            //    response.ErrorMessage = "该审批信息已修复,无需重复执行";
            //    response.Code = ResponseCode.NoExistsData;
            //    return Json(response);
            //}
            var message = $@"<xml><ToUserName><![CDATA[wwcee0ce9fd7961e97]]></ToUserName><FromUserName><![CDATA[sys]]></FromUserName><CreateTime>1692336160</CreateTime><MsgType><![CDATA[event]]></MsgType><Event><![CDATA[sys_approval_change]]></Event><AgentID>3010040</AgentID><ApprovalInfo><SpNo>{spno}</SpNo><SpName><![CDATA[审批]]></SpName><SpStatus>{data.Info.SpStatus}</SpStatus><TemplateId><![CDATA[C4UE66oVYU5xi3TTiFV2iD7Vb1d5ozqhEqpVeqisD]]></TemplateId><ApplyTime>1692336153</ApplyTime><Applyer><UserId><![CDATA[HuJie]]></UserId><Party><![CDATA[397]]></Party></Applyer><SpRecord><SpStatus>2</SpStatus><ApproverAttr>1</ApproverAttr><Details><Approver><UserId><![CDATA[HuJie]]></UserId></Approver><Speech><![CDATA[]]></Speech><SpStatus>2</SpStatus><SpTime>1692336160</SpTime></Details></SpRecord><Notifyer><UserId><![CDATA[LiYing]]></UserId></Notifyer><Notifyer><UserId><![CDATA[MaoJiaWang]]></UserId></Notifyer><Notifyer><UserId><![CDATA[SuSong]]></UserId></Notifyer><Notifyer><UserId><![CDATA[LiYing]]></UserId></Notifyer><Notifyer><UserId><![CDATA[MaoJiaWang]]></UserId></Notifyer><Notifyer><UserId><![CDATA[SuSong]]></UserId></Notifyer><StatuChangeEvent>2</StatuChangeEvent></ApprovalInfo></xml>";
            ef.ThirdpartyApplication.WeiXinMessage weiXinMessage = new()
            {
                ApplicationDetailId = 3,
                Message = message,
                IsComplete = false,
                CreateDate = DateTime.Now,
                CompleteDate = DateTime.Now,
                Result = "",
                StackTrace = null,
                Spno = spno,
                Status = data.Info.SpStatus
            };
            _workbenchDbContext.WeiXinMessage.Add(weiXinMessage);
            _workbenchDbContext.SaveChanges();
            var weiXin = _workbenchDbContext.WeiXinMessage.Include(c => c.ApplicationDetail).FirstOrDefault(it => it.Id == weiXinMessage.Id);
            weiXin.IsComplete = await _approvalService.WeiXinCallBack(weiXin);
            if (weiXin.IsComplete)
            {
                _workbenchDbContext.SaveChanges();
            }
            else
            {
                response.Code = ResponseCode.WeiXinError;
            }
            _logger.LogInformation($"审批流回调手工修复:RepairSp：{spno}返回{JsonSerializerOptionsUtils.Serialize(result.Data)}");
            return Json(response);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //[ServiceFilter(typeof(TokenAuthorityAttribute))]
        //public IActionResult Synchronize()
        //{
        //    return Json(_weixinWorkUtils.Synchronize());
        //}

        /// <summary>
        /// 获取部门详情
        /// </summary>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetDepartmentDetials(long departmentId)
        {
            return Json(await _addressBookServiceClient.GetDepartmentList<WorkbenchContext, User>(_workbenchContext, departmentId));
        }

        /// <summary>
        /// 获取部门成员详情
        /// </summary>
        /// <param name="departmentId">部门Id</param>
        /// <param name="fetchChild">是否递归子部门</param>
        /// <returns></returns>
        public IActionResult GetDepartmentList(long departmentId, int fetchChild = 0)
        {
            ResponseMessage<dynamic> response = new();
            var userlist = _weixinWorkUtils.GetDepartmentList(_workbenchContext.User.AppId, departmentId, fetchChild);
            response.Code = ResponseCode.Success;
            response.Data = userlist;
            return Json(response);
        }

        /// <summary>
        /// 获取企业微信用户信息
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="mymoooCompany"></param>
        /// <returns></returns>
        public IActionResult GetUserInfo(string userCode, string mymoooCompany = "weixinwork")
        {
            ResponseMessage<User> response = new();
            var userInfo = _weixinWorkUtils.GetUserInfo(userCode, mymoooCompany);
            response.Code = ResponseCode.Success;
            response.Data = new User()
            {
                Code = userInfo.UserId,
                AppId = _workbenchContext.User.AppId,
                MymoooCompany = _workbenchContext.User.MymoooCompany,
                Name = userInfo.Name,
                Email = userInfo.Email,
                Mobile = userInfo.Mobile,
                Address = userInfo.Address,
                MainDepartment = userInfo.Main_department,
                ExternalPosition = userInfo.ExternalPosition,
                Gender = userInfo.Gender,
                OpenUserId = userInfo.OpenUserId,
                QrCode = userInfo.QrCode,
                IsAdmin = _workbenchContext.User.IsAdmin,
                IsKeeponAttache = _workbenchContext.User.IsKeeponAttache,
                UserId = userInfo.Id,
                Extattr = userInfo.Extattr
            };
            return Json(response);
        }

        /// <summary>
        /// 获取企业微信用户Id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IActionResult GetUserId([FromBody] GetUserInfoRequest request)
        {
            ResponseMessage<User> response = new ResponseMessage<User>();
            var userInfo = _weixinWorkUtils.GetUserInfo(request.UserCode, request.MymoooCompany);
            response.Code = ResponseCode.Success;
            response.Data = new User()
            {
                UserId = userInfo == null ? 0 : userInfo.Id
            };
            return Json(response);
        }

        /// <summary>
        /// 获取用户信息ById
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult GetUserInfoById(long id)
        {
            ResponseMessage<dynamic> response = new();
            var userInfo = _workbenchDbContext.User.FirstOrDefault(c => c.Id == id);
            if (userInfo != null)
            {
                response.Data = new User()
                {
                    Code = userInfo.UserId,
                    MymoooCompany = userInfo.AppId,
                    Name = userInfo.Name,
                    Email = userInfo.Email,
                    Mobile = userInfo.Mobile,
                    Address = userInfo.Address,
                    Gender = userInfo.Gender,
                    MainDepartment = (int)userInfo.MainDepartmentId,
                    ExternalPosition = userInfo.ExternalPosition,
                    OpenUserId = userInfo.OpenUserid,
                    QrCode = userInfo.QrCode,
                    UserId = userInfo.Id
                };
            }
            response.Code = ResponseCode.Success;
            return Json(response);
        }
        /// <summary>
        /// 获取用户信息ByCode
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public IActionResult GetUserInfoByCode(string code)
        {
            ResponseMessage<dynamic> response = new();
            var userInfo = _workbenchDbContext.User.FirstOrDefault(c => c.UserId == code);
            if (userInfo != null)
            {
                response.Data = new User()
                {
                    Code = userInfo.UserId,
                    MymoooCompany = userInfo.AppId,
                    Name = userInfo.Name,
                    Email = userInfo.Email,
                    Mobile = userInfo.Mobile,
                    Address = userInfo.Address,
                    Gender = userInfo.Gender,
                    MainDepartment = (int)userInfo.MainDepartmentId,
                    ExternalPosition = userInfo.ExternalPosition,
                    OpenUserId = userInfo.OpenUserid,
                    QrCode = userInfo.QrCode,
                    UserId = userInfo.Id
                };
            }
            response.Code = ResponseCode.Success;
            return Json(response);
        }

        /// <summary>
        /// 获取上级
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public IActionResult GetHigherUps(string userCode)
        {
            ResponseMessage<dynamic> response = new();
            var user = _workbenchDbContext.User.FirstOrDefault(it => it.UserId == userCode && !it.IsDelete && it.AppId == "weixinwork");
            if (user == null)
            {
                response.Code = ResponseCode.NoExistsData;
                response.Message = "该用户不存在";
                return Json(response);
            }
            var leaderUserid = _workbenchDbContext.DepartmentUser.Where(it => it.Department.DepartmentId == user.MainDepartmentId && it.Department.AppId == "weixinwork" && it.IsLeaderInDepartment == 1).Select(it => it.UserId);
            response.Data = _workbenchDbContext.User.Where(it => leaderUserid.Contains(it.Id) && it.AppId == "weixinwork" && !it.IsDelete && it.Status == 1).Select(it => new User()
            {
                Code = it.UserId,
                MymoooCompany = it.AppId,
                Name = it.Name,
                Email = it.Email,
                Mobile = it.Mobile,
                Address = it.Address,
                Gender = it.Gender,
                MainDepartment = (int)it.MainDepartmentId,
                ExternalPosition = it.ExternalPosition,
                OpenUserId = it.OpenUserid,
                QrCode = it.QrCode,
                UserId = it.Id
            }).ToList();
            response.Code = ResponseCode.Success;
            return Json(response);
        }


        /// <summary>
        /// 返回JSSDK用的Ticket
        /// </summary>
        /// <returns></returns>
        public IActionResult GetTicket(string appId)
        {
            ResponseMessage<AccessTicketResponse> response = new ResponseMessage<AccessTicketResponse>();
            //string myMoooCompany = _workbenchContext.User.MymoooCompany;
            //#if DEBUG
            //            myMoooCompany = "Developweixinwork";
            //#endif
            var result = _weixinWorkUtils.GetTicketByAccessToken("Application", appId);

            if (result.errcode != 0)
            {
                response.Code = ResponseCode.ThirdpartyError;
                response.ErrorMessage = result.errmsg;
            }
            else
            {
                response.Data = result;
                response.Code = ResponseCode.Success;
            }
            return Json(response);
        }

        /// <summary>
        /// 从微信下载并上传文件到Minio
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="mediaId"></param>
        /// <param name="pathFile"></param>
        /// <returns></returns>
        public async Task<IActionResult> DownloadByMediaId(string appId, string mediaId, string pathFile)
        {
            ResponseMessage<string> response = new();
            var suffix = Path.GetExtension(pathFile);
            if (string.IsNullOrEmpty(suffix))
            {
                pathFile = pathFile + ".jpg"; // 没有扩展名,默认.jpg
            }
            
            var fileUrl = await _weixinWorkUtils.DownloadByMediaId(appId, mediaId, pathFile, HttpContext.Request.IsHttps);

            if (string.IsNullOrEmpty(fileUrl))
            {
                response.Code = ResponseCode.ThirdpartyError;
                response.ErrorMessage = "下载或上传到Minio失败";
            }
            else
            {
                response.Data = fileUrl;
                response.Code = ResponseCode.Success;
            }
            return Json(response);
        }

        /// <summary>
        /// 接收企业微信推送消息
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult ReceiveMessage(string appId, string appCode, string msg_signature, string timestamp, string nonce, string echostr)
        {
            if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(appCode))
            {
                return Content("appid和appcode不能为空");
            }
            string data = "";
            using (var reader = new System.IO.StreamReader(HttpContext.Request.Body, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                var task = reader.ReadToEndAsync();
                data = task.Result;
            }
            return Content(_weiXinWorkMessage.AddMessage(appId, appCode, msg_signature, timestamp, nonce, echostr, data));
        }

        /// <summary>
        /// 获取审批模板列表
        /// </summary>
        /// <returns></returns>
        public IActionResult GetApprovalTemplateList([FromBody] PageRequest<ApprovalFilter> filter)
        {
            ResponseMessage<PageResponse<dynamic>> response = new();

            filter ??= new PageRequest<ApprovalFilter>();

            var query = _workbenchContext.SqlSugar.Queryable<ApprovalTemplate>().Includes(p => p.ApprovalTemplateFields);
            var total = query.Count();

            response.Data = new PageResponse<dynamic>(filter.PageIndex, filter.PageSize, total);

            //计算需要跳过是数量
            var skip = (filter.PageIndex - 1) * filter.PageSize;
            if (total <= skip)
            {
                //总数小于跳过的数量,直接返回
                response.Code = ResponseCode.Success;
                return Json(response);
            }

            var result = query.ToOffsetPage(filter.PageIndex, filter.PageSize);

            result.ForEach(c =>
                response.Data.Rows.Add(new
                {
                    c.ApprovalTemplateFields,
                    c.CreateDate,
                    c.CreateUser,
                    c.TemplateId,
                    c.TemplateName
                })
            );
            response.Code = ResponseCode.Success;
            return Json(response);
        }

        /// <summary>
        /// 获取审批流配置列表
        /// </summary>
        /// <returns></returns>
        public IActionResult GetAuditFlowConfigList(string templateId)
        {
            ResponseMessage<dynamic> response = new();
            if (templateId != null)
            {
                var query = _workbenchContext.SqlSugar.Queryable<AuditFlowConfig>().Includes(c => c.AuditFlowConfigDetails.OrderBy(c => c.SonId).ToList()).Where(c => c.TemplateId == templateId).ToList();
                response.Data = query;
                response.Code = ResponseCode.Success;
                return Json(response);
            }
            else
            {
                response.Code = ResponseCode.NoExistsData;
                return Json(response);
            }
        }

        /// <summary>
        /// 添加审批流配置列表
        /// </summary>
        /// <returns></returns>
        public IActionResult AddAuditFlowConfigList([FromBody] List<AuditFlowConfigDetail> auditFlowConfigDetail)
        {
            ResponseMessage<dynamic> response = new();

            //response.Code = ResponseCode.Success;
            //return Json(response);

            if (auditFlowConfigDetail != null && auditFlowConfigDetail.Count > 0)
            {
                auditFlowConfigDetail.ForEach(p =>
                {
                    p.CreateDateTime = DateTime.Now;
                    p.CreateUserCode = _workbenchContext.User.Code;
                });
                var result = _workbenchContext.SqlSugar.UseTran(() =>
                {
                    _workbenchContext.SqlSugar.Deleteable<AuditFlowConfigDetail>().Where(p => p.AuditFlowConfigId == auditFlowConfigDetail[0].AuditFlowConfigId).ExecuteCommand();
                    _workbenchContext.SqlSugar.Insertable(auditFlowConfigDetail).ExecuteCommand();
                });
                if (!result.IsSuccess)
                {
                    response.Code = ResponseCode.DbUpdateException;
                    response.ErrorMessage = "插入数据失败";
                }
                return Json(response);
            }
            else
            {
                response.Code = ResponseCode.Empty;
                return Json(response);
            }
        }

        /// <summary>
        /// 新增审批模板
        /// </summary>
        /// <returns></returns>
        public IActionResult AddApprovalTemplate([FromBody] ApprovalTemplate approvalTemplate)
        {
            ResponseMessage<dynamic> response = new();
            if (approvalTemplate != null)
            {
                approvalTemplate.CreateDate = DateTime.Now;
                approvalTemplate.CreateUser = _workbenchContext.User.Code;
                _workbenchContext.SqlSugar.Insertable(approvalTemplate).IgnoreColumnsNull().ExecuteCommand();
                return Json(response);
            }
            else
            {
                response.Code = ResponseCode.ModelError;
                response.ErrorMessage = "空数据";
                return Json(response);
            }
        }

        /// <summary>
        /// 加载审批缓存
        /// </summary>
        /// <param name="approvalTemplate"></param>
        /// <returns></returns>
        public IActionResult ReloadCache([FromBody] ApprovalTemplate approvalTemplate)
        {
            ResponseMessage<dynamic> response = new();
            //if (!_workbenchContext.ApigatewayConfig.EnvCode.EqualsOrdinalIgnoreCase("production"))
            //{
            //    response.Code = ResponseCode.StatusException;
            //    response.ErrorMessage = "非生产环境不加缓存!";
            //    return Json(response);
            //}
            if (approvalTemplate != null)
            {
                var tamplate = _workbenchContext.SqlSugar.Queryable<ApprovalTemplate>().Includes(c => c.ApprovalTemplateFields)
                    .Includes(c => c.AuditFlowConfigs, d => d.AuditFlowConfigDetails).First(c => c.TemplateId == approvalTemplate.TemplateId);
                if (tamplate == null)
                {
                    response.Code = ResponseCode.Empty;
                    response.ErrorMessage = "空数据";
                    return Json(response);
                }
                _workbenchContext.WorkbenchRedisCache.HashSet(tamplate);
                foreach (var auditFlowConfig in tamplate.AuditFlowConfigs)
                {
                    _workbenchContext.WorkbenchRedisCache.HashSet(auditFlowConfig);
                }
                return Json(response);
            }
            else
            {
                response.Code = ResponseCode.Empty;
                response.ErrorMessage = "空数据";
                return Json(response);
            }
        }

        /// <summary>
        /// 加载全部的审批模板缓存
        /// </summary>
        /// <returns></returns>
        public IActionResult ReloadAllCache()
        {
            ResponseMessage<dynamic> response = new();
            //if (!_workbenchContext.ApigatewayConfig.EnvCode.EqualsOrdinalIgnoreCase("production"))
            //{
            //    response.Code = ResponseCode.StatusException;
            //    response.ErrorMessage = "非生产环境不加缓存!";
            //    return Json(response);
            //}
            var tamplates = _workbenchContext.SqlSugar.Queryable<ApprovalTemplate>().Includes(c => c.ApprovalTemplateFields).Includes(c => c.AuditFlowConfigs, d => d.AuditFlowConfigDetails).ToList();
            foreach (var tamplate in tamplates)
            {
                _workbenchContext.WorkbenchRedisCache.HashSet(tamplate);
                foreach (var auditFlowConfig in tamplate.AuditFlowConfigs)
                {
                    _workbenchContext.WorkbenchRedisCache.HashSet(auditFlowConfig);
                }
            }
            return Json(response);

        }

        /// <summary>
        /// 新增审批配置
        /// </summary>
        /// <returns></returns>
        public IActionResult AddApprovalConfig([FromBody] AuditFlowConfig auditFlowConfig)
        {
            ResponseMessage<dynamic> response = new();
            if (auditFlowConfig != null)
            {
                auditFlowConfig.CreateTime = DateTime.Now;
                auditFlowConfig.CreateUserCode = _workbenchContext.User.Code;
                _workbenchContext.SqlSugar.Insertable(auditFlowConfig).IgnoreColumnsNull().ExecuteCommand();
                response.Code = ResponseCode.Success;
                return Json(response);
            }
            else
            {
                response.Code = ResponseCode.Exception;
                return Json(response);
            }
        }

        /// <summary>
        /// 获取审批模板字段
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public IActionResult GetTemplateField(string templateId)
        {
            ResponseMessage<dynamic> response = new();
            if (templateId != null)
            {
                var query = _workbenchContext.SqlSugar.Queryable<ApprovalTemplateField>().Includes(c => c.ApprovalTemplate).Where(c => c.TemplateId == templateId && c.FieldType != ApproverFieldType.File && c.FieldType != ApproverFieldType.Textarea).ToList();
                response.Data = query;
                return Json(response);
            }
            else
            {
                response.Code = ResponseCode.Exception;
                return Json(response);
            }
        }

        /// <summary>
        /// 更新审批模板字段
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public IActionResult UpdateTemplateFieldNumber([FromBody] ApprovalTemplateField field)
        {
            ResponseMessage<dynamic> response = new();
            if (field != null)
            {
                if (field.Id == 0)
                {
                    field.CreateDate = DateTime.Now;
                    field.CreateUser = _workbenchContext.User.Code;
                    field.Id = _workbenchContext.SqlSugar.Insertable(field).IgnoreColumnsNull().ExecuteReturnBigIdentity();
                }
                else
                {
                    _workbenchContext.SqlSugar.Updateable(field).UpdateColumns(c => c.FieldNumber).ExecuteCommand();
                }

                response.Data = new
                {
                    field.Id,
                    createDate = DateTime.Now,
                    createUser = _workbenchContext.User.Code
                };
                return Json(response);
            }
            else
            {
                response.Code = ResponseCode.Empty;
                return Json(response);
            }
        }

        /// <summary>
        /// 获取模板字段
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetField(string templateId)
        {
            return Json(await _approvalServiceClient.GetTemplateFields<WorkbenchContext, User>(_workbenchContext, templateId));
        }

        /// <summary>
        /// 删除审批流配置
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult DelApprovalConfig(long id)
        {
            ResponseMessage<dynamic> response = new();
            if (id != 0)
            {
                var result = _workbenchContext.SqlSugar.UseTran(() =>
                {
                    _workbenchContext.SqlSugar.Deleteable<AuditFlowConfigDetail>().Where(c => c.AuditFlowConfigId == id).ExecuteCommand();
                    _workbenchContext.SqlSugar.Deleteable<AuditFlowConfig>().Where(c => c.Id == id).ExecuteCommand();
                });
                if (result.IsSuccess)
                {
                    return Json(response);
                }
                else
                {
                    response.Code = ResponseCode.DbUpdateException;
                    return Json(response);
                }
            }
            else
            {
                response.Code = ResponseCode.Exception;
                return Json(response);
            }
        }

        /// <summary>
        /// 获取环境变量信息
        /// </summary>
        /// <returns></returns>
        public IActionResult GetEnvCodes()
        {
            var response = new ResponseMessage<List<EnvironmentConfig>>
            {
                Data = _workbenchContext.GatewayRedisCache.HashGetAll<EnvironmentConfig>()
            };
            return Json(response);
        }

        /// <summary>
        /// 当前运行环境是否生产环境
        /// </summary>
        /// <returns></returns>
        public IActionResult GetEnvIsProduction()
        {
            var response = new ResponseMessage<bool>
            {
                Data = _workbenchContext.ApigatewayConfig.EnvCode.Equals("production", StringComparison.OrdinalIgnoreCase)
            };
            return Json(response);
        }
        
        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IActionResult GetWxCallbackMessage([FromBody] PageRequest<MessageFilter> filter)
        {
            ResponseMessage<PageResponse<dynamic>> response = new();

            filter ??= new PageRequest<MessageFilter>();

            var query = _workbenchDbContext.WeiXinMessage.Join(_workbenchDbContext.ThirdpartyApplicationDetail, c => c.ApplicationDetailId, d => d.Id, (c, d) => new
            {
                c.Id,
                c.IsComplete,
                c.Message,
                c.Result,
                c.StackTrace,
                c.CreateDate,
                c.CompleteDate,
                c.Spno,
                c.Status,
                c.ApplicationDetailId,
                d.DetailCode,
                d.DetailName
            });

            //条件过滤
            if (filter.Filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Filter.Spno))
                {
                    query = query.Where(c => c.Spno == filter.Filter.Spno);
                }
                if (filter.Filter.IsComplete != null)
                {
                    query = query.Where(c => c.IsComplete == filter.Filter.IsComplete);
                }
                if (filter.Filter.Status != null)
                {
                    query = query.Where(c => c.Status == filter.Filter.Status);
                }
                if (filter.Filter.CreateDate != null && filter.Filter.CompleteDate != null)
                {
                    query = query.Where(c => c.CreateDate >= filter.Filter.CreateDate && c.CreateDate <= filter.Filter.CompleteDate);
                }
                if (!string.IsNullOrWhiteSpace(filter.Filter.detailName))
                {
                    query = query.Where(c => c.DetailName == filter.Filter.detailName);
                }
            }
            query = query.OrderByDescending(c => c.CreateDate);
            var total = query.Count();

            response.Data = new PageResponse<dynamic>(filter.PageIndex, filter.PageSize, total);

            //计算需要跳过是数量
            var skip = (filter.PageIndex - 1) * filter.PageSize;
            if (total <= skip)
            {
                //总数小于跳过的数量,直接返回
                response.Code = ResponseCode.Success;
                return Json(response);
            }

            var result = query.Skip(skip).Take(filter.PageSize).ToList();

            result.ForEach(c =>
                response.Data.Rows.Add(c)
            );
            response.Code = ResponseCode.Success;
            return Json(response);
        }

        /// <summary>
        /// 审批审批详情
        /// </summary>
        /// <param name="spno"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetSpDetail(string spno)
        {
            ResponseMessage<dynamic> response = new();
            if (string.IsNullOrWhiteSpace(spno))
            {
                response.Code = ResponseCode.Exception;
                response.ErrorMessage = "单号不能为空";
            }
            else
            {
                var result = await _approvalServiceClient.GetApprovalDetail<WorkbenchContext, User>(_workbenchContext, new GetApprovalDetailRequest() { SpNo = spno });
                if (!result.IsSuccess)
                {
                    return Json(result);
                }
                var data = result.Data;

                foreach (var item in data.Info.SpRecords)
                {
                    foreach (var item2 in item.Details)
                    {
                        var name = _workbenchDbContext.User.Where(c => c.UserId == item2.Approver.UserId).Select(c => c.Name).FirstOrDefault();
                        item2.Approver.UserId = name;
                    }
                }
                foreach (var item in data.Info.Notifyer)
                {
                    var name = _workbenchDbContext.User.Where(c => c.UserId == item.UserId).Select(c => c.Name).FirstOrDefault();
                    item.UserId = name;
                }
                data.Info.Applyer.UserId = _workbenchDbContext.User.Where(c => c.UserId == data.Info.Applyer.UserId).Select(c => c.Name).FirstOrDefault();
                response.Data = data;
                response.Code = ResponseCode.Success;
            }
            return Json(response);
        }
        /// <summary>
        /// 应用发送文本信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IActionResult> SendTextMessage([FromBody] SendTextMessageRequest request)
        {
            return Json(await _applicationServiceClient.SendTextMessage<WorkbenchContext, User>(_workbenchContext, request));
        }

        /// <summary>
        /// 应用发送附件信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IActionResult> SendFileMessage([FromBody] SendFileMessageRequest request)
        {
            return Json(await _applicationServiceClient.SendFileMessageAsync<WorkbenchContext, User>(_workbenchContext, request));
        }

        /// <summary>
        /// 应用发送卡片信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IActionResult> SendtextcardMessage([FromBody] SendTextCardMessageRequest request)
        {
            return Json(await _applicationServiceClient.SendTextcardMessage<WorkbenchContext, User>(_workbenchContext, request));
        }

        /// <summary>
        /// 流失客户微信提醒
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IActionResult> SendLostCustomerTextMessage([FromBody] List<string> request)
        {
            // 这里收到的是,需要发送的绑定销售的WechatId
            // 绑定的可能包括已离职的.这里要把离职的过滤掉. 
            var SendTo = _weixinWorkUtils.GetSalesAndLeaderWechatList(request);

            SendTo.Add("LiYing"); // DEBUG 调试跟踪一下, 下一个版本去掉.

            SendTextMessageRequest message = new()
            {
                ToUser = string.Join("|", SendTo),
                Text = new SendTextMessageRequest.TextMessage()
                {
                    Content = "你有部分客户已经3个月未询价，请及时跟踪，否则会被系统回收。请到报表系统中的【数据分析】【客户数据分析】【客户流失统计】中查看具体信息。"
                }
            };

            return Json(await _applicationServiceClient.SendTextMessage<WorkbenchContext, User>(_workbenchContext, message));
        }

    }
}
