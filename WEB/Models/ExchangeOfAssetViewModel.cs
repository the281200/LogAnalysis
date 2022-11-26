using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class ExchangeOfAssetViewModel : ExchangeOfAsset
    {
        public string SellerShow { get; set; }
        public string BuyerShow { get; set; }

        public string AssetShow { get; set; }

        public string InterestFloat { get; set; }

        public string StringValue { get; set; }

        public string StringNumber { get; set; }

        public string StringPrice { get; set; }

    }
}