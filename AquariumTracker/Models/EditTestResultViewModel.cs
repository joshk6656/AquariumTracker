using System;
using System.Collections.Generic;

namespace AquariumTracker.Models
{
    public class EditTestResultViewModel : ViewModelBase
    {
        public TestResult Result { get; set; } = new TestResult();
    }
}