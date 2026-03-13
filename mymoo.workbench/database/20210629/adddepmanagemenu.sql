use mymooo_workbench

  declare @id1 bigint
  set @id1 = (select Id from [Menu] where AppId = 'workbench' and [Path] = '/systemManage')
  insert into [Menu] values
  (@id1,'workbench', 'depmanage', '部门管理', '', 10, 1, '', 'YinSheng',GETDATE(),'YinSheng',GETDATE(),'/systemManage/depmanage/index', 'depmanage')
  go

  declare @id1 bigint
  declare @adminId bigint
  set @id1 = (select Id from [Menu] where AppId = 'workbench' and [Path] = 'depmanage')
  set @adminId = (select Id from Roles where Code = 'admin')

  insert into RolesMenu values (@adminId, @id1, 1, 'YinSheng', GETDATE())
  go

  alter table Department add FunctionAttr nvarchar(100) not null default 'System_FunctionAttr_Other'
  go
update Department set FunctionAttr = 'System_FunctionAttr_Business' where Name like '%业务%' or Name like '%营销%'
update Department set FunctionAttr = 'System_FunctionAttr_Purchase' where Name like '%采购%' 
update Department set FunctionAttr = 'System_FunctionAttr_Manage' where Name like '%管理%' 
update Department set FunctionAttr = 'System_FunctionAttr_R&D' where Name like '%研发%' 
update Department set FunctionAttr = 'System_FunctionAttr_Finance' where Name like '%财务%' 

go

declare @id1 bigint
  set @id1 = (select Id from [Menu] where AppId = 'workbench' and [Path] = '/systemManage')
  insert into [Menu] values
  (@id1,'workbench', 'postmanage', '岗位管理', '', 10, 1, '', 'YinSheng',GETDATE(),'YinSheng',GETDATE(),'/systemManage/postmanage/index', 'postmanage')
  go

  declare @id1 bigint
  declare @adminId bigint
  set @id1 = (select Id from [Menu] where AppId = 'workbench' and [Path] = 'postmanage')
  set @adminId = (select Id from Roles where Code = 'admin')

  insert into RolesMenu values (@adminId, @id1, 1, 'YinSheng', GETDATE())
  go

  declare @id1 bigint
  set @id1 = (select Id from [Menu] where AppId = 'workbench' and [Path] = '/systemManage')
  insert into [Menu] values
  (@id1,'workbench', 'systemparameter', '系统参数', '', 10, 1, '', 'YinSheng',GETDATE(),'YinSheng',GETDATE(),'/systemManage/systemparameter/index', 'systemparameter')
  go

  declare @id1 bigint
  declare @adminId bigint
  set @id1 = (select Id from [Menu] where AppId = 'workbench' and [Path] = 'systemparameter')
  set @adminId = (select Id from Roles where Code = 'admin')

  insert into RolesMenu values (@adminId, @id1, 1, 'YinSheng', GETDATE())
  go

