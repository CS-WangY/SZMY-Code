using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Expressage.ShunFeng
{
    public class ShunFengAccessToken
    {
        public string AccessToken { get; set; }
        public DateTime ExpiresTime { get; set; }
        public string ApiResultCode { get; set; }
        public string ApiErrorMsg { get; set; }
        public string ApiResponseID { get; set; }
        public int ExpiresIn { get; set; }
    }

}
