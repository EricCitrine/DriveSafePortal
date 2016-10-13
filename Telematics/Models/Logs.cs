using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Telematics.Models
{
    public class Logs
    {
        public int id { get; set; }
        public string lic_no { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
    }
}