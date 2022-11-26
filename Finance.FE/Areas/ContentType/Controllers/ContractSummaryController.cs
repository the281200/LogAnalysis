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
using System.Globalization;
using System.Drawing;

namespace WEB.Areas.ContentType.Controllers
{
    /*[VanTaiAuthorize]*/
    public class ContractSummaryController : Controller
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
        public ActionResult GetInvestment()
        {



            var user = WebSecurity.GetUserId(User.Identity.Name);
            int i = 1;
            int c = 1;

            var listReport = new List<InvestmentReport>();

            var group = (from im in db.AssetCategorys.Where(x => x.IsActive != false)

                         join pr in db.BuyAndSellBonds.Where(x => x.IsActive != false && x.CustomerId == user) on im.Id equals pr.AssetCategorysId into group1
                         from item1 in group1
                         select new
                         {
                             STT = 0,
                             ContractCodeName = im.AssetCode,
                             BondsCodeName = "Nhóm: " + im.Name,
                             NumOfBondString = group1.Where(pr => pr.AssetCategorysId == im.Id).Select(x => x.Id).Count(),
                             Value = group1.Where(pr => pr.AssetCategorysId == im.Id).Select(x => x.Value).Sum(),
                             RoundedPurchaseValue = group1.Where(pr => pr.AssetCategorysId == im.Id).Select(x => x.RoundedPurchaseValue).Sum(),
                             PreTaxProfit = group1.Where(pr => pr.AssetCategorysId == im.Id).Select(x => x.PreTaxProfit).Sum(),
                             ProfitAfterTax = group1.Where(pr => pr.AssetCategorysId == im.Id).Select(x => x.ProfitAfterTax).Sum(),
                             SumMoney = group1.Where(pr => pr.AssetCategorysId == im.Id).Select(x => x.ProfitAfterTax).Sum() + group1.Where(pr => pr.AssetCategorysId == im.Id).Select(x => x.Value).Sum(),

                         }).AsNoTracking().ToList().Select(B => new InvestmentReport()
                         {
                             STT = B.STT,
                             ContractCodeName = B.ContractCodeName,
                             BondsCodeName = B.BondsCodeName,
                             NumOfBondString = B.NumOfBondString.ToString() + " " + "(Số lượng hợp đồng)",
                             Value = B.Value,
                             RoundedPurchaseValue = B.RoundedPurchaseValue,
                             InvestmentPurchaseDateString = null,
                             PeriodDateString = null,
                             PreTaxProfit = B.PreTaxProfit,
                             ProfitAfterTax = B.ProfitAfterTax,
                             SumMoney = B.SumMoney

                         }).ToList().GroupBy(x => x.ContractCodeName).Select(x => x.First()).ToList();
            group.Each(x => x.STT = i++);
            foreach (var item in group)
            {
                var data = (from im in db.BuyAndSellBonds.Where(x => x.IsActive != false && x.AssetCategorys.AssetCode == item.ContractCodeName && x.CustomerId == user)
                            join st in db.IncurredPurchases.Where(x => x.IsActive != false && x.InterestPaymentPeriodsId == null

                            ) on im.Id equals st.BuyAndSellBondId into group1
                            from item1 in group1.DefaultIfEmpty()
                            select new
                            {
                                STT = 0,
                                ContractCodeName = im.ContractCode + " - " + im.ContractName,
                                BondsCodeName = im.AssetCategorys.AssetCode + " - " + im.AssetCategorys.Name,
                                Quantily = im.Quantily,
                                UnitPrice = im.UnitPrice,
                                Value = im.Value,
                                RoundedPurchaseValue = im.RoundedPurchaseValue,
                                InvestmentPurchaseDateString = im.PurchaseDate,

                                PeriodDateString = im.PeriodDate,
                                InterestPayPeriod = im.InterestPayPeriod,
                                PreTaxProfit = im.PreTaxProfit,
                                ProfitAfterTax = im.ProfitAfterTax,
                                SumMoney = im.ProfitAfterTax + im.Value,
                                Rates = im.Rates,
                                Status = item1.TransactionType > 1 ? "Đã thanh lý/tất toán" : "Đang thực hiện"


                            }).AsNoTracking().ToList().Select(B => new InvestmentReport()
                            {
                                STT = B.STT,
                                ContractCodeName = B.ContractCodeName,
                                BondsCodeName = B.BondsCodeName,
                                NumOfBondString = B.Quantily.ToString() + "/" + B.UnitPrice.Value.ToString("N0", CultureInfo.InvariantCulture),
                                Value = B.Value,
                                RoundedPurchaseValue = B.RoundedPurchaseValue,
                                InvestmentPurchaseDateString = String.Format("{0:dd/MM/yyyy}", B.InvestmentPurchaseDateString),
                                CalculateInterestDateString = (B.PeriodDateString.Value.Date - B.InvestmentPurchaseDateString.Value.Date).Days.ToString(),
                                PeriodDateString = String.Format("{0:dd/MM/yyyy}", B.PeriodDateString),
                                InterestPayPeriod = B.InterestPayPeriod,
                                PreTaxProfit = B.PreTaxProfit,
                                ProfitAfterTax = B.ProfitAfterTax,
                                SumMoney = B.SumMoney,
                                Rates = B.Rates,
                                Status = B.Status

                            }).ToList().GroupBy(x => x.ContractCodeName).Select(x => x.First()).ToList();

                data.Each(x => x.STT = c++);
                listReport.Add(item);
                foreach (var child in data)
                {
                    listReport.Add(child);
                }

            }


            return Json(listReport.ToList(), JsonRequestBehavior.AllowGet);

            
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult ExportExcel()
        {


            var user = WebSecurity.GetUserId(User.Identity.Name);
            int i = 1;
            int c = 1;

            var listReport = new List<InvestmentReport>();

            var group = (from im in db.AssetCategorys.Where(x => x.IsActive != false)

                         join pr in db.BuyAndSellBonds.Where(x => x.IsActive != false && x.CustomerId == user) on im.Id equals pr.AssetCategorysId into group1
                         from item1 in group1
                         select new
                         {
                             STT = 0,
                             ContractCodeName = im.AssetCode,
                             BondsCodeName = "Nhóm: " + im.Name,
                             NumOfBondString = group1.Where(pr => pr.AssetCategorysId == im.Id).Select(x => x.Id).Count(),
                             Value = group1.Where(pr => pr.AssetCategorysId == im.Id).Select(x => x.Value).Sum(),
                             RoundedPurchaseValue = group1.Where(pr => pr.AssetCategorysId == im.Id).Select(x => x.RoundedPurchaseValue).Sum(),
                             PreTaxProfit = group1.Where(pr => pr.AssetCategorysId == im.Id).Select(x => x.PreTaxProfit).Sum(),
                             ProfitAfterTax = group1.Where(pr => pr.AssetCategorysId == im.Id).Select(x => x.ProfitAfterTax).Sum(),
                             SumMoney = group1.Where(pr => pr.AssetCategorysId == im.Id).Select(x => x.ProfitAfterTax).Sum() + group1.Where(pr => pr.AssetCategorysId == im.Id).Select(x => x.Value).Sum(),

                         }).AsNoTracking().ToList().Select(B => new InvestmentReport()
                         {
                             STT = B.STT,
                             ContractCodeName = B.ContractCodeName,
                             BondsCodeName = B.BondsCodeName,
                             NumOfBondString = B.NumOfBondString.ToString() + " " + "(Số lượng hợp đồng)",
                             Value = B.Value,
                             RoundedPurchaseValue = B.RoundedPurchaseValue,
                             InvestmentPurchaseDateString = null,
                             PeriodDateString = null,
                             PreTaxProfit = B.PreTaxProfit,
                             ProfitAfterTax = B.ProfitAfterTax,
                             SumMoney = B.SumMoney

                         }).ToList().GroupBy(x => x.ContractCodeName).Select(x => x.First()).ToList();
            group.Each(x => x.STT = i++);
            foreach (var item in group)
            {
                var data = (from im in db.BuyAndSellBonds.Where(x => x.IsActive != false && x.AssetCategorys.AssetCode == item.ContractCodeName && x.CustomerId == user)
                            join st in db.IncurredPurchases.Where(x => x.IsActive != false && x.InterestPaymentPeriodsId == null

                            ) on im.Id equals st.BuyAndSellBondId into group1
                            from item1 in group1.DefaultIfEmpty()
                            select new
                            {
                                STT = 0,
                                ContractCodeName = im.ContractCode + " - " + im.ContractName,
                                BondsCodeName = im.AssetCategorys.AssetCode + " - " + im.AssetCategorys.Name,
                                Quantily = im.Quantily,
                                UnitPrice = im.UnitPrice,
                                Value = im.Value,
                                RoundedPurchaseValue = im.RoundedPurchaseValue,
                                InvestmentPurchaseDateString = im.PurchaseDate,

                                PeriodDateString = im.PeriodDate,
                                InterestPayPeriod = im.InterestPayPeriod,
                                PreTaxProfit = im.PreTaxProfit,
                                ProfitAfterTax = im.ProfitAfterTax,
                                SumMoney = im.ProfitAfterTax + im.Value,
                                Rates = im.Rates,
                                Status = item1.TransactionType > 1 ? "Đã thanh lý/tất toán" : "Đang thực hiện"


                            }).AsNoTracking().ToList().Select(B => new InvestmentReport()
                            {
                                STT = B.STT,
                                ContractCodeName = B.ContractCodeName,
                                BondsCodeName = B.BondsCodeName,
                                NumOfBondString = B.Quantily.ToString() + "/" + B.UnitPrice.Value.ToString("N0", CultureInfo.InvariantCulture),
                                Value = B.Value,
                                RoundedPurchaseValue = B.RoundedPurchaseValue,
                                InvestmentPurchaseDateString = String.Format("{0:dd/MM/yyyy}", B.InvestmentPurchaseDateString),
                                CalculateInterestDateString = (B.PeriodDateString.Value.Date - B.InvestmentPurchaseDateString.Value.Date).Days.ToString(),
                                PeriodDateString = String.Format("{0:dd/MM/yyyy}", B.PeriodDateString),
                                InterestPayPeriod = B.InterestPayPeriod,
                                PreTaxProfit = B.PreTaxProfit,
                                ProfitAfterTax = B.ProfitAfterTax,
                                SumMoney = B.SumMoney,
                                Rates = B.Rates,
                                Status = B.Status

                            }).ToList().GroupBy(x => x.ContractCodeName).Select(x => x.First()).ToList();

                data.Each(x => x.STT = c++);
                listReport.Add(item);
                foreach (var child in data)
                {
                    listReport.Add(child);
                }

            }

            var result = DownloadFile(listReport);
            return Json(new
            {
                fileStream = Convert.ToBase64String(result),
                fileName = "Báo_cáo_tổng_hợp_tình_hình_hợp_đồng.xlsx"
            }, JsonRequestBehavior.AllowGet);
        }
        public byte[] DownloadFile(List<InvestmentReport> models)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\BaoCaoTongHopTinhHinhHopDong.xlsx", HostingEnvironment.MapPath("/Uploads")));

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


                    foreach (var item in models)
                    {
                        productWorksheet.Cells[i + 7, 1].Value = item.STT;
                        productWorksheet.Cells[i + 7, 2].Value = item.ContractCodeName;
                        productWorksheet.Cells[i + 7, 3].Value = item.BondsCodeName;
                        productWorksheet.Cells[i + 7, 4].Value = item.NumOfBondString;
                        productWorksheet.Cells[i + 7, 5].Value = item.Value;
                        productWorksheet.Cells[i + 7, 6].Value = item.RoundedPurchaseValue;
                        productWorksheet.Cells[i + 7, 7].Value = item.InvestmentPurchaseDateString;
                        productWorksheet.Cells[i + 7, 8].Value = item.CalculateInterestDateString;
                        productWorksheet.Cells[i + 7, 9].Value = item.PeriodDateString;
                        productWorksheet.Cells[i + 7, 10].Value = item.InterestPayPeriod;
                        productWorksheet.Cells[i + 7, 11].Value = item.PreTaxProfit;
                        productWorksheet.Cells[i + 7, 12].Value = item.ProfitAfterTax;
                        productWorksheet.Cells[i + 7, 13].Value = item.SumMoney;
                        productWorksheet.Cells[i + 7, 14].Value = item.Rates;
                        productWorksheet.Cells[i + 7, 15].Value = item.Status;

                        productWorksheet.Cells[i + 7, 5].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 6].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 9].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 10].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 11].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 12].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 13].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 14].Style.Numberformat.Format = "#,##0.00";

                        if ((item.InvestmentPurchaseDateString == "" || item.InvestmentPurchaseDateString == null) && (item.PeriodDateString == "" || item.PeriodDateString == null))
                        {
                            Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#FFE699");
                            var address = "A" + (i + 7) + ":O" + (i + 7);
                            productWorksheet.Cells[address].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            productWorksheet.Cells[address].Style.Fill.BackgroundColor.SetColor(colFromHex);

                            var boldText = productWorksheet.Cells[address].Style.Font;
                            boldText.SetFromFont(new Font("Times New Roman", 11)); //Do this first
                            boldText.Bold = true;
                        }

                        i++;
                    }

                    productWorksheet.Cells[i + 7, 2].Value = "Tổng cộng:";
                    productWorksheet.Cells[i + 7, 5].Formula = "=SUBTOTAL(9,E7:E" + (i + 6).ToString() + ")/2";
                    productWorksheet.Cells[i + 7, 6].Formula = "=SUBTOTAL(9,F7:F" + (i + 6).ToString() + ")/2";
                    productWorksheet.Cells[i + 7, 11].Formula = "=SUBTOTAL(9,K7:K" + (i + 6).ToString() + ")/2";
                    productWorksheet.Cells[i + 7, 12].Formula = "=SUBTOTAL(9,L7:L" + (i + 6).ToString() + ")/2";
                    productWorksheet.Cells[i + 7, 13].Formula = "=SUBTOTAL(9,M7:M" + (i + 6).ToString() + ")/2";
                    


                    productWorksheet.Cells[i + 7, 5].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 6].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 10].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 11].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 12].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 13].Style.Numberformat.Format = "#,##0";
                   

                    productWorksheet.Cells[i + 7, 2].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 5].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 6].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 10].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 11].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 12].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 13].Style.Font.Bold = true;
                  


                    string columnRange = "A6:O" + (i + 7).ToString();
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