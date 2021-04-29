using System;
using System.Collections.Generic;

namespace AquariumTracker.Models
{
    public class AquariumsViewModel : ViewModelBase
    {
        public List<Aquarium> AquariumList { get; set; } = new List<Aquarium>();
    }
}