using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebModels;
using System.IO;
using WebMatrix.WebData;
using System.Web.Security;
using System.Data;
using WEB.Models;
using System.Data.Entity;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Net;
using System.Globalization;
using System.Drawing;

namespace WEB.Areas.ContentType.Controllers
{
    /*[VanTaiAuthorize]*/
    public class MyWalletController : Controller
    {
        WebContext db = new WebContext();

        CultureInfo culture;
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        public ActionResult _PubIndex(int? id)
        {
            ViewBag.WebModule = db.WebModules.FirstOrDefault(x => x.ID == id);
            var user = WebSecurity.GetUserId(User.Identity.Name);
            
            var buyAndSellBonds = db.BuyAndSellBonds.Where(x=> x.CustomerId == user && x.IsActive != false ).ToList();

            var incurredPurchase = db.IncurredPurchases.Where(x => x.CustomerId == user && x.IsActive != false).ToList();
            //sum value user's contract
            var getSumValueContract = buyAndSellBonds.Where(x => x.IsActive == true).Select(x => x.Value).ToList().Sum();

            //sum user's contract interest
            var getSumInterestContract = db.IncurredPurchases.Where(x => x.Customer.UserId == user && (x.TransactionType == (int)(TypeTransaction.GetInterestByTime) || x.TransactionType == (int)(TypeTransaction.GetInterestOntime)) && x.IsActive == true).Select(x => x.AmountOfMoney).ToList().Sum();

            //sum value source
            var getSumSource = db.Periods.Where(x =>x.BuyAndSellBond.CustomerId == user && x.IsActive != false && x.IncurredId != null).Select(x => x.Value).ToList().Sum();

           /* ViewBag.SumValueContract = getSumValueContract + getSumInterestContract - getSumSource;*/

            long? sumValue = getSumValueContract + getSumInterestContract - getSumSource;
            double sumValueDouble = Convert.ToDouble(sumValue);
            var SumValueView = "";
            
                double a = sumValueDouble / 1000000000;
                sumValueDouble = Math.Round(a, 2); 
                SumValueView = sumValueDouble.ToString("G3", CultureInfo.InvariantCulture);
            
            ViewBag.SumValueViewDetail = sumValue.Value.ToString("N0", CultureInfo.InvariantCulture) + " "+"Đồng";
            ViewBag.SumValueView = SumValueView;

            ViewBag.CountTypeOfAssets = buyAndSellBonds.Select(x => x.AssetTypeId).Distinct().ToList().Count();
           
            //Sum interest
            var getSumInterest= incurredPurchase.Where(x => x.TransactionType == 1).Select(x=>x.AmountOfMoney).ToList().Sum();
            double sumInteresDouble = Convert.ToDouble(getSumInterest);
            var SumInterestView = "";

            double b = sumInteresDouble / 1000000000;
            sumInteresDouble = Math.Round(b, 2); ;
            SumInterestView = sumInteresDouble.ToString("G3", CultureInfo.InvariantCulture);

            ViewBag.SumInteresViewDetail = getSumInterest.Value.ToString("N0", CultureInfo.InvariantCulture) + " " + "Đồng";
            ViewBag.SumInteresView = SumInterestView;
            return View();
        }

