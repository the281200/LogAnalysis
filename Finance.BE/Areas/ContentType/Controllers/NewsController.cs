using Kendo.Mvc.Extensions;
using System.Linq;
using System.Web.Mvc;
using WebModels;
using WebMatrix.WebData;
using System.Data;
namespace WEB.Areas.ContentType.Controllers
{
    public class NewsController : Controller
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        // GET: ContentType/News
        public ActionResult _PubIndex(int? id)
        {
            var user = WebSecurity.GetUserId(User.Identity.Name);
            ViewBag.WebModule = db.WebModules.FirstOrDefault(x => x.ID == id);
            ViewBag.ListNew = db.News.Where(x => x.IsActive != false && x.IsPublish == true && x.CustomerId == user).OrderByDescending(x => x.CreatedAt).ToList();
            return View();
        }
    }
}