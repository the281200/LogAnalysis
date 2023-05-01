using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.Models
{
    public class RequestStatusCodeModel
    {
        public int TotalRequest { set; get; }
        public int FailedRequest { set; get; }
        public int TwoHundredStatusCodes { set; get; }
        public int ThreeHundredStatusCodes { set; get; }
        public int FourHundredStatusCodes { set; get; }
        public int FiveHundredStatusCodes { set; get; }
        public string dataString { get; set; }

    }
}