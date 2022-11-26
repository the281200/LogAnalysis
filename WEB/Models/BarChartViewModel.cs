using System.Collections.Generic;

namespace WEB.Models
{
    public class BarChartViewModel
    {
        public string label { set; get; }
        public string type { set; get; }
        public string stack { set; get; }
        public string backgroundColor { set; get; }
        public List<long> data { set; get; }
    }
}