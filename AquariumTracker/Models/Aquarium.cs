using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace AquariumTracker.Models
{
    public partial class Aquarium
    {

        public int AquariumId { get; set; }
        public int AquariumOwnerId { get; set; }
        public string AquariumName { get; set; }
        public string OwnerFirstName { get; set; }
        public string OwnerLastName { get; set; }
        public int? Volume { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; }
        public List<SelectListItem> OwnerDropdown { get; set; }

    }
}