//namespace mymooo.weixinWork.SDK.Approval.Model
//{
//	// 注意: 生成的代码可能至少需要 .NET Framework 4.5 或 .NET Core/Standard 2.0。
//	/// <remarks/>
//	[System.SerializableAttribute()]
//	[System.ComponentModel.DesignerCategory("code")]
//	[System.Xml.Serialization.XmlType(AnonymousType = true)]
//	[System.Xml.Serialization.XmlRoot(Namespace = "", IsNullable = false)]
//	public partial class ApprovalMessage
//	{

//		private string toUserNameField;

//		private string fromUserNameField;

//		private uint createTimeField;

//		private string msgTypeField;

//		private string eventField;

//		private uint agentIDField;

//		private ApprovalInfo approvalInfoField;

//		/// <remarks/>
//		public string ToUserName
//		{
//			get
//			{
//				return this.toUserNameField;
//			}
//			set
//			{
//				this.toUserNameField = value;
//			}
//		}

//		/// <remarks/>
//		public string FromUserName
//		{
//			get
//			{
//				return this.fromUserNameField;
//			}
//			set
//			{
//				this.fromUserNameField = value;
//			}
//		}

//		/// <remarks/>
//		public uint CreateTime
//		{
//			get
//			{
//				return this.createTimeField;
//			}
//			set
//			{
//				this.createTimeField = value;
//			}
//		}

//		/// <remarks/>
//		public string MsgType
//		{
//			get
//			{
//				return this.msgTypeField;
//			}
//			set
//			{
//				this.msgTypeField = value;
//			}
//		}

//		/// <remarks/>
//		public string Event
//		{
//			get
//			{
//				return this.eventField;
//			}
//			set
//			{
//				this.eventField = value;
//			}
//		}

//		/// <remarks/>
//		public uint AgentID
//		{
//			get
//			{
//				return this.agentIDField;
//			}
//			set
//			{
//				this.agentIDField = value;
//			}
//		}

//		/// <remarks/>
//		public ApprovalInfo ApprovalInfo
//		{
//			get
//			{
//				return this.approvalInfoField;
//			}
//			set
//			{
//				this.approvalInfoField = value;
//			}
//		}
//	}

//	/// <remarks/>
//	[System.SerializableAttribute()]
//	[System.ComponentModel.DesignerCategory("code")]
//	[System.Xml.Serialization.XmlType(AnonymousType = true)]
//	public partial class ApprovalInfo
//	{

//		private string spNoField;

//		private string spNameField;

//		private byte spStatusField;

//		private string templateIdField;

//		private uint applyTimeField;

//		private ApprovalInfoApplyer applyerField;

//		private ApprovalInfoSpRecord[] spRecordField;

//		private ApprovalInfoNotifyer[] notifyerField;

//		private byte statuChangeEventField;

//		/// <remarks/>
//		public string SpNo
//		{
//			get
//			{
//				return this.spNoField;
//			}
//			set
//			{
//				this.spNoField = value;
//			}
//		}

//		/// <remarks/>
//		public string SpName
//		{
//			get
//			{
//				return this.spNameField;
//			}
//			set
//			{
//				this.spNameField = value;
//			}
//		}

//		/// <remarks/>
//		public byte SpStatus
//		{
//			get
//			{
//				return this.spStatusField;
//			}
//			set
//			{
//				this.spStatusField = value;
//			}
//		}

//		/// <remarks/>
//		public string TemplateId
//		{
//			get
//			{
//				return this.templateIdField;
//			}
//			set
//			{
//				this.templateIdField = value;
//			}
//		}

//		/// <remarks/>
//		public uint ApplyTime
//		{
//			get
//			{
//				return this.applyTimeField;
//			}
//			set
//			{
//				this.applyTimeField = value;
//			}
//		}

//		/// <remarks/>
//		public ApprovalInfoApplyer Applyer
//		{
//			get
//			{
//				return this.applyerField;
//			}
//			set
//			{
//				this.applyerField = value;
//			}
//		}

//		/// <remarks/>
//		[System.Xml.Serialization.XmlElement("SpRecord")]
//		public ApprovalInfoSpRecord[] SpRecord
//		{
//			get
//			{
//				return this.spRecordField;
//			}
//			set
//			{
//				this.spRecordField = value;
//			}
//		}

