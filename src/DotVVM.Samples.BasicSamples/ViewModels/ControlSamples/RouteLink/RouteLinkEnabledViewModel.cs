using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.RouteLink
{
    public class RouteLinkEnabledViewModel
    {
        public bool Enabled { get; set; }
        public int RouteParameter { get; set; } = 1;
    }
}