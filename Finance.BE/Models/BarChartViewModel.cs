using System.Collections.Generic;

namespace WEB.Models
{
    public class BarChartViewModel
    {
        public string label { set; get; }
        public string type { set; get; }
        public string stack { set; get; }
        public string backgroundColor { set; get; }
        public List<int> data { set; get; }
    }

    public class VerticalBarChartViewModel
    {
        public string axis { set; get; }
        public string fill { set; get; }
        public string borderWidth { set; get; }
        public string[] backgroundColor { set; get; }
        public int[] data { set; get; }
    }
}