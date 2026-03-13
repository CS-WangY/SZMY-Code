using System;
using System.Collections.Generic;

#nullable disable

namespace com.mymooo.workbench.ef.AccountContext
{
    public partial class AssistantUser
    {
        public long Id { get; set; }
        public long AssistantId { get; set; }
        public long UserId { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
