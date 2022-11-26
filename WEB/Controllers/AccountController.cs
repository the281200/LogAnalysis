using Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
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

namespace WEB.Controllers
{
    public class AccountController : Controller
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            var userName = WebSecurity.CurrentUserName;
            if (userName != null && userName != "")
            {
                return RedirectToAction("Index", "Home");
            }

            HttpCookie user = Request.Cookies["Login"];
            var captchasitekey = ConfigurationManager.AppSettings.Get("sitekey");
            ViewBag.sitekey = captchasitekey;
            var model = new LoginCustomerModel();
            if (user != null)
            {
                model.Email = user["UserName"].ToString();
                model.Password = user["Password"].ToString();
                ViewBag.Password = model.Password;
                return View(model);
            }
            return View(model);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            Session["AdminSite-"  + "-" + HttpContext.User.Identity.Name] = null;
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
        public ActionResult SendMailForgotPass()
        {
            var model = new MailForgotModel();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public JsonResult SendMailForgotPost(MailForgotModel model)
        {
            var user = db.UserProfiles.Where(x => x.Email != null && x.Email.Trim() == model.Email.Trim() && x.Type != null
            && x.Type == (int)TypeAccount.Customer).FirstOrDefault();

            if (user == null || user.Type != (int)TypeAccount.Customer)
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

            var checkSendMail = SendMailLinkForgot(configInfo, linkChange);


            return true;
        }

        [AllowAnonymous]
        public PartialViewResult ChangePasswordForgot(string token)
        {
            var timeNow = DateTime.Now;
            var user = new UserProfile();
            var userID = db.ForgotPassWords.Where(x => x.RandomString == token && DateTime.Compare(timeNow, x.ExpiredTime) < 0).FirstOrDefault();
            if (userID == null)
            {
                ViewBag.UserName = null;
            }
            else
            {
                user = db.UserProfiles.Where(x => x.UserId == userID.UserID).FirstOrDefault();
                ViewBag.UserName = user.UserName;
                ViewBag.Token = token;
            }

            return PartialView();
        }

        [AllowAnonymous]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult ChangePasswordForgot(LocalPasswordModel model, string userName, string token)
        {
            bool changePasswordSucceeded;
            try
            {
                string tokenResetPassword = WebSecurity.GeneratePasswordResetToken(userName, 30);
                changePasswordSucceeded = WebSecurity.ResetPassword(tokenResetPassword, model.NewPassword);
                var item = db.ForgotPassWords.Where(x => x.RandomString == token).FirstOrDefault();
                item.Used = true;
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
            }

            catch (Exception)
            {
                changePasswordSucceeded = false;
            }

            if (changePasswordSucceeded)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult InviteCustomer(string stringInvite)
        {
            var timeNow = DateTime.Now;
            var user = db.UserProfiles.Where(x => x.InviteCode.Trim() == stringInvite.Trim() && x.InviteCode != null && x.IsCustomerActive != true).FirstOrDefault();
            if(user != null) ViewBag.UserName = user.UserName;
            else return RedirectToAction("Login", "Account");
            return PartialView();
        }

        [AllowAnonymous]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult InviteCustomer(LocalPasswordModel model, string userName)
        {
            bool changePasswordSucceeded;
            try
            {
                string tokenResetPassword = WebSecurity.GeneratePasswordResetToken(userName, 30);
                changePasswordSucceeded = WebSecurity.ResetPassword(tokenResetPassword, model.NewPassword);
            }

            catch (Exception)
            {
                changePasswordSucceeded = false;
            }

            if (changePasswordSucceeded)
            {
                try
                {
                    var getUser = db.UserProfiles.Where(x => x.UserName == userName).FirstOrDefault();
                    if (getUser != null)
                    {
                        getUser.IsCustomerActive = true;
                        getUser.CustomerActive = (int)SendMailActive.Send;
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                { return View(model); }

                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }


        public bool SendMailLinkForgot(ConfigSendMailInfo configSendMailInfo, string link)
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
                message.Subject = "Thông tin thay đổi mật khẩu";
                message.To.Add(new MailAddress(configSendMailInfo.EmailTo));
                message.Body = "Vui lòng truy cập vào đường dẫn sau để thực hiện đổi mật khẩu: " + link;
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

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public JsonResult LoginPost(LoginCustomerModel model)
        {
            try
            {
                ModelState.Remove("UserName");

                if (!ModelState.IsValid)
                {
                    return Json(new { Success = false, Error = "Vui lòng điền đầy đủ thông tin", Type = "namepass" });
                }

                model.UserName = "customer_" + model.Email.Trim();

                var checkUser = db.UserProfiles.Where(x => x.UserName == model.UserName && x.Type == (int)TypeAccount.Customer).Any();

                if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe) && checkUser)
                {
                    if (model.RememberMe)
                    {
                        HttpCookie cookie = new HttpCookie("Login");
                        cookie.Values.Add("UserName", model.Email);
                        cookie.Values.Add("Password", model.Password);
                        cookie.Expires = DateTime.Now.AddDays(60);
                        Response.Cookies.Add(cookie);
                    }
                    else
                    {
                        Response.Cookies["Login"].Expires = DateTime.Now.AddDays(-1);
                    }
                    System.Web.HttpContext.Current.Session["UserName"] = model.UserName;
                    return Json(new { Success = true });
                }
                else
                {
                    return Json(new { Success = false, Error = AccountResources.LoginIncorrect, Type = "name" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Error = "Đăng nhập không thành công vui lòng thử lại", Type = "name" });
            }
        }

        [ChildActionOnly]
        // [OutputCache(Duration = 60 )]
        public ActionResult _AdvHoSoTuLieu(string position)
        {
            var adv = db.Advertisements.Where(
                x =>
                x.Status == (int)StatusEnum.Public
                && x.AdvertisementPosition.UID.ToLower().Equals(position));
            ViewBag.Position = position;
            int coutn = adv.Count();
            return PartialView(adv);
        }

        [ChildActionOnly]
        [HttpGet]
        // [OutputCache(Duration = 60 )]
        public ActionResult _AdvBanner(string position)
        {
            var adv = db.Advertisements.Where(
                x => x.Status == (int)StatusEnum.Public
                &&
                x.AdvertisementPosition.UID.ToLower().Equals(position));
            ViewBag.Position = position;
            int coutn = adv.Count();
            return PartialView(adv);
        }

        [ChildActionOnly]
        [HttpGet]
        // [OutputCache(Duration = 60 )]
        public ActionResult _AdvCenterHome(string position)
        {
            var adv = db.Advertisements.Where(
                x => x.Status == (int)StatusEnum.Public
                && x.AdvertisementPosition.UID.ToLower().Equals(position));
            ViewBag.Position = position;
            int coutn = adv.Count();
            return PartialView(adv);
        }

        [ChildActionOnly]
        [HttpGet]
        // [OutputCache(Duration = 60 )]
        public ActionResult _AdvFooter(string position)
        {
            var adv = db.Advertisements.Where(
                x => x.AdvertisementPosition.UID.ToLower().Equals(position) &&
                x.Status == (int)StatusEnum.Public
                &&
                         ((x.Culture == null ||
                              (!string.IsNullOrEmpty(x.Culture) && x.Culture.Equals(ApplicationService.Culture)))
                              || (ApplicationService.Culture == null))

                );
            ViewBag.Position = position;
            return PartialView(adv);
        }
        [HttpGet]
        public JsonResult JIndex(string position)
        {
            var adv = db.Advertisements.Where(x =>
                x.AdvertisementPosition.UID.ToLower().Equals(position) &&

                         ((x.Culture == null ||
                              (!string.IsNullOrEmpty(x.Culture) && x.Culture.Equals(ApplicationService.Culture)))
                              || (ApplicationService.Culture == null))


                ).Select(x => new { x.ID, x.Title, x.Description, x.Link, x.Media, x.Target });
            return Json(adv, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _Info()
        {
            var user = db.Set<UserProfile>().Find(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.Displayname = user.FullName == null ? "Noname" : user.FullName;
            ViewBag.Avatar = user.Avatar == null ? "/Images/no_avatar.jpg" : user.Avatar;
            return PartialView();
        }
    }
}