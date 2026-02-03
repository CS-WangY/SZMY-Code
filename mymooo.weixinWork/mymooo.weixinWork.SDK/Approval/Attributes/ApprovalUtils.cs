using mymooo.core.Attributes;
using mymooo.core.Cache;
using mymooo.core.Utils;
using mymooo.core.Utils.JsonConverter;
using mymooo.core.Utils.Script;
using mymooo.weixinWork.SDK.AddressBook;
using mymooo.weixinWork.SDK.Approval.Model;
using mymooo.weixinWork.SDK.Approval.Model.Enum;
using mymooo.weixinWork.SDK.Media;
using mymooo.weixinWork.SDK.Media.Model;
using mymooo.weixinWork.SDK.SqlSugarCore;
using SqlSugar;
using System.Collections.Concurrent;
using static mymooo.weixinWork.SDK.Approval.Model.ApproverDetailsResponse;

namespace mymooo.weixinWork.SDK.Approval.Attributes
{
    /// <summary>
    /// 
    /// </summary>
    [AutoInject(InJectType.Single)]
    public class ApprovalUtils
    {
        private readonly Dictionary<ApproverFieldType, Func<string, string, dynamic, dynamic>> _createControl;
        private readonly ConcurrentDictionary<Type, List<ApprovalTemplateField>> _approvalTemplateFields = new();
        private readonly ConcurrentDictionary<Type, ApprovalTemplateAttribute> _approvalTemplate = new();
        private readonly WorkbenchRedisCache _redisCache;
        private readonly PythonEngineService _pythonEngineService;
        private readonly AddressBookServiceClient _addressBookServiceClient;
        private readonly MediaServiceClient _mediaServiceClient;

        /// <summary>
        /// 
        /// </summary>
        public ApprovalUtils(WorkbenchRedisCache redisCache, PythonEngineService pythonEngineService, AddressBookServiceClient addressBookServiceClient, MediaServiceClient mediaServiceClient)
        {
            _createControl = new Dictionary<ApproverFieldType, Func<string, string, dynamic, dynamic>>
            {
                [ApproverFieldType.Text] = CreateTextValue,
                [ApproverFieldType.Date] = CreateDateValue,
                [ApproverFieldType.File] = CreateFileValue,
                [ApproverFieldType.Money] = CreateMoneyValue,
                [ApproverFieldType.Number] = CreateNumberValue,
                [ApproverFieldType.Textarea] = CreateTextareaValue,
                [ApproverFieldType.Selector] = CreateSelectorValue,
                [ApproverFieldType.Contact] = CreateContactValue
            };
            _redisCache = redisCache;
            _pythonEngineService = pythonEngineService;
            _addressBookServiceClient = addressBookServiceClient;
            _mediaServiceClient = mediaServiceClient;
        }

