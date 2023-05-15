using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.Models
{
    public class RequestPerformanceViewModel
    {
        public string PercentSuccess { set; get; }
        public string PercentFail { set; get; }
        public string PercentFail4xxStatus { set; get; }
        public string PercentFail5xxStatus { set; get; }
        public string AverageByteSent { set; get; }
        public string AverageByteReceive { set; get; }
        public string AverageTimeTaken { set; get; }
        public string MinByteSent { set; get; }
        public string MinByteReceive { set; get; }
        public string MaxByteSent { set; get; }
        public string MaxByteReceive { set; get; }
        public string MaxTimeTaken { set; get; }
    }
}