using Microsoft.AspNetCore.Mvc;
using mymooo.core.Model.BussinessModel.K3Cloud;
using mymooo.core.Model.BussinessModel.K3Cloud.Finance;
using mymooo.core.Utils;
using mymooo.k3cloud.business.Services;

namespace mymooo.k3cloud.Controllers
{
	/// <summary>
	/// 财务
	/// </summary>
	public class FinanceController(FinanceService financeService) : Controller
	{
		private readonly FinanceService _financeService = financeService;

		/// <summary>
		/// 核销记录
		/// </summary>
		/// <returns></returns>
		public async Task<IActionResult> MatchRecord([FromBody] K3CloudRabbitMQMessage<MatchRecord, MatchRecord> request, CancellationToken cancellationToken = default)
		{
			var result = await this.ModelVerify(request);
			if (!result.IsSuccess)
			{
				return Json(result);
			}
			return Json(_financeService.MatchRecord(request, cancellationToken));
		}
	}
}