        /// <summary>
        /// 创建审批数据
        /// </summary>
        /// <typeparam name="Q"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<WechatApprovalRequest> CreateApprovalRequest<Q>(Q request) where Q : ApprovalRequest
        {
            var template = GetApprovalTemplate<Q>();
            var fields = GetApprovalTemplateFields<Q>(template);
            WechatApprovalRequest wechatApprovalRequest = new()
            {
                CreatorUserCode = request.CreatorUserCode,
                ApplyData = new ApprovalDetails(),
                TemplateId = template.TemplateId,
                NotifyType = request.NotifyType,
                EnvCode = request.EnvCode,
                SummaryInfo = request.SummaryInfo
            };

            foreach (var field in fields)
            {
                if (field.FieldType == ApproverFieldType.File)
                {
                    if (field.PropertyInfo?.PropertyType != typeof(List<MediaInfo>))
                    {
                        throw new Exception($"{field.FieldName} 为附件, 对应{field.FieldNumber}属性必须是List<MediaInfo>类型!");
                    }
                    await CreateFileContent(request, wechatApprovalRequest, field);
                }
                else if (field.FieldType == ApproverFieldType.Selector)
                {
                    if (field.PropertyInfo?.PropertyType != typeof(string))
                    {
                        throw new Exception($"{field.FieldName} 为下拉框, 对应{field.FieldNumber}属性必须是string类型!");
                    }
                    dynamic? value = field.PropertyInfo?.GetValue(request, null);
                    var selectList = JsonSerializerOptionsUtils.Deserialize<List<SelectOption>>(field.SelectOptionJson);
                    var selectValue = selectList?.FirstOrDefault(c => c.Value.Any(o => o.Text == value));
                    if (selectValue != null)
                    {
                        wechatApprovalRequest.ApplyData.Contents.Add(_createControl[field.FieldType](field.FieldId, field.FieldName, selectValue.Key));
                    }
                }
                else
                {
                    dynamic? value = field.PropertyInfo?.GetValue(request, null);
                    wechatApprovalRequest.ApplyData.Contents.Add(_createControl[field.FieldType](field.FieldId, field.FieldName, value));
                }
            }
            CreateApprovalAuditFlow(wechatApprovalRequest, request, template, fields);
            if (wechatApprovalRequest.Notifyer != null)
            {
                wechatApprovalRequest.Notifyer = wechatApprovalRequest.Notifyer.Where(p => !p.IsNullOrWhiteSpace()).Distinct().ToList();
            }
            return wechatApprovalRequest;
        }

        private void CreateApprovalAuditFlow<Q>(WechatApprovalRequest wechatApprovalRequest, Q request, ApprovalTemplateAttribute template, List<ApprovalTemplateField> fields) where Q : ApprovalRequest
        {
            AuditFlowConfig? auditFlowConfig = _redisCache.HashGet(new AuditFlowConfig() { TemplateId = template.TemplateId, EnvCode = request.EnvCode });
            if (auditFlowConfig == null)
            {
                wechatApprovalRequest.UseTemplateApprover = TemplateApproverModel.background;
            }
            else
            {
                wechatApprovalRequest.UseTemplateApprover = auditFlowConfig.ApprovalMode;
                wechatApprovalRequest.NotifyType = auditFlowConfig.NotifyType;
                if (auditFlowConfig.ApprovalMode == TemplateApproverModel.interfaceType)
                {
                    AssignCreateUser(wechatApprovalRequest, auditFlowConfig);
                }
                var values = Transform(request);
                RecursionApprovalAuditFlow(wechatApprovalRequest, auditFlowConfig.AuditFlowConfigDetails?.Where(it => it.Type != AuditFlowDetailType.CreateUser).ToList(), fields, values, request);
            }
        }

        private static string CreateConditons(List<ApproverCondition> conditions, List<ApprovalTemplateField> fields)
        {
            List<string> formauls = [];
            foreach (var condition in conditions)
            {
                var fieldType = fields.First(c => c.FieldNumber == condition.Name);
                if (fieldType.FieldType.Equals(ApproverFieldType.Money) || fieldType.FieldType.Equals(ApproverFieldType.Number))
                {
                    formauls.Add(condition.Name + condition.Symbol + condition.Value);
                }
                else
                {
                    formauls.Add(condition.Name + condition.Symbol + "\"" + condition.Value + "\"");
                }
            }
            return string.Join(" and ", formauls);
        }

