namespace WEB.Models
{
    public class ReportViewModel
    {
        public string Time { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string DataExport { get; set; }
    }
}