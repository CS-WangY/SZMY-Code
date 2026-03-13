-- 添加菜单-产品小类机器人设置
  insert into [Menu]([ParentId]
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
 select Id,'scm','smallClassRobot','产品小类机器人设置','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(),'/systemManage/smallClassRobot','smallclassrobot' from [Menu] where AppId = 'scm' and path = '/systemManage'
go
 insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
 select 1,Id,1,'YinSheng',GETDATE() from [Menu] where AppId = 'scm' and path = 'smallClassRobot'


