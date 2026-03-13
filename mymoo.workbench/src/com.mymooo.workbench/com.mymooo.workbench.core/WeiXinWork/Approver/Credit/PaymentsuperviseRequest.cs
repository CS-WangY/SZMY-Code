using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.WeiXinWork.Approver.Credit
{
    public class PaymentsuperviseRequest
    {
        public string BillNo { get; set; }
        public DateTime? Date { get; set; }
        public decimal Amount { get; set; }
        public string Urgency { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public string Attachment { get; set; }
        public DateTime? SubmitDate { get; set; }
        public DateTime? CompleteDate { get; set; }
    }
}
