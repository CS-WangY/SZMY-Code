using System;
using System.Collections.Generic;


namespace com.mymooo.workbench.ef.AccountContext
{
    public partial class UserPosition
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long PositionId { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }

        public virtual Position Position { get; set; }
    }
}
