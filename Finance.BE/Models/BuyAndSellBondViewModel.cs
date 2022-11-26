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
        public string AssetCategoryCode { get; set; }

        public long? AccruedValue { get; set; }

        public DateTime? EarningDate { get; set; }

        public DateTime? DisbursementDate { get; set; }

        public string DaysLeft { get; set; }
        public string SourceString { get; set; }

        //[RegularExpression(@"[\d]{1,8}([.][\d]{1,2})?", ErrorMessage = "Vui lòng nhập đúng định dạng")]
        public string InterestRateString { get; set; }
        public string PeriodString { get; set; }
        public string RatesString { get; set; }
        public string PreTaxProfitString { get; set; }
        public string ProfitAfterTaxString { get; set; }
        public string WealthManageBenefitsString { get; set; }
        public string NetInterestString { get; set; }
        public string InterestRateInOutString { get; set; }
        public string PersonalIncomeTaxString { get; set; }
        public string PropertyManagementFeesString { get; set; }
        public string InputInterestRateString { get; set; }
        public string InvestmentReturnString { get; set; }
        public string PersonalIncomeTaxCalculationString { get; set; }
        public string RoundedPurchaseValueString { get; set; }
        public string TotalValueSoldString { get; set; }

        public int? CalculateInterestDate { get; set; }
        
        public string StringValue { get; set; }

        public string StringQuantily { get; set; }

        public string StringUnitPrice { get; set; }

        public string BondsCode { get; set; }

        public string BondsName { get; set; }

        public string PaymentPeriodsContent { get; set; }
        public string DenominationsString { get; set; }
        public string RealValueReceivedString { get; set; }


    }
}