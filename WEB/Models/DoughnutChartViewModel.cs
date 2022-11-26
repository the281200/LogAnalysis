using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.Models
{
    public class DoughnutChartViewModel
    {

        public string label { set; get; }
        public string type { set; get; }
        
        public string backgroundColor { set; get; }
        public List<long> data { set; get; }
    }
}