namespace mymooo.weixinWork.SDK.Approval.Model.Enum
{
    /// <summary>
    /// 审批字段类型
    /// </summary>
    public enum ApproverFieldType
    {
        /// <summary>
        /// 字符
        /// </summary>
        Text,

        /// <summary>
        /// 多行文本
        /// </summary>
        Textarea,

        /// <summary>
        /// 日期
        /// </summary>
        Date,

        /// <summary>
        /// 金额
        /// </summary>
        Money,

        /// <summary>
        /// 数字
        /// </summary>
        Number,

        /// <summary>
        /// 附件
        /// </summary>
        File,

        /// <summary>
        /// 下拉列表
        /// </summary>
        Selector,

        /// <summary>
        /// 员工
        /// </summary>
        Contact,

        /// <summary>
        /// 关联申请单
        /// </summary>
        /// 
        RelatedApproval
    }
}
