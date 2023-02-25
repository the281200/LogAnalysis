using Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
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
                if (userInfo != null && (userInfo.Type == null || (userInfo.Type != null && userInfo.Type != (int)TypeAccount.Admin)))
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

            var getSumInterestContract = db.IncurredPurchases.Where(x => (x.TransactionType == (int)(TypeTransaction.GetInterestByTime) || x.TransactionType == (int)(TypeTransaction.GetInterestOntime)) && x.IsActive != false).Select(x => x.AmountOfMoney).ToList().Sum();

            var getSumSource = db.Periods.Where(x => x.IsActive != false && x.IncurredId != null && x.Incurred.IsActive != false && x.BuyAndSellBondId != 0).Select(x => x.Value).ToList().Sum();

            ViewBag.NumInvestors = getNumInvestors;
            long? sumValue = getSumValueContract + getSumInterestContract - getSumSource;
            double sumValueDouble = Convert.ToDouble(sumValue);
            double sumValueDetail = Convert.ToDouble(sumValue);
            var SumValueView = "";
            if (sumValueDouble >= 1000000000 && sumValueDouble <1000000000000)
            {
                double a = sumValueDouble / 1000000000;
                sumValueDouble = Math.Round(a, 2); ;
                SumValueView = sumValueDouble.ToString("G3", CultureInfo.InvariantCulture) +" "+"Tỷ đồng";
            }
            else if (sumValueDouble >= 1000000000000)
            {
                double a = sumValueDouble / 1000000000000;
                sumValueDouble = Math.Round(a, 2); ;
                SumValueView = sumValueDouble.ToString("G3", CultureInfo.InvariantCulture) + " " + "Nghìn tỷ đồng";
            }
            else
            {
                SumValueView = sumValueDouble.ToString("N0", CultureInfo.InvariantCulture) + " "+ "đ";
            }
            ViewBag.SumValueDetail = sumValueDetail.ToString("N0", CultureInfo.InvariantCulture) + " "+ "Đồng"; 
            ViewBag.SumValueView = SumValueView;
            ViewBag.NumContract = getNumContract;
            // query logdata overview
            var logData = db.LogData.AsNoTracking().ToList();
            var totalRequest = logData.Count();
            var failedRequest = logData.Where(x => x.scStatus >= 400);//fail
            var twoHundredStatusCodes = logData.Where(x => x.scStatus >= 200 && x.scStatus < 300); //200
            var threeHundredStatusCodes = logData.Where(x => x.scStatus >= 300 && x.scStatus < 400);//300
            var fourHundredStatusCodes = logData.Where(x => x.scStatus >= 400 && x.scStatus < 500);//400
            var fiveHundredStatusCodes = logData.Where(x => x.scStatus >= 500);//500

            var badRequestStatusCodes = logData.Where(x => x.scStatus == 400); //400
            var unauthorizedStatusCodes = logData.Where(x => x.scStatus == 401); //401
            var forbidenStatusCodes = logData.Where(x => x.scStatus == 403); //403
            var notFoundStatusCodes = logData.Where(x => x.scStatus == 404); //404
            var internalServerErrorStatusCodes = logData.Where(x => x.scStatus == 500); //500
            var badGatewayStatusCodes = logData.Where(x => x.scStatus == 502); //502
            var serviceUnavailableStatusCodes = logData.Where(x => x.scStatus == 503); //503
            var gatewayTimeoutStatusCodes = logData.Where(x => x.scStatus == 504); //504


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
            var listKeyValue = new List<ChartViewModel>();
            var now = DateTime.Now.Date;
            var listDaysBetween = new List<DateTime>();
            for (int i=-7; i<=0 ; i++)
            {
                var day = now.AddDays(i);
                listDaysBetween.Add(day);
            }
            foreach(var item in listDaysBetween)
            {
                var dayAfter = item.AddDays(1).Date;
                var keyValue = new ChartViewModel();
                var test = db.LogData.Where(x => DbFunctions.TruncateTime(x.date) >= DbFunctions.TruncateTime(item) && DbFunctions.TruncateTime(x.date) < DbFunctions.TruncateTime(dayAfter)).ToList();
                var requestInTime = db.LogData.Where(x => DbFunctions.TruncateTime(x.date) >= DbFunctions.TruncateTime(item) && DbFunctions.TruncateTime(x.date) < DbFunctions.TruncateTime(dayAfter)).Count();
                var key = item.ToString("dd/MM/yyyy");
                keyValue.xAxes = key;
                keyValue.yAxes = requestInTime;
                listKeyValue.Add(keyValue);
            }
            return Json(listKeyValue, JsonRequestBehavior.AllowGet);

        }

        //bar chart Request

        [AllowAnonymous]
        public JsonResult GetDataRequest()
        {
            var listQueryValue = new List<QueryBarChartModel>();
            var listColor = new List<string>
                { "#C0392B","#8E44AD", "#2980B9", "#16A085", "#27AE60" , "#F1C40F", "#F39C12", "#F39C12", "#BA4A00", "#95A5A6", "#566573", "#4FC3F7"};

            var timenow = DateTime.Now;
            var listTime = new List<DateTime>();
            var listTimeString = new List<string>();
            for (int i=0; i>-60; i--)
            {
                var time = timenow.AddMinutes(i);
                listTime.Add(time);
                listTimeString.Add(time.ToString("HH:mm"));
            }
            listTime.Reverse();
            listTimeString.Reverse();
            var xAxes = listTimeString.ToArray();
            List<StatusCodeViewModel> listStatusCode = new List<StatusCodeViewModel>
            {
                new StatusCodeViewModel{ code = "2xx" , number = 200},
                new StatusCodeViewModel{ code = "3xx" , number = 300},
                new StatusCodeViewModel{ code = "4xx" , number = 400},
                new StatusCodeViewModel{ code = "5xx" , number = 500}
            };

            var listDataExport = new List<BarChartViewModel>();

            foreach (var item in listStatusCode)
            {
                var numberPlus = item.number + 100;
                var barChartViewModel = new BarChartViewModel();
                barChartViewModel.label = item.code;
                barChartViewModel.type = "bar";
                barChartViewModel.stack = "base";
                var listValue = new List<int>();

                foreach (var time in listTime)
                {
                    var timedate = time.Date;
                    var timeOfDay = time.TimeOfDay;
                    var timePlus = time.AddMinutes(1);
                    var timePlusOfDay = time.AddMinutes(1).TimeOfDay;
                    var test = db.LogData.Where(x => DbFunctions.TruncateTime(x.date) == timedate 
                    && SqlFunctions.DatePart("hour", x.date) == time.Hour
                    && SqlFunctions.DatePart("minute", x.date) == time.Minute
                    && x.scStatus >= item.number 
                    && x.scStatus < numberPlus
                    ).ToList();
                    //var value = db.LogData.Where(x => DbFunctions.TruncateTime(x.date) >= timedate && DbFunctions.DiffMinutes(x.) &&x.date.TimeOfDay >= timeOfDay && x.date.TimeOfDay < timeOfDayPlus && x.scStatus >= item.number && x.scStatus < numberPlus).ToList();
                    listValue.Add(test.Count());
                }

                

                barChartViewModel.data = listValue;
                listDataExport.Add(barChartViewModel);
            }
            for (var i = 0; i < listDataExport.Count(); i++)
            {
                listDataExport[i].backgroundColor = listColor[i];
            }
            var stringJsonYAxes = listDataExport.ToArray();
            return Json(new { xAxes = xAxes, yAxes = stringJsonYAxes }, JsonRequestBehavior.AllowGet);

        }


        //TopPath chart Request 
        [AllowAnonymous]
        public JsonResult GetDataTopPath()
        {
            var listColor = new List<string>
                { "#C0392B","#8E44AD", "#2980B9", "#16A085", "#27AE60" , "#F1C40F", "#F39C12", "#F39C12", "#BA4A00", "#95A5A6", "#566573", "#4FC3F7"};
            var now = DateTime.Now.Date;
            var listPath = (from log in db.LogData
                            where log.csReferer != null
                            group log by log.csReferer into refererGroup
                            orderby refererGroup.Count() descending
                            select new { Referer = refererGroup.Key, Count = refererGroup.Count() })
             .Take(5);
            var barChartViewModel = new VerticalBarChartViewModel();
            barChartViewModel.axis = "y";
            barChartViewModel.fill = "false";
            barChartViewModel.borderWidth = "1";
            var listxAxes = new List<string>();
            var listdata = new List<int>();
            var listbackground = new List<string>();
            foreach (var item in listPath)
            {
                listxAxes.Add(item.Referer.Replace("https://phantichnhatky.xyz", "").Replace("https://www.phantichnhatky.xyz", ""));
                listdata.Add(item.Count);
            }
            for (var i = 0; i < listdata.Count(); i++)
            {
                listbackground.Add(listColor[i]);
            }
            barChartViewModel.data = listdata.ToArray();
            barChartViewModel.backgroundColor = listbackground.ToArray();
             
            var xAxesArr = listxAxes.ToArray();
            var yAxesList = new List<VerticalBarChartViewModel>();
            yAxesList.Add(barChartViewModel);
            var yAxesArr = yAxesList.ToArray();
            return Json(new { xAxes = xAxesArr, yAxes = yAxesArr }, JsonRequestBehavior.AllowGet);

        }

        //ClientIP chart  
        [AllowAnonymous]
        public JsonResult GetDataClientIP()
        {
            var listColor = new List<string>
                { "#C0392B","#8E44AD", "#2980B9", "#16A085", "#27AE60" , "#F1C40F", "#F39C12", "#F39C12", "#BA4A00", "#95A5A6", "#566573", "#4FC3F7"};
            var now = DateTime.Now.Date;
            var listPath = (from log in db.LogData
                            where log.cIp != null
                            group log by log.cIp into refererGroup
                            orderby refererGroup.Count() descending
                            select new { Referer = refererGroup.Key, Count = refererGroup.Count() })
             .Take(5);
            var barChartViewModel = new VerticalBarChartViewModel();
            barChartViewModel.axis = "y";
            barChartViewModel.fill = "false";
            barChartViewModel.borderWidth = "1";
            var listxAxes = new List<string>();
            var listdata = new List<int>();
            var listbackground = new List<string>();
            foreach (var item in listPath)
            {
                listxAxes.Add(item.Referer);
                listdata.Add(item.Count);
            }
            for (var i = 0; i < listdata.Count(); i++)
            {
                listbackground.Add(listColor[i]);
            }
            barChartViewModel.data = listdata.ToArray();
            barChartViewModel.backgroundColor = listbackground.ToArray();

            var xAxesArr = listxAxes.ToArray();
            var yAxesList = new List<VerticalBarChartViewModel>();
            yAxesList.Add(barChartViewModel);
            var yAxesArr = yAxesList.ToArray();
            return Json(new { xAxes = xAxesArr, yAxes = yAxesArr }, JsonRequestBehavior.AllowGet);

        }

        //ClientIP chart  
        [AllowAnonymous]
        public JsonResult GetDataProtocolVersion()
        {
            var listColor = new List<string>
                { "#C0392B","#8E44AD", "#2980B9", "#16A085", "#27AE60" , "#F1C40F", "#F39C12", "#F39C12", "#BA4A00", "#95A5A6", "#566573", "#4FC3F7"};
            var now = DateTime.Now.Date;
            var listPath = (from log in db.LogData
                            where log.csVersion != null
                            group log by log.csVersion into refererGroup
                            orderby refererGroup.Count() descending
                            select new { Referer = refererGroup.Key, Count = refererGroup.Count() })
             .Take(3);
            var barChartViewModel = new VerticalBarChartViewModel();
            barChartViewModel.axis = "y";
            barChartViewModel.fill = "false";
            barChartViewModel.borderWidth = "1";
            var listxAxes = new List<string>();
            var listdata = new List<int>();
            var listbackground = new List<string>();
            foreach (var item in listPath)
            {
                listxAxes.Add(item.Referer);
                listdata.Add(item.Count);
            }
            for (var i = 0; i < listdata.Count(); i++)
            {
                listbackground.Add(listColor[i]);
            }
            barChartViewModel.data = listdata.ToArray();
            barChartViewModel.backgroundColor = listbackground.ToArray();

            var xAxesArr = listxAxes.ToArray();
            var yAxesList = new List<VerticalBarChartViewModel>();
            yAxesList.Add(barChartViewModel);
            var yAxesArr = yAxesList.ToArray();
            return Json(new { xAxes = xAxesArr, yAxes = yAxesArr }, JsonRequestBehavior.AllowGet);

        }

        //ClientIP chart  
        [AllowAnonymous]
        public JsonResult GetDataHttpMethod()
        {
            var listColor = new List<string>
                { "#C0392B","#8E44AD", "#2980B9", "#16A085", "#27AE60" , "#F1C40F", "#F39C12", "#F39C12", "#BA4A00", "#95A5A6", "#566573", "#4FC3F7"};
            var now = DateTime.Now.Date;
            var listPath = (from log in db.LogData
                            where log.csMethod != null
                            group log by log.csMethod into refererGroup
                            orderby refererGroup.Count() descending
                            select new { Referer = refererGroup.Key, Count = refererGroup.Count() })
             .Take(5);
            var barChartViewModel = new VerticalBarChartViewModel();
            barChartViewModel.axis = "y";
            barChartViewModel.fill = "false";
            barChartViewModel.borderWidth = "1";
            var listxAxes = new List<string>();
            var listdata = new List<int>();
            var listbackground = new List<string>();
            foreach (var item in listPath)
            {
                listxAxes.Add(item.Referer);
                listdata.Add(item.Count);
            }
            for (var i = 0; i < listdata.Count(); i++)
            {
                listbackground.Add(listColor[i]);
            }
            barChartViewModel.data = listdata.ToArray();
            barChartViewModel.backgroundColor = listbackground.ToArray();

            var xAxesArr = listxAxes.ToArray();
            var yAxesList = new List<VerticalBarChartViewModel>();
            yAxesList.Add(barChartViewModel);
            var yAxesArr = yAxesList.ToArray();
            return Json(new { xAxes = xAxesArr, yAxes = yAxesArr }, JsonRequestBehavior.AllowGet);

        }



        /* [AllowAnonymous]
         public JsonResult GetTotalMoneyUser()
         {
             //var listCustomer = db.UserProfiles.Where(x => x.IsActive != false && x.Type == (int)TypeAccount.Customer).ToList();
             var listQueryValue = new List<DoughnutChartQuery>();

             var listAccetType = db.TypeOfAssets.Where(x => x.IsActive != false).ToList();
             var buyAndSellBonds = db.BuyAndSellBonds.Where(x => x.IsActive != false).ToList();
             var listColor = new List<string>
                 { "#C0392B","#8E44AD", "#2980B9", "#16A085", "#27AE60" , "#F1C40F", "#F39C12", "#BA4A00", "#95A5A6", "#566573", "#4FC3F7", "#a1eb34", "#34e5eb", "#34a2eb", "#eb3474", "#eb8034", "#b134eb", "#eb4f34"};


             foreach (var type in listAccetType)
             {
                 var dataQuery = new DoughnutChartQuery();

                 //sum and get list Id all value with userId and type
                 var listBuyAndSellBonds = buyAndSellBonds.Where(x => x.IsActive != false && x.TypeOfAsset == type).ToList();
                 var listIdBuyAndSellBond = listBuyAndSellBonds.Select(x => x.Id).ToList();
                 var valueSumContract = listBuyAndSellBonds.Sum(x => x.Value);

                 //Sum value Interest Contract with TransactionType is GetInterest contain list id in listBuyAndSellBonds
                 var valueSumInterestContract = db.IncurredPurchases.Where(x => (x.TransactionType == (int)(TypeTransaction.GetInterestByTime) || x.TransactionType == (int)(TypeTransaction.GetInterestOntime)) && x.IsActive != false
                 && listIdBuyAndSellBond.Contains(x.BuyAndSellBondId ?? default)).Select(x => x.AmountOfMoney).ToList().Sum();

                 //subtract using source by listBuyAndSellBonds
                 var getSumSource = db.Periods.Where(x => x.IsActive != false && x.IncurredId != null && listIdBuyAndSellBond.Contains((x.Incurred.BuyAndSellBondId ?? default))).Select(x => x.Value).ToList().Sum();

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

         }*/


        [AllowAnonymous]
        public JsonResult GetTotalMoneyUserDetail(string label)
        {
            //var listCustomer = db.UserProfiles.Where(x => x.IsActive != false && x.Type == (int)TypeAccount.Customer).ToList();
            var listQueryValue = new List<DoughnutChartQuery>();

            var listAccetType = db.AssetCategorys.Where(x => x.IsActive != false).ToList(); //get asset category
            var buyAndSellBonds = db.BuyAndSellBonds.Where(x => x.IsActive != false).ToList();
            var listColor = new List<string>
                { "#C0392B","#8E44AD", "#2980B9", "#16A085", "#27AE60" , "#F1C40F", "#F39C12", "#BA4A00", "#95A5A6", "#566573", "#4FC3F7", "#a1eb34", "#34e5eb", "#34a2eb", "#eb3474", "#eb8034", "#b134eb", "#eb4f34"};


            foreach (var type in listAccetType)
            {
                var dataQuery = new DoughnutChartQuery();

                //sum and get list Id all value with userId and type
                var listBuyAndSellBonds = buyAndSellBonds.Where(x => x.IsActive != false && x.AssetCategorys == type && x.TypeOfAsset.AssetName == label).ToList();
                var listIdBuyAndSellBond = listBuyAndSellBonds.Select(x => x.Id).ToList();
                var valueSumContract = listBuyAndSellBonds.Sum(x => x.Value);

                //Sum value Interest Contract with TransactionType is GetInterest contain list id in listBuyAndSellBonds
                var valueSumInterestContract = db.IncurredPurchases.Where(x => (x.TransactionType == (int)(TypeTransaction.GetInterestByTime) || x.TransactionType == (int)(TypeTransaction.GetInterestOntime)) && x.IsActive != false
                && listIdBuyAndSellBond.Contains(x.BuyAndSellBondId ?? default)).Select(x => x.AmountOfMoney).ToList().Sum();

                //subtract using source by listBuyAndSellBonds
                var getSumSource = db.Periods.Where(x => x.IsActive != false && x.IncurredId != null && listIdBuyAndSellBond.Contains((x.Incurred.BuyAndSellBondId ?? default))).Select(x => x.Value).ToList().Sum();

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

        public ConfigSendMailInfo GetInfoConfigSendMail()
        {
            var getConfigInfo = db.WebConfigs.ToList();
            var listEmailUser = db.UserProfiles.Select(x => x.Email).ToList();
            var listEmailTo = ""; 
            foreach (var item in listEmailUser)
            {
                listEmailTo = listEmailTo + item + ",";
            }
            return new ConfigSendMailInfo
            {
                Host = getConfigInfo.Where(x => x.Key == "email-send-smtp").FirstOrDefault().Value,
                Port = getConfigInfo.Where(x => x.Key == "email-send-port").FirstOrDefault().Value,
                SendFrom = getConfigInfo.Where(x => x.Key == "email-send").FirstOrDefault().Value,
                EmailPass = getConfigInfo.Where(x => x.Key == "email-send-password").FirstOrDefault().Value,
                Ssl = getConfigInfo.Where(x => x.Key == "email-send-ssl").FirstOrDefault().Value,
                EmailTo = listEmailTo
            };
        }
        public MailTitleAndBodyModel CreateTitleAndBody(UserProfile user, string detailAlert, DateTime? time, string title)
        {
            var body = "";
            body += "<table align='center' border='0' cellpadding='0' cellspacing='0' lang='container' style='max-width:700px' width='100%'>";
            body += "<tbody>";
            body += "<tr>";
            body += "<td bgcolor='#f0f0f0' style='background:#f0f0f0' valign='top' width='100%'>";
            body += "<table border='0' cellpadding='0' cellspacing='0' lang='main_content' style='width:100%' width='100%'>";
            body += "<tbody>";
            body += "<tr>";
            body += "<td valign='top' width='100%'>";
            body += "<div style='font-size:30px;line-height:30px;height:30px'>";
            body += "</div>";
            body += "</td>";
            body += "</tr>";
            body += "<tr>";
            body += "<td valign = 'top' width = '100%'>";
            body += "<table border='0' cellpadding='0' cellspacing='0' style='width:100%' width='100%'>";
            body += "<tbody>";
            body += "<tr>";
            body += "<td style='width:20px' width='20'>";
            body += "<div lang='space40'>";
            body += "</div>";
            body += "</td>";
            body += "<td valign='top'>";
            body += "<p style='margin:0;padding:0;font-size:18px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:bold;font-style: italic;'>";
            body += "Kính chào: Quý khách" + " " + user.FullName;
            body += "</p>";
            body += "<div style='font-size:10px;line-height:10px;height:10px'>";
            body += "</div>";
            body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
            body += "Website phantichnhatky.xyz xin thông báo và lưu ý khách hàng về " + detailAlert;
            body += "</p>";
            body += "<br>";

            body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
            body += "Mong quý khách hàng lưu ý và kiểm tra lại hệ thống.";
            body += "</p>";
            body += "<br/>";

            body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
            body += "Link truy cập hệ thống thu thập và phân tích nhật ký máy chủ web: https://phantichnhatky.xyz/" + " ";
            body += "</p>";
            body += "<br/>";
            body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
            body += "Chân thành cảm ơn Quý khách hàng đã quan tâm và sử dụng dịch vụ của phantichnhatky.xyz<br>";
            body += "Mọi yêu cầu cần giải đáp, Xin Quý khách vui lòng liên hệ với chúng tôi.";
            body += "</p>";
            body += "<br/>";
            body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
            body += "Thông tin liên hệ (Contact Center): <br>";
            body += "Điện thoại: 0984247608<br>";
            body += "Website: https://phantichnhatky.xyz/ <br>";
            body += " Email: nguyenmanhthe281200@gmail.com<br>";
            body += "</p>";
            body += "<br/>";
           /* body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:bold;font-style: italic;'>HỆ THỐNG THU THẬP VÀ PHÂN TÍCH NHẬT KÝ MÁY CHỦ WEB</p>";
            body += "<img src = '" + "https://phantichnhatky.xyz" + "/Content/themes/admin/img/footerMail.jpg' style='max-width:650px' width='100%' />";*/
            body += "</td>";
            body += "<td style='width:20px' width='20'>";
            body += "<div lang='space40'>";
            body += "</div>";
            body += "</td>";
            body += "</tr>";
            body += "</tbody>";
            body += "</table>";
            body += "</td>";
            body += "</tr>";
            body += "<tr>";
            body += "<td valign='top' width='100%'>";
            body += "<div style='font-size:30px;line-height:30px;height:30px'>";
            body += "</div>";
            body += "</td>";
            body += "</tr>";
            body += "</tbody>";
            body += "</table>";
            body += "</td>";
            body += "</tr>";
            body += "</tbody>";
            body += "</table>";

            return new MailTitleAndBodyModel
            {
                Title = "[phantichnhatky.xyz] " + title,
                Body = body
            };
        }

        public static bool SendMailSystem(ConfigSendMailInfo configSendMailInfo, MailTitleAndBodyModel mailTitleAndBodyModel)
        {
            bool _return = false;

            try
            {
                var client = new SmtpClient(configSendMailInfo.Host, int.Parse(configSendMailInfo.Port));
                client.UseDefaultCredentials = false;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(configSendMailInfo.SendFrom, configSendMailInfo.EmailPass);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)48 | (SecurityProtocolType)192 |
                (SecurityProtocolType)768 | (SecurityProtocolType)3072;
                var message = new MailMessage();
                message.From = new MailAddress(configSendMailInfo.SendFrom, configSendMailInfo.SendFrom);
                message.Subject = mailTitleAndBodyModel.Title;
                string[] Multi = configSendMailInfo.EmailTo.Split(',');
                foreach (string email in Multi)
                {
                    message.To.Add(new MailAddress(email));
                }
                message.Body = mailTitleAndBodyModel.Body;
                message.IsBodyHtml = true;
                message.BodyEncoding = Encoding.UTF8;

                var mailThread = new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        client.Send(message);
                    }
                    catch (Exception ex)
                    {
                    }
                }));

                mailThread.Start();

                _return = true;
            }
            catch (Exception ex)
            {
                _return = false;
            }

            return _return;
        }
    }
}
