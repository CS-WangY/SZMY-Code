insert into ThirdpartyApplicationConfig(AppId,AppName,Token,Url,Validity,CreateUser,CreateDate)
values('DGweixinwork','东莞企业微信','wwa4d9df34a9ec01f5','https://qyapi.weixin.qq.com/',1000,'MoYiFeng',getdate())
go

insert into ThirdpartyApplicationDetail(AppId,DetailCode,DetailName,Agentid,AppSecret,RedirectUri,Token,EncodingAESKey,MessageExecute,CreateUser,CreateDate)
values('DGweixinwork','AddressBook','通讯录应用','','_MZVsfRTiw6R7TBh3NJL-xZkrEqRRUMfAMaE5jyVUZs','','nsBhMb4KreM2yKR5HlIeH5UWo','NBqkTnmCj6fLR7BLtCJAb9xdUzT2yfzQjWpZgpSYBU4','com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute.AddressBookMessageExecute,com.mymooo.workbench.weixin.work','MoYiFeng',getdate());
--开发 莫
insert into ThirdpartyApplicationDetail(AppId,DetailCode,DetailName,Agentid,AppSecret,RedirectUri,Token,EncodingAESKey,MessageExecute,CreateUser,CreateDate)
values('DGweixinwork','Application','工作平台应用','1000002','NN-2r6DgPNPbrodqHYMUMO3McOKSklgwTi4ntv_U2C4','http://devwork.mymooo.com:1202/home/WeiXinWorklogin','Jw8oAQU9w6ehzmKUGmgOivOz','Hq2qbl64fzhhmiCZKFKBVcNR4oquFvvtPNWxUWqDvcS','com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute.ApplicationMessageExecute,com.mymooo.workbench.weixin.work','MoYiFeng',getdate());

--测试
insert into ThirdpartyApplicationDetail(AppId,DetailCode,DetailName,Agentid,AppSecret,RedirectUri,Token,EncodingAESKey,MessageExecute,CreateUser,CreateDate)
values('DGweixinwork','Application','工作平台应用','1000003','UJ_8C8Vo6LDcQdV7wKfvU92NfhxkQkUC3PC_QaTuNDg','http://testworkbench.mymooo.com:9502/home/WeiXinWorklogin','McznHbpENVbMwqRzx8tNiRWIZ','wCn8jAFiPpjhLg4kFdV9HB3UlzvbRSX3U9fT9b7qyKE','com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute.ApplicationMessageExecute,com.mymooo.workbench.weixin.work','MoYiFeng',getdate());

--生产环境
insert into ThirdpartyApplicationDetail(AppId,DetailCode,DetailName,Agentid,AppSecret,RedirectUri,Token,EncodingAESKey,MessageExecute,CreateUser,CreateDate)
values('DGweixinwork','Application','工作平台应用','1000004','WvNPL3-_DekXU-iQjXDu4dxBNnvePkIktnAJX_nWvj4','http://workbench.mymooo.com/home/WeiXinWorklogin','CWOfpMexeT9UWQ0jj0NW','Nf8c89iTobWiltrkml3Ycg8v4kcMyEKIjQYGKCsVR9t','com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute.ApplicationMessageExecute,com.mymooo.workbench.weixin.work','MoYiFeng',getdate());


insert into ThirdpartyApplicationDetail(AppId,DetailCode,DetailName,Agentid,AppSecret,RedirectUri,Token,EncodingAESKey,MessageExecute,CreateUser,CreateDate)
values('DGweixinwork','Approval','审批应用','3010040','5jJ-rviW0bJ2o6I2wXZCllQk1JvgRq79d8LWO9aB45A','','aFTiYUL6HnaxcYIHdCBG3In5nNU','FpFOht6ko1isYTJndMXSMs3eA61XAEnHeVC6CXv2JWO','com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute.ApprovalMessageExecute,com.mymooo.workbench.weixin.work','MoYiFeng',getdate());

go