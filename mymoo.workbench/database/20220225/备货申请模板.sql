
  insert into [ApprovalTemplate]([TemplateId]
      ,[AppId]
      ,[TemplateName]
      ,[MessageExecute]
      ,[CallbackUrl]
      ,[CreateUser]
      ,[CreateDate]) values ('3WK73TG63GfBwLJTDEKsQ4fibC9JvL3VbiT1NYzg','crm','备货申请模板','com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute.Approver.StockApplyAuditCallBack,com.mymooo.workbench.weixin.work','SalesOrder/StockApplyAudit','YinSheng', GETDATE())

	insert into ApprovalTemplateField([TemplateId]
      ,[FieldNumber]
      ,[FieldName]
      ,[FieldId]
      ,[FieldType]
      ,[KeywordSeq]
      ,[CreateUser]
      ,[CreateDate])
	  values ('3WK73TG63GfBwLJTDEKsQ4fibC9JvL3VbiT1NYzg','CompanyName','客户名称','Textarea-1632729275238','Textarea',0,'YinSheng',GETDATE()),
	  ('3WK73TG63GfBwLJTDEKsQ4fibC9JvL3VbiT1NYzg','CompanyGrade','客户等级','Text-1645414780659','Text',0,'YinSheng',GETDATE()),
	  ('3WK73TG63GfBwLJTDEKsQ4fibC9JvL3VbiT1NYzg','PaymentMethod','结算方式','Text-1645414785778','Text',0,'YinSheng',GETDATE()),
	  ('3WK73TG63GfBwLJTDEKsQ4fibC9JvL3VbiT1NYzg','StockAmount','备货金额','Money-1632729334866','Money',0,'YinSheng',GETDATE()),
	  ('3WK73TG63GfBwLJTDEKsQ4fibC9JvL3VbiT1NYzg','Reason','申请理由','Textarea-1632729350510','Textarea',0,'YinSheng',GETDATE()),
	  ('3WK73TG63GfBwLJTDEKsQ4fibC9JvL3VbiT1NYzg','MediaInfos','附件','File-1632729345206','File',0,'YinSheng',GETDATE())

