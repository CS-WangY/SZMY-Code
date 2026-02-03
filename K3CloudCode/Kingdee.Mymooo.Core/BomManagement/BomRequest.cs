using System.Collections.Generic;

namespace Kingdee.Mymooo.Core.BomManagement
{
    /// <summary>
    /// SCMAPI返回实体
    /// </summary>
    public class BomApiRequest
    {
        public string code { get; set; }
        public bool isSuccess { get; set; }
        public object data { get; set; }
        public string errorMessage { get; set; }
        public string[] errorMessages { get; set; }
    }
    /// <summary>
    /// 销售订单获取BOM模板实体
    /// </summary>
    public class BomTemplateEntity
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 模板编号
        /// </summary>
        public string TemplateCode { get; set; }
        /// <summary>
        ///模板名称
        /// </summary>
        public string TemplateName { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }
    }
    /// <summary>
    /// 销售订单获取BOM模板头部
    /// </summary>
    public class BomTemplateMaster
    {

        public int pageIndex { get; set; }
        public int pageSize { get; set; }
        public int total { get; set; }
        public int totalPage { get; set; }

        public List<BomTemplateEntity> rows { get; set; }
    }

    /// <summary>
    /// 获取BOM模板参数
    /// </summary>
    public class BomTemplateParameterEntity
    {
        /// <summary>
        /// 模板ID
        /// </summary>
        public long BomTemplateId { get; set; }
        /// <summary>
        /// 参数编号
        /// </summary>
        public string ParamCode { get; set; }
        /// <summary>
        ///参数名称
        /// </summary>
        public string ParamName { get; set; }
        /// <summary>
        /// 参数类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 参数范围
        /// </summary>
        public List<Scope> Scope { get; set; }

    }

    /// <summary>
    /// 参数范围
    /// </summary>
    public class Scope
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// BOM信息
    /// </summary>
    public class BomMaterialEntity
    {
        /// <summary>
        /// 物料编号
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 使用量
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 仓存单位
        /// </summary>
        public string Uom { get; set; }

        /// <summary>
        /// 采购单位
        /// </summary>
        public string PoUom { get; set; }

        /// <summary>
        /// 销售单位
        /// </summary>
        public string SoUom { get; set; }

        /// <summary>
        /// 产品ID
        /// </summary>
        public int ProductId { get; set; } = 0;

        public int SmallClassId { get; set; }
        public string SmallClassName { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public decimal LossRate { get; set; }
        public bool AutoCraft { get; set; }
        /// <summary>
        /// 发料方式 1/空-直接领料0-不发料
        /// </summary>
        public bool IsSueType { get; set; } = true;
        /// <summary>
        /// 是否委外 1-委外0/空-自制
        /// </summary>
        public bool ErpClsID { get; set; } = false;


        public List<BomMaterialEntity> materials { get; set; }
        public bool SendMes { get; set; }
        public string Texture { get; set; }
    }
}
