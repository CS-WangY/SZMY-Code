using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Minio
{
    public class CloudStockMinioConfig
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string MinioServer { get; set; }
        public string BucketName { get; set; }
        public string MinioImageUrl { get; set; }
    }
}
