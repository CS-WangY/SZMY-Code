
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
 select Id,'workbench','','SCM','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'','scm' from Menu where AppId = 'workbench' and Title = '其他系统管理'
 go
 insert into RolesMenu(RoleId
      ,[MenuId]
      ,[IsRight]
      ,[CreateUser]
      ,[CreateDate])
select 1,Id,1,'YinSheng',GETDATE() from Menu where AppId = 'workbench' and Name = 'scm'

