using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper.BaseManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;

namespace Kingdee.Mymooo.Business.PlugIn.BaseManagement
{
    /// <summary>
    /// 供应商分配组织
    /// </summary>
    public class SupplierAllotOrgBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            var request = JsonConvert.DeserializeObject<List<SupplierAllotOrgRequest>>(message);
            if (request == null)
            {
                response.Message = "参数信息不能为空";
                response.Code = ResponseCode.NoExistsData;
                return response;
            }
            return BasicDataSyncServiceHelper.AddSupplierAllotOrg(ctx, request);
        }
    }
}
