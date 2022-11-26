using Kendo.Mvc.UI;
//using Kendo.Mvc.Extensions;
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
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Net;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Web.Hosting;

namespace WEB.Areas.ContentType.Controllers
{
    /*[VanTaiAuthorize]*/
    public class ReportController : Controller
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        public ActionResult _PubIndex(int? id)
        {
          
            ViewBag.WebModule = db.WebModules.FirstOrDefault(x => x.ID == id);
            return View();
        }
        
        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetInvestment( string time)
        {
            string[] stringSeparators = new string[] { " Đến " };
            var timeArray = time.Split(stringSeparators, StringSplitOptions.None);
            var startTime = timeArray[0];
            var endTime = timeArray[1];
            DateTime timeStart = DateTime.Parse(startTime);
            DateTime timeEnd = DateTime.Parse(endTime);

            var user = WebSecurity.GetUserId(User.Identity.Name);
            int i = 1;

            var data = (from im in db.BuyAndSellBonds.Where(x => x.IsActive != false && DbFunctions.TruncateTime(x.PurchaseDate) <= DbFunctions.TruncateTime(timeEnd)
                        && DbFunctions.TruncateTime(x.PurchaseDate) >= DbFunctions.TruncateTime(timeStart) && x.CustomerId == user)

                        join pr in db.UserProfiles.Where(x => x.IsActive != false && x.Type != (int)TypeAccount.Admin) on im.CustomerId equals pr.UserId into group1
                        from item1 in group1

                        join st in db.IncurredPurchases.Where(x => x.IsActive != false && (x.TransactionType == (int)(TypeTransaction.GetInterestByTime) || x.TransactionType == (int)(TypeTransaction.GetInterestOntime))
                        //&& DbFunctions.TruncateTime(x.IncurredDate) <= DbFunctions.TruncateTime(timeEnd)
                        //&& DbFunctions.TruncateTime(x.IncurredDate) >= DbFunctions.TruncateTime(timeStart)
                        ) on im.Id equals st.BuyAndSellBondId into group2
                        from item2 in group2.DefaultIfEmpty()

                        join nk in db.InterestPaymentPeriods.Where(x => x.IsActive != false) on im.Id equals nk.BuyAndSellBondId into group3
                        from item3 in group3.DefaultIfEmpty()
                        select new 
                        {
                            STT = 0,
                            AccruedInterest = group3.Where(nk => nk.BuyAndSellBondId == im.Id).Select(x => x.AccruedInterest).Sum(),
                            InvestmentCode = im.ContractCode,
                            BondsName = im.AssetCategorys.Name,
                            BondsCode = im.AssetCategorys.AssetCode,
                            PurchaseDate = im.PurchaseDate,
                            PeriodDate = im.PeriodDate,
                            Value = im.Value,
                            RoundedPurchaseValue = im.RoundedPurchaseValue,
                            InvestmentInterestRate = im.InterestRate,
                            Count = im.Quantily,
                            RealIncome = item2.AmountOfMoney,
                            Total = im.Value + (item2.AmountOfMoney != null ? item2.AmountOfMoney : 0)
                        }).AsNoTracking().ToList().Select(B=> new InvestmentReport() {
                            STT = B.STT,
                            AccruedInterest = B.AccruedInterest,
                            InvestmentCode = B.InvestmentCode,
                            BondsName = B.BondsName,
                            BondsCode = B.BondsCode,
                            InvestmentPeriodDate = B.PeriodDate != DateTime.MinValue ? (B.PeriodDate ?? default).ToString("dd/MM/yyyy") : "",
                            InvestmentPurchaseDate = B.PurchaseDate != DateTime.MinValue ? (B.PurchaseDate ?? default).ToString("dd/MM/yyyy") : "",
                            Value = B.Value,
                            RoundedPurchaseValue = B.RoundedPurchaseValue,
                            InvestmentInterestRate = B.InvestmentInterestRate,
                            Count = B.Count,
                            RealIncome = B.RealIncome,
                            Total = B.Total
                        }).ToList().GroupBy(x => x.InvestmentCode).Select(x => x.First()).ToList(); 

            data.Each(x => x.STT = i++);

            return Json(data.ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult ExportExcel(string time)
        {
            string[] stringSeparators = new string[] { " Đến " };
            var timeArray = time.Split(stringSeparators, StringSplitOptions.None);
            var startTime = timeArray[0];
            var endTime = timeArray[1];
            DateTime timeStart = DateTime.Parse(startTime);
            DateTime timeEnd = DateTime.Parse(endTime);

            var user = WebSecurity.GetUserId(User.Identity.Name);
            int i = 1;

            var listData = (from im in db.BuyAndSellBonds.Where(x => x.IsActive != false && DbFunctions.TruncateTime(x.PurchaseDate) <= DbFunctions.TruncateTime(timeEnd)
                       && DbFunctions.TruncateTime(x.PurchaseDate) >= DbFunctions.TruncateTime(timeStart) && x.CustomerId == user)

                            join pr in db.UserProfiles.Where(x => x.IsActive != false && x.Type != (int)TypeAccount.Admin) on im.CustomerId equals pr.UserId into group1
                            from item1 in group1

                            join st in db.IncurredPurchases.Where(x => x.IsActive != false && (x.TransactionType == (int)(TypeTransaction.GetInterestByTime) || x.TransactionType == (int)(TypeTransaction.GetInterestOntime))
                            //&& DbFunctions.TruncateTime(x.IncurredDate) <= DbFunctions.TruncateTime(timeEnd)
                            //&& DbFunctions.TruncateTime(x.IncurredDate) >= DbFunctions.TruncateTime(timeStart)
                            ) on im.Id equals st.BuyAndSellBondId into group2
                            from item2 in group2.DefaultIfEmpty()

                            join nk in db.InterestPaymentPeriods.Where(x => x.IsActive != false) on im.Id equals nk.BuyAndSellBondId into group3
                            from item3 in group3.DefaultIfEmpty()
                            select new
                            {
                                STT = 0,
                                AccruedInterest = group3.Where(nk => nk.BuyAndSellBondId == im.Id).Select(x => x.AccruedInterest).Sum(),
                                InvestmentCode = im.ContractCode,
                                BondsName = im.AssetCategorys.Name,
                                BondsCode = im.AssetCategorys.AssetCode,
                                PurchaseDate = im.PurchaseDate,
                                PeriodDate = im.PeriodDate,
                                Value = im.Value,
                                RoundedPurchaseValue = im.RoundedPurchaseValue,
                                InvestmentInterestRate = im.InterestRate,
                                Count = im.Quantily,
                                RealIncome = item2.AmountOfMoney,
                                Total = im.Value + (item2.AmountOfMoney != null ? item2.AmountOfMoney : 0)
                            }).AsNoTracking().ToList().Select(B => new InvestmentReport()
                            {
                                STT = B.STT,
                                AccruedInterest = B.AccruedInterest,
                                InvestmentCode = B.InvestmentCode,
                                BondsName = B.BondsName,
                                BondsCode = B.BondsCode,
                                InvestmentPeriodDate = B.PeriodDate != DateTime.MinValue ? (B.PeriodDate ?? default).ToString("dd/MM/yyyy") : "",
                                InvestmentPurchaseDate = B.PurchaseDate != DateTime.MinValue ? (B.PurchaseDate ?? default).ToString("dd/MM/yyyy") : "",
                                Value = B.Value,
                                RoundedPurchaseValue = B.RoundedPurchaseValue,
                                InvestmentInterestRate = B.InvestmentInterestRate,
                                Count = B.Count,
                                RealIncome = B.RealIncome,
                                Total = B.Total
                            }).ToList().GroupBy(x => x.InvestmentCode).Select(x => x.First()).ToList();


            listData.Each(x => x.STT = i++);
            var result = DownloadFile(listData,startTime,endTime);
            return Json(new
            {
                fileStream = Convert.ToBase64String(result),
                fileName = "Báo_cáo_danh_mục_tài_sản_đầu_tư.xlsx"
            },JsonRequestBehavior.AllowGet);
        }
        public byte[] DownloadFile(List<InvestmentReport> models,string startTime, string EndTime)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\BaoCaoDanhMucTaiSanDauTu.xlsx", HostingEnvironment.MapPath("/Uploads")));

            if (fileinfo.Exists)
            {
                var user = WebSecurity.GetUserId(User.Identity.Name);
                var infoCustomer = db.UserProfiles.Where(x => x.IsActive != false && x.Type != (int)TypeAccount.Admin && x.UserId == user).FirstOrDefault();

                using (var p = new ExcelPackage(fileinfo))
                {
                    var productWorksheet = p.Workbook.Worksheets[0];
                    productWorksheet.Select();
                    int i = 0;

                    productWorksheet.Cells[2, 1].Value = "Tên người dùng: " + infoCustomer.FullName;
                    productWorksheet.Cells[3, 1].Value = "SĐT: " + infoCustomer.Mobile;
                    productWorksheet.Cells[4, 1].Value = "Email: " + infoCustomer.Email;
                    productWorksheet.Cells[5, 1].Value = "Từ ngày " + startTime + EndTime;

                    foreach (var item in models)
                    {
                        productWorksheet.Cells[i + 7, 1].Value = i + 1;
                        productWorksheet.Cells[i + 7, 2].Value = item.InvestmentCode;
                        productWorksheet.Cells[i + 7, 3].Value = item.BondsCode;
                        productWorksheet.Cells[i + 7, 4].Value = item.BondsName;
                        productWorksheet.Cells[i + 7, 5].Value = item.InvestmentPurchaseDate;
                        productWorksheet.Cells[i + 7, 6].Value = item.InvestmentPeriodDate;
                        productWorksheet.Cells[i + 7, 7].Value = item.Count;
                        productWorksheet.Cells[i + 7, 8].Value = item.Value;
                        productWorksheet.Cells[i + 7, 9].Value = item.RoundedPurchaseValue;
                        productWorksheet.Cells[i + 7, 10].Value = item.InvestmentInterestRate;
                        productWorksheet.Cells[i + 7, 11].Value = item.AccruedInterest;
                        productWorksheet.Cells[i + 7, 12].Value = item.RealIncome;
                        productWorksheet.Cells[i + 7, 13].Value = item.Total;

                        productWorksheet.Cells[i + 7, 7].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 8].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 9].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 10].Style.Numberformat.Format = "#,##0.00";
                        productWorksheet.Cells[i + 7, 11].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 12].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 13].Style.Numberformat.Format = "#,##0";


