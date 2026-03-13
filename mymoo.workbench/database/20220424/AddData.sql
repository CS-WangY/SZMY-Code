
use mymooo_workbench
go

insert into ApprovalTemplate

values
('C4NyrqJ321adtcwvnJDaYPKyjfL77NkAfVtxkgTH4','platformAdmin','变更单申请模板','com.mymooo.workbench.weixin.work.ReceiveMessage.MessageExecute.Approver.ChangeOrderApplyCallBack,com.mymooo.workbench.weixin.work','SalesOrderChange/ApplyChangeOrderCallBack','WangZhuo',sysdatetime())

insert ApprovalTemplateField
values
('C4NyrqJ321adtcwvnJDaYPKyjfL77NkAfVtxkgTH4','ApplyDate','申请日期','Date-1650360868476','Date',0,'WangZhuo',SYSDATETIME(),'')
,
('C4NyrqJ321adtcwvnJDaYPKyjfL77NkAfVtxkgTH4','SaleName','业务员','Text-1650360888125','Text',1,'WangZhuo',SYSDATETIME(),'')
,
('C4NyrqJ321adtcwvnJDaYPKyjfL77NkAfVtxkgTH4','CompanyCode','客户编码','Text-1650360894405','Text',2,'WangZhuo',SYSDATETIME(),'')
,
('C4NyrqJ321adtcwvnJDaYPKyjfL77NkAfVtxkgTH4','CompanyName','客户名称','Text-1650360903113','Text',3,'WangZhuo',SYSDATETIME(),'')
,
('C4NyrqJ321adtcwvnJDaYPKyjfL77NkAfVtxkgTH4','PayMethodName','结算方式','Text-1650360912834','Text',4,'WangZhuo',SYSDATETIME(),'')
,
('C4NyrqJ321adtcwvnJDaYPKyjfL77NkAfVtxkgTH4','CreditLine','信用额度','Money-1650360929461','Money',5,'WangZhuo',SYSDATETIME(),'')
,
('C4NyrqJ321adtcwvnJDaYPKyjfL77NkAfVtxkgTH4','OccupyCredit','占用额度','Money-1650360936927','Money',6,'WangZhuo',SYSDATETIME(),'')
,
('C4NyrqJ321adtcwvnJDaYPKyjfL77NkAfVtxkgTH4','AvailableCredit','可用额度','Money-1650360946155','Money',7,'WangZhuo',SYSDATETIME(),'')
,
('C4NyrqJ321adtcwvnJDaYPKyjfL77NkAfVtxkgTH4','OverdueAmount','逾期金额','Money-1650360955006','Money',8,'WangZhuo',SYSDATETIME(),'')
,
('C4NyrqJ321adtcwvnJDaYPKyjfL77NkAfVtxkgTH4','ExpiryDay','逾期天数','Number-1650360962452','Number',9,'WangZhuo',SYSDATETIME(),'')
,
('C4NyrqJ321adtcwvnJDaYPKyjfL77NkAfVtxkgTH4','OrderTotalAmount','原订单金额','Money-1650360975313','Money',10,'WangZhuo',SYSDATETIME(),'')
,
('C4NyrqJ321adtcwvnJDaYPKyjfL77NkAfVtxkgTH4','ChangeOrderTotalAmount','变更后订单金额','Money-1650360983613','Money',11,'WangZhuo',SYSDATETIME(),'')
,
('C4NyrqJ321adtcwvnJDaYPKyjfL77NkAfVtxkgTH4','ChangeReason','变更原因','Textarea-1650360996767','Textarea',12,'WangZhuo',SYSDATETIME(),'')