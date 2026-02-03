using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    public class DispatchProgramming
    {
        /// <summary>
        /// （***必要参数***）加工顺序，一般步长10，比如：0010、0020
        /// </summary>
        public string operationNum { get; set; }
        /// <summary>
        /// ***必要参数***）工序代码，比如：IMCNC1
        /// </summary>
        public string processId { get; set; }
        /// <summary>
        /// （***必要参数***）机床类型
        /// </summary>
        public string machineType { get; set; }
        /// <summary>
        /// NC程式X向补偿
        /// </summary>
        public double ncCompensateX { get; set; }
        /// <summary>
        /// NC程式Y向补偿
        /// </summary>
        public double ncCompensateY { get; set; }
        /// <summary>
        /// NC程式Z向补偿
        /// </summary>
        public double ncCompensateZ { get; set; }
        /// <summary>
        /// （***必要参数***）NC加工程式号（一般为4位），比如：1021
        /// </summary>
        public string ncProgrammingNum { get; set; }
        /// <summary>
        /// 清洗频率, 表示加工多少件进行一次清洗
        /// </summary>
        public int cleanFrequency { get; set; }
        /// <summary>
        /// 测量频率: 表示加工多少件进行一次测量
        /// </summary>
        public int measureFrequency { get; set; }
        /// <summary>
        /// 测量程式格式，比如：cmm
        /// </summary>
        public string measureFileType { get; set; }
        /// <summary>
        /// 测量程式Url地址
        /// </summary>
        public string measureFileUrl { get; set; }
        /// <summary>
        /// NC程式文件类型（新增）
        /// </summary>
        public string ncFileType { get; set; }
        /// <summary>
        /// NC程式文件地址（新增）
        /// </summary>
        public string ncFileUrl { get; set; }
    }
}
