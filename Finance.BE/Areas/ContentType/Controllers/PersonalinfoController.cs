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

namespace WEB.Areas.ContentType.Controllers
{
    /*[VanTaiAuthorize]*/
    public class PersonalInfoController : Controller
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        [ChildActionOnly]
        public ActionResult Index(int? m_id, UserProfile model)
        {
            if(string.IsNullOrEmpty(model.UserName) || string.IsNullOrWhiteSpace(model.UserName))
            {
                model = db.Set<UserProfile>().Find(WebSecurity.CurrentUserId);
                if (model == null)
                {
                    return HttpNotFound();
                }
                ViewBag.cUserName = model.UserName;

            }

            ViewBag.WebModule = db.WebModules.FirstOrDefault(x => x.ID == m_id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "")] UserProfile model, int? m_id,string cUserName, string[] roles, HttpPostedFileBase image)
        {
            //ModelState.Remove("UserName");
            var status = false;
            ViewBag.WebModule = db.WebModules.FirstOrDefault(x => x.ID == m_id);
            if (ModelState.IsValid)
            {
                var userProfile = db.UserProfiles.AsNoTracking().ToList();
                var checkAccountExist = userProfile.Where(p => p.UserId != model.UserId && p.UserName.Equals(model.UserName, StringComparison.OrdinalIgnoreCase)).Any();
                if (checkAccountExist)
                {
                    ModelState.AddModelError("Email", "Người dùng đã tồn tại hoặc bị xóa khỏi hệ thống!");
                    return View(model);
                }
                else
                {
                    var checkBug = true;
                    

                    var checkMobileExist = userProfile.Where(p => p.Mobile != null && p.Type != (int)(TypeAccount.Admin) && p.UserId != model.UserId && p.Mobile.Equals(model.Mobile)).Any();
                    if (checkMobileExist)
                    {
                        ModelState.AddModelError("Mobile", "Số điện thoại đã tồn tại trên hệ thống!");
                        checkBug = false;
                    }

                    if (!checkBug) return View(model);

                    var delavatar = false;
                    if (image != null)
                    {
                        var name = image.FileName;
                        string extension = Path.GetExtension(name);
                        var newName = model.UserName + extension;
                        var dir = new System.IO.DirectoryInfo(Server.MapPath("/content/uploads/avatars/"));
                        if (!dir.Exists) dir.Create();
                        var uri = "/content/uploads/avatars/" + newName;
                        image.SaveAs(HttpContext.Server.MapPath(uri));
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
                                model.Avatar = null;
                            }
                        }
                        catch (Exception)
                        { }
                    }
                    else
                    {
                        if (model.Avatar == null)
                        {
                            delavatar = true;
                        }
                    }

                    model.IsActive = true;
                    model.ModifiedAt = DateTime.Now;
                    model.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;

                    db.UserProfiles.Attach(model);
                    db.Entry(model).Property(a => a.UserName).IsModified = false;
                    db.Entry(model).Property(a => a.FullName).IsModified = true;
                    db.Entry(model).Property(a => a.Mobile).IsModified = true;
                    db.Entry(model).Property(a => a.Gender).IsModified = true;
                    db.Entry(model).Property(a => a.Address).IsModified = true;
                    db.Entry(model).Property(a => a.Avatar).IsModified = true; 
                    db.Entry(model).Property(a => a.Passport).IsModified = true;
                    db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                    db.Entry(model).Property(a => a.ModifiedAt).IsModified = true;
                    db.Entry(model).Property(a => a.Avatar).IsModified = (model.Avatar != null) || delavatar;
                    db.SaveChanges();
                    ViewBag.StartupScript = "create_success();";
                    return Json(new { status = !status });
                    
                }
            }
            else
            {
                return Json(new { status = status });
            }
        }
        
    }
}