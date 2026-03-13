namespace com.mymooo.workbench.core.SystemManage
{
    /// <summary>
    /// 用户信息
    /// </summary>
    public class AuthorityUser
    {
        /// <summary>
        /// 用户编码
        /// </summary>
        public string UserCode { get; set; } = null!;

        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; } = null!;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            AuthorityUser other = (AuthorityUser)obj;
            return UserCode == other.UserCode && UserId == other.UserId && UserName == other.UserName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            
            return HashCode.Combine(UserCode, UserId, UserName);
        }
    }

    /// <summary>
    /// 
    /// </summary>

    public class AuthorityUserAndDept : AuthorityUser
    {
        /// <summary>
        /// 部门Id 
        /// </summary>
        public long DepartmentId { get; set; }

        /// <summary>
        /// 是否部门负责人
        /// </summary>
        public bool IsLeaderInDepartment { get; set; }
    }

}
