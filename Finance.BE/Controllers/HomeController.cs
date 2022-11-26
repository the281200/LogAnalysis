using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using WebModels;
using System.Data;
using System;
using System.Collections.Generic;
using WEB.Models;
using System.Data.Entity;
using System.Web.Routing;
using System.Globalization;

namespace WEB.Controllers
{
    public class HomeController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index(int? id, string uid, string lang, string metatitle, int? page, string controller)
        {
            //var checkUserLogin = WebSecurity.CurrentUserId > 0 && WebSecurity.CurrentUserName != null;
            //var home = new List<string> { "home", "index", "home.html", "index.html", "trangchu", "trang-chu", "trangchu.html", "trang-chu.html" };

            //if (checkUserLogin)
            //{
            //    var userInfo = db.UserProfiles.Find(WebSecurity.CurrentUserId);
            //    if (userInfo.Type == null || (userInfo.Type != null && userInfo.Type != (int)TypeAccount.Customer))
            //    {
            //        FormsAuthentication.SignOut();
            //        return RedirectToAction("Login", "Account", new { area = "" });
            //    }
            //    ViewBag.UserId = WebSecurity.CurrentUserId;
            //    ViewBag.UserName = WebSecurity.CurrentUserName;
            //    ViewBag.Controller = (controller == null || controller == "") ? "Home" : controller;

            //    if (id.HasValue)
            //    {
            //        var module = db.Set<WebModule>().Find(id);
            //        TempData["WebModule"] = module;
            //        ViewBag.Page = page;
            //        return View(module);

            //    }
            //    else if (!string.IsNullOrEmpty(uid))
            //    {

            //        var module = (from x in db.WebModules
            //                      where
            //                          (x.UID.ToLower().Equals(uid.ToLower()))
            //                      select x).AsNoTracking().FirstOrDefault();
            //        return View(module);
            //    }
            //    else
            //    {
            //        var module = (from x in db.WebModules
            //                      where
            //                          (x.UID == null || home.Contains(x.ContentType.ID.ToLower()))
            //                      select x).AsNoTracking().FirstOrDefault();
            //        ViewBag.dashboard = 1;
            //        return View(module);
            //    }

            //}

            return RedirectToAction("Login", "Account", new { area = "Admin" });
        }

