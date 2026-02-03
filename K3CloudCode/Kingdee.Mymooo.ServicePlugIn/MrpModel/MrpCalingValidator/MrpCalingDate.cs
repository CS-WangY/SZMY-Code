using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.App.Data;
using Kingdee.K3.MFG.Contracts.PLN;
using Kingdee.K3.MFG.PLN.App.MrpModel;
using Kingdee.BOS.ServiceHelper;
using static Kingdee.K3.Core.MFG.EnumConst.Enums.PLN_MrpModel;
using Kingdee.K3.Core.MFG.PLN.ParamOption;
using Kingdee.K3.MFG.App;

namespace Kingdee.Mymooo.ServicePlugIn.MrpModel.MrpCalingValidator
{
    public class MrpCalingGetSupplyDateStart : AbstractMrpLogicUnit
    {
        public override string Description
        {
            get { return "二开获取供需处理时间"; }
        }
        protected override void OnInitializeLogicUnit()
        {
            base.OnInitializeLogicUnit();
            WriteInfoLog("获取计划运算开始时间", 1, 83);
            foreach (var orgId in this.MrpGlobalDataContext.AllOrgInfo)
            {
                // 获取系统参数的元数据
                var formMetadata = FormMetaDataCache.GetCachedFormMetaData(this.Context, "MFG_PLNParam");
                // 读取系统参数包
                var parameterData = SystemParameterServiceHelper.Load(this.Context, orgId, 0, "MFG_PLNParam");
                // 从系统参数包中获取某一个参数
                if (parameterData != null)
                {
                    if (Convert.ToBoolean(parameterData["FIsMrpCaling"]))
                    {
                        parameterData["FIsMrpRepeat"] = true;
                        var result = SystemParameterServiceHelper.Save(this.Context, formMetadata.BusinessInfo, parameterData, orgId, 0, true);
                        throw new Exception("请等待其他运算完成后重试！");
                    }
                    else
                    {
                        parameterData["FIsMrpRepeat"] = false;
                        parameterData["FIsMrpCaling"] = true;
                        parameterData["FDateStart"] = System.DateTime.Now;
                        var result = SystemParameterServiceHelper.Save(this.Context, formMetadata.BusinessInfo, parameterData, orgId, 0, true);
                    }
                }
            }
        }
        protected override void AfterExecuteLogicUnit()
        {

        }
        public void WriteInfoLog(string msg, int logclass, int logDetailclass)
        {
            base.ExtendServiceProvider.GetService<IMrpLogService>().WriteLog(
                msg, (Enu_MrpLogClass)logclass, (Enu_MrpLogDetailClass)logDetailclass, false);
        }
    }

    public class MrpCalingGetSupplyDateEnd : AbstractMrpLogicUnit
    {
        public override string Description
        {
            get { return "二开获取供需结束时间"; }
        }
        protected override void AfterExecuteLogicUnit()
        {
            WriteInfoLog("获取计划运算结束时间", 1, 83);
            foreach (var orgId in this.MrpGlobalDataContext.AllOrgInfo)
            {
                // 获取系统参数的元数据
                var formMetadata = FormMetaDataCache.GetCachedFormMetaData(this.Context, "MFG_PLNParam");
                // 读取系统参数包
                var parameterData = SystemParameterServiceHelper.Load(this.Context, orgId, 0, "MFG_PLNParam");
                // 从系统参数包中获取某一个参数
                if (parameterData != null)
                {
                    parameterData["FIsMrpRepeat"] = false;
                    parameterData["FIsMrpCaling"] = false;
                    parameterData["FDateEnd"] = System.DateTime.Now;
                    var result = SystemParameterServiceHelper.Save(this.Context, formMetadata.BusinessInfo, parameterData, orgId, 0, true);
                }
            }

        }
        public void WriteInfoLog(string msg, int logclass, int logDetailclass)
        {
            base.ExtendServiceProvider.GetService<IMrpLogService>().WriteLog(
                msg, (Enu_MrpLogClass)logclass, (Enu_MrpLogDetailClass)logDetailclass, false);
        }
    }

    public class MrpCalingSupplyOrgRunMutex : AbstractMrpLogicUnit
    {
        public override string Description
        {
            get { return "二开判断相同组织运算互斥"; }
        }
        protected override void OnInitializeLogicUnit()
        {
            base.OnInitializeLogicUnit();
            WriteInfoLog("判断相同组织运算互斥", 1, 83);

            //MrpCalculatingInfo mrpCalculatingInfo = AppServiceContext.GetService<IMrpLogExtService>().GetMrpCalculatingInfo(this.Context);
            //var ss = this.MrpGlobalDataContext.MrpDataObject[];
            //foreach (var orgId in this.MrpGlobalDataContext.AllOrgInfo)
            //{
            //    if (mrpCalculatingInfo.DemandOrgIds.Contains(orgId))
            //    {
            //        throw new Exception("请等待其他运算完成后重试！");
            //    }
            //}
            //foreach (var orgId in this.MrpGlobalDataContext.AllOrgInfo)
            //{
            //    // 获取系统参数的元数据
            //    var formMetadata = FormMetaDataCache.GetCachedFormMetaData(this.Context, "MFG_PLNParam");
            //    // 读取系统参数包
            //    var parameterData = SystemParameterServiceHelper.Load(this.Context, orgId, 0, "MFG_PLNParam");
            //    // 从系统参数包中获取某一个参数
            //    if (parameterData != null)
            //    {
            //        if (Convert.ToBoolean(parameterData["FIsMrpCaling"]))
            //        {
            //            parameterData["FIsMrpRepeat"] = true;
            //            var result = SystemParameterServiceHelper.Save(this.Context, formMetadata.BusinessInfo, parameterData, orgId, 0, true);
            //            throw new Exception("请等待其他运算完成后重试！");
            //        }
            //        else
            //        {
            //            parameterData["FIsMrpRepeat"] = false;
            //            parameterData["FIsMrpCaling"] = true;
            //            parameterData["FDateStart"] = System.DateTime.Now;
            //            var result = SystemParameterServiceHelper.Save(this.Context, formMetadata.BusinessInfo, parameterData, orgId, 0, true);
            //        }
            //    }
            //}
        }
        protected override void AfterExecuteLogicUnit()
        {

        }
        public void WriteInfoLog(string msg, int logclass, int logDetailclass)
        {
            base.ExtendServiceProvider.GetService<IMrpLogService>().WriteLog(
                msg, (Enu_MrpLogClass)logclass, (Enu_MrpLogDetailClass)logDetailclass, false);
        }
    }
}
