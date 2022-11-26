using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class ConfigSendMailInfo
    {
        
        public string Host { get; set; }
        public string Port { get; set; }
        public string SendFrom { get; set; }
        public string EmailPass { get; set; }
        public string Ssl { get; set; }
        public string EmailTo { get; set; }

    }
}