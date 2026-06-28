using System;
using System.Collections.Generic;
using System.Text;

namespace LearningWpf
{
    public class AppSettings
    {
        public string ApplicationName { get; set; } = "LearningWpf";
        public int MaxItemsPerPage { get; set; } = 20;
        public bool EnableFeatureX { get; set; }
    }
}
