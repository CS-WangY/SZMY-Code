namespace mymooo.weixinWork.SDK.Approval.Model.Enum
{
    /// <summary>
    /// 抄送方式 仅use_template_approver为0时生效
    /// </summary>
    public enum NotifyType
    {
        /// <summary>
        /// 提单时抄送（默认值）
        /// </summary>
        submitNotify = 1,

        /// <summary>
        /// 单据通过后抄送
        /// </summary>
        approvalNotify = 2,

        /// <summary>
        /// 提单和单据通过后抄送
        /// </summary>
        approvalSubmitNotify = 3
    }
}
