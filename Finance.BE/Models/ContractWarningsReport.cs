
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class ContractWarningsReport
    {
        public int ID { get; set; }
        public int Count { get; set; }
        public long? ContractValue { get; set; }
        public DateTime? ContractDate { get; set; }


        public string ContractCode { get; set; }
        public string CustomerName { get; set; }


        public string BondsCode { get; set; }
       
        
        public string BondsName { get; set; }

        public DateTime? SettlementDate { get; set; }

        public string DaysLeft { get; set; }

        public long? ValueNotGet { get; set; }

        
    }
}