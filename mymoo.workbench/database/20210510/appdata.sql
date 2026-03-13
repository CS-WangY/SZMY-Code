

insert into ApprovalTemplate(TemplateId,AppId,TemplateName,MessageExecute,CallbackUrl,CreateUser,CreateDate) 
values('BsAbuZ4LM4h8wXibGZwWsR7iycohepPW3KH56vg8G','credit','申请客户临时额度模板','com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute.Approver.TemporaryCreditApplyeventAuditCallBack,com.mymooo.workbench.weixin.work','Credit/TemporaryAudit','MoYiFeng',getdate())
go

insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('BsAbuZ4LM4h8wXibGZwWsR7iycohepPW3KH56vg8G','Date','日期','Date-1572590781131','Date',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('BsAbuZ4LM4h8wXibGZwWsR7iycohepPW3KH56vg8G','CustNumber','客户编码','Text-1572590786232','Text',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('BsAbuZ4LM4h8wXibGZwWsR7iycohepPW3KH56vg8G','CustName','客户名称','Text-1572590795031','Text',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('BsAbuZ4LM4h8wXibGZwWsR7iycohepPW3KH56vg8G','SettleType','结算方式','Text-1572590819703','Text',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('BsAbuZ4LM4h8wXibGZwWsR7iycohepPW3KH56vg8G','ApprovalReason','申请理由','Textarea-1572590809063','Textarea',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('BsAbuZ4LM4h8wXibGZwWsR7iycohepPW3KH56vg8G','MediaInfos','附件','File-1591751636327','File',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('BsAbuZ4LM4h8wXibGZwWsR7iycohepPW3KH56vg8G','ApprovalCredit','申请临时额度','Money-1572590802623','Money',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('BsAbuZ4LM4h8wXibGZwWsR7iycohepPW3KH56vg8G','Validity','有效期(月)','Number-1620625122118','Number',0,'MoYiFeng',getdate())
go

select * from ApprovalTemplate


select * from ThirdpartyApplicationDetail

select * from ApprovalTemplateField
