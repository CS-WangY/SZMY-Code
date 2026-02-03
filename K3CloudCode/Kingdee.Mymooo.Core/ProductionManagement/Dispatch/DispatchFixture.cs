using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.ProductionManagement
{
    public class DispatchFixture
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
        /// （***必要参数***）夹具号
        /// </summary>
        public string fixtureNum { get; set; }
        /// <summary>
        /// （***必要参数***）加工位置
        /// </summary>
        public string blockNum { get; set; }
        /// <summary>
        /// （***必要参数***）物料装夹方向
        /// </summary>
        public string materialOriention { get; set; }
        /// <summary>
        /// （***必要参数***）装夹布局图类型，如：PNG、JPG等图片
        /// </summary>
        public string assyGuidFileType { get; set; }
        /// <summary>
        /// 装夹布局图Url地址
        /// </summary>
        public string assyGuidFileUrl { get; set; }
        /// <summary>
        /// 模型装夹布局图, 3D threejs浏览网址
        /// </summary>
        public string assyModelFileUrl { get; set; }
    }
}
