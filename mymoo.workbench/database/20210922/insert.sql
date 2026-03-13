-- 添加分类报价转化率菜单
  insert into menu([ParentId]
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
  select m.Id,'report','classquoconvertrate','分类报价转化率','',10,1,'','YinSheng',GETDATE(),'YinSheng',GETDATE(), '/reportManage/insidetable/classQuoConvertRate','classquoconvertrate' 
  from menu m where AppId = 'report' and Path = 'insidetable'

  -- 管理员授权
  insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
  select 1,m.Id,1,'Yinsheng',GETDATE() from menu m where AppId = 'report' and Path = 'classquoconvertrate'


  update menu set [Component] = '/reportManage/insidetable/classQuoConvertRate' where AppId = 'report' and Path = 'classquoconvertrate'