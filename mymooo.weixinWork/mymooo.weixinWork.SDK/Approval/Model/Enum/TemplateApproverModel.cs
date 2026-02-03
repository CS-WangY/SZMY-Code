namespace mymooo.weixinWork.SDK.Approval.Model.Enum
{
    /// <summary>
    /// 审批人模式：0-通过接口指定审批人、抄送人（此时approver、notifyer等参数可用）; 1-使用此模板在管理后台设置的审批流程(需要保证审批流程中没有“申请人自选”节点)，支持条件审批。默认为0
    /// </summary>
    public enum TemplateApproverModel
    {
        /// <summary>
        /// 通过接口指定审批人、抄送人
        /// </summary>
        interfaceType,

        /// <summary>
        /// 使用此模板在管理后台设置的审批流程
        /// </summary>
        background
    }
}
