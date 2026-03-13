-- 添加开发环境云仓储地址
insert into ThirdpartyApplicationConfig
select 'devcloudstock' appId, 
'开发环境云仓储' AppName,
Token,
Nonce,
'http://localhost:9000/' Url,
EncodingAESKey,
SignLoginUrl,
Validity,
CreateUser,
CreateDate,
IsWeiXinWork,
IsProduction
from ThirdpartyApplicationConfig where AppId='testmymoooerp'

-- 添加测试环境云仓储地址
insert into ThirdpartyApplicationConfig
select 'testcloudstock' appId, 
'测试环境云仓储' AppName,
Token,
Nonce,
'http://testcloudstock.mymooo.com/' Url,
EncodingAESKey,
SignLoginUrl,
Validity,
CreateUser,
CreateDate,
IsWeiXinWork,
IsProduction
from ThirdpartyApplicationConfig where AppId='testmymoooerp'

-- 添加bug环境云仓储地址
insert into ThirdpartyApplicationConfig
select 'bugcloudstock' appId, 
'bug环境云仓储' AppName,
Token,
Nonce,
'http://bugcloudstock.mymooo.com/' Url,
EncodingAESKey,
SignLoginUrl,
Validity,
CreateUser,
CreateDate,
IsWeiXinWork,
IsProduction
from ThirdpartyApplicationConfig where AppId='testmymoooerp'

-- 添加预发版环境云仓储地址
insert into ThirdpartyApplicationConfig
select 'previewcloudstock' appId, 
'预发版环境云仓储' AppName,
Token,
Nonce,
'http://previewcloudstock.mymooo.com/' Url,
EncodingAESKey,
SignLoginUrl,
Validity,
CreateUser,
CreateDate,
IsWeiXinWork,
IsProduction
from ThirdpartyApplicationConfig where AppId='testmymoooerp'

-- 添加金蝶预发版环境云仓储地址
insert into ThirdpartyApplicationConfig
select 'previewcloudstockapi2' appId, 
'金蝶预发版环境云仓储' AppName,
Token,
Nonce,
'http://previewcloudstockapi2.mymooo.com/' Url,
EncodingAESKey,
SignLoginUrl,
Validity,
CreateUser,
CreateDate,
IsWeiXinWork,
IsProduction
from ThirdpartyApplicationConfig where AppId='testmymoooerp'

-- 添加生产环境云仓储地址
insert into ThirdpartyApplicationConfig
select 'cloudstockapi' appId, 
'生产环境云仓储' AppName,
Token,
Nonce,
'http://cloudstockapi.mymooo.com/' Url,
EncodingAESKey,
SignLoginUrl,
Validity,
CreateUser,
CreateDate,
IsWeiXinWork,
IsProduction
from ThirdpartyApplicationConfig where AppId='testmymoooerp'