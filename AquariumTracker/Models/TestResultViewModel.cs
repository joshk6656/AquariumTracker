using System;
using System.Collections.Generic;

namespace AquariumTracker.Models
{
    public class TestResultViewModel : ViewModelBase
    {
        public List<TestResult> TestResults { get; set; } = new List<TestResult>();
    }
}