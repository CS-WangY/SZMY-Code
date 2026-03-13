using com.mymooo.mall.core.Utils.JsonConverter;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace com.mymooo.mall.core.Model.Quotation
{
	public  partial class ProductSeriesViewModel
    {

        public long ProductId { get; set; }
        public long TypeId { get; set; }

        [Required(ErrorMessage = "产品型号必填")]
        [StringLength(255, ErrorMessage = "产品型号长度不能超过255个字符")]
        private string _code { get; set; } = string.Empty;
        [JsonConverter(typeof(ProductModelConverter))]
        public string Code
        {
            get
            {
                return MyRegex().Replace(_code, "");
            }
            set
            {
                _code = value;
            }
        }

        [JsonConverter(typeof(NameConverter))]
        [StringLength(255, ErrorMessage = "产品名称长度不能超过255个字符")]
        private string _name = string.Empty;
        public string Name
        {
            get
            {
                return MyRegex().Replace(_name, "");
            }
            set
            {
                _name = value;
            }
        }

        public string ImageUrl { get; set; } = string.Empty;

        public bool Published { get; set; }

        public BrandViewModel? Brand { get; set; }

        [GeneratedRegex(@"\s")]
        private static partial Regex MyRegex();

    }

 
}
