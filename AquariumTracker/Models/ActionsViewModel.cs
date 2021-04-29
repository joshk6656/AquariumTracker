using System;
using System.Collections.Generic;

namespace AquariumTracker.Models
{
    public class ActionsViewModel : ViewModelBase
    {
        public List<Models.Action> Actions { get; set; } = new List<Models.Action>();
    }
}