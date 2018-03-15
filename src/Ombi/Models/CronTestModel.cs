﻿using System;
using System.Collections.Generic;

namespace Ombi.Models
{
    public class CronTestModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<DateTime> Schedule { get; set; } = new List<DateTime>();
    }
}