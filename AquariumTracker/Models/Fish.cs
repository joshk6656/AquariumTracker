using System;

namespace AquariumTracker.Models
{
    public partial class Fish
    {
        public int FishId { get; set; }
        public int AquariumId { get; set; }
        public string Name { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public DateTime? DateRemoved { get; set; }
        public bool Quarantined { get; set; }
    }
}