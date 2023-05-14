using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.Models
{
    public class RequestPerformanceViewModel
    {
        public double PercentSuccess { set; get; }
        public double PercentFail { set; get; }
        public double PercentFail4xxStatus { set; get; }
        public double PercentFail5xxStatus { set; get; }
        public double AverageByteSent { set; get; }
        public double AverageByteReceive { set; get; }
        public double AverageTimeTaken { set; get; }
        public int MinByteSent { set; get; }
        public int MinByteReceive { set; get; }
        public int MaxByteSent { set; get; }
        public int MaxByteReceive { set; get; }
        
    }
}