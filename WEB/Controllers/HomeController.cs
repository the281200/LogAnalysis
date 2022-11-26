using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WEB.Filters;
using WebMatrix.WebData;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using WebModels;
using Newtonsoft.Json.Linq;
using System.Data;
using System;
using System.Collections.Generic;
using Common;
using Kendo.Mvc;
using WEB.Models;
using System.Data.Entity;
using System.Configuration;
using System.Transactions;
using System.Web.Routing;
using System.IO;
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
            var checkUserLogin = WebSecurity.CurrentUserId > 0 && WebSecurity.CurrentUserName != null;
            var home = new List<string> { "home", "index", "home.html", "index.html", "trangchu", "trang-chu", "trangchu.html", "trang-chu.html" };

            if (checkUserLogin)
            {
                var userInfo = db.UserProfiles.Find(WebSecurity.CurrentUserId);
                if (userInfo.Type == null || (userInfo.Type != null && userInfo.Type != (int)TypeAccount.Customer))
                {
                    FormsAuthentication.SignOut();
                    return RedirectToAction("Login", "Account", new { area = "" });
                }
                ViewBag.UserId = WebSecurity.CurrentUserId;
                ViewBag.UserName = WebSecurity.CurrentUserName;
                ViewBag.Controller = (controller == null || controller == "") ? "Home" : controller;

                if (id.HasValue)
                {
                    var module = db.Set<WebModule>().Find(id);
                    TempData["WebModule"] = module;
                    ViewBag.Page = page;
                    return View(module);

                }
                else if (!string.IsNullOrEmpty(uid))
                {

                    var module = (from x in db.WebModules
                                  where
                                      (x.UID.ToLower().Equals(uid.ToLower()))
                                  select x).AsNoTracking().FirstOrDefault();
                    return View(module);
                }
                else
                {
                    var module = (from x in db.WebModules
                                  where
                                      (x.UID == null || home.Contains(x.ContentType.ID.ToLower()))
                                  select x).AsNoTracking().FirstOrDefault();
                    ViewBag.dashboard = 1;
                    return View(module);
                }

            }

            return RedirectToAction("Login", "Account", new { area = "" });
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
            var getListDisbursementid = db.IncurredPurchases.Where(x => x.ContactDisbursementId != null).Select(x => x.ContactDisbursementId).ToList();
            //today warning

            var getNumEarningDueWarning = db.InterestPaymentPeriods.Where(y => (!getInterestPaymentPeriodsId.Contains(y.Id)) && y.BuyAndSellBondId != 0 && DbFunctions.TruncateTime(y.CreatedAt) == DateTime.Today && y.IsActive != false && y.InterestPaymentDate != null && DbFunctions.TruncateTime(y.InterestPaymentDate) < DbFunctions.TruncateTime(dateCompare)).Count();

            var getNumSettlementWarning = db.IncurredPurchases.Where(y => y.TransactionType == 2 && y.IsActive != false && DbFunctions.TruncateTime(y.CreatedAt) == DateTime.Today && y.IncurredDate != null && DbFunctions.TruncateTime(y.IncurredDate) < DbFunctions.TruncateTime(dateCompare)).Count();

            var getNumDisbursementWarning = db.ContactDisbursementDetails.Where(y => (!getListDisbursementid.Contains(y.Id)) && y.BuyAndSellBondId != 0 && DbFunctions.TruncateTime(y.CreatedAt) == DateTime.Today && y.IsActive != false && y.ImplementationDate != null && DbFunctions.TruncateTime(y.ImplementationDate) < DbFunctions.TruncateTime(dateCompare)).Count();

            var sumTodayWarning = getNumEarningDueWarning + getNumSettlementWarning + getNumDisbursementWarning;

            //last 5 days
            var getNumEarningDueWarningLast5Days = db.InterestPaymentPeriods.Where(y => (!getInterestPaymentPeriodsId.Contains(y.Id)) && y.BuyAndSellBondId != 0 && y.IsActive != false && y.InterestPaymentDate != null && DbFunctions.TruncateTime(y.InterestPaymentDate) == last5days).Count();

            var getNumSettlementWarningLast5Days = db.IncurredPurchases.Where(y => y.TransactionType == 2 && y.IsActive != false && y.IncurredDate != null && DbFunctions.TruncateTime(y.IncurredDate) == last5days).Count();

            var getNumDisbursementWarningLast5Days = db.ContactDisbursementDetails.Where(y => (!getListDisbursementid.Contains(y.Id)) && y.BuyAndSellBondId != 0 && y.IsActive != false && y.ImplementationDate != null && DbFunctions.TruncateTime(y.ImplementationDate) == last5days).Count();

            var sumLast5DaysWarning = getNumEarningDueWarningLast5Days + getNumSettlementWarningLast5Days + getNumDisbursementWarningLast5Days;

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
            SendMailContactDisbursementDetail();

            return RedirectToAction("Index");
        }

        public void SendMailContractSettlement()
        {
            var checkContractSettlement = db.IncurredPurchases.Where(
               x => DbFunctions.TruncateTime(x.IncurredDate) == DbFunctions.TruncateTime(DateTime.Now)
               && x.IsActive != false
               && (x.TransactionType == (int)(TypeTransaction.ContractSettlementByTime) || x.TransactionType == (int)(TypeTransaction.ContractSettlementOnTime)) && x.BuyAndSellBond.Customer != null).Include(x => x.BuyAndSellBond).Include(x => x.BuyAndSellBond.Customer).ToList();
            foreach (var item in checkContractSettlement)
            {
                var title = "[Hệ thống quản lý tài sản] Thông báo đến hạn tất toán hợp đồng " + item.BuyAndSellBond.ContractName;
                var body = "Hệ thống quản lý tài sản xin gửi tới Quý khách hàng thông tin hợp đồng " + item.BuyAndSellBond.ContractName + " đã đến hạn tất toán vào ngày " + string.Format("{0:dd/MM/yyyy}", item.IncurredDate) +
                    ". Quý khách hàng có thể truy cập vào hệ thống để kiểm tra và theo dõi tình trạng đầu tư.<br/>Xin cám ơn!";

                var checkSendMail = Models.ApplicationService.SendMailSystem(item.BuyAndSellBond.Customer, GetInfoConfigSendMail(), title, body);
                ApplicationService.SendJsonSMS(item.BuyAndSellBond.Customer.Mobile, body);

            }
        }

        public void SendMailContractGetInterest()
        {
            var checkContractGetInterest = db.IncurredPurchases.Where(
               x => DbFunctions.TruncateTime(x.IncurredDate) == DbFunctions.TruncateTime(DateTime.Now)
               && x.IsActive != false
               && (x.TransactionType == (int)(TypeTransaction.GetInterestByTime) || x.TransactionType == (int)(TypeTransaction.GetInterestOntime)) && x.BuyAndSellBond.Customer != null).Include(x => x.BuyAndSellBond).Include(x => x.BuyAndSellBond.Customer).ToList();
            foreach (var item in checkContractGetInterest)
            {
                var title = "[Hệ thống quản lý tài sản] Thông báo đến ngày nhận lợi tức " + item.BuyAndSellBond.ContractName;
                var body = "Hệ thống quản lý tài sản xin gửi tới Quý khách hàng thông tin hợp đồng " + item.BuyAndSellBond.ContractName + " đã đến ngày nhận lợi tức vào ngày " + string.Format("{0:dd/MM/yyyy}", item.IncurredDate) +
                    ". Quý khách hàng có thể truy cập vào hệ thống để kiểm tra và theo dõi tình trạng đầu tư.<br/>Xin cám ơn!";

                var checkSendMail = Models.ApplicationService.SendMailSystem(item.BuyAndSellBond.Customer, GetInfoConfigSendMail(), title, body);
                ApplicationService.SendJsonSMS(item.BuyAndSellBond.Customer.Mobile, body);
            }
        }

        public void SendMailContactDisbursementDetail()
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
