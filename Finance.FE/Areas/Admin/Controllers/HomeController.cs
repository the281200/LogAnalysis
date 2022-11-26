using Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WEB.Models;
using WebMatrix.WebData;
using WebModels;

namespace WEB.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        WebModels.WebContext db = new WebModels.WebContext();
        //
        // GET: /Admin/Home/
        CultureInfo culture;
       
        public ActionResult Index()
        {
            if (WebSecurity.CurrentUserId != 0 && WebSecurity.CurrentUserId != null)
            {
                var userInfo = db.UserProfiles.Find(WebSecurity.CurrentUserId);
                if (userInfo != null && (userInfo.Type == null || (userInfo.Type != null && userInfo.Type == (int)TypeAccount.Customer)))
                {
                    FormsAuthentication.SignOut();
                    return RedirectToAction("Login", "Account", new { area = "Admin" });
                }
            }

            List<string> role = Roles.GetRolesForUser(WebHelpers.UserInfoHelper.GetUserData().UserName).ToList();


            var buyAndSellBonds = db.BuyAndSellBonds.ToList();
            var getNumInvestors = buyAndSellBonds.Where(x => x.IsActive != false).Select(x => x.CustomerId).Distinct().ToList().Count();

            var getNumContract = buyAndSellBonds.Where(x => x.IsActive != false).Select(x => x.Id).ToList().Count();

            var getSumValueContract = buyAndSellBonds.Where(x => x.IsActive != false).Select(x => x.Value).ToList().Sum();

            var getSumInterestContract = db.IncurredPurchases.Where(x => x.TransactionType == (int)(TypeTransaction.GetInterest) && x.IsActive != false).Select(x => x.AmountOfMoney).ToList().Sum();

            var getSumSource = db.Periods.Where(x => x.IsActive != false && x.IncurredId != null && x.BuyAndSellBondId != 0).Select(x => x.Value).ToList().Sum();

            ViewBag.NumInvestors = getNumInvestors;
            long? sumValue = getSumValueContract + getSumInterestContract - getSumSource;
            double sumValueDouble = Convert.ToDouble(sumValue);
            double sumValueDetail = Convert.ToDouble(sumValue);
            var SumValueView = "";
            if (sumValueDouble >= 1000000000)
            {
                double a = sumValueDouble / 1000000000;
                sumValueDouble = Math.Round(a, 2); ;
                SumValueView = sumValueDouble.ToString("G3", CultureInfo.InvariantCulture) +" "+"Tỷ đồng";
            }
            else
            {
                SumValueView = sumValueDouble.ToString("N0", CultureInfo.InvariantCulture) + " "+ "đ";
            }
            ViewBag.SumValueDetail = sumValueDetail.ToString("N0", CultureInfo.InvariantCulture) + " "+ "Đồng"; 
            ViewBag.SumValueView = SumValueView;
            ViewBag.NumContract = getNumContract;
            return View();
        }
        public ActionResult Loading()
        {
            return View();
        }

        public ActionResult _ContentLeft()
        {
            var userId = WebHelpers.UserInfoHelper.GetUserData().UserId;
            // join webmodule
            var result = from wm in db.WebModules
                         join awmr in db.AccessWebModuleRoles on wm.ID equals awmr.WebModuleID
                         join uir in db.UserInRoles on awmr.RoleId equals uir.RoleId
                         where uir.UserId == userId && ((awmr.Add.HasValue && awmr.Add.Value) || (awmr.Edit.HasValue && awmr.Edit.Value)
                         || (awmr.Delete.HasValue && awmr.Delete.Value) || (awmr.View.HasValue && awmr.View.Value))
                         select wm;
            var test = result.ToList().OrderBy(x => x.Order);
            //var nav = db.AdminSites.Where(x => x.ParentID == null).OrderBy(x => x.Order).ToList();
            return PartialView(test);
        }

        [AllowAnonymous]
        public JsonResult GetPackageBuy()
        {
            var buyAndSellBonds = db.BuyAndSellBonds.ToList();
            var listKeyValue = new List<ChartViewModel>();
            var yearAgo = DateTime.Now.AddYears(-1);
            var months = MonthsBetween(yearAgo, DateTime.Now);
            long? sumMonthValue = 0;

            //lấy tổng giá trị tài sản trc 12 tháng trong chart
            var getSumValueContractAgo = buyAndSellBonds.Where(x => x.IsActive == true && x.PurchaseDate < yearAgo).Select(x => x.Value).ToList().Sum();

            var getSumInterestContractAgo = db.IncurredPurchases.Where(x => x.TransactionType == (int)(TypeTransaction.GetInterest) && x.IsActive == true && x.IncurredDate < yearAgo).Select(x => x.AmountOfMoney).ToList().Sum();

            var getSumSourceAgo = db.Periods.Where(x => x.IsActive != false && x.IncurredId != null && x.Incurred.IncurredDate < yearAgo && x.BuyAndSellBondId != 0).Select(x => x.Value).ToList().Sum();

            var SumValueAgo = getSumValueContractAgo + getSumInterestContractAgo - getSumSourceAgo; //tổng giá trị tài sản trc 12 tháng trong chart

            foreach (var item in months)
            {
                var keyValue = new ChartViewModel();

                var valueSumContract = buyAndSellBonds.Where(x => x.IsActive != false && x.PurchaseDate.Value.Month == item.Month && x.PurchaseDate.Value.Year == item.Year).Select(x => x.Value).ToList().Sum();
                var valueSumInterestContract = db.IncurredPurchases.Where(x => x.TransactionType == (int)(TypeTransaction.GetInterest) && x.IsActive != false && x.IncurredDate.Value.Month == item.Month && x.IncurredDate.Value.Year == item.Year).Select(x => x.AmountOfMoney).ToList().Sum();
                var getSumSource = db.Periods.Where(x => x.IsActive != false && x.IncurredId != null && x.Incurred.IncurredDate.Value.Month == item.Month && x.Incurred.IncurredDate.Value.Year == item.Year).Select(x => x.Value).ToList().Sum();
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
            var listCustomer = db.UserProfiles.Where(x => x.IsActive != false && x.Type == (int)TypeAccount.Customer).ToList();
            var listQueryValue = new List<QueryBarChartModel>();

            var listAccetType = db.TypeOfAssets.Where(x => x.IsActive != false).ToList();
            var buyAndSellBonds = db.BuyAndSellBonds.ToList();

            var listColor = new List<string>
                { "#C0392B","#8E44AD", "#2980B9", "#16A085", "#27AE60" , "#F1C40F", "#F39C12", "#F39C12", "#BA4A00", "#95A5A6", "#566573", "#4FC3F7"};


            foreach (var user in listCustomer)
            {
                foreach (var type in listAccetType)
                {
                    var dataQuery = new QueryBarChartModel();

                    //sum and get list Id all value with userId and type
                    var listBuyAndSellBonds = buyAndSellBonds.Where(x => x.IsActive != false && x.CustomerId == user.UserId && x.TypeOfAsset == type).ToList();
                    var listIdBuyAndSellBond = listBuyAndSellBonds.Select(x => x.Id).ToList();
                    var valueSumContract = listBuyAndSellBonds.Sum(x => x.Value);

                    //Sum value Interest Contract with TransactionType is GetInterest contain list id in listBuyAndSellBonds
                    var valueSumInterestContract = db.IncurredPurchases.Where(x => x.TransactionType == (int)(TypeTransaction.GetInterest) && x.IsActive != false
                    && listIdBuyAndSellBond.Contains(x.BuyAndSellBondId ?? default)).Select(x => x.AmountOfMoney).ToList().Sum();

                    //subtract using source by listBuyAndSellBonds
                    var getSumSource = db.Periods.Where(x => x.IsActive != false && x.IncurredId != null && listIdBuyAndSellBond.Contains((x.Incurred.BuyAndSellBondId ?? default))).Select(x => x.Value).ToList().Sum();

                    var sumValue = valueSumContract + valueSumInterestContract - getSumSource;

                    dataQuery.UserId = user.UserId;
                    dataQuery.UserName = user.FullName;
                    dataQuery.Type = type.AssetName;
                    dataQuery.Value = sumValue;
                    listQueryValue.Add(dataQuery);
                }
            }

            var listUserValueMax = new List<SelectValueMax>();
            foreach (var item in listQueryValue.GroupBy(x => x.UserId))
            {
                var sumTypeOfValue = item.Sum(x => x.Value);
                listUserValueMax.Add(new SelectValueMax
                {
                    UserId = item.Key,
                    Value = sumTypeOfValue,
                    UserName = item.First().UserName
                });
            }

            var listUserResult = listUserValueMax.OrderByDescending(x => x.Value).Take(10);

            var xAxes = listUserResult.Select(x => x.UserName).ToArray();

            var listDataExport = new List<BarChartViewModel>();

            foreach (var item in listAccetType)
            {
                var barChartViewModel = new BarChartViewModel();
                barChartViewModel.label = item.AssetName;
                barChartViewModel.type = "bar";
                barChartViewModel.stack = "base";

                var random = new Random();
                barChartViewModel.backgroundColor = getRandColor(random);

                var listValue = new List<long>();

                foreach (var user in listUserResult)
                {
                    var value = listQueryValue.Where(x => x.UserId == user.UserId && x.Type == item.AssetName).Select(x => x.Value).FirstOrDefault() ?? default;
                    listValue.Add(value);
                }

                barChartViewModel.data = listValue;
                listDataExport.Add(barChartViewModel);
            }


            for (var i = 0; i < listDataExport.Count(); i++){
                listDataExport[i].backgroundColor = listColor[i];
            }

            var stringJsonYAxes = listDataExport.ToArray();

            return Json(new { xAxes = xAxes, yAxes = stringJsonYAxes }, JsonRequestBehavior.AllowGet);

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
