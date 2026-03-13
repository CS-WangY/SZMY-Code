
use mymooo_workbench
go
insert into ApprovalTemplate(TemplateId,AppId,TemplateName,MessageExecute,CallbackUrl,CreateUser,CreateDate)
values('3TjypEBGALiBwJ4WAmCcokh9LqV3Y5x6bm5RPBBM','crm',N'客户结算方式变更申请模板','com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute.Approver.SettlementChangeAuditCallBack,com.mymooo.workbench.weixin.work','Company/SettlementChangeAudit','MoYiFeng',getdate())
go

insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TjypEBGALiBwJ4WAmCcokh9LqV3Y5x6bm5RPBBM','Date','申请日期','Date-1622700977286','Date',0,'MoYiFeng',getdate())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TjypEBGALiBwJ4WAmCcokh9LqV3Y5x6bm5RPBBM','CompanyCode','客户编码','Text-1622700932944','Text',0,'MoYiFeng',getdate())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TjypEBGALiBwJ4WAmCcokh9LqV3Y5x6bm5RPBBM','CompanyName','客户名称','item-1551429413872','Text',0,'MoYiFeng',getdate())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TjypEBGALiBwJ4WAmCcokh9LqV3Y5x6bm5RPBBM','OriginalSettlementType','原结算方式','item-1551429425720','Text',0,'MoYiFeng',getdate())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TjypEBGALiBwJ4WAmCcokh9LqV3Y5x6bm5RPBBM','ChangeSettlementType','变更结算方式','Text-1622701007222','Text',0,'MoYiFeng',getdate())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TjypEBGALiBwJ4WAmCcokh9LqV3Y5x6bm5RPBBM','ChangeDays','变更后月结天数','Number-1622701305167','Number',0,'MoYiFeng',getdate())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TjypEBGALiBwJ4WAmCcokh9LqV3Y5x6bm5RPBBM','MediaInfos','附件','item-1551429473368','File',0,'MoYiFeng',getdate())
go
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TjypEBGALiBwJ4WAmCcokh9LqV3Y5x6bm5RPBBM','Remarks','原因说明','item-1551429464553','Textarea',0,'MoYiFeng',getdate())
go




select * from ApprovalTemplate
select * from ApprovalTemplateField
select * from ThirdpartyApplicationConfig
