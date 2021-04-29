using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace AquariumTracker.Models
{
    public partial class TestResult
    {

        public int TestResultId { get; set; }
        public int TestId { get; set; }
        public DateTime TestDate { get; set; } = DateTime.Now;
        public string TestName { get; set; }
        public decimal ResultValue { get; set; }
        public List<SelectListItem> TestDropdown { get; set; }
    }
}