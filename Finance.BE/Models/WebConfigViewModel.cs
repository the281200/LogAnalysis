using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB.Models
{
    public class WebConfigViewModel
    {
        public string EmailReceive { set; get; }
        public string EmailSend { set; get; }
        public string EmailSendSMTP { set; get; }
        public string EmailSendPort { set; get; }
        public string EmailSendPassword { set; get; }
        public string EmailSendSSL { set; get; }
        public string LicenseKey { set; get; }
        public string Threshold { set; get; }
        public string LinkLog { set; get; }
    }
}