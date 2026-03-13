USE [mymooo_workbench]

create table AssistantUser
(
	Id					bigint	identity(1,1) primary key,
	AssistantCode			varchar(30)		not null	default '',
	UserCode			varchar(30)		not null	default '',
	CreateUser			varchar(30)		not null	default '',
	CreateDate			datetime
)

