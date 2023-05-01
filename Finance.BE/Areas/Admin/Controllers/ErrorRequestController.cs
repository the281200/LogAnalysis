using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using WEB.Models;
using WebModels;

namespace WEB.Areas.Admin.Controllers
{
    public class ErrorRequestController : Controller
    {
        WebModels.WebContext db = new WebModels.WebContext();
        CultureInfo culture;
        public ActionResult Index()
        {
            // query logdata overview
            var logData = db.LogData.AsNoTracking().ToList();
            var totalRequest = logData.Count();
            var failedRequest = logData.Where(x => x.scStatus >= 400);//fail
            var twoHundredStatusCodes = logData.Where(x => x.scStatus >= 200 && x.scStatus < 300); //200
            var threeHundredStatusCodes = logData.Where(x => x.scStatus >= 300 && x.scStatus < 400);//300
            var fourHundredStatusCodes = logData.Where(x => x.scStatus >= 400 && x.scStatus < 500);//400
            var fiveHundredStatusCodes = logData.Where(x => x.scStatus >= 500);//500

            var badRequestStatusCodes = logData.Where(x => x.scStatus == 400); //400
            var unauthorizedStatusCodes = logData.Where(x => x.scStatus == 401); //401
            var forbidenStatusCodes = logData.Where(x => x.scStatus == 403); //403
            var notFoundStatusCodes = logData.Where(x => x.scStatus == 404); //404
            var internalServerErrorStatusCodes = logData.Where(x => x.scStatus == 500); //500
            var badGatewayStatusCodes = logData.Where(x => x.scStatus == 502); //502
            var serviceUnavailableStatusCodes = logData.Where(x => x.scStatus == 503); //503
            var gatewayTimeoutStatusCodes = logData.Where(x => x.scStatus == 504); //504

            var errorsRequestStatusCode = new ErrorsRequestStatusCode()
            {
                FailedRequest = failedRequest.Count(),
                FourHundredStatusCodes = fourHundredStatusCodes.Count(),
                FiveHundredStatusCodes = fiveHundredStatusCodes.Count(),
                BadRequestStatusCodes = badRequestStatusCodes.Count(),
                UnauthorizedStatusCodes = unauthorizedStatusCodes.Count(),
                ForbidenStatusCodes = forbidenStatusCodes.Count(),
                NotFoundStatusCodes = notFoundStatusCodes.Count(),
                InternalServerErrorStatusCodes = internalServerErrorStatusCodes.Count(),
                BadGatewayStatusCodes = badGatewayStatusCodes.Count(),
                ServiceUnavailableStatusCodes = serviceUnavailableStatusCodes.Count(),
                GatewayTimeoutStatusCodes = gatewayTimeoutStatusCodes.Count()

            };
            return View(errorsRequestStatusCode);
        }


