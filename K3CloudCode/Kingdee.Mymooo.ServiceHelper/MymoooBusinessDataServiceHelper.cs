using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.Operation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.Mymooo.Contracts;
using Kingdee.Mymooo.Core;
using System;

namespace Kingdee.Mymooo.ServiceHelper
{
    /// <summary>
    /// 业务层调用逻辑处理层
    /// </summary>
    public class MymoooBusinessDataServiceHelper
    {
        public static IOperationResult SaveAndAuditBill(Context ctx, BusinessInfo businessInfo, params DynamicObject[] dynamicObjects)
        {
            IMymoooBusinessDataService service = ServiceFactory.GetService<IMymoooBusinessDataService>(ctx);
            try
            {
                return service.SaveAndAuditBill(ctx, businessInfo, dynamicObjects);
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

        public static IOperationResult SubmitBill(Context ctx, BusinessInfo businessInfo, params object[] ids)
        {
            IMymoooBusinessDataService service = ServiceFactory.GetService<IMymoooBusinessDataService>(ctx);
            try
            {
                return service.SubmitBill(ctx, businessInfo, ids);
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

        public static IOperationResult Audit(Context ctx, BusinessInfo businessInfo, params object[] ids)
        {
            IMymoooBusinessDataService service = ServiceFactory.GetService<IMymoooBusinessDataService>(ctx);
            try
            {
                return service.Audit(ctx, businessInfo, ids);
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

        public static IOperationResult UnAudit(Context ctx, BusinessInfo businessInfo, params object[] ids)
        {
            IMymoooBusinessDataService service = ServiceFactory.GetService<IMymoooBusinessDataService>(ctx);
            try
            {
                return service.UnAudit(ctx, businessInfo, ids);
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

        public static IOperationResult DeleteBill(Context ctx, BusinessInfo businessInfo, params object[] ids)
        {
            IMymoooBusinessDataService service = ServiceFactory.GetService<IMymoooBusinessDataService>(ctx);
            try
            {
                return service.DeleteBill(ctx, businessInfo, ids);
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

        public static IOperationResult SaveBill(Context ctx, BusinessInfo businessInfo, params DynamicObject[] dynamicObjects)
        {
            IMymoooBusinessDataService service = ServiceFactory.GetService<IMymoooBusinessDataService>(ctx);
            try
            {
                return service.SaveBill(ctx, businessInfo, dynamicObjects);
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

		public static IOperationResult SaveBill(Context ctx, BusinessInfo businessInfo, bool isRemoveValidators, params DynamicObject[] dynamicObjects)
		{
			IMymoooBusinessDataService service = ServiceFactory.GetService<IMymoooBusinessDataService>(ctx);
			try
			{
				return service.SaveBill(ctx, businessInfo, isRemoveValidators, dynamicObjects);
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
		
		public static IOperationResult SetBillStatus(Context ctx, BusinessInfo businessInfo, object[] ids, string operationNumber)
        {
            IMymoooBusinessDataService service = ServiceFactory.GetService<IMymoooBusinessDataService>(ctx);
            try
            {
                return service.SetBillStatus(ctx, businessInfo, ids, operationNumber);
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

        public static IOperationResult Allocate(Context ctx, AllocateParameter allocateParameter)
        {
            IMymoooBusinessDataService service = ServiceFactory.GetService<IMymoooBusinessDataService>(ctx);
            try
            {
                return service.Allocate(ctx, allocateParameter);
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

        public static ResponseMessage<long> AddRabbitMqMeaage(Context ctx, string action, string keyword, string data)
        {
            IMymoooBusinessDataService service = ServiceFactory.GetService<IMymoooBusinessDataService>(ctx);
            try
            {
                return service.AddRabbitMqMeaage(ctx, action, keyword, data);
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

        public static ResponseMessage<dynamic> AddRabbitMqMeaageResult(Context ctx, string action, string keyword, string data, bool isSucceed, string result)
        {
            IMymoooBusinessDataService service = ServiceFactory.GetService<IMymoooBusinessDataService>(ctx);
            try
            {
                return service.AddRabbitMqMeaageResult(ctx, action, keyword, data, isSucceed, result);
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
