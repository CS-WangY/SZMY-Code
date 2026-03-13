using System;
using System.Collections.Generic;
using System.Text;

namespace com.mymooo.workbench.core.Minio
{
    public class MinioConfig
    {

        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string MinioServer { get; set; }
        public string BucketName { get; set; }
        public string MinioImageUrl { get; set; }

    }
}
