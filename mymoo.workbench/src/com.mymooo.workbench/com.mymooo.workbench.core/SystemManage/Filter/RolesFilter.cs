using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.SystemManage.Filter
{
    public class RolesFilter
    {
    }

    public class ApprovalFilter
    {
    }

    public class MessageFilter
    {
        public string Spno { get; set; }
        public bool? IsComplete { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? CompleteDate { get; set; }

        //申请单状态：1-审批中；2-已通过；3-已驳回；4-已撤销；6-通过后撤销；7-已删除；10-已支付
        public int? Status { get; set; }

        public string detailName { get; set; }
    }

}