        private void RecursionApprovalAuditFlow(WechatApprovalRequest wechatApprovalRequest, List<AuditFlowConfigDetail>? details, List<ApprovalTemplateField> fields, Dictionary<string, dynamic> values, ApprovalRequest request, int id = 0)
        {
            var detail = details?.Where(c => c.ParentId == id).OrderBy(c => c.Seq).OrderBy(c => c.SonId).ToList();
            if (detail == null || detail.Count == 0)
            {
                return;
            }
            if (detail[0].Type == AuditFlowDetailType.Approve)
            {
                var users = JsonSerializerOptionsUtils.Deserialize<List<ApproverUserConfig>>(detail[0].UserCode);
                if (users != null)
                {
                    var approvalUsers = this.FlowUsers(wechatApprovalRequest, users);
                    wechatApprovalRequest.Approver.Add(new ApproverNode
                    {
                        Attr = detail[0].SPType,
                        Userid = approvalUsers.Distinct().ToList()
                    });
                    RecursionApprovalAuditFlow(wechatApprovalRequest, details, fields, values, request, detail[0].SonId);
                }
            }
            if (detail[0].Type == AuditFlowDetailType.Copy)
            {
                var users = JsonSerializerOptionsUtils.Deserialize<List<ApproverUserConfig>>(detail[0].UserCode);
                if (users != null && users.Count > 0)
                {
                    var notifyers = request.Notifyer ?? [];
                    notifyers.AddRange(this.FlowUsers(wechatApprovalRequest, users));
                    wechatApprovalRequest.Notifyer.AddRange(notifyers.Distinct());
                    RecursionApprovalAuditFlow(wechatApprovalRequest, details, fields, values, request, detail[0].SonId);
                }
            }
            if (detail[0].Type == AuditFlowDetailType.Condition)
            {
                int count = 0;
                foreach (var condition in detail.OrderBy(c => c.Seq))
                {
                    if (string.IsNullOrWhiteSpace(condition.Formal))
                    {
                        count += 1;
                        continue;
                    }
                    if (condition.Seq != detail.Count - count)
                    {
                        var conditions = JsonSerializerOptionsUtils.Deserialize<List<ApproverCondition>>(condition.Formal);
                        if (conditions != null)
                        {
                            string formulaString = CreateConditons(conditions, fields);
                            if (_pythonEngineService.CalFormula(formulaString, values))
                            {
                                RecursionApprovalAuditFlow(wechatApprovalRequest, details, fields, values, request, condition.SonId);
                                break;
                            }
                        }
                    }
                    else
                    {
                        RecursionApprovalAuditFlow(wechatApprovalRequest, details, fields, values, request, condition.SonId);
                    }
                }
            }
        }

        private List<string> FlowUsers(WechatApprovalRequest wechatApprovalRequest, List<ApproverUserConfig> users)
        {
            List<string> result = [];
            foreach (var user in users)
            {
                if (user.Iden == ApproverSpecificPrincipal.Departmenthead)
                {
                    var parent = _addressBookServiceClient.GetLevelParent(wechatApprovalRequest.CreatorUserCode);
                    if (parent != null)
                    {
                        result.Add(parent.ParentUserCode);
                    }
                }
                else if (user.Iden == ApproverSpecificPrincipal.Departmentheads)
                {
                    result.AddRange(_addressBookServiceClient.GetParent(wechatApprovalRequest.CreatorUserCode).Select(p => p.ParentUserCode).Distinct());
                }
                else if (user.Iden == ApproverSpecificPrincipal.CreateUser)
                {
                    result.Add(wechatApprovalRequest.CreatorUserCode);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(user.Id))
                    {
                        result.Add(user.Id);
                    }
                }
            }

            return result;
        }

        private static Dictionary<string, dynamic> Transform<Q>(Q request) where Q : ApprovalRequest
        {
            Dictionary<string, dynamic> Keys = [];
            var properties = request.GetType().GetProperties();
            foreach (System.Reflection.PropertyInfo info in properties)
            {
                var value = info.GetValue(request, null);
                if (value != null)
                {
                    Keys.Add(info.Name, value);
                }
            }
            return Keys;
        }

        private static void AssignCreateUser(WechatApprovalRequest wechatApprovalRequest, AuditFlowConfig auditFlowConfig)
        {
            var userCode = auditFlowConfig.AuditFlowConfigDetails?.FirstOrDefault(it => it.Type == AuditFlowDetailType.CreateUser)?.UserCode;
            if (!string.IsNullOrWhiteSpace(userCode))
            {
                var createUser = JsonSerializerOptionsUtils.Deserialize<List<ApproverUserConfig>>(userCode);
                if (createUser != null && createUser.Count > 0)
                {
                    var userId = createUser[0].Id;
                    if (userId != null)
                    {
                        wechatApprovalRequest.CreatorUserCode = userId;
                    }
                }
            }
        }