        [AllowAnonymous]
        public JsonResult GetSumUserValueContract()
        {
            var user = WebSecurity.GetUserId(User.Identity.Name);

            var buyAndSellBonds = db.BuyAndSellBonds.Where(x => x.CustomerId == user).ToList();
            var listKeyValue = new List<ChartViewModel>();
            var yearAgo = DateTime.Now.AddYears(-1);
            var months = MonthsBetween(yearAgo, DateTime.Now);
            long? sumMonthValue = 0;

            //lấy tổng giá trị tài sản của user trc 12 tháng trong chart
            var getSumValueContractAgo = buyAndSellBonds.Where(x => x.IsActive == true && x.PurchaseDate < yearAgo).Select(x => x.Value).ToList().Sum();

            var getSumInterestContractAgo = db.IncurredPurchases.Where(x => x.Customer.UserId == user && (x.TransactionType == (int)(TypeTransaction.GetInterestByTime) || x.TransactionType == (int)(TypeTransaction.GetInterestOntime)) && x.IsActive == true && x.IncurredDate < yearAgo).Select(x => x.AmountOfMoney).ToList().Sum();

            var getSumSourceAgo = db.Periods.Where(x => x.BuyAndSellBond.CustomerId == user && x.IsActive != false && x.IncurredId != null && x.Incurred.IncurredDate < yearAgo).Select(x => x.Value).ToList().Sum();

            var SumValueAgo = getSumValueContractAgo + getSumInterestContractAgo - getSumSourceAgo; //tổng giá trị tài sản của user trc 12 tháng trong chart

            foreach (var item in months)
            {
                var keyValue = new ChartViewModel();

                var valueSumContract = buyAndSellBonds.Where(x => x.IsActive != false && x.PurchaseDate.Value.Month == item.Month && x.PurchaseDate.Value.Year == item.Year).Select(x => x.Value).ToList().Sum();
                var valueSumInterestContract = db.IncurredPurchases.Where(x => x.Customer.UserId == user && (x.TransactionType == (int)(TypeTransaction.GetInterestByTime) || x.TransactionType == (int)(TypeTransaction.GetInterestOntime)) && x.IsActive != false && x.IncurredDate.Value.Month == item.Month && x.IncurredDate.Value.Year == item.Year).Select(x => x.AmountOfMoney).ToList().Sum();
                var getSumSource = db.Periods.Where(x => x.BuyAndSellBond.CustomerId == user && x.IsActive != false && x.IncurredId != null && x.Incurred.IncurredDate.Value.Month == item.Month && x.Incurred.IncurredDate.Value.Year == item.Year).Select(x => x.Value).ToList().Sum();
                var sumValuePerMonth = valueSumContract + valueSumInterestContract - getSumSource;

                sumMonthValue = sumMonthValue + sumValuePerMonth;

                var key = "Tháng " + item.Month + "/" + item.Year;
                keyValue.xAxes = key;
                keyValue.yAxes = sumMonthValue + SumValueAgo;
                listKeyValue.Add(keyValue);
            }
            return Json(listKeyValue, JsonRequestBehavior.AllowGet);

        }

        [AllowAnonymous]
        public JsonResult GetTotalMoneyUser()
        {
            //var listCustomer = db.UserProfiles.Where(x => x.IsActive != false && x.Type == (int)TypeAccount.Customer).ToList();
            var listQueryValue = new List<DoughnutChartQuery>();
            var user = WebSecurity.GetUserId(User.Identity.Name);
            
            var buyAndSellBonds = db.BuyAndSellBonds.Where(x => x.CustomerId == user && x.IsActive != false).ToList();
            var listUserAssetType = buyAndSellBonds.Select(x => x.AssetTypeId).ToList();
            var listAccetType = db.TypeOfAssets.Where(x => (listUserAssetType.Contains(x.ID)) && x.IsActive != false).ToList();
            var listColor = new List<string>
                { "#C0392B","#8E44AD", "#2980B9", "#16A085", "#27AE60" , "#F1C40F", "#F39C12",  "#BA4A00", "#95A5A6", "#566573", "#4FC3F7", "#a1eb34", "#34e5eb", "#34a2eb", "#eb3474", "#eb8034", "#b134eb", "#eb4f34"};


            foreach (var type in listAccetType)
            {
                var dataQuery = new DoughnutChartQuery();

                //sum and get list Id all value with userId and type
                var listBuyAndSellBonds = buyAndSellBonds.Where(x => x.IsActive != false && x.TypeOfAsset == type).ToList();
                var listIdBuyAndSellBond = listBuyAndSellBonds.Select(x => x.Id).ToList();
                var valueSumContract = listBuyAndSellBonds.Sum(x => x.Value);

                //Sum value Interest Contract with TransactionType is GetInterest contain list id in listBuyAndSellBonds
                var valueSumInterestContract = db.IncurredPurchases.Where(x => x.Customer.UserId == user && (x.TransactionType == (int)(TypeTransaction.GetInterestByTime) || x.TransactionType == (int)(TypeTransaction.GetInterestOntime)) && x.IsActive != false
                && listIdBuyAndSellBond.Contains(x.BuyAndSellBondId ?? default)).Select(x => x.AmountOfMoney).ToList().Sum();

                //subtract using source by listBuyAndSellBonds
                var getSumSource = db.Periods.Where(x => x.BuyAndSellBond.CustomerId == user && x.IsActive != false && x.IncurredId != null && listIdBuyAndSellBond.Contains((x.Incurred.BuyAndSellBondId ?? default))).Select(x => x.Value).ToList().Sum();

                var sumValue = valueSumContract + valueSumInterestContract - getSumSource;
                var random = new Random();

                dataQuery.xAxes = type.AssetName; //Type
                dataQuery.yAxes = sumValue; //Value
                dataQuery.zAxes = getRandColor(random); //Color
                listQueryValue.Add(dataQuery);
            }
            for (var i = 0; i < listQueryValue.Count(); i++)
            {
                listQueryValue[i].zAxes = listColor[i];
            }

            return Json(listQueryValue, JsonRequestBehavior.AllowGet);

        }

