using System;
using System.Collections.Generic;

namespace AquariumTracker.Models
{
    public class EditOwnerViewModel : ViewModelBase
    {
        public AquariumOwner Owner { get; set; } = new AquariumOwner();
    }
}