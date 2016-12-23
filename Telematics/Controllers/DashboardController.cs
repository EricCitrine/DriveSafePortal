using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Services;
using Microsoft.PowerBI.Api.V1;
using Microsoft.PowerBI.Security;
using Microsoft.Rest;
using Telematics.Models;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using System.Net.Mail;
using System.Net;

namespace Telematics.Controllers
{
    public class DashboardController : Controller
    {
        private readonly string workspaceCollection;
        private readonly string workspaceId;
        private readonly string accessKey;
        private readonly string apiUrl;
        private string currentworkspaceid;

        public DashboardController()
        {
            this.workspaceCollection = ConfigurationManager.AppSettings["powerbi:WorkspaceCollection"];
            this.workspaceId = ConfigurationManager.AppSettings["powerbi:WorkspaceId"];
            this.accessKey = ConfigurationManager.AppSettings["powerbi:AccessKey"];
            this.apiUrl = ConfigurationManager.AppSettings["powerbi:ApiUrl"];
        }

        [WebMethod]
        public JsonResult GetAllItems()
        {
            var gps_update_query = new List<Logs>();
            using (var db = new ApplicationDbContext())
            {
                gps_update_query = (from g in db.Logs
                                    select g).ToList();
            }
            return Json(new { Data = gps_update_query }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Scoring()
        {
            var trip_list = new List<ScoreList>();
            var grade_a = 0;
            var grade_b = 0;
            var grade_c = 0;
            using (var db = new ApplicationDbContext())
            {
                var current_user = (from u in db.Users
                                    where u.UserName == User.Identity.Name
                                    select u).FirstOrDefault();

                trip_list = (from t in db.Trips
                             group t by new { t.driver } into new_t
                             select new ScoreList
                             {
                                 driver = new_t.Key.driver,
                                 score = new_t.Sum(t => t.score),
                                 count = new_t.Count()
                             }).ToList();
                foreach(ScoreList sl in trip_list)
                {
                    if(sl.score > 75)
                    {
                        grade_a++;
                    }
                    else if(sl.score > 50)
                    {
                        grade_b++;
                    }
                    else
                    {
                        grade_c++;
                    }
                }
            }
            var model = new ScoreViewModel()
            {
                scores = trip_list,
                gradeA = grade_a,
                gradeB = grade_b,
                gradeC = grade_c
            };

            return View(model);
        }

        public async Task<ActionResult> Details(string driver)
        {
            var driver_details = new List<Trips>();
            using (var db = new ApplicationDbContext())
            {
                driver_details = (from t in db.Trips
                                  orderby t.timeStamp descending
                                  where t.driver == driver
                                  select t).ToList();
            }
            var model = new DetailsViewModel()
            {
                trips = driver_details,
                driver = driver
            };
            return View(model);
        }

        public async Task<ActionResult> Track()
        {
            var driverLatestLocation = new List<Drivers_GPSData>();
            using (var db = new ApplicationDbContext())
            {
                var unique_driverids = (from d in db.Drivers_GPSData
                                        select d.driverid).Distinct().ToList();

                driverLatestLocation = (from d in unique_driverids
                                            from g in db.Drivers_GPSData.Where(dg => dg.driverid == d)
                                                                        .OrderByDescending(dg => dg.timeStamp)
                                                                        .Take(1)
                                                                        .DefaultIfEmpty()
                                            select g).ToList();
                

            };
            var model = new TrackViewModel()
            {
                drivergps = driverLatestLocation.ToList()
            };
            return View(model);
        }

        public async Task<ActionResult> Index()
        {
            var vehicle_gps_list = new List<VehicleGPS>();
            using (var db = new ApplicationDbContext())
            {
                var current_user = (from u in db.Users
                                where u.UserName == User.Identity.Name
                                select u).FirstOrDefault();                

                vehicle_gps_list = (from g in db.VehicleStatus
                                    join o in db.OBD on g.vehicle_id equals o.device_id
                                    join v in db.Vehicle on g.vehicle_id equals v.device_no
                                    where o.company == current_user.Company
                                    select new VehicleGPS { vVehicleStatus = g, vOBD = o, vehicle = v }).ToList();
            }
            var model = new GPSViewModel()
            {
                gps_location_list = vehicle_gps_list.ToList()
            };

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult Reports()
        {
            using (var db = new ApplicationDbContext())
            {
                var current_company = (from c in db.Companie
                                       join u in db.Users on c.name equals u.Company
                                       where u.UserName == User.Identity.Name
                                       select c).FirstOrDefault();
                currentworkspaceid = current_company.reportID;
            }
            using (var client = this.CreatePowerBIClient())
            {
                var reportsResponse = client.Reports.GetReports(this.workspaceCollection, currentworkspaceid);

                var viewModel = new ReportsViewModel
                {
                    Reports = reportsResponse.Value.ToList()
                };

                return PartialView(viewModel);
            }
        }

        public async Task<ActionResult> Report(string reportId)
        {
            using (var db = new ApplicationDbContext())
            {
                var current_company = (from c in db.Companie
                                       join u in db.Users on c.name equals u.Company
                                       where u.UserName == User.Identity.Name
                                       select c).FirstOrDefault();
                currentworkspaceid = current_company.reportID;
            }
            using (var client = this.CreatePowerBIClient())
            {
                var reportsResponse = await client.Reports.GetReportsAsync(this.workspaceCollection, currentworkspaceid);
                var report = reportsResponse.Value.FirstOrDefault(r => r.Id == reportId);
                var embedToken = PowerBIToken.CreateReportEmbedToken(this.workspaceCollection, currentworkspaceid, report.Id);

                var viewModel = new ReportViewModel
                {
                    Report = report,
                    AccessToken = embedToken.Generate(this.accessKey)
                };

                return View(viewModel);
            }
        }

        private IPowerBIClient CreatePowerBIClient()
        {
            var credentials = new TokenCredentials(accessKey, "AppKey");
            var client = new PowerBIClient(credentials)
            {
                BaseUri = new Uri(apiUrl)
            };

            return client;
        }

        [WebMethod]
        public JsonResult GetGPSUpdates()
        {


            //try
            //{
            //    //SmtpClient client = new SmtpClient();
            //    //client.Port = 587;
            //    //client.DeliveryMethod = SmtpDeliveryMethod.Network;
            //    //client.UseDefaultCredentials = false;
            //    //client.Host = "smtp.gmail.com";
            //    //client.EnableSsl = true;
            //    //client.UseDefaultCredentials = false;
            //    //client.Credentials = new System.Net.NetworkCredential("itadmin@digital-safety.sg", "Atarash13");

            //    //MailMessage mail = new MailMessage("itadmin@digital-safety.sg", "andrewwongcr@hotmail.com");
            //    //mail.Subject = "This is test mail";
            //    //mail.Body = "this is test mail body";

            //    //client.Send(mail);

            //    string username = "atarashib@gmail.com"; // "ali@digital-safety.sg";
            //    string password = "Atarash13";
            //    string smtpserver = "smtp.gmail.com";

            //    SmtpClient smtpClient = new SmtpClient();
            //    NetworkCredential basicCredential = new NetworkCredential(username, password);
            //    MailMessage message = new MailMessage();
            //    MailAddress fromAddress = new MailAddress(username);

            //    // setup up the host, increase the timeout to 5 minutes
            //    smtpClient.Host = smtpserver;
            //    smtpClient.UseDefaultCredentials = false;
            //    smtpClient.Credentials = basicCredential;
            //    smtpClient.Timeout = (60 * 5 * 1000);

            //    message.From = fromAddress;
            //    message.Subject = "Email from portal";
            //    message.IsBodyHtml = false;
            //    message.Body = "Test email from portal";
            //    message.To.Add("andrewwongcr@hotmail.com");
            //    smtpClient.Send(message);

            //    Console.WriteLine("Sent mail");

            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Cant send mail : " + ex);
            //}
            //var today = DateTime.Now;
            //var yesterday = today.AddDays(-1);
            //var gps_update_query = new List<GPSLocation>();
            var vehicle_gps_list = new List<VehicleGPS>();
            using (var db = new ApplicationDbContext())
            {
                var current_user = (from u in db.Users
                                    where u.UserName == User.Identity.Name
                                    select u).FirstOrDefault();
                if (current_user == null)
                {
                    RedirectToAction("Login", "Account");
                }
                //gps_update_query = (from g in db.GPSLocation
                //                    join o in db.OBD on g.vehicle_id equals o.device_id
                //                    where g.time_stamp > yesterday
                //                  && o.company == current_user.Company
                //                    select g).ToList();
                try
                {
                    vehicle_gps_list = (from g in db.VehicleStatus
                                        join o in db.OBD on g.vehicle_id equals o.device_id
                                        join v in db.Vehicle on g.vehicle_id equals v.device_no
                                        where o.company == current_user.Company
                                        && g.lat != "0"
                                        select new VehicleGPS { vVehicleStatus = g, vOBD = o, vehicle = v }).ToList();// { vGPSLocation = g, vOBD = o })).ToList();

                    var model = new GPSViewModel()
                    {
                        gps_location_list = vehicle_gps_list.ToList()
                    };
                }
                catch (Exception e)
                {

                }
            }
            return Json(new { Data = vehicle_gps_list }, JsonRequestBehavior.AllowGet);
        }

        [WebMethod]
        public JsonResult GetLatestDriverGPS()
        {
            //var today = DateTime.Now;
            //var yesterday = today.AddDays(-1);
            //var gps_update_query = new List<GPSLocation>();
            var vehicle_gps_list = new List<VehicleGPS>();
            var driverLatestLocation = new List<Drivers_GPSData>();
            using (var db = new ApplicationDbContext())
            {
                try
                {
                    //vehicle_gps_list = (from g in db.VehicleStatus
                    //                    join o in db.OBD on g.vehicle_id equals o.device_id
                    //                    join v in db.Vehicle on g.vehicle_id equals v.device_no
                    //                    where o.company == current_user.Company
                    //                    && g.lat != "0"
                    //                    select new VehicleGPS { vVehicleStatus = g, vOBD = o, vehicle = v }).ToList();// { vGPSLocation = g, vOBD = o })).ToList();

                    //var model = new GPSViewModel()
                    //{
                    //    gps_location_list = vehicle_gps_list.ToList()
                    //};

                    var unique_driverids = (from d in db.Drivers_GPSData
                                            select d.driverid).Distinct().ToList();

                    driverLatestLocation = (from d in unique_driverids
                                            from g in db.Drivers_GPSData.Where(dg => dg.driverid == d)
                                                                        .OrderByDescending(dg => dg.timeStamp)
                                                                        .Take(1)
                                                                        .DefaultIfEmpty()
                                            select g).ToList();
                }
                catch (Exception e)
                {

                }
            }
            return Json(new { Data = driverLatestLocation }, JsonRequestBehavior.AllowGet);
        }
    }
}