using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.Mymooo.Contracts;
using Kingdee.Mymooo.Contracts.ProductionManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ProductionManagement;
using Kingdee.Mymooo.Core.ProductionManagement.Dispatch;
using System;
using System.Collections.Generic;

namespace Kingdee.Mymooo.ServiceHelper.ProductionManagement
{
    public class ProductionMoServiceHelper
    {
        public static IOperationResult SendMakeDispatch(Context ctx, long entryId)
        {
            IProductionMoService service = ServiceFactory.GetService<IProductionMoService>(ctx);
            try
            {
                return service.SendMakeDispatch(ctx, entryId);
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

        public static IOperationResult SendMakeDispatchs(Context ctx, SendMakeDispatchRequest request)
        {
            IProductionMoService service = ServiceFactory.GetService<IProductionMoService>(ctx);
            try
            {
                return service.SendMakeDispatchs(ctx, request);
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

        public static IOperationResult SendMakeDispatchs(Context ctx, List<long> entryIds)
        {
            IProductionMoService service = ServiceFactory.GetService<IProductionMoService>(ctx);
            try
            {
                return service.SendMakeDispatchs(ctx, entryIds);
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

        public static IOperationResult SendMakeDispatchForIds(Context ctx, List<long> headIds)
        {
            IProductionMoService service = ServiceFactory.GetService<IProductionMoService>(ctx);
            try
            {
                return service.SendMakeDispatchForIds(ctx, headIds);
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

        public static IOperationResult CancelMakeDispatch(Context ctx, MakeDispatchRequest request)
        {
            IProductionMoService service = ServiceFactory.GetService<IProductionMoService>(ctx);
            try
            {
                return service.CancelMakeDispatch(ctx, request);
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

        public static IOperationResult CancelMakeDispatchs(Context ctx, List<long> entryIds)
        {
            IProductionMoService service = ServiceFactory.GetService<IProductionMoService>(ctx);
            try
            {
                return service.CancelMakeDispatchs(ctx, entryIds);
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

        public static IOperationResult CancelMakeDispatchForIds(Context ctx, List<long> headIds)
        {
            IProductionMoService service = ServiceFactory.GetService<IProductionMoService>(ctx);
            try
            {
                return service.CancelMakeDispatchForIds(ctx, headIds);
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

        public static ResponseMessage<dynamic> MesProductionMoStatus(Context ctx, SendMesMakeResponse request)
        {
            IProductionMoService service = ServiceFactory.GetService<IProductionMoService>(ctx);
            try
            {
                return service.MesProductionMoStatus(ctx, request);
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

        public static ResponseMessage<dynamic> MesCancelMoStatus(Context ctx, SendMesMakeResponse request)
        {
            IProductionMoService service = ServiceFactory.GetService<IProductionMoService>(ctx);
            try
            {
                return service.MesCancelMoStatus(ctx, request);
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

        public static ResponseMessage<dynamic> SendMakeDispatchForBill(Context ctx, MakeRequest request)
        {
            IProductionMoService service = ServiceFactory.GetService<IProductionMoService>(ctx);
            try
            {
                return service.SendMakeDispatchForBill(ctx, request);
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
        /// <summary>
        /// Mes一键退料
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static ResponseMessage<dynamic> MesOneClickReturnMaterial(Context ctx, MesOneClickReturnMaterialRequest request)
        {
            IProductionMoService service = ServiceFactory.GetService<IProductionMoService>(ctx);
            try
            {
                return service.MesOneClickReturnMaterial(ctx, request);
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
