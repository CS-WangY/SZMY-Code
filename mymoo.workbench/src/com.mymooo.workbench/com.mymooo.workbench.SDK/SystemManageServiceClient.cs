using com.mymooo.workbench.core.SystemManage;
using mymooo.core;
using mymooo.core.Account;
using mymooo.core.Attributes;
using mymooo.core.Utils.Service;

namespace com.mymooo.workbench.SDK
{
    /// <summary>
    /// 系统管理
    /// </summary>
    /// <param name="httpService"></param>
    [AutoInject(InJectType.Single)]
    public class SystemManageServiceClient<C, U>(HttpService httpService) where U : UserBase, new() where C : MymoooContext<U>
    {
        private readonly HttpService _httpService = httpService;

        /// <summary>
        /// 获取部门详情
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseMessage<dynamic>> GetDepartment(C mymoooContext)
        {
            return await _httpService.InvokeWebServiceAsync<C, U, dynamic>(mymoooContext, $"workbench/{mymoooContext.ApigatewayConfig.EnvCode}/SystemManage/GetDepartment");
        }

        /// <summary>
        /// 模糊查询员工信息
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseMessage<List<MenuModel>>> FuzzyQuery(C mymoooContext, string code, int count = 5, bool isAssistant = false, bool isMymo = false)
        {
            return await _httpService.InvokeWebServiceAsync<C, U, List<MenuModel>>(mymoooContext, $"workbench/{mymoooContext.ApigatewayConfig.EnvCode}/SystemManage/FuzzyQuery?code={code}&count={count}&=isAssistant{isAssistant}&isMymo={isMymo}");
        }
    }
}
