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
                x.LegalRepresentative,
                x.RepresentativePosition,
                x.BusinessRegistrationNumber,
                x.AuthorizedPerson,
                x.AuthorizationNumber,
                CustomerActiveString = x.CustomerActive != null ? ((SendMailActive)x.CustomerActive).GetStringValue() : ((SendMailActive)0).GetStringValue(),
 
            });


            var jsonResult = Json(users.OrderByDescending(x => x.UserId).ToList().ToDataSourceResult(request));
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

            //return Json(users.OrderByDescending(x=>x.UserId).ToDataSourceResult(request));
        }

        public ActionResult Business_Read([DataSourceRequest] DataSourceRequest request)
        {
            var users = db.UserProfiles.Where(x => x.Type != null && x.Type == (int)(TypeAccount.Business) && x.IsActive != false).ToList().Select(x => new
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
                x.LegalRepresentative,
                x.RepresentativePosition,
                x.BusinessRegistrationNumber,
                x.AuthorizedPerson,
                x.AuthorizationNumber,
                CustomerActiveString = x.CustomerActive != null ? ((SendMailActive)x.CustomerActive).GetStringValue() : ((SendMailActive)0).GetStringValue(),

            });


            var jsonResult = Json(users.OrderByDescending(x => x.UserId).ToList().ToDataSourceResult(request));
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

            //return Json(users.OrderByDescending(x=>x.UserId).ToDataSourceResult(request));
        }



        public ActionResult Read_UndoData([DataSourceRequest] DataSourceRequest request)
        {
            var users = db.UserProfiles.Where(x => x.Type != null && x.Type != (int)(TypeAccount.Admin) && x.IsActive == false).ToList().Select(x => new
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
            var users = db.UserProfiles.Where(x => x.Type != null && x.Type != (int)(TypeAccount.Admin) && x.IsActive != false).ToList().Select(x => new
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
        public ActionResult AddIndividualCustomers()
        {

            var model = new CustomerModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddIndividualCustomers([Bind(Exclude = "")] CustomerModel model, HttpPostedFileBase image)
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

                    var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                    AuditLog log = new AuditLog
                    {
                        ActiveType = LogConstant.AddCustomer,
                        FunctionName = LogConstant.ManageCustomer,
                        Information = model.ToJson(),
                        DataTable = LogConstant.Customer,
                        UserName = userCurent.FullName + "(" + userCurent.UserName + ")",
                        CreatedBy = WebSecurity.CurrentUserId,
                        CreatedAt = DateTime.Now
                    };
                    db.AuditLogs.Add(log);
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

        public ActionResult AddBusinessCustomers()
        {

            var model = new CustomerModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddBusinessCustomers([Bind(Exclude = "")] CustomerModel model, HttpPostedFileBase image)
        {
            ModelState.Remove("UserName");
          
            model.UserName = "customer_" + model.Mobile;
            
            if (ModelState.IsValid)
            {
                var userProfile = db.UserProfiles.AsNoTracking().ToList();
                var checkAccountExist = userProfile.Where(p => p.UserName.Equals(model.UserName, StringComparison.OrdinalIgnoreCase)).Any();
                if (checkAccountExist)
                {
                    ModelState.AddModelError("Mobile", "Người dùng đã được thêm hoặc bị xóa khỏi hệ thống!");
                    return View(model);
                }
                else
                {
                    var checkBug = true;

                    var checkEmailExist = userProfile.Where(p => p.Email != null && p.Email.Equals(model.Email) && p.Type == (int)(TypeAccount.Business)).Any();
                    if (checkEmailExist)
                    {
                        ModelState.AddModelError("Email", "Email đã tồn tại trên hệ thống!");
                        checkBug = false;
                    }

                    var checkMobileExist = userProfile.Where(p => p.Mobile != null && p.Mobile.Equals(model.Mobile) && p.Type == (int)(TypeAccount.Business)).Any();
                    if (checkMobileExist)
                    {
                        ModelState.AddModelError("Mobile", "Số điện thoại đã tồn tại trên hệ thống!");
                        checkBug = false;
                    }

                    if (!checkBug) return View(model);

                    model.Type = (int)(TypeAccount.Business);
                    model.InviteCode = Guid.NewGuid().ToString();



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
                        model.LegalRepresentative,
                        model.RepresentativePosition,
                        model.BusinessRegistrationNumber,
                        model.AuthorizedPerson,
                        model.AuthorizationNumber,

                        CustomerActive = SendMailActive.UnSend,
                        CreatedAt = DateTime.Now,
                        CreatedBy = WebSecurity.CurrentUserId

                    });

                    var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                    AuditLog log = new AuditLog
                    {
                        ActiveType = LogConstant.AddCustomer,
                        FunctionName = LogConstant.ManageCustomer,
                        Information = model.ToJson(),
                        DataTable = LogConstant.Customer,
                        UserName = userCurent.FullName + "(" + userCurent.UserName + ")",
                        CreatedBy = WebSecurity.CurrentUserId,
                        CreatedAt = DateTime.Now
                    };
                    db.AuditLogs.Add(log);
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

                    var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                    AuditLog log = new AuditLog
                    {
                        ActiveType = LogConstant.AddCustomer,
                        FunctionName = LogConstant.ManageCustomer,
                        Information = model.ToJson(),
                        DataTable = LogConstant.Customer,
                        UserName = userCurent.FullName + "(" + userCurent.UserName + ")",
                        CreatedBy = WebSecurity.CurrentUserId,
                        CreatedAt = DateTime.Now
                    };
                    db.AuditLogs.Add(log);
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

        public ActionResult Edit(int id)
        {
            var user = db.Set<UserProfile>().Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.cUserName = user.UserName;
            if(user.Type == (int)(TypeAccount.Customer))
            {
                return View("Edit", user);
            }
            else
            {
                return View("EditBusiness", user);
            }
            
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
                    db.Entry(model).Property(a => a.LegalRepresentative).IsModified = true;
                    db.Entry(model).Property(a => a.RepresentativePosition).IsModified = true;
                    db.Entry(model).Property(a => a.BusinessRegistrationNumber).IsModified = true;
                    db.Entry(model).Property(a => a.AuthorizedPerson).IsModified = true;
                    db.Entry(model).Property(a => a.AuthorizationNumber).IsModified = true;
                    db.Entry(model).Property(a => a.Avatar).IsModified = (model.Avatar != null) || delavatar;

                    db.SaveChanges();

                    var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                    AuditLog log = new AuditLog
                    {
                        ActiveType = LogConstant.EditCustomer,
                        FunctionName = LogConstant.ManageCustomer,
                        Information = model.ToJson(),
                        DataTable = LogConstant.Customer,
                        UserName = userCurent.FullName + "(" + userCurent.UserName + ")",
                        CreatedBy = WebSecurity.CurrentUserId,
                        CreatedAt = DateTime.Now
                    };
                    db.AuditLogs.Add(log);
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

                var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                AuditLog log = new AuditLog
                {
                    ActiveType = LogConstant.DeleteCustomer,
                    FunctionName = LogConstant.ManageCustomer,
                    Information = listData.ToJson(),
                    DataTable = LogConstant.Customer,
                    UserName = userCurent.FullName + "(" + userCurent.UserName + ")",
                    CreatedBy = WebSecurity.CurrentUserId,
                    CreatedAt = DateTime.Now
                };
                db.AuditLogs.Add(log);
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

                var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                AuditLog log = new AuditLog
                {
                    ActiveType = LogConstant.DeleteCustomer,
                    FunctionName = LogConstant.ManageCustomer,
                    Information = userDelete.ToJson(),
                    DataTable = LogConstant.Customer,
                    UserName = userCurent.FullName + "(" + userCurent.UserName + ")",
                    CreatedBy = WebSecurity.CurrentUserId,
                    CreatedAt = DateTime.Now
                };
                db.AuditLogs.Add(log);
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

                var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                AuditLog log = new AuditLog
                {
                    ActiveType = LogConstant.UndoCustomer,
                    FunctionName = LogConstant.ManageCustomer,
                    Information = userDelete.ToJson(),
                    DataTable = LogConstant.Customer,
                    UserName = userCurent.FullName + "(" + userCurent.UserName + ")",
                    CreatedBy = WebSecurity.CurrentUserId,
                    CreatedAt = DateTime.Now
                };
                db.AuditLogs.Add(log);
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
                var getBeSite = db.WebConfigs.Where(x => x.Key == "SiteBE").Select(x => x.Value).FirstOrDefault().ToString();
                configSendMailInfo.EmailTo = userProfile.Email;
                var url = db.WebConfigs.Where(x => x.Key == "SiteFE").Select(x => x.Value).FirstOrDefault();
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
                message.Subject = "[Quản lý danh mục đầu tư TLBonds] Đăng nhập hệ thống Quản lý danh mục tài sản";
                message.To.Add(new MailAddress(configSendMailInfo.EmailTo));
                /*message.Body = "Mời Quý khách hàng click vào đường link sau để thiết lập mật khẩu ban đầu và bắt đầu sử dụng hệ thống: " + url +link;*/
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
                message.Body += "Kính chào: Quý khách (Dear Mr./Ms.)" + " " + userProfile.FullName;
                message.Body += "</p>";
                message.Body += "<div style='font-size:10px;line-height:10px;height:10px'>";
                message.Body += "</div>";
                message.Body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                message.Body += "Công ty cổ phần Đầu tư trái phiếu Thăng Long (TLBONDS) xin trân trọng thông báo Tài khoản của Quý khách hàng trên hệ thống “Quản quản lý danh mục đầu tư TLBonds” của TLBonds đã được kích hoạt.";
                message.Body += "</p>";
                message.Body += "<br>";
                message.Body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                message.Body += "Thông tin tài khoản của quý khách hàng như sau:";
                message.Body += "</p>";
                message.Body += "<br/>";

                message.Body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                message.Body += "- Email truy cập :" + " " + userProfile.Email;
                message.Body += "</p>";
                message.Body += "<br/>";

                message.Body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                message.Body += "- Tên khách hàng : " + " " + userProfile.FullName;
                message.Body += "</p>";
                message.Body += "<br/>";


                message.Body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                message.Body += "- Điện thoại :" + " " + userProfile.Mobile;
                message.Body += "</p>";
                message.Body += "<br/>";


                message.Body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                message.Body += "Quý khách có thể truy cập vào hệ thống Quản lý danh mục đầu tư TLBonds và bắt đầu theo dõi tình hình đầu tư ngay sau đây. Để bắt đầu, xin mời Quý khách thiết lập tài khoản của mình theo đường dẫn dưới đây:" + " " + url + link;
                message.Body += "</p>";
                message.Body += "<br/>";

                message.Body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                message.Body += "Link truy cập hệ thống quản lý danh mục đầu tư: http://quanlydanhmucdautu.tlbonds.vn/" + " ";
                message.Body += "<br/>";
                message.Body += "hoặc truy cập website https://tlbonds.vn/ ";
                message.Body += "</p>";
                message.Body += "<br/>";
                message.Body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                message.Body += "Trong trường hợp cần thêm thông tin chi tiết, Quý khách vui lòng liên hệ Tổng đài 024.32047782 hoặc gửi email tới hòm thư support@tlbonds.vn để được tư vấn và hỗ trợ. TLBonds xin chân thành cảm ơn Quý Khách hàng đã tin tưởng và lựa chọn sử dụng dịch vụ của chúng tôi.<br>";
                message.Body += "<br/>";
                message.Body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px;font-style: italic;'>";
                message.Body += "Kính chúc Quý khách hàng sức khỏe và đầu tư hiệu quả cùng TLBonds!";
                message.Body += "</p>";
                message.Body += "<br/>";
                message.Body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:normal;line-height:24px'>";
                message.Body += "Mọi yêu cầu cần giải đáp, Xin Quý khách vui lòng liên hệ với TLBonds.<br>";
                message.Body += "Thông tin liên hệ (Contact Center): <br>";
                message.Body += "Điện thoại: 024.32047782<br>";
                message.Body += "Website: https://www.tlbonds.vn<br>";
                message.Body += " Email: support@tlbonds.vn<br>";
                message.Body += "</p>";
                message.Body += "<br/>";
                message.Body += "<p style='margin:0;padding:0;font-size:16px;color:#202020;font-family:Helvetica,Arial,sans-serif;font-weight:bold;font-style: italic;'>CÔNG TY CỔ PHẦN ĐẦU TƯ TRÁI PHIẾU THĂNG LONG (TLBONDS)</p>";
                message.Body += "<img src = '" + getBeSite + "/Content/themes/admin/img/footerMail.jpg' style='max-width:650px' width='100%' />";
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
