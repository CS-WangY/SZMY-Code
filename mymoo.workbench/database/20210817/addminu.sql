use mymooo_workbench

  declare @id1 bigint
  set @id1 = (select Id from [Menu] where AppId = 'crm' and [Path] = '/receivableManage')
  insert into [Menu] values
  (@id1,'crm', 'recivestrategyanalysis', '应收账龄分析', '', 10, 1, '', 'YinSheng',GETDATE(),'YinSheng',GETDATE(),'/receivableManage/recivestrategyanalysis/index', 'recivestrategyanalysis')
  insert into [Menu] values
	(@id1,'crm', 'reciveagingdetail', '应收账龄详情', '', 10, 1, '', 'YinSheng',GETDATE(),'YinSheng',GETDATE(),'/receivableManage/reciveagingdetail/index', 'reciveagingdetail')
   go

   declare @adminid bigint
   declare @id2 bigint
   declare @id3 bigint

   set @adminid = (select Id from Roles where Code = 'admin')
	set @id2 = (select Id from [Menu] where AppId = 'crm' and [Path] = 'recivestrategyanalysis')
	set @id3 = (select Id from [Menu] where AppId = 'crm' and [Path] = 'reciveagingdetail')

   insert into RolesMenu values (@adminid,@id2,1,'YinSheng', GETDATE());
   insert into RolesMenu values (@adminid,@id3,1,'YinSheng', GETDATE());
   go

    declare @salesmanid bigint
   declare @id2 bigint
   declare @id3 bigint

   set @salesmanid = (select Id from Roles where Code = 'salesman')
	set @id2 = (select Id from [Menu] where AppId = 'crm' and [Path] = 'recivestrategyanalysis')
	set @id3 = (select Id from [Menu] where AppId = 'crm' and [Path] = 'reciveagingdetail')

   insert into RolesMenu values (@salesmanid,@id2,1,'YinSheng', GETDATE());
   insert into RolesMenu values (@salesmanid,@id3,1,'YinSheng', GETDATE());

