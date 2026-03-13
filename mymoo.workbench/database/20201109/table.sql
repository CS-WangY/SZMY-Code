if not exists(select 1 from sys.tables where name = 'UserToken')
create table UserToken
(
	Token				varchar(36)		not null	default '',
	UserCode			varchar(30)		not null	default '',
	LoginDate			datetime		null,
	AppId				varchar(30)		not null	default '',
	Validity			int				not null	default 10,
	LogonModel			varchar(30)		not null	default '',
	FailureDate			datetime		null,
	UserAgent			varchar(500)	not null	default '',
	[Ip]				varchar(50)		not null	default ''
)
go

DECLARE @CURRENTUSER SYSNAME
SELECT @CURRENTUSER = USER_NAME()
EXECUTE SP_ADDEXTENDEDPROPERTY 'MS_Description', 
   N'用户登录token表',
   'user', @CURRENTUSER, 'table', 'UserToken'
go

DECLARE @CURRENTUSER SYSNAME
SELECT @CURRENTUSER = USER_NAME()
EXECUTE SP_ADDEXTENDEDPROPERTY 'MS_Description', 
   N'UserToken 唯一,每次登录记录一个有效的token',
   'user', @CURRENTUSER, 'table', 'UserToken', 'column', 'Token'
go


alter table UserToken Add constraint PK_UserToken primary key (Token)
go

create index Idx_UserToken_UserCode on UserToken(UserCode)
go

create table ThirdpartyApplicationConfig
(
	AppId				varchar(30)		not null	default '',
	AppName				nvarchar(30)	not null	default '',
	Token				nvarchar(500)	not null	default '',
	Nonce				nvarchar(500)	not null	default '',
	[Url]				nvarchar(500)	not null	default '',
	EncodingAESKey		varchar(500)	not null	default '',
	SignLoginUrl		nvarchar(100)	not null	default '',
	Validity			int				not null	default 1000,
	CreateUser			varchar(30)		not null	default '',
	CreateDate			datetime		null
)
go

alter table ThirdpartyApplicationConfig Add constraint PK_ThirdpartyApplicationConfig primary key (AppId)
go

insert into ThirdpartyApplicationConfig(AppId,AppName,Token,[Url],CreateUser,CreateDate)
values('weixinwork',N'企业微信','wwcee0ce9fd7961e97','https://qyapi.weixin.qq.com/','MoYiFeng',getdate())
go
insert into ThirdpartyApplicationConfig(AppId,AppName,Token,Nonce,[Url],CreateUser,CreateDate)
values('QiChaCha',N'企查查','0c12e92dd5ce4a62b3daacb9a7350094','F11570145DCE5F097F80F70E9E9757C7','http://api.qichacha.com/','MoYiFeng',getdate())
go
insert into ThirdpartyApplicationConfig(AppId,AppName,Token,Nonce,EncodingAESKey,Url,CreateUser,CreateDate)
values('crm',N'客户关系管理','b80b9c1148434c8fb975185238a7965a','b80b9c1148434c8fb975185238a7965b','b80b9c1148434c8fb975185238a7965a','http://testcrm.mymooo.com/','MoYiFeng',getdate())
go
insert into ThirdpartyApplicationConfig(AppId,AppName,Token,Nonce,Url,CreateUser,CreateDate)
values('platformAdmin',N'蚂蚁平台管理(旧)','b80b9c1148434c8fb975185238a7965a','b80b9c1148434c8fb975185238a7965c','http://testadmin.fastemall.com/','MoYiFeng',getdate())
go
insert into ThirdpartyApplicationConfig(AppId,AppName,Token,Nonce,Url,CreateUser,CreateDate)
values('mymoooerp',N'蚂蚁工场ERP系统','c26d78b3d9785249caac9a6d43c76bf9','b80b9c1148434c8fb975185238a7965c','http://192.168.11.30:15555/api/','MoYiFeng',getdate())
go
insert into ThirdpartyApplicationConfig(AppId,AppName,Token,Nonce,Url,CreateUser,CreateDate)
values('credit',N'信用管理','c26d78b3d9785249caac9a6d43c76bf9','b80b9c1148434c8fb975185238a7965c','http://testcredit.mymooo.com/','MoYiFeng',getdate())
go
insert into ThirdpartyApplicationConfig(AppId,AppName,Url,CreateUser,CreateDate)
values('workbench',N'蚂蚁工场统一工作平台','http://devwork.mymooo.com:1202/','MoYiFeng',getdate())
go

