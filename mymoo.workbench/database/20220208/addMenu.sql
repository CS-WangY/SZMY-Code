
update Menu set IsPublish = 1 where AppId = 'crm' and Title = '系统管理'
update Menu set IsPublish = 0 where AppId = 'crm' and Title in ('用户管理','角色管理','菜单管理')
go
  insert into Menu([ParentId]
      ,[AppId]
      ,[Path]
      ,[Title]
      ,[Icon]
      ,[Sort]
      ,[IsPublish]
      ,[Description]
      ,[CreateUser]
      ,[CreateDate]
      ,[PublishUser]
      ,[PublishDate]
      ,[Component]
      ,[Name]) 
	  select Id,'crm','autodiscountplan','自动折扣方案设置','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/systemManage/autodiscountplan/index','autodiscountplan' from Menu 
	  where AppId = 'crm' and Title = '系统管理'

	  go
	  insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
	  select 1,Id,1,'YinSheng',GETDATE() from Menu where AppId = 'crm' and Title = '自动折扣方案设置'