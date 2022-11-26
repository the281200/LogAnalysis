using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.Models
{
    public class PeriodPostCheck
    {
        public int Id { get; set; }
        public int IncurredId { get; set; }
        public long? Value { get; set; }
        public string Datatable { get; set; }
        public string BuyAndSellBondId { get; set; }
    }
}