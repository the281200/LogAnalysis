using System;
using WebModels;

namespace WEB.Models
{
    public class IncurredPurchaseViewModel : IncurredPurchase
    {
        public int? Count { get; set; }
        public string CustomerName { get; set; }
        public string Mobile { get; set; }
        public string ContractName { get; set; }

        public DateTime? SettlementDate { get; set; }
        public string TypeOfTransaction { get; set; }

        public string DaysLeft { get; set; }

        public string StringAmountOfMoney { get; set; }

    }
}