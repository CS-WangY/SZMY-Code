using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.Mymooo.Business.PlugIn.PurchaseManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Business.PlugIn.PayableManagement;

namespace Kingdee.Mymooo.WebApi.ServicesStub
{
	/// <summary>
	/// 应付款管理
	/// </summary>
	public class PayableManagementService : KDBaseService
	{
		public PayableManagementService(KDServiceContext context) : base(context)
		{
		}

		/// <summary>
		/// MES费用采购下推费用应付
		/// </summary>
		public string MesCostPurGenerateCostPayable()
		{
			string data = string.Empty;
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			try
			{
				using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
				{
					var task = reader.ReadToEndAsync();
					data = task.Result;
				}
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				PayableOrderBusiness business = new PayableOrderBusiness();
				response = business.MesCostPurGenerateCostPayable(ctx, data);
			}
			catch (Exception ex)
			{
				response = new ResponseMessage<dynamic>()
				{
					Code = ResponseCode.Exception,
					Message = ex.Message
				};
			}
			return JsonConvertUtils.SerializeObject(response);
		}

        /// <summary>
        /// 费用应付删除
        /// </summary>
        /// <returns></returns>
        public string CostPayableOrderDelete()
        {
            string data = string.Empty;
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                PayableOrderBusiness business = new PayableOrderBusiness();
                response = business.CostPayableOrderDelete(ctx, data);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return JsonConvertUtils.SerializeObject(response);
        }
    }
}
