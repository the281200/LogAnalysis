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
    public class NewsDetailController : Controller
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        
        public ActionResult _PubIndex(int? m_id , int Id)
        {
            var user = WebSecurity.GetUserId(User.Identity.Name);
            ViewBag.WebModule = db.WebModules.FirstOrDefault(x => x.ID == m_id);
            var newsDetail = db.News.Find(Id);
            return View(newsDetail);
        }
    }
}
