using System;
using System.Collections.Generic;

namespace AquariumTracker.Models
{
    public class EditFishViewModel : ViewModelBase
    {
        public Fish Fish { get; set; } = new Fish();
    }
}