        [HttpPost]
        public ActionResult ExportExcel(string dataString)
        {
            List<LogData> listData = new List<LogData>();
            var dataListJson = dataString.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                LogData dataObj = null;
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
                dataObj = JsonConvert.DeserializeObject<LogData>(dataObjString);
                dataObj.date = dataObj.date.ToLocalTime();
                listData.Add(dataObj);
            }
            var result = DownloadDriverPay(listData.OrderByDescending(x => x.ID).ToList());
            var fileStream = new MemoryStream(result);
            return File(fileStream, "application/ms-excel", "Phien_truy_cap_loi_nhat_ky_may_chu.xlsx");
        }
        public byte[] DownloadDriverPay(List<LogData> models)
        {

            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\Dulieunhatkymaychu.xlsx", HostingEnvironment.MapPath("/Uploads")));

            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var productWorksheet = p.Workbook.Worksheets[0];
                    productWorksheet.Select();

                    if (models.Count == 0)
                    {
                        return p.GetAsByteArray();
                    }

                    int i = 6;
                    List<String> listNumber = new List<String>();
                    var queryPlan = from a in models
                                    group a by new { a.ID };

                    /*                    var queryPlanForDriver = from a in models
                                                                 group a by new { a.EmployeeName };*/

                    /*                    foreach (var listPlan in queryPlan)
                                        {
                                            listNumber.Add(listPlan.First().NumberPlate);
                                        }*/

                    if (models.Select(x => x.date).Min().Date == models.Select(x => x.date).Max().Date)
                    {
                        var date = models.Select(x => x.date).Min();
                        var startDate = new DateTime(date.Year, date.Month, 1);
                        var endDate = startDate.AddMonths(1).AddDays(-1);
                        productWorksheet.Cells[2, 1].Value = "Từ ngày " + String.Format("{0:dd/MM/yyyy}", startDate) + " đến ngày "
                          + String.Format("{0:dd/MM/yyyy}", endDate);
                    }
                    else
                    {
                        var dateMin = models.Select(x => x.date).Min();
                        var dateMax = models.Select(x => x.date).Max();
                        productWorksheet.Cells[2, 1].Value = "Từ ngày " + String.Format("{0:dd/MM/yyyy}", dateMin) + " đến ngày "
                          + String.Format("{0:dd/MM/yyyy}", dateMax);
                    }

                    var count = 1;
                    foreach (var listItem in queryPlan)
                    {
                        int start = i;

                        foreach (var item in listItem)
                        {
                            productWorksheet.Cells[i, 1].Value = count++;
                            productWorksheet.Cells[i, 2].Value = item.date;
                            productWorksheet.Cells[i, 3].Value = item.csMethod;
                            productWorksheet.Cells[i, 4].Value = item.cIp;
                            productWorksheet.Cells[i, 5].Value = item.csVersion;
                            productWorksheet.Cells[i, 6].Value = item.scStatus;
                            productWorksheet.Cells[i, 7].Value = item.scBytes;
                            productWorksheet.Cells[i, 8].Value = item.csBytes;
                            productWorksheet.Cells[i, 9].Value = item.timeTaken;
                            i++;
                        }
                        //var count = listItem.Count();
                        //productWorksheet.Cells["A" + start.ToString() + ":A" + (i - 1).ToString()].Merge = true;
                        //productWorksheet.Cells[start, 2].Value = listItem.First().CarOwerName;

                    }

                    return p.GetAsByteArray();
                }
            }
            else
            {
                return null;
            }
        }


        [AllowAnonymous]
        public ActionResult ErrorData_Read([DataSourceRequest] DataSourceRequest request)
        {
            var data = db.LogData.AsNoTracking().Where(x=> x.scStatus >= 400).OrderByDescending(x => x.date).ToList().Select(x => new
            {
                x.ID,
                x.date,
                x.csMethod,
                x.sPort,
                x.cIp,
                x.csVersion,
                x.scStatus,
                x.scBytes,
                x.csBytes,
                x.timeTaken
            }).Take(100);

            return Json(data.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);


        }

        [AllowAnonymous]
        public JsonResult GetDataRequest()
        {
            var listQueryValue = new List<QueryBarChartModel>();
            var listColor = new List<string>
                { "#C0392B","#8E44AD", "#2980B9", "#16A085", "#27AE60" , "#F1C40F", "#F39C12", "#F39C12", "#BA4A00", "#95A5A6", "#566573", "#4FC3F7"};

            var timenow = DateTime.Now;
            var listTime = new List<DateTime>();
            var listTimeString = new List<string>();
            for (int i = 0; i > -60; i--)
            {
                var time = timenow.AddMinutes(i);
                listTime.Add(time);
                listTimeString.Add(time.ToString("HH:mm"));
            }
            listTime.Reverse();
            listTimeString.Reverse();
            var xAxes = listTimeString.ToArray();
            List<StatusCodeViewModel> listStatusCode = new List<StatusCodeViewModel>
            {
                new StatusCodeViewModel{ code = "2xx" , number = 200},
                new StatusCodeViewModel{ code = "3xx" , number = 300},
                new StatusCodeViewModel{ code = "4xx" , number = 400},
                new StatusCodeViewModel{ code = "5xx" , number = 500}
            };

            var listDataExport = new List<BarChartViewModel>();

            foreach (var item in listStatusCode)
            {
                var numberPlus = item.number + 100;
                var barChartViewModel = new BarChartViewModel();
                barChartViewModel.label = item.code;
                barChartViewModel.type = "bar";
                barChartViewModel.stack = "base";
                var listValue = new List<int>();

                foreach (var time in listTime)
                {
                    var timedate = time.Date;
                    var timeOfDay = time.TimeOfDay;
                    var timePlus = time.AddMinutes(1);
                    var timePlusOfDay = time.AddMinutes(1).TimeOfDay;
                    var test = db.LogData.Where(x => DbFunctions.TruncateTime(x.date) == timedate
                    && SqlFunctions.DatePart("hour", x.date) == time.Hour
                    && SqlFunctions.DatePart("minute", x.date) == time.Minute
                    && x.scStatus >= item.number
                    && x.scStatus < numberPlus
                    ).ToList();
                    //var value = db.LogData.Where(x => DbFunctions.TruncateTime(x.date) >= timedate && DbFunctions.DiffMinutes(x.) &&x.date.TimeOfDay >= timeOfDay && x.date.TimeOfDay < timeOfDayPlus && x.scStatus >= item.number && x.scStatus < numberPlus).ToList();
                    listValue.Add(test.Count());
                }



                barChartViewModel.data = listValue;
                listDataExport.Add(barChartViewModel);
            }
            for (var i = 0; i < listDataExport.Count(); i++)
            {
                listDataExport[i].backgroundColor = listColor[i];
            }
            var stringJsonYAxes = listDataExport.ToArray();
            return Json(new { xAxes = xAxes, yAxes = stringJsonYAxes }, JsonRequestBehavior.AllowGet);

        }


        //TopPath chart Request 
        [AllowAnonymous]
        public JsonResult GetDataTopPath()
        {
            var listColor = new List<string>
                { "#C0392B","#8E44AD", "#2980B9", "#16A085", "#27AE60" , "#F1C40F", "#F39C12", "#F39C12", "#BA4A00", "#95A5A6", "#566573", "#4FC3F7"};
            var now = DateTime.Now.Date;
            var listPath = (from log in db.LogData
                            where log.csReferer != null
                            group log by log.csReferer into refererGroup
                            orderby refererGroup.Count() descending
                            select new { Referer = refererGroup.Key, Count = refererGroup.Count() })
             .Take(5);
            var barChartViewModel = new VerticalBarChartViewModel();
            barChartViewModel.axis = "y";
            barChartViewModel.fill = "false";
            barChartViewModel.borderWidth = "1";
            var listxAxes = new List<string>();
            var listdata = new List<int>();
            var listbackground = new List<string>();
            foreach (var item in listPath)
            {
                listxAxes.Add(item.Referer.Replace("https://phantichnhatky.xyz", "").Replace("https://www.phantichnhatky.xyz", ""));
                listdata.Add(item.Count);
            }
            for (var i = 0; i < listdata.Count(); i++)
            {
                listbackground.Add(listColor[i]);
            }
            barChartViewModel.data = listdata.ToArray();
            barChartViewModel.backgroundColor = listbackground.ToArray();

            var xAxesArr = listxAxes.ToArray();
            var yAxesList = new List<VerticalBarChartViewModel>();
            yAxesList.Add(barChartViewModel);
            var yAxesArr = yAxesList.ToArray();
            return Json(new { xAxes = xAxesArr, yAxes = yAxesArr }, JsonRequestBehavior.AllowGet);

        }

        //ClientIP chart  
        [AllowAnonymous]
        public JsonResult GetDataClientIP()
        {
            var listColor = new List<string>
                { "#C0392B","#8E44AD", "#2980B9", "#16A085", "#27AE60" , "#F1C40F", "#F39C12", "#F39C12", "#BA4A00", "#95A5A6", "#566573", "#4FC3F7"};
            var now = DateTime.Now.Date;
            var listPath = (from log in db.LogData
                            where log.cIp != null
                            group log by log.cIp into refererGroup
                            orderby refererGroup.Count() descending
                            select new { Referer = refererGroup.Key, Count = refererGroup.Count() })
             .Take(5);
            var barChartViewModel = new VerticalBarChartViewModel();
            barChartViewModel.axis = "y";
            barChartViewModel.fill = "false";
            barChartViewModel.borderWidth = "1";
            var listxAxes = new List<string>();
            var listdata = new List<int>();
            var listbackground = new List<string>();
            foreach (var item in listPath)
            {
                listxAxes.Add(item.Referer);
                listdata.Add(item.Count);
            }
            for (var i = 0; i < listdata.Count(); i++)
            {
                listbackground.Add(listColor[i]);
            }
            barChartViewModel.data = listdata.ToArray();
            barChartViewModel.backgroundColor = listbackground.ToArray();

            var xAxesArr = listxAxes.ToArray();
            var yAxesList = new List<VerticalBarChartViewModel>();
            yAxesList.Add(barChartViewModel);
            var yAxesArr = yAxesList.ToArray();
            return Json(new { xAxes = xAxesArr, yAxes = yAxesArr }, JsonRequestBehavior.AllowGet);

        }

        //ClientIP chart  
        [AllowAnonymous]
        public JsonResult GetDataProtocolVersion()
        {
            var listColor = new List<string>
                { "#C0392B","#8E44AD", "#2980B9", "#16A085", "#27AE60" , "#F1C40F", "#F39C12", "#F39C12", "#BA4A00", "#95A5A6", "#566573", "#4FC3F7"};
            var now = DateTime.Now.Date;
            var listPath = (from log in db.LogData
                            where log.csVersion != null
                            group log by log.csVersion into refererGroup
                            orderby refererGroup.Count() descending
                            select new { Referer = refererGroup.Key, Count = refererGroup.Count() })
             .Take(3);
            var barChartViewModel = new VerticalBarChartViewModel();
            barChartViewModel.axis = "y";
            barChartViewModel.fill = "false";
            barChartViewModel.borderWidth = "1";
            var listxAxes = new List<string>();
            var listdata = new List<int>();
            var listbackground = new List<string>();
            foreach (var item in listPath)
            {
                listxAxes.Add(item.Referer);
                listdata.Add(item.Count);
            }
            for (var i = 0; i < listdata.Count(); i++)
            {
                listbackground.Add(listColor[i]);
            }
            barChartViewModel.data = listdata.ToArray();
            barChartViewModel.backgroundColor = listbackground.ToArray();

            var xAxesArr = listxAxes.ToArray();
            var yAxesList = new List<VerticalBarChartViewModel>();
            yAxesList.Add(barChartViewModel);
            var yAxesArr = yAxesList.ToArray();
            return Json(new { xAxes = xAxesArr, yAxes = yAxesArr }, JsonRequestBehavior.AllowGet);

        }

        //ClientIP chart  
        [AllowAnonymous]
        public JsonResult GetDataHttpMethod()
        {
            var listColor = new List<string>
                { "#C0392B","#8E44AD", "#2980B9", "#16A085", "#27AE60" , "#F1C40F", "#F39C12", "#F39C12", "#BA4A00", "#95A5A6", "#566573", "#4FC3F7"};
            var now = DateTime.Now.Date;
            var listPath = (from log in db.LogData
                            where log.csMethod != null
                            group log by log.csMethod into refererGroup
                            orderby refererGroup.Count() descending
                            select new { Referer = refererGroup.Key, Count = refererGroup.Count() })
             .Take(5);
            var barChartViewModel = new VerticalBarChartViewModel();
            barChartViewModel.axis = "y";
            barChartViewModel.fill = "false";
            barChartViewModel.borderWidth = "1";
            var listxAxes = new List<string>();
            var listdata = new List<int>();
            var listbackground = new List<string>();
            foreach (var item in listPath)
            {
                listxAxes.Add(item.Referer);
                listdata.Add(item.Count);
            }
            for (var i = 0; i < listdata.Count(); i++)
            {
                listbackground.Add(listColor[i]);
            }
            barChartViewModel.data = listdata.ToArray();
            barChartViewModel.backgroundColor = listbackground.ToArray();

            var xAxesArr = listxAxes.ToArray();
            var yAxesList = new List<VerticalBarChartViewModel>();
            yAxesList.Add(barChartViewModel);
            var yAxesArr = yAxesList.ToArray();
            return Json(new { xAxes = xAxesArr, yAxes = yAxesArr }, JsonRequestBehavior.AllowGet);

        }
    }
}