                        i++;
                    }

                    productWorksheet.Cells[i + 7, 2].Value = "Tổng cộng:";
                    productWorksheet.Cells[i + 7, 7].Formula = "=SUBTOTAL(9,G7:G" + (i + 6).ToString() + ")";
                    productWorksheet.Cells[i + 7, 8].Formula = "=SUBTOTAL(9,H7:H" + (i + 6).ToString() + ")";
                    productWorksheet.Cells[i + 7, 9].Formula = "=SUBTOTAL(9,I7:I" + (i + 6).ToString() + ")";
                    productWorksheet.Cells[i + 7, 11].Formula = "=SUBTOTAL(9,K7:K" + (i + 6).ToString() + ")";
                    productWorksheet.Cells[i + 7, 12].Formula = "=SUBTOTAL(9,L7:L" + (i + 6).ToString() + ")";
                    productWorksheet.Cells[i + 7, 13].Formula = "=SUBTOTAL(9,M7:M" + (i + 6).ToString() + ")";

                    productWorksheet.Cells[i + 7, 7].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 8].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 9].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 11].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 12].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 13].Style.Numberformat.Format = "#,##0";


                    productWorksheet.Cells[i + 7, 2].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 7].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 8].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 9].Style.Font.Bold = true;

                    productWorksheet.Cells[i + 7, 11].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 12].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 13].Style.Font.Bold = true;

                    string columnRange = "A6:M" + (i + 7).ToString();
                    var modelTable = productWorksheet.Cells[columnRange];
                    modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    modelTable.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    modelTable.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                    return p.GetAsByteArray();
                }
            }
            else
            {
                return null;
            }
        }
    }
}