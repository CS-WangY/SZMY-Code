using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.BDCustomer
{
    internal class SaveValidator : AbstractValidator
    {
        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, BOS.Context ctx)
        {
            foreach (var dataEntitie in dataEntities)
            {
                bool isExit = false;
                //检查客户编码和名称是否存在
                var sql = "select 1 result from T_BD_CUSTOMER where FFORBIDSTATUS='A' and FNumber = @FNumber and FCUSTID<>@FCUSTID ";
                using (var reader = DBUtils.ExecuteReader(ctx, sql, new List<SqlParam> { new SqlParam("@FNumber", KDDbType.String, dataEntitie.DataEntity["Number"]), new SqlParam("@FCUSTID", KDDbType.String, dataEntitie.DataEntity["Id"]) }))
                {
                    if (reader.Read())
                    {
                        isExit = Convert.ToBoolean(reader["result"]);
                    }
                }
                if (isExit)
                {
                    validateContext.AddError(dataEntitie, new ValidationErrorInfo(
                                  string.Empty,
                                  dataEntitie["Id"].ToString(),
                                  dataEntitie.DataEntityIndex,
                                  dataEntitie.RowIndex,
                                  dataEntitie["Id"].ToString(),
                                  string.Format("客户编码{0}在系统中已经存在", dataEntitie.DataEntity["Number"]),
                                  "保存",
                                  ErrorLevel.FatalError));
                }

                bool nameIsExit = false;
                if (dataEntitie.DataEntity["Number"] != null && dataEntitie.DataEntity["Number"].ToString().Contains('E'))
                {
                    sql = "select 1 result from T_BD_CUSTOMER_L a left join T_BD_CUSTOMER b on a.FCUSTID=b.FCUSTID where b.FFORBIDSTATUS='A' and a.FCUSTID<>@FCUSTID  and  a.FNAME=@FName and a.FName like 'E%'";

                    using (var reader = DBUtils.ExecuteReader(ctx, sql, new List<SqlParam> { new SqlParam("@FName", KDDbType.String, dataEntitie.DataEntity["Name"].ToString()), new SqlParam("@FCUSTID", KDDbType.String, dataEntitie.DataEntity["Id"]) }))
                    {
                        if (reader.Read())
                        {
                            nameIsExit = Convert.ToBoolean(reader["result"]);
                        }
                    }
                    if (nameIsExit)
                    {
                        validateContext.AddError(dataEntitie, new ValidationErrorInfo(
                                      string.Empty,
                                      dataEntitie["Id"].ToString(),
                                      dataEntitie.DataEntityIndex,
                                      dataEntitie.RowIndex,
                                      dataEntitie["Id"].ToString(),
                                      string.Format("客户名称{0}在系统中已经存在", dataEntitie.DataEntity["Name"].ToString()),
                                      "保存",
                                      ErrorLevel.FatalError));
                    }
                }
            }
        }

    }
}
