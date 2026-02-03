using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.Mymooo.Business.PlugIn.BaseManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.WebApi.ServicesStub
{
    public class SupplierManagermentService : KDBaseService
    {
        public SupplierManagermentService(KDServiceContext context) : base(context)
        {
        }
		//供应商供应产品小类
		public string SyncSupplierSmall()
        {
            string data = string.Empty;
            try
            {
                using (var reader = new StreamReader(KDContext.WebContext.Context.Request.InputStream, Encoding.UTF8))
                {
                    var task = reader.ReadToEndAsync();
                    data = task.Result;
                }
                var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
                SupplierBusiness supplierBusiness = new SupplierBusiness();
                return JsonConvertUtils.SerializeObject(supplierBusiness.SupplierSmall(ctx, data));
            }
            catch (Exception ex)
            {
                return JsonConvertUtils.SerializeObject(new ResponseMessage<dynamic>() { Code = ResponseCode.Exception, ErrorMessage = ex.Message });
            }
        }
		/// <summary>
		/// 新增供应商小类检验评分
		/// </summary>
		/// <returns></returns>
		public string AddSupplierClassInspectScore()
		{
			string data = string.Empty;
			try
			{
				var ctx = LoginServiceUtils.SignLogin(KDContext.WebContext.Context.Request);
				SupplierBusiness supplierBusiness = new SupplierBusiness();
				return JsonConvertUtils.SerializeObject(supplierBusiness.AddSupplierClassInspectScore(ctx));
			}
			catch (Exception ex)
			{
				return JsonConvertUtils.SerializeObject(new ResponseMessage<dynamic>() { Code = ResponseCode.Exception, ErrorMessage = ex.Message });
			}
		}
	}
}
