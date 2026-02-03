using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.BusinessEntity.BusinessFlow;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.PLN.ParamOption;
using Kingdee.K3.Core.MFG;
using Kingdee.K3.MFG.App;
using Kingdee.K3.MFG.Contracts.PLN;
using System.Security.Cryptography;

namespace Kingdee.Mymooo.Business.PlugIn.ReservedManagement
{
	[Description("销售订单预留修改列表插件"), HotUpdate]
	public class ReservedListPlugIn : AbstractListPlugIn
	{

		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			base.AfterBarItemClick(e);

			if (e.BarItemKey.Equals("PENY_ReservedEdit"))
			{
				//string sSql = $"SELECT * FROM dbo.T_MRP_MRPTABLEMANAGE WHERE FISUSED=1";
				//var mrpused = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql).Count();
				//if (mrpused > 0)
				//{
				//    throw new Exception("已有计划运算中,不允许修改预留！");
				//}
				//    PermissionAuthResult iResult = PermissionServiceHelper.FuncPermissionAuth
				//(this.Context, new BusinessObject() { Id = "SAL_SaleOrder" }, "653f662afc5345");
				//    if (!iResult.Passed)
				//    {
				//        this.View.ShowMessage("没有预留修改的综合权限，不能操作！");
				//        return;
				//    }



			}
		}
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			if (e.BarItemKey.Equals("PENY_ReservedEdit", StringComparison.OrdinalIgnoreCase))
			{
				//权限项内码，通过 T_SEC_PermissionItem 权限项表格进行查询。
				string permissionItem = "653f662afc5345";
				PermissionAuthResult permissionAuthResult = this.View.Model.FuncPermissionAuth(
					new string[] { "" }, permissionItem, null, false).FirstOrDefault();
				if (permissionAuthResult != null && !permissionAuthResult.Passed)
				{
					this.View.ShowErrMessage("没有权限");
					e.Cancel = true;
					return;
				}

				DynamicFormShowParameter formParameter = new DynamicFormShowParameter();
				formParameter.FormId = "PENY_ReservedEdit";
				//formParameter.OpenStyle.ShowType = ShowType.Modal;
				formParameter.PageId = Guid.NewGuid().ToString();
				formParameter.ParentPageId = this.View.PageId;
				formParameter.PermissionItemId = PermissionConst.View;
				formParameter.NoBusy = true;
				//FID通过字符串传递过去
				var lv = this.View as IListView;
				if (lv == null)
				{
					return;
				}
				var selectedRows = lv.SelectedRowsInfo;
				if (selectedRows == null || selectedRows.Count == 0)
				{
					this.View.ShowMessage("当前没有行被选中！");
					return;
				}

				long materid = 0;
				long torgid = 0;
				foreach (var row in selectedRows)
				{
					torgid = Convert.ToInt64(row.DataRow["FSupplyTargetOrgId_Id"]);
					MrpCalculatingInfo mrpCalculatingInfo = AppServiceContext.GetService<IMrpLogExtService>().GetMrpCalculatingInfo(this.Context);
					if (mrpCalculatingInfo.DemandOrgIds.Contains(torgid))
					{
						throw new Exception("已有计划运算中,不允许修改预留！");
					}

					var sSql = $"SELECT FDeliQty,FStockOutQty FROM T_SAL_ORDERENTRY_R WHERE FENTRYID={row.EntryPrimaryKeyValue} AND FDeliQty>FStockOutQty";
					var NDqty = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql).Count();
					if (NDqty > 0)
					{
						throw new Exception("已有发货通知单但还未出库时,不允许修改预留！");
					}
					if (materid == 0)
					{
						materid = Convert.ToInt64(row.DataRow["FMaterialId_Id"]);
						torgid = Convert.ToInt64(row.DataRow["FSupplyTargetOrgId_Id"]);
					}
					else
					{
						var mnum2 = Convert.ToInt64(row.DataRow["FMaterialId_Id"]);
						var tnum2 = Convert.ToInt64(row.DataRow["FSupplyTargetOrgId_Id"]);
						if (materid != mnum2 || torgid != tnum2)
						{
							throw new Exception("只允许修改相同供货组织下相同物料的预留关系！");
						}
					}
				}
				var rowIndexs = string.Join(",", selectedRows.Select(o => o.EntryPrimaryKeyValue));
				formParameter.CustomParams.Add("BillID", rowIndexs);
				this.View.ShowForm(formParameter);
			}
		}
	}
}
