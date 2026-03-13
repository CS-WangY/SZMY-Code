
  insert into [Position]([Code]
      ,[Name]
      ,[Description]
      ,[IsForbidden]
      ,[CreateUser]
      ,[CreateDate]
      ,[ForbiddenUser]
      ,[ForbiddenDate]
      ,[IsAssistant]) 
  values (
   'manager','产品经理','产品经理',0,'YinSheng',GETDATE(),'', null, 0
  ),(
   'purchaser','采购员','采购员',0,'YinSheng',GETDATE(),'', null, 0
  )
