using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Authentication;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.Contracts.BaseManagement;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.Common;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.App.Core.BaseManagement
{
    public class UserService: IUserService
    {
        public UserInfo GetUserInfoForUserID(Context ctx, long userid)
        {
            UserInfo userinfo = null;

            string sSql = $@"select U.FCREATEORG as OrgID,O.FNUMBER as OrgNumber,O_L.FNAME as OrgName
                            ,U.FUSERID,U.FUSERACCOUNT, U.FNAME FUSERNAME
                            ,E.FID FEMPID, E_L.FNAME FEMPNAME,E.FWECHATCODE
                            ,S.FDEPTID,D.FNUMBER as FDEPTNUMBER,D_L.FNAME as FDEPTNAME
                            from T_SEC_USER U
                            inner join T_ORG_ORGANIZATIONS O on U.FCREATEORG=O.FORGID
                            left join T_ORG_ORGANIZATIONS_L O_L on O.FORGID=O_L.FORGID and FLOCALEID=2052
                            left join t_BD_Person P ON (u.FLINKOBJECT = P.FPERSONID)
                            left join T_HR_EmpInfo E ON (P.FPERSONID = E.FPERSONID)
                            left join T_HR_EmpInfo_L E_L ON (E.FID = E_L.FID)
                            left join T_BD_STAFF S ON (P.FPERSONID = S.FPERSONID)
                            left join T_BD_DEPARTMENT D ON (S.FDEPTID = D.FDEPTID)
                            left join T_BD_DEPARTMENT_L D_L ON (D.FDEPTID= D_L.FDEPTID AND D_L.FLOCALEID = 2052)
                            left join T_BD_STAFFPOSTINFO T ON S.FSTAFFID=t.FSTAFFID
                            where U.FUSERID={userid}";
            var user = DBUtils.ExecuteDynamicObject(ctx, sSql);
            if (user.Count > 0)
            {
                userinfo = JsonConvertUtils.DeserializeObject<UserInfo>(JsonConvertUtils.SerializeObject(user[0]));
            }
            sSql = $@"select S.FSTAFFNUMBER
                    ,T.FPOSTID,PT.FNUMBER as FPOSTNUMBER,PTL.FNAME FPOSTNAME,T.FISFIRSTPOST
                    ,T.FDEPTID,DEP.FNUMBER as FDEPTNUMBER,DEPL.FNAME as FDEPTNAME
                    from T_SEC_USER U
                    inner join t_BD_Person P ON (u.FLINKOBJECT = P.FPERSONID)
                    inner join T_BD_STAFF S ON (P.FPERSONID = S.FPERSONID)
                    inner join T_BD_STAFFTEMP T ON S.FSTAFFID=T.FSTAFFID
                    left join T_ORG_POST PT ON PT.FPOSTID=T.FPOSTID
                    left join T_ORG_POST_L PTL ON PTL.FPOSTID=PT.FPOSTID
                    left join T_BD_DEPARTMENT DEP ON DEP.FDEPTID=T.FDEPTID
                    left join T_BD_DEPARTMENT_L DEPL ON DEPL.FDEPTID=T.FDEPTID
                    where U.FUSERID={userid}";
            var post = DBUtils.ExecuteDynamicObject(ctx, sSql);
            if (post.Count > 0)
            {
                List<postList> postList = JsonConvertUtils.DeserializeObject<List<postList>>(JsonConvertUtils.SerializeObject(post));
                userinfo.post = postList;
            }
            sSql = $@"select O.FNUMBER
                    from T_SEC_USER U
                    inner join t_BD_Person P ON (u.FLINKOBJECT = P.FPERSONID)
                    inner join T_BD_STAFF S ON (P.FPERSONID = S.FPERSONID)
                    left join T_BD_STAFF_L S_L ON (S.FSTAFFID = S_L.FSTAFFID AND S_L.FLOCALEID = 2052)
                    inner join T_BD_OPERATORENTRY O ON (O.FSTAFFID = S.FSTAFFID AND O.FOPERATORTYPE = 'CGY')
                    where U.FUSERID ={userid}";
            var cg = DBUtils.ExecuteDynamicObject(ctx, sSql);
            if (cg.Count > 0)
            {
                userinfo.FCGCode = cg[0]["FNUMBER"].ToString();
            }
            sSql = $@"select O.FNUMBER
                    from T_SEC_USER U
                    inner join t_BD_Person P ON (u.FLINKOBJECT = P.FPERSONID)
                    inner join T_BD_STAFF S ON (P.FPERSONID = S.FPERSONID)
                    left join T_BD_STAFF_L S_L ON (S.FSTAFFID = S_L.FSTAFFID AND S_L.FLOCALEID = 2052)
                    inner join T_BD_OPERATORENTRY O ON (O.FSTAFFID = S.FSTAFFID AND O.FOPERATORTYPE = 'WHY')
                    where U.FUSERID ={userid}";
            var wh = DBUtils.ExecuteDynamicObject(ctx, sSql);
            if (wh.Count > 0)
            {
                userinfo.FWHCode = wh[0]["FNUMBER"].ToString();
            }
            sSql = $@"select O.FNUMBER
                    from T_SEC_USER U
                    inner join t_BD_Person P ON (u.FLINKOBJECT = P.FPERSONID)
                    inner join T_BD_STAFF S ON (P.FPERSONID = S.FPERSONID)
                    left join T_BD_STAFF_L S_L ON (S.FSTAFFID = S_L.FSTAFFID AND S_L.FLOCALEID = 2052)
                    inner join T_BD_OPERATORENTRY O ON (O.FSTAFFID = S.FSTAFFID AND O.FOPERATORTYPE = 'XSY')
                    where U.FUSERID ={userid}";
            var xs = DBUtils.ExecuteDynamicObject(ctx, sSql);
            if (xs.Count > 0)
            {
                userinfo.FXSCode = xs[0]["FNUMBER"].ToString();
            }

            return userinfo;
        }
        //根据登录ID获取微信Code
        public string GetUserWxCode(Context ctx, long userid)
        {
            List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@FuserID", KDDbType.Int64, userid) };
            string sql = $@"SELECT TOP 1 c.FWECHATCODE FROM T_SEC_USER a -- 用户表
                            INNER JOIN T_BD_PERSON b ON a.FLINKOBJECT = b.FPERSONID -- 人员表
                            INNER JOIN T_HR_EMPINFO c ON b.FPERSONID=c.FPERSONID
                            where a.FUSERID=@FuserID and a.FTYPE=1 ";

            return DBUtils.ExecuteScalar<string>(ctx, sql, string.Empty, paramList: pars.ToArray());
        }
    }
}
