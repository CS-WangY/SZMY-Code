using System;
using System.Collections.Generic;
using System.Text;

namespace com.mymooo.workbench.weixin.work.Utils
{
    /// <summary>
    /// 企业微信调用地址
    /// </summary>
    public class WeWorkPath
    {
        /// <summary>
        /// 获取access_token
        /// </summary>
        public const string GetAccessToken = "cgi-bin/gettoken?corpid={0}&corpsecret={1}";

        /// <summary>
        /// 获取访问用户身份
        /// </summary>
        public const string GetUserId = "cgi-bin/user/getuserinfo?access_token={0}&code={1}";

        /// <summary>
        /// openid转userid
        /// </summary>
        public const string convert_to_userid = "cgi-bin/user/convert_to_userid?access_token={0}";

        /// <summary>
        /// 读取成员
        /// </summary>
        public const string GetUserInfo = "cgi-bin/user/get?access_token={0}&userid={1}";

        /// <summary>
        /// 读取Ticket, 给JSSDK用
        /// </summary>
        public const string GetTicket = "cgi-bin/ticket/get?access_token={0}&type=agent_config";
        
        /// <summary>
        /// 获取部门成员
        /// </summary>
        public const string GetSimpleDepartment = "cgi-bin/user/simplelist?access_token={0}&department_id={1}&fetch_child={2}";

        /// <summary>
        /// 获取部门成员详情
        /// </summary>
        public const string GetDepartmentList = "cgi-bin/user/list?access_token={0}&department_id={1}&fetch_child={2}";

        /// <summary>
        /// 获取部门详情
        /// </summary>
        public const string GetDepartmentDetail = "cgi-bin/department/list?access_token={0}&id={1}";

        /// <summary>
        /// 提交审批信息
        /// </summary>
        public const string OaApplyevent = "cgi-bin/oa/applyevent?access_token={0}";

        /// <summary>
        /// 获取审批申请详情
        /// </summary>
        public const string GetOaApplyeventDetail = "cgi-bin/oa/getapprovaldetail?access_token={0}";

        /// <summary>
        /// 发送应用消息
        /// </summary>
        public const string SendMessage = "cgi-bin/message/send?access_token={0}";

        /// <summary>
        /// 上传临时素材
        /// </summary>
        public const string UpLoadMedia = "cgi-bin/media/upload?access_token={0}&type={1}";

        public const string DownloadMedia = "cgi-bin/media/get?access_token={0}&media_id={1}";
    }
}
