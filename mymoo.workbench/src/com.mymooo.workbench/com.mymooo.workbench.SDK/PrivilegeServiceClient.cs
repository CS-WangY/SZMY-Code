using com.mymooo.workbench.core.SystemManage;
using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Utils.Service;

namespace com.mymooo.workbench.SDK
{
    /// <summary>
    /// 权限
    /// </summary>
    [AutoInject(InJectType.Single)]
    public class PrivilegeServiceClient<C, U>(HttpService httpService) where U : UserBase, new() where C : MymoooContext<U>
    {
        private readonly HttpService _httpService = httpService;

        /// <summary>
        /// 获取菜单权限
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseMessage<List<MenuModel>>> GetSystemMenu(C mymoooContext)
        {
            return await _httpService.InvokeWebServiceAsync<C, U, List<MenuModel>>(mymoooContext, $"workbench/{mymoooContext.ApigatewayConfig.EnvCode}/Privilege/GetMenu?SystemCode={mymoooContext.ApigatewayConfig.SystemCode}");
        }

        /// <summary>
        /// 获取是否有功能权限
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseMessage<List<MenuModel>>> GetFunction(C mymoooContext, string path) 
        {
            return await _httpService.InvokeWebServiceAsync<C, U, List<MenuModel>>(mymoooContext, $"workbench/{mymoooContext.ApigatewayConfig.EnvCode}/Privilege/GetFunction?SystemCode={mymoooContext.ApigatewayConfig.SystemCode}&path={path}");
        }

        /// <summary>
        /// 获取用户下全部用户,当前登录用户的关联权限
        /// </summary>
        /// <param name="mymoooContext"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<List<AuthorityUser>>> GetAuthorityUserList(C mymoooContext)
        {
            return await _httpService.InvokeWebServiceAsync<C, U, List<AuthorityUser>>(mymoooContext, $"workbench/{mymoooContext.ApigatewayConfig.EnvCode}/Privilege/GetAuthorityUserList");
        }

        /// <summary>
        /// 获取用户下全部用户,第二个参数未指定用户关联权限,不一定是当前用户
        /// </summary>
        /// <param name="mymoooContext"></param>
        /// <param name="wechatCode"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<List<AuthorityUser>>> GetAuthorityUserList(C mymoooContext,string wechatCode)
        {
            return await _httpService.InvokeWebServiceAsync<C, U, List<AuthorityUser>>(mymoooContext, $"workbench/{mymoooContext.ApigatewayConfig.EnvCode}/Privilege/GetAuthorityUserListByWechatCode?wechatCode={wechatCode}");
        }


        /// <summary>
        /// 判断某用户是否管理员或所有数据权限
        /// </summary>
        /// <param name="mymoooContext"></param>
        /// <param name="wechatCode"></param>
        /// <returns></returns>
        public async Task<ResponseMessage<bool>> IsAdminOrAllDataPrivilege(C mymoooContext,string wechatCode)
        {
            return await _httpService.InvokeWebServiceAsync<C, U, bool>(mymoooContext, $"workbench/{mymoooContext.ApigatewayConfig.EnvCode}/Privilege/IsAdminOrAllDataPrivilege?wechatCode={wechatCode}");
        }



    }
}