//		/// <remarks/>
//		[System.Xml.Serialization.XmlElement("Notifyer")]
//		public ApprovalInfoNotifyer[] Notifyer
//		{
//			get
//			{
//				return this.notifyerField;
//			}
//			set
//			{
//				this.notifyerField = value;
//			}
//		}

//		/// <remarks/>
//		public byte StatuChangeEvent
//		{
//			get
//			{
//				return this.statuChangeEventField;
//			}
//			set
//			{
//				this.statuChangeEventField = value;
//			}
//		}
//	}

//	/// <remarks/>
//	[System.SerializableAttribute()]
//	[System.ComponentModel.DesignerCategory("code")]
//	[System.Xml.Serialization.XmlType(AnonymousType = true)]
//	public partial class ApprovalInfoApplyer	
//	{

//		private string userIdField;

//		private string partyField;

//		/// <remarks/>
//		public string UserId
//		{
//			get
//			{
//				return this.userIdField;
//			}
//			set
//			{
//				this.userIdField = value;
//			}
//		}

//		/// <remarks/>
//		public string Party
//		{
//			get
//			{
//				return this.partyField;
//			}
//			set
//			{
//				this.partyField = value;
//			}
//		}
//	}

//	/// <remarks/>
//	[System.SerializableAttribute()]
//	[System.ComponentModel.DesignerCategory("code")]
//	[System.Xml.Serialization.XmlType(AnonymousType = true)]
//	public partial class ApprovalInfoSpRecord
//	{

//		private byte spStatusField;

//		private byte approverAttrField;

//		private ApprovalInfoSpRecordDetails detailsField;

//		/// <remarks/>
//		public byte SpStatus
//		{
//			get
//			{
//				return this.spStatusField;
//			}
//			set
//			{
//				this.spStatusField = value;
//			}
//		}

//		/// <remarks/>
//		public byte ApproverAttr
//		{
//			get
//			{
//				return this.approverAttrField;
//			}
//			set
//			{
//				this.approverAttrField = value;
//			}
//		}

//		/// <remarks/>
//		public ApprovalInfoSpRecordDetails Details
//		{
//			get
//			{
//				return this.detailsField;
//			}
//			set
//			{
//				this.detailsField = value;
//			}
//		}
//	}

//	/// <remarks/>
//	[System.SerializableAttribute()]
//	[System.ComponentModel.DesignerCategory("code")]
//	[System.Xml.Serialization.XmlType(AnonymousType = true)]
//	public partial class ApprovalInfoSpRecordDetails
//	{

//		private ApprovalInfoSpRecordDetailsApprover approverField;

//		private string speechField;

//		private int spStatusField;

//		private uint spTimeField;

//		/// <remarks/>
//		public ApprovalInfoSpRecordDetailsApprover Approver
//		{
//			get
//			{
//				return this.approverField;
//			}
//			set
//			{
//				this.approverField = value;
//			}
//		}

//		/// <remarks/>
//		public string Speech
//		{
//			get
//			{
//				return this.speechField;
//			}
//			set
//			{
//				this.speechField = value;
//			}
//		}

//		/// <remarks/>
//		public int SpStatus
//		{
//			get
//			{
//				return this.spStatusField;
//			}
//			set
//			{
//				this.spStatusField = value;
//			}
//		}

//		/// <remarks/>
//		public uint SpTime
//		{
//			get
//			{
//				return this.spTimeField;
//			}
//			set
//			{
//				this.spTimeField = value;
//			}
//		}
//	}

//	/// <remarks/>
//	[System.SerializableAttribute()]
//	[System.ComponentModel.DesignerCategory("code")]
//	[System.Xml.Serialization.XmlType(AnonymousType = true)]
//	public partial class ApprovalInfoSpRecordDetailsApprover
//	{

//		private string userIdField;

//		/// <remarks/>
//		public string UserId
//		{
//			get
//			{
//				return this.userIdField;
//			}
//			set
//			{
//				this.userIdField = value;
//			}
//		}
//	}

//	/// <remarks/>
//	[System.SerializableAttribute()]
//	[System.ComponentModel.DesignerCategory("code")]
//	[System.Xml.Serialization.XmlType(AnonymousType = true)]
//	public partial class ApprovalInfoNotifyer
//	{

//		private string userIdField;

//		/// <remarks/>
//		public string UserId
//		{
//			get
//			{
//				return this.userIdField;
//			}
//			set
//			{
//				this.userIdField = value;
//			}
//		}
//	}


//}
