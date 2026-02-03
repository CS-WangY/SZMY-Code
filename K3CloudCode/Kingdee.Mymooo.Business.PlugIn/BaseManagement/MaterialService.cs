using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Enums;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.Operation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.ServiceHelper.ManagementCenter;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.FormService;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using Kingdee.Mymooo.ServiceHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kingdee.Mymooo.Business.PlugIn.BaseManagement
{
    public class MaterialService
    {
        public ResponseMessage<dynamic> DeleteRepeatMaterial(Context ctx)
        {
            ResponseMessage<dynamic> result = new ResponseMessage<dynamic>();

            var sql = @"select m.FMATERIALID,m.FNUMBER,m.FDOCUMENTSTATUS,m.FFORBIDSTATUS 
from T_BD_MATERIAL m
	inner join (select FNUMBER,min(FMATERIALID) FMATERIALID from T_BD_MATERIAL where FUSEORGID = 1 group by FNUMBER having count(1) > 1) d on m.FNUMBER = d.FNUMBER and m.FMATERIALID > d.FMATERIALID
where FUSEORGID = 1
order by m.FMATERIALID";

            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            FormMetadata meta = MetaDataServiceHelper.Load(ctx, "BD_MATERIAL") as FormMetadata;
            var ids = datas.Select(p => p["FMATERIALID"]).ToArray();
            MymoooBusinessDataServiceHelper.UnAudit(ctx, meta.BusinessInfo, ids);
            MymoooBusinessDataServiceHelper.DeleteBill(ctx, meta.BusinessInfo, ids);

            return result;
        }

        public ResponseMessage<dynamic> SaveMaterial(Context ctx)
        {
            ResponseMessage<dynamic> result = new ResponseMessage<dynamic>();

            var sql = @"select top 100 FMATERIALID from T_BD_MATERIAL where FDOCUMENTSTATUS = 'Z'";

            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            while (datas.Count > 0)
            {
                FormMetadata meta = MetaDataServiceHelper.Load(ctx, "BD_MATERIAL") as FormMetadata;
                var ids = datas.Select(p => p["FMATERIALID"]).ToArray();
                MymoooBusinessDataServiceHelper.SubmitBill(ctx, meta.BusinessInfo, ids);
                MymoooBusinessDataServiceHelper.Audit(ctx, meta.BusinessInfo, ids);

                datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            }
            return result;
        }

        /// <summary>
        /// 获取大类列表
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public ResponseMessage<dynamic> GetItemGrpList(Context ctx)
        {
            ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();

            List<ItemGrpEntity> entityList = new List<ItemGrpEntity>();
            var sql = $@"/*dialect*/select distinct g.FPARENTID ITEM_GRP,gll.FNAME GROUP_DESC from T_BD_MATERIALGROUP g
                        left join T_BD_MATERIALGROUP_L gll on g.FPARENTID = gll.FID and gll.FLOCALEID = 2052
                        where isnull(g.FPARENTID,0)>0
                        order by gll.FNAME asc";

            var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            foreach (var data in datas)
            {
                entityList.Add(new ItemGrpEntity
                {
                    ItemGrp = Convert.ToString(data["ITEM_GRP"]),
                    GrpDesc = Convert.ToString(data["GROUP_DESC"]),
                });
            }
            response.Message = "获取成功";
            response.Data = entityList;
            response.Code = ResponseCode.Success;
            return response;
        }
    }
}
