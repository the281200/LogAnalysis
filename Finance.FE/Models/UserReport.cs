using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.Models
{
    public class UserReport
    {
        public int STT { get; set; }
        public string InvestmentName { get; set; }
        public string InvestmentType { get; set; }
        public long? Value { get; set; }
        public int? Count { get; set; }
        public Double? RealIncome { get; set; }
        public long? Total { get; set; }
    }
}