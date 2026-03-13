
 update [Menu] set [IsPublish] = 0 where [AppId] = 'crm' and [Path] = 'myrecord' 
  update [Menu] set [Title] = '备案列表' where [AppId] = 'crm' and [Path] = 'recordlist'  
  update [Menu] set [IsPublish] = 0 where [AppId] = 'crm' and [Path] = 'mycreditapplyrecord'
  go
    declare @id bigint
  declare @id1 bigint
  declare @id2 bigint
  declare @id3 bigint
  declare @id4 bigint
  declare @salesId bigint

  set @id = (select Id from [Menu] where AppId = 'crm' and [Path] = 'custlist')
  set @id1 = (select Id from [Menu] where AppId = 'crm' and [Path] = 'creditlist')
  set @id2 = (select Id from [Menu] where AppId = 'crm' and [Path] = 'recordlist')
  set @id3 = (select Id from [Menu] where AppId = 'crm' and [Path] = 'creditapplyrecordlist')
  set @id4 = (select Id from [Menu] where AppId = 'crm' and [Path] = 'custfirstordertime')
  set @salesId = (select Id from Roles where Code = 'salesman')

  insert into RolesMenu values (@salesId, @id, 1, 'YinSheng', GETDATE())
  insert into RolesMenu values (@salesId, @id1, 1, 'YinSheng', GETDATE())
  insert into RolesMenu values (@salesId, @id2, 1, 'YinSheng', GETDATE())
  insert into RolesMenu values (@salesId, @id3, 1, 'YinSheng', GETDATE())
  insert into RolesMenu values (@salesId, @id4, 1, 'YinSheng', GETDATE())


