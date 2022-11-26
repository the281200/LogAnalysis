using Kendo.Mvc.UI;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebModels;
using System.Data;
using WEB.Models;
using System.Globalization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web.Hosting;
using System.ComponentModel;
using OfficeOpenXml;
using System.IO;
using OfficeOpenXml.Style;

namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public class ReportTypeOfAssetsController : BaseController
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
        public ActionResult InvestmentReportRead([DataSourceRequest] DataSourceRequest request, ImportModel model)
        {

            int i = 1;

             if(model.CustomerId == 0)
            {
                var data = (from im in db.TypeOfAssets.Where(x => x.IsActive != false)

                            join pr in db.BuyAndSellBonds.Where(x => x.IsActive != false ) on im.ID equals pr.AssetTypeId into group1
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
            else
            {
                var data = (from im in db.TypeOfAssets.Where(x => x.IsActive != false)

                            join pr in db.BuyAndSellBonds.Where(x => x.IsActive != false && x.CustomerId == model.CustomerId) on im.ID equals pr.AssetTypeId into group1
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
           
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetUserInfo(int? id)
        {
            var check = false;
            var userInfo = db.UserProfiles.Where(x => x.IsActive != false && x.Type != (int)TypeAccount.Admin && x.UserId == id).FirstOrDefault();

            if (userInfo != null) check = true;
            else
            {
                userInfo = new UserProfile()
                {
                    FullName = "",
                    Mobile = "",
                    Email = ""
                };
            }

            return Json(new { ErrorMessage = string.Empty, success = check, name = "Tên khách hàng: " + userInfo.FullName, phone = "Số điện thoại: " + userInfo.Mobile, email = "Email: " + userInfo.Email }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ExportExcel(ReportViewModel model)
        {
            List<InvestmentReport> listData = new List<InvestmentReport>();
            var dataListJson = model.DataExport.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                InvestmentReport dataObj = null;
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
                dataObj = JsonConvert.DeserializeObject<InvestmentReport>(dataObjString);
                listData.Add(dataObj);
            }
            var result = DownloadFile(listData, model);
            var fileStream = new MemoryStream(result);
            return File(fileStream, "application/ms-excel", "Báo_cáo_tổng_hợp_tình_hình_nhóm_tài_sản_loại_hợp_đồng.xlsx");
        }
        public byte[] DownloadFile(List<InvestmentReport> models, ReportViewModel infoPost)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\BaoCaoTongHopTinhHinhNhomTaiSanLoaiHopDong.xlsx", HostingEnvironment.MapPath("/Uploads")));

            if (fileinfo.Exists)
            {
                

                using (var p = new ExcelPackage(fileinfo))
                {
                    var productWorksheet = p.Workbook.Worksheets[0];
                    productWorksheet.Select();
                    int i = 0;

                    if (infoPost.CustomerId != 0)
                    {
                        var infoCustomer = db.UserProfiles.Where(x => x.IsActive != false && x.Type != (int)TypeAccount.Admin && x.UserId == infoPost.CustomerId).FirstOrDefault();
                        productWorksheet.Cells[2, 1].Value = "Tên người dùng: " + infoCustomer.FullName;
                        productWorksheet.Cells[3, 1].Value = "SĐT: " + infoCustomer.Mobile;
                        productWorksheet.Cells[4, 1].Value = "Email: " + infoCustomer.Email;
                    }
                    


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