create table ThirdpartyApplicationDetail
(
	Id									bigint	identity(1,1) primary key,
	AppId								varchar(30)		not null	default '',
	DetailCode							varchar(30)		not null	default '',
	DetailName							nvarchar(30)	not null	default '',
	Agentid								varchar(100)	not null	default '',
	AppSecret							varchar(500)	not null	default '',
	RedirectUri							varchar(500)	not null	default '',
	Token								varchar(500)	not null	default '',
	EncodingAESKey						varchar(500)	not null	default '',
	AccessToken							varchar(500)	not null	default '',
	ExpiresTime							datetime		null,
	MessageExecute						varchar(500)	not null	default '',
	CreateUser							varchar(30)		not null	default '',
	CreateDate							datetime		null
)
go

alter table ThirdpartyApplicationDetail add constraint FK_ThirdpartyApplicationDetail_AppId foreign key (AppId) references ThirdpartyApplicationConfig(AppId)
go

insert into ThirdpartyApplicationDetail(AppId,DetailCode,DetailName,Agentid,AppSecret,RedirectUri,Token,EncodingAESKey,MessageExecute,CreateUser,CreateDate)
values('weixinwork','AddressBook',N'通讯录应用','','Dc9hFRmEpDW79cV5Zht-l84naqhgU6qLpPofnJaXgZk','http://work.mymooo.com/WeWork/AddressBook','OO1C','G7OuLxJCYGzYoJpDxCpV6rhOlq9kddxBlSsh4Ta1dsg','com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute.AddressBookMessageExecute,com.mymooo.workbench.weixin.work','MoYiFeng',getdate())
go

insert into ThirdpartyApplicationDetail(AppId,DetailCode,DetailName,Agentid,AppSecret,RedirectUri,Token,EncodingAESKey,MessageExecute,CreateUser,CreateDate)
values('weixinwork','Application',N'工作平台应用','1000010','G4Wms4wkEEUCmqWwue9PzUf5kuBX_g3rorF3VoYhus4','http://devwork.mymooo.com:1202/home/WeiXinWorklogin','T1tDKibBHxi94M8zu9S1eziQeCOUO','G4TRMQdyEhamf3gQP1kziGQEkOWTW6y5Gte93opQLIW','com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute.ApplicationMessageExecute,com.mymooo.workbench.weixin.work','MoYiFeng',getdate())
go

insert into ThirdpartyApplicationDetail(AppId,DetailCode,DetailName,Agentid,AppSecret,RedirectUri,Token,EncodingAESKey,MessageExecute,CreateUser,CreateDate)
values('weixinwork','Approval',N'审批应用','3010040','A0lbG-dDcanA5UQxL_JtiVHO97Nj0h-ZBa1bA8BHw-4','http://work.mymooo.com/WeWork/audit','OO1C','G7OuLxJCYGzYoJpDxCpV6rhOlq9kddxBlSsh4Ta1dsg','com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute.ApprovalMessageExecute,com.mymooo.workbench.weixin.work','MoYiFeng',getdate())
go

create table Roles
(
	Id					bigint	identity(1,1) primary key,
	Code				varchar(30)		not null	default '',
	[Name]				nvarchar(30)	not null	default '',
	[Description]		nvarchar(500)	not null	default '',
	IsForbidden			bit				not null	default 0,
	CreateUser			varchar(30)		not null	default '',
	CreateDate			datetime,
	ForbiddenUser		varchar(30)		not null	default '',
	ForbiddenDate		datetime
)
go

