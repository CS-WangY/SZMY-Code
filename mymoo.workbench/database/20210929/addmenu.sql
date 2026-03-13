
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
select Id,'report','overdueNoWarning','逾期未预警','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/detail/overduenowarning','overduenowarning' from Menu where AppId = 'report' and Path = '/detail'

go

insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
select 1,Id,1,'YinSheng',GETDATE() from Menu where AppId = 'report' and Path = 'overdueNoWarning' 