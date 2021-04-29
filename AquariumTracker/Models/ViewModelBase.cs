using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace AquariumTracker.Models
{
    public abstract class ViewModelBase
    {
        public List<SelectListItem> Owners { get; set; }
    }

}