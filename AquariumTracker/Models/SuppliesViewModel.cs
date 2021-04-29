using System;
using System.Collections.Generic;

namespace AquariumTracker.Models
{
    public class SuppliesViewModel : ViewModelBase
    {
        public List<Supply> Supplies { get; set; } = new List<Supply>();
    }
}