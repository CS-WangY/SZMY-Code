using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Util;

namespace Kingdee.Mymooo.Business.PlugIn.GLVoucher
{
	[Description("移除凭证保存标准校验"), HotUpdate]
	public class RemoveValidator : AbstractOperationServicePlugIn
	{

		public override void OnAddValidators(BOS.Core.DynamicForm.PlugIn.Args.AddValidatorsEventArgs e)
		{
			base.OnAddValidators(e);
			if (!e.Validators.IsEmpty())
			{
				foreach (var entit in e.DataEntities)
				{
					if (Convert.ToString(entit["DocumentStatus"]) == "C")
					{
						foreach (var item in e.Validators.ToList())
						{
							if (item.GetType().FullName.Contains("GLVoucher"))
							{
								e.Validators.Remove(item);
							}
						}
					}

				}
			}
		}
	}
}