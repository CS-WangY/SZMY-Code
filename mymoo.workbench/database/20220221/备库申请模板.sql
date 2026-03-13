
  insert into [ApprovalTemplate]([TemplateId]
      ,[AppId]
      ,[TemplateName]
      ,[MessageExecute]
      ,[CallbackUrl]
      ,[CreateUser]
      ,[CreateDate]) values ('C4NvshbcGa1cGNUDrqqmhKw5z4Qq6au3SpZp8Vq4N','scm','备库申请模板','com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute.Approver.ReserveApplyAuditCallBack,com.mymooo.workbench.weixin.work','Supplier/ReserveApplyAudit','YinSheng', GETDATE())

	insert into ApprovalTemplateField([TemplateId]
      ,[FieldNumber]
      ,[FieldName]
      ,[FieldId]
      ,[FieldType]
      ,[KeywordSeq]
      ,[CreateUser]
      ,[CreateDate])
	  values ('C4NvshbcGa1cGNUDrqqmhKw5z4Qq6au3SpZp8Vq4N','ReserveType','备库类型','Selector-1638516844932','Selector',0,'YinSheng',GETDATE()),
	  ('C4NvshbcGa1cGNUDrqqmhKw5z4Qq6au3SpZp8Vq4N','ReserveReason','备库原因','Textarea-1638516905596','Textarea',0,'YinSheng',GETDATE()),
	  ('C4NvshbcGa1cGNUDrqqmhKw5z4Qq6au3SpZp8Vq4N','CategoryName','品类名称','Text-1638516917862','Text',0,'YinSheng',GETDATE()),
	  ('C4NvshbcGa1cGNUDrqqmhKw5z4Qq6au3SpZp8Vq4N','ProductSeries','备库产品系列','Text-1638516926612','Text',0,'YinSheng',GETDATE()),
	  ('C4NvshbcGa1cGNUDrqqmhKw5z4Qq6au3SpZp8Vq4N','ReserveAmount','备库金额','Number-1638516954377','Number',0,'YinSheng',GETDATE()),
	  ('C4NvshbcGa1cGNUDrqqmhKw5z4Qq6au3SpZp8Vq4N','SupplierName','供应商名称','Text-1638516966425','Text',0,'YinSheng',GETDATE()),
	  ('C4NvshbcGa1cGNUDrqqmhKw5z4Qq6au3SpZp8Vq4N','AccountPeriod','账期','Text-1638516986730','Text',0,'YinSheng',GETDATE()),
	  ('C4NvshbcGa1cGNUDrqqmhKw5z4Qq6au3SpZp8Vq4N','NowCustNumbers','现有客户数','Number-1638517001625','Number',0,'YinSheng',GETDATE()),
	  ('C4NvshbcGa1cGNUDrqqmhKw5z4Qq6au3SpZp8Vq4N','MonthlyDeliveryQuantity','月出库数量（近三月平均）','Number-1638517010405','Number',0,'YinSheng',GETDATE()),
	  ('C4NvshbcGa1cGNUDrqqmhKw5z4Qq6au3SpZp8Vq4N','MonthlyDeliveryAmount','月出库金额（近三月平均）','Money-1638517046859','Money',0,'YinSheng',GETDATE()),
	  ('C4NvshbcGa1cGNUDrqqmhKw5z4Qq6au3SpZp8Vq4N','ReturnToRiskRatio','收益风险比（可能收益/可能损失）','Number-1638517070147','Number',0,'YinSheng',GETDATE()),
	  ('C4NvshbcGa1cGNUDrqqmhKw5z4Qq6au3SpZp8Vq4N','RiskDescription','风险说明','Textarea-1638517079949','Textarea',0,'YinSheng',GETDATE()),
	  ('C4NvshbcGa1cGNUDrqqmhKw5z4Qq6au3SpZp8Vq4N','Remark','备注','Textarea-1638517114180','Textarea',0,'YinSheng',GETDATE()),
	  ('C4NvshbcGa1cGNUDrqqmhKw5z4Qq6au3SpZp8Vq4N','MediaInfos','附件（备库计划、备库清单等）','File-1638517093361','File',0,'YinSheng',GETDATE())

