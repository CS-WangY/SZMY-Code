

update ThirdpartyApplicationConfig set SignLoginUrl = 'Account/SignLogin',EncodingAESKey = 'b80b9c1148434c8fb975185238a7965a',Token='b80b9c1148434c8fb975185238a7965a',Nonce='b80b9c1148434c8fb975185238a7965b' where AppId= 'credit'
update ApprovalTemplate set CallbackUrl = 'Credit/Audit' where TemplateId= '3TkaHCQ5mvgfS9WWqdPqRRxrk3KekYSzKxTgFYYs'
update ApprovalTemplate set AppId = 'platformAdmin',CallbackUrl='salesorder/audit' where TemplateId= '3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5'


update ThirdpartyApplicationConfig set Token='c26d78b3d9785249caac9a6d43c76bf9' where AppId ='platformAdmin'
go

insert into ApprovalTemplate(TemplateId,AppId,TemplateName,MessageExecute,CallbackUrl,CreateUser,CreateDate) 
values('BsAbJx8NnpqCgnKeq7SbHNrjJyFgB4GyAnH1KRmRs','credit','付款监管模板','com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute.Approver.PaymentSuperviseAuditCallBack,com.mymooo.workbench.weixin.work','Credit/PaymentSupervise','MoYiFeng',getdate())
go

insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('BsAbJx8NnpqCgnKeq7SbHNrjJyFgB4GyAnH1KRmRs','Date','日期','Date-1618193650582','Date',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('BsAbJx8NnpqCgnKeq7SbHNrjJyFgB4GyAnH1KRmRs','Amount','付款金额','Money-1618193664429','Money',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('BsAbJx8NnpqCgnKeq7SbHNrjJyFgB4GyAnH1KRmRs','Urgency','紧急程度','Selector-1618193988539','Selector',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('BsAbJx8NnpqCgnKeq7SbHNrjJyFgB4GyAnH1KRmRs','Description','付款备注','Textarea-1618193669781','Textarea',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('BsAbJx8NnpqCgnKeq7SbHNrjJyFgB4GyAnH1KRmRs','Attachment','附件','File-1618193164888','File',0,'MoYiFeng',getdate())
go


insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCQ5mvgfS9WWqdPqRRxrk3KekYSzKxTgFYYs','Date','日期','Date-1572590781131','Date',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCQ5mvgfS9WWqdPqRRxrk3KekYSzKxTgFYYs','CustNumber','客户编码','Text-1572590786232','Text',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCQ5mvgfS9WWqdPqRRxrk3KekYSzKxTgFYYs','CustName','客户名称','Text-1572590795031','Text',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCQ5mvgfS9WWqdPqRRxrk3KekYSzKxTgFYYs','SettleType','结算方式','Text-1572590819703','Text',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCQ5mvgfS9WWqdPqRRxrk3KekYSzKxTgFYYs','ApprovalReason','申请理由','Textarea-1572590809063','Textarea',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCQ5mvgfS9WWqdPqRRxrk3KekYSzKxTgFYYs','MediaInfos','附件','File-1591751636327','File',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCQ5mvgfS9WWqdPqRRxrk3KekYSzKxTgFYYs','ApprovalCredit','申请额度','Money-1572590802623','Money',0,'MoYiFeng',getdate())
go



insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5','Date','日期','Date-1572572900811','Date',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5','SalesOrderNo','销售订单号','Text-1572573004144','Text',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5','CustName','客户名称','Text-1572572928147','Text',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5','SettleType','结算方式','Text-1572572940083','Text',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5','SalesOrderAmount','订单金额','Money-1572572967091','Money',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5','CreditLine','预授额度','Money-1572573056346','Money',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5','OccupyCredit','占用额度','Money-1572573040431','Money',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5','AvailableCredit','可用额度','Money-1572573074903','Money',0,'MoYiFeng',getdate())
insert into ApprovalTemplateField(TemplateId,FieldNumber,FieldName,FieldId,FieldType,KeywordSeq,CreateUser,CreateDate)
values('3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5','ApprovalReason','申请理由','Textarea-1572573130176','Textarea',0,'MoYiFeng',getdate())

go


update ApprovalTemplateField set FieldId='Text-1614922093790' where TemplateId='3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5' and FieldNumber = 'SettleType'
update ApprovalTemplateField set FieldId='Money-1614914421315' where TemplateId='3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5' and FieldNumber = 'OccupyCredit'
update ApprovalTemplateField set FieldId='Money-1614922189996' where TemplateId='3TkaHCPgcyoh2uHDMxwEH1FWxNJ1thqjSoycMAf5' and FieldNumber = 'AvailableCredit'

go

select* from ThirdpartyApplicationConfig
select * from ApprovalTemplate
select * from ApprovalTemplateField 



	