using System;
using System.Collections.Generic;

#nullable disable

namespace com.mymooo.workbench.ef.AccountContext
{
    public partial class SystemParam
    {
        public long Id { get; set; }
        public long GroupId { get; set; }
        public string SystemParamKey { get; set; }
        public string SystemParamValue { get; set; }
        public string SystemParamDesc { get; set; }
    }
}
