using System;
using System.Collections.Generic;

namespace AquariumTracker.Models
{
    public class FishViewModel : ViewModelBase
    {
        public List<Fish> FishList { get; set; } = new List<Fish>();
    }
}