        [AllowAnonymous]
        public JsonResult GetTotalMoneyUserDetail(string label)
        {
            //var listCustomer = db.UserProfiles.Where(x => x.IsActive != false && x.Type == (int)TypeAccount.Customer).ToList();
            var listQueryValue = new List<DoughnutChartQuery>();
            var user = WebSecurity.GetUserId(User.Identity.Name);
         
            var buyAndSellBonds = db.BuyAndSellBonds.Where(x => x.CustomerId == user && x.IsActive != false).ToList();
            var listUserAssetType = buyAndSellBonds.Select(x => x.AssetCategorysId).ToList();
            var listAccetType = db.AssetCategorys.Where(x => (listUserAssetType.Contains(x.Id)) && x.IsActive != false).ToList(); //get asset category
            var listColor = new List<string>
                { "#C0392B","#8E44AD", "#2980B9", "#16A085", "#27AE60" , "#F1C40F", "#F39C12",  "#BA4A00", "#95A5A6", "#566573", "#4FC3F7", "#a1eb34", "#34e5eb", "#34a2eb", "#eb3474", "#eb8034", "#b134eb", "#eb4f34"};


            foreach (var type in listAccetType)
            {
                var dataQuery = new DoughnutChartQuery();

                //sum and get list Id all value with userId and type
                var listBuyAndSellBonds = buyAndSellBonds.Where(x => x.IsActive != false && x.AssetCategorys == type && x.TypeOfAsset.AssetName == label).ToList();
                var listIdBuyAndSellBond = listBuyAndSellBonds.Select(x => x.Id).ToList();
                var valueSumContract = listBuyAndSellBonds.Sum(x => x.Value);

                //Sum value Interest Contract with TransactionType is GetInterest contain list id in listBuyAndSellBonds
                var valueSumInterestContract = db.IncurredPurchases.Where(x => x.Customer.UserId == user && (x.TransactionType == (int)(TypeTransaction.GetInterestByTime) || x.TransactionType == (int)(TypeTransaction.GetInterestOntime)) && x.IsActive != false
                && listIdBuyAndSellBond.Contains(x.BuyAndSellBondId ?? default)).Select(x => x.AmountOfMoney).ToList().Sum();

                //subtract using source by listBuyAndSellBonds
                var getSumSource = db.Periods.Where(x => x.BuyAndSellBond.CustomerId == user && x.IsActive != false && x.IncurredId != null && listIdBuyAndSellBond.Contains((x.Incurred.BuyAndSellBondId ?? default))).Select(x => x.Value).ToList().Sum();

                var sumValue = valueSumContract + valueSumInterestContract - getSumSource;
                var random = new Random();

                dataQuery.xAxes = type.Name; //Type
                dataQuery.yAxes = sumValue; //Value
                dataQuery.zAxes = getRandColor(random); //Color
                listQueryValue.Add(dataQuery);
            }
            for (var i = 0; i < listQueryValue.Count(); i++)
            {
                listQueryValue[i].zAxes = listColor[i];
            }

            return Json(listQueryValue, JsonRequestBehavior.AllowGet);

        }

        private string getRandColor(Random random)
        {
            Color randomColor = Color.FromArgb(random.Next(1, 256), random.Next(1, 256), random.Next(1, 256));
            var color = "#" + randomColor.Name;
            return color;
        }

        public static IEnumerable<(int Month, int Year)> MonthsBetween(
        DateTime startDate,
        DateTime endDate)
        {
            DateTime iterator;
            DateTime limit;

            if (endDate > startDate)
            {
                iterator = new DateTime(startDate.Year, startDate.Month, 1);
                limit = endDate;
            }
            else
            {
                iterator = new DateTime(endDate.Year, endDate.Month, 1);
                limit = startDate;
            }

            var dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
            while (iterator <= limit)
            {
                yield return (
                    iterator.Month,
                    iterator.Year
                );

                iterator = iterator.AddMonths(1);
            }
        }
        static int GetWeekNumberOfMonth(DateTime date)
        {
            date = date.Date;
            DateTime firstMonthDay = new DateTime(date.Year, date.Month, 1);
            DateTime firstMonthMonday = firstMonthDay.AddDays((DayOfWeek.Monday + 7 - firstMonthDay.DayOfWeek) % 7);
            if (firstMonthMonday > date)
            {
                firstMonthDay = firstMonthDay.AddMonths(-1);
                firstMonthMonday = firstMonthDay.AddDays((DayOfWeek.Monday + 7 - firstMonthDay.DayOfWeek) % 7);
            }
            return (date - firstMonthMonday).Days / 7 + 1;
        }
    }
}