using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.ServicePlugIn.PRD_MO
{
	public class UnAuditValidator : AbstractValidator
	{
		public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
		{
            foreach (var dataEntitie in dataEntities)
            {
                
            }
        }
	}
}
