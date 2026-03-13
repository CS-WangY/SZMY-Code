using com.mymooo.mall.business.Service.SystemService;
using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.Model.SystemManage;
using Microsoft.AspNetCore.Mvc;
using mymooo.core;

namespace com.mymooo.mall.Controllers
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="systemWebParamService"></param>
    /// <param name="mallContext"></param>
	public class SystemController(SystemWebParamService systemWebParamService, MallContext mallContext) : Controller
	{
		private readonly SystemWebParamService _systemWebParamService = systemWebParamService;
        private readonly MallContext _mymoooContext = mallContext;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ResponseMessage<List<WebParamModel>> WebSiteParamConfigList()
        {
            ResponseMessage<List<WebParamModel>> result = new();
            result.Data = _systemWebParamService.GetAllWebParam();
            result.Code = ResponseCode.Success;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public ResponseMessage<string>  SaveSiteParamConfig(KeyValuePair<string,string> req)
        {
            ResponseMessage<string> result = new();
            result.Code = ResponseCode.Success;
            result.Message = _systemWebParamService.SaveSiteParamConfig(req);
            return result;
        }



    }
}
