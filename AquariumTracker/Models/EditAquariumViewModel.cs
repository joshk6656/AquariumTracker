using System;
using System.Collections.Generic;

namespace AquariumTracker.Models
{
    public class EditAquariumViewModel : ViewModelBase
    {
        public Aquarium Aquarium { get; set; } = new Aquarium();
    }
}