        public ActionResult Detail(int id, string metatitle, int m_id, string m_metatitle)
        {
            ViewBag.ID = id;
            ViewBag.m_id = 3131; //WebModule.Id chi-tiet
            var module = db.Set<WebModule>().Find(3131);
            return View(module);
        }
        [OutputCache(Duration = 120, VaryByCustom = "culture")]
        public ActionResult SiteMap()
        {
            ViewBag.Language = Language;
            var webmodules = from e in db.WebModules
                             where (e.ParentID == null)
                             orderby e.Order
                             select e;
            return View(webmodules);
        }
        public ActionResult SiteMapUser(int? id)
        {
            var webmodules = db.WebModules.Include("SubWebModules").FirstOrDefault(x => x.ID == id);
            var allParent = new List<WebModule>();
            getParentModule(id, allParent);

            ViewBag.AllParent = allParent;
            return View(webmodules);
        }
        public List<WebModule> getParentModule(int? id, List<WebModule> result)
        {
            var webmodules = db.WebModules.FirstOrDefault(x => x.ID == id);
            result.Add(webmodules);
            if (webmodules.ParentID != null)
            {
                getParentModule(webmodules.ParentID, result);
            }
            return result;
        }
        public ActionResult ProductSearch(string keyword)
        {
            return View();
        }
        public ActionResult Order()
        {
            return View();
        }
        public ActionResult Search(int? page)
        {
            WebContext db = new WebContext();
            var keyword = Request.QueryString["keyword"];
            var skeyword = "";

            if (keyword != null) skeyword = keyword.ToString().ToLower();

            ViewBag.RouteValues = new RouteValueDictionary(new
            {
                controller = "Home",
                action = "Search",
                area = "",
                keyword = keyword
            });

            var a = skeyword.UnsignNormalize();

            var contents = db.WebContents.Where(x => x.WebModule.ContentTypeID.Equals("Article") && x.Status == (int)Status.Public).ToList();

            if (!string.IsNullOrWhiteSpace(skeyword))
            {
                contents = contents.Where(x => x.Status == (int)Status.Public
                && (x.Title.ToLower().Contains(skeyword)
                || (x.MetaTitle != null && x.MetaTitle.ToLower().Contains(a)
                || (x.MetaKeywords != null && x.MetaKeywords.ToLower().Contains(skeyword)))
                || (x.Body != null && x.Body.ToLower().Contains(a)
                ))).ToList();
            }

            var ipage = 1; if (page != null) ipage = page.Value;
            ViewBag.TotalItemCount = contents.Count();
            ViewBag.CurrentPage = ipage;
            return View(contents.Skip((ipage - 1) * ApplicationService.PageSize).Take(ApplicationService.PageSize).OrderByDescending(x => x.CreatedDate).ToList());
        }
        public ActionResult Event(int? page)
        {
            WebContext db = new WebContext();

            var date = Request.QueryString["date"];
            var sdate = DateTime.Now;
            if (date != null) sdate = Convert.ToDateTime(date);
            ViewBag.RouteValues = new RouteValueDictionary(new
            {
                controller = "Home",
                action = "Event",
                area = "",
                date = date
            });
            var contents = db.WebContents.Where(x => x.Status == (int)Status.Public && x.Event == sdate).ToList();
            var ipage = 1; if (page != null) ipage = page.Value;
            ViewBag.TotalItemCount = contents.Count();
            ViewBag.CurrentPage = ipage;
            return View(contents.Skip((ipage - 1) * ApplicationService.PageSize).Take(ApplicationService.PageSize).OrderByDescending(x => x.CreatedDate).ToList());
        }
        [ChildActionOnly]
        public ActionResult _Language()
        {
            return PartialView(this.Language);
        }
        public ActionResult PartnersHome()
        {
            var webmodul = db.WebModules.Where(x => x.UID == "partner" && x.Status == (int)Status.Public).FirstOrDefault();

            var webcontent = db.WebContents.Where(x => x.WebModuleID == webmodul.ID).ToList();
            return PartialView(webcontent);
        }
        [ChildActionOnly]
        public ActionResult _SiteMap(int id = 0, int m_id = 0)
        {
            id = m_id > 0 ? m_id : id;

            if (id == 0)
            {
                return PartialView(null);
            }

            WebModule module = this.db.WebModules.Find(id);
            Stack<WebModule> modules = new Stack<WebModule>();

            do
            {
                modules.Push(module);

                module = module.Parent;
            }
            while (module != null);

            return PartialView(modules);
        }
        public static WebConfig getconfig(string key)
        {
            WebContext db = new WebContext();

            var config = (from c in db.WebConfigs
                          where c.Key.Equals(key)
                          select c);

            return config.FirstOrDefault();
        }

        public List<WebContent> GetListContents(int webModuleId, List<WebContent> results)
        {
            var webContents = db.WebContents.Where(x => x.WebModuleID == webModuleId);
            results.AddRange(webContents);

            var childWebModules = db.WebModules.Where(x => x.ParentID == webModuleId);

            foreach (var childWebModule in childWebModules)
            {
                GetListContents(childWebModule.ID, results);
            }

            return results;
        }

        //public static void RequestCheckContract()
        //{