        private async Task CreateFileContent<Q>(Q request, WechatApprovalRequest wechatApprovalRequest, ApprovalTemplateField field) where Q : ApprovalRequest
        {
            List<MediaInfo>? mediaInfos = (List<MediaInfo>?)field.PropertyInfo?.GetValue(request, null);
            //判断是否需要上传文件
            if (mediaInfos != null && mediaInfos.Count > 0)
            {
                List<string> files = [];
                foreach (var mediaInfo in mediaInfos)
                {
                    if (!string.IsNullOrWhiteSpace(mediaInfo.MediaId))
                    {
                        files.Add(mediaInfo.MediaId);
                    }
                    else
                    {
                        mediaInfo.SystemCode = "weixinwork-Approval";
                        mediaInfo.EnvCode = "production";
                        //下载文件
                        var upLoadMediaResponese = await _mediaServiceClient.UpLoadMediaAsync(mediaInfo);
                        if (upLoadMediaResponese.ErrCode == 0)
                        {
                            files.Add(upLoadMediaResponese.MediaId);
                        }
                        else
                        {
                            throw new Exception(string.Format("调用企业微信发生错误:{0} json : {1}", upLoadMediaResponese.ErrorMessage, JsonSerializerOptionsUtils.Serialize(request)));
                        }
                    }
                }
                dynamic value = new { files = (from r in files select new { file_id = r }).ToArray() };
                wechatApprovalRequest.ApplyData.Contents.Add(_createControl[field.FieldType](field.FieldId, field.FieldName, value));
            }
        }

        /// <summary>
        /// 获取审批模板
        /// </summary>
        /// <typeparam name="Q"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private ApprovalTemplateAttribute GetApprovalTemplate<Q>() where Q : ApprovalRequest
        {
            Type type = typeof(Q);
            var template = _approvalTemplate.GetOrAdd(type, (t) =>
            {
                if (type.GetCustomAttributes(true).FirstOrDefault(p => p is ApprovalTemplateAttribute) is not ApprovalTemplateAttribute key)
                {
                    throw new ArgumentNullException(nameof(type));
                }
                if (!_redisCache.HashExists(new ApprovalTemplate() { TemplateId = key.TemplateId }))
                {
                    throw new ArgumentNullException("此模板Id不存在");
                }
                return key;
            });
            return template;
        }

        /// <summary>
        /// 获取审批模板字段
        /// </summary>
        /// <typeparam name="Q"></typeparam>
        /// <returns></returns>
        private List<ApprovalTemplateField> GetApprovalTemplateFields<Q>(ApprovalTemplateAttribute template) where Q : ApprovalRequest
        {
            Type type = typeof(Q);
            ApprovalTemplate approvalTemplate = new() { TemplateId = template.TemplateId };
            var approvalTemplateFields = _redisCache.HashGet(approvalTemplate, p => p.ApprovalTemplateFields);
            if (approvalTemplateFields != null)
            {
                var fields = _approvalTemplateFields.GetOrAdd(type, (t) =>
                {
                    var properties = type.GetProperties();
                    List<ApprovalTemplateField> result = [];
                    foreach (var propertyInfo in properties)
                    {
                        var field = approvalTemplateFields.FirstOrDefault(p => p.FieldNumber == propertyInfo.Name);
                        if (field != null)
                        {
                            field.PropertyInfo = propertyInfo;
                            result.Add(field);
                        }
                    }
                    return result;
                });
                return [.. fields.OrderBy(p => p.ApprovalSeq)];
            }
            return [];
        }

