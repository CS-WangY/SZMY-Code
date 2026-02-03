using Kingdee.BOS;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Business.PlugIn.BaseManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.RabbitMQ;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.Mymooo.Core.ProductionManagement;

namespace Kingdee.Mymooo.Business.PlugIn.ProductionManagement
{
    public class DepartmentBusiness : IMessageExecute
    {
        public ResponseMessage<dynamic> Execute(Context ctx, string message)
        {
            DepartmentRequest request = JsonConvert.DeserializeObject<DepartmentRequest>(message);

            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();

            //var orgCreate = FormMetadataUtils.GetIdForNumber(ctx, new OrganizationsInfo(request.FCreateOrgId, ""));
            //if (orgCreate.Id == 0 || !orgCreate.DocumentStatus.EqualsIgnoreCase("C"))
            //{
            //    response.Code = ResponseCode.NoExistsData;
            //    response.Message = "对应的创建组织不存在未审核";
            //    return response;
            //}

            //var orgUse = FormMetadataUtils.GetIdForNumber(ctx, new OrganizationsInfo(request.FUseOrgId, ""));
            //if (orgUse.Id == 0 || !orgUse.DocumentStatus.EqualsIgnoreCase("C"))
            //{
            //    response.Code = ResponseCode.NoExistsData;
            //    response.Message = "对应的使用组织不存在未审核";
            //    return response;
            //}

            CetateDepartment(ctx, request, response);
            return response;
        }

        /// <summary>
        /// 创建部门
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void CetateDepartment(Context ctx, DepartmentRequest request, ResponseMessage<dynamic> response)
        {
            var billView = FormMetadataUtils.CreateBillView(ctx, "BD_Department");
            billView.Model.SetItemValueByNumber("FCreateOrgId", request.FCreateOrgId,0);
            billView.Model.SetItemValueByNumber("FUseOrgId", request.FUseOrgId,0);
            billView.Model.SetValue("FNumber", request.workshopCode);
            billView.Model.SetValue("FName", request.workshopName);
            billView.Model.SetValue("FFullName", request.workshopName);
            billView.Model.SetItemValueByNumber("FDeptProperty", request.FDeptProperty,0);
            
            var oper = MymoooBusinessDataServiceHelper.SaveAndAuditBill(ctx, billView.BusinessInfo, billView.Model.DataObject);
            if (!oper.IsSuccess)
            {
                if (oper.ValidationErrors.Count > 0)
                {
                    throw new Exception(string.Join(";", oper.ValidationErrors.Select(p => p.Message)));
                }
                else
                {
                    throw new Exception(string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message)));
                }
            }

            response.Code = ResponseCode.Success;
            response.Message = "成功";
        }
    }
}