create table Menu
(
	Id					bigint	identity(1,1) primary key,
	ParentId			bigint			not null	default 0,
	AppId				varchar(30)		not null	default '',
	[Path]				varchar(30)		not null	default '',
	Title				nvarchar(30)	not null	default '',
	Icon				varchar(30)		not null	default '',
	Sort				int				not null	default 0,
	IsPublish			bit				not null	default 0,
	[Description]		nvarchar(50)	not null	default '',
	CreateUser			varchar(30)		not null	default '',
	CreateDate			datetime,
	PublishUser			varchar(30)		not null	default '',
	PublishDate			datetime
)
go

create index Idx_Menu_ParentId on Menu(ParentId)
go
alter table Menu add constraint FK_Menu_AppId foreign key (AppId) references ThirdpartyApplicationConfig(AppId)
go


insert into Menu(ParentId,AppId,[Path],Title,Icon,Sort,IsPublish,CreateUser,CreateDate,PublishUser,PublishDate)
values(0,'crm','',N'备案管理','nested',10,1,'MoYiFeng',getdate(),'MoYiFeng',getdate())
declare @Id bigint = @@identity
insert into Menu(ParentId,AppId,[Path],Title,Icon,Sort,IsPublish,CreateUser,CreateDate,PublishUser,PublishDate)
values(@Id,'crm','#/nested/createrecord',N'新建备案','',10,1,'MoYiFeng',getdate(),'MoYiFeng',getdate())
insert into Menu(ParentId,AppId,[Path],Title,Icon,Sort,IsPublish,CreateUser,CreateDate,PublishUser,PublishDate)
values(@Id,'crm','#/nested/examine',N'审核备案','',20,1,'MoYiFeng',getdate(),'MoYiFeng',getdate())
insert into Menu(ParentId,AppId,[Path],Title,Icon,Sort,IsPublish,CreateUser,CreateDate,PublishUser,PublishDate)
values(@Id,'crm','#/nested/recordlist',N'备案总表','',30,1,'MoYiFeng',getdate(),'MoYiFeng',getdate())
insert into Menu(ParentId,AppId,[Path],Title,Icon,Sort,IsPublish,CreateUser,CreateDate,PublishUser,PublishDate)
values(@Id,'crm','#/nested/myrecord',N'我的备案','',30,1,'MoYiFeng',getdate(),'MoYiFeng',getdate())
go
insert into Menu(ParentId,AppId,[Path],Title,Icon,Sort,IsPublish,CreateUser,CreateDate,PublishUser,PublishDate)
values(0,'workbench','',N'系统管理','nested',10,1,'MoYiFeng',getdate(),'MoYiFeng',getdate())
declare @Id bigint = @@identity
insert into Menu(ParentId,AppId,[Path],Title,Icon,Sort,IsPublish,CreateUser,CreateDate,PublishUser,PublishDate)
values(@Id,'workbench','/SystemManage/Menu',N'菜单管理','',10,1,'MoYiFeng',getdate(),'MoYiFeng',getdate())
go

create table UserRoles
(
	Id					bigint	identity(1,1) primary key,
	UserCode			varchar(30)		not null	default '',
	RoleId				bigint			not null	default 0,
	CreateUser			varchar(30)		not null	default '',
	CreateDate			datetime
)
go

create index Idx_UserRoles_UserCode on UserRoles(UserCode)
go
alter table UserRoles add constraint FK_UserRoles_RoleId foreign key (RoleId) references Roles(Id)
go

create table RolesMenu
(
	Id					bigint	identity(1,1) primary key,
	RoleId				bigint			not null	default 0,
	MenuId				bigint			not null	default 0,
	IsRight				bit				not null	default 0,
	CreateUser			varchar(30)		not null	default '',
	CreateDate			datetime
)

go

alter table RolesMenu add constraint FK_RolesMenu_RoleId foreign key (RoleId) references Roles(Id)
go
alter table RolesMenu add constraint FK_RolesMenu_MenuId foreign key (MenuId) references Menu(Id)
go

insert into Roles(Code,[Name],CreateUser,CreateDate)
values('admin',N'管理员','MoYiFeng',GETDATE())
insert into Roles(Code,[Name],CreateUser,CreateDate)
values('salesman',N'销售员','MoYiFeng',GETDATE())
insert into Roles(Code,[Name],CreateUser,CreateDate)
values('keeponAttache',N'备案专员','MoYiFeng',GETDATE())
go

insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
select 1,Id,1,'MoYiFeng',GETDATE()
from Menu
go

insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
select 2,Id,1,'MoYiFeng',GETDATE()
from Menu
where Id in (1,2)
go

insert into RolesMenu(RoleId,MenuId,IsRight,CreateUser,CreateDate)
select 3,Id,1,'MoYiFeng',GETDATE()
from Menu
where Id in (1,3)
go

insert into UserRoles(UserCode,RoleId,CreateUser,CreateDate)
values('MoYiFeng',2,'MoYiFeng',GETDATE())
insert into UserRoles(UserCode,RoleId,CreateUser,CreateDate)
values('ZhongYanDan',3,'MoYiFeng',GETDATE())

go

create table MenuItem
(
	Id					bigint	identity(1,1) primary key,
	MenuId				bigint			not null	default 0,
	[Path]				varchar(30)		not null	default '',
	Title				nvarchar(30)	not null	default '',
	[Description]		nvarchar(50)	not null	default '',
	CreateUser			varchar(30)		not null	default '',
	CreateDate			datetime
)
go
alter table MenuItem add constraint FK_MenuItem_MenuId foreign key (MenuId) references Menu(Id)
go
alter table MenuItem add ControlPrivilege bit not null default 0
go

create index Idx_MenuItem_Path on MenuItem([Path])
go

insert into MenuItem(MenuId,[Path],Title,[Description],CreateUser,CreateDate)
values(2,'/Company/FuzzyQuery',N'模糊查询企业客户','','MoYiFeng',GETDATE())
insert into MenuItem(MenuId,[Path],Title,[Description],CreateUser,CreateDate)
values(2,'/CompanyKeepon/LoadKeepon',N'加载备案','','MoYiFeng',GETDATE())
insert into MenuItem(MenuId,[Path],Title,[Description],CreateUser,CreateDate)
values(2,'/CompanyKeepon/GetKeeponForId',N'获取单个备案信息','','MoYiFeng',GETDATE())
insert into MenuItem(MenuId,[Path],Title,[Description],CreateUser,CreateDate)
values(2,'/CompanyKeepon/SubmitKeepon',N'提交备案','','MoYiFeng',GETDATE())
insert into MenuItem(MenuId,[Path],Title,[Description],CreateUser,CreateDate)
values(2,'/CompanyKeepon/Keepon',N'备案','','MoYiFeng',GETDATE())
insert into MenuItem(MenuId,[Path],Title,[Description],CreateUser,CreateDate)
values(3,'/CompanyKeepon/LoadAuditKeepon',N'审批备案列表','','MoYiFeng',GETDATE())
insert into MenuItem(MenuId,[Path],Title,[Description],CreateUser,CreateDate)
values(3,'/CompanyKeepon/ApproveKeepon',N'备案审批通过','','MoYiFeng',GETDATE())
insert into MenuItem(MenuId,[Path],Title,[Description],CreateUser,CreateDate)
values(3,'/CompanyKeepon/RejectKeepon',N'驳回备案','','MoYiFeng',GETDATE())
insert into MenuItem(MenuId,[Path],Title,[Description],CreateUser,CreateDate)
values(3,'/CompanyKeepon/AllRejectKeepon',N'驳回全部备案','','MoYiFeng',GETDATE())
go



create table RolesMenuItem
(
	Id					bigint	identity(1,1) primary key,
	RoleId				bigint			not null	default 0,
	MenuItemId			bigint			not null	default 0,
	IsRight				bit				not null	default 0,
	CreateUser			varchar(30)		not null	default '',
	CreateDate			datetime
)

go

alter table RolesMenuItem add constraint FK_RolesMenuItem_RoleId foreign key (RoleId) references Roles(Id)
go
alter table RolesMenuItem add constraint FK_RolesMenuItem_MenuItemId foreign key (MenuItemId) references MenuItem(Id)
go


insert into RolesMenuItem(RoleId,MenuItemId,IsRight,CreateUser,CreateDate)
select 1,Id,1,'MoYiFeng',GETDATE()
from MenuItem
go

