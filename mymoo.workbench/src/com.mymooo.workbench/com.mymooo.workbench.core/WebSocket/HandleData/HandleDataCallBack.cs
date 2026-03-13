using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.WebSocket.HandleData
{
    public class HandleDataCallBack<T>
    {
        private int _progress = 0;
        /// <summary>
        /// 进度
        /// </summary>
        public int Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                if (value >= 100)
                {
                    this._progress = 99;
                }
                else
                {
                    this._progress = value;
                }
            }
        }

        /// <summary>
        /// 结果
        /// </summary>
        public List<T> Result { get; set; }

        /// <summary>
        /// 导出文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// code码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; }

        private bool _isEnd = false;
        /// <summary>
        /// 是否结束
        /// </summary>
        public bool IsEnd
        {
            get
            {
                return _isEnd;
            }
            set
            {
                if (value)
                {
                    this._progress = 100;
                }
                this._isEnd = value;
            }
        }
    }
}