        //}
        public int TodayWarning()
        {
            var dateCompare = DateTime.Now.AddDays(6);
            var last5days = DateTime.Today.AddDays(5);
            var getInterestPaymentPeriodsId = db.IncurredPurchases.Where(x => x.InterestPaymentPeriodsId != null).Select(x => x.InterestPaymentPeriodsId).ToList();
            /*var getListDisbursementid = db.IncurredPurchases.Where(x => x.ContactDisbursementId != null).Select(x => x.ContactDisbursementId).ToList();*/
            //today warning

            var getNumEarningDueWarning = db.InterestPaymentPeriods.Where(y => (!getInterestPaymentPeriodsId.Contains(y.Id)) && y.BuyAndSellBondId != 0 && DbFunctions.TruncateTime(y.CreatedAt) == DateTime.Today && y.IsActive != false && y.InterestPaymentDate != null && DbFunctions.TruncateTime(y.InterestPaymentDate) < DbFunctions.TruncateTime(dateCompare)).Count();

            var getNumSettlementWarning = db.IncurredPurchases.Where(y => (y.TransactionType == (int)(TypeTransaction.ContractSettlementByTime) || y.TransactionType == (int)(TypeTransaction.ContractSettlementOnTime)) && y.IsActive != false && DbFunctions.TruncateTime(y.CreatedAt) == DateTime.Today && y.IncurredDate != null && DbFunctions.TruncateTime(y.IncurredDate) < DbFunctions.TruncateTime(dateCompare)).Count();

            /*var getNumDisbursementWarning = db.ContactDisbursementDetails.Where(y => (!getListDisbursementid.Contains(y.Id)) && y.BuyAndSellBondId != 0 && DbFunctions.TruncateTime(y.CreatedAt) == DateTime.Today && y.IsActive != false && y.ImplementationDate != null && DbFunctions.TruncateTime(y.ImplementationDate) < DbFunctions.TruncateTime(dateCompare)).Count();*/

            var sumTodayWarning = getNumEarningDueWarning + getNumSettlementWarning /*+ getNumDisbursementWarning*/;

            //last 5 days
            var getNumEarningDueWarningLast5Days = db.InterestPaymentPeriods.Where(y => (!getInterestPaymentPeriodsId.Contains(y.Id)) && y.BuyAndSellBondId != 0 && y.IsActive != false && y.InterestPaymentDate != null && DbFunctions.TruncateTime(y.InterestPaymentDate) == last5days).Count();

            var getNumSettlementWarningLast5Days = db.IncurredPurchases.Where(y => (y.TransactionType == (int)(TypeTransaction.ContractSettlementByTime) || y.TransactionType == (int)(TypeTransaction.ContractSettlementOnTime)) && y.IsActive != false && y.IncurredDate != null && DbFunctions.TruncateTime(y.IncurredDate) == last5days).Count();

            /*var getNumDisbursementWarningLast5Days = db.ContactDisbursementDetails.Where(y => (!getListDisbursementid.Contains(y.Id)) && y.BuyAndSellBondId != 0 && y.IsActive != false && y.ImplementationDate != null && DbFunctions.TruncateTime(y.ImplementationDate) == last5days).Count();*/

            var sumLast5DaysWarning = getNumEarningDueWarningLast5Days + getNumSettlementWarningLast5Days /*+ getNumDisbursementWarningLast5Days*/;

            //total
            var notification = sumTodayWarning + sumLast5DaysWarning;

            return notification;
        }

