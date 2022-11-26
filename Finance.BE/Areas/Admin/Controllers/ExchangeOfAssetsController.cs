using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebModels;
using WebMatrix.WebData;
using System.Data;
using WEB.Models;
using System.Data.Entity;
using System.Transactions;
using System.Globalization;
using System.Text.RegularExpressions;

namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public class ExchangeOfAssetsController : Controller
    {
        WebContext db = new WebContext();

        public string Culture { get; private set; }

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
        public ActionResult ExchangeOfAssets_Read([DataSourceRequest] DataSourceRequest request)
        {
            var incurred = db.ExchangeOfAssets.AsNoTracking().ToList().Select(x => new
            {
                x.ID,
                x.IncurredDate,
                SellerShow = x.SellerUser != null ? x.SellerUser.FullName : "",
                BuyerShow = x.BuyerUser != null ? x.BuyerUser.FullName : "",
                AssetShow = x.BuyAndSellBond != null ? x.BuyAndSellBond.ContractName : "",
                x.Value,
                x.Price,
                x.Interest,
                x.Note,
                x.Number,
                x.IsActive,
                x.CreatedAt,
                x.UnitPrice,
                AssetCategorysId = x.BuyAndSellBond.AssetCategorysId.ToString()

            }).Where(x => x.IsActive == true).OrderByDescending(x => x.CreatedAt).ToList();
            return Json(incurred.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        public ActionResult Read_UndoData([DataSourceRequest] DataSourceRequest request)
        {
            var incurred = db.ExchangeOfAssets.AsNoTracking().ToList().Select(x => new
            {
                x.ID,
                x.IncurredDate,
                SellerShow = x.SellerUser != null ? x.SellerUser.FullName : "",
                BuyerShow = x.BuyerUser != null ? x.BuyerUser.FullName : "",
                AssetShow = x.BuyAndSellBond != null ? x.BuyAndSellBond.ContractName : "",
                x.Value,
                x.Price,
                x.Interest,
                x.Number,
                x.Note,
                x.IsActive,
                x.UnitPrice,
                AssetCategorysId = x.BuyAndSellBond.AssetCategorysId.ToString()

            }).Where(x => x.IsActive == false);
            return Json(incurred.ToDataSourceResult(request));
        }

        public ActionResult Read_Data([DataSourceRequest] DataSourceRequest request)
        {
            var incurred = db.ExchangeOfAssets.AsNoTracking().ToList().Select(x => new
            {
                x.ID,
                x.IncurredDate,
                SellerShow = x.SellerUser != null ? x.SellerUser.FullName : "",
                BuyerShow = x.BuyerUser != null ? x.BuyerUser.FullName : "",
                AssetShow = x.BuyAndSellBond != null ? x.BuyAndSellBond.ContractName : "",
                x.Value,
                x.Price,
                x.Interest,
                x.Number,
                x.Note,
                x.IsActive,
                x.UnitPrice,
                AssetCategorysId = x.BuyAndSellBond.AssetCategorysId.ToString()

            }).Where(x => x.IsActive == true);
            return Json(incurred.ToDataSourceResult(request));
        }



        [AllowAnonymous]
        public JsonResult Get_Seller()
        {
            var users = from x in db.UserProfiles.AsNoTracking() where (x.IsActive != false && x.Type != (int)TypeAccount.Admin) select x;
            return Json(users.Select(x => new
            {
                Id = x.UserId,
                Name = x.Mobile + " - " + x.FullName
            }), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public JsonResult Get_Buyer(int? Seller)
        {

            var users = from x in db.UserProfiles.AsNoTracking() where (x.IsActive != false && x.Type != (int)TypeAccount.Admin && x.UserId != Seller) select x;
            return Json(users.Select(x => new
            {
                Id = x.UserId,
                Name = x.Mobile + " - " + x.FullName
            }), JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public JsonResult Get_Asset(int? Seller, string assetFilter)
        {
            var assets = from x in db.BuyAndSellBonds.AsNoTracking() where (x.IsActive != false && x.CustomerId == Seller) select x;
            return Json(assets.ToList().Select(x => new
            {
                Id = x.Id,
                Name = x.ContractCode + " - " + x.ContractName
            }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(/*[Bind(Exclude = "ID")]*/ ExchangeOfAssetViewModel model)
        {
            if (ModelState.IsValid)
            {
                var checkValidate = true;

                var getValueOfAsset = db.BuyAndSellBonds.Where(x => x.Id == model.Asset).Select(y => y.Value).FirstOrDefault();


                if (model.Value <= 0 || model.Value > getValueOfAsset)
                {
                    ModelState.AddModelError("Value", "Giá trị phải lớn hơn 0 và nhỏ hơn giá trị của Tài sản!");
                    checkValidate = false;
                }

                if (model.Price <= 0)
                {
                    ModelState.AddModelError("Price", "Giá bán phải lớn hơn 0!");
                    checkValidate = false;
                }
                if (model.Number <= 0)
                {
                    ModelState.AddModelError("Number", "Số lượng phải lớn hơn 0!");
                    checkValidate = false;
                }

                if (model.UnitPrice <= 0)
                {
                    ModelState.AddModelError("UnitPrice", "Đơn giá chuyển nhượng phải lớn hơn 0!");
                    checkValidate = false;
                }

                if (model.InterestFloat == null)
                {
                    model.InterestFloat = "0";
                }
                Match match = Regex.Match(model.InterestFloat, @"(\d+)");
                if (!match.Success)
                {
                    ModelState.AddModelError("InterestFloat", "Lãi suất chuyển nhượng phải là số!");
                    checkValidate = false;
                }
                Match comma = Regex.Match(model.InterestFloat, @"(\,+)");
                if (comma.Success)
                {
                    ModelState.AddModelError("InterestFloat", "Lãi suất chuyển nhượng phải là số thập phân có dạng x.yz");
                    checkValidate = false;
                }

                if (!checkValidate) return View(model);

                string value = model.InterestFloat;
                NumberStyles style;
                CultureInfo provider;

                /*style = NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands;*/
                style = NumberStyles.AllowDecimalPoint;
                provider = new CultureInfo("en-US");

                model.Interest = Decimal.Parse(value, style, provider);
                model.CreatedBy = WebSecurity.CurrentUserId;
                model.CreatedAt = DateTime.Now;
                model.IsActive = true;

                var modelAdd = new ExchangeOfAsset()
                {
                    Interest = model.Interest,
                    CreatedBy = model.CreatedBy,
                    CreatedAt = model.CreatedAt,
                    IsActive = model.IsActive,
                    IncurredDate = model.IncurredDate,
                    CustomerId = model.CustomerId,
                    BuyAndSellBondId = model.BuyAndSellBondId,
                    Seller = model.Seller,
                    Buyer = model.Buyer,
                    Asset = model.Asset,
                    Value = model.Value,
                    Price = model.Price,
                    Number = model.Number,
                    Note = model.Note,
                    UnitPrice = model.UnitPrice
                    
                };

                db.Set<ExchangeOfAsset>().Add(modelAdd);
                db.SaveChanges();

                var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                AuditLog log = new AuditLog
                {
                    ActiveType = LogConstant.AddExchangeOfAssets,
                    FunctionName = LogConstant.ManageExchangeOfAssets,
                    Information = modelAdd.ToJson(),
                    DataTable = LogConstant.ExchangeOfAssets,
                    UserName = userCurent.FullName + "(" + userCurent.UserName + ")",
                    CreatedBy = WebSecurity.CurrentUserId,
                    CreatedAt = DateTime.Now
                };
                db.AuditLogs.Add(log);
                db.SaveChanges();

                ViewBag.StartupScript = "create_success();";
                return View(model);
            }
            else
            {
                return View(model);
            }
        }

        public ActionResult Edit(int id)
        {

            var incurred = db.Set<ExchangeOfAsset>().Find(id);
            var number = incurred.Number;
            var price = incurred.Price;
            var value = incurred.Value;
            var stringunitprice = incurred.UnitPrice;


            var AssetCategorysId = db.BuyAndSellBonds.Where(x => x.Id == incurred.Asset).Select(x => x.AssetCategorysId).FirstOrDefault();
            var AssetCategorysName = db.BuyAndSellBonds.Where(x => x.Id == incurred.Asset).Select(x => x.AssetCategorys.Name).FirstOrDefault();
            var AssetCategory = AssetCategorysId.ToString() + " - " + AssetCategorysName;

            string stringNumber = String.Format(new CultureInfo("en-US"), "{0:#,##0.##}", number);
            string stringValue = String.Format(new CultureInfo("en-US"), "{0:#,##0.##}", value);
            string stringPrice = String.Format(new CultureInfo("en-US"), "{0:#,##0.##}", price);

            string stringUnitPrice = String.Format(new CultureInfo("en-US"), "{0:#,##0.##}", stringunitprice);
           
            var exViewModel = new ExchangeOfAssetViewModel()
            {
                InterestFloat = incurred.Interest.Value.ToString(CultureInfo.CreateSpecificCulture("en-US")),
                /*CreatedBy = model.CreatedBy,
                CreatedAt = model.CreatedAt,*/
                IsActive = incurred.IsActive,
                IncurredDate = incurred.IncurredDate,
                CustomerId = incurred.CustomerId,
                BuyAndSellBondId = incurred.BuyAndSellBondId,
                Seller = incurred.Seller,
                Number = incurred.Number,
                Buyer = incurred.Buyer,
                Asset = incurred.Asset,
                Value = incurred.Value,
                Price = incurred.Price,
                Note = incurred.Note,
                ID = incurred.ID,
                StringValue = stringValue,
                StringNumber = stringNumber,
                StringPrice = stringPrice,
                UnitPrice = incurred.UnitPrice,
                StringUnitPrice = stringUnitPrice,
                AssetCategorysId = AssetCategory


            };

            if (incurred == null)
            {
                return HttpNotFound();
            }

            ViewBag.Seller = incurred.Seller;

            //ViewBag.Roles = Roles.GetRolesForUser(assets.UserName);
            return View("Edit", exViewModel);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ExchangeOfAssetViewModel model, string assetId, DateTime? incurred)
        {
            if (ModelState.IsValid)
            {
                var checkValidate = true;

                var getValueOfAsset = db.BuyAndSellBonds.Where(x => x.Id == model.Asset).Select(y => y.Value).FirstOrDefault();


                if (model.Value <= 0 || model.Value > getValueOfAsset)
                {
                    ModelState.AddModelError("Value", "Giá trị phải lớn hơn 0 và nhỏ hơn giá trị của Tài sản!");
                    checkValidate = false;
                }

                if (model.Price <= 0)
                {
                    ModelState.AddModelError("Price", "Giá bán phải lớn hơn 0!");
                    checkValidate = false;
                }
                if (model.Number <= 0)
                {
                    ModelState.AddModelError("Number", "Số lượng phải lớn hơn 0!");
                    checkValidate = false;
                }
                if (model.UnitPrice <= 0)
                {
                    ModelState.AddModelError("UnitPrice", "Đơn giá chuyển nhượng phải lớn hơn 0!");
                    checkValidate = false;
                }

               

                if (model.InterestFloat == null)
                {
                    model.InterestFloat = "0";
                }
                Match match = Regex.Match(model.InterestFloat, @"(\d+)");
                if (!match.Success)
                {
                    ModelState.AddModelError("InterestFloat", "Lãi suất chuyển nhượng phải là số!");
                    checkValidate = false;
                }
                Match comma = Regex.Match(model.InterestFloat, @"(\,+)");
                if (comma.Success)
                {
                    ModelState.AddModelError("InterestFloat", "Lãi suất chuyển nhượng phải là số thập phân có dạng x.yz");
                    checkValidate = false;
                }

                if (!checkValidate) return View(model);

                string value = model.InterestFloat;
                NumberStyles style;
                CultureInfo provider;

                /*style = NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands;*/
                style = NumberStyles.AllowDecimalPoint;
                provider = new CultureInfo("en-US");

                model.Interest = Decimal.Parse(value, style, provider);
                model.ModifiedAt = DateTime.Now;
                model.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;

                var modelAdd = new ExchangeOfAsset()
                {
                    Interest = model.Interest,
                    ModifiedBy = model.ModifiedBy,
                    ModifiedAt = model.ModifiedAt,
                    IsActive = model.IsActive,
                    IncurredDate = incurred,
                    CustomerId = model.CustomerId,
                    BuyAndSellBondId = model.BuyAndSellBondId,
                    Seller = model.Seller,
                    Number = model.Number,
                    Buyer = model.Buyer,
                    Asset = model.Asset,
                    Value = model.Value,
                    Price = model.Price,
                    Note = model.Note,
                    ID = model.ID,
                    UnitPrice = model.UnitPrice,
                   

                };

                db.ExchangeOfAssets.Attach(modelAdd);
                db.Entry(modelAdd).Property(a => a.Seller).IsModified = true;
                db.Entry(modelAdd).Property(a => a.IncurredDate).IsModified = true;
                db.Entry(modelAdd).Property(a => a.Buyer).IsModified = true;
                db.Entry(modelAdd).Property(a => a.Asset).IsModified = true;
                db.Entry(modelAdd).Property(a => a.Value).IsModified = true;
                db.Entry(modelAdd).Property(a => a.Price).IsModified = true;
                db.Entry(modelAdd).Property(a => a.Number).IsModified = true;
                db.Entry(modelAdd).Property(a => a.Interest).IsModified = true;
                db.Entry(modelAdd).Property(a => a.Note).IsModified = true;
                db.Entry(modelAdd).Property(a => a.ModifiedBy).IsModified = true;
                db.Entry(modelAdd).Property(a => a.ModifiedAt).IsModified = true;
                db.Entry(modelAdd).Property(a => a.UnitPrice).IsModified = true;
              


                db.SaveChanges();

                var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                AuditLog log = new AuditLog
                {
                    ActiveType = LogConstant.EditExchangeOfAssets,
                    FunctionName = LogConstant.ManageExchangeOfAssets,
                    Information = modelAdd.ToJson(),
                    DataTable = LogConstant.ExchangeOfAssets,
                    UserName = userCurent.FullName + "(" + userCurent.UserName + ")",
                    CreatedBy = WebSecurity.CurrentUserId,
                    CreatedAt = DateTime.Now
                };
                db.AuditLogs.Add(log);
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
                var incurredDelete = db.ExchangeOfAssets.Find(id);
                incurredDelete.ModifiedAt = DateTime.Now;
                incurredDelete.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                incurredDelete.IsActive = false;
                db.SaveChanges();

                var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                AuditLog log = new AuditLog
                {
                    ActiveType = LogConstant.DeleteExchangeOfAssets,
                    FunctionName = LogConstant.ManageExchangeOfAssets,
                    Information = incurredDelete.ToJson(),
                    DataTable = LogConstant.ExchangeOfAssets,
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
        public JsonResult Deletes(string data)
        {
            try
            {
                var listDataConvert = StringToListInt(data);
                var listData = db.ExchangeOfAssets.Where(x => listDataConvert.Contains(x.ID));

                foreach (var item in listData)
                {
                    item.IsActive = false;
                    item.ModifiedAt = DateTime.Now;
                    item.ModifiedBy = WebSecurity.CurrentUserId;
                }

                db.SaveChanges();

                var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                AuditLog log = new AuditLog
                {
                    ActiveType = LogConstant.DeleteExchangeOfAssets,
                    FunctionName = LogConstant.ManageExchangeOfAssets,
                    Information = listData.ToJson(),
                    DataTable = LogConstant.ExchangeOfAssets,
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
                var assetDelete = db.ExchangeOfAssets.Where(x => listDataConvert.Contains(x.ID));
                foreach (var item in assetDelete)
                {
                    item.ModifiedAt = DateTime.Now;
                    item.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    item.IsActive = true;
                }

                db.SaveChanges();

                var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                AuditLog log = new AuditLog
                {
                    ActiveType = LogConstant.UndoExchangeOfAssets,
                    FunctionName = LogConstant.ManageExchangeOfAssets,
                    Information = assetDelete.ToJson(),
                    DataTable = LogConstant.ExchangeOfAssets,
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
        [AllowAnonymous]
        public ActionResult FindInterest(BuyAndSellBond model)
        {
            var interest = db.BuyAndSellBonds.Where(x => x.Id == model.Id).Select(x => x.InterestRate).FirstOrDefault();
            var AssetCategorysId = db.BuyAndSellBonds.Where(x => x.Id == model.Id).Select(x => x.AssetCategorysId).FirstOrDefault();
            var AssetCategorysName = db.BuyAndSellBonds.Where(x => x.Id == model.Id).Select(x => x.AssetCategorys.Name).FirstOrDefault();
            var AssetCategory = AssetCategorysId.ToString() +" - "+ AssetCategorysName;
            return Json(new { ErrorMessage = string.Empty, Interest = interest, AssetCategory = AssetCategory }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult CheckSellerValid(BuyAndSellBond model)
        {
            var getUserValid = db.UserProfiles.Where(x => x.UserId == model.CustomerId && x.IsActive != false).Select(x => x.UserId).ToList();

            var getAsset = db.BuyAndSellBonds.Where(x => x.CustomerId == model.CustomerId && x.IsActive == true).Select(x => x.Id).ToList();

            return Json(new { ErrorMessage = string.Empty, GetAsset = getAsset, GetUserValid = getUserValid }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult CheckBuyerValid(BuyAndSellBond model)
        {
            var getBuyerValid = db.UserProfiles.Where(x => x.UserId == model.CustomerId && x.IsActive != false).Select(x => x.UserId).ToList();

          

            return Json(new { ErrorMessage = string.Empty, GetBuyerValid = getBuyerValid }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult CalMoney(ExchangeOfAsset model)
        {
            /*var getNumber = db.BuyAndSellBonds.Where(x => x.Id == model.Asset && x.IsActive != false).Select(x => x.Quantily).FirstOrDefault();
            var getValue = db.BuyAndSellBonds.Where(x => x.Id == model.Asset && x.IsActive != false).Select(x => x.Value).FirstOrDefault();

            var getUnitPrice = getValue / getNumber;
            var Value = model.Number * getUnitPrice;
            string stringValue = String.Format(new CultureInfo("en-US"), "{0:#,##0.##}", Value);*/
            var Value = model.Number * model.UnitPrice;
            string stringValue = String.Format(new CultureInfo("en-US"), "{0:#,##0.##}", Value);
            return Json(new { ErrorMessage = string.Empty, Value = stringValue, JsonRequestBehavior.AllowGet });


           
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