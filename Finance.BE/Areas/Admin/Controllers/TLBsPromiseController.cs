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

    public class TLBsPromiseController : BaseController
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
        public JsonResult Get_Customers()
        {
            var users = from x in db.UserProfiles.AsNoTracking() where (x.IsActive != false && x.Type != (int)TypeAccount.Admin) select x;
            var result = users.Select(x => new
            {
                Id = x.UserId,
                Name = x.Mobile + " - " + x.FullName
            });
            var addAll = result.ToList().Concat(new[] { new { Id = 0, Name = "Tất cả khách hàng" } });
            
            
            return Json(addAll, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult InvestmentReportRead([DataSourceRequest] DataSourceRequest request, ImportModel model)
        {
            
            int i = 1;
            if(model.CustomerId == 0)
            {
                var data = (from im in db.BuyAndSellBonds.Where(x => x.IsActive != false)

                               
                            select new InvestmentReport()
                            {
                                STT = 0,
                                InvestmentCode = im.ContractCode,
                                BondsCode = im.AssetCategorys.AssetCode,
                                BondsName = im.AssetCategorys.Name,
                                Count = im.Quantily,
                                Value = im.Value,
                                RoundedPurchaseValue = im.RoundedPurchaseValue,
                                InvestmentPurchaseDate = im.PurchaseDate,
                                InvestmentPeriodDate = im.PeriodDate,
                                InvestmentInterestRate = im.InterestRate,
                                InputInterestRate = im.InputInterestRate,
                                InterestRateInOut = im.InterestRateInOut,
                                WealthManageBenefits = im.WealthManageBenefits,
                                Sum = im.WealthManageBenefits + im.InterestRateInOut

                            }).AsNoTracking().ToList();
                data.Each(x => x.STT = i++);

                return Json(data.ToList(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                var data = (from im in db.BuyAndSellBonds.Where(x => x.IsActive != false && x.CustomerId == model.CustomerId)

                                
                            select new InvestmentReport()
                            {
                                STT = 0,
                                InvestmentCode = im.ContractCode,
                                BondsCode = im.AssetCategorys.AssetCode,
                                BondsName = im.AssetCategorys.Name,
                                Count = im.Quantily,
                                Value = im.Value,
                                RoundedPurchaseValue = im.RoundedPurchaseValue,
                                InvestmentPurchaseDate = im.PurchaseDate,
                                InvestmentPeriodDate = im.PeriodDate,
                                InvestmentInterestRate = im.InterestRate,
                                InputInterestRate = im.InputInterestRate,
                                InterestRateInOut = im.InterestRateInOut,
                                WealthManageBenefits = im.WealthManageBenefits,
                                Sum = im.WealthManageBenefits + im.InterestRateInOut

                            }).AsNoTracking().ToList();
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
            return File(fileStream, "application/ms-excel", "Báo_cáo_doanh_thu_TLBs.xlsx");
        }
        public byte[] DownloadFile(List<InvestmentReport> models, ReportViewModel infoPost)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\BaoCaoDoanhThuTLBs.xlsx", HostingEnvironment.MapPath("/Uploads")));

            if (fileinfo.Exists)
            {   
               
               

                using (var p = new ExcelPackage(fileinfo))
                {
                    var productWorksheet = p.Workbook.Worksheets[0];
                    productWorksheet.Select();
                    int i = 0;

                    if(infoPost.CustomerId != 0)
                    {
                        var infoCustomer = db.UserProfiles.Where(x => x.IsActive != false && x.Type != (int)TypeAccount.Admin && x.UserId == infoPost.CustomerId).FirstOrDefault();
                        productWorksheet.Cells[2, 1].Value = "Tên người dùng: " + infoCustomer.FullName;
                        productWorksheet.Cells[3, 1].Value = "SĐT: " + infoCustomer.Mobile;
                        productWorksheet.Cells[4, 1].Value = "Email: " + infoCustomer.Email;
                    }
                   
                    

                    foreach (var item in models)
                    {
                        productWorksheet.Cells[i + 7, 1].Value = i + 1;
                        productWorksheet.Cells[i + 7, 2].Value = item.InvestmentCode;
                        productWorksheet.Cells[i + 7, 3].Value = item.BondsCode;
                        productWorksheet.Cells[i + 7, 4].Value = item.BondsName;
                        productWorksheet.Cells[i + 7, 5].Value = item.Count;
                        productWorksheet.Cells[i + 7, 6].Value = item.Value;
                        productWorksheet.Cells[i + 7, 7].Value = item.RoundedPurchaseValue;
                        productWorksheet.Cells[i + 7, 8].Value = item.InvestmentPurchaseDate.Value.AddDays(1) != null ? (item.InvestmentPurchaseDate ?? DateTime.Now).AddDays(1).ToString("dd/MM/yyyy") : "";
                        productWorksheet.Cells[i + 7, 9].Value = item.InvestmentPeriodDate.Value.AddDays(1) != null ? (item.InvestmentPeriodDate ?? DateTime.Now).AddDays(1).ToString("dd/MM/yyyy") : "";
                        productWorksheet.Cells[i + 7, 10].Value = item.InvestmentInterestRate;
                        productWorksheet.Cells[i + 7, 11].Value = item.InputInterestRate;
                        productWorksheet.Cells[i + 7, 12].Value = item.InterestRateInOut;
                        productWorksheet.Cells[i + 7, 13].Value = item.WealthManageBenefits;
                        productWorksheet.Cells[i + 7, 14].Value = item.Sum;


                        productWorksheet.Cells[i + 7, 5].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 6].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 7].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 10].Style.Numberformat.Format = "#,##0.00";
                        productWorksheet.Cells[i + 7, 11].Style.Numberformat.Format = "#,##0.00";
                        productWorksheet.Cells[i + 7, 12].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 13].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i + 7, 14].Style.Numberformat.Format = "#,##0";

                        i++;
                    }

                    productWorksheet.Cells[i + 7, 2].Value = "Tổng cộng:";
                    productWorksheet.Cells[i + 7, 5].Formula = "=SUBTOTAL(9,E7:E" + (i + 6).ToString() + ")";
                    productWorksheet.Cells[i + 7, 6].Formula = "=SUBTOTAL(9,F7:F" + (i + 6).ToString() + ")";
                    productWorksheet.Cells[i + 7, 7].Formula = "=SUBTOTAL(9,G7:G" + (i + 6).ToString() + ")";
                   
                    productWorksheet.Cells[i + 7, 12].Formula = "=SUBTOTAL(9,L7:L" + (i + 6).ToString() + ")";
                    productWorksheet.Cells[i + 7, 13].Formula = "=SUBTOTAL(9,M7:M" + (i + 6).ToString() + ")";
                    productWorksheet.Cells[i + 7, 14].Formula = "=SUBTOTAL(9,N7:N" + (i + 6).ToString() + ")";



                    productWorksheet.Cells[i + 7, 5].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 6].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 7].Style.Numberformat.Format = "#,##0";
                   
                    productWorksheet.Cells[i + 7, 12].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 13].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i + 7, 14].Style.Numberformat.Format = "#,##0";



                    productWorksheet.Cells[i + 7, 1].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 5].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 6].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 7].Style.Font.Bold = true;
                   
                    productWorksheet.Cells[i + 7, 12].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 13].Style.Font.Bold = true;
                    productWorksheet.Cells[i + 7, 14].Style.Font.Bold = true;

                    string columnRange = "A6:N" + (i + 7).ToString();
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
