using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.Models
{
    public class ErrorsRequestStatusCode
    {
        public string FailedRequest { set; get; }
        public string FourHundredStatusCodes { set; get; }
        public string FiveHundredStatusCodes { set; get; }
        public string BadRequestStatusCodes { set; get; }
        public string UnauthorizedStatusCodes { set; get; }
        public string ForbidenStatusCodes { set; get; }
        public string NotFoundStatusCodes { set; get; }
        public string InternalServerErrorStatusCodes { set; get; }
        public string BadGatewayStatusCodes { set; get; }
        public string ServiceUnavailableStatusCodes { set; get; }
        public string GatewayTimeoutStatusCodes { set; get; }

        public string dataString { get; set; }

    }
}