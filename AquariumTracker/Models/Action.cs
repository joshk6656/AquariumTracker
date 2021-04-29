using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace AquariumTracker.Models
{
    public partial class Action
    {
        public int ActionId { get; set; }
        public int SupplyId { get; set; }
        public string SupplyName { get; set; }
        public int AquariumId { get; set; }
        public DateTime ActionDate { get; set; } = DateTime.Now;
        public int AmountUsed { get; set; }
        public List<SelectListItem> SupplyDropdown { get; set; }
    }
}