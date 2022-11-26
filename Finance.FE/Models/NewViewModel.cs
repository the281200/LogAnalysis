using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class NewViewModel : New
    {
        public string CustomerName { get; set; }
        public string ContractName { get; set; }

    }
}