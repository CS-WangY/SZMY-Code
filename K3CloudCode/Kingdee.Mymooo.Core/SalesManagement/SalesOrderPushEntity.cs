using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Core.SalesManagement
{
    public class SalesOrderPushEntity
    {
        public long FID { get; set; }
        public long FEntryID { get; set; }
        public long FSalOrgID { get; set; }
    }
    public class SalesOrderPushReceiveBillEntity
    {
        public long FID { get; set; }
        public long FEntryID { get; set; }
        public List<ReplaceVal> rval { get; set; }
    }
    public class ReplaceVal
    {
        public EntityType EntityType { get; set; }
        public string EntityKey { get; set; }
        public string EntityValue { get; set; }
        public TargetValueType valueType { get; set; }
        public object Val { get; set; }
    }
    public enum EntityType { BillHand, BillEntry }
    public enum TargetValueType { Value, Object }
}
