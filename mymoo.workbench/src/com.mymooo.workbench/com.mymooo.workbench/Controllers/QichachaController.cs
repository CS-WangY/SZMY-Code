using com.mymooo.workbench.qichacha;
using Microsoft.AspNetCore.Mvc;

namespace com.mymooo.workbench.Controllers
{
    /// <summary>
    /// 企查查
    /// </summary>
    [Route("[controller]/[action]")]
    [ApiController]
    public class QichachaController(QichachaService qichachaService) : Controller
    {
        private readonly QichachaService _qichachaService = qichachaService;

        /// <summary>
        /// 模糊查询
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ECIV4_Search(string keyword)
        {
            return Json(_qichachaService.ECIV4_Search(keyword));
        }

        /// <summary>
        /// 多维度查询
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ECIV4_SearchWide(string keyword)
        {
            return Json(_qichachaService.ECIV4_SearchWide(keyword));
        }

        /// <summary>
        /// 企业工商详情
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ECIV4_GetBasicDetailsByName(string keyword)
        {
            return Json(_qichachaService.ECIV4_GetBasicDetailsByName(keyword));
        }

        /// <summary>
        /// 被执行人核查
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ZhixingCheck(string keyword,string code, bool isOnlyCashe = false)
        {
            return Json(_qichachaService.ZhixingCheck(keyword,code,isOnlyCashe)) ;
        }

        /// <summary>
        /// 立案信息核查
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CaseFilingCheck(string keyword,string code, bool isOnlyCashe = false)
        {
            return Json(_qichachaService.CaseFilingCheck(keyword, code, isOnlyCashe));
        }

        /// <summary>
        /// 税收违法核查
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult TaxIllegalCheck(string keyword,string code, bool isOnlyCashe = false)
        {
            return Json(_qichachaService.TaxIllegalCheck(keyword, code, isOnlyCashe));
        }

        /// <summary>
        /// 严重违法核查
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult SeriousIllegalCheck(string keyword,string code, bool isOnlyCashe = false)
        {
            return Json(_qichachaService.SeriousIllegalCheck(keyword, code, isOnlyCashe));
        }
    }
}
