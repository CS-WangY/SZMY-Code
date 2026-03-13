namespace com.mymooo.mall.core.Model.Product
{

	public partial class ReplaceModelModel
    {
		private string replaceModel = string.Empty;

        public string CompanyCode { get; private set; } = string.Empty;
		public string ReplaceModelId { get; private set; } = string.Empty;
        public int Id { get; set; }
        public int RMId { get; set; }
        public int ProductSubclassId { get; set; }
        public string? ProductSubclassName { get; set; }
        public string AntModel { get; set; } = string.Empty;
		public string ProductName { get; set; } = string.Empty;
		public string? ReplaceBrand { get; set; }
		public string ReplaceModel { get => replaceModel; set
            {  
                replaceModel = value; 
				this.ReplaceModelId = value.Replace("-", "").Trim().ToLower();
			}
		}
		public string? DifferenceRemark { get; set; }

        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 数据来源
        /// 0.型号替换导入
        /// 1.供应商价目表
        /// 2.资料工程部门导入
        /// </summary>
        public int DataSource { get; set; }
        public int ProductId { get; set; }

        public string DataType {  get; set; } = string.Empty;
    }

}
