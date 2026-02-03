namespace mymooo.weixinWork.SDK.WeixinWorkMessage.Model.Enum
{
	/// <summary>
	/// 企业微信发送消息类型
	/// </summary>
	public enum SendMessageType
	{
		/// <summary>
		/// 文本
		/// </summary>
		text,

		/// <summary>
		/// 卡片
		/// </summary>
		textcard,

		/// <summary>
		/// 图片
		/// </summary>
		image,

		/// <summary>
		/// 语音
		/// </summary>
		voice,

		/// <summary>
		/// 视频
		/// </summary>
		video,

		/// <summary>
		/// 文件
		/// </summary>
		file,

		/// <summary>
		/// 图文
		/// </summary>
		news,

		/// <summary>
		/// mpnews类型的图文消息，跟普通的图文消息一致，唯一的差异是图文内容存储在企业微信。
		/// 多次发送mpnews，会被认为是不同的图文，阅读、点赞的统计会被分开计算。
		/// </summary>
		mpnews,

		/// <summary>
		/// markdown消息
		/// 标题 （支持1至6级标题，注意#与文字中间要有空格）
		/// # 标题一
		///	## 标题二
		///	### 标题三
		///	#### 标题四
		///	##### 标题五
		///	###### 标题六
		///	
		/// 加粗	**bold**
		/// 链接	[这是一个链接](http://work.weixin.qq.com/api/doc)
		/// 行内代码段（暂不支持跨行）	`code`
		/// 引用	> 引用文字
		/// 字体颜色(只支持3种内置颜色)
		///		<font color="info">绿色</font>
		///		<font color="comment">灰色</font>
		///		<font color="warning">橙红色</font>
		/// </summary>
		markdown
	}
}
