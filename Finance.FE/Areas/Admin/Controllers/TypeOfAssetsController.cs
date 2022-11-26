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

namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public class TypeOfAssetsController : BaseController
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
        public ActionResult TypeOfAssets_Read([DataSourceRequest] DataSourceRequest request)
        {
            var assets = db.TypeOfAssets.AsNoTracking().OrderByDescending(x => x.CreatedAt).Select(x => new { x.ID, x.AssetId, x.AssetName, x.Note, x.IsActive }).Where(x => x.IsActive == true).ToList();
            return Json(assets.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        public ActionResult Read_UndoData([DataSourceRequest] DataSourceRequest request)
        {
            var assets = from x in db.TypeOfAssets.Where(x => x.AssetId != null && x.IsActive == false)
                        select new { x.ID, x.AssetId, x.AssetName, x.Note };
            var testData = assets.ToList();
            return Json(assets.ToDataSourceResult(request));
        }

        public ActionResult Read_Data([DataSourceRequest] DataSourceRequest request)
        {
            var assets = from x in db.TypeOfAssets.Where(x => x.AssetId != null  && x.IsActive != false)
                        select new { x.ID, x.AssetId, x.AssetName, x.Note };
            return Json(assets.ToDataSourceResult(request));
        }


        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(/*[Bind(Exclude = "ID")]*/ TypeOfAsset model)
        {
            if (ModelState.IsValid)
            {


                var assetId = (from p in db.Set<TypeOfAsset>().AsNoTracking()
                               where p.AssetId.Equals(model.AssetId, StringComparison.OrdinalIgnoreCase)
                               select p).FirstOrDefault();
                if (assetId != null)
                {
                    ModelState.AddModelError("AssetId", "Mã tài sản đã tồn tại");

                    return View(model);
                }
                else
                {
                    var assetName = (from p in db.Set<TypeOfAsset>().AsNoTracking()
                                     where p.AssetName.Equals(model.AssetName, StringComparison.OrdinalIgnoreCase)
                                     select p).FirstOrDefault();
                    if (assetName != null)
                    {
                        ModelState.AddModelError("AssetName", "Tên tài sản đã tồn tại");

                        return View(model);
                    }
                    else
                    {
                        model.CreatedBy = WebSecurity.CurrentUserId;
                        model.CreatedAt = DateTime.Now;
                        model.IsActive = true;
                        //model.ModifiedBy = WebSecurity.CurrentUserId;
                        //model.ModifiedAt = DateTime.Now;
                        db.Set<TypeOfAsset>().Add(model);
                        db.SaveChanges();


                        //(new WebModels.AccessLog("Entity: Role, Item: " + model.RoleId + ": " + model.RoleName, WebModels.AccessLogActions.Add.ToString(), WebSecurity.CurrentUserId + ":" + WebSecurity.CurrentUserName)).Insert();

                        ViewBag.StartupScript = "create_success();";
                        return View(model);
                    }

                }

            }
            else
            {
                return View(model);
            }
        }

        public ActionResult Edit(int id)
        {

            var assets = db.Set<TypeOfAsset>().Find(id);
            if (assets == null)
            {
                return HttpNotFound();
            }
            ViewBag.assetId = assets.AssetId;
            ViewBag.assetName = assets.AssetName;
            //ViewBag.Roles = Roles.GetRolesForUser(assets.UserName);
            return View("Edit", assets);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TypeOfAsset model, string assetId, string assetName)
        {
            if (ModelState.IsValid)
            {
                var checkValidate = true;
                var asset = db.TypeOfAssets.AsNoTracking().ToList();
                var checkAssetIdExist = asset.Where(p => p.ID != model.ID && p.AssetId == model.AssetId).Any();
                if (checkAssetIdExist)
                {
                    ModelState.AddModelError("AssetId", "Mã tài sản đã tồn tại trên hệ thống");
                    checkValidate = false;
                }

                var checkAssetNameExist = asset.Where(p => p.ID != model.ID && p.AssetName.Equals(model.AssetName, StringComparison.OrdinalIgnoreCase)).Any();
                if (checkAssetNameExist)
                {
                    ModelState.AddModelError("AssetName", "Tên tài sản đã tồn tại trên hệ thống");
                    checkValidate = false;
                }

                if (!checkValidate) return View(model);

                model.ModifiedAt = DateTime.Now;
                model.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;

                db.TypeOfAssets.Attach(model);
                db.Entry(model).Property(a => a.AssetId).IsModified = true;
                db.Entry(model).Property(a => a.AssetName).IsModified = true;
                db.Entry(model).Property(a => a.Note).IsModified = true;
                db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                db.Entry(model).Property(a => a.ModifiedAt).IsModified = true;


                db.SaveChanges();

                ViewBag.StartupScript = "edit_success();";
                return View(model);

            }
            else
            {
                return View(model);
            }
            
        }

       
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var assetsDelete = db.TypeOfAssets.Find(id);
                assetsDelete.ModifiedAt = DateTime.Now;
                assetsDelete.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                assetsDelete.IsActive = false;
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });

            }
        }
        /*      
                [HttpPost]
                [ValidateAntiForgeryToken]
                public ActionResult Delete(TypeOfAsset model)
                {
                    try
                    {

                        //var log = (new WebModels.AccessLog("Entity: Role, Item: " + model.RoleId + ": " + model.RoleName, WebModels.AccessLogActions.Delete.ToString(), WebSecurity.CurrentUserId + ":" + WebSecurity.CurrentUserName));
                        *//*var assets = db.TypeOfAssets.Attach(model);
                        db.Set<TypeOfAsset>().Remove(assets);*//*

                        TypeOfAsset sl = db.TypeOfAssets.FirstOrDefault(x => x.ID == model.ID);
                        sl.IsActive = false;

                        db.SaveChanges();


                        //log.Insert();
                        ViewBag.StartupScript = "delete_success();";
                        return View(model);
                    }
                    catch (Exception ex)
                    {

                        ModelState.AddModelError("", ex.Message);
                        return View(model);
                    }
                }*/

        /* public ActionResult Deletes(string id)
         {
             var objects = id.Split(',');
             var lstAssetsId = new List<int>();
             foreach (var obj in objects)
             {
                 try
                 {
                     lstAssetsId.Add(int.Parse(obj.ToString()));
                 }
                 catch (Exception)
                 { }
             }
             var temp = from p in db.Set<TypeOfAsset>()
                        where lstAssetsId.Contains(p.ID)
                        select p;
             return View(temp.ToList());

         }*/
        [HttpPost]
        public JsonResult Deletes(string data)
        {
            try
            {
                var listDataConvert = StringToListInt(data);
                var listData = db.TypeOfAssets.Where(x => listDataConvert.Contains(x.ID));

                foreach (var item in listData)
                {
                    item.IsActive = false;
                    item.ModifiedAt = DateTime.Now;
                    item.ModifiedBy = WebSecurity.CurrentUserId;
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

        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<TypeOfAsset> model)
        {
            var temp = new List<TypeOfAsset>();
            foreach (var item in model)
            {
                try
                {

                   
                    db.Entry(item).State = EntityState.Deleted;
                    db.SaveChanges();
                    //var log = new WebModels.AccessLog("Entity: Role, Item: " + item.RoleId + ":" + item.RoleName, WebModels.AccessLogActions.Delete.ToString(), WebSecurity.CurrentUserId + ":" + WebSecurity.CurrentUserName);

                }
                catch (Exception)
                {
                    db.Entry(item).State = EntityState.Unchanged;
                    temp.Add(item);
                }
            }

            if (temp.Count == 0)
            {
                ViewBag.StartupScript = "deletes_success();";
                return View(temp);
            }
            else if (temp.Count > 0)
            {
                ViewBag.StartupScript = "top.$('#grid').data('kendoGrid').dataSource.read();";
                ModelState.AddModelError("", Resources.Common.ErrorDeleteItems);
                return View(temp);
            }
            else
            {
                ViewBag.StartupScript = "deletes_success();";
                return View(model);
            }

        }*/
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
                var assetDelete = db.TypeOfAssets.Where(x => listDataConvert.Contains(x.ID));
                foreach (var item in assetDelete)
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




        #region Module and Site

        public ActionResult AdminSitesMapping(int id)
        {
            var role = db.WebRoles.Find(id);
            if (role == null)
                return HttpNotFound();

            var temp = role.AccessAdminSiteRoles.Select(x => x.AdminSite).Where(x => x != null).Select(y => new { ID = y.ID }).ToArray();
            string[] accesses = temp.Count() > 0 ? new string[temp.Count()] : new string[0];
            for (int i = 0; i < temp.Count(); i++)
                accesses[i] = temp[i].ID.ToString();
            ViewBag.Tree = GetAdminSitesTree();
            ViewBag.RoleId = id;
            return View(accesses);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdminSitesMapping(int id, string[] checkedNodes)
        {
            var role = db.WebRoles.Find(id);
            if (role == null)
                return HttpNotFound();

            List<int> lstSiteID = new List<int>();
            try
            {
                using (var ts = new TransactionScope())
                {
                    if (checkedNodes != null && checkedNodes.Count() > 0)
                        foreach (var x in checkedNodes)
                            lstSiteID.Add(int.Parse(x));
                    role.AccessAdminSiteRoles.Clear();
                    if (lstSiteID.Count > 0)
                        foreach (var x in lstSiteID)
                            role.AccessAdminSiteRoles.Add(new AccessAdminSiteRole() { AdminSiteID = x });

                    db.SaveChanges();
                    ts.Complete();
                    ViewBag.StartupScript = "create_success();";

                    // clear cache
                    var sessionKey = "AdminSite-" + Culture + "-" + HttpContext.User.Identity.Name;
                    Session[sessionKey] = null;

                    return View();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var temp = role.AccessAdminSiteRoles.Select(x => x.AdminSite).Select(x => new { ID = x.ID }).ToArray();
                string[] accesses = temp.Count() > 0 ? new string[temp.Count()] : new string[0];
                for (int i = 0; i < temp.Count(); i++)
                    accesses[i] = temp[i].ID.ToString();
                ViewBag.Tree = GetAdminSitesTree();
                ViewBag.RoleId = id;
                return View(accesses);
            }
        }

        [AllowAnonymous]
        public JsonResult GetAccessAdminSites(int? id)
        {
            var adminSites = from e in db.AdminSites.AsNoTracking()
                             where (id.HasValue ? e.ParentID == id : e.ParentID == null)
                             select new
                             {
                                 id = e.ID,
                                 Name = e.Title,
                                 hasChildren = e.SubAdminSites.Any()
                             };

            return Json(adminSites, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        private IEnumerable<TreeViewItemModel> GetAdminSitesTree()
        {
            List<TreeViewItemModel> tree = new List<TreeViewItemModel>();
            var roots = db.AdminSites.AsNoTracking().Where(x => x.ParentID == null).AsEnumerable();
            if (roots.Count() > 0)
                foreach (var item in roots)
                {
                    TreeViewItemModel node = new TreeViewItemModel();
                    node.Id = item.ID.ToString();
                    node.Text = item.Title;
                    node.HasChildren = item.SubAdminSites.Any();
                    if (node.HasChildren)
                        SubAdminSitesTree(ref node, ref tree);
                    tree.Add(node);
                }
            return tree;
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

        public ActionResult ModulesMapping(int id)
        {
            ViewBag.RoleId = id;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult GetModulesPermissions(int id)
        {
            var role = db.WebRoles.Find(id);
            if (role == null)
                return Json(null);

            var modulesPermissions = role.AccessWebModuleRoles
                .Select(x =>
                    new
                    {
                        WebModuleID = x.WebModuleID,
                        View = x.View,
                        Add = x.Add,
                        Edit = x.Edit,
                        Delete = x.Delete
                    });
            return Json(modulesPermissions, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateModulePerm(AccessWebModuleRole data)
        {
            if (ModelState.IsValid)
            {
                var item = db.AccessWebModuleRoles.AsNoTracking().SingleOrDefault(x => x.RoleId == data.RoleId && x.WebModuleID == data.WebModuleID);
                if (item != null)
                {
                    db.Entry(item).State = EntityState.Detached;
                    db.AccessWebModuleRoles.Attach(data);
                    db.Entry(data).Property(x => x.View).IsModified = true;
                    db.Entry(data).Property(x => x.Add).IsModified = true;
                    db.Entry(data).Property(x => x.Edit).IsModified = true;
                    db.Entry(data).Property(x => x.Delete).IsModified = true;
                    db.SaveChanges();
                }
                else
                {
                    db.AccessWebModuleRoles.Add(data);
                    db.SaveChanges();
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public JsonResult UpdateModulePermAll(AccessWebModuleRole data)
        {
            if (ModelState.IsValid)
            {
                var allWebModules = db.WebModules.ToList();

                foreach (var webModule in allWebModules)
                {
                    var item = db.AccessWebModuleRoles.SingleOrDefault(x => x.RoleId == data.RoleId && x.WebModuleID == webModule.ID);
                    if (item != null)
                    {
                        if (data.CheckAllName == "chkview")
                        {
                            item.View = data.View;
                            db.Entry(item).Property(x => x.View).IsModified = true;
                        }
                        else if (data.CheckAllName == "chkadd")
                        {
                            item.Add = data.Add;
                            db.Entry(item).Property(x => x.Add).IsModified = true;
                        }
                        else if (data.CheckAllName == "chkedit")
                        {
                            item.Edit = data.Edit;
                            db.Entry(item).Property(x => x.Edit).IsModified = true;
                        }
                        else if (data.CheckAllName == "chkdelete")
                        {
                            item.Delete = data.Delete;
                            db.Entry(item).Property(x => x.Delete).IsModified = true;
                        }
                    }
                    else
                    {
                        db.AccessWebModuleRoles.Add(new AccessWebModuleRole
                        {
                            RoleId = data.RoleId,
                            WebModuleID = webModule.ID,
                            View = data.CheckAllName == "chkview" ? data.View : false,
                            Add = data.CheckAllName == "chkadd" ? data.Add : false,
                            Edit = data.CheckAllName == "chkedit" ? data.Edit : false,
                            Delete = data.CheckAllName == "chkdelete" ? data.Delete : false
                        });
                    }
                }

                db.SaveChanges();

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        #endregion
    }
}
