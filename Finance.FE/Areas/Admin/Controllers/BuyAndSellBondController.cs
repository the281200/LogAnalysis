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
using System.Globalization;

namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public class BuyAndSellBondController : BaseController
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
        public ActionResult ContactDisbursementDetails_Read([DataSourceRequest] DataSourceRequest request, int? tableParentId)
        {
            var data = from x in db.ContactDisbursementDetails.Where(x => x.IsActive != false && x.BuyAndSellBondId == tableParentId)
                       select new
                       {
                           x.Id,
                           x.BuyAndSellBondId,
                           x.Content,
                           x.ImplementationDate,
                           x.Value
                       };
            var test = data.ToList();
            return Json(data.OrderByDescending(x => x.Id).ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Periods_Read([DataSourceRequest] DataSourceRequest request, int? tableParentId, string buyAndSellBondId, int? change)
        {
            var data = new List<PeriodViewModel>();

            if (change != null && change == 1)
            {
                var listId = ApplicationService.StringToListInt(buyAndSellBondId);

                var incurreds = db.IncurredPurchases.Where(x => x.IsActive != false && listId.Contains(x.BuyAndSellBondId ?? default)
                && (x.TransactionType == (int)TypeTransaction.ContractSettlement || x.TransactionType == (int)TypeTransaction.GetInterest)).ToList();

                var incurredIds = incurreds.Select(x => x.ID);
                var periods = db.Periods.Where(x => incurredIds.Contains(x.IncurredId ?? default) && x.IsActive != false);
                var periodIds = periods.Select(x => x.IncurredId);

                var listNotUsed = incurreds.Where(x => !periodIds.Contains(x.ID));
                var listUsed = incurreds.Where(x => periodIds.Contains(x.ID));

                foreach (var item in listNotUsed)
                {
                    var period = new PeriodViewModel()
                    {
                        Id = 0,
                        Value = item.AmountOfMoney,
                        IncurredId = item.ID,
                        IncurredName = item.IncurredName
                    };

                    data.Add(period);
                }

                foreach (var item in listUsed)
                {
                    var total = periods.Where(x => x.IncurredId == item.ID).Sum(x => x.Value);

                    if (total < item.AmountOfMoney)
                    {
                        var period = new PeriodViewModel()
                        {
                            Id = 0,
                            Value = item.AmountOfMoney - total,
                            IncurredId = item.ID,
                            IncurredName = item.IncurredName
                        };

                        data.Add(period);
                    }
                }
            }
            else
            {
                data = (from x in db.Periods.Where(x => x.BuyAndSellBondId == tableParentId && x.IsActive != false)
                        select new
                        {
                            x.Id,
                            x.BuyAndSellBondId,
                            x.IncurredId,
                            IncurredName = x.Incurred.IncurredName,
                            x.Value
                        }).AsEnumerable().Select(B => new PeriodViewModel
                        {
                            Id = B.Id,
                            BuyAndSellBondId = B.BuyAndSellBondId,
                            IncurredId = B.IncurredId,
                            IncurredName = B.IncurredName,
                            Value = B.Value
                        }).ToList();
            }

            return Json(data.Count() > 0 ? data.ToList() : null, JsonRequestBehavior.AllowGet);
        }

        public ActionResult InterestPaymentPeriods_Read([DataSourceRequest] DataSourceRequest request, int? period, int? tableParentId, int? change, long? value, string purchaseDate, Double? interestPayPeriod, Double? interestRate)
        {
            var data = new List<InterestPaymentPeriod>();
            if (change != null && change == 1)
            {
                if (purchaseDate != null && purchaseDate != "" && value != null)
                {
                    var count = (period ?? default) / (interestPayPeriod ?? default);

                    var mostRecentInterestDate = purchaseDate != null ? DateTime.Parse(purchaseDate) : DateTime.Now;

                    for (int i = 1; i <= count; i++)
                    {
                        var interestPaymentDate = mostRecentInterestDate.AddMonths((int)(interestPayPeriod ?? default));
                        var calculateInterestNumber = mostRecentInterestDate != null ? (interestPaymentDate - mostRecentInterestDate).TotalDays : 0;
                        var accruedInterest = calculateInterestNumber * (value ?? default) * (((interestRate ?? default) * 0.01) / 365) * 0.95;

                        var interestPaymentPeriod = new InterestPaymentPeriod()
                        {
                            BuyAndSellBondId = tableParentId,
                            Content = "Kỳ trả lãi " + i,
                            InterestPaymentDate = interestPaymentDate,
                            CalculateInterestNumber = (int)calculateInterestNumber,
                            AccruedInterest = (long)accruedInterest
                        };
                        data.Add(interestPaymentPeriod);

                        mostRecentInterestDate = interestPaymentDate;
                    }
                }
            }
            else
            {
                data = db.InterestPaymentPeriods.Where(x => x.BuyAndSellBondId == tableParentId && x.IsActive != false).ToList();
            }


            return Json(data.ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Datas_Read([DataSourceRequest] DataSourceRequest request)
        {
            var sourceList = db.SourceInBuyAndSellBonds.Where(x => x.IsActive != false).Include(x => x.Source).Include(x => x.BuyAndSellBond).ToList();

            var data = db.BuyAndSellBonds.Where(x => x.IsActive != false).ToList()
                .Select(x => new
                {
                    x.Id,
                    x.AssetTypeId,
                    x.Content,
                    x.ContractName,
                    CustomerName = x.Customer != null ? x.Customer.FullName : "",
                    x.CustomerId,
                    x.FixedInterestRate,
                    x.FundId,
                    x.InterestRate,
                    x.Period,
                    x.PeriodDate,
                    x.PurchaseDate,
                    x.Quantily,
                    x.TransferredTime,
                    AssetName = x.TypeOfAsset != null ? x.TypeOfAsset.AssetName : "",
                    x.Value,
                    x.InterestPayPeriod,
                    SourceString = sourceList.Where(y => y.BuyAndSellBondId == x.Id).ToList() != null ?
                           String.Join(", ", sourceList.Where(y => y.BuyAndSellBondId == x.Id).Select(y => y.Source.ContractName).ToArray()) :
                           ""
                });

            return Json(data.OrderByDescending(x => x.Id).ToList().ToDataSourceResult(request));
        }

        public ActionResult Read_UndoData([DataSourceRequest] DataSourceRequest request)
        {
            var sourceList = db.SourceInBuyAndSellBonds.Where(x => x.IsActive != false).Include(x => x.Source).ToList();

            var data = db.BuyAndSellBonds.Where(x => x.IsActive == false).ToList()
               .Select(x => new
               {
                   x.Id,
                   x.AssetTypeId,
                   x.Content,
                   x.ContractName,
                   CustomerName = x.Customer != null ? x.Customer.FullName : "",
                   x.CustomerId,
                   x.FixedInterestRate,
                   x.FundId,
                   x.InterestRate,
                   x.Period,
                   x.PeriodDate,
                   x.PurchaseDate,
                   x.Quantily,
                   x.TransferredTime,
                   AssetName = x.TypeOfAsset != null ? x.TypeOfAsset.AssetName : "",
                   x.Value,
                   x.InterestPayPeriod,
                   SourceString = sourceList.Where(y => y.BuyAndSellBondId == x.Id).ToList() != null ?
                          String.Join(", ", sourceList.Where(y => y.BuyAndSellBondId == x.Id).Select(y => y.Source.ContractName).ToArray()) :
                          ""
               });

            return Json(data.OrderByDescending(x => x.Id).ToDataSourceResult(request));
        }

        public ActionResult Read_Data([DataSourceRequest] DataSourceRequest request)
        {
            var sourceList = db.SourceInBuyAndSellBonds.Where(x => x.IsActive != false).Include(x => x.Source).ToList();

            var data = db.BuyAndSellBonds.Where(x => x.IsActive != false).ToList()
               .Select(x => new
               {
                   x.Id,
                   x.AssetTypeId,
                   x.Content,
                   x.ContractName,
                   CustomerName = x.Customer != null ? x.Customer.FullName : "",
                   x.CustomerId,
                   x.FixedInterestRate,
                   x.FundId,
                   x.InterestRate,
                   x.Period,
                   x.PeriodDate,
                   x.PurchaseDate,
                   x.Quantily,
                   x.TransferredTime,
                   AssetName = x.TypeOfAsset != null ? x.TypeOfAsset.AssetName : "",
                   x.Value,
                   x.InterestPayPeriod,
                   SourceString = sourceList.Where(y => y.BuyAndSellBondId == x.Id).ToList() != null ?
                          String.Join(", ", sourceList.Where(y => y.BuyAndSellBondId == x.Id).Select(y => y.Source.ContractName).ToArray()) :
                          ""
               });

            return Json(data.OrderByDescending(x => x.Id).ToDataSourceResult(request));
        }

        [AllowAnonymous]
        public JsonResult Get_Customers()
        {
            var users = from x in db.UserProfiles.AsNoTracking() where (x.IsActive != false && x.Type == (int)TypeAccount.Customer) select x;
            return Json(users.Select(x => new
            {
                Id = x.UserId,
                Name = x.FullName
            }), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public JsonResult Get_TypeOfAssets()
        {
            var assets = from x in db.TypeOfAssets.AsNoTracking() where (x.IsActive != false) select x;
            return Json(assets.Select(x => new
            {
                Id = x.ID,
                Name = x.AssetName
            }), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public JsonResult Get_BuyAndSellBonds(int? customerId)
        {
            var listIncurredPurchases = db.IncurredPurchases.AsNoTracking().Where(x => x.IsActive != false && x.BuyAndSellBond.CustomerId == customerId
            && (x.TransactionType == (int)TypeTransaction.GetInterest || x.TransactionType == (int)TypeTransaction.ContractSettlement)).ToList();

            var periodList = db.Periods.Where(x=>x.IsActive != false).Include(x=>x.Incurred).ToList();
            var incurredPurchases = db.IncurredPurchases.AsNoTracking().Where(x => x.IsActive != false
               && (x.TransactionType == (int)TypeTransaction.ContractSettlement || x.TransactionType == (int)TypeTransaction.GetInterest)).ToList();
            var buyAndSellBonds = db.BuyAndSellBonds.ToList();
            var data = new  List<BuyAndSellBond>();
            //var data = db.BuyAndSellBonds.Where(x => listBuyAndSellBonds.Contains(x.Id)).ToList();

            foreach(var item in listIncurredPurchases)
            {
                var periods = periodList.Where(x => x.IncurredId == item.ID && x.IsActive != false);

                if(periods != null)
                {
                    var sum = periods.Sum(x=>x.Value);
                    if (sum < item.AmountOfMoney)
                    {
                        var dataAdd = buyAndSellBonds.Where(x => x.Id == item.BuyAndSellBondId).FirstOrDefault();
                        data.Add(dataAdd);
                    }
                }    
            }

            return Json(data.Select(x => new
            {
                Id = x.Id,
                Name = x.ContractName
            }), JsonRequestBehavior.AllowGet);
        }


        [AllowAnonymous]
        public JsonResult IncurredList(string dataTable)
        {
            var listReturnData = new List<ComboboxIncurred>();
            var listIntTable3 = (ConvertJsonDataPeriod(dataTable)).Where(x => x.Value != null);
            IEnumerable<int> listIdViewData = (listIntTable3 != null) ? listIntTable3.Select(x => x.Id) : null;

            var incurredPurchases = db.IncurredPurchases.AsNoTracking().Where(x => x.IsActive != false
                && (x.TransactionType == (int)TypeTransaction.ContractSettlement || x.TransactionType == (int)TypeTransaction.GetInterest)).ToList();

            var periodList = db.Periods.ToList();
            foreach (var item in incurredPurchases)
            {
                var periods = periodList.Where(x => !listIdViewData.Contains(x.Id) && x.IncurredId == item.ID && x.IsActive != false);

                if (periods != null)
                {
                    var listPeriod = new List<Period>();
                    listPeriod.AddRange(listIntTable3.Where(x => x.IncurredId == item.ID && x.IsActive != false));
                    listPeriod.AddRange(periods.ToList());

                    var sum = listPeriod.Sum(x => x.Value);
                    if (sum < item.AmountOfMoney)
                    {
                        var dataItem = new ComboboxIncurred
                        {
                            Id = item.ID,
                            IncurredName = item.IncurredName
                        };

                        listReturnData.Add(dataItem);
                    }
                }
            }

            return Json(listReturnData.ToList(), JsonRequestBehavior.AllowGet);

        }

        public ActionResult Add()
        {
            var model = new BuyAndSellBondViewModel();
            model.FixedInterestRate = true;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add([Bind(Exclude = "")] BuyAndSellBondViewModel model, DateTime? purchase, DateTime? transfer, DateTime? periodDate, string dataTable1, string dataTable2, string dataTable3, int?[] sourceInBuyAndSellBond)
        {
            ModelState.Remove("InterestRate");
            model.InterestRate = model.InterestRateString != null ? Double.Parse(model.InterestRateString.Trim().Replace(".", ",")) : 0;
            if (ModelState.IsValid)
            {
                var checkValidate = true;

                var buyAndSellBonds = db.BuyAndSellBonds.AsNoTracking().ToList();
                var checkNameExist = buyAndSellBonds.Where(p => p.ContractName.ToLower().Equals(model.ContractName.Trim().ToLower())).Any();
                if (checkNameExist)
                {
                    ModelState.AddModelError("ContractName", "Tên hợp đồng đã tồn tại!");
                    checkValidate = false;
                }

                if (purchase != null || transfer != null)
                {
                    if (purchase == null && transfer != null)
                    {
                        ModelState.AddModelError("PurchaseDate", "Vui lòng chọn ngày mua!");
                        checkValidate = false;
                    }

                    if (purchase != null && transfer != null && purchase.Value.Date > transfer.Value.Date)
                    {
                        ModelState.AddModelError("PurchaseDate", "Vui lòng chọn thời gian chuyển nhượng không trước ngày mua!");
                        checkValidate = false;
                    }
                }

                model.PurchaseDate = purchase;
                model.PeriodDate = purchase.Value.AddMonths(model.Period ?? default);
                model.TransferredTime = transfer;
                model.IsActive = true;
                model.CreatedAt = DateTime.Now;
                model.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;

                if (!checkValidate) return View(model);

                using (DbContextTransaction transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var dataAdd = new BuyAndSellBond()
                        {
                            PurchaseDate = model.PurchaseDate,
                            PeriodDate = model.PeriodDate,
                            AssetTypeId = model.AssetTypeId,
                            BuyAndSellBondId = model.BuyAndSellBondId,
                            Content = model.Content,
                            ContractName = model.ContractName,
                            CreatedAt = model.CreatedAt,
                            CreatedBy = model.CreatedBy,
                            CustomerId = model.CustomerId,
                            FixedInterestRate = model.FixedInterestRate,
                            InterestPayPeriod = model.InterestPayPeriod,
                            InterestRate = model.InterestRate,
                            FundId = model.FundId,
                            IsActive = model.IsActive,
                            Period = model.Period,
                            Quantily = model.Quantily,
                            TransferredTime = model.TransferredTime,
                            Value = model.Value,
                            ContractCode = model.ContractCode
                        };
                        db.BuyAndSellBonds.Add(dataAdd);
                        db.SaveChanges();

                        //add source multiple 
                        var checkAdd = AddSourceMultiple(dataAdd.Id, sourceInBuyAndSellBond);
                        if (!checkAdd)
                        {
                            transaction.Rollback();
                            return View(model);
                        }

                        var listIntTable1 = ConvertJsonDataContactDetail(dataTable1);
                        var listIntTable2 = ConvertJsonDataInterestPaymentPeriod(dataTable2);
                        var listIntTable3 = ConvertJsonDataPeriod(dataTable3);

                        //add list data contact detail (table child 1)
                        AddContactDetail(dataAdd.Id, listIntTable1, false);

                        //add list data contact detail (table child 2)
                        AddInterestPaymentPeriod(dataAdd.Id, listIntTable2, false);

                        //add list data contact detail (table child 2)
                        AddPeriod(dataAdd.Id, listIntTable3, false);

                        transaction.Commit();
                        ViewBag.StartupScript = "create_success();";
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", "Đã có lỗi xảy ra trong quá trình lưu! Vui lòng thử lại.");
                        transaction.Rollback();
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
            var model = db.Set<BuyAndSellBond>().Find(id);
            var quantily = model.Quantily;
            var value = model.Value;

            string stringQuantily = String.Format(new CultureInfo("en-US"), "{0:#,##0.##}", quantily);
            string stringValue = String.Format(new CultureInfo("en-US"), "{0:#,##0.##}", value);
            if (model == null)
            {
                return HttpNotFound();
            }

            ViewData["Incurred"] = db.IncurredPurchases.AsNoTracking().Where(x => x.IncurredDate != null && x.IsActive != false && x.BuyAndSellBondId == 4
            && (x.TransactionType == (int)TypeTransaction.ContractSettlement || x.TransactionType == (int)TypeTransaction.GetInterest)).ToList().Select(x => new
            {
                Id = x.ID,
                Name = x.IncurredName
            });

            ViewBag.SourceInBuyAndSellBond = (db.SourceInBuyAndSellBonds.Where(x => x.BuyAndSellBondId == id && x.IsActive != false).Select(x => x.SourceId).ToList()).ConvertAll<string>(delegate (int i) { return i.ToString(); }).ToArray();

            var dataEdit = new BuyAndSellBondViewModel()
            {
                PurchaseDate = model.PurchaseDate,
                PeriodDate = model.PeriodDate,
                AssetTypeId = model.AssetTypeId,
                BuyAndSellBondId = model.BuyAndSellBondId,
                Content = model.Content,
                ContractName = model.ContractName,
                CustomerId = model.CustomerId,
                FixedInterestRate = model.FixedInterestRate,
                InterestPayPeriod = model.InterestPayPeriod,
                InterestRate = model.InterestRate,
                FundId = model.FundId,
                IsActive = model.IsActive,
                Period = model.Period,
                Quantily = model.Quantily,
                TransferredTime = model.TransferredTime,
                Value = model.Value,
                InterestRateString = model.InterestRate.ToString().Replace(",", "."),
                Id = model.Id,
                ContractCode = model.ContractCode,
                StringQuantily = stringQuantily,
                StringValue = stringValue,

            };
            return View("Edit", dataEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "")] BuyAndSellBondViewModel model, DateTime? purchase, DateTime? transfer, DateTime? periodDate, string dataTable1, string dataTable2, string dataTable3, int?[] sourceInBuyAndSellBond)
        {
            ModelState.Remove("InterestRate");
            model.InterestRate = model.InterestRateString != null ? Double.Parse(model.InterestRateString.Trim().Replace(".", ",")) : 0;
            if (ModelState.IsValid)
            {
                var checkValidate = true;

                var buyAndSellBonds = db.BuyAndSellBonds.AsNoTracking().ToList();
                var checkNameExist = buyAndSellBonds.Where(p => p.ContractName.ToLower().Equals(model.ContractName.Trim().ToLower()) && p.Id != model.Id).Any();
                if (checkNameExist)
                {
                    ModelState.AddModelError("ContractName", "Tên hợp đồng đã tồn tại!");
                    checkValidate = false;
                }

                if (purchase != null || transfer != null)
                {
                    if (purchase == null && transfer != null)
                    {
                        ModelState.AddModelError("PurchaseDate", "Vui lòng chọn ngày mua!");
                        checkValidate = false;
                    }

                    if (purchase != null && transfer != null && purchase.Value.Date > transfer.Value.Date)
                    {
                        ModelState.AddModelError("PurchaseDate", "Vui lòng chọn thời gian chuyển nhượng không trước ngày mua!");
                        checkValidate = false;
                    }
                }


                model.PurchaseDate = purchase;
                model.TransferredTime = transfer;
                model.PeriodDate = purchase.Value.AddMonths(model.Period ?? default);
                model.IsActive = true;
                model.ModifiedAt = DateTime.Now;
                model.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                //model.InterestRate = model.InterestRateString != null ? Double.Parse(model.InterestRateString.Replace(".", ",")) : 0;
                if (!checkValidate) return View(model);

                using (DbContextTransaction transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var dataEdit = new BuyAndSellBond()
                        {
                            PurchaseDate = model.PurchaseDate,
                            PeriodDate = model.PeriodDate,
                            AssetTypeId = model.AssetTypeId,
                            BuyAndSellBondId = model.BuyAndSellBondId,
                            Content = model.Content,
                            ContractName = model.ContractName,
                            ModifiedAt = model.ModifiedAt,
                            ModifiedBy = model.ModifiedBy,
                            CustomerId = model.CustomerId,
                            FixedInterestRate = model.FixedInterestRate,
                            InterestPayPeriod = model.InterestPayPeriod,
                            InterestRate = model.InterestRate,
                            FundId = model.FundId,
                            IsActive = model.IsActive,
                            Period = model.Period,
                            Quantily = model.Quantily,
                            TransferredTime = model.TransferredTime,
                            Value = model.Value,
                            Id = model.Id,
                            ContractCode = model.ContractCode
                        };

                        db.BuyAndSellBonds.Attach(dataEdit);
                        db.Entry(dataEdit).Property(a => a.AssetTypeId).IsModified = true;
                        db.Entry(dataEdit).Property(a => a.BuyAndSellBondId).IsModified = true;
                        db.Entry(dataEdit).Property(a => a.Content).IsModified = true;
                        db.Entry(dataEdit).Property(a => a.ContractName).IsModified = true;
                        db.Entry(dataEdit).Property(a => a.CustomerId).IsModified = true;
                        db.Entry(dataEdit).Property(a => a.FixedInterestRate).IsModified = true;
                        db.Entry(dataEdit).Property(a => a.FundId).IsModified = true;
                        db.Entry(dataEdit).Property(a => a.InterestRate).IsModified = true;
                        db.Entry(dataEdit).Property(a => a.ModifiedAt).IsModified = true;
                        db.Entry(dataEdit).Property(a => a.ModifiedBy).IsModified = true;
                        db.Entry(dataEdit).Property(a => a.Period).IsModified = true;
                        db.Entry(dataEdit).Property(a => a.PeriodDate).IsModified = true;
                        db.Entry(dataEdit).Property(a => a.PurchaseDate).IsModified = true;
                        db.Entry(dataEdit).Property(a => a.Quantily).IsModified = true;
                        db.Entry(dataEdit).Property(a => a.TransferredTime).IsModified = true;
                        db.Entry(dataEdit).Property(a => a.Value).IsModified = true;
                        db.Entry(dataEdit).Property(a => a.InterestPayPeriod).IsModified = true;
                        db.Entry(dataEdit).Property(a => a.ContractCode).IsModified = true;
                        db.SaveChanges();

                        //add source multiple 
                        var checkAdd = AddSourceMultiple(model.Id, sourceInBuyAndSellBond);
                        if (!checkAdd)
                        {
                            transaction.Rollback();
                            return View(model);
                        }

                        var listIntTable1 = ConvertJsonDataContactDetail(dataTable1);
                        var listIntTable2 = ConvertJsonDataInterestPaymentPeriod(dataTable2);
                        var listIntTable3 = ConvertJsonDataPeriod(dataTable3);

                        //add list data contact detail (table child 1)
                        AddContactDetail(model.Id, listIntTable1, true);

                        //add list data contact detail (table child 2)
                        AddInterestPaymentPeriod(model.Id, listIntTable2, true);

                        //add list data contact detail (table child 2)
                        AddPeriod(model.Id, listIntTable3, true);

                        transaction.Commit();

                        ViewBag.StartupScript = "edit_success();";
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", "Đã có lỗi xảy ra trong quá trình lưu! Vui lòng thử lại.");
                        transaction.Rollback();
                    }
                }

                return View(model);

            }
            else
            {
                return View(model);
            }
        }

        public IEnumerable<ContactDisbursementDetail> ConvertJsonDataContactDetail(string dataString)
        {
            List<ContactDisbursementDetail> listData = new List<ContactDisbursementDetail>();
            var dataObjSplit0 = dataString.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                ContactDisbursementDetail dataObj = null;
                var dataObjString = string.Empty;
                if (i == 0)
                {
                    dataObjString = dataObjSplit1[i] + "}";
                }
                else
                {
                    var dataObjString0 = dataObjSplit1[i].Substring(1);
                    dataObjString = dataObjString0 + "}";
                }
                dataObj = JsonConvert.DeserializeObject<ContactDisbursementDetail>(dataObjString);

                dataObj.ImplementationDate = dataObj.ImplementationDate != null ? ((dataObj.ImplementationDate ?? default).ToLocalTime()) : dataObj.ImplementationDate;
                listData.Add(dataObj);
            }

            return listData;
        }

        public IEnumerable<InterestPaymentPeriod> ConvertJsonDataInterestPaymentPeriod(string dataString)
        {
            List<InterestPaymentPeriod> listData = new List<InterestPaymentPeriod>();
            var dataObjSplit0 = dataString.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                InterestPaymentPeriod dataObj = null;
                var dataObjString = string.Empty;
                if (i == 0)
                {
                    dataObjString = dataObjSplit1[i] + "}";
                }
                else
                {
                    var dataObjString0 = dataObjSplit1[i].Substring(1);
                    dataObjString = dataObjString0 + "}";
                }
                dataObj = JsonConvert.DeserializeObject<InterestPaymentPeriod>(dataObjString);

                dataObj.InterestPaymentDate = dataObj.InterestPaymentDate != null ? ((dataObj.InterestPaymentDate ?? default).ToLocalTime()) : dataObj.InterestPaymentDate;
                listData.Add(dataObj);
            }

            return listData;
        }

        public IEnumerable<PeriodViewModel> ConvertJsonDataPeriod(string dataString)
        {
            List<PeriodViewModel> listData = new List<PeriodViewModel>();
            if (dataString != null)
            {
                var dataObjSplit0 = dataString.Split('[');
                var dataObjSplit1 = dataObjSplit0[1].Split('}');
                for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
                {
                    PeriodViewModel dataObj = null;
                    var dataObjString = string.Empty;
                    if (i == 0)
                    {
                        dataObjString = dataObjSplit1[i] + "}";
                    }
                    else
                    {
                        var dataObjString0 = dataObjSplit1[i].Substring(1);
                        dataObjString = dataObjString0 + "}";
                    }
                    dataObj = JsonConvert.DeserializeObject<PeriodViewModel>(dataObjString);

                    listData.Add(dataObj);
                }
            }

            return listData;
        }

        public Boolean AddSourceMultiple(int buyAndSellBondId, int?[] sourceInBuyAndSellBond)
        {
            try
            {
                var sourceOld = db.SourceInBuyAndSellBonds.Where(x => x.BuyAndSellBondId == buyAndSellBondId && x.IsActive != false).ToList();
                //foreach (var item in sourceOld)
                //{
                //    item.IsActive = false;
                //    item.ModifiedAt = DateTime.Now;
                //    item.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                //}

                if (sourceOld != null)
                {
                    db.SourceInBuyAndSellBonds.RemoveRange(sourceOld);
                    db.SaveChanges();
                }


                if (sourceInBuyAndSellBond != null)
                {
                    var listSource = new List<SourceInBuyAndSellBond>();
                    foreach (var item in sourceInBuyAndSellBond)
                    {
                        var source = new SourceInBuyAndSellBond()
                        {
                            BuyAndSellBondId = buyAndSellBondId,
                            SourceId = item.Value,
                            CreatedAt = DateTime.Now,
                            CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId
                        };

                        listSource.Add(source);
                    }

                    db.SourceInBuyAndSellBonds.AddRange(listSource);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public Boolean AddContactDetail(int buyAndSellBondId, IEnumerable<ContactDisbursementDetail> contactDisbursementDetails, bool isEdit)
        {
            try
            {
                if (isEdit)
                {
                    var contactOld = db.ContactDisbursementDetails.Where(x => x.BuyAndSellBondId == buyAndSellBondId && x.IsActive != false).ToList();
                    //foreach (var item in contactOld)
                    //{
                    //    item.IsActive = false;
                    //    item.ModifiedAt = DateTime.Now;
                    //    item.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    //}

                    if (contactOld != null)
                    {
                        db.ContactDisbursementDetails.RemoveRange(contactOld);
                        db.SaveChanges();
                    }

                    //db.SaveChanges();
                }

                if (contactDisbursementDetails != null)
                {
                    var listContact = new List<ContactDisbursementDetail>();
                    foreach (var item in contactDisbursementDetails)
                    {
                        var source = new ContactDisbursementDetail()
                        {
                            BuyAndSellBondId = buyAndSellBondId,
                            ImplementationDate = item.ImplementationDate,
                            Value = item.Value,
                            Content = item.Content,
                            CreatedAt = DateTime.Now,
                            CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId
                        };

                        listContact.Add(source);
                    }

                    db.ContactDisbursementDetails.AddRange(listContact);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public Boolean AddInterestPaymentPeriod(int buyAndSellBondId, IEnumerable<InterestPaymentPeriod> interestPaymentPeriods, bool isEdit)
        {
            try
            {
                if (isEdit)
                {
                    var interestPeriodOld = db.InterestPaymentPeriods.Where(x => x.BuyAndSellBondId == buyAndSellBondId && x.IsActive != false).ToList();
                    //foreach (var item in interestPeriodOld)
                    //{
                    //    item.IsActive = false;
                    //    item.ModifiedAt = DateTime.Now;
                    //    item.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    //}

                    if (interestPeriodOld != null)
                    {
                        db.InterestPaymentPeriods.RemoveRange(interestPeriodOld);
                        db.SaveChanges();
                    }

                    //db.SaveChanges();
                }

                if (interestPaymentPeriods != null)
                {
                    var listInterest = new List<InterestPaymentPeriod>();
                    foreach (var item in interestPaymentPeriods)
                    {
                        var source = new InterestPaymentPeriod()
                        {
                            BuyAndSellBondId = buyAndSellBondId,
                            InterestPaymentDate = item.InterestPaymentDate,
                            AccruedInterest = item.AccruedInterest,
                            CalculateInterestNumber = item.CalculateInterestNumber,
                            CreatedAt = DateTime.Now,
                            CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId,
                            Content = item.Content
                        };

                        listInterest.Add(source);
                    }

                    db.InterestPaymentPeriods.AddRange(listInterest);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public Boolean AddPeriod(int buyAndSellBondId, IEnumerable<PeriodViewModel> periods, bool isEdit)
        {
            try
            {
                if (isEdit)
                {
                    var periodOld = db.Periods.Where(x => x.BuyAndSellBondId == buyAndSellBondId && x.IsActive != false).ToList();
                    //foreach (var item in periodOld)
                    //{
                    //    item.IsActive = false;
                    //    item.ModifiedAt = DateTime.Now;
                    //    item.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    //}

                    if (periodOld != null)
                    {
                        db.Periods.RemoveRange(periodOld);
                        db.SaveChanges();
                    }

                    db.SaveChanges();

                    //db.SaveChanges();
                }

                if (periods != null)
                {
                    var listPeriod = new List<Period>();
                    foreach (var item in periods)
                    {
                        var source = new Period()
                        {
                            BuyAndSellBondId = buyAndSellBondId,
                            IncurredId = item.IncurredId,
                            Note = item.Note,
                            Value = item.Value,
                            CreatedAt = DateTime.Now,
                            CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId
                        };

                        listPeriod.Add(source);
                    }

                    db.Periods.AddRange(listPeriod);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult CloseWindow()
        {
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var listDataContactDetailDelete = db.ContactDisbursementDetails.Where(x => x.BuyAndSellBondId == null);
                    var listInterestPaymentPeriodDelete = db.InterestPaymentPeriods.Where(x => x.BuyAndSellBondId == null);

                    db.ContactDisbursementDetails.RemoveRange(listDataContactDetailDelete);
                    db.SaveChanges();

                    db.InterestPaymentPeriods.RemoveRange(listInterestPaymentPeriodDelete);
                    db.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return Json(new { success = false });
                }
            }

            return Json(new { success = true });
        }


        [HttpPost]
        public JsonResult Deletes(string data)
        {
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var listDataConvert = ConvertJsonDeleteData(data);
                    var listData = db.BuyAndSellBonds.Where(x => listDataConvert.Contains(x.Id));
                    var listDataContact = db.ContactDisbursementDetails;
                    var listDataInterest = db.InterestPaymentPeriods;
                    var listDataPeriods = db.Periods;
                    var listDataIncurred = db.IncurredPurchases;

                    foreach (var item in listData)
                    {
                        item.IsActive = false;
                        item.ModifiedAt = DateTime.Now;
                        item.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                        db.SaveChanges();

                        var contact = listDataContact.Where(x => x.BuyAndSellBondId == item.Id && x.IsActive != false);
                        foreach (var childItem in contact)
                        {
                            childItem.IsActive = false;
                            childItem.ModifiedAt = DateTime.Now;
                            childItem.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                        }
                        db.SaveChanges();

                        var interestPaymentPeriods = listDataInterest.Where(x => x.BuyAndSellBondId == item.Id && x.IsActive != false);
                        foreach (var childItem in interestPaymentPeriods)
                        {
                            childItem.IsActive = false;
                            childItem.ModifiedAt = DateTime.Now;
                            childItem.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                        }
                        db.SaveChanges();

                        var periods = listDataPeriods.Where(x => x.BuyAndSellBondId == item.Id && x.IsActive != false);
                        foreach (var childItem in periods)
                        {
                            childItem.IsActive = false;
                            childItem.ModifiedAt = DateTime.Now;
                            childItem.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                        }
                        db.SaveChanges();

                        var incurredPurchases = listDataIncurred.Where(x => x.BuyAndSellBondId == item.Id && x.IsActive != false);
                        foreach (var childItem in incurredPurchases)
                        {
                            childItem.IsActive = false;
                            childItem.ModifiedAt = DateTime.Now;
                            childItem.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                        }
                        db.SaveChanges();
                    }

                    transaction.Commit();

                }
                catch (Exception e)
                {
                    transaction.Rollback();

                    return Json(new { success = false });
                }
            }
            return Json(new { success = true });
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
        public JsonResult Delete(int id)
        {
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var userDelete = db.BuyAndSellBonds.Find(id);
                    userDelete.ModifiedAt = DateTime.Now;
                    userDelete.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    userDelete.IsActive = false;
                    db.SaveChanges();

                    var contact = db.ContactDisbursementDetails.Where(x => x.BuyAndSellBondId == id && x.IsActive != false).ToList();
                    foreach (var item in contact)
                    {
                        item.IsActive = false;
                        item.ModifiedAt = DateTime.Now;
                        item.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    }
                    db.SaveChanges();

                    var interestPaymentPeriods = db.InterestPaymentPeriods.Where(x => x.BuyAndSellBondId == id && x.IsActive != false).ToList();
                    foreach (var item in interestPaymentPeriods)
                    {
                        item.IsActive = false;
                        item.ModifiedAt = DateTime.Now;
                        item.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    }
                    db.SaveChanges();

                    var periods = db.Periods.Where(x => x.BuyAndSellBondId == id && x.IsActive != false).ToList();
                    foreach (var item in periods)
                    {
                        item.IsActive = false;
                        item.ModifiedAt = DateTime.Now;
                        item.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    }
                    db.SaveChanges();

                    var incurredPurchases = db.IncurredPurchases.Where(x => x.BuyAndSellBondId == id && x.IsActive != false).ToList();
                    foreach (var item in incurredPurchases)
                    {
                        item.IsActive = false;
                        item.ModifiedAt = DateTime.Now;
                        item.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    }
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
        public JsonResult Undo(string data)
        {
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var listDataConvert = ConvertJsonDeleteData(data);
                    var listData = db.BuyAndSellBonds.Where(x => listDataConvert.Contains(x.Id));
                    var listDataContact = db.ContactDisbursementDetails;
                    var listDataInterest = db.InterestPaymentPeriods;
                    var listDataPeriods = db.Periods;
                    var listDataIncurred = db.IncurredPurchases;

                    foreach (var item in listData)
                    {
                        item.IsActive = true;
                        item.ModifiedAt = DateTime.Now;
                        item.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                        db.SaveChanges();

                        var contact = listDataContact.Where(x => x.BuyAndSellBondId == item.Id && x.IsActive == false);
                        foreach (var childItem in contact)
                        {
                            childItem.IsActive = true;
                            childItem.ModifiedAt = DateTime.Now;
                            childItem.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                        }
                        db.SaveChanges();

                        var interestPaymentPeriods = listDataInterest.Where(x => x.BuyAndSellBondId == item.Id && x.IsActive == false);
                        foreach (var childItem in interestPaymentPeriods)
                        {
                            childItem.IsActive = true;
                            childItem.ModifiedAt = DateTime.Now;
                            childItem.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                        }
                        db.SaveChanges();

                        var periods = listDataPeriods.Where(x => x.BuyAndSellBondId == item.Id && x.IsActive == false);
                        foreach (var childItem in periods)
                        {
                            childItem.IsActive = true;
                            childItem.ModifiedAt = DateTime.Now;
                            childItem.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                        }
                        db.SaveChanges();

                        var incurredPurchases = listDataIncurred.Where(x => x.BuyAndSellBondId == item.Id && x.IsActive == false);
                        foreach (var childItem in incurredPurchases)
                        {
                            childItem.IsActive = true;
                            childItem.ModifiedAt = DateTime.Now;
                            childItem.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                        }
                        db.SaveChanges();
                    }

                    transaction.Commit();

                }
                catch (Exception e)
                {
                    transaction.Rollback();

                    return Json(new { success = false });
                }
            }
            return Json(new { success = true });
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
        public ActionResult PeriodDetail(int? parrentID)
        {
            ViewBag.parentID = parrentID;

            return PartialView();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult CheckValueIncurred(PeriodPostCheck model)
        {
            var check = true;
            var test = ConvertJsonDataPeriod(model.Datatable).ToList();
            var listIntTable3 = test.Where(x => x.Value != null && x.IncurredId == model.IncurredId).ToList();
            IEnumerable<int> listIdViewData = (listIntTable3 != null) ? listIntTable3.Select(x => x.Id) : null;

            var incurred = db.IncurredPurchases.Find(model.IncurredId);

            if (incurred != null)
            {
                var periods = db.Periods.Where(x => !listIdViewData.Contains(x.Id) && x.IncurredId == model.IncurredId && x.IsActive != false);

                var listPeriod = new List<Period>();
                listPeriod.AddRange(listIntTable3);
                listPeriod.AddRange(periods);

                var sum = listPeriod.Sum(x => x.Value);
                if (sum > incurred.AmountOfMoney) check = false;
            }
            else check = false;

            return Json(new { ErrorMessage = string.Empty, success = check }, JsonRequestBehavior.AllowGet);
        }
    }
}
