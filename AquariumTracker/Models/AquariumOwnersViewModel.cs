using System;
using System.Collections.Generic;

namespace AquariumTracker.Models
{
    public class AquariumOwnersViewModel : ViewModelBase
    {
        public List<AquariumOwner> OwnerList { get; set; } = new List<AquariumOwner>();
    }
}