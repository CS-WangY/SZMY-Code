using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ProductionManagement;
using Kingdee.Mymooo.Core.ProductionManagement.Dispatch;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.ProductionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.ProductionManagement
{
	public class ProductionMoBussiness
	{
		public ResponseMessage<dynamic> MesCancelMoStatus(Context ctx, string data)
		{
			var request = JsonConvertUtils.DeserializeObject<SendMesMakeResponse>(data);
			return ProductionMoServiceHelper.MesCancelMoStatus(ctx, request);
		}

		public ResponseMessage<dynamic> MesProductionMoStatus(Context ctx, string data)
		{
			var request = JsonConvertUtils.DeserializeObject<SendMesMakeResponse>(data);

			return ProductionMoServiceHelper.MesProductionMoStatus(ctx, request);
		}
        /// <summary>
        /// Mes一键退料
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> MesOneClickReturnMaterial(Context ctx, string data)
        {
            var request = JsonConvertUtils.DeserializeObject<MesOneClickReturnMaterialRequest>(data);

            return ProductionMoServiceHelper.MesOneClickReturnMaterial(ctx, request);
        }
    }
}
