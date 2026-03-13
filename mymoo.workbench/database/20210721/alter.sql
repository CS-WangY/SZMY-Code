use mymooo_workbench
  delete from [UserRoles] where UserId = 0
  go
  alter table UserRoles add constraint FK_UserRoles_UserId foreign key (UserId) references MymoooUser(Id)
  go
  alter table UserPosition add constraint FK_UserPosition_UserId foreign key (UserId) references MymoooUser(Id)
  go
  alter table UserRoles drop constraint DF__UserRoles__UserC__59063A47
  go
  drop index Idx_UserRoles_UserCode on UserRoles
  go
  alter table UserRoles drop column UserCode 
  go

  alter table UserPosition drop constraint DF__UserPosit__UserC__5F141958
  go
  drop index Idx_UserPosition_UserCode on UserPosition
  go
  alter table UserPosition drop column UserCode 
  go

  alter table AssistantUser drop constraint DF__Assistant__UserC__65C116E7
  go
 alter table AssistantUser drop column UserCode 
 go
 alter table AssistantUser drop constraint DF__Assistant__Assis__64CCF2AE
  go
 alter table AssistantUser drop column AssistantCode 