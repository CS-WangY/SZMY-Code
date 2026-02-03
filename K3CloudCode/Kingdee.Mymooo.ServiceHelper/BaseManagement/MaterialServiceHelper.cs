using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.Mymooo.Contracts;
using Kingdee.Mymooo.Contracts.BaseManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Core.ProductionManagement;
using Kingdee.Mymooo.Core.Common;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;

namespace Kingdee.Mymooo.ServiceHelper.BaseManagement
{
    public class MaterialServiceHelper
    {
        public static MaterialSmallBusinessDivision GetMaterialSmallBusinessDivision(Context ctx, long materialId)
        {
            IMaterialService service = ServiceFactory.GetService<IMaterialService>(ctx);
            try
            {
                return service.GetMaterialSmallBusinessDivision(ctx, materialId);
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

        public static MaterialInfo TryGetOrAdd(Context ctx, MaterialInfo material, List<long> useorgid)
        {
            IMaterialService service = ServiceFactory.GetService<IMaterialService>(ctx);
            try
            {
                return service.TryGetOrAdd(ctx, material, useorgid);
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
        public static MaterialInfo TryBomGetOrAdd(Context ctx, MaterialInfo material)
        {
            IMaterialService service = ServiceFactory.GetService<IMaterialService>(ctx);
            try
            {
                return service.TryBomGetOrAdd(ctx, material);
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

        public static MaterialInfo[] TryGetOrAdds(Context ctx, MaterialInfo[] materials, List<long> useorgid)
        {
            IMaterialService service = ServiceFactory.GetService<IMaterialService>(ctx);
            try
            {
                return service.TryGetOrAdds(ctx, materials, useorgid);
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

        public static ResponseMessage<dynamic> GroupSave(Context ctx, List<SalesOrderBillRequest.Productsmallclass> datas)
        {
            IMaterialService service = ServiceFactory.GetService<IMaterialService>(ctx);
            try
            {
                return service.GroupSave(ctx, datas);
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

        public static ResponseMessage<dynamic> CreateBOM(Context ctx, DispatchInfo request)
        {
            IMaterialService service = ServiceFactory.GetService<IMaterialService>(ctx);
            try
            {
                return service.CreateBOM(ctx, request);
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

        public static MaterialInfo[] TryGetOrAddCustMsterials(Context ctx, CustomerInfo customer, params MaterialInfo[] materials)
        {
            IMaterialService service = ServiceFactory.GetService<IMaterialService>(ctx);
            try
            {
                return service.TryGetOrAddCustMsterials(ctx, customer, materials);
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

        public static ResponseMessage<dynamic> MaterialAllocate(Context ctx, List<long> materiallist)
        {
            IMaterialService service = ServiceFactory.GetService<IMaterialService>(ctx);
            try
            {
                return service.MaterialAllocate(ctx, materiallist);
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
        public static void MaterialAllocateToAll(Context ctx, List<long> materiallist)
        {
            IMaterialService service = ServiceFactory.GetService<IMaterialService>(ctx);
            try
            {
                service.MaterialAllocateToAll(ctx, materiallist);
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
