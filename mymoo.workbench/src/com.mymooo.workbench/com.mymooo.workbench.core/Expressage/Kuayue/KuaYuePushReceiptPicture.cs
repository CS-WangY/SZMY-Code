using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.Kuayue
{
    public class KuaYuePushReceiptPicture
    {
        public string WaybillNumber { get; set; }

        public string FileUrl { get; set; }

        public List<FilePictureInfoRe> filePictureInfoRes { get; set; }
    }

    public class FilePictureInfoRe
    {
        public string ExtendName { get; set; }

        public string Picture { get; set; }

        public string OriginalName { get; set; }

        public string PictureType { get; set; }

        public int Size { get; set; }
    }
}
