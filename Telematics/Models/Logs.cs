using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.ComponentModel.DataAnnotations;

namespace Telematics.Models
{
    public class Logs
    {
        public int id { get; set; }
        public string lic_no { get; set; }
        public string lat { get; set; }
        public string lng { get; set; }
    }

    public class VehicleGPS
    {
        public VehicleStatus vVehicleStatus { get; set; }
        public OBD vOBD { get; set; }
        public Vehicle vehicle { get; set; }
        public VehicleGPS() { }
    }

    public class VehicleStatus
    {
        public int id { get; set; }
        public string vehicle_id { get; set; }
        public DateTime time_stamp { get; set; }
        public string lat { get; set; }
        public string lng { get; set; }
        public string speed { get; set; }
        public string direction { get; set; }
        public string ignition { get; set; }
        public string alarm { get; set; }
        public DateTime alarm_time { get; set; }
    }

    public class GPSLocation
    {
        public int id { get; set; }
        public string vehicle_id { get; set; }
        public string driver_id { get; set; }
        public DateTime time_stamp { get; set; }
        public string lat { get; set; }
        public string lng { get; set; }
        public GPSLocation() { }
    }

    public class Actions
    {
        public int id { get; set; }
        public string driver_id { get; set; }
        public string device_id { get; set; }
        public string type { get; set; }
        public string alarm_msg { get; set; }
        public string alarm_type { get; set; }
        public string location { get; set; }
        public DateTime time_stamp { get; set; }
        public string lat { get; set; }
        public string lng { get; set; }
    }

    public class OBD
    {
        public int id { get; set; }
        public string device_id { get; set; }
        public string company { get; set; }
        public OBD() { }
    }

    public class Companie
    {
        public string id { get; set; }
        public string name { get; set; }
        [Display(Name = "Company Code")]
        public string code { get; set; }
    }
}