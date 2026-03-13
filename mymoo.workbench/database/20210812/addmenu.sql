use mymooo_workbench

  insert into [Menu] values
  (0,'crm', '/receivableManage', '应收管理', 'email', 10, 1, '', 'YinSheng',GETDATE(),'YinSheng',GETDATE(),'/index', '')
  go
  declare @id1 bigint
  set @id1 = (select Id from [Menu] where AppId = 'crm' and [Path] = '/receivableManage')
  insert into [Menu] values
  (@id1,'crm', 'recivebilllist', '收款单列表', '', 10, 1, '', 'YinSheng',GETDATE(),'YinSheng',GETDATE(),'/receivableManage/recivebilllist/index', 'recivebilllist')
   go

   declare @adminid bigint
   declare @id1 bigint
   declare @id2 bigint

   set @adminid = (select Id from Roles where Code = 'admin')
   set @id1 = (select Id from [Menu] where AppId = 'crm' and [Path] = '/receivableManage')
	set @id2 = (select Id from [Menu] where AppId = 'crm' and [Path] = 'recivebilllist')

   insert into RolesMenu values (@adminid,@id1,1,'YinSheng', GETDATE());
   insert into RolesMenu values (@adminid,@id2,1,'YinSheng', GETDATE());
   go

    declare @salesmanid bigint
   declare @id1 bigint
   declare @id2 bigint

   set @salesmanid = (select Id from Roles where Code = 'salesman')
   set @id1 = (select Id from [Menu] where AppId = 'crm' and [Path] = '/receivableManage')
	set @id2 = (select Id from [Menu] where AppId = 'crm' and [Path] = 'recivebilllist')

   insert into RolesMenu values (@salesmanid,@id1,1,'YinSheng', GETDATE());
   insert into RolesMenu values (@salesmanid,@id2,1,'YinSheng', GETDATE());