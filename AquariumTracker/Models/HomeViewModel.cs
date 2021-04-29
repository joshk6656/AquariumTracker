using System;
using System.Collections.Generic;

namespace AquariumTracker.Models
{
    public class HomeViewModel : ViewModelBase
    {
        public List<TestAverages> Averages { get; set; } = new List<TestAverages>();
        public List<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
    }
}