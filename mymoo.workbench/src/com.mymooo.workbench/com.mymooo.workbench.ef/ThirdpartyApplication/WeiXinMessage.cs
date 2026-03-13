using mymooo.core.Attributes.Redis;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace com.mymooo.workbench.ef.ThirdpartyApplication
{
	[RedisKey("mymooo-workbench-weixinmessage", 14, false)]
	public partial class WeiXinMessage
	{
		public WeiXinMessage()
		{
			this.ApprovalMessage = [];
		}

		/// <summary>
		/// Id
		/// </summary>
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]  //设置自增
		[Key]
		[SqlSugar.SugarColumn(IsPrimaryKey = true)]
		public long Id { get; set; }

		/// <summary>
		/// 第三方应用id
		/// </summary>
		[Required]
		public long ApplicationDetailId { get; set; }

		/// <summary>
		/// 接收到的消息
		/// </summary>
		[Required]
		public string Message { get; set; }

		/// <summary>
		/// 是否分析完成
		/// </summary>
		public bool IsComplete { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime? CreateDate { get; set; }

		/// <summary>
		/// 分析完成时间
		/// </summary>
		public DateTime? CompleteDate { get; set; }

		/// <summary>
		/// 分析结果
		/// </summary>
		public string Result { get; set; }

		/// <summary>
		/// 错误信息堆栈
		/// </summary>
		public string StackTrace { get; set; }

		public string Spno { get; set; }
		public int Status { get; set; }

		[Navigate(NavigateType.OneToOne, nameof(ApplicationDetailId))]
		public virtual ThirdpartyApplicationDetail ApplicationDetail { get; set; }

		[SugarColumn(IsIgnore = true)]
		public virtual ICollection<ApprovalMessage> ApprovalMessage { get; set; }
	}
}
