using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class PeriodViewModel : Period
    {
        public string IncurredName { get; set; }
    }
}