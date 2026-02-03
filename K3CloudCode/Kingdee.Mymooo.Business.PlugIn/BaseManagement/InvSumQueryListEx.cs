using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.Stock.Business.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.BaseManagement
{
    [Description("即时库存扩展采购价"), HotUpdate]
    public class InvSumQueryListEx : InvSumQueryList
    {
        public override void PrepareFilterParameter(FilterArgs e)
        {
            base.PrepareFilterParameter(e);
            var transID = this.TransactionID;
            string sSql = $@"/*dialect*/UPDATE T_STK_INVSUMQUERY 
							SET FPENYCGCB = T.FPRICE 
							--SELECT *
							FROM
								T_STK_INVSUMQUERY S
								LEFT JOIN (
								SELECT
									T.FID,
								CASE
			
										WHEN _K3.FPRICE IS NULL THEN
										ISNULL( _erp.UNIT_PRICE, 0 ) ELSE _K3.FPRICE 
									END AS FPRICE 
								FROM
									T_STK_INVSUMQUERY T
									LEFT JOIN (
									SELECT
										* 
									FROM
										(
										SELECT
											m.FMASTERID,
											t1.FMATERIALID,
											t2.FDATE,
											tf.FPRICE,
											t2.FPURCHASEORGID,
											row_number ( ) OVER ( partition BY t1.FMATERIALID ORDER BY t2.FDATE DESC ) AS rn 
										FROM
											t_PUR_POOrderEntry t1
											LEFT JOIN T_BD_MATERIAL m ON t1.FMATERIALID= m.FMATERIALID
											LEFT JOIN T_PUR_POORDERENTRY_F tf ON t1.FENTRYID= tf.FENTRYID
											LEFT JOIN t_PUR_POOrder t2 ON t1.FID= t2.FID 
										) t1 
									WHERE
										t1.rn= 1 
									) _K3 ON T.FSTOCKORGID= _K3.FPURCHASEORGID 
									AND T.FMATERIALID= _K3.FMASTERID
									LEFT JOIN (
									SELECT
										* 
									FROM
										(
										SELECT
											m.FMASTERID,
											t1.ITEM_NO,
											t2.PO_DATE,
											t1.UNIT_PRICE,
											t2.COMP_CODE,
											g.FORGID,
											row_number ( ) OVER ( partition BY t1.ITEM_NO ORDER BY t2.PO_DATE DESC ) AS rn 
										FROM
											M_POD_DET t1
											LEFT JOIN M_PO_MSTR t2 ON t1.New_PO_NO= t2.New_PO_NO
											INNER JOIN T_BD_MATERIAL m ON t1.ITEM_NO= m.FNUMBER
											INNER JOIN T_ORG_ORGANIZATIONS g ON g.FNUMBER= t2.COMP_CODE 
										WHERE
											1 = 1 
										) t2 
									WHERE
										t2.rn= 1 
									) _erp ON T.FSTOCKORGID= _erp.FORGID 
									AND T.FMATERIALID= _erp.FMASTERID 
								) T ON S.FID= T.FID 
							WHERE
								S.FTRANSID= '{transID}'";
            DBServiceHelper.Execute(this.Context, sSql);
        }
    }
}
