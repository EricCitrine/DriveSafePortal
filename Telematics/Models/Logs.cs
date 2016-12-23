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

    public class Trips
    {
        public string id { get; set; }
        public string driver { get; set; }
        public Double distance { get; set; }
        public Double duration { get; set; }
        public Double hardAccelerations { get; set; }
        public Double hardStops { get; set; }
        public Double hardTurns { get; set; }
        public Double speeding = 0;
        public Double phoneUsage = 0;
        public DateTimeOffset timeStamp { get; set; }
        public DateTimeOffset createdAt { get; set; }
        public DateTimeOffset updatedAt { get; set; }
        public Boolean deleted { get; set; }
        public Double maxSpeed { get; set; }
        public Double score { get; set; }
        public string getDistance()
        {
            return (distance / 1000).ToString("#.##");
        }
        public string getDuration()
        {
            return (duration / 60).ToString("#.##");
        }
        public Double getSpeeding()
        {
            if(maxSpeed > 80)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }

    public class DriverGPS
    {
        public string driverid { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public DateTimeOffset timeStamp { get; set; }
    }

    public class Drivers_GPSData
    {
        public string id { get; set; }
        public string driverid { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public DateTimeOffset timeStamp { get; set; }

    }

    public class ScoreList
    {
        public string driver { get; set; }
        public double score { get; set; }
        public int count { get; set; }
        public double getAvgScore()
        {
            return Math.Round(score / count, 2);
        }
    }

    public class DriverDetails
    {
        public List<Trips> trips { get; set; }
        public int AvgHardAcceleration { get;set; }

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
        public string reportID { get; set; }
    }
}