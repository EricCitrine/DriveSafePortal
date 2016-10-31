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

        public string GetGPSUpdate()
        {
            return "test";
        }

        public ActionResult Index()
        {
            var gps_location_list_query = new List<GPSLocation>();
            using (var db = new ApplicationDbContext())
            {
                gps_location_list_query = (from g in db.GPSLocation
                                        select g).ToList();
            }
            var model = new GPSViewModel()
            {
                gps_location_list = gps_location_list_query.ToList()
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
            var gps_update_query = new List<GPSLocation>();
            using (var db = new ApplicationDbContext())
            {
                gps_update_query = (from g in db.GPSLocation
                                    select g).ToList();
            }
            gps_update_query.Add(new GPSLocation() { id = 1, driver_id = "123", vehicle_id = "222", lat = "-123", lng = "123", time_stamp = new DateTime() });
            gps_update_query.Add(new GPSLocation() { id = 2, driver_id = "321", vehicle_id = "333", lat = "-123", lng = "123", time_stamp = new DateTime() });
            return Json(new { Data = gps_update_query }, JsonRequestBehavior.AllowGet);
        }
    }
}