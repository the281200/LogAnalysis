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
using Helpers.Net;
using System.ComponentModel;
using System.Globalization;
using System.Data.Entity.SqlServer;

namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public class IncurredPurchasesController : BaseController
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
        public JsonResult Get_BuyAndSellBonds()
        {
            var assets = from x in db.BuyAndSellBonds.AsNoTracking() where (x.IsActive != false) select x;
            return Json(assets.Select(x => new
            {
                Id = x.Id,
                Name = x.ContractName
            }), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public JsonResult Get_Customer()
        {
            var users = from x in db.UserProfiles.AsNoTracking() where (x.IsActive != false && x.Type != (int)TypeAccount.Admin) select x;
            return Json(users.Select(x => new
            {
                Id = x.UserId,
                Name = x.Mobile + " - " + x.FullName
            }), JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public JsonResult Get_Asset(int? CustomerId, string assetFilter)
        {
            var getSettlementIncurred = db.IncurredPurchases.Where(y => y.IsActive != false && (y.TransactionType == (int)(TypeTransaction.ContractSettlementByTime) || y.TransactionType == (int)(TypeTransaction.ContractSettlementOnTime))).Select(y => y.BuyAndSellBondId).ToList();

            /* var assets = from x in db.BuyAndSellBonds.AsNoTracking() where (x.IsActive != false && x.CustomerId == CustomerId) select x;*/
            var assets = db.BuyAndSellBonds.AsNoTracking().Where(x => (!getSettlementIncurred.Contains(x.Id)) && x.IsActive != false && x.CustomerId == CustomerId).ToList();
            return Json(assets.ToList().Select(x => new
            {
                Id = x.Id,
                Name = x.ContractCode + " - " + x.ContractName
            }), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public JsonResult Get_Asset_Edit(int? CustomerId, int? BuyAndSellBondId, string assetFilter)
        {
            var getSettlementIncurred = db.IncurredPurchases.Where(y => y.IsActive != false && (y.TransactionType == (int)(TypeTransaction.ContractSettlementByTime) || y.TransactionType == (int)(TypeTransaction.ContractSettlementOnTime)) && y.BuyAndSellBondId != BuyAndSellBondId).Select(y => y.BuyAndSellBondId).ToList();

            /* var assets = from x in db.BuyAndSellBonds.AsNoTracking() where (x.IsActive != false && x.CustomerId == CustomerId) select x;*/
            var assets = db.BuyAndSellBonds.AsNoTracking().Where(x => (!getSettlementIncurred.Contains(x.Id)) && x.IsActive != false && x.CustomerId == CustomerId).ToList();
            return Json(assets.ToList().Select(x => new
            {
                Id = x.Id,
                Name = x.ContractCode + " - " + x.ContractName
            }), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public JsonResult Get_InterestPayment(int? BuyAndSellBondId)
        {
            var interestPayment = from x in db.InterestPaymentPeriods.AsNoTracking() where (x.IsActive != false && x.BuyAndSellBondId == BuyAndSellBondId) select x;
            return Json(interestPayment.Select(x => new
            {
                Id = x.Id,
                Name = x.Content
            }), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public JsonResult Get_TransactionType()
        {
            var listTransactionType = new List<ComboBoxViewModel>(){
           /* new ComboBoxViewModel{
                Id = (int)TypeTransaction.GetDisbursed,
                Name = ((TypeTransaction)TypeTransaction.GetDisbursed).GetStringValue()
            },*/
            new ComboBoxViewModel{
                Id = (int)TypeTransaction.GetInterestByTime,
                Name = ((TypeTransaction)TypeTransaction.GetInterestByTime).GetStringValue()
            },
            new ComboBoxViewModel{
                Id = (int)TypeTransaction.GetInterestOntime,
                Name = ((TypeTransaction)TypeTransaction.GetInterestOntime).GetStringValue()
            },
            new ComboBoxViewModel{
                Id = (int)TypeTransaction.ContractSettlementByTime,
                Name = ((TypeTransaction)TypeTransaction.ContractSettlementByTime).GetStringValue()
            },
             new ComboBoxViewModel{
                Id = (int)TypeTransaction.ContractSettlementOnTime,
                Name = ((TypeTransaction)TypeTransaction.ContractSettlementOnTime).GetStringValue()
            }

        };

            return Json(listTransactionType, JsonRequestBehavior.AllowGet);
        }



        [AllowAnonymous]
        public ActionResult IncurredPurchases_Read([DataSourceRequest] DataSourceRequest request)
        {
            var incurred = db.IncurredPurchases.AsNoTracking().Where(x => x.IsActive == true).OrderByDescending(x => x.CreatedAt).ToList().Select(x => new
            {
                x.ID,
                x.IncurredDate,
                x.IncurredName,
                ContractName = x.BuyAndSellBond != null ? x.BuyAndSellBond.ContractName : "",
                CustomerName = x.Customer != null ? x.Customer.FullName : "",
                TypeOfTransaction = x.TransactionType != null ? ((TypeTransaction)x.TransactionType).GetStringValue() : "",
                x.AmountOfMoney,
                x.Note,
                x.IsActive,

            });

            return Json(incurred.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);


        }
        public ActionResult Read_UndoData([DataSourceRequest] DataSourceRequest request)
        {
            var incurred = db.IncurredPurchases.AsNoTracking().Where(x => x.IsActive == false).ToList().Select(x => new
            {
                x.ID,
                x.IncurredDate,
                x.IncurredName,
                ContractName = x.BuyAndSellBond != null ? x.BuyAndSellBond.ContractName : "",
                CustomerName = x.Customer != null ? x.Customer.FullName : "",
                TypeOfTransaction = x.TransactionType != null ? ((TypeTransaction)x.TransactionType).GetStringValue() : "",
                x.AmountOfMoney,
                x.Note,
                x.IsActive
            });
            return Json(incurred.ToDataSourceResult(request));
        }

        public ActionResult Read_Data([DataSourceRequest] DataSourceRequest request)
        {
            var incurred = db.IncurredPurchases.AsNoTracking().Where(x => x.IsActive == true).ToList().Select(x => new
            {
                x.ID,
                x.IncurredDate,
                x.IncurredName,
                ContractName = x.BuyAndSellBond != null ? x.BuyAndSellBond.ContractName : "",
                CustomerName = x.Customer != null ? x.Customer.FullName : "",
                TypeOfTransaction = x.TransactionType != null ? ((TypeTransaction)x.TransactionType).GetStringValue() : "",
                x.AmountOfMoney,
                x.Note,
                x.IsActive
            });
            return Json(incurred.ToDataSourceResult(request));
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult CheckSellerValid(BuyAndSellBond model)
        {
            var getUserValid = db.UserProfiles.Where(x =>x.UserId == model.CustomerId && x.IsActive != false).Select(x => x.UserId).ToList();


            var getAsset = db.BuyAndSellBonds.Where(x => x.CustomerId == model.CustomerId && x.IsActive != false).Select(x => x.Id).ToList();

            return Json(new { ErrorMessage = string.Empty, GetAsset = getAsset, GetUserValid = getUserValid }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(/*[Bind(Exclude = "ID")]*/ IncurredPurchase model)
        {
            if (ModelState.IsValid)
            {

                var checkValidate = true;
               /* var getSumValueDisbursed = db.IncurredPurchases.Where(x => x.BuyAndSellBondId == model.BuyAndSellBondId && x.IsActive == true && x.TransactionType == (int)(TypeTransaction.GetDisbursed)).Select(y => y.AmountOfMoney).Sum();*/
                var getValueOfAsset = db.BuyAndSellBonds.Where(x => x.Id == model.BuyAndSellBondId).Select(y => y.Value).FirstOrDefault();

                var getSumValue = db.IncurredPurchases.Where(x => x.BuyAndSellBondId == model.BuyAndSellBondId && x.IsActive == true).Select(y => y.AmountOfMoney).Sum();
                var getSumValueInterestByTime = db.IncurredPurchases.Where(x => x.BuyAndSellBondId == model.BuyAndSellBondId && x.IsActive == true && x.TransactionType == (int)(TypeTransaction.GetInterestByTime)).Select(y => y.AmountOfMoney).Sum();
                var getSumValueInterestOnTime = db.IncurredPurchases.Where(x => x.BuyAndSellBondId == model.BuyAndSellBondId && x.IsActive == true && x.TransactionType == (int)(TypeTransaction.GetInterestOntime)).Select(y => y.AmountOfMoney).Sum();

                /* if (model.TransactionType == (int)(TypeTransaction.GetDisbursed) && model.AmountOfMoney + getSumValueDisbursed > getValueOfAsset)
                 {
                     ModelState.AddModelError("AmountOfMoney", "Tổng giá trị các lần giải ngân vượt quá giá trị hợp đồng");
                     checkValidate = false;
                 }*/

                if (model.TransactionType == (int)(TypeTransaction.GetInterestByTime) && model.AmountOfMoney + getSumValue > getValueOfAsset + getSumValueInterestByTime)
                {
                    ModelState.AddModelError("AmountOfMoney", "Tổng giá trị của các phát sinh cho 1 hợp đồng vượt quá giá trị của hợp đồng + tổng lãi suất/lợi tức");

                    checkValidate = false;
                }
                if (model.TransactionType == (int)(TypeTransaction.GetInterestOntime) && model.AmountOfMoney + getSumValue > getValueOfAsset + getSumValueInterestOnTime)
                {
                    ModelState.AddModelError("AmountOfMoney", "Tổng giá trị của các phát sinh cho 1 hợp đồng vượt quá giá trị của hợp đồng + tổng lãi suất/lợi tức");

                    checkValidate = false;
                }
                if(model.AmountOfMoney < 0)
                {
                    ModelState.AddModelError("AmountOfMoney", "Số tiền phải lớn hơn 0");

                    checkValidate = false;
                }


                if (!checkValidate) return View(model);


                model.CreatedBy = WebSecurity.CurrentUserId;
                model.CreatedAt = DateTime.Now;
                model.IsActive = true;
                //model.ModifiedBy = WebSecurity.CurrentUserId;
                //model.ModifiedAt = DateTime.Now;
                db.Set<IncurredPurchase>().Add(model);
                db.SaveChanges();

                var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                AuditLog log = new AuditLog
                {
                    ActiveType = LogConstant.AddIncurredPurchases,
                    FunctionName = LogConstant.ManageIncurredPurchases,
                    Information = model.ToJson(),
                    DataTable = LogConstant.IncurredPurchases,
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
            var incurred = db.Set<IncurredPurchase>().Find(id);
            var number = incurred.AmountOfMoney;
            string stringMoney = String.Format(new CultureInfo("en-US"), "{0:#,##0.##}", number);
            var exViewModel = new IncurredPurchaseViewModel()
            {
                ContactDisbursementId = incurred.ContactDisbursementId,
                InterestPaymentPeriodsId = incurred.InterestPaymentPeriodsId,
                CustomerId = incurred.CustomerId,
                TransactionType = incurred.TransactionType,
                StringAmountOfMoney = stringMoney,
                IncurredName = incurred.IncurredName,
                IsActive = incurred.IsActive,
                IncurredDate = incurred.IncurredDate,
                BuyAndSellBondId = incurred.BuyAndSellBondId,
                Note = incurred.Note,
                ID = incurred.ID
            };
            if (incurred == null)
            {
                return HttpNotFound();
            }

            ViewBag.CustomerNameValue = incurred.Customer.FullName;

            //ViewBag.Roles = Roles.GetRolesForUser(assets.UserName);
            return View("Edit", exViewModel);


            /*var incurred = db.Set<IncurredPurchase>().Find(id);
            if (incurred == null)
            {
                return HttpNotFound();
            }
            ViewBag.CustomerNameValue = incurred.Customer.FullName;

            //ViewBag.BondContract = incurred.BondContract;

            //ViewBag.Roles = Roles.GetRolesForUser(assets.UserName);
            return View("Edit", incurred);*/

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IncurredPurchaseViewModel model, string assetId, DateTime? incurred)
        {
            if (ModelState.IsValid)
            {
                var checkValidate = true;

                var getValueOfAsset = db.BuyAndSellBonds.Where(x => x.Id == model.BuyAndSellBondId).Select(y => y.Value).FirstOrDefault();

                var getSumValue = db.IncurredPurchases.Where(x => x.BuyAndSellBondId == model.BuyAndSellBondId && x.IsActive == true).Select(y => y.AmountOfMoney).Sum();
                var getSumValueInterestByTime = db.IncurredPurchases.Where(x => x.BuyAndSellBondId == model.BuyAndSellBondId && x.IsActive == true && x.TransactionType == (int)(TypeTransaction.GetInterestByTime)).Select(y => y.AmountOfMoney).Sum();
                var getSumValueInterestOnTime = db.IncurredPurchases.Where(x => x.BuyAndSellBondId == model.BuyAndSellBondId && x.IsActive == true && x.TransactionType == (int)(TypeTransaction.GetInterestOntime)).Select(y => y.AmountOfMoney).Sum();


                if (model.TransactionType == (int)(TypeTransaction.GetInterestByTime) && model.AmountOfMoney + getSumValue > getValueOfAsset + getSumValueInterestByTime)
                {
                    ModelState.AddModelError("AmountOfMoney", "Tổng giá trị của các phát sinh cho 1 hợp đồng vượt quá giá trị của hợp đồng + tổng lãi suất/lợi tức");

                    checkValidate = false;
                }
                if (model.TransactionType == (int)(TypeTransaction.GetInterestOntime) && model.AmountOfMoney + getSumValue > getValueOfAsset + getSumValueInterestOnTime)
                {
                    ModelState.AddModelError("AmountOfMoney", "Tổng giá trị của các phát sinh cho 1 hợp đồng vượt quá giá trị của hợp đồng + tổng lãi suất/lợi tức");

                    checkValidate = false;
                }

                if (!checkValidate) return View(model);

                model.ModifiedAt = DateTime.Now;
                model.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;

                var modelAdd = new IncurredPurchase()
                {
                    IncurredDate = incurred,
                    IncurredName = model.IncurredName,
                    BuyAndSellBondId = model.BuyAndSellBondId,
                    CustomerId = model.CustomerId,
                    TransactionType = model.TransactionType,
                    AmountOfMoney = model.AmountOfMoney,
                    ModifiedBy = model.ModifiedBy,
                    ModifiedAt = model.ModifiedAt,
                    Note = model.Note,
                    ID = model.ID,
                    InterestPaymentPeriodsId = model.InterestPaymentPeriodsId

                };

                db.IncurredPurchases.Attach(modelAdd);

                db.Entry(modelAdd).Property(a => a.IncurredDate).IsModified = true;
                db.Entry(modelAdd).Property(a => a.IncurredName).IsModified = true;
                db.Entry(modelAdd).Property(a => a.BuyAndSellBondId).IsModified = true;
                db.Entry(modelAdd).Property(a => a.CustomerId).IsModified = true;
                db.Entry(modelAdd).Property(a => a.TransactionType).IsModified = true;
                db.Entry(modelAdd).Property(a => a.AmountOfMoney).IsModified = true;
                db.Entry(modelAdd).Property(a => a.Note).IsModified = true;
                db.Entry(modelAdd).Property(a => a.ModifiedBy).IsModified = true;
                db.Entry(modelAdd).Property(a => a.ModifiedAt).IsModified = true;
                db.Entry(modelAdd).Property(a => a.InterestPaymentPeriodsId).IsModified = true;


                db.SaveChanges();

                var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                AuditLog log = new AuditLog
                {
                    ActiveType = LogConstant.EditIncurredPurchases,
                    FunctionName = LogConstant.ManageIncurredPurchases,
                    Information = model.ToJson(),
                    DataTable = LogConstant.IncurredPurchases,
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
                var incurredDelete = db.IncurredPurchases.Find(id);
                incurredDelete.ModifiedAt = DateTime.Now;
                incurredDelete.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                incurredDelete.IsActive = false;
                db.SaveChanges();

                var userCurent = db.UserProfiles.Where(x => x.UserName == WebSecurity.CurrentUserName).FirstOrDefault();
                AuditLog log = new AuditLog
                {
                    ActiveType = LogConstant.DeleteIncurredPurchases,
                    FunctionName = LogConstant.ManageIncurredPurchases,
                    Information = incurredDelete.ToJson(),
                    DataTable = LogConstant.IncurredPurchases,
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
                var listData = db.IncurredPurchases.Where(x => listDataConvert.Contains(x.ID));

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
                    ActiveType = LogConstant.DeleteIncurredPurchases,
                    FunctionName = LogConstant.ManageIncurredPurchases,
                    Information = listData.ToJson(),
                    DataTable = LogConstant.IncurredPurchases,
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
                var assetDelete = db.IncurredPurchases.Where(x => listDataConvert.Contains(x.ID));
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
                    ActiveType = LogConstant.UndoIncurredPurchases,
                    FunctionName = LogConstant.ManageIncurredPurchases,
                    Information = assetDelete.ToJson(),
                    DataTable = LogConstant.IncurredPurchases,
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

        /*[HttpPost]
        [AllowAnonymous]
        public ActionResult FindCustomer(BuyAndSellBond model)
        {

            var getCustomerID = db.BuyAndSellBonds.Where(x => x.Id == model.BuyAndSellBondId).Select(y => y.CustomerId).FirstOrDefault();
            string customerName = "";
            if (getCustomerID != null)
            {
                customerName = db.UserProfiles.Where(x => x.UserId == getCustomerID).Select(x => x.FullName).FirstOrDefault();
            }
            return Json(new { ErrorMessage = string.Empty, CustomerName = customerName, CustomerId = getCustomerID }, JsonRequestBehavior.AllowGet);
        }*/

        [HttpPost]
        [AllowAnonymous]
        public ActionResult CalMoney(IncurredPurchase model)
        {


            long? amountOfMoney = null;
            int? idDisbursement = null;
            
            string stringAmountOfMoney = "";
            /*if (model.TransactionType == 0)
            {
                var listImplementationDate = db.ContactDisbursementDetails.Where(x => x.IsActive != false && x.BuyAndSellBondId == model.BuyAndSellBondId).Select(y => y.ImplementationDate).ToList();

                var getImplementationDate = listImplementationDate.Where(x => x <= model.IncurredDate).OrderByDescending(x => x.Value.Date).FirstOrDefault();
                amountOfMoney = db.ContactDisbursementDetails.Where(x => x.IsActive != false && x.BuyAndSellBondId == model.BuyAndSellBondId && x.ImplementationDate == getImplementationDate).Select(y => y.Value).FirstOrDefault();
                idDisbursement = db.ContactDisbursementDetails.Where(x => x.Value == amountOfMoney && x.BuyAndSellBondId == model.BuyAndSellBondId && x.ImplementationDate == getImplementationDate).Select(y => y.Id).FirstOrDefault();
                stringAmountOfMoney = String.Format(new CultureInfo("en-US"), "{0:#,##0.##}", amountOfMoney);

            }*/
            if (model.TransactionType == (int)(TypeTransaction.GetInterestByTime) || model.TransactionType == (int)(TypeTransaction.GetInterestOntime))

            {
                
                amountOfMoney = db.InterestPaymentPeriods.Where(x => x.IsActive != false && x.Id == model.InterestPaymentPeriodsId).Select(y => y.AccruedInterest).FirstOrDefault();
                
                stringAmountOfMoney = String.Format(new CultureInfo("en-US"), "{0:#,##0.##}", amountOfMoney);
            }
            else if (model.TransactionType == (int)(TypeTransaction.ContractSettlementByTime) || model.TransactionType == (int)(TypeTransaction.ContractSettlementOnTime))

            {
                /*var listInterestPaymentDate = db.InterestPaymentPeriods.Where(x => x.IsActive != false && x.BuyAndSellBondId == model.BuyAndSellBondId).Select(y => y.InterestPaymentDate).ToList();

                var getInterestPaymentDate = listInterestPaymentDate.Where(x => x <= model.IncurredDate).OrderByDescending(x => x.Value.Date).FirstOrDefault();*/
                var getBuyAndSellBondValue = db.BuyAndSellBonds.Where(x => x.IsActive != false && x.Id == model.BuyAndSellBondId).Select(y => y.Value).FirstOrDefault();
                /*var getAccruedInterest = db.InterestPaymentPeriods.Where(x => x.IsActive != false && x.BuyAndSellBondId == model.BuyAndSellBondId && x.InterestPaymentDate == getInterestPaymentDate).Select(y => y.AccruedInterest).FirstOrDefault();*/
                /*if (getAccruedInterest == null)
                {*/
                    amountOfMoney = getBuyAndSellBondValue;
                /*}
                else
                {
                    amountOfMoney = getAccruedInterest + getBuyAndSellBondValue;
                }*/

                stringAmountOfMoney = String.Format(new CultureInfo("en-US"), "{0:#,##0.##}", amountOfMoney);
            }
            return Json(new { ErrorMessage = string.Empty, AmountOfMoney = stringAmountOfMoney, ContactDisbursementId = idDisbursement }, JsonRequestBehavior.AllowGet);
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