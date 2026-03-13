using com.mymooo.workbench.core.Account;
using mymooo.core.Attributes;
using mymooo.weixinWork.SDK.AddressBook.Model;

namespace com.mymooo.workbench.business.WeixinWork.AddressBook
{
    /// <summary>
    /// 通讯录服务
    /// </summary>
    [AutoInject(InJectType.Scope)]
    public class AddressBookService(WorkbenchContext workbenchContext)
    {
        private readonly WorkbenchContext _workbenchContext = workbenchContext;
        public void ReloadCache()
        {
            _workbenchContext.RedisCache.HashDelete<ParentUserInfo>();
            var users = _workbenchContext.SqlSugar.Queryable<MymoooUser>().Where(p => p.AppId == "weixinwork" && p.IsDelete == false).ToArray();
            foreach (var user in users)
            {
                _workbenchContext.RedisCache.HashSet(user);
            }

            var parentUsers = _workbenchContext.SqlSugar.Ado.SqlQuery<ParentUserInfo>(@"with AllDepartment as
                (
                        select u.Id,u.UserId,u.Name, du.DepartmentId,d.ParentId,d.Name DepartmentName,1 RowIndex
                        from MymoooUser u
                                inner join DepartmentUser du on u.Id = du.UserId
                                inner join Department d on du.DepartmentId = d.Id and d.DepartmentId=u.MainDepartmentId
                        where u.AppId  = 'weixinwork' and u.IsDelete = 0
                        union all
                        select d.Id,d.UserId,d.Name,ad.Id,ad.ParentId,ad.Name,d.RowIndex + 1
                        from AllDepartment d
                                inner join Department ad on d.ParentId = ad.DepartmentId and ad.AppId = 'weixinwork'
                )
                select d.Id UserId,d.UserId UserCode,d.Name UserName,mu.Id ParentUserId,mu.UserId as ParentUserCode ,mu.Name ParentUserName,d.DepartmentId,d.DepartmentName,d.ParentId ParentDepartmentId,d.RowIndex
                from AllDepartment d
                        inner join DepartmentUser du on d.DepartmentId = du.DepartmentId and du.IsLeaderInDepartment = 1
                        inner join MymoooUser mu on du.UserId = mu.Id and mu.IsDelete = 0
                order by d.RowIndex");
            foreach (var user in parentUsers)
            {
                _workbenchContext.RedisCache.HashSet(user);
            }
        }
    }
}
