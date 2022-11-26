using System;
using WebModels;

namespace WEB.Models
{
    public class BuyAndSellBondViewModel : BuyAndSellBond
    {
        public int? Count { get; set; }
        public string CustomerName { get; set; }

        public string Mobile { get; set; }
        public string AssetName { get; set; }

        public long? AccruedValue { get; set; }

        public DateTime? EarningDate { get; set; }

        public DateTime? DisbursementDate { get; set; }

        public string DaysLeft { get; set; }
        public string SourceString { get; set; }

        //[RegularExpression(@"[\d]{1,8}([.][\d]{1,2})?", ErrorMessage = "Vui lòng nhập đúng định dạng")]
        public string InterestRateString { get; set; }
        public int? CalculateInterestDate { get; set; }

        public string StringValue { get; set; }

        public string StringQuantily { get; set; }


    }
}