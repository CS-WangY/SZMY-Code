using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.Core.PUR;
using Kingdee.Mymooo.Core.BaseManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.Business.PlugIn.SalesManagement
{
    [Description("销售获取BOM模块表单插件"), HotUpdate]
    public class SalesBOMBusiness : AbstractDynamicFormPlugIn
    {
        public static int BillEntityRowIndex;
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            this.Model.SetValue("FSDate", System.DateTime.Now.AddMonths(-1).ToShortDateString());
            this.Model.SetValue("FEDate", Convert.ToDateTime(DateTime.Now.AddDays(1).ToString("D").ToString()).AddSeconds(-1));
            this.View.UpdateView("FSDate");
            this.View.UpdateView("FEDate");
        }
        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);
            string customerNumber = this.Model.DataObject["FCustomerNumber"] == null ? "" : this.Model.DataObject["FCustomerNumber"].ToString();
            string saleBillNo = this.Model.DataObject["FSaleBillNO"] == null ? "" : this.Model.DataObject["FSaleBillNO"].ToString();
            string materialNumber = this.Model.DataObject["FMaterialNumber"] == null ? "" : this.Model.DataObject["FMaterialNumber"].ToString();
            string sDate = this.Model.DataObject["FSDate"] == null ? "" : this.Model.DataObject["FSDate"].ToString();
            string eDate = this.Model.DataObject["FEDate"] == null ? "" : this.Model.DataObject["FEDate"].ToString();
            bool checkBoxBOM = (Boolean)this.Model.DataObject["FCheckBoxBOM"];
            bool checkBoxBill = (Boolean)this.Model.DataObject["FCheckBoxBill"];

            string sSql = $@"select t1.FID,t1.FENTRYID,t1.FSEQ,t2.FBILLNO,t1.FMATERIALID,t1.FQTY,t1.FUNITID,t1.FNOTE,t1.FBOMID,t2.FDATE,t2.FMODIFIERID,t2.FMODIFYDATE,t2.FSaleOrgId,t1.FSupplyTargetOrgId 
                            from T_SAL_ORDERENTRY t1
                            inner join T_BD_MATERIAL tm on t1.FMATERIALID=tm.FMATERIALID
                            inner join T_SAL_ORDER t2 on t1.FID=t2.FID
                            inner join T_BD_CUSTOMER t3 on t2.FCUSTID=t3.FCUSTID
                            where t3.FNUMBER like '%{customerNumber}%' and t2.FBILLNO like '%{saleBillNo}%'
                            and tm.FNUMBER like '%{materialNumber}%'
                            and t2.FDATE between '{sDate}' and '{eDate}'
                            and t1.FSTOCKORGID = {this.Context.CurrentOrganizationInfo.ID}
                            ";
            if (checkBoxBOM)
            {
                sSql = sSql + " and t1.FBOMID=0";
            }
            if (checkBoxBill)
            {
                sSql = sSql + " and t1.FMRPCLOSESTATUS='A'";
            }
            var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
            this.Model.DeleteEntryData("FBillEntity");
            for (int i = 0; i < datas.Count(); i++)
            {
                this.Model.CreateNewEntryRow("FBillEntity");
                this.Model.SetValue("FID", datas[i]["FID"].ToString(), i);
                this.Model.SetValue("FENTRYID", datas[i]["FENTRYID"].ToString(), i);
                this.Model.SetValue("SEQ", i + 1, i);
                this.Model.SetValue("FSEQ", datas[i]["FSEQ"].ToString(), i);
                this.Model.SetValue("FBillNo", datas[i]["FBILLNO"].ToString(), i);
                this.Model.SetValue("FMATERIALID", datas[i]["FMATERIALID"].ToString(), i);
                this.Model.SetValue("FQty", datas[i]["FQTY"].ToString(), i);
                this.Model.SetValue("FUnitID", datas[i]["FUNITID"].ToString(), i);
                this.Model.SetValue("FRemark", datas[i]["FNOTE"].ToString(), i);
                this.Model.SetValue("FBOMID", datas[i]["FBOMID"].ToString(), i);
                this.Model.SetValue("FBillDate", datas[i]["FDATE"].ToString(), i);
                this.Model.SetValue("FModifierId", datas[i]["FMODIFIERID"].ToString(), i);
                this.Model.SetValue("FModifyDate", datas[i]["FMODIFYDATE"].ToString(), i);
                this.Model.SetItemValueByID("FSaleOrgId", datas[i]["FSaleOrgId"].ToString(), i);
                this.Model.SetItemValueByID("FSupplyTargetOrgId", datas[i]["FSupplyTargetOrgId"].ToString(), i);
                this.Model.SetValue("FLinkButton", "导入", i);
                this.Model.SetValue("FLinkGetBOM", "获取", i);
            }
            this.View.UpdateView("FBillEntity");

        }
        public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
        {
            base.EntryButtonCellClick(e);
            //获取动态BOM
            if (e.FieldKey.EqualsIgnoreCase("FLinkGetBOM"))
            {
                DynamicObject selectedEntityObj = (this.View.Model.DataObject["FBillEntity"] as DynamicObjectCollection)[e.Row];
                if (selectedEntityObj != null)
                {
                    var fid = selectedEntityObj["FID"];
                    var entryid = selectedEntityObj["FENTRYID"];
                    //var materialid = datarow.DynamicObject["FMaterialId_Id"].ToString();
                    var material = selectedEntityObj["FMaterialId"] as DynamicObject;
                    DynamicObjectCollection materialbase = material["MaterialBase"] as DynamicObjectCollection;
                    DynamicObject materialunit = materialbase[0]["BaseUnitId"] as DynamicObject;

                    string materialNumber = material["Number"].ToString();
                    long productid = (Int64)material["FProductId"];
                    long supplyTargetOrgid = (Int64)selectedEntityObj["FSupplyTargetOrgId_Id"];
                    long msterid = (Int64)material["msterID"];
                    long saleOrgid = (Int64)selectedEntityObj["FSaleOrgId_Id"];
                    DynamicFormShowParameter param = new DynamicFormShowParameter();
                    param.Resizable = false;
                    param.OpenStyle.ShowType = ShowType.ModalInCurrentForm;
                    param.FormId = "PENY_GetBom";
                    param.CustomComplexParams.Add("MaterialNumber", materialNumber);
                    param.CustomComplexParams.Add("MaterialName", material["Name"].ToString());
                    param.CustomComplexParams.Add("ProductId", productid);
                    param.CustomComplexParams.Add("SupplyTargetOrgid", supplyTargetOrgid);
                    param.CustomComplexParams.Add("SaleOrgid", saleOrgid);
                    param.CustomComplexParams.Add("MsterId", msterid);
                    this.View.ShowForm(param, new Action<FormResult>((result) =>
                    {
                        if (result.ReturnData != null)
                        {
                            ENGBomInfo resdata = result.ReturnData as ENGBomInfo;
                            var sSql = $@"update T_SAL_ORDERENTRY set FBOMID={resdata.Id} where FID={fid} and FEntryID={entryid}";
                            DBServiceHelper.Execute(this.Context, sSql);

                            this.Model.SetValue("FBOMID", resdata.Id, e.Row);
                            this.View.UpdateView("FBOMID");
                            EntryGrid entryGrid = (EntryGrid)this.View.GetControl("FBillEntity");
                            entryGrid.SetFocusRowIndex(BillEntityRowIndex);
                        }
                    }));
                }
            }
            //导入BOM
            if (e.FieldKey.EqualsIgnoreCase("FLinkButton"))
            {
                string sDate = this.Model.DataObject["FSDate"] == null ? "" : this.Model.DataObject["FSDate"].ToString();
                string eDate = this.Model.DataObject["FEDate"] == null ? "" : this.Model.DataObject["FEDate"].ToString();
                DynamicObject selectedEntityObj = (this.View.Model.DataObject["FBillEntity"] as DynamicObjectCollection)[e.Row];
                if (selectedEntityObj != null)
                {
                    var fid = selectedEntityObj["FID"];
                    var entryid = selectedEntityObj["FENTRYID"];
                    //var materialid = datarow.DynamicObject["FMaterialId_Id"].ToString();
                    var material = selectedEntityObj["FMaterialId"] as DynamicObject;
                    //DynamicObjectCollection materialbase = material["MaterialBase"] as DynamicObjectCollection;
                    //DynamicObject materialunit = materialbase[0]["BaseUnitId"] as DynamicObject;

                    string materialNumber = material["Number"].ToString();
                    DynamicFormShowParameter param = new DynamicFormShowParameter();
                    param.Resizable = false;
                    param.OpenStyle.ShowType = ShowType.ModalInCurrentForm;
                    param.FormId = "PENY_ImportExcel";
                    param.CustomComplexParams.Add("MaterialNumber", materialNumber);
                    long bomid = 0;
                    this.View.ShowForm(param, new Action<FormResult>((result) =>
                    {
                        if (result.ReturnData != null)
                        {
                            ENGBomInfo resdata = result.ReturnData as ENGBomInfo;
                            var sSql = $@"/*dialect*/update t1
                                        set t1.FBOMID={resdata.Id}
                                        from T_SAL_ORDERENTRY t1
                                        inner join T_SAL_ORDER t2 on t1.FID=t2.FID
                                        where t1.FMATERIALID={material["Id"]}
                                        and t2.FDATE between '{sDate}' and '{eDate}'
                                        and t1.FSTOCKORGID = {this.Context.CurrentOrganizationInfo.ID}
                                        and t1.FBOMID=0
                                        and t1.FMRPCLOSESTATUS='A'";
                            DBServiceHelper.Execute(this.Context, sSql);
                            bomid = resdata.Id;
                            this.Model.SetValue("FBOMID", bomid, e.Row);
                            this.View.UpdateView("FBOMID");
                            EntryGrid entryGrid = (EntryGrid)this.View.GetControl("FBillEntity");
                            entryGrid.SetFocusRowIndex(BillEntityRowIndex);
                        }
                    }));
                }
            }
        }

        public static int rowcount;
        public override void EntityRowClick(EntityRowClickEventArgs e)
        {
            base.EntityRowClick(e);
            if (e.Key.EqualsIgnoreCase("FBillEntity"))
            {
                BillEntityRowIndex = this.View.Model.GetEntryCurrentRowIndex("FBillEntity");
                //DynamicObject selectedEntityObj = (this.View.Model.DataObject["FBillEntity"] as DynamicObjectCollection)[e.Row];
                DynamicObject bomid = this.Model.GetValue("FBOMID", e.Row) as DynamicObject;
                string bomnumber = "";
                if (bomid != null)
                {
                    bomnumber = bomid["Number"].ToString();
                }
                string sSql = $@"/*dialect*/;with cte as
                                (
	                                --1、定点（Anchor）子查询，用来查询最顶级的产品的BOM的
	                                select 
		                                0 as BOM层次,t1.fid as 最顶级BOM内码
		                                ,t1.FNUMBER as BOM版本,fxwl.FMATERIALID as 物料内码,t3.FUNITID as 单位,fxwl.FNUMBER as 父项物料代码,fxwl_L.FNAME as 父项物料名称,t3.FSEQ as 分录行号
		                                ,t3.FREPLACEGROUP as 项次,CAST(10000+t3.FREPLACEGROUP AS nvarchar) as 项次组合
		                                ,cast(CAST(t1.fid AS nvarchar)+'-'+CAST(10000+t3.FREPLACEGROUP AS nvarchar) as nvarchar(max)) as BOM内码和项次组合
		                                ,t3.FMATERIALID as 子项物料内码,zxwl.FNUMBER as 子项物料代码,zxwl_L.FNAME as 子项物料名称
		                                ,case when FMATERIALTYPE = 1 then '标准件'
				                                when FMATERIALTYPE = 2 then '返还件' 
				                                when FMATERIALTYPE = 3 then '替代件' 
				                                else '未知类型' end as 子项类型
		                                ,t3.FNUMERATOR as 分子,t3.FDENOMINATOR as 分母,t3.FFIXSCRAPQTY as 固定损耗,t3.FSCRAPRATE as 变动损耗,t3.FBOMID,t1.FUSEORGID
		                                ,0 as 是否有子项BOM版本

	                                from dbo.T_ENG_BOM t1
		                                join T_BD_MATERIAL fxwl			--用父项关联物料表
			                                on fxwl.FMATERIALID = t1.FMATERIALID
				                                and t1.FFORBIDSTATUS = 'A'	--只取未禁用状态的BOM
		                                join T_BD_MATERIAL_L fxwl_L		--用父项关联物料多语言表
			                                on fxwl.FMATERIALID = fxwl_l.FMATERIALID and fxwl_L.FLOCALEID =2052
		                                --join T_BD_MATERIALPRODUCE fxwl_P
			                            --    on fxwl_P.FMATERIALID = fxwl.FMATERIALID
		                                join T_ENG_BOMCHILD t3
			                                on t1.fid = t3.FID		
		                                join T_BD_MATERIAL zxwl			--用子项关联物料表
			                                on zxwl.FMATERIALID = t3.FMATERIALID
		                                join T_BD_MATERIAL_L zxwl_L		--用子项关联物料多语言表
			                                on zxwl.FMATERIALID = zxwl_L.FMATERIALID and zxwl_L.FLOCALEID =2052
	                                where 1=1
		                                --and fxwl_P.FISMAINPRD = 1		--物料-生产页签的'可为主产品'属性FISMAINPRD,等于1就意味着可以建立BOM 
		                                and t1.FNUMBER in ('{bomnumber}') --这里可以输入一个产品BOM版本,则只会查询一个产品的BOM多级展开;如果这一句注释掉了,就可以查询全部产品物料的多级展开;下面还有一个控制的条件要同步改,一共两个.

	                                union all

	                                --2、递归子查询，根据定点子查询的查询结果来关联展开它的所有下级的BOM
	                                select  
		                                p.BOM层次+1 as BOM层次,P.最顶级BOM内码 as 最顶级BOM内码
		                                ,t1.FNUMBER as BOM版本,fxwl.FMATERIALID as 物料内码,t3.FUNITID,fxwl.FNUMBER as 父项物料代码,fxwl_L.FNAME as 父项物料名称,t3.FSEQ as 分录行号
		                                ,t3.FREPLACEGROUP as 项次,cast(p.项次组合+'.'+CAST(10000+t3.FREPLACEGROUP AS nvarchar) as nvarchar) as 项次组合
		                                ,cast(p.BOM内码和项次组合 +'.'+ ( CAST(t1.FID AS nvarchar) + '-' +CAST(10000+t3.FREPLACEGROUP AS nvarchar) ) as nvarchar(max))  as BOM内码组合
		                                ,t3.FMATERIALID as 子项物料内码,zxwl.FNUMBER as 子项物料代码,zxwl_L.FNAME as 子项物料名称
		                                ,case when FMATERIALTYPE = 1 then '标准件'
				                                when FMATERIALTYPE = 2 then '返还件' 
				                                when FMATERIALTYPE = 3 then '替代件' 
				                                else '未知类型' end as 子项类型
		                                ,t3.FNUMERATOR as 分子,t3.FDENOMINATOR as 分母,t3.FFIXSCRAPQTY as 固定损耗,t3.FSCRAPRATE as 变动损耗,t3.FBOMID,t1.FUSEORGID
		                                ,case when p.FBOMID = t1.FID then 1 else 0 end as 是否有子项BOM版本
	                                from cte P		--调用递归CTE本身
		                                join dbo.T_ENG_BOM t1
			                                on t1.FMATERIALID = p.子项物料内码
		                                join T_BD_MATERIAL fxwl			--父项关联物料表
			                                on fxwl.FMATERIALID = t1.FMATERIALID
			                                and t1.FFORBIDSTATUS = 'A'
		                                join T_BD_MATERIAL_L fxwl_L		--父项关联物料多语言表
			                                on fxwl.FMATERIALID = fxwl_l.FMATERIALID and fxwl_L.FLOCALEID =2052
		                                join T_ENG_BOMCHILD t3
			                                on t1.fid = t3.FID		
		                                join T_BD_MATERIAL zxwl			--子项关联物料表
			                                on zxwl.FMATERIALID = t3.FMATERIALID
		                                join T_BD_MATERIAL_L zxwl_L		--子项关联物料多语言表
			                                on zxwl.FMATERIALID = zxwl_L.FMATERIALID and zxwl_L.FLOCALEID =2052
                                )
                                --select * from cte		----调试第一段CTE
                                ,cte2_ZuiXinZiXiangBom as		--这个cte2是用来取非0层的子项BOM的最新BOM版本的,然后和0层的父项信息union在一起
                                (
	                                select 
		                                t1.BOM层次 as BOM层级,t1.最顶级BOM内码,t1.BOM版本
		                                ,t1.物料内码,t1.单位,t1.父项物料代码 as 物料代码,t1.父项物料名称 as 物料名称,0 as 分录行号,0 as 项次,t1.项次组合 as 项次组合,BOM内码和项次组合,t1.物料内码 as 子项物料内码,'' as 子项物料代码,'' as 子项物料名称,'最顶层父项' as 子项类型,0 as 分子,0 as 分母,0 as 固定损耗,0 as 变动损耗,0 as BOM内码,t1.FUSEORGID,t1.是否有子项BOM版本
		                                ,dense_rank() over(partition by t1.最顶级BOM内码,t1.父项物料代码 order by t1.BOM版本 desc) as BOM版本号分区
	                                from cte t1
	                                where 1=1 
		                                and t1.BOM层次 = 0 and t1.项次组合 = '10001'		--这里是只显示0层的产品
		                                and t1.BOM版本 in ('{bomnumber}')	--这里可以输入一个产品BOM版本,则只会查询一个产品的BOM多级展开;如果这一句注释掉了,就可以查询全部产品物料的多级展开;上面还有一个控制的条件要同步改,一共两个.

	                                union 
	                                select 
		                                t1.BOM层次+1 as BOM层级,t1.最顶级BOM内码,t1.BOM版本
		                                ,t1.物料内码,t1.单位,t1.子项物料代码 as 物料代码,t1.子项物料名称 as 物料名称,t1.分录行号 as 分录行号,t1.项次 as 项次,t1.项次组合 as 项次组合,BOM内码和项次组合,t1.子项物料内码 as 子项物料内码,t1.子项物料代码 as 子项物料代码,'' as 子项物料名称,t1.子项类型 as 子项类型,t1.分子 as 分子,t1.分母 as 分母,t1.固定损耗 as 固定损耗,t1.变动损耗 as 变动损耗,t1.FBOMID as BOM内码,t1.FUSEORGID,t1.是否有子项BOM版本
		                                ,dense_rank() over(partition by t1.最顶级BOM内码,t1.父项物料代码 order by t1.BOM层次+1,t1.是否有子项BOM版本 desc,t1.BOM版本 desc) as BOM版本号分区	--通过这个字段标识最新版本的BOM，按照父项物料分区之后，把BOM版本降序排列，BOM版本高的排序序号就是1
	                                from cte t1
	                                where 1=1
		                                --and t1.BOM层次+1 <=2	--可以通过BOM层次字段来控制递归循环的次数,如果这里不加控制，那系统默认最多是循环100次
                                )
                                --select * from cte2_ZuiXinZiXiangBom t2		----调试第二段CTE
                                select t2.BOM层级 as BOM层级
		                                ,t2.物料内码,t2.单位,g.FPARENTID as 大类,ms.FMATERIALGROUP as 小类
		                                ,t2.物料代码 as 子项物料代码,t2.物料名称 as 物料名称,t2.分录行号 as 分录行号,t2.项次 as 项次,t2.子项类型 as 子项类型,t2.分子 as 分子,t2.分母 as 分母
		                                ,t2.固定损耗 as 固定损耗,t2.变动损耗 as 变动损耗
		                                ,t2.FUSEORGID,t2.项次组合 as 项次组合,t2.BOM内码和项次组合,t2.BOM内码 as 子项BOM版本内码,t2.BOM版本 as 所属BOM,t2.最顶级BOM内码	--这一行的可以注释掉,只是为了排查SQL问题用的.
                                from cte2_ZuiXinZiXiangBom t2
                                left join T_BD_MATERIAL ms on t2.子项物料内码=ms.FMATERIALID
								left join T_BD_MATERIALGROUP g on ms.FMATERIALGROUP = g.FID
                                where 1=1
	                                and t2.BOM版本号分区 = 1		--通过“BOM版本号分区”标识最新版本的BOM，按照父项物料分区之后，把BOM版本降序排列，BOM版本高的值就是1
	                                and ( (t2.BOM层级 = 0 and t2.项次组合 = '10001' ) or (t2.BOM层级 > 0) )	--这个是为了查询出最终的结果.
	                                and t2.FUSEORGID ={this.Context.CurrentOrganizationInfo.ID}	--蓝海机械账套的‘总装事业部’组织
                                order by t2.BOM内码和项次组合
                            ";
                var datas = DBServiceHelper.ExecuteDynamicObject(this.Context, sSql);
                this.Model.DeleteEntryData("FBOMEntity");

                foreach (var item in datas)
                {
                    rowcount = this.Model.GetEntryRowCount("FBOMEntity");
                    this.Model.CreateNewEntryRow("FBOMEntity");
                    this.Model.SetValue("F_PENY_Text", item["BOM层级"].ToString(), rowcount);
                    this.Model.SetItemValueByNumber("FChildMATERIALID", item["子项物料代码"].ToString(), rowcount);
                    this.Model.SetValue("FChildUnitID", item["单位"].ToString(), rowcount);
                    this.Model.SetValue("FITEMQ", item["项次"].ToString(), rowcount);
                    this.Model.SetValue("FCHILDTYPE", item["子项类型"].ToString(), rowcount);
                    this.Model.SetValue("FChildQTY", item["分子"].ToString(), rowcount);
                    this.Model.SetValue("FMQTY", item["分母"].ToString(), rowcount);
                    this.Model.SetValue("FPARENTSMALLID", item["大类"].ToString(), rowcount);
                    this.Model.SetValue("FSMALLID", item["小类"].ToString(), rowcount);
                }
                this.View.UpdateView("FBOMEntity");
            }
        }
    }
}
