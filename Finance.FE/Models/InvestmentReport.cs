using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class InvestmentReport
    {
        public int STT { get; set; }
        public string InvestmentName { get; set; }
        public string InvestmentType { get; set; }
        public long? Value { get; set; }

        public string StringValue { get; set; }
        public int? Count { get; set; }
        public Double? RealIncome { get; set; }
        public long? Total { get; set; }

        public string InvestmentCode { get; set; }
        public string BondsName { get; set; }
        public string BondsCode { get; set; }

        public string InvestmentPurchaseDate { get; set; }
        public string InvestmentPeriodDate { get; set; }

        public Double? InvestmentInterestRate { get; set; }

        public long? AccruedInterest { get; set; }

        public Double? InputInterestRate { get; set; }

        public Double? InterestRateInOut { get; set; }

        public string AssetId { get; set; }

        public string AssetName { get; set; }

        public int? NumOfContract { get; set; }

        public long? SumOfContract { get; set; }

        public long? SumOfInterest { get; set; }

        public string ContractCodeName { get; set; }
        public string BondsCodeName { get; set; }

        public long? NumOfBonds { get; set; }

        public int? Period { get; set; }

        public string PeriodDate { get; set; }

        public Double? InterestPayPeriod { get; set; }

        public Double? PreTaxProfit { get; set; }

        public Double? ProfitAfterTax { get; set; }

        public Double? SumMoney { get; set; }

        public Double? Rates { get; set; }


        public string NumOfBondString { get; set; }

        public string InvestmentPurchaseDateString { get; set; }
        public string PeriodDateString { get; set; }

        
        public string CalculateInterestDateString { get; set; }

        public Double? RoundedPurchaseValue { get; set; }

        public Double? WealthManageBenefits { get; set; }

        public Double? Sum { get; set; }

        public string Status { get; set; }
    }


}