﻿using System.Collections.Generic;
using Microsoft.PowerBI.Api.V1.Models;

namespace Telematics.Models
{
    public class ReportsViewModel
    {
        public List<Report> Reports { get; set; }
    }

    public class LogsViewModel
    {
        public IList<Logs> logs_list { get; set; }
    }
}