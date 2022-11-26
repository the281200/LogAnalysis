
using Common;
using Microsoft.Web.WebPages.OAuth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WEB.Filters;
using WEB.Models;
using WEB.WebHelpers;
using WebMatrix.WebData;
using WebModels;

namespace WEB.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AccountController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        //
        // GET: /Admin/Account/

        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.Languages = this.Language;
            ViewBag.ReturnUrl = returnUrl;
            LoginModel model = new LoginModel();
            if (Request.Cookies["Login"] != null)
            {
                model.UserName = Request.Cookies["Login"].Values["UserName"];
                model.Password = Request.Cookies["Login"].Values["Password"];
            }
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var userProfile = db.UserProfiles.AsNoTracking().ToList();
                var checkAccount = userProfile.Where(p => p.UserName == model.UserName && p.Type != null && p.Type == (int)(TypeAccount.Admin)).Any();
                if (!checkAccount)
                {
                    ViewBag.Languages = this.Language;
                    ModelState.AddModelError("", AccountResources.LoginIncorrect);
                    return View(model);
                }
                else
                {
                    if (WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
                    {
                        FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                        Session["UserName"] = model.UserName;
                        Session["Password"] = model.Password;

                        if (model.RememberMe)
                        {
                            HttpCookie cookie = new HttpCookie("Login");
                            cookie.Values.Add("UserName", model.UserName);
                            cookie.Values.Add("Password", model.Password);
                            cookie.Expires = DateTime.Now.AddDays(60);
                            Response.Cookies.Add(cookie);
                        }

                        List<string> role = Roles.GetRolesForUser(model.UserName).ToList();
                        var user = db.UserProfiles.Where(x => x.UserName == model.UserName).FirstOrDefault();
                        UserInfoHelper.SetUserData(user);
                        if (role.Contains("Lái xe"))
                        {
                            return RedirectToAction("Index", "DrivePlan");
                        }
                        else
                        {
                            return RedirectToLocal(returnUrl);
                        }
                    }
                    ViewBag.Languages = this.Language;
                    ModelState.AddModelError("", AccountResources.LoginIncorrect);
                    return View(model);
                }
            }

            /*if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                Session["UserName"] = model.UserName;
                Session["Password"] = model.Password;

                if (model.RememberMe)
                {
                    HttpCookie cookie = new HttpCookie("Login");
                    cookie.Values.Add("UserName", model.UserName);
                    cookie.Values.Add("Password", model.Password);
                    cookie.Expires = DateTime.Now.AddDays(60);
                    Response.Cookies.Add(cookie);
                }

                List<string> role = Roles.GetRolesForUser(model.UserName).ToList();
                var user = db.UserProfiles.Where(x => x.UserName == model.UserName).FirstOrDefault();
                UserInfoHelper.SetUserData(user);
                if (role.Contains("Lái xe"))
                {
                    return RedirectToAction("Index", "DrivePlan");
                }
                else
                {
                    return RedirectToLocal(returnUrl);
                }
            }*/

            ViewBag.Languages = this.Language;
            ModelState.AddModelError("", AccountResources.LoginIncorrect);
            return View(model);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Denied()
        {
            return View();
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult Logout()
        {
            Session["AdminSite-" + Culture + "-" + HttpContext.User.Identity.Name] = null;
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult UserProfile()
        {
            var user = db.Set<UserProfile>().Find(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.Avatar = user.Avatar == null ? "/Images/no_avatar_60x60.jpg" : user.Avatar;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserProfile([Bind(Exclude = "")] UserProfile model, HttpPostedFileBase imagefile)
        {
            if (ModelState.IsValid)
            {
                if (imagefile != null)
                {
                    var name = imagefile.FileName;
                    string extension = Path.GetExtension(name);
                    var newName = model.UserName + extension;
                    var dir = new System.IO.DirectoryInfo(Server.MapPath("/content/uploads/avatars/"));
                    if (!dir.Exists) dir.Create();
                    var uri = "/content/uploads/avatars/" + newName;
                    imagefile.SaveAs(HttpContext.Server.MapPath(uri));
                    try
                    {
                        if (ImageTools.ValidateImage(System.Web.HttpContext.Current.Server.MapPath(uri)))
                        {
                            var result = ImageTools.ResizeImage(Server.MapPath(uri), Server.MapPath(uri), 240, 240, true, 80);
                            model.Avatar = uri;
                        }
                        else
                        {
                            Utility.DeleteFile(uri);
                        }
                    }
                    catch (Exception)
                    { }
                }

                db.UserProfiles.Attach(model);
                db.Entry(model).Property(a => a.UserName).IsModified = false;
                db.Entry(model).Property(a => a.FullName).IsModified = true;
                db.Entry(model).Property(a => a.Email).IsModified = true;
                db.Entry(model).Property(a => a.Mobile).IsModified = true;
                db.Entry(model).Property(a => a.Avatar).IsModified = true;
                db.SaveChanges();
            }
            ViewBag.Avatar = model.Avatar == null ? "/Images/no_avatar_60x60.jpg" : model.Avatar;
            ViewBag.Alert = "<div class='alert alert-info'>Thông tin cá nhân của bạn đã được thay đổi thành công !</div>";
            return View();
        }

        public ActionResult _Info()
        {
            var user = db.Set<UserProfile>().Find(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.Displayname = user.FullName == null ? "Noname" : user.FullName;
            
            var linkAvt = "/Images/no_avatar.jpg";
            if (user.Avatar != null)
            {
                linkAvt = user.Avatar;
            }  
            
            ViewBag.Avatar = linkAvt;
            ViewBag.Position = user.Position;
            return PartialView();
        }

        public ActionResult _Notification()
        {
            var user = db.Set<UserProfile>().Find(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.IsRead = (user.IsReadNotification != true) ? false : true;
            var totalUnRead = user.UnReadNotiCount ?? default;
            ViewBag.TotalCount = totalUnRead;

            return PartialView();
        }

        public PartialViewResult _ChangePassword()
        {
            return PartialView();
        }

        [HttpPost]
        public JsonResult _ChangePassword(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));

            if (!hasLocalAccount || !ModelState.IsValid)
            {
                return Json(new { Success = false, Error = "The current password is incorrect or the new password is invalid." });
            }

            bool changePasswordSucceeded;

            try
            {
                changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
            }
            catch (Exception)
            {
                changePasswordSucceeded = false;
            }

            return Json(new { Success = changePasswordSucceeded, Error = changePasswordSucceeded ? "" : "The current password is incorrect or the new password is invalid." });
        }

        [AllowAnonymous]
        public ActionResult _PVForgotPasswordModal()
        {
            return View();
        }

        /* ==================== Send Mail ===================*/

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public JsonResult ForgotPassword(MailForgotModel model)
        {
            var user = db.UserProfiles.Where(x => x.Email != null && x.Email.Trim() == model.Email.Trim() && x.Type != null
            && x.Type == (int)TypeAccount.Admin).FirstOrDefault();

            if (user == null || user.Type != (int)TypeAccount.Admin)
            {
                return Json(new { Success = false, Error = "Bạn đã nhập sai email vui lòng kiểm tra lại" });
            }

            var url = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + ":" + Request.Url.Port;

            var checkSendMail = ForgotPassword(user, url);

            return Json(new { Success = true, Error = "" });
        }

        public bool ForgotPassword(UserProfile user, string url)
        {
            var configInfo = GetInfoConfigSendMail();
            var forgot = AddForgorPasswordToken(user);

            configInfo.EmailTo = user.Email;

            var link = url + "/Account/ChangePasswordForgot?token=";
            var linkChange = link + forgot.RandomString;

            var checkSendMail = SendMailLinkForgot(configInfo, linkChange, user);


            return true;
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

        public ForgotPassWord AddForgorPasswordToken(UserProfile user)
        {
            // set mail and expried Time 
            ForgotPassWord forgotPassWord = new ForgotPassWord();
            forgotPassWord.RandomString = UniqueKeyGenerator.RNGTicks(10);
            forgotPassWord.ExpiredTime = DateTime.Now.AddMinutes(10);
            forgotPassWord.UserID = user.UserId;
            forgotPassWord.Used = false;

            var checkUserID = db.ForgotPassWords.Where(x => x.UserID == forgotPassWord.UserID && x.Used == false);

            foreach (var item in checkUserID)
            {
                item.Used = true;
            }

            db.ForgotPassWords.Add(forgotPassWord);
            db.SaveChanges();

            return forgotPassWord;
        }

        public bool SendMailLinkForgot(ConfigSendMailInfo configSendMailInfo, string link, UserProfile user )
        {
            bool _return = false;
            try
            {
                var getBeSite = db.WebConfigs.Where(x => x.Key == "SiteBE").Select(x => x.Value).FirstOrDefault().ToString();
                var client = new SmtpClient(configSendMailInfo.Host, int.Parse(configSendMailInfo.Port));
                client.UseDefaultCredentials = false;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(configSendMailInfo.SendFrom, configSendMailInfo.EmailPass);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)48 | (SecurityProtocolType)192 |
                (SecurityProtocolType)768 | (SecurityProtocolType)3072;
                var message = new MailMessage();
                message.From = new MailAddress(configSendMailInfo.SendFrom, configSendMailInfo.SendFrom);
                message.Subject += "[Quản lý danh mục đầu tư TLBonds] Thiết lập lại mật khẩu";
                message.To.Add(new MailAddress(configSendMailInfo.EmailTo));
                
                message.Body += "<table align='center' border='0' cellpadding='0' cellspacing='0' lang='container' style='max-width:700px' width='100%'>";
                message.Body += "<tbody>";
                message.Body += "<tr>";
                message.Body += "<td bgcolor='#f0f0f0' style='background:#f0f0f0' valign='top' width='100%'>";
                message.Body += "<table border='0' cellpadding='0' cellspacing='0' lang='main_content' style='width:100%' width='100%'>";
                message.Body += "<tbody>";
                message.Body += "<tr>";
                message.Body += "<td valign='top' width='100%'>";
                message.Body += "<div style='font-size:30px;line-height:30px;height:30px'>";
                message.Body += "</div>";
                message.Body += "</td>";
                message.Body += "</tr>";
                message.Body += "<tr>";
                message.Body += "<td valign = 'top' width = '100%'>";
                message.Body += "<table border='0' cellpadding='0' cellspacing='0' style='width:100%' width='100%'>";
                message.Body += "<tbody>";
                message.Body += "<tr>";
                message.Body += "<td style='width:20px' width='20'>";
                message.Body += "<div lang='space40'>";
                message.Body += "</div>";
                message.Body += "</td>";
                message.Body += "<td valign='top'>";
                message.Body += "<p style='margin:0;padding:0;font-size:18px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:bold;font-style: italic;'>";
                message.Body += "Kính chào: Quý khách (Dear Mr./Ms.)"+" "+ user.FullName;
                message.Body += "</p>";
                message.Body += "<div style='font-size:10px;line-height:10px;height:10px'>";
                message.Body += "</div>";
                message.Body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                message.Body += "Công ty cổ phần Đầu tư trái phiếu Thăng Long (TLBONDS) xin trân trọng thông báo Mật khẩu tài khoản của Quý khách hàng trên hệ thống 'Quản lý danh mục đầu tư TLBonds' của TLBonds đã thiết lập lại. ";
                message.Body += "</p>";
                message.Body += "<br>";
                message.Body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                message.Body += "Xin mời Quý khách đặt lại mật khẩu tài khoản của mình theo đường dẫn dưới đây:" + " "+ link;
                message.Body += "</p>";
                message.Body += "<br/>";
                message.Body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                message.Body += "Link truy cập hệ thống quản lý danh mục đầu tư: http://quanlydanhmucdautu.tlbonds.vn/" + " ";
                message.Body += "<br/>";
                message.Body += "hoặc truy cập website https://tlbonds.vn/ để sử dụng.";
                message.Body += "</p>";
                message.Body += "<br/>";
                message.Body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                message.Body += "Chân thành cảm ơn Quý khách hàng đã quan tâm và sử dụng dịch vụ của TLBonds. <br>";
                message.Body += "Mọi yêu cầu cần giải đáp, Xin Quý khách vui lòng liên hệ với TLBonds.";
                message.Body += "</p>";
                message.Body += "<br/>";
                message.Body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                message.Body += "Thông tin liên hệ (Contact Center): <br>";
                message.Body += "Điện thoại: 024.32047782<br>";
                message.Body += "Website: https://www.tlbonds.vn<br>";
                message.Body += " Email: support@tlbonds.vn<br>";
                message.Body += "</p>";
                message.Body += "<br/>";
                message.Body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:bold;font-style: italic;'>CÔNG TY CỔ PHẦN ĐẦU TƯ TRÁI PHIẾU THĂNG LONG (TLBONDS)</p>";
                message.Body += "<img src = '"+ getBeSite + "/Content/themes/admin/img/footerMail.jpg' style='max-width:650px' width='100%' />";
                message.Body += "</td>";
                message.Body += "<td style='width:20px' width='20'>";
                message.Body += "<div lang='space40'>";
                message.Body += "</div>";
                message.Body += "</td>";
                message.Body += "</tr>";
                message.Body += "</tbody>";
                message.Body += "</table>";
                message.Body += "</td>";
                message.Body += "</tr>";
                message.Body += "<tr>";
                message.Body += "<td valign='top' width='100%'>";
                message.Body += "<div style='font-size:30px;line-height:30px;height:30px'>";
                message.Body += "</div>";
                message.Body += "</td>";
                message.Body += "</tr>";
                message.Body += "</tbody>";
                message.Body += "</table>";
                message.Body += "</td>";
                message.Body += "</tr>";
                message.Body += "</tbody>";
                message.Body += "</table>";


                message.IsBodyHtml = true;
                message.BodyEncoding = Encoding.UTF8;

                try
                {

                    var mailThread = new Thread(new ThreadStart(() =>
                    {
                        client.Send(message);
                    }));

                    mailThread.Start();
                }
                catch (Exception ex)
                {
                }

                _return = true;
            }
            catch (Exception ex)
            {
                _return = false;
            }

            return _return;
        }


        /* ==================== Send Mail ===================*/

        [AllowAnonymous]

        protected string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (var sw = new ApplicationService.StringWriterWithEncoding(Encoding.Unicode))
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                var stringBuilder = sw.GetStringBuilder();

                return stringBuilder.ToString();
            }
        }
        [AllowAnonymous]

        public static WebConfig getconfig(string key)
        {
            WebContext db = new WebContext();

            var config = (from c in db.WebConfigs
                          where c.Key.Equals(key)
                          select c);

            return config.FirstOrDefault();
        }

        [HttpPost]
        public JsonResult ChangeStatusReadNoti()
        {
            try
            {
                var getUser = db.Set<UserProfile>().Find(WebSecurity.GetUserId(User.Identity.Name));
                getUser.IsReadNotification = true;
                getUser.UnReadNotiCount = 0;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }

            return Json(new { success = true });
        }

    }
}
