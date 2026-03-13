using System.ComponentModel;

namespace com.mymooo.mall.core.Model.Quotation
{
    public class SysLogReq
    {

        public long UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Ip {get; set; } = string.Empty;

        public string HostName { get; set; } = string.Empty;


        public string MainParam { get; set; } = string.Empty;


        public string ActionPath { get; set; } = string.Empty;



        public DateTime StartDate { get; set; }


        public DateTime EndDate { get; set; }

        public decimal NTime { get; set; }
        public string FVersion { get; set; } = string.Empty;


    }
}
