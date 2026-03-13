using System;
using System.Collections.Generic;


namespace com.mymooo.workbench.ef.AccountContext
{
    public partial class Position
    {
        public Position()
        {
            UserPositions = new HashSet<UserPosition>();
        }

        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsForbidden { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string ForbiddenUser { get; set; }
        public DateTime? ForbiddenDate { get; set; }
        public bool? IsAssistant { get; set; }

        public virtual ICollection<UserPosition> UserPositions { get; set; }
    }
}
