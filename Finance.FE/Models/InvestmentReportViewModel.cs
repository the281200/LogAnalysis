using System.ComponentModel.DataAnnotations;
using WebModels;

namespace WEB.Models
{
    public class InvestmentReportViewModel
    {
        public int STT { get; set; }
        public string InvestmentName { get; set; }
        public string InvestmentType { get; set; }
        public long? Value { get; set; }
        public double? Count { get; set; }
        public double? RealIncome { get; set; }
        public double? Total { get; set; }
    }
}