insert into RolesMenuItem(RoleId,MenuItemId,IsRight,CreateUser,CreateDate)
select 2,Id,1,'MoYiFeng',GETDATE()
from MenuItem
where Id <=5
go

insert into RolesMenuItem(RoleId,MenuItemId,IsRight,CreateUser,CreateDate)
select 3,Id,1,'MoYiFeng',GETDATE()
from MenuItem
where Id  > 5
go


create table WeiXinMessage
(
	Id					bigint	identity(1,1) primary key,
	ApplicationDetailId	bigint			not null	default 0,
	[Message]			ntext			not null	default '',
	IsComplete			bit				not null	default 0,
	CreateDate			datetime,
	CompleteDate		datetime,
	Result				nvarchar(500)	not null	default ''
)

go

alter table WeiXinMessage add constraint FK_WeiXinMessage_ApplicationDetailId foreign key (ApplicationDetailId) references ThirdpartyApplicationDetail(Id)
go

create table ApprovalMessage
(
	Id					bigint	identity(1,1) primary key,
	MessageId			bigint			not null	default 0,
	TemplateId			varchar(100)	not null	default '',
	SpNo				varchar(30)		not null	default '',
	SpName				nvarchar(30)	not null	default '',
	SpStatus			varchar(30)		not null	default '',
	Keyword1			varchar(500)	null		default '',
	Keyword2			varchar(500)	null		default '',
	Keyword3			varchar(500)	null		default '',
	ApplyUser			varchar(30)		not null	default '',
	MessageDetail		ntext,
	ApplyTime			datetime,
	ApprovalUser		varchar(30)		not null	default '',
	ApprovalTime		datetime,
	CreateDate			datetime
)
go

alter table ApprovalMessage add constraint FK_ApprovalMessage_MessageId foreign key (MessageId) references WeiXinMessage(Id)
go

create table ApprovalTemplate
(
	TemplateId			varchar(100)	not null	default '' primary key,
	AppId				varchar(30)		not null	default '',
	TemplateName		nvarchar(100)	not null	default '',
	MessageExecute		varchar(500)	not null	default '',
	CallbackUrl			varchar(500)	not null	default '',
	CreateUser			varchar(30)		not null	default '',
	CreateDate			datetime
)
go
alter table ApprovalTemplate add constraint FK_ApprovalTemplate_AppId foreign key (AppId) references ThirdpartyApplicationConfig(AppId)
go


insert into ApprovalTemplate(TemplateId,AppId,TemplateName,MessageExecute,CallbackUrl,CreateUser,CreateDate)
values ('ZLyTiiaRP2YextgSgS6UZ8n6uqmEFLbKPf5sXd','mymoooerp',N'发货审批模板','com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute.Approver.ErpDnOrderAuditCallback,com.mymooo.workbench.weixin.work','dnOrder/UpdateDnSubmitState','MoYiFeng',GETDATE())
go

insert into ApprovalTemplate(TemplateId,AppId,TemplateName,MessageExecute,CallbackUrl,CreateUser,CreateDate)
values ('3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5','credit',N'超信用额度订单审批模板','com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute.Approver.SurpassCreditOrderAuditCallBack,com.mymooo.workbench.weixin.work','CreditApplyevent/SurpassCreditOrderAudit','MoYiFeng',GETDATE())
go

insert into ApprovalTemplate(TemplateId,AppId,TemplateName,MessageExecute,CallbackUrl,CreateUser,CreateDate)
values ('3TkaHCQ5mvgfS9WWqdPqRRxrk3KekYSzKxTgFYYs','credit',N'申请客户额度审批模板','com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute.Approver.CreditApplyeventAuditCallBack,com.mymooo.workbench.weixin.work','CreditApplyevent/Audit','MoYiFeng',GETDATE())
go



create table ApprovalTemplateField
(
	Id					bigint	identity(1,1) primary key,
	TemplateId			varchar(100)	not null	default '',
	FieldNumber			varchar(100)	not null	default '',
	FieldName			nvarchar(100)	not null	default '',
	FieldId				varchar(100)	not null	default '',
	FieldType			varchar(500)	not null	default '',
	KeywordSeq			int				not null	default 0,
	CreateUser			varchar(30)		not null	default '',
	CreateDate			datetime
)
go

