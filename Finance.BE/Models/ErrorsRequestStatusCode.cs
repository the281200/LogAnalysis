using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.Models
{
    public class ErrorsRequestStatusCode
    {
        public int FailedRequest { set; get; }
        public int FourHundredStatusCodes { set; get; }
        public int FiveHundredStatusCodes { set; get; }
        public int BadRequestStatusCodes { set; get; }
        public int UnauthorizedStatusCodes { set; get; }
        public int ForbidenStatusCodes { set; get; }
        public int NotFoundStatusCodes { set; get; }
        public int InternalServerErrorStatusCodes { set; get; }
        public int BadGatewayStatusCodes { set; get; }
        public int ServiceUnavailableStatusCodes { set; get; }
        public int GatewayTimeoutStatusCodes { set; get; }

        public string dataString { get; set; }

    }
}