using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Cache;
using mymooo.weixinWork.SDK.AddressBook.Model;
using mymooo.weixinWork.SDK.Application.Model;
using mymooo.weixinWork.SDK.Utils;

namespace mymooo.weixinWork.SDK.AddressBook
{
    /// <summary>
    /// 通讯录
    /// </summary>
    [AutoInject(InJectType.Single)]
    public class AddressBookServiceClient(WeixinWorkHttpService httpService, RedisCache redisCache)
    {
        private readonly WeixinWorkHttpService _httpService = httpService;
        private readonly RedisCache _redisCache = redisCache;
        private readonly string _systemCode = "weixinwork-AddressBook";

        /// <summary>
        /// 获取第几级父级用户信息
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public ParentUserInfo? GetLevelParent(string userCode, int level = 1)
        {
            ArgumentNullException.ThrowIfNull(nameof(userCode));
            return _redisCache.HashGet(new ParentUserInfo { UserCode = userCode, RowIndex = level }, "parent-user");
        }

        /// <summary>
        /// 获取第几级父级用户信息
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public List<ParentUserInfo> GetParent(string userCode)
        {
            ArgumentNullException.ThrowIfNull(nameof(userCode));
            return _redisCache.HashGetMatchs(new ParentUserInfo { UserCode = userCode }, "parent-user-*");
        }

        /// <summary>
        /// 获取用户的二维码
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public string? GetUserQrCode(string userCode)
        {
            ArgumentNullException.ThrowIfNull(nameof(userCode));
            return _redisCache.HashGet(new MymoooUser { UserId = userCode }, p => p.QrCode);
        }

        /// <summary>
        /// 异步 获取用户的二维码
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public async Task<string?> GetUserQrCodeAsync(string userCode)
        {
            ArgumentNullException.ThrowIfNull(nameof(userCode));
            return await _redisCache.HashGetAsync(new MymoooUser { UserId = userCode }, p => p.QrCode);
        }

        /// <summary>
        /// 获取访问用户身份
        /// </summary>
        /// <typeparam name="C">蚂蚁上下文</typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<UserInfoResponse>> GetUserInfo<C, U>(C mymoooContext, string userid) where U : UserBase, new() where C : MymoooContext<U>
        {
            return await _httpService.InvokeWebServiceAsync<C, U, UserInfoResponse>(mymoooContext, $"{_systemCode}/production/cgi-bin/user/get?userid={userid}");
        }

        /// <summary>
        /// 获取部门列表
        /// </summary>
        /// <typeparam name="C">蚂蚁上下文</typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="departmentId">部门Id</param>
        /// <returns></returns>
        public async Task<ResponseMessage<DepartmentListResponse>> GetDepartmentList<C, U>(C mymoooContext, long departmentId = 0) where U : UserBase, new() where C : MymoooContext<U>
        {
            return await _httpService.InvokeWebServiceAsync<C, U, DepartmentListResponse>(mymoooContext, $"{_systemCode}/production/cgi-bin/department/list?&id={departmentId}");
        }

        /// <summary>
        /// 获取部门成员
        /// </summary>
        /// <typeparam name="C">蚂蚁上下文</typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="mymoooContext">蚂蚁上下文</param>
        /// <param name="departmentId">部门Id</param>
        /// <param name="fetchChild">是否递归子部门</param>
        /// <returns></returns>
        public async Task<ResponseMessage<SimpleDepartmentResponse>> GetDepartmentUsers<C, U>(C mymoooContext, long departmentId = 0, int fetchChild = 0) where U : UserBase, new() where C : MymoooContext<U>
        {
            return await _httpService.InvokeWebServiceAsync<C, U, SimpleDepartmentResponse>(mymoooContext, $"{_systemCode}/production/cgi-bin/user/simplelist?&department_id={departmentId}&fetch_child={fetchChild}");
        }
    }
}
