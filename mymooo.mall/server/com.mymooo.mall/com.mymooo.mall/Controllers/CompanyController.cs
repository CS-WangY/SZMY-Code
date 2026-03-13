using com.mymooo.mall.business.Service.BaseService;
using com.mymooo.mall.core.Account;
using com.mymooo.mall.core.SqlSugarCore.BaseInformation.CustomerInformation;
using Microsoft.AspNetCore.Mvc;
using mymooo.core;
using mymooo.core.Utils;

namespace com.mymooo.mall.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class CompanyController(MallContext mallContext, CompanyService companyService) : Controller
    {
        private readonly MallContext _mallContext = mallContext;
        private readonly CompanyService _companyService = companyService;

        /// <summary>
        /// 添加绑定关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IActionResult> BindRelation([FromBody] SubAndParentCompany request)
        {
            var result = await this.ModelVerify(request);
            if (!result.IsSuccess)
            {
                return Json(result);
            }
            result = await _companyService.BindRelation(request);
            return Json(result);
        }

        /// <summary>
        /// 清除绑定关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IActionResult> UnBindRelation([FromBody] SubAndParentCompany request)
        {
            ResponseMessage<SubAndParentCompany> result = new();
            if (request.Id == 0)
            {
                result.Code = ResponseCode.abort;
                return Json(result);
            }
            result = await _companyService.UnBindRelation(request);
            return Json(result);
        }

        /// <summary>
        /// 更新集团客户缓存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
		public async Task<IActionResult> UpdateCompanyGroupCache([FromBody] SubAndParentCompany request)
        {
			var result = await this.ModelVerify(request);
			if (!result.IsSuccess)
			{
				return Json(result);
			}
			var response = await _companyService.UpdateCompanyGroupCache(request);
			return Json(response);
		}
	}
}
