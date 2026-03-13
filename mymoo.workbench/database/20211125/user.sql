use mymooo_workbench
go

alter table MymoooUser add DirectSupervisor varchar(255)
alter table MymoooUser add Grade varchar(255)
alter table MymoooUser add Education varchar(255)
alter table MymoooUser add EntryDate datetime