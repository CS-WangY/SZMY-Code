using System;
using System.Collections.Generic;

#nullable disable

namespace com.mymooo.workbench.ef.ThirdpartyApplication
{
    public partial class CallbackUrlConfig
    {
        public long Id { get; set; }
        public string Spno { get; set; }
        public string TemplateId { get; set; }
        public long? MessageId { get; set; }
        public string AppId { get; set; }
        public DateTime? CreateTime { get; set; }
        public string CreateUserCode { get; set; }
    }
}
