using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data;
using WebMatrix.WebData;
using System.Globalization;
using WebModels;

namespace WEB.Areas.ContentType.Controllers
{
    public class ContractWarningController : Controller
    {
        WebModels.WebContext db = new WebModels.WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        // GET: ContentType/ContractWarning
        public ActionResult _PubIndex(int? id)
        {
            ViewBag.WebModule = db.WebModules.FirstOrDefault(x => x.ID == id);
            var user = WebSecurity.GetUserId(User.Identity.Name);

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult EarningDueWarning()
        {
            var user = WebSecurity.GetUserId(User.Identity.Name);
            var dateCompare = DateTime.Now.AddDays(6);
            var getInterestPaymentPeriodsId = db.IncurredPurchases.Where(x => x.IsActive!= false && x.InterestPaymentPeriodsId != null).Select(x => x.InterestPaymentPeriodsId).ToList();

            var data = db.InterestPaymentPeriods.Where(y => (!getInterestPaymentPeriodsId.Contains(y.Id)) 
            && y.BuyAndSellBond.Customer.UserId == user && y.IsActive != false && y.InterestPaymentDate != null && DbFunctions.TruncateTime(y.InterestPaymentDate) < DbFunctions.TruncateTime(dateCompare)).ToList();
            var i = 1;

            var result = data.OrderByDescending(x => x.CreatedAt).Select(x => new
            {
                Count = i++,
                x.Id,
                ContractName = x.BuyAndSellBond != null ? x.BuyAndSellBond.ContractName : "",
                ContractCode = x.BuyAndSellBond != null ? x.BuyAndSellBond.ContractCode : "",
                BondsCode = x.BuyAndSellBond != null ? x.BuyAndSellBond.AssetCategorys.AssetCode : "",
                BondsName = x.BuyAndSellBond != null ? x.BuyAndSellBond.AssetCategorys.Name : "",
                PaymentPeriodsContent = x.Content,
                EarningDate = x.InterestPaymentDate != DateTime.MinValue ? (x.InterestPaymentDate ?? default).ToString("dd/MM/yyyy") : "",
                AccruedValue = x.AccruedInterest,
                DaysLeft = (x.InterestPaymentDate - DateTime.Now).Value.TotalDays > 0 ?
                           "Còn " + (x.InterestPaymentDate.Value.Date - DateTime.Now.Date).TotalDays + " ngày" :
                           "Quá " + -(x.InterestPaymentDate.Value.Date - DateTime.Now.Date).TotalDays + " ngày"
            });
            return Json(result.ToList(), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult SettlementWarning()
        {
            var user = WebSecurity.GetUserId(User.Identity.Name);
            var dateCompare = DateTime.Now.AddDays(6);

            var getSettlementIncurred = db.IncurredPurchases.Where(y => y.IsActive != false && y.Customer.UserId == user && (y.TransactionType == (int)(TypeTransaction.ContractSettlementByTime) || y.TransactionType == (int)(TypeTransaction.ContractSettlementOnTime))).Select(y => y.BuyAndSellBondId).ToList();

            /*var data = db.IncurredPurchases.Where(y => y.TransactionType == 2  && y.IsActive != false && y.Customer.UserId == user && y.IncurredDate != null && DbFunctions.TruncateTime(y.IncurredDate) < DbFunctions.TruncateTime(dateCompare)).ToList();*/
            var data = db.BuyAndSellBonds.Where(y => (!getSettlementIncurred.Contains(y.Id)) && y.IsActive != false && y.Customer.UserId == user && y.PeriodDate != null && DbFunctions.TruncateTime(y.PeriodDate) < DbFunctions.TruncateTime(dateCompare)).ToList();
            var i = 1;

            var result = data.OrderByDescending(x => x.CreatedAt).Select(x => new
            {
                Count = i++,
                x.Id,
                ContractName = x.ContractName,
                ContractValue = x.Value,
                ContractDate = x.PurchaseDate != DateTime.MinValue ? (x.PurchaseDate ?? default).ToString("dd/MM/yyyy") : "",
                ContractCode = x.ContractCode,
                CustomerName = x.Customer.FullName,
                BondsCode = x.AssetCategorys.AssetCode,
                BondsName = x.AssetCategorys.Name,
                SettlementDate = x.PeriodDate != DateTime.MinValue ? (x.PeriodDate ?? default).ToString("dd/MM/yyyy") : "",
                DaysLeft = (x.PeriodDate - DateTime.Now).Value.TotalDays > 0 ?
                           "Còn " + (x.PeriodDate.Value.Date - DateTime.Now.Date).TotalDays + " ngày" :
                           "Quá " + -(x.PeriodDate.Value.Date - DateTime.Now.Date).TotalDays + " ngày"
            });
            return Json(result.ToList(), JsonRequestBehavior.AllowGet);
        }

       /* [AllowAnonymous]
        public ActionResult DisbursementWarning()
        {
            var user = WebSecurity.GetUserId(User.Identity.Name);
            var dateCompare = DateTime.Now.AddDays(6);
            var getListDisbursementid = db.IncurredPurchases.Where(x => x.ContactDisbursementId != null && x.TransactionType == 0 && x.IsActive != null).Select(x => x.ContactDisbursementId).ToList();

            var data = db.ContactDisbursementDetails.Where(y => (!getListDisbursementid.Contains(y.Id)) && y.IsActive != false && y.BuyAndSellBond.Customer.UserId == user && y.ImplementationDate != null && DbFunctions.TruncateTime(y.ImplementationDate) < DbFunctions.TruncateTime(dateCompare)).ToList();
            var i = 1;

            var result = data.OrderByDescending(x => x.CreatedAt).Select(x => new
            {
                Count = i++,
                x.Id,
                ContractName = x.BuyAndSellBond != null ? x.BuyAndSellBond.ContractName : "",
                DisbursementDate = x.ImplementationDate != DateTime.MinValue ? (x.ImplementationDate ?? default).ToString("dd/MM/yyyy") : "",
                DaysLeft = (x.ImplementationDate - DateTime.Now).Value.TotalDays > 0 ?
                             "Còn " + (x.ImplementationDate.Value.Date - DateTime.Now.Date).TotalDays + " ngày" :
                             "Quá " + -(x.ImplementationDate.Value.Date - DateTime.Now.Date).TotalDays + " ngày"
            });

            return Json(result.ToList(), JsonRequestBehavior.AllowGet);
        }*/
    }
}