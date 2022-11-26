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

namespace WEB.Areas.ContentType.Controllers
{
    public class ChangePasswordController : Controller
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        // GET: ContentType/ChangePassword
        public ActionResult _PubIndex(int? id)
        {
            var username = WebSecurity.CurrentUserName;
            ViewBag.WebModule = db.WebModules.FirstOrDefault(x => x.ID == id);
            ViewBag.UserName = username;
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public JsonResult ChangePasswordPost(LocalPasswordModel model, string userName)
        {
            try
            {
                bool changePasswordSucceeded = true;
                try
                {
                    string token = WebSecurity.GeneratePasswordResetToken(userName, 30);
                    changePasswordSucceeded = WebSecurity.ResetPassword(token, model.NewPassword);
                }
                catch (Exception)
                {
                    return Json(new { Success = false, Error = "Đổi mật khẩu không thành công vui lòng thử lại", Type = "confirm" });
                    
                }
                return Json(new { Success = true });              
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Error = "Đổi mật khẩu không thành công vui lòng thử lại", Type = "confirm" });
            }
        }
    }
}