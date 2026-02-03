using Kingdee.Mymooo.Contracts.DirectSaleManagement;
using Kingdee.Mymooo.Contracts;
using Kingdee.Mymooo.Core.DirectSaleManagement;
using Kingdee.Mymooo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.Mymooo.Contracts.PurchaseManagement;
using Kingdee.Mymooo.Core.PurchaseManagement;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.BOS.Core.Msg;

namespace Kingdee.Mymooo.ServiceHelper.PurRequisitionManagement
{
    public class PurRequisitionBillServiceHelper
    {
        public static ResponseMessage<dynamic> Add_PUR_Requisition(Context ctx, PUR_Requisition message)
        {
            IPur_RequisitionOrderService service = ServiceFactory.GetService<IPur_RequisitionOrderService>(ctx);
            try
            {
                return service.Add_PUR_Requisition(ctx, message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ServiceFactory.CloseService(service);
            }
        }

        public static IOperationResult Del_PUR_Requisition(Context ctx, string message)
        {
            IPur_RequisitionOrderService service = ServiceFactory.GetService<IPur_RequisitionOrderService>(ctx);
            try
            {
                return service.Del_PUR_Requisition(ctx, message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ServiceFactory.CloseService(service);
            }
        }
    }
}
