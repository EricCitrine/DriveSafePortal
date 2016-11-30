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

namespace Telematics.Controllers
{
    public class DashboardController : Controller
    {
        private readonly string workspaceCollection;
        private readonly string workspaceId;
        private readonly string accessKey;
        private readonly string apiUrl;

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
            using (var client = this.CreatePowerBIClient())
            {
                var reportsResponse = client.Reports.GetReports(this.workspaceCollection, this.workspaceId);

                var viewModel = new ReportsViewModel
                {
                    Reports = reportsResponse.Value.ToList()
                };

                return PartialView(viewModel);
            }
        }

        public async Task<ActionResult> Report(string reportId)
        {
            using (var client = this.CreatePowerBIClient())
            {
                var reportsResponse = await client.Reports.GetReportsAsync(this.workspaceCollection, this.workspaceId);
                var report = reportsResponse.Value.FirstOrDefault(r => r.Id == reportId);
                var embedToken = PowerBIToken.CreateReportEmbedToken(this.workspaceCollection, this.workspaceId, report.Id);

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
    }
}