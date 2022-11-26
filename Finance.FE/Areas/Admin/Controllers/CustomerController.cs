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
using Helpers.Net;

namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public class CustomerController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [AllowAnonymous]
        public JsonResult Get_Gender()
        {
            var listGender = new List<ComboBoxViewModel>(){
            new ComboBoxViewModel{
                Id = (int)Gender.Male,
                Name = ((Gender)Gender.Male).GetStringValue()
            },
            new ComboBoxViewModel{
                Id = (int)Gender.Female,
                Name = ((Gender)Gender.Female).GetStringValue()
            }

        };

            return Json(listGender, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {

            return View();
        }

        public ActionResult Customers_Read([DataSourceRequest] DataSourceRequest request)
        {
            var users = db.UserProfiles.Where(x => x.Type != null && x.Type == (int)(TypeAccount.Customer) && x.IsActive != false).ToList().Select(x => new
            {
                x.UserId,
                x.UserName,
                x.Avatar,
                x.Email,
                x.FullName,
                x.Gender,
                x.Mobile,
                x.Passport,
                x.Position,
                x.Infomation,
                x.InviteCode,
                x.Address,
                CustomerActiveString = x.CustomerActive != null ? ((SendMailActive)x.CustomerActive).GetStringValue() : ((SendMailActive)0).GetStringValue()
            });


            var jsonResult = Json(users.OrderByDescending(x => x.UserId).ToList().ToDataSourceResult(request));
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

            //return Json(users.OrderByDescending(x=>x.UserId).ToDataSourceResult(request));
        }

        public ActionResult Read_UndoData([DataSourceRequest] DataSourceRequest request)
        {
            var users = db.UserProfiles.Where(x => x.Type != null && x.Type == (int)(TypeAccount.Customer) && x.IsActive == false).ToList().Select(x => new
            {
                x.UserId,
                x.UserName,
                x.Avatar,
                x.Email,
                x.FullName,
                x.Gender,
                x.Mobile,
                x.Passport,
                x.Position,
                x.Infomation,
                x.InviteCode,
                x.Address,
                CustomerActiveString = x.CustomerActive != null ? ((SendMailActive)x.CustomerActive).GetStringValue() : ((SendMailActive)0).GetStringValue()
            });

            var jsonResult = Json(users.OrderByDescending(x => x.UserId).ToList().ToDataSourceResult(request));
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult Read_Data([DataSourceRequest] DataSourceRequest request)
        {
            var users = db.UserProfiles.Where(x => x.Type != null && x.Type == (int)(TypeAccount.Customer) && x.IsActive != false).ToList().Select(x => new
            {
                x.UserId,
                x.UserName,
                x.Avatar,
                x.Email,
                x.FullName,
                x.Gender,
                x.Mobile,
                x.Passport,
                x.Position,
                x.Infomation,
                x.InviteCode,
                x.Address,
                CustomerActiveString = x.CustomerActive != null ? ((SendMailActive)x.CustomerActive).GetStringValue() : ((SendMailActive)0).GetStringValue()
            });

            var jsonResult = Json(users.OrderByDescending(x => x.UserId).ToList().ToDataSourceResult(request));
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add([Bind(Exclude = "")] CustomerModel model, HttpPostedFileBase image)
        {
            ModelState.Remove("UserName");
            model.UserName = "customer_" + model.Email;
            if (ModelState.IsValid)
            {
                var userProfile = db.UserProfiles.AsNoTracking().ToList();
                var checkAccountExist = userProfile.Where(p => p.UserName.Equals(model.UserName, StringComparison.OrdinalIgnoreCase)).Any();
                if (checkAccountExist)
                {
                    ModelState.AddModelError("Email", "Người dùng đã được thêm hoặc bị xóa khỏi hệ thống!");
                    return View(model);
                }
                else
                {
                    var checkBug = true;
                    var checkEmailExist = userProfile.Where(p => p.Email != null && p.Email.Equals(model.Email) && p.Type == (int)(TypeAccount.Customer)).Any();
                    if (checkEmailExist)
                    {
                        ModelState.AddModelError("Email", "Email đã tồn tại trên hệ thống!");
                        checkBug = false;
                    }

                    var checkMobileExist = userProfile.Where(p => p.Mobile != null && p.Mobile.Equals(model.Mobile) && p.Type == (int)(TypeAccount.Customer)).Any();
                    if (checkMobileExist)
                    {
                        ModelState.AddModelError("Mobile", "Số điện thoại đã tồn tại trên hệ thống!");
                        checkBug = false;
                    }

                    if (!checkBug) return View(model);

                    model.Type = (int)(TypeAccount.Customer);
                    model.InviteCode = Guid.NewGuid().ToString();

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
                            }
                        }
                        catch (Exception)
                        { }
                    }

                    WebSecurity.CreateUserAndAccount(model.UserName, "default", propertyValues: new
                    {
                        model.FullName,
                        model.Email,
                        model.Mobile,
                        model.Avatar,
                        model.Address,
                        model.Gender,
                        model.Passport,
                        model.Infomation,
                        model.InviteCode,
                        model.Type,
                        CustomerActive = SendMailActive.UnSend,
                        CreatedAt = DateTime.Now,
                        CreatedBy = WebSecurity.CurrentUserId

                    });

                    ViewBag.StartupScript = "create_success();";
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }

        public ActionResult Edit(int id)
        {
            var user = db.Set<UserProfile>().Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.cUserName = user.UserName;
            return View("Edit", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "")] UserProfile model, string cUserName, string[] roles, HttpPostedFileBase image)
        {
            //ModelState.Remove("UserName");
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
                    var checkEmailExist = userProfile.Where(p => p.Email != null && p.Type == (int)(TypeAccount.Customer) && p.UserId != model.UserId && p.Email.Equals(model.Email)).Any();
                    if (checkEmailExist)
                    {
                        ModelState.AddModelError("Email", "Email đã tồn tại trên hệ thống!");
                        checkBug = false;
                    }

                    var checkMobileExist = userProfile.Where(p => p.Mobile != null && p.Type == (int)(TypeAccount.Customer) && p.UserId != model.UserId && p.Mobile.Equals(model.Mobile)).Any();
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
                        var dir = new System.IO.DirectoryInfo(Server.MapPath("/content/uplo" +
                            "ads/avatars/"));
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
                    db.Entry(model).Property(a => a.Email).IsModified = true;
                    db.Entry(model).Property(a => a.Gender).IsModified = true;
                    db.Entry(model).Property(a => a.Mobile).IsModified = true;
                    db.Entry(model).Property(a => a.Address).IsModified = true;
                    db.Entry(model).Property(a => a.Avatar).IsModified = true;
                    db.Entry(model).Property(a => a.Infomation).IsModified = true;
                    db.Entry(model).Property(a => a.Passport).IsModified = true;
                    db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                    db.Entry(model).Property(a => a.ModifiedAt).IsModified = true;
                    db.Entry(model).Property(a => a.Avatar).IsModified = (model.Avatar != null) || delavatar;

                    db.SaveChanges();

                    ViewBag.StartupScript = "create_success();";
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }

        [HttpPost]
        public JsonResult Deletes(string data)
        {
            try
            {
                var listDataConvert = StringToListInt(data);
                var listData = db.UserProfiles.Where(x => listDataConvert.Contains(x.UserId));

                foreach (var item in listData)
                {
                    item.IsActive = false;
                    item.ModifiedAt = DateTime.Now;
                    item.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                }

                db.SaveChanges();
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }

            return Json(new { success = true });
        }

        public List<int> StringToListInt(string data)
        {
            var listData = new List<int>();
            var dataObjTrim = data.Trim('[').Trim(']');
            listData = (dataObjTrim.Split(',').ToList()).Select(int.Parse).ToList();

            return listData;
        }

        public ActionResult ChangePassword(string userName)
        {
            if (!WebSecurity.UserExists(userName))
            {
                return HttpNotFound();
            }

            ViewBag.UserName = userName;
            return View();
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(LocalPasswordModel model, string userName)
        {
            bool changePasswordSucceeded;
            try
            {
                string token = WebSecurity.GeneratePasswordResetToken(userName, 30);
                changePasswordSucceeded = WebSecurity.ResetPassword(token, model.NewPassword);
            }
            catch (Exception)
            {
                changePasswordSucceeded = false;
            }

            if (changePasswordSucceeded)
            {
                ViewBag.StartupScript = "change_success();";
            }
            else
            {
                ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
            }

            return View();
        }

        public ActionResult Undo()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Delete(int userId)
        {
            try
            {
                var userDelete = db.UserProfiles.Find(userId);
                userDelete.ModifiedAt = DateTime.Now;
                userDelete.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                userDelete.IsActive = false;
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });

            }
        }

        [HttpPost]
        public JsonResult Undo(string data)
        {
            var listDataConvert = ConvertJsonDeleteData(data);

            try
            {
                var userDelete = db.UserProfiles.Where(x => listDataConvert.Contains(x.UserId));
                foreach (var item in userDelete)
                {
                    item.ModifiedAt = DateTime.Now;
                    item.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    item.IsActive = true;
                }

                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }
        }

        public List<int> ConvertJsonDeleteData(string data)
        {
            var listData = new List<int>();
            var dataObjTrim = data.Trim('[').Trim(']');
            listData = (dataObjTrim.Split(',').ToList()).Select(int.Parse).ToList();

            return listData;
        }

        [HttpPost]
        public JsonResult InviteCustomer(string data)
        {
            try
            {
                var listDataConvert = StringToListInt(data);
                var listData = db.UserProfiles.Where(x => listDataConvert.Contains(x.UserId)).ToList();
                ConfigSendMailInfo configSendMailInfo = GetInfoConfigSendMail();

                foreach (var item in listData)
                {
                    var checkSend = SendMailCustomer(item, configSendMailInfo);
                    item.CustomerActive = (int)SendMailActive.WaitingActive;
                }

                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }
        }

        public bool SendMailCustomer(UserProfile userProfile, ConfigSendMailInfo configSendMailInfo)
        {
            bool _return = false;
            try
            {
                configSendMailInfo.EmailTo = userProfile.Email;
                var url = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + ":" + Request.Url.Port;
                var link = Url.Action("InviteCustomer", "Account", new { area = "", stringInvite = userProfile.InviteCode });

                var client = new SmtpClient(configSendMailInfo.Host, int.Parse(configSendMailInfo.Port));
                client.UseDefaultCredentials = false;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(configSendMailInfo.SendFrom, configSendMailInfo.EmailPass);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)48 | (SecurityProtocolType)192 |
                (SecurityProtocolType)768 | (SecurityProtocolType)3072;
                var message = new MailMessage();
                message.From = new MailAddress(configSendMailInfo.SendFrom, configSendMailInfo.SendFrom);
                message.Subject = "[Hệ thống quản lý tài sản] Mời Quý khách hàng đăng nhập";
                message.To.Add(new MailAddress(configSendMailInfo.EmailTo));
                message.Body = "Mời Quý khách hàng click vào đường link sau để thiết lập mật khẩu ban đầu và bắt đầu sử dụng hệ thống: " + url +link;
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

        private void SubAdminSitesTree(ref TreeViewItemModel parentNode, ref List<TreeViewItemModel> tree)
        {
            int parentID = int.Parse(parentNode.Id);
            var nodes = db.AdminSites.AsNoTracking().Where(x => x.ParentID == parentID).AsEnumerable();
            foreach (var item in nodes)
            {
                TreeViewItemModel node = new TreeViewItemModel();
                node.Id = item.ID.ToString();
                node.Text = item.Title;
                node.HasChildren = item.SubAdminSites.Any();
                parentNode.Items.Add(node);
                if (node.HasChildren)
                    SubAdminSitesTree(ref node, ref tree);
            }
        }
    }
}
