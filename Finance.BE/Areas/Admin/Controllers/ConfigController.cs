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
                EmailSendSMTP = db.WebConfigs.Where(x => x.Key == "email-send-smtp").Select(x => x.Value).FirstOrDefault(),
                EmailSendPort = db.WebConfigs.Where(x => x.Key == "email-send-port").Select(x => x.Value).FirstOrDefault(),
                EmailSendPassword = db.WebConfigs.Where(x => x.Key == "email-send-password").Select(x => x.Value).FirstOrDefault(),
                EmailSendSSL = db.WebConfigs.Where(x => x.Key == "email-send-ssl").Select(x => x.Value).FirstOrDefault(),
                LicenseKey = db.WebConfigs.Where(x => x.Key == "LicenseKey").Select(x => x.Value).FirstOrDefault(),
                Threshold = db.WebConfigs.Where(x => x.Key == "threshold").Select(x => x.Value).FirstOrDefault()
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult ChangeConfig(WebConfigViewModel model)
        {
            var EmailReceive = db.WebConfigs.Where(x => x.Key == "email-receive").FirstOrDefault();
            var EmailSend = db.WebConfigs.Where(x => x.Key == "email-send").FirstOrDefault();
            var EmailSendSMTP = db.WebConfigs.Where(x => x.Key == "email-send-smtp").FirstOrDefault();
            var EmailSendPort = db.WebConfigs.Where(x => x.Key == "email-send-port").FirstOrDefault();
            var EmailSendPassword = db.WebConfigs.Where(x => x.Key == "email-send-password").FirstOrDefault();
            var EmailSendSSL = db.WebConfigs.Where(x => x.Key == "email-send-ssl").FirstOrDefault();
            var LicenseKey = db.WebConfigs.Where(x => x.Key == "LicenseKey").FirstOrDefault();
            var Threshold = db.WebConfigs.Where(x => x.Key == "threshold").FirstOrDefault();

            EmailReceive.Value = model.EmailReceive;
            EmailSend.Value = model.EmailSend;
            EmailSendSMTP.Value = model.EmailSendSMTP;
            EmailSendPort.Value = model.EmailSendPort;
            EmailSendPassword.Value = model.EmailSendPassword;
            EmailSendSSL.Value = model.EmailSendSSL;
            LicenseKey.Value = model.LicenseKey;
            Threshold.Value = model.Threshold;

            db.SaveChanges();
            return Json(new { Success = true });
            
        }
    }
}