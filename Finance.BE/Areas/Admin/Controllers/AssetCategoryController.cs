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
using System.Text.RegularExpressions;
using System.Globalization;

namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public class AssetCategoryController : BaseController
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
        public ActionResult AssetCategorys_Read([DataSourceRequest] DataSourceRequest request)
        {
            var assets = db.AssetCategorys.AsNoTracking().OrderByDescending(x => x.CreatedAt).Where(x => x.IsActive != false).ToList();
            return Json(assets.ToDataSourceResult(request));
        }
        public ActionResult Read_UndoData([DataSourceRequest] DataSourceRequest request)
        {
            var assets = db.AssetCategorys.Where(x => x.IsActive == false).ToList();
            return Json(assets.ToDataSourceResult(request));
        }

        public ActionResult Read_Data([DataSourceRequest] DataSourceRequest request)
        {
            var assets = db.AssetCategorys.Where(x => x.IsActive != false).ToList();
            return Json(assets.ToDataSourceResult(request));
        }


        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(/*[Bind(Exclude = "ID")]*/ AssetCategoryViewModel model, DateTime? release)
        {
            if (ModelState.IsValid)
            {

                var checkValidate = true;
                var checkAssetCodeExist = db.AssetCategorys.Where(p => p.AssetCode != null && p.AssetCode.Equals(model.AssetCode) && p.IsActive != false).Any();
                if (checkAssetCodeExist)
                {
                    ModelState.AddModelError("AssetCode", "Mã tài sản đã tồn tại trên hệ thống!");
                    checkValidate = false;
                }
                if (model.InterestFloat == null)
                {
                    model.InterestFloat = "0";
                }
                Match match = Regex.Match(model.InterestFloat, @"(\d+)");
                if (!match.Success)
                {
                    ModelState.AddModelError("InterestFloat", "Lãi suất phải là số!");
                    checkValidate = false;
                }
                Match comma = Regex.Match(model.InterestFloat, @"(\,+)");
                if (comma.Success)
                {
                    ModelState.AddModelError("InterestFloat", "Lãi suất phải là số thập phân có dạng x.yz");
                    checkValidate = false;
                }

                if (model.PeriodFloat == null)
                {
                    model.PeriodFloat = "0";
                }
                Match matchPeriod = Regex.Match(model.PeriodFloat, @"(\d+)");
                if (!matchPeriod.Success)
                {
                    ModelState.AddModelError("PeriodFloat", "Kỳ hạn phải là số!");
                    checkValidate = false;
                }
                Match commaPeriod = Regex.Match(model.PeriodFloat, @"(\,+)");
                if (commaPeriod.Success)
                {
                    ModelState.AddModelError("PeriodFloat", "Kỳ hạn phải là số thập phân có dạng x.y");
                    checkValidate = false;
                }

                if (!checkValidate) return View(model);
                string value = model.InterestFloat;
                string valuePeriod = model.PeriodFloat;
                NumberStyles style;
                CultureInfo provider;

                /*style = NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands;*/
                style = NumberStyles.AllowDecimalPoint;
                provider = new CultureInfo("en-US");

                model.InterestRate = Decimal.Parse(value, style, provider);
                model.Period = Decimal.Parse(valuePeriod, style, provider);

                model.CreatedBy = WebSecurity.CurrentUserId;
                model.CreatedAt = DateTime.Now;
                model.IsActive = true;
                model.ReleaseDate = release;

                var modelAdd = new AssetCategory()
                {
                    AssetCode = model.AssetCode,
                    CreatedBy = model.CreatedBy,
                    CreatedAt = model.CreatedAt,
                    IsActive = model.IsActive,
                    ReleaseDate = model.ReleaseDate,
                    Name = model.Name,
                    Price = model.Price,
                    TradeOrganization = model.TradeOrganization,
                    PropertySecurity = model.PropertySecurity,
                    ConsulAndPublishOrg = model.ConsulAndPublishOrg,
                    PaymentAgent = model.PaymentAgent,
                    DepositoryAgent = model.DepositoryAgent,
                    InterestPayment = model.InterestPayment,
                    Period = model.Period,
                    InterestRate = model.InterestRate,
                    Note = model.Note

                };
                db.Set<AssetCategory>().Add(modelAdd);
                db.SaveChanges();

                var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                AuditLog log = new AuditLog
                {
                    ActiveType = LogConstant.AddAssetCategory,
                    FunctionName = LogConstant.ManageAssetCategory,
                    Information = modelAdd.ToJson(),
                    DataTable = LogConstant.AssetCategorys,
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

            var assets = db.Set<AssetCategory>().Find(id);
            var number = assets.Price;
            string stringPrice = String.Format(new CultureInfo("en-US"), "{0:#,##0.##}", number);
            var exViewModel = new AssetCategoryViewModel()
            {
                InterestFloat = assets.InterestRate.Value.ToString(CultureInfo.CreateSpecificCulture("en-US")),
                AssetCode = assets.AssetCode,
                IsActive = assets.IsActive,
                ReleaseDate = assets.ReleaseDate,
                Name = assets.Name,
                Price = assets.Price,
                StringPrice = stringPrice,
                TradeOrganization = assets.TradeOrganization,
                PropertySecurity = assets.PropertySecurity,
                ConsulAndPublishOrg = assets.ConsulAndPublishOrg,
                PaymentAgent = assets.PaymentAgent,
                DepositoryAgent = assets.DepositoryAgent,
                InterestPayment = assets.InterestPayment,
                Period = assets.Period,             
                Note = assets.Note,
                PeriodFloat = assets.Period.Value.ToString(CultureInfo.CreateSpecificCulture("en-US")),


            };

            if (assets == null)
            {
                return HttpNotFound();
            }
            //ViewBag.Roles = Roles.GetRolesForUser(assets.UserName);
            return View("Edit", exViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AssetCategoryViewModel model, DateTime? release)
        {
            if (ModelState.IsValid)
            {
                var checkValidate = true;
                var checkAssetCodeExist = db.AssetCategorys.Where(p => p.AssetCode != null && p.AssetCode.Equals(model.AssetCode) && p.IsActive != false && p.Id != model.Id ).Any();
                if (checkAssetCodeExist)
                {
                    ModelState.AddModelError("AssetCode", "Mã tài sản đã tồn tại trên hệ thống!");
                    checkValidate = false;
                }
                if (model.InterestFloat == null)
                {
                    model.InterestFloat = "0";
                }
                Match match = Regex.Match(model.InterestFloat, @"(\d+)");
                if (!match.Success)
                {
                    ModelState.AddModelError("InterestFloat", "Lãi suất phải là số!");
                    checkValidate = false;
                }

                Match comma = Regex.Match(model.InterestFloat, @"(\,+)");
                if (comma.Success)
                {
                    ModelState.AddModelError("InterestFloat", "Lãi suất phải là số thập phân có dạng x.yz");
                    checkValidate = false;
                }

                if (model.PeriodFloat == null)
                {
                    model.PeriodFloat = "0";
                }
                Match matchPeriod = Regex.Match(model.PeriodFloat, @"(\d+)");
                if (!matchPeriod.Success)
                {
                    ModelState.AddModelError("PeriodFloat", "Kỳ hạn phải là số!");
                    checkValidate = false;
                }
                Match commaPeriod = Regex.Match(model.PeriodFloat, @"(\,+)");
                if (commaPeriod.Success)
                {
                    ModelState.AddModelError("PeriodFloat", "Kỳ hạn phải là số thập phân có dạng x.y");
                    checkValidate = false;
                }

                var asset = db.AssetCategorys.AsNoTracking().ToList();
                var checkAssetIdExist = asset.Where(p => p.Id != model.Id && p.AssetCode.Trim().ToLower() == model.AssetCode.Trim().ToLower()).Any();
                if (checkAssetIdExist)
                {
                    ModelState.AddModelError("AssetCode", "Mã trái phiếu/tài sản đã tồn tại trên hệ thống");
                    checkValidate = false;
                }

                if (!checkValidate) return View(model);
                string value = model.InterestFloat;
                string valuePeriod = model.PeriodFloat;
                NumberStyles style;
                CultureInfo provider;

                /*style = NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands;*/
                style = NumberStyles.AllowDecimalPoint;
                provider = new CultureInfo("en-US");

                model.InterestRate = Decimal.Parse(value, style, provider);
                model.Period = Decimal.Parse(valuePeriod, style, provider);
                model.ModifiedAt = DateTime.Now;
                model.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                model.ReleaseDate = release;

                var modelAdd = new AssetCategory()
                {
                    AssetCode = model.AssetCode,
                    ModifiedBy = model.ModifiedBy,
                    ModifiedAt = model.ModifiedAt,
                    IsActive = model.IsActive,
                    ReleaseDate = model.ReleaseDate,
                    Name = model.Name,
                    Price = model.Price,
                    TradeOrganization = model.TradeOrganization,
                    PropertySecurity = model.PropertySecurity,
                    ConsulAndPublishOrg = model.ConsulAndPublishOrg,
                    PaymentAgent = model.PaymentAgent,
                    DepositoryAgent = model.DepositoryAgent,
                    InterestPayment = model.InterestPayment,
                    Period = model.Period,
                    InterestRate = model.InterestRate,
                    Note = model.Note,
                    Id = model.Id

                };

                db.AssetCategorys.Attach(modelAdd);
                db.Entry(modelAdd).Property(a => a.AssetCode).IsModified = true;
                db.Entry(modelAdd).Property(a => a.Name).IsModified = true;
                db.Entry(modelAdd).Property(a => a.PaymentAgent).IsModified = true;
                db.Entry(modelAdd).Property(a => a.DepositoryAgent).IsModified = true;
                db.Entry(modelAdd).Property(a => a.ConsulAndPublishOrg).IsModified = true;
                db.Entry(modelAdd).Property(a => a.InterestPayment).IsModified = true;
                db.Entry(modelAdd).Property(a => a.InterestRate).IsModified = true;
                db.Entry(modelAdd).Property(a => a.ModifiedAt).IsModified = true;
                db.Entry(modelAdd).Property(a => a.ModifiedBy).IsModified = true;
                db.Entry(modelAdd).Property(a => a.Note).IsModified = true;
                db.Entry(modelAdd).Property(a => a.Period).IsModified = true;
                db.Entry(modelAdd).Property(a => a.Price).IsModified = true;
                db.Entry(modelAdd).Property(a => a.PropertySecurity).IsModified = true;
                db.Entry(modelAdd).Property(a => a.ReleaseDate).IsModified = true;
                db.Entry(modelAdd).Property(a => a.TradeOrganization).IsModified = true;
                db.Entry(modelAdd).Property(a => a.IsActive).IsModified = true;
                db.Entry(modelAdd).Property(a => a.Id).IsModified = true;


                db.SaveChanges();

                var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                AuditLog log = new AuditLog
                {
                    ActiveType = LogConstant.EditAssetCategory,
                    FunctionName = LogConstant.ManageAssetCategory,
                    Information = modelAdd.ToJson(),
                    DataTable = LogConstant.AssetCategorys,
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
                var assetsDelete = db.AssetCategorys.Find(id);
                assetsDelete.ModifiedAt = DateTime.Now;
                assetsDelete.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                assetsDelete.IsActive = false;
                db.SaveChanges();

                var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                AuditLog log = new AuditLog
                {
                    ActiveType = LogConstant.DeleteAssetCategory,
                    FunctionName = LogConstant.ManageAssetCategory,
                    Information = assetsDelete.ToJson(),
                    DataTable = LogConstant.AssetCategorys,
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
                var listData = db.AssetCategorys.Where(x => listDataConvert.Contains(x.Id));

                foreach (var item in listData)
                {
                    item.IsActive = false;
                    item.ModifiedAt = DateTime.Now;
                    item.ModifiedBy = WebSecurity.CurrentUserId;
                }

                var listId = listData.Select(y => y.Id).ToList();
                var idBondRelated = db.BuyAndSellBonds.Where(x => x.IsActive != false && listId.Contains(x.AssetCategorysId ?? default)).Select(x=>x.Id).ToList();

                    //(from im in listData.Where(x => x.IsActive != false)

                    //               join pr in db.BuyAndSellBonds.Where(x => x.IsActive != false) on im.Id equals pr.AssetCategorysId into group1
                    //               from item1 in group1
                    //               select new { item1.Id }).ToList().Select(x=>x.Id);



                var listBuyAndSellBond = db.BuyAndSellBonds.Where(x => idBondRelated.Contains(x.Id) && x.IsActive != false);
                foreach (var item in listBuyAndSellBond)
                {
                    item.IsActive = false;
                    item.ModifiedAt = DateTime.Now;
                    item.ModifiedBy = WebSecurity.CurrentUserId;
                }

                db.SaveChanges();


                
                /*var listBuyAndSellBond = db.BuyAndSellBonds.Where(x => bondRelated.Contains(x.Id));
                foreach (var item in listBuyAndSellBond)
                {
                    item.IsActive = false;
                    item.ModifiedAt = DateTime.Now;
                    item.ModifiedBy = WebSecurity.CurrentUserId;
                }*/
                db.SaveChanges();

                var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                AuditLog log = new AuditLog
                {
                    ActiveType = LogConstant.DeleteAssetCategory,
                    FunctionName = LogConstant.ManageAssetCategory,
                    Information = listData.ToJson(),
                    DataTable = LogConstant.AssetCategorys,
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
                var assetDelete = db.AssetCategorys.Where(x => listDataConvert.Contains(x.Id));
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
                    ActiveType = LogConstant.UndoAssetCategory,
                    FunctionName = LogConstant.ManageAssetCategory,
                    Information = assetDelete.ToJson(),
                    DataTable = LogConstant.AssetCategorys,
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

    }
}
