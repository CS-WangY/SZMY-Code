
insert into ThirdpartyApplicationConfig([AppId]
      ,[AppName]
      ,[Token]
      ,[Nonce]
      ,[Url]
      ,[EncodingAESKey]
      ,[SignLoginUrl]
      ,[Validity]
      ,[CreateUser]
      ,[CreateDate]
      ,[IsWeiXinWork])
	  values (
	  'scm','供应链管理',	'b80b9c1148434c8fb975185238a7965a',	'b80b9c1148434c8fb975185238a7965b',	'http://testscm.mymooo.com/','b80b9c1148434c8fb975185238a7965a','Account/SignLogin',1000, 'YinSheng',GETDATE(),	0

	  )