alter table ApprovalTemplateField add constraint FK_ApprovalTemplateField_TemplateId foreign key (TemplateId) references ApprovalTemplate(TemplateId)
go


insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('ZLyTiiaRP2YextgSgS6UZ8n6uqmEFLbKPf5sXd','Date',N'日期','Date-1585550201026','Date',0,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('ZLyTiiaRP2YextgSgS6UZ8n6uqmEFLbKPf5sXd','SalesOrderNo',N'销售订单号','Text-1585550233331','Text',1,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('ZLyTiiaRP2YextgSgS6UZ8n6uqmEFLbKPf5sXd','ConsignNum',N'发货单号','Text-1585550218906','Text',2,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('ZLyTiiaRP2YextgSgS6UZ8n6uqmEFLbKPf5sXd','CustNumber',N'客户编码','Text-1585550246122','Text',3,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('ZLyTiiaRP2YextgSgS6UZ8n6uqmEFLbKPf5sXd','CustName',N'客户名称','Text-1585550253987','Text',0,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('ZLyTiiaRP2YextgSgS6UZ8n6uqmEFLbKPf5sXd','SalesOrderAmount',N'订单金额','Money-1585550464065','Money',0,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('ZLyTiiaRP2YextgSgS6UZ8n6uqmEFLbKPf5sXd','ConsignNumAmount',N'发货金额','Money-1585550471929','Money',0,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('ZLyTiiaRP2YextgSgS6UZ8n6uqmEFLbKPf5sXd','ExpiryAmount',N'逾期金额','Money-1585550293586','Money',0,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('ZLyTiiaRP2YextgSgS6UZ8n6uqmEFLbKPf5sXd','ExpiryDay',N'逾期天数','Number-1585550377794','Number',0,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('ZLyTiiaRP2YextgSgS6UZ8n6uqmEFLbKPf5sXd','Reserved',N'预留字段','Text-1586419705909','Text',0,'MoYiFeng',GETDATE())
go


insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCQ5mvgfS9WWqdPqRRxrk3KekYSzKxTgFYYs','Date',N'日期','Date-1572590781131','Date',0,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCQ5mvgfS9WWqdPqRRxrk3KekYSzKxTgFYYs','CustNumber',N'客户编码','Text-1572590786232','Text',0,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCQ5mvgfS9WWqdPqRRxrk3KekYSzKxTgFYYs','CustName',N'客户名称','Text-1572590795031','Text',0,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCQ5mvgfS9WWqdPqRRxrk3KekYSzKxTgFYYs','SettleType',N'结算方式','Text-1572590819703','Text',0,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCQ5mvgfS9WWqdPqRRxrk3KekYSzKxTgFYYs','ApprovalReason',N'申请理由','Textarea-1572590809063','Textarea',0,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCQ5mvgfS9WWqdPqRRxrk3KekYSzKxTgFYYs','MediaInfos',N'附件','File-1591751636327','File',0,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCQ5mvgfS9WWqdPqRRxrk3KekYSzKxTgFYYs','ApprovalCredit',N'申请额度','Money-1572590802623','Money',0,'MoYiFeng',GETDATE())
go



insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5','Date',N'日期','Date-1572572900811','Date',0,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5','SalesOrderNo',N'销售订单号','Text-1572573004144','Text',0,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5','CustName',N'客户名称','Text-1572572928147','Text',0,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5','SettleType',N'结算方式','Text-1572572940083','Text',0,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5','SalesOrderAmount',N'订单金额','Money-1572572967091','Money',0,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5','CreditLine',N'预授额度','Money-1572573056346','Money',0,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5','OccupyCredit',N'占用额度','Money-1572573040431','Money',0,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5','AvailableCredit',N'可用额度','Money-1572573074903','Money',0,'MoYiFeng',GETDATE())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5','ApprovalReason',N'申请理由','Textarea-1572573130176','Textarea',0,'MoYiFeng',GETDATE())
go