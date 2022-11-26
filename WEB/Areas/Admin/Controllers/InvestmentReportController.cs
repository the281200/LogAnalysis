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

    public class InvestmentReportController : BaseController
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
            DateTime timeStart = DateTime.ParseExact(model.StartTime, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime timeEnd = DateTime.ParseExact(model.EndTime, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            int i = 1;

            var data = (from im in db.BuyAndSellBonds.Where(x => x.IsActive != false && DbFunctions.TruncateTime(x.PurchaseDate) <= DbFunctions.TruncateTime(timeEnd)
                       && DbFunctions.TruncateTime(x.PurchaseDate) >= DbFunctions.TruncateTime(timeStart) && x.CustomerId == model.CustomerId)

                        join pr in db.UserProfiles.Where(x => x.IsActive != false && x.Type == (int)TypeAccount.Customer) on im.CustomerId equals pr.UserId into group1
                        from item1 in group1

                        join st in db.IncurredPurchases.Where(x => x.IsActive != false && (x.TransactionType == (int)(TypeTransaction.GetInterestByTime) || x.TransactionType == (int)(TypeTransaction.GetInterestOntime))
                        //&& DbFunctions.TruncateTime(x.IncurredDate) <= DbFunctions.TruncateTime(timeEnd)
                        //&& DbFunctions.TruncateTime(x.IncurredDate) >= DbFunctions.TruncateTime(timeStart)
                        ) on im.Id equals st.BuyAndSellBondId into group2
                        from item2 in group2.DefaultIfEmpty()
                        select new InvestmentReport()
                        {
                            STT = 0,
                            InvestmentName = im.ContractName,
                            InvestmentType = im.TypeOfAsset != null ? im.TypeOfAsset.AssetName : "",
                            Value = im.Value,
                            Count = im.Quantily,
                            RealIncome = item2.AmountOfMoney,
                            Total = im.Value + (item2.AmountOfMoney != null ? item2.AmountOfMoney : 0)
                        }).AsNoTracking().ToList();

            data.Each(x => x.STT = i++);

            return Json(data.ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetUserInfo(int? id)
        {
            var check = false;
            var userInfo = db.UserProfiles.Where(x => x.IsActive != false && x.Type == (int)TypeAccount.Customer && x.UserId == id).FirstOrDefault();

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
            return File(fileStream, "application/ms-excel", "Báo_cáo_danh_mục_tài_sản_đầu_tư.xlsx");
        }
        public byte[] DownloadFile(List<InvestmentReport> models, ReportViewModel infoPost)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\BaoCaoDanhMucTaiSanDauTu.xlsx", HostingEnvironment.MapPath("/Uploads")));

            if (fileinfo.Exists)
            {
                var infoCustomer = db.UserProfiles.Where(x => x.IsActive != false && x.Type == (int)TypeAccount.Customer && x.UserId == infoPost.CustomerId).FirstOrDefault();

                using (var p = new ExcelPackage(fileinfo))
                {
                    var productWorksheet = p.Workbook.Worksheets[0];
                    productWorksheet.Select();
                    int i = 0;

                    productWorksheet.Cells[2, 1].Value = "Tên người dùng: " + infoCustomer.FullName;
                    productWorksheet.Cells[3, 1].Value = "SĐT: " + infoCustomer.Mobile;
                    productWorksheet.Cells[4, 1].Value = "Email: " + infoCustomer.Email;
                    productWorksheet.Cells[5, 1].Value = "Từ ngày " + infoPost.StartTime + " đến ngày " + infoPost.EndTime;

                    foreach (var item in models)
                    {
                        productWorksheet.Cells[i + 7, 1].Value = i + 1;
                        productWorksheet.Cells[i + 7, 2].Value = item.InvestmentName;
                        productWorksheet.Cells[i + 7, 3].Value = item.InvestmentType;
                        productWorksheet.Cells[i + 7, 4].Value = item.Value;
                        productWorksheet.Cells[i + 7, 5].Value = item.Count;
                        productWorksheet.Cells[i + 7, 6].Value = item.RealIncome;
                        productWorksheet.Cells[i + 7, 7].Value = item.Total;

                        i++;
                    }

                    productWorksheet.Cells[i + 7, 2].Value = "Tổng cộng:";
                    productWorksheet.Cells[i + 7, 4].Formula = "=SUBTOTAL(9,D7:D" + (i + 6).ToString() + ")";
                    productWorksheet.Cells[i + 7, 5].Formula = "=SUBTOTAL(9,E7:E" + (i + 6).ToString() + ")";
                    productWorksheet.Cells[i + 7, 6].Formula = "=SUBTOTAL(9,F7:F" + (i + 6).ToString() + ")";
                    productWorksheet.Cells[i + 7, 7].Formula = "=SUBTOTAL(9,G7:G" + (i + 6).ToString() + ")";

                    productWorksheet.Cells[i + 7, 4].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 5].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 6].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 7].Style.Numberformat.Format = "#,##0";


                    productWorksheet.Cells[i + 7, 1].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 4].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 5].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 6].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 7].Style.Font.Bold = true;

                    string columnRange = "A6:G" + (i + 7).ToString();
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
