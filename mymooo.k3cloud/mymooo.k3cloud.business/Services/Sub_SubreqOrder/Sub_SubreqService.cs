using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch.Core.Search;
using mymooo.core.Attributes;
using mymooo.core.Model.Gateway;
using mymooo.core;
using mymooo.k3cloud.core.Account;
using mymooo.k3cloud.core.ProductionModel;
using mymooo.product.selection.SelectionModel;
using mymooo.product.selection;
using Mymooo.Threed.WebService.Core.ThreedModel;
using Mymooo.Threed.WebService.SDK;
using mymooo.k3cloud.core.SubReqModel;
using mymooo.core.Account;

namespace mymooo.k3cloud.business.Services.Sub_SubreqOrder
{
    /// <summary>
    /// 委外订单服务
    /// </summary>
    [AutoInject(InJectType.Scope)]
    public class Sub_SubreqService(KingdeeContent kingdeeContent, ThreedServiceClient<KingdeeContent, User> threedService, ProductCache<KingdeeContent, User> productCache)
    {
        public async Task<ResponseMessage<SubReqOrderRequests>> SendMakeDispatch(SubReqOrderRequests request)
        {
            ResponseMessage<SubReqOrderRequests> response = new();
            var threedUrl = kingdeeContent.GatewayRedisCache.HashGet(new SystemEnvironmentConfig() { SystemEnvCode = "threed" }, "system");
            if (threedUrl == null)
            {
                response.Code = ResponseCode.NotFound;
                response.ErrorMessage = "3D服务不存在";
                return response;
            }
            response.Data = request;
            SourceConfig sourceConfig = new(new SourceFilter() { Includes = new string[] { "productId", "pdfUrl", "isRelease", "threedParamSeq", "brandId", "brandName", "brandEnName", "threedModelFolder" } });
            if (request.Details != null)
            {
                foreach (var detail in request.Details)
                {
                    if (detail.BomChildren != null)
                    {
                        foreach (var child in detail.BomChildren)
                        {
                            ProductParameterValueRequest parameterValueRequest = new() { Number = child.FMaterialNumber };
                            var product = await productCache.GetProductParameterValue(kingdeeContent, parameterValueRequest, sourceConfig);
                            if (product != null)
                            {
                                child.ProductId = product.ProductId;
                                child.PlaneUrl = product.PdfUrl?.Replace("/pdfview/pdfView.html?file=..", "");
                                child.ParameterValues = product.ParameterValues;
                                var threedResponse = await threedService.GenerateThreedFile(kingdeeContent, product, ThreedFileType.STP);
                                if (threedResponse.IsSuccess && threedResponse.Data != null)
                                {
                                    child.ThreeUrl = threedUrl.Url + "Threed/DownLoad3dFile?file=" + threedResponse.Data.MeshFile;
                                    child.ThreeVer = threedResponse.Data.DateTime.ToString("yyyy-MM-dd HH:mm:ss");
                                }
                            }
                        }
                    }
                    
                }
            }


            return response;
        }
    }
}
