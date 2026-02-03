using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    public class DispatchProcess
    {
        /// <summary>
        /// ***必要参数***）图号，对于3D，用零件名或文件名，不宜过长，否则显示会截断
        /// 且一个订单内不应重复
        /// </summary>
        public string drawNum { get; set; }

        /// <summary>
        /// 图号版本 ，如果不输入，默认为A
        /// </summary>
        public string drawNumVersion { get; set; } = "A";

        /// <summary>
        /// 图纸难度等级，可输入：10、20、30，其中：10 - A等级，最难图纸；20 - B等级，普通难度图纸；30 - C等级，简单图纸
        /// </summary>
        public int drawLevel { get; set; } = 20;

        /// <summary>
        /// 工艺图纸Url，只支持PDF格式
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// 工艺模型图的Url,适用于3D模型的threejs 3D视图地址
        /// </summary>
        public string modelUrl { get; set; }

        /// <summary>
        /// 模型图的缩略图Url
        /// </summary>
        public string modelPreviewUrl { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remarks { get; set; }

        /// <summary>
        /// 工艺版次 默认1，每升级一次，版次加1
        /// </summary>
        public int smallVersion { get; set; } = 1;

        /// <summary>
        /// 工序
        /// </summary>
        public List<DispatchProcessLine> processLines { get; set; } = new List<DispatchProcessLine>();

        /// <summary>
        /// 夹具
        /// </summary>
        public List<DispatchFixture> fixtures { get; set; } = new List<DispatchFixture>();

        /// <summary>
        /// 数控编程代码
        /// </summary>
        public List<DispatchProgramming> programmings { get; set; } = new List<DispatchProgramming>();

        /// <summary>
        /// 刀具
        /// </summary>
        public List<DispatchTool> toolings { get; set; } = new List<DispatchTool>();

    }
}
