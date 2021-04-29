using System;
using System.Collections.Generic;

namespace AquariumTracker.Models
{
    public class EditActionViewModel : ViewModelBase
    {
        public Models.Action Action { get; set; } = new Models.Action();
    }
}