        /// <summary>
        /// 创建文件控件
        /// </summary>
        /// <param name="controlId">控件id</param>
        /// <param name="controlName">控件名称</param>
        /// <param name="value">控件值</param>
        /// <returns>文件控件</returns>
        public static dynamic CreateTextValue(string controlId, string controlName, dynamic value)
        {
            return new
            {
                control = "Text",
                id = controlId,
                title = new List<ControlTitle>
                {
                    new()
                    {
                        Text = controlName
                    }
                },
                value = new
                {
                    text = value ?? ""
                }
            };
        }

        /// <summary>
        /// 创建日期
        /// </summary>
        /// <param name="controlId">控件id</param>
        /// <param name="controlName">控件名称</param>
        /// <param name="value">控件值</param>
        /// <returns>日期</returns>
        public static dynamic CreateDateValue(string controlId, string controlName, dynamic value)
        {
            return new
            {
                control = "Date",
                id = controlId,
                title = new List<ControlTitle>
                {
                    new()
                    {
                        Text = controlName
                    }
                },
                value = new
                {
                    date = new
                    {
                        s_timestamp = ((value.Date.ToUniversalTime().Ticks - 621355968000000000) / 10000000).ToString()
                    }
                }
            };
        }

        private static dynamic CreateContactValue(string controlId, string controlName, dynamic value)
        {
            return new
            {
                control = "Contact",
                id = controlId,
                title = new List<ControlTitle>
                {
                    new()
                    {
                        Text = controlName
                    }
                },
                value = new
                {
                    members = new dynamic[]
                    {
                        value
                    }
                }
            };
        }

        /// <summary>
        /// 创建选择器控件
        /// </summary>
        /// <param name="controlId"></param>
        /// <param name="controlName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dynamic CreateSelectorValue(string controlId, string controlName, dynamic value)
        {
            return new
            {
                control = "Selector",
                id = controlId,
                title = new List<ControlTitle>
                {
                    new()
                    {
                        Text = controlName
                    }
                },
                value = new
                {
                    selector = new
                    {
                        type = "single",
                        options = new dynamic[]
                        {
                            new {
                                key = value
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// 创建多行文件控件
        /// </summary>
        /// <param name="controlId">控件id</param>
        /// <param name="controlName">控件名称</param>
        /// <param name="value">控件值</param>
        /// <returns>文件控件</returns>
        public static dynamic CreateTextareaValue(string controlId, string controlName, dynamic value)
        {
            return new
            {
                control = "Textarea",
                id = controlId,
                title = new List<ControlTitle>
                {
                    new()
                    {
                        Text = controlName
                    }
                },
                value = new
                {
                    text = value ?? ""
                }
            };
        }
        /// <summary>
        /// 创建金额控件
        /// </summary>
        /// <param name="controlId">控件id</param>
        /// <param name="controlName">控件名称</param>
        /// <param name="value">控件值</param>
        /// <returns>金额控件</returns>
        public static dynamic CreateMoneyValue(string controlId, string controlName, dynamic value)
        {
            return new
            {
                control = "Money",
                id = controlId,
                title = new List<ControlTitle>
                {
                    new()
                    {
                        Text = controlName
                    }
                },
                value = new
                {
                    new_money = value < 0 ? "0" : value.ToString("#0.00")
                }
            };
        }

        /// <summary>
        /// 创建数字控件
        /// </summary>
        /// <param name="controlId">控件id</param>
        /// <param name="controlName">控件名称</param>
        /// <param name="value">控件值</param>
        /// <returns>数字控件</returns>
        public static dynamic CreateNumberValue(string controlId, string controlName, dynamic value)
        {
            return new
            {
                control = "Number",
                id = controlId,
                title = new List<ControlTitle>
                {
                    new()
                    {
                        Text = controlName
                    }
                },
                value = new
                {
                    new_number = value == 0 ? "" : value.ToString("#0.00")
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlId"></param>
        /// <param name="controlName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dynamic CreateFileValue(string controlId, string controlName, dynamic value)
        {
            return new
            {
                control = "File",
                id = controlId,
                title = new List<ControlTitle>
                {
                    new()
                    {
                        Text = controlName
                    }
                },
                value
            };
        }
    }
}
