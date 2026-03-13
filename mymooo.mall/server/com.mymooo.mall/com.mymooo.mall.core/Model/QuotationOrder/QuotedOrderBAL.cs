using com.mymooo.mall.core.Model.InquiryOrder;

namespace com.mymooo.mall.core.Model.QuotationOrder
{

	public class QuotedOrderBAL
    {
        private decimal? _discount;
        private decimal _tax;
        private decimal _totalWithoutTax;
        private decimal _shipping;
        private decimal _shippingDiscount;
        private decimal _knockOff;

        public long Id { get; set; }

        public  InquiryOrderBAL InquiryOrder { get; set; } = new InquiryOrderBAL();

        public string? FileName { get; set; }

        /// <summary>
        /// 是否运费到付。
        /// </summary>
        public bool FreightToBeCollected { get; set; }

        /// <summary>
        /// 运费。
        /// </summary>
        public decimal Shipping
        {
            get { return _shipping; }
            set { _shipping = Math.Round(value, 2, MidpointRounding.AwayFromZero); }
        }

        /// <summary>
        /// 运费折扣。
        /// </summary>
        public decimal ShippingDiscount
        {
            get { return _shippingDiscount; }
            set { _shippingDiscount = Math.Round(value, 2, MidpointRounding.AwayFromZero); }
        }

        /// <summary>
        /// 税。
        /// </summary>
        public decimal Tax
        {
            get { return _tax; }
            set { _tax = Math.Round(value, 2, MidpointRounding.AwayFromZero); }
        }

        /// <summary>
        /// 未税总计。
        /// </summary>
        public decimal TotalWithoutTax
        {
            get { return _totalWithoutTax; }
            set { _totalWithoutTax = Math.Round(value, 2, MidpointRounding.AwayFromZero); }
        }

        /// <summary>
        /// 折扣。
        /// </summary>
        public decimal? Discount
        {
            get { return _discount; }
            set
            {
                if (value != null && (value < 0 || value > 1))
                {
                    throw new ArgumentOutOfRangeException();
                }
                _discount = value;
            }
        }

        /// <summary>
        /// 减价。
        /// </summary>
        public decimal KnockOff
        {
            get { return _knockOff; }
            set { _knockOff = Math.Round(value, 2, MidpointRounding.AwayFromZero); }
        }

        /// <summary>
        /// 备注。
        /// </summary>
        public string? Remark { get; set; }

        #region Auto calculate property.
        /// <summary>
        /// 含税总计。
        /// </summary>
        public decimal TotalWithTax
        {
            get { return Math.Round(TotalWithoutTax + GetShipping() + Tax - GetAmountOfConcessions(), 2, MidpointRounding.AwayFromZero); }
        }


        //// 优惠类型
        //public PreferentialType PreferentialType
        //{
        //    get
        //    {
        //        if (Discount > 0)
        //        {
        //            return PreferentialType.Discount;
        //        }

        //        if (KnockOff > 0 && (Discount == null || Discount == 0))
        //        {
        //            return PreferentialType.KnockOff;
        //        }

        //        return PreferentialType.None;
        //    }
        //}
        #endregion

        #region Methods
        /// <summary>
        /// 获取优惠金额。
        /// </summary>
        /// <returns></returns>
        public decimal GetAmountOfConcessions()
        {
            if (Discount == null && KnockOff == 0) // 无折扣无优惠金额。
            {
                return 0;
            }

            if (Discount == null) // 有优惠金额。
            {
                return KnockOff;
            }

            var total = TotalWithoutTax + Tax; // 整单折扣。
            return total - total * (decimal)Discount;
        }

        /// <summary>
        /// 获取运费。
        /// </summary>
        /// <returns></returns>
        private decimal GetShipping()
        {
            return Shipping - ShippingDiscount;
        }

