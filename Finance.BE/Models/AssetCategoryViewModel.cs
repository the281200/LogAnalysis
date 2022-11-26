using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class AssetCategoryViewModel : AssetCategory
    {
        public string InterestFloat { get; set; }

        public string StringPrice { get; set; }

        public string PeriodFloat { get; set; }

    }
}