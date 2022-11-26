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
            return Json(assets.Select(x => new
            {
                Id = x.Id,
                Name = x.ContractName
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
                try
                {
                    var checkValidate = true;
                    if (image != null)
                    {
                        var now = DateTime.Now;
                        model.Image = image.ImageSave("/uploads/image/" + (now.Month.ToString("00") + now.Year), 1366, 1366);
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

                    if(model.Body == null)
                    {
                        ModelState.AddModelError("Body", "Vui lòng không để trống nội dung");
                        checkValidate = false;
                    } 

                    if(!checkValidate)  return View(model);

                    model.CreatedAt = DateTime.Now;
                    model.CreatedBy = WebSecurity.CurrentUserId;
                    model.IsActive = true;
                    model.IsPublish = true;

                    db.News.Add(model);
                    db.SaveChanges();

                    ViewBag.StartupScript = "create_success();";
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Đã có lỗi xảy ra trong quá trình lưu! Vui lòng thử lại.");
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
                try
                {
                    var checkValidate = true;

                    if (image != null)
                    {
                        var now = DateTime.Now;
                        model.Image = image.ImageSave("/uploads/image/" + (now.Month.ToString("00") + now.Year), 1366, 1366);
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
                        if(model.Image == null)
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

                    ViewBag.StartupScript = "edit_success();";
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Đã có lỗi xảy ra trong quá trình lưu! Vui lòng thử lại.");
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
            try
            {
                var delete = db.News.Find(id);
                delete.ModifiedAt = DateTime.Now;
                delete.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                delete.IsPublish = true;
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public JsonResult UnPublic(int? id)
        {
            try
            {
                var delete = db.News.Find(id);
                delete.ModifiedAt = DateTime.Now;
                delete.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                delete.IsPublish = false;
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var delete = db.News.Find(id);
                delete.ModifiedAt = DateTime.Now;
                delete.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                delete.IsActive = false;
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public JsonResult Deletes(string data)
        {
            var listDataConvert = ConvertJsonDeleteData(data);

            try
            {
                var userDelete = db.News.Where(x => listDataConvert.Contains(x.Id));
                foreach (var item in userDelete)
                {
                    item.ModifiedAt = DateTime.Now;
                    item.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    item.IsActive = false;
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

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }
        }
    }
}
