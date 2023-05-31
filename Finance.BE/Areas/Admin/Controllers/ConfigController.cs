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
using System.Transactions;
using Helpers.Net;
using System.ComponentModel;
using System.Globalization;
using System.Data.Entity.SqlServer;

namespace WEB.Areas.Admin.Controllers
{
    public class ConfigController : BaseController
    {
        WebContext db = new WebContext();
        // GET: Admin/Config
        public ActionResult Index()
        {
            var model = new WebConfigViewModel
            {
                EmailReceive = db.WebConfigs.Where(x => x.Key == "email-receive").Select(x => x.Value).FirstOrDefault(),
                EmailSend = db.WebConfigs.Where(x => x.Key == "email-send").Select(x => x.Value).FirstOrDefault(),
                EmailSendPassword = db.WebConfigs.Where(x => x.Key == "email-send-password").Select(x => x.Value).FirstOrDefault(),
                Threshold = db.WebConfigs.Where(x => x.Key == "threshold").Select(x => x.Value).FirstOrDefault(),
                LinkLog = db.WebConfigs.Where(x => x.Key == "LinkLog").Select(x => x.Value).FirstOrDefault(),
                LoginAttempt = db.WebConfigs.Where(x => x.Key == "LoginAttempt").Select(x => x.Value).FirstOrDefault(),
                XssTitle = db.WebConfigs.Where(x => x.Key == "XssTitle").Select(x => x.Value).FirstOrDefault(),
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateInput(false)]
        public JsonResult ChangeConfig(WebConfigViewModel model)
        {
            var EmailReceive = db.WebConfigs.Where(x => x.Key == "email-receive").FirstOrDefault();
            var EmailSend = db.WebConfigs.Where(x => x.Key == "email-send").FirstOrDefault();
            var EmailSendPassword = db.WebConfigs.Where(x => x.Key == "email-send-password").FirstOrDefault();
            var Threshold = db.WebConfigs.Where(x => x.Key == "threshold").FirstOrDefault();
            var LinkLog = db.WebConfigs.Where(x => x.Key == "LinkLog").FirstOrDefault();
            var LoginAttempt = db.WebConfigs.Where(x => x.Key == "LoginAttempt").FirstOrDefault();
            var XssTitle = db.WebConfigs.Where(x => x.Key == "XssTitle").FirstOrDefault();

            EmailReceive.Value = model.EmailReceive;
            EmailSend.Value = model.EmailSend;
            EmailSendPassword.Value = model.EmailSendPassword;
            Threshold.Value = model.Threshold;
            LinkLog.Value = model.LinkLog;
            LoginAttempt.Value = model.LoginAttempt;
            XssTitle.Value = model.XssTitle;
            db.SaveChanges();
            return Json(new { Success = true });
            
        }
    }
}