        public string TotalWithTaxInChinese(decimal? totalWithTax = null)
        {
			string? value = totalWithTax != null ? totalWithTax.ToString() : TotalWithTax.ToString();
            var isNegative = false; // 是否是负数
            if (value?[..1] == "-")
            {
                // 是负数则先转为正数
                value = value.Trim().Remove(0, 1);
                isNegative = true;
            }

			string? strUpart = null;
			// 保留两位小数 123.489→123.49　　123.4→123.4
			value = Math.Round(double.Parse(value ?? "0"), 2, MidpointRounding.AwayFromZero).ToString();
            if (value.IndexOf(".") > 0)
            {
                if (value.IndexOf(".") == value.Length - 2)
                {
                    value = value + "0";
                }
            }
            else
            {
                value = value + ".00";
            }
			string? strLower = value;
			int iTemp = 1;
			string strUpper = "";
			while (iTemp <= strLower.Length)
            {
                switch (strLower.Substring(strLower.Length - iTemp, 1))
                {
                    case ".":
                        strUpart = "圆";
                        break;
                    case "0":
                        strUpart = "零";
                        break;
                    case "1":
                        strUpart = "壹";
                        break;
                    case "2":
                        strUpart = "贰";
                        break;
                    case "3":
                        strUpart = "叁";
                        break;
                    case "4":
                        strUpart = "肆";
                        break;
                    case "5":
                        strUpart = "伍";
                        break;
                    case "6":
                        strUpart = "陆";
                        break;
                    case "7":
                        strUpart = "柒";
                        break;
                    case "8":
                        strUpart = "捌";
                        break;
                    case "9":
                        strUpart = "玖";
                        break;
                }

                switch (iTemp)
                {
                    case 1:
                        strUpart += "分";
                        break;
                    case 2:
                        strUpart += "角";
                        break;
                    case 3:
						strUpart += "";
                        break;
                    case 4:
                        strUpart += "";
                        break;
                    case 5:
                        strUpart += "拾";
                        break;
                    case 6:
                        strUpart += "佰";
                        break;
                    case 7:
                        strUpart += "仟";
                        break;
                    case 8:
                        strUpart += "万";
                        break;
                    case 9:
                        strUpart = strUpart + "拾";
                        break;
                    case 10:
                        strUpart = strUpart + "佰";
                        break;
                    case 11:
                        strUpart = strUpart + "仟";
                        break;
                    case 12:
                        strUpart = strUpart + "亿";
                        break;
                    case 13:
                        strUpart = strUpart + "拾";
                        break;
                    case 14:
                        strUpart = strUpart + "佰";
                        break;
                    case 15:
                        strUpart = strUpart + "仟";
                        break;
                    case 16:
                        strUpart = strUpart + "万";
                        break;
                    default:
                        strUpart = strUpart + "";
                        break;
                }
                strUpper = strUpart + strUpper;
                iTemp = iTemp + 1;
            }
            strUpper = strUpper.Replace("零拾", "零");
            strUpper = strUpper.Replace("零佰", "零");
            strUpper = strUpper.Replace("零仟", "零");
            strUpper = strUpper.Replace("零零零", "零");
            strUpper = strUpper.Replace("零零", "零");
            strUpper = strUpper.Replace("零角零分", "整");
            strUpper = strUpper.Replace("零分", "整");
            strUpper = strUpper.Replace("零角", "零");
            strUpper = strUpper.Replace("零亿零万零圆", "亿圆");
            strUpper = strUpper.Replace("亿零万零圆", "亿圆");
            strUpper = strUpper.Replace("零亿零万", "亿");
            strUpper = strUpper.Replace("零万零圆", "万圆");
            strUpper = strUpper.Replace("零亿", "亿");
            strUpper = strUpper.Replace("零万", "万");
            strUpper = strUpper.Replace("零圆", "圆");
            strUpper = strUpper.Replace("零零", "零");

            // 对壹圆以下的金额的处理
            if (strUpper.Substring(0, 1) == "圆")
            {
                strUpper = strUpper.Substring(1, strUpper.Length - 1);
            }
            if (strUpper.Substring(0, 1) == "零")
            {
                strUpper = strUpper.Substring(1, strUpper.Length - 1);
            }
            if (strUpper.Substring(0, 1) == "角")
            {
                strUpper = strUpper.Substring(1, strUpper.Length - 1);
            }
            if (strUpper.Substring(0, 1) == "分")
            {
                strUpper = strUpper.Substring(1, strUpper.Length - 1);
            }
            if (strUpper.Substring(0, 1) == "整")
            {
                strUpper = "零圆整";
            }
			string? Chinese = strUpper;
			return isNegative ? "负" + Chinese : Chinese;
        }
        #endregion

        /// <summary>
        /// 报价项列表。
        /// </summary>
       // public virtual IList<QuotedItem> Items { get; set; }
    }
}
