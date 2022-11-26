using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebModels;
using System.Data;
using WEB.Models;
using System.Data.Entity;
using Newtonsoft.Json;
using System.Web;
using WebMatrix.WebData;
using System.IO;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public class NewsController : BaseController
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

        [AllowAnonymous]
        public ActionResult News_Read([DataSourceRequest] DataSourceRequest request)
        {
            var data = from x in db.News.Where(x => x.IsActive != false)
                       select new
                       {
                           x.Id,
                           x.BuyAndSellBondId,
                           x.Body,
                           x.Title,
                           x.IsPublish,
                           x.Image,
                           ContractName = x.BuyAndSellBondId != null ? x.BuyAndSellBond.ContractName : "",
                           CustomerName = x.CustomerId != null ? x.Customer.FullName : ""
                       };

            var test = db.News.Where(x => x.IsActive != false).ToList();
            var test1 = data.ToList();

            var jsonResult = Json(data.OrderByDescending(x => x.Id).ToList().ToDataSourceResult(request));
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult Read_UndoData([DataSourceRequest] DataSourceRequest request)
        {
            var data = from x in db.News.Where(x => x.IsActive == false)
                       select new
                       {
                           x.Id,
                           x.BuyAndSellBondId,
                           x.Body,
                           x.Title,
                           x.IsPublish,
                           x.Image,
                           ContractName = x.BuyAndSellBondId != null ? x.BuyAndSellBond.ContractName : "",
                           CustomerName = x.CustomerId != null ? x.Customer.FullName : ""
                       };

            var jsonResult = Json(data.OrderByDescending(x => x.Id).ToList().ToDataSourceResult(request));
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult Read_Data([DataSourceRequest] DataSourceRequest request)
        {
            var data = from x in db.News.Where(x => x.IsActive != false)
                       select new
                       {
                           x.Id,
                           x.BuyAndSellBondId,
                           x.Body,
                           x.Title,
                           x.IsPublish,
                           x.Image,
                           ContractName = x.BuyAndSellBondId != null ? x.BuyAndSellBond.ContractName : "",
                           CustomerName = x.CustomerId != null ? x.Customer.FullName : ""
                       };

            var jsonResult = Json(data.OrderByDescending(x => x.Id).ToList().ToDataSourceResult(request));
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

        [AllowAnonymous]
        public JsonResult Get_BuyAndSellBonds(int? customerId)
        {
            var assets = from x in db.BuyAndSellBonds.AsNoTracking() where (x.IsActive != false && x.CustomerId == customerId) select x;
            return Json(assets.ToList().Select(x => new
            {
                Id = x.Id,
                Name = x.ContractCode + " - " + x.ContractName
            }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Add()
        {
            var model = new New();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]

        public ActionResult Add([Bind(Exclude = "")] New model, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                using (DbContextTransaction transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var saveSite = db.WebConfigs.Where(x => x.Key == "SiteBE").Select(x => x.Value).FirstOrDefault();
                        var checkValidate = true;
                        if (image != null)
                        {
                            var name = image.FileName;
                            string extension = Path.GetExtension(name);
                            var dir = new System.IO.DirectoryInfo(Server.MapPath("/uploads/image/"));
                            if (!dir.Exists) dir.Create();
                            var uri = "/uploads/image/" + name;
                            image.SaveAs(HttpContext.Server.MapPath(uri));
                            try
                            {
                                if (ImageTools.ValidateImage(System.Web.HttpContext.Current.Server.MapPath(uri)))
                                {
                                    var result = ImageTools.ResizeImage(Server.MapPath(uri), Server.MapPath(uri), 1280, 720, true, 80);
                                    model.Image = saveSite + uri;
                                }
                                else
                                {
                                    Utility.DeleteFile(uri);
                                }
                            }
                            catch (Exception)
                            { }
                            db.WebContentUploads.Add(new WebContentUpload()
                            {
                                Title = image.FileName,
                                MetaTitle = image.FileName.UnsignNormalize(),
                                FullPath = model.Image,
                                UID = ViewBag.GAK,
                                MimeType = ApplicationService.GetMimeType(Path.GetExtension(image.FileName)),
                                CreatedBy = WebSecurity.CurrentUserName,
                                CreatedDate = DateTime.Now
                            });
                            db.SaveChanges();
                        }
                        else
                        {
                            ModelState.AddModelError("Image", "Vui lòng chọn ảnh bìa");
                            checkValidate = false;
                        }

                        if (model.Body == null)
                        {
                            ModelState.AddModelError("Body", "Vui lòng không để trống nội dung");
                            checkValidate = false;
                        }

                        if (!checkValidate) return View(model);

                        model.CreatedAt = DateTime.Now;
                        model.CreatedBy = WebSecurity.CurrentUserId;
                        model.IsActive = true;
                        model.IsPublish = true;

                        db.News.Add(model);
                        db.SaveChanges();

                        var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                        AuditLog log = new AuditLog
                        {
                            ActiveType = LogConstant.AddNews,
                            FunctionName = LogConstant.ManageNews,
                            Information = model.ToJson(),
                            DataTable = LogConstant.News,
                            UserName = userCurent.FullName + "(" + userCurent.UserName + ")",
                            CreatedBy = WebSecurity.CurrentUserId,
                            CreatedAt = DateTime.Now
                        };
                        db.AuditLogs.Add(log);
                        db.SaveChanges();
                        transaction.Commit();

                        ViewBag.StartupScript = "create_success();";
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "Đã có lỗi xảy ra trong quá trình lưu! Vui lòng thử lại.");
                    }
                }
                return View(model);

            }
            else
            {
                return View(model);
            }
        }

        public ActionResult Edit(int id)
        {
            var model = db.Set<New>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]

        public ActionResult Edit([Bind(Include = "")] New model, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                using (DbContextTransaction transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var saveSite = "http://financetestadmin.innosoftware.vn/";
                        var checkValidate = true;
                        if (image != null)
                        {
                            var name = image.FileName;
                            string extension = Path.GetExtension(name);
                            var dir = new System.IO.DirectoryInfo(Server.MapPath("/uploads/image/"));
                            if (!dir.Exists) dir.Create();
                            var uri = "/uploads/image/" + name;
                            image.SaveAs(HttpContext.Server.MapPath(uri));
                            try
                            {
                                if (ImageTools.ValidateImage(System.Web.HttpContext.Current.Server.MapPath(uri)))
                                {
                                    var result = ImageTools.ResizeImage(Server.MapPath(uri), Server.MapPath(uri), 1280, 720, true, 80);
                                    model.Image = saveSite + uri;
                                }
                                else
                                {
                                    Utility.DeleteFile(uri);
                                }
                            }
                            catch (Exception)
                            { }
                            db.WebContentUploads.Add(new WebContentUpload()
                            {
                                Title = image.FileName,
                                MetaTitle = image.FileName.UnsignNormalize(),
                                FullPath = model.Image,
                                UID = ViewBag.GAK,
                                MimeType = ApplicationService.GetMimeType(Path.GetExtension(image.FileName)),
                                CreatedBy = WebSecurity.CurrentUserName,
                                CreatedDate = DateTime.Now
                            });
                            db.SaveChanges();
                        }
                        
                        else
                        {
                            if (model.Image == null)
                            {
                                ModelState.AddModelError("Image", "Vui lòng chọn ảnh bìa");
                                checkValidate = false;
                            }
                        }

                        if (model.Body == null)
                        {
                            ModelState.AddModelError("Body", "Vui lòng không để trống nội dung");
                            checkValidate = false;
                        }

                        if (!checkValidate) return View(model);

                        model.ModifiedAt = DateTime.Now;
                        model.ModifiedBy = WebSecurity.CurrentUserId;

                        db.News.Attach(model);
                        db.Entry(model).Property(a => a.Title).IsModified = true;
                        db.Entry(model).Property(a => a.Body).IsModified = true;
                        db.Entry(model).Property(a => a.BuyAndSellBondId).IsModified = true;
                        db.Entry(model).Property(a => a.CustomerId).IsModified = true;
                        db.Entry(model).Property(a => a.Image).IsModified = true;
                        db.Entry(model).Property(a => a.ModifiedAt).IsModified = true;
                        db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                        db.SaveChanges();

                        var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                        AuditLog log = new AuditLog
                        {
                            ActiveType = LogConstant.EditNews,
                            FunctionName = LogConstant.ManageNews,
                            Information = model.ToJson(),
                            DataTable = LogConstant.News,
                            UserName = userCurent.FullName + "(" + userCurent.UserName + ")",
                            CreatedBy = WebSecurity.CurrentUserId,
                            CreatedAt = DateTime.Now
                        };
                        db.AuditLogs.Add(log);
                        db.SaveChanges();

                        transaction.Commit();
                        ViewBag.StartupScript = "edit_success();";
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();

                        ModelState.AddModelError("", "Đã có lỗi xảy ra trong quá trình lưu! Vui lòng thử lại.");
                    }
                }

                return View(model);
            }
            else
            {
                return View(model);
            }
        }

        [HttpPost]
        public JsonResult Public(int? id)
        {
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var publicNews = db.News.Find(id);
                    publicNews.ModifiedAt = DateTime.Now;
                    publicNews.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    publicNews.IsPublish = true;
                    db.SaveChanges();

                    var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                    AuditLog log = new AuditLog
                    {
                        ActiveType = LogConstant.PublicNews,
                        FunctionName = LogConstant.ManageNews,
                        Information = publicNews.ToJson(),
                        DataTable = LogConstant.News,
                        UserName = userCurent.FullName + "(" + userCurent.UserName + ")",
                        CreatedBy = WebSecurity.CurrentUserId,
                        CreatedAt = DateTime.Now
                    };
                    db.AuditLogs.Add(log);
                    db.SaveChanges();

                    transaction.Commit();
                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    transaction.Rollback();

                    return Json(new { success = false });
                }
            }
        }

        [HttpPost]
        public JsonResult UnPublic(int? id)
        {
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var unPublicNews = db.News.Find(id);
                    unPublicNews.ModifiedAt = DateTime.Now;
                    unPublicNews.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    unPublicNews.IsPublish = false;
                    db.SaveChanges();

                    var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                    AuditLog log = new AuditLog
                    {
                        ActiveType = LogConstant.UnPublicNews,
                        FunctionName = LogConstant.ManageNews,
                        Information = unPublicNews.ToJson(),
                        DataTable = LogConstant.News,
                        UserName = userCurent.FullName + "(" + userCurent.UserName + ")",
                        CreatedBy = WebSecurity.CurrentUserId,
                        CreatedAt = DateTime.Now
                    };
                    db.AuditLogs.Add(log);
                    db.SaveChanges();

                    transaction.Commit();
                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    transaction.Commit();

                    return Json(new { success = false });
                }
            }
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var delete = db.News.Find(id);
                    delete.ModifiedAt = DateTime.Now;
                    delete.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    delete.IsActive = false;
                    db.SaveChanges();

                    var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                    AuditLog log = new AuditLog
                    {
                        ActiveType = LogConstant.DeleteNews,
                        FunctionName = LogConstant.ManageNews,
                        Information = delete.ToJson(),
                        DataTable = LogConstant.News,
                        UserName = userCurent.FullName + "(" + userCurent.UserName + ")",
                        CreatedBy = WebSecurity.CurrentUserId,
                        CreatedAt = DateTime.Now
                    };
                    db.AuditLogs.Add(log);
                    db.SaveChanges();

                    transaction.Commit();
                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    transaction.Rollback();

                    return Json(new { success = false });
                }
            }
        }

        [HttpPost]
        public JsonResult Deletes(string data)
        {
            var listDataConvert = ConvertJsonDeleteData(data);

            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();

                    var userDelete = db.News.Where(x => listDataConvert.Contains(x.Id));
                    foreach (var item in userDelete)
                    {
                        item.ModifiedAt = DateTime.Now;
                        item.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                        item.IsActive = false;
                    }

                    db.SaveChanges();

                    var options = new JsonSerializerOptions
                    {
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        WriteIndented = true
                    };

                    var jsonString = System.Text.Json.JsonSerializer.Serialize(userDelete, options);
                    AuditLog log = new AuditLog
                    {
                        ActiveType = LogConstant.DeleteNews,
                        FunctionName = LogConstant.ManageNews,
                        Information = userDelete.ToJson(),
                        DataTable = LogConstant.News,
                        UserName = userCurent.FullName + "(" + userCurent.UserName + ")",
                        CreatedBy = WebSecurity.CurrentUserId,
                        CreatedAt = DateTime.Now
                    };
                    db.AuditLogs.Add(log);
                    db.SaveChanges();

                    transaction.Commit();
                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    transaction.Rollback();

                    return Json(new { success = false });
                }
            }
        }

        public List<int> ConvertJsonDeleteData(string data)
        {
            var listData = new List<int>();
            var dataObjTrim = data.Trim('[').Trim(']');
            if (dataObjTrim != "")
            {
                listData = (dataObjTrim.Split(',').ToList()).Select(int.Parse).ToList();
            }
            else
            {
                listData.Add(0);
            }

            return listData;
        }


        public ActionResult Undo()
        {
            return View();
        }


        [HttpPost]
        public JsonResult Undo(string data)
        {
            var listDataConvert = ConvertJsonDeleteData(data);

            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var news = db.News.Where(x => listDataConvert.Contains(x.Id));
                    foreach (var item in news)
                    {
                        item.ModifiedAt = DateTime.Now;
                        item.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                        item.IsActive = true;
                    }

                    db.SaveChanges();

                    var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();

                    var options = new JsonSerializerOptions
                    {
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        WriteIndented = true
                    };

                    var jsonString = System.Text.Json.JsonSerializer.Serialize(news, options);

                    AuditLog log = new AuditLog
                    {
                        ActiveType = LogConstant.UndoNews,
                        FunctionName = LogConstant.ManageNews,
                        Information = jsonString.ToJson(),
                        DataTable = LogConstant.News,
                        UserName = userCurent.FullName + "(" + userCurent.UserName + ")",
                        CreatedBy = WebSecurity.CurrentUserId,
                        CreatedAt = DateTime.Now
                    };
                    db.AuditLogs.Add(log);
                    db.SaveChanges();

                    transaction.Commit();
                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    transaction.Rollback();

                    return Json(new { success = false });
                }
            }
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult CheckBuyerValid(BuyAndSellBond model)
        {
            var getBuyerValid = db.UserProfiles.Where(x => x.UserId == model.CustomerId && x.IsActive != false).Select(x => x.UserId).ToList();



            return Json(new { ErrorMessage = string.Empty, GetBuyerValid = getBuyerValid }, JsonRequestBehavior.AllowGet);
        }
    }
}
