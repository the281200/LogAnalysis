using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.Models
{
    public class RequestStatusCodeModel
    {
        public string TotalRequest { set; get; }
        public string FailedRequest { set; get; }
        public string TwoHundredStatusCodes { set; get; }
        public string ThreeHundredStatusCodes { set; get; }
        public string FourHundredStatusCodes { set; get; }
        public string FiveHundredStatusCodes { set; get; }
        public string dataString { get; set; }

    }
}