using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Data;
using WebModels;
using WebMatrix.WebData;
using WEB.Models;

namespace WEB.Areas.Admin.Controllers
{
    public class ContractWarningsController : Controller
    {
        WebModels.WebContext db = new WebModels.WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        // GET: Admin/ContractWarnings
        public ActionResult Index()
        {
            var getUser = db.Set<UserProfile>().Find(WebSecurity.GetUserId(User.Identity.Name));

            if(getUser.IsReadNotification != true)
            {
                getUser.IsReadNotification = true;
                getUser.UnReadNotiCount = 0;
                db.SaveChanges();
            }


/*            var dateCompare = DateTime.Now.AddDays(6);
            var last5days = DateTime.Today.AddDays(5);
            var getInterestPaymentPeriodsId = db.IncurredPurchases.Where(x => x.InterestPaymentPeriodsId != null).Select(x => x.InterestPaymentPeriodsId).ToList();
            var getListDisbursementid = db.IncurredPurchases.Where(x => x.ContactDisbursementId != null).Select(x => x.ContactDisbursementId).ToList();
            //today warning

            var getNumEarningDueWarning = db.InterestPaymentPeriods.Where(y => (!getInterestPaymentPeriodsId.Contains(y.Id)) && y.BuyAndSellBondId != 0 && DbFunctions.TruncateTime(y.CreatedAt) == DateTime.Today && y.IsActive != false && y.InterestPaymentDate != null && DbFunctions.TruncateTime(y.InterestPaymentDate) < DbFunctions.TruncateTime(dateCompare)).Count();

            var getNumSettlementWarning = db.IncurredPurchases.Where(y => y.TransactionType == 2 && y.IsActive != false && DbFunctions.TruncateTime(y.CreatedAt) == DateTime.Today && y.IncurredDate != null && DbFunctions.TruncateTime(y.IncurredDate) < DbFunctions.TruncateTime(dateCompare)).Count();

            var getNumDisbursementWarning = db.ContactDisbursementDetails.Where(y => (!getListDisbursementid.Contains(y.Id)) && y.BuyAndSellBondId != 0 && DbFunctions.TruncateTime(y.CreatedAt) == DateTime.Today && y.IsActive != false && y.ImplementationDate != null && DbFunctions.TruncateTime(y.ImplementationDate) < DbFunctions.TruncateTime(dateCompare)).Count();

            var sumTodayWarning = getNumEarningDueWarning + getNumSettlementWarning + getNumDisbursementWarning;

            //last 5 days
            var getNumEarningDueWarningLast5Days = db.InterestPaymentPeriods.Where(y => (!getInterestPaymentPeriodsId.Contains(y.Id)) && y.BuyAndSellBondId != 0  && y.IsActive != false && y.InterestPaymentDate != null && DbFunctions.TruncateTime(y.InterestPaymentDate) == last5days).Count();

            var getNumSettlementWarningLast5Days = db.IncurredPurchases.Where(y => y.TransactionType == 2 && y.IsActive != false  && y.IncurredDate != null && DbFunctions.TruncateTime(y.IncurredDate) == last5days).Count();

            var getNumDisbursementWarningLast5Days = db.ContactDisbursementDetails.Where(y => (!getListDisbursementid.Contains(y.Id)) && y.BuyAndSellBondId != 0 && y.IsActive != false && y.ImplementationDate != null && DbFunctions.TruncateTime(y.ImplementationDate) == last5days).Count();

            var sumLast5DaysWarning = getNumEarningDueWarningLast5Days + getNumSettlementWarningLast5Days + getNumDisbursementWarningLast5Days;

            //total
            var notification = sumTodayWarning + sumLast5DaysWarning;*/

         


            return View();
        }
        public ActionResult EarningDueWarning()
        {

            return View();
        }
        public ActionResult SettlementWarning()
        {

            return View();
        }
       /* public ActionResult DisbursementWarning()
        {

            return View();
        }*/
        [AllowAnonymous]
        public ActionResult EarningDueWarning_Read([DataSourceRequest] DataSourceRequest request)
        {
            var dateCompare = DateTime.Now.AddDays(6);
            var getInterestPaymentPeriodsId = db.IncurredPurchases.Where(x => x.InterestPaymentPeriodsId != null && x.IsActive != false).Select(x => x.InterestPaymentPeriodsId).ToList();

            var data = db.InterestPaymentPeriods.Where(y => (!getInterestPaymentPeriodsId.Contains(y.Id)) && y.IsActive != false && y.InterestPaymentDate != null && DbFunctions.TruncateTime(y.InterestPaymentDate) < DbFunctions.TruncateTime(dateCompare)).ToList();
            var i = 1;

            var result = data.OrderByDescending(x => x.CreatedAt).Select(x => new
            {
                Count = i++,
                x.Id,
                ContractCode = x.BuyAndSellBond != null ? x.BuyAndSellBond.ContractCode : "",
                BondsCode = x.BuyAndSellBond != null ? x.BuyAndSellBond.AssetCategorys.AssetCode : "",
                BondsName = x.BuyAndSellBond != null ? x.BuyAndSellBond.AssetCategorys.Name : "",
                PaymentPeriodsContent = x.Content,
                CustomerName = x.BuyAndSellBond != null ?(x.BuyAndSellBond.Customer != null ? x.BuyAndSellBond.Customer.FullName : "") : "",
                EarningDate = x.InterestPaymentDate,
                AccruedValue = x.AccruedInterest,
                DaysLeft = (x.InterestPaymentDate - DateTime.Now).Value.TotalDays > 0 ?
                           "Còn "+ (x.InterestPaymentDate.Value.Date - DateTime.Now.Date).TotalDays + " ngày" :
                           "Quá "+ -(x.InterestPaymentDate.Value.Date - DateTime.Now.Date).TotalDays + " ngày"
            });

            //var data = from x in db.BuyAndSellBonds
            //           join y in db.InterestPaymentPeriods on x.Id equals y.BuyAndSellBondId
            //           where (y.InterestPaymentDate != null && DbFunctions.TruncateTime(y.InterestPaymentDate) < DbFunctions.TruncateTime(dateCompare))
                       

            return Json(result.ToList().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

            

           
        }

        [AllowAnonymous]
        public ActionResult SettlementWarning_Read([DataSourceRequest] DataSourceRequest request)
        {
            var dateCompare = DateTime.Now.AddDays(6);
            var getSettlementIncurred = db.IncurredPurchases.Where(y => y.IsActive != false && (y.TransactionType == (int)(TypeTransaction.ContractSettlementByTime) || y.TransactionType == (int)(TypeTransaction.ContractSettlementOnTime))).Select(y => y.BuyAndSellBondId).ToList();
            var getInterestIncurred = db.IncurredPurchases.Where(y => y.IsActive != false).Select(y => y.InterestPaymentPeriodsId).ToList();
            var data = db.BuyAndSellBonds.Where(y => (!getSettlementIncurred.Contains(y.Id)) && y.IsActive != false && y.PeriodDate != null && DbFunctions.TruncateTime(y.PeriodDate) < DbFunctions.TruncateTime(dateCompare)).ToList();
            var i = 1;

           /* var result = data.OrderByDescending(x => x.CreatedAt).Select(x => new
            {
                Count = i++,
                x.Id,
                ContractValue = x.Value,
                ContractDate = x.PurchaseDate,
                ContractCode = x.ContractCode,
                CustomerName = x.Customer.FullName,
                BondsCode = x.AssetCategorys.AssetCode,
                BondsName = x.AssetCategorys.Name,
                SettlementDate = x.PeriodDate,
                DaysLeft = (x.PeriodDate - DateTime.Now).Value.TotalDays > 0 ?
                           "Còn " + (x.PeriodDate.Value.Date - DateTime.Now.Date).TotalDays + " ngày" :
                           "Quá " + -(x.PeriodDate.Value.Date - DateTime.Now.Date).TotalDays + " ngày"
            });*/


            var dataresult = (from im in db.BuyAndSellBonds.Where(y => (!getSettlementIncurred.Contains(y.Id)) && y.IsActive != false && y.PeriodDate != null && DbFunctions.TruncateTime(y.PeriodDate) < DbFunctions.TruncateTime(dateCompare))

                              join st in db.IncurredPurchases.Where(x =>  x.IsActive != false && (x.TransactionType == (int)(TypeTransaction.GetInterestByTime) || x.TransactionType == (int)(TypeTransaction.GetInterestOntime))

                              ) on im.Id equals st.BuyAndSellBondId into group1
                              from item1 in group1.DefaultIfEmpty()

                              join nk in db.InterestPaymentPeriods.Where(x => (!getInterestIncurred.Contains(x.Id)) && x.IsActive != false) on im.Id equals nk.BuyAndSellBondId into group2
                              from item2 in group2.DefaultIfEmpty()
                              select new /*ContractWarningsReport()*/
                              {
                                  Count = 0,
                                  ID = im.Id,
                                  ContractValue = im.Value,
                                  ContractDate = im.PurchaseDate,
                                  ContractCode = im.ContractCode,
                                  CustomerName = im.Customer.FullName,
                                  BondsCode = im.AssetCategorys.AssetCode,
                                  BondsName = im.AssetCategorys.Name,
                                  SettlementDate = im.PeriodDate,
                                  ValueNotGet = (group2.Where(nk => nk.BuyAndSellBondId == im.Id).Select(x => x.AccruedInterest).Sum() != null  ? group2.Where(nk => nk.BuyAndSellBondId == im.Id).Select(x => x.AccruedInterest).Sum() : 0 ) + im.Value,
                                  

                              }).AsNoTracking().ToList().Select(B => new ContractWarningsReport()
                              {
                                  Count = B.Count,
                                  ID = B.ID,
                                  ContractValue = B.ContractValue,
                                  ContractDate = B.ContractDate,
                                  ContractCode = B.ContractCode,
                                  CustomerName = B.CustomerName,
                                  BondsCode = B.BondsCode,
                                  BondsName = B.BondsName,
                                  SettlementDate = B.SettlementDate,
                                  ValueNotGet = B.ValueNotGet,
                                  DaysLeft = (B.SettlementDate - DateTime.Now).Value.TotalDays > 0 ?
                                 "Còn " + (B.SettlementDate.Value.Date - DateTime.Now.Date).TotalDays + " ngày" :
                                 "Quá " + -(B.SettlementDate.Value.Date - DateTime.Now.Date).TotalDays + " ngày"
                              }).ToList().GroupBy(x => x.ID).Select(x => x.First()).ToList();

            dataresult.ForEach(x => x.Count = i++);


            return Json(dataresult.ToList().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);




        }

       /* [AllowAnonymous]
        public ActionResult DisbursementWarning_Read([DataSourceRequest] DataSourceRequest request)
        {
            var dateCompare = DateTime.Now.AddDays(6);
            var getListDisbursementid = db.IncurredPurchases.Where(x => x.ContactDisbursementId != null && x.TransactionType == 0 && x.IsActive != null).Select(x=>x.ContactDisbursementId).ToList();

            var data = db.ContactDisbursementDetails.Where(y =>( !getListDisbursementid.Contains(y.Id) )  && y.IsActive != false && y.ImplementationDate != null && DbFunctions.TruncateTime(y.ImplementationDate) < DbFunctions.TruncateTime(dateCompare)).ToList();
            var i = 1;

            var result = data.OrderByDescending(x=>x.CreatedAt).Select(x => new
            {
                Count = i++,
                x.Id,
                ContractName = x.BuyAndSellBond != null ? x.BuyAndSellBond.ContractName : "",
                CustomerName = x.BuyAndSellBond != null ? (x.BuyAndSellBond.Customer != null ? x.BuyAndSellBond.Customer.FullName : "") : "",
                Mobile = x.BuyAndSellBond != null ? (x.BuyAndSellBond.Customer != null ? x.BuyAndSellBond.Customer.Mobile : "") : "",
                DisbursementDate = x.ImplementationDate,
                DaysLeft = (x.ImplementationDate - DateTime.Now).Value.TotalDays > 0 ?
                           "Còn " + (x.ImplementationDate.Value.Date - DateTime.Now.Date).TotalDays + " ngày" :
                           "Quá " + -(x.ImplementationDate.Value.Date - DateTime.Now.Date).TotalDays + " ngày"
            });

            //var data = from x in db.BuyAndSellBonds
            //           join y in db.InterestPaymentPeriods on x.Id equals y.BuyAndSellBondId
            //           where (y.InterestPaymentDate != null && DbFunctions.TruncateTime(y.InterestPaymentDate) < DbFunctions.TruncateTime(dateCompare))


            return Json(result.ToList().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);




        }*/
    }
}