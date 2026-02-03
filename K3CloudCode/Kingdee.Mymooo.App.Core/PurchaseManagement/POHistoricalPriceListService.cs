using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.Mymooo.Contracts.PurchaseManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.App.Core.PurchaseManagement
{
    /// <summary>
    /// 历史价供应商查询,23.05.11取消
    /// </summary>
    public class POHistoricalPriceListService : IPOHistoricalPriceListService
    {
        public ResponseMessage<dynamic> GetHistoricalPriceList(Context ctx, string sDate)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
            try
            {
                var SDate = Convert.ToDateTime(sDate);
            }
            catch (Exception ex)
            {
                response = new ResponseMessage<dynamic>()
                {
                    Code = ResponseCode.Exception,
                    Message = ex.Message
                };
            }
            return response;
        }
    }
}
