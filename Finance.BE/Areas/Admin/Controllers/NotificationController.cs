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
namespace WEB.Areas.Admin.Controllers
{
    [VanTaiAuthorize]
    public class NotificationController : BaseController
    {

        // GET: Admin/Notification
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        public ActionResult Index()
        {
            var unreadNoti = db.Notification.Where(x => x.IsRead != true).ToList();
            foreach (var item in unreadNoti)
            {
                item.IsRead = true;
            }
            db.SaveChanges();
            return View();
        }

        [AllowAnonymous]
        public ActionResult Noti_Read([DataSourceRequest] DataSourceRequest request)
        {
            var noti = db.Notification.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToList().Select(x => new
            {
                x.ID,
                x.Title,
                x.Content,
                x.Type,
                x.CreatedAt

            });

            return Json(noti.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);


        }


        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var notidel = db.Notification.Find(id);
                db.Set<Notification>().Remove(notidel);
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
            try
            {
                var listDataConvert = StringToListInt(data);
                var listData = db.Notification.Where(x => listDataConvert.Contains(x.ID));

                foreach (var item in listData)
                {
                    var notidel = db.Notification.Find(item.ID);
                    db.Set<Notification>().Remove(notidel);
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
    }
}