using System;
using System.Collections.Generic;

namespace AquariumTracker.Models
{
    public class EditSupplyViewModel : ViewModelBase
    {
        public Supply Supply { get; set; } = new Supply();
    }
}