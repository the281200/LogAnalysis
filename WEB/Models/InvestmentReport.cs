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
    }
}