        public ActionResult RequestCheckNoti()
        {
            var sumWarning = TodayWarning();

            var userAdmin = db.UserProfiles.Where(x => x.IsActive != false && x.Type == (int)TypeAccount.Admin).ToList();

            foreach(var item in userAdmin)
            {
                item.UnReadNotiCount = (item.UnReadNotiCount ?? default) +  sumWarning;
                if(item.UnReadNotiCount > 0)
                {
                    item.IsReadNotification = false;
                }
            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult RequestCheckContract()
        {
            //Đến hạn tất toán hợp đồng
            SendMailContractSettlement();

            //Đến ngày nhận lợi tức
            SendMailContractGetInterest();

            //Đến ngày thực hiện giải ngân
            /*SendMailContactDisbursementDetail();*/

            return RedirectToAction("Index");
        }

        public void SendMailContractSettlement()
        {
            var dataAdd10Day = DateTime.Now.AddDays(-10);
            var dataAdd5Day = DateTime.Now.AddDays(-5);
            var checkContractSettlement = db.IncurredPurchases.Where(
               x => (DbFunctions.TruncateTime(x.IncurredDate) == DbFunctions.TruncateTime(DateTime.Now)|| DbFunctions.TruncateTime(x.IncurredDate) == DbFunctions.TruncateTime(dataAdd10Day) 
               || DbFunctions.TruncateTime(x.IncurredDate) == DbFunctions.TruncateTime(dataAdd5Day)) && x.IsActive != false
               &&( x.TransactionType == (int)TypeTransaction.ContractSettlementByTime || x.TransactionType == (int)TypeTransaction.ContractSettlementOnTime) && x.BuyAndSellBond.Customer != null).Include(x => x.BuyAndSellBond).Include(x => x.BuyAndSellBond.Customer).ToList();
            var getBeSite = db.WebConfigs.Where(x => x.Key == "SiteBE").Select(x => x.Value).FirstOrDefault().ToString();
            foreach (var item in checkContractSettlement)
            {
                
                var title = "[Quản lý danh mục đầu tư TLBonds] Thông báo đến hạn thanh lý hợp đồng " ;
                /*var body = "Quý khách có “hợp đồng” đến hạn tất toán với giá trị: " + item.BuyAndSellBond.ContractName + " đã đến hạn tất toán vào ngày " + string.Format("{0:dd/MM/yyyy}", item.IncurredDate) +
                    ". Quý khách hàng có thể truy cập vào hệ thống để kiểm tra và theo dõi tình trạng đầu tư.<br/>Xin cám ơn!";*/
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
                body += "Kính chào: Quý khách (Dear Mr./Ms.)" + " " + item.Customer.FullName;
                body += "</p>";
                body += "<div style='font-size:10px;line-height:10px;height:10px'>";
                body += "</div>";
                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                body += "Công ty cổ phần Đầu tư trái phiếu Thăng Long(TLBONDS) xin trân trọng thông báo và lưu ý khách hàng về tình trạng hợp đồng đến kỳ hạn thanh lý/ tất toán.";
                body += "</p>";
                body += "<br>";
                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                body += "-	Mã hợp đồng :" + " " + item.BuyAndSellBond.ContractCode;
                body += "</p>";
                body += "<br/>";

                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                body += "-	Tên hợp đồng  :" + " " + item.BuyAndSellBond.ContractName;
                body += "</p>";
                body += "<br/>";

                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                body += "-	Giá trị hợp đồng : " + " " + item.BuyAndSellBond.Value.Value.ToString("N0", CultureInfo.InvariantCulture) + " " + "đ";
                body += "</p>";
                body += "<br/>";

                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                body += "-	Đã đến hạn tất toán/thanh lý vào ngày :" + " " + string.Format("{0:dd/MM/yyyy}", item.IncurredDate);
                body += "</p>";
                body += "<br/>";

                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                body += "Đề nghị quý khách hàng lưu ý trong quá trình giao dịch.";
                body += "</p>";
                body += "<br/>";

                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                body += "Link truy cập hệ thống quản lý danh mục đầu tư: http://quanlydanhmucdautu.tlbonds.vn/" + " ";
                body += "<br/>";
                body += "hoặc truy cập website https://tlbonds.vn/ để sử dụng.";
                body += "</p>";
                body += "<br/>";
                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                body += "Chân thành cảm ơn Quý khách hàng đã quan tâm và sử dụng dịch vụ của TLBonds. <br>";
                body += "Mọi yêu cầu cần giải đáp, Xin Quý khách vui lòng liên hệ với TLBonds.";
                body += "</p>";
                body += "<br/>";
                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                body += "Thông tin liên hệ (Contact Center): <br>";
                body += "Điện thoại: 024.32047782<br>";
                body += "Website: https://www.tlbonds.vn<br>";
                body += " Email: support@tlbonds.vn<br>";
                body += "</p>";
                body += "<br/>";
                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:bold;font-style: italic;'>CÔNG TY CỔ PHẦN ĐẦU TƯ TRÁI PHIẾU THĂNG LONG (TLBONDS)</p>";
                body += "<img src = '" + getBeSite + "/Content/themes/admin/img/footerMail.jpg' style='max-width:650px' width='100%' />";
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

                var bodySMS = "Quý khách có “hợp đồng” đến hạn tất toán với giá trị: " + item.AmountOfMoney + " đ vào ngày " + string.Format("{0:dd/MM/yyyy}", item.IncurredDate);
                var checkSendMail = Models.ApplicationService.SendMailSystem(item.BuyAndSellBond.Customer, GetInfoConfigSendMail(), title, body);
                //ApplicationService.SendJsonSMS(item.BuyAndSellBond.Customer.Mobile, bodySMS);

            }
        }

        public void SendMailContractGetInterest()
        {
            var dataAdd10Day = DateTime.Now.AddDays(-10);
            var dataAdd5Day = DateTime.Now.AddDays(-5);

            var checkContractGetInterest = db.IncurredPurchases.Where(
               x => (DbFunctions.TruncateTime(x.IncurredDate) == DbFunctions.TruncateTime(DateTime.Now) || DbFunctions.TruncateTime(x.IncurredDate) == DbFunctions.TruncateTime(dataAdd10Day)
               || DbFunctions.TruncateTime(x.IncurredDate) == DbFunctions.TruncateTime(dataAdd5Day))
               && x.IsActive != false
               && (x.TransactionType == (int)TypeTransaction.GetInterestByTime || x.TransactionType == (int)TypeTransaction.GetInterestOntime) && x.BuyAndSellBond.Customer != null).Include(x => x.BuyAndSellBond).Include(x => x.BuyAndSellBond.Customer).ToList();
            var getBeSite = db.WebConfigs.Where(x => x.Key == "SiteBE").Select(x => x.Value).FirstOrDefault().ToString();
            foreach (var item in checkContractGetInterest)
            {
               
                var title = "[Quản lý danh mục đầu tư TLBonds] Thông báo đến hạn trái tức/lãi " ;
                /*var body = "Hệ thống quản lý tài sản xin gửi tới Quý khách hàng thông tin hợp đồng " + item.BuyAndSellBond.ContractName + " đã đến ngày nhận lợi tức vào ngày " + string.Format("{0:dd/MM/yyyy}", item.IncurredDate) +
                    ". Quý khách hàng có thể truy cập vào hệ thống để kiểm tra và theo dõi tình trạng đầu tư.<br/>Xin cám ơn!";*/
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
                body += "Kính chào: Quý khách (Dear Mr./Ms.)" + " " + item.Customer.FullName;
                body += "</p>";
                body += "<div style='font-size:10px;line-height:10px;height:10px'>";
                body += "</div>";
                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                body += "Công ty cổ phần Đầu tư trái phiếu Thăng Long (TLBONDS) xin trân trọng thông báo và lưu ý khách hàng tiền lãi/trái tức của hợp đồng đã đến kỳ hạn được thụ hưởng.";
                body += "</p>";
                body += "<br>";
                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                body += "-	Mã hợp đồng :" + " " + item.BuyAndSellBond.ContractCode;
                body += "</p>";
                body += "<br/>";
                
                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                body += "-	Tên hợp đồng  :" + " " + item.BuyAndSellBond.ContractName;
                body += "</p>";
                body += "<br/>";

                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                body += "-	Nội dung :" + " " + item.Note;
                body += "</p>";
                body += "<br/>";

                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                body += "-	Giá trị : " + " " + item.AmountOfMoney.Value.ToString("N0", CultureInfo.InvariantCulture) + " "+"đ";
                body += "</p>";
                body += "<br/>";

                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                body += "-	Đã đến hạn hưởng lãi/trái tức vào ngày : " + " " + string.Format("{0:dd/MM/yyyy}", item.IncurredDate);
                body += "</p>";
                body += "<br/>";

                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                body += "Đề nghị quý khách hàng lưu ý trong quá trình giao dịch.";
                body += "</p>";
                body += "<br/>";

                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                body += "Link truy cập hệ thống quản lý danh mục đầu tư: http://quanlydanhmucdautu.tlbonds.vn/" + " ";
                body += "<br/>";
                body += "hoặc truy cập website https://tlbonds.vn/ để sử dụng.";
                body += "</p>";
                body += "<br/>";
                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                body += "Chân thành cảm ơn Quý khách hàng đã quan tâm và sử dụng dịch vụ của TLBonds. <br>";
                body += "Mọi yêu cầu cần giải đáp, Xin Quý khách vui lòng liên hệ với TLBonds.";
                body += "</p>";
                body += "<br/>";
                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                body += "Thông tin liên hệ (Contact Center): <br>";
                body += "Điện thoại: 024.32047782<br>";
                body += "Website: https://www.tlbonds.vn<br>";
                body += " Email: support@tlbonds.vn<br>";
                body += "</p>";
                body += "<br/>";
                body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:bold;font-style: italic;'>CÔNG TY CỔ PHẦN ĐẦU TƯ TRÁI PHIẾU THĂNG LONG (TLBONDS)</p>";
                body += "<img src = '" + getBeSite + "/Content/themes/admin/img/footerMail.jpg' style='max-width:650px' width='100%' />";
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

                var bodySMS = "Quý khách có “hợp đồng” đến hạn nhận lãi tức/trái tức/lãi với số tiền " + item.AmountOfMoney + " vào ngày " + string.Format("{0:dd/MM/yyyy}", item.IncurredDate);

                var checkSendMail = Models.ApplicationService.SendMailSystem(item.BuyAndSellBond.Customer, GetInfoConfigSendMail(), title, body);
                //ApplicationService.SendJsonSMS(item.BuyAndSellBond.Customer.Mobile, bodySMS);
            }
        }

       /* public void SendMailContactDisbursementDetail()
        {
            var checkContactDisbursementDetail = db.ContactDisbursementDetails.Where(
               x => DbFunctions.TruncateTime(x.ImplementationDate) == DbFunctions.TruncateTime(DateTime.Now)
               && x.IsActive != false
               && x.BuyAndSellBond.Customer != null).Include(x => x.BuyAndSellBond).Include(x => x.BuyAndSellBond.Customer).ToList();

            foreach (var item in checkContactDisbursementDetail)
            {
                var title = "[Hệ thống quản lý tài sản] Thông báo đến ngày nhận lợi tức " + item.BuyAndSellBond.ContractName;
                var body = "Hệ thống quản lý tài sản xin gửi tới Quý khách hàng thông tin hợp đồng " + item.BuyAndSellBond.ContractName + " đã đến ngày nhận lợi tức vào ngày " + string.Format("{0:dd/MM/yyyy}", item.ImplementationDate) +
                    ". Quý khách hàng có thể truy cập vào hệ thống để kiểm tra và theo dõi tình trạng đầu tư.<br/>Xin cám ơn!";

                var checkSendMail = ApplicationService.SendMailSystem(item.BuyAndSellBond.Customer, GetInfoConfigSendMail(), title, body);
                ApplicationService.SendJsonSMS(item.BuyAndSellBond.Customer.Mobile, body);
            }
        }
*/
        public ConfigSendMailInfo GetInfoConfigSendMail()
        {
            var getConfigInfo = db.WebConfigs.ToList();

            return new ConfigSendMailInfo
            {
                Host = getConfigInfo.Where(x => x.Key == "email-send-smtp").FirstOrDefault().Value,
                Port = getConfigInfo.Where(x => x.Key == "email-send-port").FirstOrDefault().Value,
                SendFrom = getConfigInfo.Where(x => x.Key == "email-send").FirstOrDefault().Value,
                EmailPass = getConfigInfo.Where(x => x.Key == "email-send-password").FirstOrDefault().Value,
                Ssl = getConfigInfo.Where(x => x.Key == "email-send-ssl").FirstOrDefault().Value
            };
        }

    }
}
