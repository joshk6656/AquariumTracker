using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace AquariumTracker.Models
{
    public partial class Supply
    {

        public int SupplyId { get; set; }
        public int AquariumOwnerId { get; set; }
        public string Name { get; set; }
        public int AmountRemaining { get; set; }
    }
}