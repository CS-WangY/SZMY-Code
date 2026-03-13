using System.Runtime.Serialization;

namespace com.mymooo.mall.core.Model.Message

{
	[KnownType(typeof(MessageInfo))]
    public class MessageHelp
    {
        /// <summary>
        ///     是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        ///     返回消息
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        ///     返回结果信息
        /// </summary>
        public string? ReturnObject { get; set; }

        public string? Data { get; set; }

    }

    public class MessageCodeHelp
    {
        /// <summary>
        ///     是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        public string? Code { get; set; }

        /// <summary>
        ///     返回消息
        /// </summary>
        public string? Message { get; set; }


        public string? Data { get; set; }

    }

    public class MessageHelpForCredit
    {
        /// <summary>
        ///     是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        ///     返回消息
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        ///返回的数据
        /// </summary>
        public CreditData? Data { get; set; }


        
    }

    public class CreditData
    {
        public bool IsPass { get; set; }

        /// <summary>
        /// 信用额度
        /// </summary>
        public decimal CreditLine { get; set; }
        /// <summary>
        /// 可用额度
        /// </summary>
        public decimal AvailableCredit { get; set; }

        /// <summary>
        /// 占用额度
        /// </summary>
        public decimal OccupyCredit { get; set; }


        /// <summary>
        /// 逾期天数，取最大天数
        /// </summary>
        public int ExpiryDay { get; set; }

        /// <summary>
        /// 逾期金额，逾期金额合计
        /// </summary>
        public decimal ExpiryAmount { get; set; }

        /// <summary>
        /// 固定额度
        /// </summary>
        public decimal FixedCreditLine { get; set; }

        /// <summary>
        /// 临时额度的有效期
        /// </summary>
        public DateTime? ValidityDate { get; set; }


        /// <summary>
        /// 出错返回码，为0表示成功，非0表示调用失败
        /// </summary>
        public int Errcode { get; set; }

        /// <summary>
        /// 返回码提示语
        /// </summary>
        public string? Errmsg { get; set; }

        /// <summary>
        /// 审批单号
        /// </summary>
        public required string Sp_no { get; set; }

    }

    /// <summary>
    /// </summary>
    public class MessagesHelp<T>
    {
        /// <summary>
        ///     是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        ///     返回消息
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        ///     返回结果信息
        /// </summary>
        public T? ReturnObject { get; set; }
        
        //为兼容Erp的返回值的反序列化，增加的属性
        public T? Data { get; set; }


        /// <summary>
        /// code码
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// 返回消息
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    public class MessageInfo : MessageHelp
    {
        /// <summary>
        ///     key
        /// </summary>
        public long ID { get; set; }
    }


    public class MessageUserInfo : MessageHelp
    {
        /// <summary>
        ///     微信关注用户
        /// </summary>
        public string? OpenId { get; set; }

        /// <summary>
        ///     微信关注用户唯一
        /// </summary>
        public string? UnionId { get; set; }

        /// <summary>
        ///     用户头像
        /// </summary>
        public string? UserHeadImg { get; set; }

        /// <summary>
        ///     用户名称
        /// </summary>
        public string? UserName { get; set; }
    }

    public class MessageScore : MessageHelp
    {
        /// <summary>
        ///     评价分数
        /// </summary>
        public decimal Score { get; set; }  

        /// <summary>
        ///     评价数量
        /// </summary>
        public long Num { get; set; }
    }

    public class MessageDem : MessageHelp
    {
        /// <summary>
        ///     数量
        /// </summary>
        public long Num { get; set; }
    }
    public class ApplyeventResponse
    {
        /// <summary>
        /// 出错返回码，为0表示成功，非0表示调用失败
        /// </summary>
        public int Errcode { get; set; }

        /// <summary>
        /// 返回码提示语
        /// </summary>
        public string? Errmsg { get; set; }

        /// <summary>
        /// 审批单号
        /// </summary>
        public required string Sp_no { get; set; }
    }
}