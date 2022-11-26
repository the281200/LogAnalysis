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
    public class ReportTypeOfAssetsController : Controller
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

            var data = (from im in db.TypeOfAssets.Where(x => x.IsActive != false)

                        join pr in db.BuyAndSellBonds.Where(x => x.IsActive != false && x.CustomerId == user) on im.ID equals pr.AssetTypeId into group1
                        from item1 in group1

                        join st in db.IncurredPurchases.Where(x => x.IsActive != false && (x.TransactionType == (int)(TypeTransaction.GetInterestByTime) || x.TransactionType == (int)(TypeTransaction.GetInterestOntime))

                        ) on item1.Id equals st.BuyAndSellBondId into group2
                        from item2 in group2.DefaultIfEmpty()

                        join nk in db.InterestPaymentPeriods.Where(x => x.IsActive != false) on item1.Id equals nk.BuyAndSellBondId into group3
                        from item3 in group3.DefaultIfEmpty()

                            /* join th in db.TypeOfAssets.Where(x => x.IsActive != false) on im.AssetTypeId equals th.ID into group4
                             from item4 in group4.DefaultIfEmpty()*/
                        select new InvestmentReport()
                        {
                            STT = 0,
                            AssetId = im.AssetId,
                            AssetName = im.AssetName,
                            NumOfContract = group1.Where(item1 => item1.AssetTypeId == im.ID).Count(),
                            SumOfContract = group1.Where(item1 => item1.AssetTypeId == im.ID).Select(x => x.Value).Sum(),
                            RoundedPurchaseValue = group1.Where(item1 => item1.AssetTypeId == im.ID).Select(x => x.RoundedPurchaseValue).Sum(),
                            SumOfInterest = group3.Where(nk => nk.BuyAndSellBondId == item1.Id).Select(x => x.AccruedInterest).Sum(),
                            RealIncome = item2.AmountOfMoney,
                            Sum = group1.Where(item1 => item1.AssetTypeId == im.ID).Select(x => x.Value).Sum() + item2.AmountOfMoney

                        }).AsNoTracking().ToList().GroupBy(x => x.AssetId).Select(x => x.First()).ToList();

            data.Each(x => x.STT = i++);

            return Json(data.ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult ExportExcel()
        {
            

            var user = WebSecurity.GetUserId(User.Identity.Name);
            int i = 1;

            var listData = (from im in db.TypeOfAssets.Where(x => x.IsActive != false)

                            join pr in db.BuyAndSellBonds.Where(x => x.IsActive != false && x.CustomerId == user) on im.ID equals pr.AssetTypeId into group1
                            from item1 in group1

                            join st in db.IncurredPurchases.Where(x => x.IsActive != false && (x.TransactionType == (int)(TypeTransaction.GetInterestByTime) || x.TransactionType == (int)(TypeTransaction.GetInterestOntime))

                            ) on item1.Id equals st.BuyAndSellBondId into group2
                            from item2 in group2.DefaultIfEmpty()

                            join nk in db.InterestPaymentPeriods.Where(x => x.IsActive != false) on item1.Id equals nk.BuyAndSellBondId into group3
                            from item3 in group3.DefaultIfEmpty()

                                /* join th in db.TypeOfAssets.Where(x => x.IsActive != false) on im.AssetTypeId equals th.ID into group4
                                 from item4 in group4.DefaultIfEmpty()*/
                            select new InvestmentReport()
                            {
                                STT = 0,
                                AssetId = im.AssetId,
                                AssetName = im.AssetName,
                                NumOfContract = group1.Where(item1 => item1.AssetTypeId == im.ID).Count(),
                                SumOfContract = group1.Where(item1 => item1.AssetTypeId == im.ID).Select(x => x.Value).Sum(),
                                RoundedPurchaseValue = group1.Where(item1 => item1.AssetTypeId == im.ID).Select(x => x.RoundedPurchaseValue).Sum(),
                                SumOfInterest = group3.Where(nk => nk.BuyAndSellBondId == item1.Id).Select(x => x.AccruedInterest).Sum(),
                                RealIncome = item2.AmountOfMoney,
                                Sum = group1.Where(item1 => item1.AssetTypeId == im.ID).Select(x => x.Value).Sum() + item2.AmountOfMoney

                            }).AsNoTracking().ToList().GroupBy(x => x.AssetId).Select(x => x.First()).ToList();

            listData.Each(x => x.STT = i++);
            var result = DownloadFile(listData);
            return Json(new
            {
                fileStream = Convert.ToBase64String(result),
                fileName = "Báo_cáo_tổng_hợp_tình_hình_nhóm_tài_sản_loại_hợp_đồng.xlsx"
            }, JsonRequestBehavior.AllowGet);
        }
        public byte[] DownloadFile(List<InvestmentReport> models)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\BaoCaoTongHopTinhHinhNhomTaiSanLoaiHopDong.xlsx", HostingEnvironment.MapPath("/Uploads")));

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
                        productWorksheet.Cells[i + 7, 1].Value = i + 1;
                        productWorksheet.Cells[i + 7, 2].Value = item.AssetId;
                        productWorksheet.Cells[i + 7, 3].Value = item.AssetName;
                        productWorksheet.Cells[i + 7, 4].Value = item.NumOfContract;
                        productWorksheet.Cells[i + 7, 5].Value = item.SumOfContract;
                        productWorksheet.Cells[i + 7, 6].Value = item.RoundedPurchaseValue;
                        productWorksheet.Cells[i + 7, 7].Value = item.SumOfInterest;
                        productWorksheet.Cells[i + 7, 8].Value = item.RealIncome;
                        productWorksheet.Cells[i + 7, 9].Value = item.Sum;

                        productWorksheet.Cells[i + 7, 4].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 5].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 6].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 7].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 8].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 9].Style.Numberformat.Format = "#,##0";

                        i++;
                    }

                    productWorksheet.Cells[i + 7, 2].Value = "Tổng cộng:";
                    productWorksheet.Cells[i + 7, 4].Formula = "=SUBTOTAL(9,D7:D" + (i + 6).ToString() + ")";
                    productWorksheet.Cells[i + 7, 5].Formula = "=SUBTOTAL(9,E7:E" + (i + 6).ToString() + ")";
                    productWorksheet.Cells[i + 7, 6].Formula = "=SUBTOTAL(9,F7:F" + (i + 6).ToString() + ")";
                    productWorksheet.Cells[i + 7, 7].Formula = "=SUBTOTAL(9,G7:G" + (i + 6).ToString() + ")";
                    productWorksheet.Cells[i + 7, 8].Formula = "=SUBTOTAL(9,H7:H" + (i + 6).ToString() + ")";
                    productWorksheet.Cells[i + 7, 9].Formula = "=SUBTOTAL(9,I7:I" + (i + 6).ToString() + ")";


                    productWorksheet.Cells[i + 7, 4].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 5].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 6].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 7].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 8].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 9].Style.Numberformat.Format = "#,##0";





                    productWorksheet.Cells[i + 7, 1].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 4].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 5].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 6].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 7].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 8].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 9].Style.Font.Bold = true;

                    string columnRange = "A6:I" + (i + 7).ToString();
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