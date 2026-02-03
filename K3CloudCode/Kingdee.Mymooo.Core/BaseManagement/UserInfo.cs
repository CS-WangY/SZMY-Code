using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.BaseManagement
{
    public class UserInfo
    {
        /// <summary>
        /// 组织ID
        /// </summary>
        public long OrgID { get; set; }
        /// <summary>
        /// 组织编码
        /// </summary>
        public string OrgNumber { get; set; }
        /// <summary>
        /// 组织名称
        /// </summary>
        public string OrgName { get; set; }
        /// <summary>
        /// 用户内码
        /// </summary>
        public long FUSERID { get; set; }
        public string FUSERACCOUNT { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string FUSERNAME { get; set; }
        /// <summary>
        /// 员工内码
        /// </summary>
        public long FEMPID { get; set; }
        /// <summary>
        /// 员工姓名
        /// </summary>
        public string FEMPNAME { get; set; }
        /// <summary>
        /// 员工微信号
        /// </summary>
        public string FWECHATCODE { get; set; }
        public List<postList> post { get; set; }
        public long FDEPTID { get; set; }
        public string FDEPTNUMBER { get; set; }
        public string FDEPTNAME { get; set; }
        public string FCGCode { get; set; }
        public string FWHCode { get; set; }
        public string FXSCode { get; set; }
    }
    public class postList
    {
        /// <summary>
        /// 员工任岗编码
        /// </summary>
        public string FSTAFFNUMBER { get; set; }
        /// <summary>
        /// 岗位ID
        /// </summary>
        public long FPOSTID { get; set; }
        /// <summary>
        /// 岗位编码
        /// </summary>
        public string FPOSTNUMBER { get; set; }
        /// <summary>
        /// 岗位名称
        /// </summary>
        public string FPOSTNAME { get; set; }
        /// <summary>
        /// 是否主岗
        /// </summary>
        public int FISFIRSTPOST { get; set; }
        /// <summary>
        /// 任岗部门ID
        /// </summary>
        public long FDEPTID { get; set; }
        /// <summary>
        /// 任岗部门编码
        /// </summary>
        public string FDEPTNUMBER { get; set; }
        /// <summary>
        /// 任岗部门名称
        /// </summary>
        public string FDEPTNAME { get; set; }
    }
}
