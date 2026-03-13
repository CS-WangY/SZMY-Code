

use mymooo_workbench
go

insert into ApprovalTemplate
values
('3TjyGzgTrLhaKHnN77z2cpzBwxtSncoh53oVebou','platformAdmin','样品申请','com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute.Approver.ApplySampleCallBack,com.mymooo.workbench.weixin.work','','WangZhuo',SYSDATETIME())




insert into ApprovalTemplateField
values
('3TjyGzgTrLhaKHnN77z2cpzBwxtSncoh53oVebou','SalesOrder','销售订单号','Text-1650950279061','Text',0,'WangZhuo',SYSDATETIME(),'')
,
('3TjyGzgTrLhaKHnN77z2cpzBwxtSncoh53oVebou','CompanyCode','客户编码','Text-1650950302138','Text',1,'WangZhuo',SYSDATETIME(),'')
,
('3TjyGzgTrLhaKHnN77z2cpzBwxtSncoh53oVebou','CompanyName','客户名称','item-1543199902662','Text',2,'WangZhuo',SYSDATETIME(),'')
,
('3TjyGzgTrLhaKHnN77z2cpzBwxtSncoh53oVebou','PayMethodName','结算方式','Text-1650950324079','Text',3,'WangZhuo',SYSDATETIME(),'')
,
('3TjyGzgTrLhaKHnN77z2cpzBwxtSncoh53oVebou','Amount','金额','item-1543199965432','Money',4,'WangZhuo',SYSDATETIME(),'')
,
('3TjyGzgTrLhaKHnN77z2cpzBwxtSncoh53oVebou','CustomerType','客户类型','item-1559093421370','Selector',5,'WangZhuo',SYSDATETIME(),'[ { "key": "option-1435536718", "value": [ { "text": "贸易商", "lang": "zh_CN" } ] }, { "key": "option-1435536719", "value": [ { "text": "终端商", "lang": "zh_CN" } ] } ]')
,
('3TjyGzgTrLhaKHnN77z2cpzBwxtSncoh53oVebou','ApplyType','申请类型','item-1559093577853','Selector',6,'WangZhuo',SYSDATETIME(),'[ { "key": "option-4256076920", "value": [ { "text": "开发客户，确认品质", "lang": "zh_CN" } ] }, { "key": "option-4256076921", "value": [ { "text": "批量采购，确认品质", "lang": "zh_CN" } ] } ]')
,
('3TjyGzgTrLhaKHnN77z2cpzBwxtSncoh53oVebou','ApplyReason','申请理由','item-1559185994550','Textarea',7,'WangZhuo',SYSDATETIME(),'')
,
('3TjyGzgTrLhaKHnN77z2cpzBwxtSncoh53oVebou','Files','附件','File-1614564107041','File',8,'WangZhuo',SYSDATETIME(),'')


