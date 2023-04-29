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
using Tx.Windows;
using System.Web.Hosting;
using System.Net;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Text;

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
        public void GetDataFromLogFile()
        {
            var uri = "/Uploads";
            var uriLogFile = uri + "/log_data.log";
            WebClient client = new WebClient();
            client.Credentials = new NetworkCredential("uainpsgt", "6gppUx3H56");
            var linkLog = db.WebConfigs.Where(x => x.Key == "LinkLog").Select(x => x.Value).FirstOrDefault();
            client.DownloadFile( linkLog != null ? linkLog : "ftp://uainpsgt@103.130.212.186/logs/iis/W3SVC50/u_extend1.log", HostingEnvironment.MapPath(uriLogFile));
            var lastDataTime = db.LogData.OrderByDescending(x => x.date).Select(x=>x.date).FirstOrDefault();
            if(lastDataTime == null)
            {
                lastDataTime = DateTime.Now.AddMonths(-1);
            }
            var iisLog = W3CEnumerable.FromFile(HostingEnvironment.MapPath(uriLogFile));
            List<LogData> listLog = new List<LogData>();
            foreach (var item in iisLog)
            {
                if (lastDataTime != null && item.dateTime.ToLocalTime() > lastDataTime)
                {
                    int tempValscStatus;
                    int tempValSubstatus;
                    int tempValscBytes;
                    int tempValcsBytes;
                    int tempValtimeTaken;
                    long tempValWin32Status;
                    var log = new LogData()
                    {
                        date = item.dateTime.ToLocalTime(),
                        sIp = item.s_ip,
                        csMethod = item.cs_method,
                        csUriStem = item.cs_uri_stem,
                        csUriQuery = item.cs_uri_query,
                        sPort = item.s_port,
                        csUsername = item.cs_username,
                        cIp = item.c_ip,
                        csVersion = item.cs_version,
                        csUserAgent = item.cs_User_Agent,
                        csReferer = item.cs_Referer,
                        csHost = item.cs_host,
                        scStatus = Int32.TryParse(item.sc_status, out tempValscStatus) ? tempValscStatus : (int?)null,
                        scSubstatus = Int32.TryParse(item.sc_substatus, out tempValSubstatus) ? tempValSubstatus : (int?)null,
                        scWin32Status = long.TryParse(item.sc_win32_status, out tempValWin32Status) ? tempValWin32Status : (long?)null,
                        scBytes = Int32.TryParse(item.sc_bytes, out tempValscBytes) ? tempValscBytes : (int?)null,
                        csBytes = Int32.TryParse(item.cs_bytes, out tempValcsBytes) ? tempValcsBytes : (int?)null,
                        timeTaken = Int32.TryParse(item.time_taken, out tempValtimeTaken) ? tempValtimeTaken : (int?)null
                    };
                    listLog.Add(log);
                }

            }
            db.LogData.AddRange(listLog);
            db.SaveChanges();


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

        public List<LogData> GetLogData(DateTime startTime, DateTime endTime)
        {

            return db.LogData
                .Where(log => log.date >= startTime && log.date <= endTime)
                .ToList();

        }

        //======================== check DDOS attack ================================

        public Dictionary<string, int> AnalyzeLogData(List<LogData> logData, int threshold)
        {
            Dictionary<string, int> ipRequestCounts = new Dictionary<string, int>();
            Dictionary<string, DateTime> lastRequestTimes = new Dictionary<string, DateTime>();

            foreach (var log in logData)
            {
                string ipAddress = log.cIp;

                if (!lastRequestTimes.ContainsKey(ipAddress))
                {
                    lastRequestTimes[ipAddress] = log.date;
                    ipRequestCounts[ipAddress] = 1;
                }
                else
                {
                    TimeSpan timeSinceLastRequest = log.date - lastRequestTimes[ipAddress];

                    if (timeSinceLastRequest.TotalSeconds > 60)
                    {
                        lastRequestTimes[ipAddress] = log.date;
                        ipRequestCounts[ipAddress] = 1;
                    }
                    else
                    {
                        ipRequestCounts[ipAddress]++;
                    }
                }
            }

            Dictionary<string, int> abnormalIps = ipRequestCounts
                .Where(ipCount => ipCount.Value > threshold)
                .ToDictionary(ipCount => ipCount.Key, ipCount => ipCount.Value);

            return abnormalIps;
        }

/*        public Dictionary<string, int> AnalyzeLogData(List<LogData> logData, int threshold)
        {
            Dictionary<string, int> ipRequestCounts = new Dictionary<string, int>();

            foreach (var log in logData)
            {
                string ipAddress = log.cIp;

                if (ipRequestCounts.ContainsKey(ipAddress))
                {
                    ipRequestCounts[ipAddress]++;
                }
                else
                {
                    ipRequestCounts[ipAddress] = 1;
                }
            }

            Dictionary<string, int> abnormalIps = ipRequestCounts
                .Where(ipCount => ipCount.Value > threshold)
                .ToDictionary(ipCount => ipCount.Key, ipCount => ipCount.Value);

            return abnormalIps;
        }*/

        public void CheckDDOSAttack()
        {
            DateTime startTime = DateTime.Now.AddMinutes(-10); // Lấy dữ liệu nhật ký trong 10 phút trước
            DateTime endTime = DateTime.Now;
            int threshold = 100; // Ngưỡng cho phép tối đa 100 yêu cầu trong 1 phút
            var thresholdString = db.WebConfigs.Where(x => x.Key == "threshold").Select(x => x.Value).FirstOrDefault();
            if(thresholdString != null)
            {
                threshold = Int16.Parse(thresholdString);
            }


            List<LogData> logData = GetLogData(startTime, endTime);
            Dictionary<string, int> abnormalIps = AnalyzeLogData(logData, threshold);
            if(abnormalIps.Count() > 0)
            {
                foreach(var item in abnormalIps)
                {
                    string titleAlert = "Vào hồi " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " Website của quý khách vừa bị tấn công DDOS";
                    string detailAlert = "Địa chỉ IP máy tấn công: " + item.Key + "<br/>" + "Số lần truy cập trong khoảng thời gian 1 phút : " + item.Value;
                    string titleMail = "[phantichnhatky.xyz] Cảnh báo trang web của bạn bị tấn công DDOS";
                    SendMailSystem(GetInfoConfigSendMail(), CreateTitleAndBody(titleAlert, detailAlert, titleMail));
                    var notification = new Notification
                    {
                        CreatedAt = DateTime.Now,
                        Title = titleMail,
                        Content = detailAlert,
                        Type = "tấn công DDOS",
                        IsRead = false
                    };
                    db.Notification.Add(notification);
                    db.SaveChanges();
                }
            }

        }

        //======================== End check DDOS attack ================================



        //======================== check XSS attack ================================

        public bool IsXssAttack(LogData logData)
        {
            string uriStem = logData.csUriStem;
            bool isXssAttack = false;

            // Kiểm tra chuỗi đầu vào có chứa các ký tự đặc biệt như <, >, &, ' hay " không.
            if (uriStem.Contains("<") || uriStem.Contains(">") || uriStem.Contains("&") || uriStem.Contains("'") || uriStem.Contains("\""))
            {
                isXssAttack = true;
            }
            else
            {
                // Kiểm tra chuỗi đầu vào có chứa các thẻ HTML không.
                string[] htmlTags = new string[] { "<script", "<img", "<iframe", "<form", "<a", "<body", "<html", "<meta" };
                foreach (string tag in htmlTags)
                {
                    if (uriStem.Contains(tag))
                    {
                        isXssAttack = true;
                        break;
                    }
                }
            }

            return isXssAttack;
        }

        public void CheckXSSAttack()
        {
            DateTime startTime = DateTime.Now.AddMinutes(-10); // Lấy dữ liệu nhật ký trong 10 phút trước
            DateTime endTime = DateTime.Now;
            List<LogData> logData = GetLogData(startTime, endTime);
            foreach(var item in logData)
            {
                bool result = IsXssAttack(item);
                if (result)
                {
                    string titleAlert = "Vào hồi " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " Website của quý khách vừa bị tấn công XSS";
                    string detailAlert = "Địa chỉ IP máy tấn công: " + item.cIp + "<br/>";
                    string titleMail = "[phantichnhatky.xyz] Cảnh báo trang web của bạn bị tấn công XSS";
                    SendMailSystem(GetInfoConfigSendMail(), CreateTitleAndBody(titleAlert, detailAlert, titleMail));
                    var notification = new Notification
                    {
                        CreatedAt = DateTime.Now,
                        Title = titleMail,
                        Content = detailAlert,
                        Type = "tấn công XSS",
                        IsRead = false
                    };
                    db.Notification.Add(notification);
                    db.SaveChanges();
                }
            }

        }


        //======================== End check XSS attack ================================

        //======================== Check Brute force attack ================================



/*        private bool IsPotentialBruteForceAttack(string requestUrl, string clientIp, string username)
        {
            // Check if request URL or user agent contains potential Brute force attack vectors
            if (requestUrl.Contains("/login") && !string.IsNullOrEmpty(username))
            {
                // Count number of login attempts from this IP address in the last 10 minutes
                int numLoginAttempts = GetNumLoginAttempts(clientIp, DateTime.Now.AddMinutes(-10));

                // If more than 5 login attempts in 1 minute, potential Brute force attack
                if (numLoginAttempts > 5)
                {
                    return true;
                }
            }

            return false;
        }*/


/*        private int GetNumLoginAttempts(string clientIp, DateTime startTime)
        {
            // Query database or log file to count number of login attempts from this IP address since start time
            int numLoginAttempts = 0;
            var endTime = DateTime.Now;
            var loginAttempts = db.LogData.Where(x => x.cIp == clientIp && x.date > startTime && x.date <= endTime);
            var minuteCount = new int[10];

            // count number of login attempts per minute
            foreach (var attempt in loginAttempts)
            {
                var minute = (int)(endTime - attempt.date).TotalMinutes;
                if (minute < 10) minuteCount[minute]++;
            }

            // check if any minute has more than 5 login attempts
            for (int i = 0; i < minuteCount.Length; i++)
            {
                if (minuteCount[i] > 5)
                {
                    numLoginAttempts += minuteCount[i];
                }
            }

            return numLoginAttempts;
        }*/
        //code tối ưu
        private int GetNumLoginAttempts(string clientIp, DateTime startTime)
        {
            var endTime = DateTime.Now;
            var loginAttempts = db.LogData
                .Where(x => x.cIp == clientIp && x.date > startTime && x.date <= endTime)
                .GroupBy(x => (int)(endTime - x.date).TotalMinutes / 1)
                .Select(g => g.Count())
                .ToList();

            return loginAttempts.Count > 0 ? loginAttempts.Max() : 0;
        }

        private Tuple<bool, string> IsPotentialBruteForceAttack(List<LogData> logData)
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddMinutes(-10);

            // Group log data by IP and minute of login attempt
            var loginGroups = logData
                .Where(x => x.csUriStem.Contains("/login") && !string.IsNullOrEmpty(x.csUsername))
                .Where(x => x.date >= startTime && x.date <= endTime)
                .GroupBy(x => new { x.cIp, Minute = x.date.Minute })
                .Select(g => new { g.Key.cIp, g.Key.Minute, Count = g.Count() });

            // Check for potential brute force attacks
            foreach (var group in loginGroups)
            {
                if (group.Count > 5)
                {
                    return Tuple.Create(true, group.cIp);
                }
            }

            return Tuple.Create(false, "");
        }


        public void CheckBruteForceAttack()
        {
            DateTime startTime = DateTime.Now.AddMinutes(-10); // Lấy dữ liệu nhật ký trong 10 phút trước
            DateTime endTime = DateTime.Now;
            List<LogData> logData = GetLogData(startTime, endTime);
            var result = IsPotentialBruteForceAttack(logData);
            if (result.Item1 == true)
            {
                string titleAlert = "Vào hồi " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " Website của quý khách vừa bị tấn công Brute Force";
                string detailAlert = "Địa chỉ IP máy tấn công: " + result.Item2 + "<br/>";
                string titleMail = "[phantichnhatky.xyz] Cảnh báo trang web của bạn bị tấn công Brute Force";
                SendMailSystem(GetInfoConfigSendMail(), CreateTitleAndBody(titleAlert, detailAlert, titleMail));
                var notification = new Notification
                {
                    CreatedAt = DateTime.Now,
                    Title = titleMail,
                    Content = detailAlert,
                    Type = "tấn công Brute Force",
                    IsRead = false
                };
                db.Notification.Add(notification);
                db.SaveChanges();
            }
            

        }
        //======================== End check Brute force attack ================================
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
        public MailTitleAndBodyModel CreateTitleAndBody(string titleAlert, string detailAlert, string title)
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
            body += "Kính chào Quý khách";
            body += "</p>";
            body += "<div style='font-size:10px;line-height:10px;height:10px'>";
            body += "</div>";
            body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:bold;line-height:24px'>";
            body += "Website phantichnhatky.xyz xin thông báo: " + titleAlert;
            body += "</p>";
            body += "<br>";

            body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
            body += detailAlert;
            body += "</p>";
            body += "<br/>";

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
